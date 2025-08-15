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

        public static void HandleClearData()
        {

            // 先停止所有图表的自动刷新
            ChartVisualizationService.StopAllChartsAutoRefresh();

            // 在清空数据前，通知图表服务战斗结束
            ChartVisualizationService.OnCombatEnd();

            DpsTableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            SkillTableDatas.SkillTable.Clear();
            DictList.Clear();

            // 强制清空进度条列表控件的数据缓存（确保在 UI 线程调用）
            //try
            //{
            //    var ctrl = DpsStatisticsForm.sortedProgressBarList1;
            //    if (ctrl != null)
            //    {
            //        if (ctrl.InvokeRequired)
            //        {
            //            ctrl.BeginInvoke(new Action(() =>
            //            {
            //                ctrl.Data = null; // 置空以触发控件内部清理
            //                ctrl.Invalidate();
            //                ctrl.Refresh();
            //            }));
            //        }
            //        else
            //        {
            //            ctrl.Data = null; // 置空以触发控件内部清理
            //            ctrl.Invalidate();
            //            ctrl.Refresh();
            //        }
            //    }
            //}
            //catch { }

            // 完全重置所有图表（包括清空历史数据和重置视图状态）
            ChartVisualizationService.FullResetAllCharts();

            // 如果当前正在抓包，重新启动图表自动刷新
            if (IsCaptureStarted)
            {
                ChartVisualizationService.StartAllChartsAutoRefresh(1000);
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
            sortedProgressBarList1.ProgressBarHeight = 50;  // 每行高度
            sortedProgressBarList1.AnimationDuration = 1000; // 动画时长（毫秒）
            sortedProgressBarList1.AnimationQuality = Quality.High; // 动画品质（你项目里的枚举）




        }
        readonly static Dictionary<long, List<RenderContent>> DictList = new Dictionary<long, List<RenderContent>>();

        List<ProgressBarData> list = [];

        List<ProgressBarData> userList = [];

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

        Dictionary<string, byte[]> imgDict = new Dictionary<string, byte[]>()
        {
            { "冰魔导师", Resources.冰魔导师 },
            { "巨刃守护者", Resources.巨刃守护者 },
            { "森语者", Resources.森语者 },
            { "灵魂乐手", Resources.灵魂乐手 },
            { "神射手", Resources.神射手 },
            { "雷影剑士", Resources.雷影剑士 },
            { "青岚骑士", Resources.青岚骑士 },
            {"未知",null }
        };
        public void RefreshDpsTable()
        {
            var statsList = StatisticData._manager.GetPlayersWithCombatData().ToArray();
            if (statsList.Count() == 0) return;

            float totalDamageSum = statsList
                .Where(p => p?.DamageStats != null)
                .Sum(p => (float)p.DamageStats.Total);
            if (totalDamageSum <= 0f) totalDamageSum = 1f;

            var maxDamage = statsList.Max(p => (float)(p?.DamageStats?.Total ?? 0));
            var ordered = statsList
                .OrderByDescending(p => p?.DamageStats?.Total ?? 0)
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {


                var p = ordered[i];
                var uid = (long)p.Uid;
                int ranking = i + 1;

                var realtime = Common.FormatWithEnglishUnits(Math.Round(p.DamageStats.GetTotalPerSecond(), 1));
                string totalFmt = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                string share = (p.DamageStats.Total / totalDamageSum * 100).ToString("0") + "%";

                float progress = maxDamage > 0 ? (float)(p.DamageStats.Total / maxDamage) : 0f;
                var ProfessionImage = imgDict[p.Profession];
                var existing = DictList.Any(x => x.Key == uid);

                if (!DictList.TryGetValue(uid, out var data))
                {
                    list.Add(
                        new ProgressBarData
                        {
                            ID = uid,
                            ContentList = data,
                            ProgressBarCornerRadius = 3,
                            ProgressBarValue = progress,
                            ProgressBarColor = colorDict[p.Profession],
                        }
                        );
                    data =
                       [
                           new RenderContent
                           {
                               Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                 Offset = new RenderContent.ContentOffset { X = 10, Y = 0 },
                                  ForeColor = Color.Black,
                                  Font = AppConfig.SaoFontBold,
                           },
                           new RenderContent
                           {
                                Type = RenderContent.ContentType.Image,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 48, Y = 0 },
                                Image = new Bitmap(new MemoryStream(ProfessionImage)),
                                ImageRenderSize = new Size(32, 32)
                           },
                           new RenderContent
                           {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 90, Y = 0 },
                                ForeColor = Color.Black,
                                Font = AppConfig.SaoFontBold,
                           },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleRight,
                                Offset = new RenderContent.ContentOffset { X = -90, Y = 4 },
                                ForeColor = Color.Black,
                                Font = AppConfig.SaoFontBold,
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleRight,
                                Offset = new RenderContent.ContentOffset { X = 0, Y = 0 },
                                ForeColor = Color.Black,
                                Font =AppConfig.SaoFontBold,
                            },
                       ];

                    DictList[uid] = data; 
                }

                // 别忘了放回字典
                
                DictList[uid][0].Text = $"{ranking}.";
                DictList[uid][1].Image = new Bitmap(new MemoryStream(ProfessionImage));
                DictList[uid][2].Text = $"{p.Nickname}({p.CombatPower})";
                DictList[uid][3].Text = $"{totalFmt}({realtime})";
                DictList[uid][4].Text = $"{share}%";
               
              
                if(p.Uid == AppConfig.Uid)
                {

                    textProgressBar1.ContentList =
                   [
                       new RenderContent
                                {
                                        Type = RenderContent.ContentType.Text,
                                        Align = RenderContent.ContentAlign.MiddleLeft,
                                        Offset = new RenderContent.ContentOffset { X = 10, Y = 20 },
                                        Text =  $"  {ranking} {p.Nickname} ({p.CombatPower})      {totalFmt} ({realtime}) {share}",

                                        ForeColor =Color.Black,
                                        Font = AppConfig.SaoFontBold,
                                }
                   ];
                    textProgressBar1.ProgressBarValue = progress;
                    textProgressBar1.ProgressBarColor = colorDict[p.Profession];

                }
       
            }
            // 如果有控件需要刷新，可以在这里重新绑定一次数据
            sortedProgressBarList1.Data = list;


        }

        /// <summary>
        /// 从 Properties.Resources 中按名字获取图片；兼容资源为 Image 或 byte[] 两种情况。
        /// 返回 null 表示未找到或格式不正确。
        /// </summary>
     


    }
}
