using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Analyze;
using StarResonanceDpsAnalysis.Core.Analyze.Exceptions;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.WinForm.Control.GDI;
using StarResonanceDpsAnalysis.WinForm.Core;
using StarResonanceDpsAnalysis.WinForm.Plugin;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class DpsStatisticsForm
    {
        private bool _isShowFullData = false;
        private int _stasticsType = 0;

        private void SetDefaultFontFromResources()
        {
            pageHeader_MainHeader.Font = AppConfig.SaoFont;
            pageHeader_MainHeader.SubFont = AppConfig.ContentFont;
            label_CurrentDps.Font = label_CurrentOrder.Font = AppConfig.ContentFont;

            button_TotalDamage.Font = AppConfig.BoldHarmonyFont;
            button_TotalTreatment.Font = AppConfig.BoldHarmonyFont;
            button_AlwaysInjured.Font = AppConfig.BoldHarmonyFont;
            button_NpcTakeDamage.Font = AppConfig.BoldHarmonyFont;
        }

        #region 加载 网卡 启动设备/初始化 统计数据/ 启动 抓包/停止抓包/清空数据/ 关闭 事件
        private void InitTableColumnsConfigAtFirstRun()
        {
            // # 启动与初始化事件：首次运行初始化表头配置 & 绑定本机身份信息
            if (AppConfig.GetConfigExists())
            {
                AppConfig.ClearPicture = AppConfig.GetValue("UserConfig", "ClearPicture", "1").ToInt();
                AppConfig.NickName = AppConfig.GetValue("UserConfig", "NickName", "未知");
                AppConfig.Uid = AppConfig.GetValue("UserConfig", "Uid", "0").ToInt64();
                AppConfig.Profession = AppConfig.GetValue("UserConfig", "Profession", "未知");
                AppConfig.CombatPower = AppConfig.GetValue("UserConfig", "CombatPower", "0").ToInt();
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
        private void LoadNetworkDevices()
        {
            // 启动与初始化事件：应用启动阶段加载网络设备列表，依据配置选择默认网卡
            Console.WriteLine("应用程序启动时加载网卡...");

            if (AppConfig.NetworkCard >= 0)
            {
                // 设备列表：SharpPcap 提供
                var devices = CaptureDeviceList.Instance;
                if (AppConfig.NetworkCard < devices.Count)
                {
                    // 根据索引选择设备
                    SelectedDevice = devices[AppConfig.NetworkCard];
                    Console.WriteLine($"启动时已选择网卡: {SelectedDevice.Description} (索引: {AppConfig.NetworkCard})");
                }
            }
            else
            {
                // 设置窗体：填充设备列表
                FormManager.SettingsForm.LoadDevices();
            }
        }

        /// <summary>
        /// 读取用户缓存
        /// </summary>
        private void LoadPlayerCache()
        {
            try
            {
                DataStorage.LoadPlayerInfoToFile();
            }
            catch (FileNotFoundException)
            {
                // 没有缓存
            }
            catch (DataTamperedException)
            {
                FormGui.Modal(this, "用户缓存错误", "用户缓存被篡改，或文件损坏。为软件正常运行，将清空用户缓存。");

                DataStorage.ClearAllPlayerInfos();
                DataStorage.SavePlayerInfoToFile();
            }
        }

        /// <summary>
        /// 软件开启后读取技能列表
        /// </summary>
        private void LoadFromEmbeddedSkillConfig()
        {
            // 1) 先用 int 键的表（已经解析过字符串）
            foreach (var kv in EmbeddedSkillConfig.AllByInt)
            {
                var id = (long)kv.Key;
                var def = kv.Value;

                // 将一条技能元数据（SkillMeta）写入 SkillBook 的全局字典中
                // 这里用的是整条更新（SetOrUpdate），如果该技能 ID 已存在则覆盖，不存在则添加
                SkillBook.SetOrUpdate(new SkillMeta
                {
                    Id = id,                         // 技能 ID（唯一标识一个技能）
                    Name = def.Name,                 // 技能名称（字符串，例如 "火球术"）
                                                     //School = def.Element.ToString(), // 技能所属元素或流派（枚举转字符串）
                                                     //Type = def.Type,                 // 技能类型（Damage/Heal/其他）——用于区分伤害技能和治疗技能
                                                     // Element = def.Element            // 技能元素类型（枚举，例如 火/冰/雷）
                });


            }

            // 2) 有些 ID 可能超出 int 或不在 AllByInt，可以再兜底遍历字符串键
            foreach (var kv in EmbeddedSkillConfig.AllByString)
            {
                if (kv.Key.TryToInt64(out var id))
                {
                    // 如果 int 表已覆盖，这里会覆盖同名；没关系，等价
                    var def = kv.Value;
                    // 将一条技能元数据（SkillMeta）写入 SkillBook 的全局字典中
                    // 这里用的是整条更新（SetOrUpdate），如果该技能 ID 已存在则覆盖，不存在则添加
                    SkillBook.SetOrUpdate(new SkillMeta
                    {
                        Id = id,                         // 技能 ID（唯一标识一个技能）
                        Name = def.Name,                 // 技能名称（字符串，例如 "火球术"）
                        //School = def.Element.ToString(), // 技能所属元素或流派（枚举转字符串）
                        //Type = def.Type,                 // 技能类型（Damage/Heal/其他）——用于区分伤害技能和治疗技能
                        //Element = def.Element            // 技能元素类型（枚举，例如 火/冰/雷）
                    });

                }
            }

            // MonsterNameResolver.Initialize(AppConfig.MonsterNames);//初始化怪物ID与名称的映射关系



            // 你也可以在这里写日志：加载了多少条技能
            // Console.WriteLine($"SkillBook loaded {EmbeddedSkillConfig.AllByInt.Count} + {EmbeddedSkillConfig.AllByString.Count} entries.");
        }

        public void SetStyle()
        {
            // # 启动与初始化事件：界面样式与渲染设置（仅 UI 外观，不涉及数据）
            // ======= 单个进度条（textProgressBar1）的外观设置 =======
            sortedProgressBarList_MainList.OrderImageOffset = new RenderContent.ContentOffset { X = 6, Y = 0 };
            sortedProgressBarList_MainList.OrderImageRenderSize = new Size(22, 22);
            sortedProgressBarList_MainList.OrderOffset = new RenderContent.ContentOffset { X = 32, Y = 0 };
            sortedProgressBarList_MainList.OrderCallback = (i) => $"{i:d2}.";
            sortedProgressBarList_MainList.OrderImages = [HandledAssets.皇冠];


            sortedProgressBarList_MainList.OrderColor =
                Config.IsLight ? Color.Black : Color.White;

            sortedProgressBarList_MainList.OrderFont = AppConfig.SaoFont;

            // ======= 进度条列表（sortedProgressBarList1）的初始化与外观 =======
            sortedProgressBarList_MainList.ProgressBarHeight = AppConfig.ProgressBarHeight;  // 每行高度
        }

        /// <summary>
        /// 通用提示气泡
        /// </summary>
        /// <param name="control"></param>
        /// <param name="text"></param>
        /// <remarks>
        /// 通用封装：在指定控件上显示提示文本
        /// </remarks>
        private void ToolTip(System.Windows.Forms.Control control, string text)
        {
            var tooltip = new TooltipComponent()
            {
                Font = HandledAssets.HarmonyOS_Sans(8),
                ArrowAlign = TAlign.TL
            };
            tooltip.SetTip(control, text);
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
        public async void StartCapture()
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

            await Task.Delay(1000);
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

        #endregion
        #endregion

        private void HandleMouseThrough()
        {
            if (!MousePenetrationHelper.IsPenetrating(this.Handle))
            {
                // 方案 O：AppConfig.Transparency 现在表示“不透明度百分比”
                MousePenetrationHelper.SetMousePenetrate(this, enable: true, opacityPercent: AppConfig.Transparency);
            }
            else
            {
                MousePenetrationHelper.SetMousePenetrate(this, enable: false);
            }
        }

        private void HandleClearAllData()
        {
            DataStorage.ClearAllDpsData();

            _fullBattleTimer.Reset();
            _battleTimer.Reset();
        }

        private void HandleClearData()
        {
            DataStorage.ClearDpsData();

            _battleTimer.Reset();
        }

        private void UpdateBattleTimerText()
        {
            label_BattleTimeText.Text = TimeSpan.FromTicks(InUsingTimer.ElapsedTicks).ToString(@"hh\:mm\:ss");
        }

    }
}
