using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Control.GDI;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Extends;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Plugin.Database;
using StarResonanceDpsAnalysis.Plugin.LaunchFunction;
using StarResonanceDpsAnalysis.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatisticsForm
    {
        // # 导航
        // # 本文件主要职责：
        // #   1) 启动/停止网络抓包的生命周期管理（StartCapture/StopCapture）。
        // #   2) 清空/重置统计数据与图表（HandleClearData/ListClear）。
        // #   3) 初始化用户与控件样式（InitTableColumnsConfigAtFirstRun/SetStyle）。
        // #   4) 处理数据包到达事件，将原始数据交给 PacketAnalyzer（Device_OnPacketArrival）。
        // #   5) 构建并刷新 DPS/治疗/承伤的 UI 列表（RefreshDpsTable/BuildUiRows）。
        // # 事件分类索引：
        // #   * [启动与初始化事件] InitTableColumnsConfigAtFirstRun / LoadNetworkDevices / SetStyle
        // #   * [抓包事件] StartCapture / StopCapture / Device_OnPacketArrival
        // #   * [清理与复位事件] HandleClearData / ListClear
        // #   * [UI 刷新事件] RefreshDpsTable / BuildUiRows
        // #   * [线程安全与状态] _dataLock / _isClearing / IsCaptureStarted / SelectedDevice

        #region 加载 网卡 启动设备/初始化 统计数据/ 启动 抓包/停止抓包/清空数据/ 关闭 事件
        private void InitTableColumnsConfigAtFirstRun()
        {
            // # 启动与初始化事件：首次运行初始化表头配置 & 绑定本机身份信息
            if (AppConfig.GetConfigExists())
            {
                AppConfig.ClearPicture = AppConfig.GetValue("UserConfig", "ClearPicture", "1").ToInt();
                AppConfig.NickName = AppConfig.GetValue("UserConfig", "NickName", "未知");
                AppConfig.Uid = (ulong)AppConfig.GetValue("UserConfig", "Uid", "0").ToInt();
                AppConfig.Profession = AppConfig.GetValue("UserConfig", "Profession", "未知");
                AppConfig.CombatPower = AppConfig.GetValue("UserConfig", "CombatPower", "0").ToInt();

                // 写入本地统计缓存（用于 UI 初始显示）
                StatisticData._manager.SetNickname(AppConfig.Uid, AppConfig.NickName);
                StatisticData._manager.SetProfession(AppConfig.Uid, AppConfig.Profession);
                StatisticData._manager.SetCombatPower(AppConfig.Uid, AppConfig.CombatPower);

                if (AppConfig.Uid != 0)
                {
                    // 仅在未知时从 API 回填（单 UID 去重）
                    PlayerDbSyncService.TryFillFromDbOnce(AppConfig.Uid);
                    // 回填完成后按当前视图刷新（避免依赖后续抓包）
                    RequestActiveViewRefresh();
                }

                SortedProgressBarStatic = this.sortedProgressBarList1; // # 关键：这里绑定实例
                return;
            }
        }

        #region —— 抓包设备/统计 —— 

        public static ICaptureDevice? SelectedDevice { get; set; } = null; // # 抓包设备：程序选中的网卡设备（可能为null，依据设置初始化）

        /// <summary>
        /// 分析器
        /// </summary>
        private PacketAnalyzer PacketAnalyzer { get; } = new(); // # 抓包/分析器：每个到达的数据包交由该分析器处理
        #endregion

        /// <summary>
        /// 启动时加载网卡设备
        /// </summary>
        public void LoadNetworkDevices()
        {
            // # 启动与初始化事件：应用启动阶段加载网络设备列表，依据配置选择默认网卡
            Console.WriteLine("应用程序启动时加载网卡...");

            if (AppConfig.NetworkCard >= 0)
            {
                var devices = CaptureDeviceList.Instance; // # 设备列表：SharpPcap 提供
                if (AppConfig.NetworkCard < devices.Count)
                {
                    SelectedDevice = devices[AppConfig.NetworkCard]; // # 根据索引选择设备
                    Console.WriteLine($"启动时已选择网卡: {SelectedDevice.Description} (索引: {AppConfig.NetworkCard})");
                }
            }
            else
            {
                // 未设置时弹出设置窗口，引导用户选择
                if (FormManager.settingsForm == null || FormManager.settingsForm.IsDisposed)
                {
                    FormManager.settingsForm = new SettingsForm();
                }
                FormManager.settingsForm.LoadDevices(); // # 设置窗体：填充设备列表
            }
        }

        /// <summary>
        /// 数据包到达事件
        /// </summary>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            // # 抓包事件：回调于数据包到达时（SharpPcap线程）
            try
            {
                var dev = (ICaptureDevice)sender;
                PacketAnalyzer.StartNewAnalyzer(dev, e.GetPacket());
            }
            catch (Exception ex)
            {
                // # 异常保护：避免抓包线程因未处理异常中断
                Console.WriteLine($"数据包到达后进行处理时发生异常 {ex.Message}\r\n{ex.StackTrace}");
            }
        }
        #region StartCapture() 抓包：开始/停止/事件/统计
        /// <summary>
        /// 是否开始抓包
        /// </summary>
        private static bool IsCaptureStarted { get; set; } = false; // # 运行状态：标识当前是否处于抓包/监控中

        /// <summary>
        /// 开始抓包
        /// </summary>
        public void StartCapture()
        {
            // # 抓包事件：用户点击“开始”或自动启动时触发
            // # 步骤 1：前置校验 —— 网络设备索引/可用性检查
            if (AppConfig.NetworkCard < 0)
            {
                MessageBox.Show("请选择一个网卡设备");
                return;
            }

            var devices = CaptureDeviceList.Instance;
            if (devices == null || devices.Count == 0)
                throw new InvalidOperationException("没有找到可用的网络抓包设备");

            if (AppConfig.NetworkCard < 0 || AppConfig.NetworkCard >= devices.Count)
                throw new InvalidOperationException($"无效的网络设备索引: {AppConfig.NetworkCard}");

            SelectedDevice = devices[AppConfig.NetworkCard];
            if (SelectedDevice == null)
                throw new InvalidOperationException($"无法获取网络设备，索引: {AppConfig.NetworkCard}");

            Task.Delay(1000);
            // # 步骤 3：图表历史与自动刷新 —— 开始新的战斗记录
            ChartVisualizationService.ClearAllHistory();

            // 启动所有图表的自动刷新 + 后台采样（满足“从DPS伤害开始就加载曲线”）
            ChartVisualizationService.StartAllChartsAutoRefresh(1000);

            // # 步骤 4：打开并启动设备监听 —— 绑定回调、设置过滤器
            SelectedDevice.Open(new DeviceConfiguration
            {
                Mode = DeviceModes.Promiscuous,
                Immediate = true,
                ReadTimeout = 1000,
                BufferSize = 1024 * 1024 * 4
            });
            SelectedDevice.Filter = "ip and tcp";
            SelectedDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            SelectedDevice.StartCapture();

            // # 步骤 5：标记状态、启动全程记录器
            IsCaptureStarted = true;
            FullRecord.Start();
            Console.WriteLine("开始抓包...");
        }

        /// <summary>
        /// 停止抓包
        /// </summary>
        public void StopCapture()
        {
            // # 抓包事件：用户点击“停止”或程序退出前触发
            // # 步骤 1：先停止所有图表的自动刷新，防止在停止抓包后继续更新数据
            ChartVisualizationService.StopAllChartsAutoRefresh();

            // 在停止抓包时，通知图表服务战斗结束，确保显示最终的0值状态
            ChartVisualizationService.OnCombatEnd();

            if (SelectedDevice != null)
            {
                try
                {
                    // # 步骤 2：解绑事件，避免回调访问已释放对象
                    SelectedDevice.OnPacketArrival -= Device_OnPacketArrival;

                    // # 步骤 3：停止抓包（内部通常是异步）
                    SelectedDevice.StopCapture();

                    // # 步骤 4：等待后台捕获线程真正退出（简单轮询，最多 ~1s）
                    for (int i = 0; i < 100; i++)
                    {
                        if (!(SelectedDevice.Started)) break;
                        System.Threading.Thread.Sleep(10);
                    }

                    // # 步骤 5：关闭并释放句柄（Dispose 很关键）
                    SelectedDevice.Close();
                    SelectedDevice.Dispose();
                    Console.WriteLine("停止抓包");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"停止抓包异常: {ex}");
                }
                finally
                {
                    SelectedDevice = null;
                }
            }

            // # 步骤 6：状态复位与解析状态清空
            IsCaptureStarted = false;

            // 清空解析/重组状态
            PacketAnalyzer.ResetCaptureState();

            // # 步骤 7：更新 UI 上的网卡设置提示
            StartupInitializer.RefreshNetworkCardSettingTip();
        }

        #region HandleClearData() 响应清空数据

        public void HandleClearData(bool ClearPicture = false)
        {
            // # 清理与复位事件：用户点击“清空”时触发（不影响抓包的开启/关闭状态）
            // 先停止所有图表的自动刷新
            ChartVisualizationService.StopAllChartsAutoRefresh();

            // 在清空数据前，通知图表服务战斗结束
            ChartVisualizationService.OnCombatEnd();
            if (FormManager.showTotal && !ClearPicture)
            {
                FullRecord.Reset(false);//全程统计数据清空
                // 同步清理“全程曲线”历史，确保时间轴从 0 重新开始
                ChartVisualizationService.ClearFullHistory();
            }

            ListClear();

            // 仅清空当次曲线历史，保留全程曲线（满足“全程伤害从伤害开始记录到F9刷新”）
            ChartVisualizationService.ClearCurrentHistory();

            // 如果当前正在抓包，重新启动图表自动刷新（继续后台采样）
            if (IsCaptureStarted)
            {
                ChartVisualizationService.StartAllChartsAutoRefresh(1000);
            }
        }
        private readonly object _dataLock = new();
        private int _isClearing = 0; // 0: 正常，1: 清空中
        public void ListClear()
        {
            // # 清理与复位事件：清空 UI 进度条列表与缓存（线程安全）
            if (Interlocked.Exchange(ref _isClearing, 1) == 1) return; // 已在清空中

            StatisticData._manager.ClearAll();
            SkillTableDatas.SkillTable.Clear();
            label1.Text = $"";
            label2.Text = $"";
            try
            {
                lock (_dataLock)
                {
                    // # 清空在内存中的数据模型
                    DictList.Clear();
                    list.Clear();
                    userRenderContent.Clear();
                    // UI 组件缓存清空（注意切回 UI 线程）
                    var ctrl = SortedProgressBarStatic;
                    // ListClear() — 清空 UI
                    if (ctrl != null && !ctrl.IsDisposed)
                    {
                        if (ctrl.InvokeRequired)
                            ctrl.BeginInvoke(new Action(() => ctrl.Data = new List<ProgressBarData>()));
                        else
                            ctrl.Data = new List<ProgressBarData>();
                    }

                }
            }
            finally
            {
                // # 退出清空状态
                Volatile.Write(ref _isClearing, 0);
            }
        }

        #endregion
        #endregion
        #endregion


        public void SetStyle()
        {
            // # 启动与初始化事件：界面样式与渲染设置（仅 UI 外观，不涉及数据）
            // ======= 单个进度条（textProgressBar1）的外观设置 =======
            sortedProgressBarList1.OrderOffset = new RenderContent.ContentOffset { X = 10, Y = 0 };
            sortedProgressBarList1.OrderCallback = (i) => $"{i:d2}";

            if (Config.IsLight)
            {
                sortedProgressBarList1.OrderColor = Color.Black;
            }
            else
            {
                sortedProgressBarList1.OrderColor = Color.White;
            }

            sortedProgressBarList1.OrderFont = AppConfig.DigitalFont;
            // ======= 进度条列表（sortedProgressBarList1）的初始化与外观 =======
            sortedProgressBarList1.ProgressBarHeight = 50;  // 每行高度
            sortedProgressBarList1.AnimationDuration = 1000; // 动画时长（毫秒）
            sortedProgressBarList1.AnimationQuality = Quality.Low; // 动画品质（你项目里的枚举）
        }

        /// <summary>
        /// 实例化 SortedProgressBarList 控件
        /// </summary>
        public static SortedProgressBarList SortedProgressBarStatic { get; private set; }

        /// <summary>
        /// 用户战斗数据字典
        /// </summary>
        readonly static Dictionary<long, List<RenderContent>> DictList = new Dictionary<long, List<RenderContent>>();

        /// <summary>
        /// 用户战斗数据更新事件
        /// </summary>
        static List<ProgressBarData> list = new List<ProgressBarData>();

        /// <summary>
        /// 用户在底下显示自己的信息
        /// </summary>
        static List<RenderContent> userRenderContent = new List<RenderContent>();

        //白窗体
        Dictionary<string, Color> colorDict = new Dictionary<string, Color>()
        {
            { "未知", ColorTranslator.FromHtml("#67AEF6") },

            { "神射手", ColorTranslator.FromHtml("#fffca3") }, //
            { "冰魔导师", ColorTranslator.FromHtml("#aaa6ff") }, // 
            { "巨刃守护者", ColorTranslator.FromHtml("#51a55d") }, // 
            { "雷影剑士", ColorTranslator.FromHtml("#9676ff") }, // 
            { "灵魂乐手", ColorTranslator.FromHtml("#ff5353") }, // 
            { "青岚骑士", ColorTranslator.FromHtml("#abfaff") }, // 
            { "森语者", ColorTranslator.FromHtml("#78ff95") }, // 
            { "神盾骑士", ColorTranslator.FromHtml("#2E86AB") }, // 
            { "射线", ColorTranslator.FromHtml("#fffca3") },
            { "协奏", ColorTranslator.FromHtml("#ff5353") },
            { "愈合", ColorTranslator.FromHtml("#78ff95") },
            { "惩戒", ColorTranslator.FromHtml("#78ff95") },
            { "狂音", ColorTranslator.FromHtml("#ff5353") },
            { "冰矛", ColorTranslator.FromHtml("#aaa6ff") },
            { "居合", ColorTranslator.FromHtml("#9676ff") },
            { "月刃", ColorTranslator.FromHtml("#9676ff") },
            { "鹰弓", ColorTranslator.FromHtml("#fffca3") },
            { "狼弓", ColorTranslator.FromHtml("#fffca3") },
            { "空枪", ColorTranslator.FromHtml("#abfaff") },
            { "重装", ColorTranslator.FromHtml("#abfaff") },
            { "防盾", ColorTranslator.FromHtml("#2E86AB") },
            { "光盾", ColorTranslator.FromHtml("#2E86AB") },
            { "岩盾", ColorTranslator.FromHtml("#51a55d") },
            { "格挡", ColorTranslator.FromHtml("#51a55d") },

        };

        //黑窗体
        // 黑窗体
        Dictionary<string, Color> blackColorDict = new Dictionary<string, Color>()
        {
            { "神射手", ColorTranslator.FromHtml("#8e8b47") }, //
            { "冰魔导师", ColorTranslator.FromHtml("#79779c") }, // 
            { "巨刃守护者", ColorTranslator.FromHtml("#537758") }, // 
            { "雷影剑士", ColorTranslator.FromHtml("#70629c") }, // 
            { "灵魂乐手", ColorTranslator.FromHtml("#9c5353") }, // 
            { "青岚骑士", ColorTranslator.FromHtml("#799a9c") }, // 
            { "森语者", ColorTranslator.FromHtml("#639c70") }, // 
            { "神盾骑士", ColorTranslator.FromHtml("#9c9b75") }, // 
            { "射线", ColorTranslator.FromHtml("#8e8b47") },
            { "协奏", ColorTranslator.FromHtml("#9c5353") },
            { "愈合", ColorTranslator.FromHtml("#639c70") },
            { "惩戒", ColorTranslator.FromHtml("#639c70") },
            { "狂音", ColorTranslator.FromHtml("#9c5353") },
            { "冰矛", ColorTranslator.FromHtml("#79779c") },
            { "居合", ColorTranslator.FromHtml("#70629c") },
            { "月刃", ColorTranslator.FromHtml("#70629c") },
            { "鹰弓", ColorTranslator.FromHtml("#8e8b47") },
            { "狼弓", ColorTranslator.FromHtml("#8e8b47") },
            { "空枪", ColorTranslator.FromHtml("#799a9c") },
            { "重装", ColorTranslator.FromHtml("#799a9c") },
            { "防盾", ColorTranslator.FromHtml("#9c9b75") },
            { "光盾", ColorTranslator.FromHtml("#9c9b75") },
            { "岩盾", ColorTranslator.FromHtml("#537758") },
            { "格挡", ColorTranslator.FromHtml("#537758") },
            { "未知", ColorTranslator.FromHtml("#67AEF6") }
        };

        public static Dictionary<string, Bitmap> imgDict = new Dictionary<string, Bitmap>()
        {
            { "冰魔导师", new Bitmap(new MemoryStream(Resources.冰魔导师)) },
            { "巨刃守护者", new Bitmap(new MemoryStream(Resources.巨刃守护者)) },
            { "森语者", new Bitmap(new MemoryStream(Resources.森语者)) },
            { "灵魂乐手", new Bitmap(new MemoryStream(Resources.灵魂乐手)) },
            { "神射手", new Bitmap(new MemoryStream(Resources.神射手)) },
            { "雷影剑士", new Bitmap(new MemoryStream(Resources.雷影剑士)) },
            { "青岚骑士", new Bitmap(new MemoryStream(Resources.青岚骑士)) },
            { "神盾骑士", new Bitmap(new MemoryStream(Resources.神盾骑士)) },
            { "未知", new Bitmap(new MemoryStream(Resources.hp_icon)) }
        };



        public enum SourceType { Current, FullRecord }
        public enum MetricType { Damage, Healing, Taken }

        // 提供一个静态入口，供回填后请求一次按当前视图刷新
        public static void RequestActiveViewRefresh()
        {
            try
            {
                var form = FormManager.dpsStatistics;
                if (form == null || form.IsDisposed) return;
                var source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;
                var metric = FormManager.currentIndex switch
                {
                    1 => MetricType.Healing,
                    2 => MetricType.Taken,
                    _ => MetricType.Damage
                };
                if (form.InvokeRequired)
                    form.BeginInvoke(new Action(() => form.RefreshDpsTable(source, metric)));
                else
                    form.RefreshDpsTable(source, metric);
            }
            catch { }
        }

        private class UiRow
        {
            public long Uid;
            public string Nickname;
            public int CombatPower;
            public string Profession;
            public ulong Total;
            public double PerSecond;
            public string SubProfession;
        }

        public void RefreshDpsTable(SourceType source, MetricType metric)
        {
            // # UI 刷新事件：根据指定数据源（单次/全程）与指标（伤害/治疗/承伤）对进度条列表进行重建与绑定
            if (Interlocked.CompareExchange(ref _isClearing, 0, 0) == 1) return;
            // —— 闸门 #1：开始前校验当前可见视图是否匹配 ——
            var visible = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;
            if (source != visible) return;

            var uiList = BuildUiRows(source, metric)
                .Where(r => (r?.Total ?? 0) > 0)   // 过滤 0 值（伤害/治疗/承伤都适用）
                .ToList();

            if (uiList.Count == 0)
            {
                if (sortedProgressBarList1.InvokeRequired)
                    sortedProgressBarList1.BeginInvoke(new Action(() => sortedProgressBarList1.Data = new List<ProgressBarData>()));
                else
                    sortedProgressBarList1.Data = new List<ProgressBarData>();
                return;
            }

            var ordered = uiList.OrderByDescending(x => x.Total).ToList();

            double teamSum = uiList.Sum(x => (double)x.Total);
            if (teamSum <= 0d) teamSum = 1d;
            double top = uiList.Max(x => (double)x.Total);
            if (top <= 0d) top = 1d;
            lock (_dataLock)
            {
                if (_isClearing == 1) return;

                // 1) 拍当前 list 的快照，用它参与所有枚举相关计算
                var snapshot = list.ToList(); // <<--- 关键

                var present = new HashSet<long>(ordered.Select(x => x.Uid));

                // 2) 先用快照算需要删除的旧行（避免直接枚举原 list）
                var toRemove = snapshot.Where(pb => !present.Contains(pb.ID))
                                       .Select(pb => pb.ID)
                                       .ToList();

                // 3) 基于快照建立索引，后面查找更快也更安全
                var byId = snapshot.ToDictionary(pb => pb.ID);

                // 4) 准备一个“下一帧”的新列表，最后一次性替换
                var next = new List<ProgressBarData>(present.Count);

                for (int i = 0; i < ordered.Count; i++)
                {
                    var p = ordered[i];
                    if (string.IsNullOrEmpty(p.Profession)) continue;

                    float ratio = (float)(p.Total / top);
                    if (!float.IsFinite(ratio)) ratio = 0f;
                    ratio = Math.Clamp(ratio, 0f, 1f);

                    string totalFmt = Common.FormatWithEnglishUnits(p.Total);
                    string perSec = Common.FormatWithEnglishUnits(Math.Round(p.PerSecond, 1));

                    var profBmp = imgDict[p.Profession];
                    var dict = Config.IsLight ? colorDict : blackColorDict;
                    var key = (p?.Profession is string pr && pr != "未知" && dict.ContainsKey(pr)) ? pr
                             : (p?.SubProfession is string sr && sr != "未知" && dict.ContainsKey(sr)) ? sr
                             : "未知";
                    var color = dict.TryGetValue(key, out var c) ? c : ColorTranslator.FromHtml("#67AEF6");


                    // 渲染行内容：DictList 也只在锁内改
                    if (!DictList.TryGetValue(p.Uid, out var row))
                    {
                        row = [
                            new() { Type = RenderContent.ContentType.Image, Align = RenderContent.ContentAlign.MiddleLeft, Offset = new RenderContent.ContentOffset{ X = 35, Y = 0 }, Image = profBmp, ImageRenderSize = new Size(25, 25) },
                            new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleLeft, Offset = new RenderContent.ContentOffset{ X = 65, Y = 0 }, ForeColor = AppConfig.colorText, Font = AppConfig.DigitalFont },
                            new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleRight, Offset = new RenderContent.ContentOffset{ X = -55, Y = 0 }, ForeColor = AppConfig.colorText, Font = AppConfig.DigitalFont },
                            new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleRight, Offset = new RenderContent.ContentOffset{ X = 0, Y = 0 },  ForeColor = AppConfig.colorText, Font = AppConfig.DigitalFont },
                        ];
                        DictList[p.Uid] = row;
                    }

                    string share = $"{Math.Round(p.Total / teamSum * 100d, 0, MidpointRounding.AwayFromZero)}%";
                    row[0].Image = profBmp;
                    // 只要子流派；没有子流派就用战力；否则只显示昵称
                    string tag = !string.IsNullOrWhiteSpace(p.SubProfession)
                        ? p.SubProfession
                        : (p.CombatPower > 0 ? Common.FormatWithEnglishUnits(p.CombatPower) : "");

                    row[1].Text = string.IsNullOrEmpty(tag)
                        ? p.Nickname
                        : $"{p.Nickname}({tag})";

                    row[2].Text = $"{totalFmt}({perSec})";
                    row[3].Text = share;

                    if (p.Uid == (long)AppConfig.Uid)
                    {
                        label1.Text = $" [{i + 1}]";
                        label2.Text = $"{totalFmt}({perSec})";
                    }

                    // 复用旧的 ProgressBarData，避免 UI 抖动；没有则新建
                    if (!byId.TryGetValue(p.Uid, out var pb))
                    {
                        pb = new ProgressBarData
                        {
                            ID = p.Uid,
                            ContentList = row,
                            ProgressBarCornerRadius = 3,
                            ProgressBarValue = ratio,
                            ProgressBarColor = color,
                        };
                    }
                    else
                    {
                        pb.ContentList = row;      // 保底同步
                        pb.ProgressBarValue = ratio;
                        pb.ProgressBarColor = color;
                    }

                    next.Add(pb);
                }

                // 5) 处理 DictList 的删除（可选，保持干净）
                if (toRemove.Count > 0)
                {
                    foreach (var uid in toRemove)
                        DictList.Remove(uid);
                }

                // 6) 一次性替换 list，避免“枚举中修改”
                list = next;

                // RefreshDpsTable(...) — 锁内最终绑定
                void Bind()
                {
                    sortedProgressBarList1.Data = list; // list 永不为 null
                }

                if (sortedProgressBarList1.InvokeRequired) sortedProgressBarList1.BeginInvoke((Action)Bind);
                else Bind();
            }
        }


        private List<UiRow> BuildUiRows(SourceType source, MetricType metric)
        {
            // # UI 刷新事件：根据数据源构建用于展示的轻量行结构（与底层统计对象解耦）
            if (source == SourceType.Current)
            {
                var statsList = StatisticData._manager.GetPlayersWithCombatData().ToArray();
                if (statsList.Length == 0) return new();

                // 新增：为当前视图内的所有玩家尝试回填昵称/职业/战力（每 UID 只请求一次）
                foreach (var p in statsList)
                {
                    try { PlayerDbSyncService.TryFillFromDbOnce(p.Uid); } catch { }
                }

                return statsList.Select(p =>
                {
                    ulong total;
                    double ps;
                    switch (metric)
                    {
                        case MetricType.Healing:
                            total = p.HealingStats.Total;
                            ps = p.HealingStats.GetTotalPerSecond();
                            break;
                        case MetricType.Taken:
                            total = p.TakenStats.Total;
                            ps = p.TakenStats.GetTotalPerSecond();
                            break;
                        default: // Damage
                            total = p.DamageStats.Total;
                            ps = p.DamageStats.GetTotalPerSecond();
                            break;
                    }

                    return new UiRow
                    {
                        Uid = (long)p.Uid,
                        Nickname = p.Nickname,
                        CombatPower = p.CombatPower,
                        Profession = p.Profession,
                        SubProfession = p.SubProfession ?? "",
                        Total = total,
                        PerSecond = ps
                    };
                }).ToList();
            }
            else // FullRecord
            {
                var fr = FullRecord.GetPlayersWithTotalsArray();
                if (fr.Length == 0) return new();

                // 新增：为全程中出现过的所有玩家尝试回填一次基础信息
                foreach (var p in fr)
                {
                    try { PlayerDbSyncService.TryFillFromDbOnce(p.Uid); } catch { }
                }

                var sessionSecs = Math.Max(1.0, FullRecord.GetSessionTotalTimeSpan().TotalSeconds);

                return fr.Select(p =>
                {
                    ulong total;
                    double ps;
                    switch (metric)
                    {
                        case MetricType.Healing:
                            total = p.TotalHealing;
                            ps = p.Hps;
                            break;
                        case MetricType.Taken:
                            total = p.TakenDamage;
                            ps = total / sessionSecs;
                            break;
                        default: // Damage
                            total = p.TotalDamage;
                            ps = p.Dps;
                            break;
                    }

                    return new UiRow
                    {
                        Uid = (long)p.Uid,
                        Nickname = p.Nickname,
                        CombatPower = p.CombatPower,
                        Profession = p.Profession,
                        Total = total,
                        PerSecond = ps
                    };
                }).ToList();
            }
        }
    }
}
