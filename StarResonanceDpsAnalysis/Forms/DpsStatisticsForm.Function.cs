using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Control.GDI;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Extends;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Plugin.LaunchFunction;
using StarResonanceDpsAnalysis.Properties;
using static ScottPlot.Plottables.SmithChartAxis;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatisticsForm
    {
        #region 加载 网卡 启动设备/初始化 统计数据/ 启动 抓包/停止抓包/清空数据/ 关闭 事件
        private void InitTableColumnsConfigAtFirstRun()
        {
            if (AppConfig.GetConfigExists())
            {
                AppConfig.NickName = AppConfig.GetValue("UserConfig", "NickName", "未知");
                AppConfig.Uid = (ulong)AppConfig.GetValue("UserConfig", "Uid", "0").ToInt();
                AppConfig.Profession = AppConfig.GetValue("UserConfig", "Profession", "未知");
                AppConfig.CombatPower = AppConfig.GetValue("UserConfig", "CombatPower", "0").ToInt();
                StatisticData._manager.SetNickname(AppConfig.Uid, AppConfig.NickName);
                StatisticData._manager.SetProfession(AppConfig.Uid, AppConfig.Profession);
                StatisticData._manager.SetCombatPower(AppConfig.Uid, AppConfig.CombatPower);
                textProgressBar1.ProgressBarColor = colorDict[AppConfig.Profession];
                SortedProgressBarStatic = this.sortedProgressBarList1; // 关键：这里绑定实例
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
            try
            {
                var dev = (ICaptureDevice)sender;
                PacketAnalyzer.StartNewAnalyzer(dev, e.GetPacket()); // # 抓包入口：把原始包交给 PacketAnalyzer 解析/解压/解码

            }
            catch (Exception ex)
            {
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
            // 前置校验 ——
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


            // 清空当前统计 ——
            DpsTableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            SkillTableDatas.SkillTable.Clear();

            // 清空图表历史数据，开始新的战斗记录
            ChartVisualizationService.ClearAllHistory();

            // 启动所有图表的自动刷新
            ChartVisualizationService.StartAllChartsAutoRefresh(1000);

            // 事件注册与启动监听 --
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

            IsCaptureStarted = true;
            Console.WriteLine("开始抓包...");


        }

        /// <summary>
        /// 停止抓包
        /// </summary>
        public void StopCapture()
        {
            // 先停止所有图表的自动刷新，防止在停止抓包后继续更新数据
            ChartVisualizationService.StopAllChartsAutoRefresh();


            // 在停止抓包时，通知图表服务战斗结束，确保显示最终的0值状态
            ChartVisualizationService.OnCombatEnd();

            if (SelectedDevice != null)
            {
                try
                {
                    // 1) 先解绑，避免回调里撞到已释放对象
                    SelectedDevice.OnPacketArrival -= Device_OnPacketArrival;

                    // 2) 停止抓包（异步）
                    SelectedDevice.StopCapture();

                    // 3) 等待后台线程真正退出（最多 ~1s）
                    for (int i = 0; i < 100; i++)
                    {
                        // 有的版本有 Started 属性；没有就直接 sleep 一下
                        if (!(SelectedDevice.Started)) break;
                        System.Threading.Thread.Sleep(10);
                    }

                    // 4) 关闭并彻底释放驱动句柄
                    SelectedDevice.Close();
                    SelectedDevice.Dispose();   // ← 关键：第二次不触发通常是没 Dispose
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

            IsCaptureStarted = false;

            // 清空解析/重组状态 ——（按你的实际字段名来）
            PacketAnalyzer.ResetCaptureState();

            // 更新网卡设置提示状态
            StartupInitializer.RefreshNetworkCardSettingTip();
        }

        #region HandleClearData() 响应清空数据

        public void HandleClearData()
        {

            // 先停止所有图表的自动刷新
           ChartVisualizationService.StopAllChartsAutoRefresh();

            // 在清空数据前，通知图表服务战斗结束
            ChartVisualizationService.OnCombatEnd();

            DpsTableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            SkillTableDatas.SkillTable.Clear();

            ListClear();
            // 完全重置所有图表（包括清空历史数据和重置视图状态）
            ChartVisualizationService.FullResetAllCharts();

            // 如果当前正在抓包，重新启动图表自动刷新
            if (IsCaptureStarted)
            {
                ChartVisualizationService.StartAllChartsAutoRefresh(1000);
            }
        }
        private readonly object _dataLock = new();
        private int _isClearing = 0; // 0: 正常，1: 清空中
        public void ListClear()
        {
            if (Interlocked.Exchange(ref _isClearing, 1) == 1) return; // 已在清空中

            try
            {
                lock (_dataLock)
                {
                    DictList.Clear();
                    list.Clear();
                    userRenderContent.Clear();
                    // UI 组件缓存清空（注意切回 UI 线程）
                    var ctrl = SortedProgressBarStatic;
                    if (ctrl != null && !ctrl.IsDisposed)
                    {
                        if (ctrl.InvokeRequired)
                            ctrl.BeginInvoke(new Action(() => ctrl.Data = null));
                        else
                            ctrl.Data = null;
                    }
                }
            }
            finally
            {
                Volatile.Write(ref _isClearing, 0);
            }
        }

        #endregion
        #endregion
        #endregion


        public void SetStyle()
        {
            // ======= 单个进度条（textProgressBar1）的外观设置 =======
            textProgressBar1.Padding = new Padding(3, 3, 3, 3);
            textProgressBar1.ProgressBarCornerRadius = 3; // 超大圆角

            // ======= 进度条列表（sortedProgressBarList1）的初始化与外观 =======
            sortedProgressBarList1.ProgressBarHeight = 30;  // 每行高度
            sortedProgressBarList1.AnimationDuration = 1000; // 动画时长（毫秒）
            sortedProgressBarList1.AnimationQuality = Quality.High; // 动画品质（你项目里的枚举）
       



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

        Dictionary<string, Color> colorDict = new Dictionary<string, Color>()
        {
            { "神射手", ColorTranslator.FromHtml("#fffca3") }, //
            { "冰魔导师", ColorTranslator.FromHtml("#aaa6ff") }, // 
            { "巨刃守护者", ColorTranslator.FromHtml("#51a55d") }, // 
            { "雷影剑士", ColorTranslator.FromHtml("#9676ff") }, // 
            { "灵魂乐手", ColorTranslator.FromHtml("#ff5353") }, // 
            { "青岚骑士", ColorTranslator.FromHtml("#abfaff") }, // 
            { "森语者", ColorTranslator.FromHtml("#78ff95") }, // 
            { "神盾骑士", ColorTranslator.FromHtml("#2E86AB") }, // 
            {"未知",  ColorTranslator.FromHtml("#67AEF6")}
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
        { "未知", new Bitmap(new MemoryStream(Resources.hp_icon)) }
    };

        public void RefreshDpsTable()
        {
            if (Interlocked.CompareExchange(ref _isClearing, 0, 0) == 1)
                return; // 清空中，直接跳过本轮，避免和 Clear 打架

            var statsList = StatisticData._manager.GetPlayersWithCombatData().ToArray();
            if (statsList.Length == 0) return;

            // 计算部分尽量在锁外完成（减少锁占用时间）
            float totalDamageSum = statsList.Where(p => p?.DamageStats != null)
                                            .Sum(p => (float)p.DamageStats.Total);
            if (totalDamageSum <= 0f) totalDamageSum = 1f;

            var maxDamage = statsList.Max(p => (float)(p?.DamageStats?.Total ?? 0));
            var ordered = statsList.OrderByDescending(p => p?.DamageStats?.Total ?? 0).ToList();

            lock (_dataLock)
            {
                if (_isClearing == 1) return; // 进入锁后再二次确认

                for (int i = 0; i < ordered.Count; i++)
                {
                    var p = ordered[i];
                    if (p is null) continue;

                    long uid = (long)p.Uid;
                    int ranking = i + 1;

                    var realtime = Common.FormatWithEnglishUnits(Math.Round(p.DamageStats.GetTotalPerSecond(), 1));
                    string totalFmt = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                    string share = (p.DamageStats.Total / totalDamageSum * 100).ToString("0") + "%";

                    float progress = maxDamage > 0 ? (float)(p.DamageStats.Total / maxDamage) : 0f;

                    // 注意：重复 new Bitmap(new MemoryStream(...)) 会泄露 GDI 资源，建议把职业图像缓存成 Bitmap
                    var profBmp = imgDict[p.Profession];

                    if (!DictList.TryGetValue(uid, out var data))
                    {
                        data = new List<RenderContent>
                    {
                        new RenderContent { Type=RenderContent.ContentType.Text,  Align=RenderContent.ContentAlign.MiddleLeft,  Offset=new RenderContent.ContentOffset { X = 10, Y = 0 },  ForeColor=Color.Black, Font=AppConfig.DpsFontBold },
                        new RenderContent { Type=RenderContent.ContentType.Image, Align=RenderContent.ContentAlign.MiddleLeft,  Offset = new RenderContent.ContentOffset { X = 30, Y = 0 },  Image=profBmp, ImageRenderSize=new Size(32,32) },
                        new RenderContent { Type=RenderContent.ContentType.Text,  Align=RenderContent.ContentAlign.MiddleLeft, Offset = new RenderContent.ContentOffset { X = 65, Y = 0 },  ForeColor=Color.Black, Font=AppConfig.DpsFontBold },
                        new RenderContent { Type=RenderContent.ContentType.Text,  Align=RenderContent.ContentAlign.MiddleRight,Offset = new RenderContent.ContentOffset { X = -55, Y = 0 }, ForeColor=Color.Black, Font=AppConfig.DpsFontBold },
                        new RenderContent { Type=RenderContent.ContentType.Text,  Align=RenderContent.ContentAlign.MiddleRight, Offset = new RenderContent.ContentOffset { X = 0, Y = 0 }, ForeColor=Color.Black, Font=AppConfig.DpsFontBold },
                    };

                        var progressBarData = new ProgressBarData
                        {
                            ID = uid,
                            ContentList = data,
                            ProgressBarCornerRadius = 3,
                            ProgressBarValue = progress,
                            ProgressBarColor = colorDict[p.Profession],
                        };

                        list.Add(progressBarData);
                        DictList[uid] = data;
                    }

                    // 更新已有项
                    var row = DictList[uid];
                    row[0].Text = $"{ranking}.";
                    row[1].Image = profBmp;
                    row[2].Text = $"{p.Nickname}({p.CombatPower})";
                    row[3].Text = $"{totalFmt}({realtime})";
                    row[4].Text = share;

                    if (p.Uid == AppConfig.Uid)
                    {
                        if (userRenderContent.Count == 0) userRenderContent = row;
                        userRenderContent[0].Text = $"{ranking}.";
                        userRenderContent[1].Image = profBmp;
                        userRenderContent[3].Text = $"{totalFmt}({realtime})";
                        textProgressBar1.ContentList = userRenderContent;
                        textProgressBar1.ProgressBarValue = progress;
                    }
                }

                // 列表绑定（确保在 UI 线程调用）
                sortedProgressBarList1.Data = list;
            }
        }

        /// <summary>
        /// 从 Properties.Resources 中按名字获取图片；兼容资源为 Image 或 byte[] 两种情况。
        /// 返回 null 表示未找到或格式不正确。
        /// </summary>



    }
}
