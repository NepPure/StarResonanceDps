using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Properties;

namespace StarResonanceDpsAnalysis
{
    public partial class MainForm : BorderlessForm
    {
        #region ========== 字段与常量 ==========

        #region —— 抓包设备/统计 —— 

        private ICaptureDevice? selectedDevice;
        private bool _isCaptureStarted = false;

        #endregion

        #region —— 计时器&超时控制 —— 

        private System.Timers.Timer? statsTimer;
        private readonly Stopwatch _combatWatch = new Stopwatch();

        #endregion

        #region —— 键盘钩子/UI状态 —— 

        private KeyboardHook? kbHook;
        private bool Top = false;

        #endregion

        #region —— 历史记录/数据结构 —— 

        Dictionary<string, BindingList<DpsTable>> HistoricalRecords = [];
        private readonly BlockingCollection<(ICaptureDevice? dev, RawCapture raw)> _queue = new(8192);

        #endregion

        #region —— 多线程处理 —— 

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task[] _workerTasks = Array.Empty<Task>();
        // private readonly int _workerCount = Math.Max(2, Environment.ProcessorCount / 2);

        #endregion

        #region —— 内部类型 —— 

        private class PacketData(object packet)
        {
            public object Packet { get; set; } = packet;
        }

        #endregion

        #endregion

        #region ========== 构造与启动加载 ==========

        public MainForm()
        {

            InitializeComponent();
            FormGui.SetDefaultGUI(this);

            /* Application.ProductVersion 默认会被 MSBuild 附加 Git 哈希, 
             * 如: "1.0.0+123456789acbdef", 
             * 将 + 后面去掉就是项目属性的版本号,
             * 这样可以让生成文件的版本号与标题版本号一致
             * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            pageHeader_MainHeader.Text += $" v{Application.ProductVersion.Split('+')[0]}";

            LoadTableColumnVisibilitySettings();
            ToggleTableView();
            LoadNetworkDevices();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            #region —— 键盘钩子初始化 ——

            kbHook = new KeyboardHook();
            kbHook.SetHook();
            kbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent;

            #endregion

            FormGui.SetColorMode(this, AppConfig.IsLight);

            RefreshHotKeyTips();
        }
        #endregion

        #region ========== 启动时设备/表格配置 ==========

        /// <summary>
        /// 启动时加载网卡设备
        /// </summary>
        private void LoadNetworkDevices()
        {
            Console.WriteLine("应用程序启动时加载网卡...");

            if (AppConfig.NetworkCard >= 0)
            {
                var devices = CaptureDeviceList.Instance;
                if (AppConfig.NetworkCard < devices.Count)
                {
                    selectedDevice = devices[AppConfig.NetworkCard];
                    Console.WriteLine($"启动时已选择网卡: {selectedDevice.Description} (索引: {AppConfig.NetworkCard})");
                }
            }
            else
            {
                using var setup = new Setup(this);
                setup.LoadDevices();
            }
        }

        /// <summary>
        /// 用于加载数据记录表格列名
        /// </summary>
        private void LoadTableColumnVisibilitySettings()
        {
            foreach (var item in ColumnSettingsManager.AllSettings)
            {
                string strValue = AppConfig.GetValue("TableSet", item.Key, string.Empty);
                item.IsVisible = strValue == "True";
            }
        }

        #endregion

        #region ========== 热键/交互事件 ==========
        #region —— 全局热键 —— 

        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == AppConfig.MouseThroughKey) { HandleMouseThrough(); }
            else if (e.KeyData == AppConfig.FormTransparencyKey) { HandleFormTransparency(); }
            else if (e.KeyData == AppConfig.WindowToggleKey) { HandleSwitchMonitoring(); }
            else if (e.KeyData == AppConfig.ClearDataKey) { HandleClearData(); }
            else if (e.KeyData == AppConfig.ClearHistoryKey) { HandleClearHistory(); }
        }
        #endregion

        #region —— 按钮/复选框/下拉事件 —— 
        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight;

            button_ThemeSwitch.Toggle = !AppConfig.IsLight;

            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(Common.skillDiary, AppConfig.IsLight);
            FormGui.SetColorMode(Common.userUidSet, AppConfig.IsLight);
        }

        private void button_AlwaysOnTop_Click(object sender, EventArgs e)
        {
            Top = !Top;

            button_AlwaysOnTop.Toggle = Top;
            TopMost = Top;
        }

        private void checkbox_PersentData_CheckedChanged(object sender, BoolEventArgs e)
        {
            ToggleTableView();
        }

        private void dropdown_History_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            if (monitor)
            {
                MessageBox.Show("请先停止监控后再查看历史数据");
                return;
            }

            TableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            ShowHistoricalDps(e.Value.ToString());
            dropdown_History.SelectedValue = -1;
        }

        private void button_SkillDiary_Click(object sender, EventArgs e)
        {
            FormGui.Modal(this, "正在开发", "正在开发");
            return;
            if (Common.skillDiary == null || Common.skillDiary.IsDisposed)
            {
                Common.skillDiary = new SkillDiary();
            }
            Common.skillDiary.Show();
        }

        private void switch_IsMonitoring_CheckedChanged(object sender, BoolEventArgs e)
        {
            if (monitor == false)
            {
                StartCapture();

                if (AppConfig.NetworkCard == -1) return;

                timer_RefreshDpsTable.Enabled = true;
                pageHeader_MainHeader.SubText = "监控已开启";
                monitor = true;
                _combatWatch.Restart();
                timer_RefreshRunningTime.Start();

                TableDatas.DpsTable.Clear();
                StatisticData._manager.ClearAll();
            }
            else
            {
                pageHeader_MainHeader.SubText = "监控已关闭";
                StopCapture();
                label_SettingTip.Text = "00:00";
                timer_RefreshDpsTable.Enabled = false;
                monitor = false;
                timer_RefreshRunningTime.Stop();
                _combatWatch.Stop();
            }
        }

        private void button_Settings_MouseClick(object sender, MouseEventArgs e)
        {
            IContextMenuStripItem[] menulist = new AntdUI.IContextMenuStripItem[]
            {
                new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up, },
                new ContextMenuStripItem("数据显示设置"){ IconSvg = Resources.data_display, },
                new ContextMenuStripItem("用户UID设置"){ IconSvg = Resources.userUid, },
            };

            AntdUI.ContextMenuStrip.open(this, it =>
            {
                switch (it.Text)
                {
                    case "基础设置":
                        OpenSettingsDialog(); break;
                    case "数据显示设置":
                        dataDisplay(); break;
                    case "用户UID设置":
                        if (Common.userUidSet == null || Common.userUidSet.IsDisposed)
                        {
                            Common.userUidSet = new UserUidSet();
                        }
                        Common.userUidSet.Show();
                        break;
                }
            }, menulist);
        }

        #endregion
        #endregion

        private PacketAnalyzer _packetAnalyzer;
        private volatile int _stopping = 0; // 0=运行,1=停止中

        /// <summary>
        /// 数据包到达事件
        /// </summary>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            if (Volatile.Read(ref _stopping) == 1) return; // 停止中，丢包

            try
            {
                var dev = (ICaptureDevice)sender;
                _packetAnalyzer.Enqueue(dev, e.GetPacket()); // 只入队
                //ServerAddressResolver.OnAnyPacketArrived(); // 记录最后到包时间

            }
            catch (InvalidOperationException)
            {
                // _queue 已 CompleteAdding，正常关闭过程
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[数据包入队异常] {ex}");
            }
        }

        #region ========== 计时器Tick事件 ==========

        private void timer_RefreshDpsTable_Tick(object sender, EventArgs e)
        {
            // Task.Run(() => RefreshDpsTable());
        }

        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e)
        {
            label_SettingTip.Text = _combatWatch.Elapsed.ToString(@"mm\:ss");
        }

        #endregion

    }
}
