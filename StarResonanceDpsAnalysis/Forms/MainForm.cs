using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

using AntdUI;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Properties;
using ZstdNet;

namespace StarResonanceDpsAnalysis
{
    public partial class MainForm : BorderlessForm
    {
        #region ========== 字段与常量 ==========
        #region —— 抓包设备/统计 —— 
        private ICaptureDevice? selectedDevice;
        private ICaptureStatistics _lastStats;
        private bool _hasAppliedFilter = false;
        private bool _isCaptureStarted = false;
        #endregion

        #region —— 计时器&超时控制 —— 
        private System.Timers.Timer? _restartCaptureTimer;
        private const double NO_PACKET_TIMEOUT_SECONDS = 5; // 无数据包超时时间（秒）
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
        private class PacketData
        {
            public object Packet { get; set; }
            public PacketData(object packet) { Packet = packet; }
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

            #region —— 配置读取（网卡/透明度/颜色/主题/热键） ——
            AppConfig.Reader.Load(AppConfig.ConfigIni);//加载配置文件
            if (!string.IsNullOrEmpty(AppConfig.Reader.GetValue("SetUp", "NetworkCard")))
            {
                AppConfig.NetworkCard = Convert.ToInt32(Convert.ToInt32(AppConfig.Reader.GetValue("SetUp", "NetworkCard")));
                label_SettingTip.Visible = false;
            }
            else
            {
                AppConfig.NetworkCard = -1;
            }

            if (!string.IsNullOrEmpty(AppConfig.Reader.GetValue("SetUp", "Transparency")))
                AppConfig.Transparency = Convert.ToInt32(AppConfig.Reader.GetValue("SetUp", "Transparency"));
            else
                AppConfig.Transparency = 100;

            var colorStr = AppConfig.Reader.GetValue("SetUp", "DpsColor");
            if (!string.IsNullOrWhiteSpace(colorStr))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    colorStr,
                    @"A=(\d+), R=(\d+), G=(\d+), B=(\d+)"
                );

                if (match.Success &&
                    byte.TryParse(match.Groups[1].Value, out byte a) &&
                    byte.TryParse(match.Groups[2].Value, out byte r) &&
                    byte.TryParse(match.Groups[3].Value, out byte g) &&
                    byte.TryParse(match.Groups[4].Value, out byte b))
                {
                    AppConfig.DpsColor = Color.FromArgb(a, r, g, b);
                }
                else
                {
                    AppConfig.DpsColor = Color.FromArgb(252, 227, 138);
                }
            }
            else
            {
                AppConfig.DpsColor = Color.FromArgb(252, 227, 138);
            }

            AppConfig.Reader.Load(AppConfig.ConfigIni);//加载配置文件
            AppConfig.IsLight = AppConfig.Reader.GetValue("SetUp", "IsLight") == "True";

            FormGui.SetColorMode(this, AppConfig.IsLight);
            button_ThemeSwitch.Toggle = !AppConfig.IsLight;

            AppConfig.Reader.Load(AppConfig.ConfigIni);

            string raw;
            if (!string.IsNullOrWhiteSpace(raw = AppConfig.Reader.GetValue("SetKey", "MouseThroughKey"))
                 && Enum.TryParse(raw, out Keys k1))
            {
                AppConfig.MouseThroughKey = k1;
            }
            if (!string.IsNullOrWhiteSpace(raw = AppConfig.Reader.GetValue("SetKey", "FormTransparencyKey"))
                 && Enum.TryParse(raw, out Keys k2))
            {
                AppConfig.FormTransparencyKey = k2;
            }
            if (!string.IsNullOrWhiteSpace(raw = AppConfig.Reader.GetValue("SetKey", "WindowToggleKey"))
                 && Enum.TryParse(raw, out Keys k3))
            {
                AppConfig.WindowToggleKey = k3;
            }
            if (!string.IsNullOrWhiteSpace(raw = AppConfig.Reader.GetValue("SetKey", "ClearDataKey"))
                 && Enum.TryParse(raw, out Keys k4))
            {
                AppConfig.ClearDataKey = k4;
            }
            if (!string.IsNullOrWhiteSpace(raw = AppConfig.Reader.GetValue("SetKey", "ClearHistoryKey"))
                 && Enum.TryParse(raw, out Keys k5))
            {
                AppConfig.ClearHistoryKey = k5;
            }

            string labe = @$"{AppConfig.MouseThroughKey}：鼠标穿透 | {AppConfig.FormTransparencyKey}：窗体透明 | {AppConfig.WindowToggleKey}：开启/关闭 | {AppConfig.ClearDataKey}：清空数据 | {AppConfig.ClearHistoryKey}：清空历史";
            label_HotKeyTips.Text = labe;
            #endregion
        }
        #endregion

        #region ========== 启动时设备/表格配置 ==========
        /// <summary>启动时加载网卡设备</summary>
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
                using (var setup = new Setup(this))
                {
                    setup.LoadDevices();
                }
            }
        }

        /// <summary>用于加载数据记录表格列名</summary>
        private void LoadTableColumnVisibilitySettings()
        {
            foreach (var item in ColumnSettingsManager.AllSettings)
            {
                AppConfig.Reader.Load(AppConfig.ConfigIni);
                string? strValue = AppConfig.Reader.GetValue("TabelSet", item.Key);
                if (strValue != null)
                {
                    item.IsVisible = strValue == "True";
                }
            }
        }
        #endregion

        #region ========== 热键/交互事件 ==========
        #region —— 全局热键 —— 
        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e)
        {
            // TODO: 后续会改成 Dictionary 触发, 维护热键与行为列表就可以了
            if (e.KeyData == AppConfig.MouseThroughKey) { HandleMouseThrough(); }
            if (e.KeyData == AppConfig.FormTransparencyKey) { HandleFormTransparency(); }
            if (e.KeyData == AppConfig.WindowToggleKey) { HandleSwitchMonitoring(); }
            if (e.KeyData == AppConfig.ClearDataKey) { HandleClearData(); }
            if (e.KeyData == AppConfig.ClearHistoryKey) { HandleClearHistory(); }
        }
        #endregion

        #region —— 按钮/复选框/下拉事件 —— 
        private void button2_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight;
            button_ThemeSwitch.Toggle = !AppConfig.IsLight;
            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(Common.skillDiary, AppConfig.IsLight);
            FormGui.SetColorMode(Common.userUidSet, AppConfig.IsLight);

            AppConfig.Reader.Load(AppConfig.ConfigIni);
            AppConfig.Reader.SaveValue("SetUp", "IsLight", AppConfig.IsLight.ToString());
            AppConfig.Reader.Save(AppConfig.ConfigIni);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Top = !Top;
            button_AlwaysOnTop.Toggle = Top;
            this.TopMost = Top;
        }

        private void checkbox1_CheckedChanged(object sender, BoolEventArgs e)
        {
            ToggleTableView();
        }

        private void dropdown1_SelectedValueChanged(object sender, ObjectNEventArgs e)
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

        private void button1_Click(object sender, EventArgs e)
        {
            FormGui.Modal(this, "正在开发", "正在开发");
            return;
            if (Common.skillDiary == null || Common.skillDiary.IsDisposed)
            {
                Common.skillDiary = new SkillDiary();
            }
            Common.skillDiary.Show();
        }

        private void switch1_CheckedChanged(object sender, BoolEventArgs e)
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

        private void button4_MouseClick(object sender, MouseEventArgs e)
        {
            IContextMenuStripItem[] menulist = new AntdUI.IContextMenuStripItem[]
            {
                new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up, },
                new ContextMenuStripItem("数据显示设置"){ IconSvg = Resources.data_display, },
                new ContextMenuStripItem("用户UID设置"){ IconSvg = Resources.userUid, },
            };

            AntdUI.ContextMenuStrip.open(this, async it =>
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

        private void dropdown1_SelectedValueChanged_1(object sender, ObjectNEventArgs e)
        {
            switch (e.Value)
            {
                case "基础设置":
                    break;
            }
        }
        #endregion
        #endregion

        #region ========== 历史记录：保存/显示 ==========
        private void SaveCurrentDpsSnapshot()
        {
            if (TableDatas.DpsTable.Count == 0) return;

            string timeOnly = @$"结束时间：{DateTime.Now:HH:mm:ss}";
            var snapshot = new BindingList<DpsTable>();

            foreach (var item in TableDatas.DpsTable)
            {
                string nickname = item.nickname;
                double.TryParse(item.critRate.TrimEnd('%'), out var cr);
                double.TryParse(item.luckyRate.TrimEnd('%'), out var lr);

                snapshot.Add(new DpsTable(
                    item.uid,
                    nickname: nickname,
                    item.damageTaken,
                    item.totalHealingDone,
                    item.criticalHealingDone,
                    item.luckyHealingDone,
                    item.critLuckyHealingDone,
                    item.instantHps,
                    item.maxInstantHps,
                    item.profession,
                    item.totalDamage,
                    item.criticalDamage,
                    item.luckyDamage,
                    item.critLuckyDamage,
                    cr.ToString(),
                    lr.ToString(),
                    item.instantDps,
                    item.maxInstantDps,
                    item.totalDps,
                    item.totalHps,
                    new CellProgress(item.CellProgress?.Value ?? 0)
                    {
                        Size = new Size(300, 10),
                        Fill = AppConfig.DpsColor
                    }
                ));
            }

            HistoricalRecords[timeOnly] = snapshot;
            dropdown_History.Items.Add(timeOnly);
            dropdown_History.SelectedValue = -1;
        }

        private void ShowHistoricalDps(string timeKey)
        {
            if (!HistoricalRecords.TryGetValue(timeKey, out var recordList))
            {
                MessageBox.Show($"未找到时间 {timeKey} 的历史记录");
                return;
            }

            foreach (var item in recordList)
            {
                double.TryParse(item.critRate.TrimEnd('%'), out var cr);
                double.TryParse(item.luckyRate.TrimEnd('%'), out var lr);

                Plugin.TableDatas.DpsTable.Add(new DpsTable(
                    item.uid,
                    item.nickname,
                    item.damageTaken,
                    item.totalHealingDone,
                    item.criticalHealingDone,
                    item.luckyHealingDone,
                    item.critLuckyHealingDone,
                    item.instantHps,
                    item.maxInstantHps,
                    item.profession,
                    item.totalDamage,
                    item.criticalDamage,
                    item.luckyDamage,
                    item.critLuckyDamage,
                    cr.ToString(),
                    lr.ToString(),
                    item.instantDps,
                    item.maxInstantDps,
                    item.totalDps,
                    item.totalHps,
                    new CellProgress(item.CellProgress?.Value ?? 0)
                    {
                        Size = new Size(300, 10),
                        Fill = AppConfig.DpsColor
                    }
                ));
            }
        }
        #endregion

        #region ========== 抓包：开始/停止/事件/统计 ==========
        /// <summary>开始抓包</summary>
        private void StartCapture()
        {
            Volatile.Write(ref _stopping, 0);

            #region —— 前置校验与设备打开 —— 
            if (AppConfig.NetworkCard < 0)
            {
                MessageBox.Show("请选择一个网卡设备");
                pageHeader_MainHeader.SubText = "监控已关闭";
                return;
            }

            var devices = CaptureDeviceList.Instance;
            if (devices == null || devices.Count == 0)
                throw new InvalidOperationException("没有找到可用的网络抓包设备");

            if (AppConfig.NetworkCard < 0 || AppConfig.NetworkCard >= devices.Count)
                throw new InvalidOperationException($"无效的网络设备索引: {AppConfig.NetworkCard}");

            selectedDevice = devices[AppConfig.NetworkCard];
            if (selectedDevice == null)
                throw new InvalidOperationException($"无法获取网络设备，索引: {AppConfig.NetworkCard}");

            selectedDevice.Open(DeviceModes.Promiscuous, 1000);
            #endregion

            #region —— 启动统计/事件注册 —— 
            InitStatsTimer();
            timer_RefreshDpsTable.Enabled = true;
            pageHeader_MainHeader.SubText = "监控已开启";
            monitor = true;
            _combatWatch.Restart();
            timer_RefreshRunningTime.Start();
            if (label_SettingTip.Visible == false)
            {
                label_SettingTip.Visible = true;
                label_SettingTip.Text = "00:00";
            }
            #endregion

            #region —— 清空当前统计 —— 
            TableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            #endregion
        }

        /// <summary>停止抓包</summary>
        private async void StopCapture()
        {
            Volatile.Write(ref _stopping, 1);

            #region —— 保存快照 —— 
            if (TableDatas.DpsTable.Count > 0)
            {
                SaveCurrentDpsSnapshot();
            }
            #endregion

            #region —— 停止设备与事件反注册 —— 
            _packetAnalyzer.Stop();
            if (selectedDevice != null)
            {
                try
                {
                    selectedDevice.OnPacketArrival -= Device_OnPacketArrival;
                    selectedDevice.StopCapture();
                    selectedDevice.Close();
                    Console.WriteLine("停止抓包");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"停止抓包异常: {ex.Message}");
                }
                selectedDevice = null;
            }
   
            #endregion

            #region —— 停止统计定时器 —— 
            if (statsTimer != null)
            {
                statsTimer.Stop();
                statsTimer.Dispose();
                statsTimer = null;
            }
            #endregion

            #region —— 停止工作线程（带超时） —— 
            if (_cancellationTokenSource != null)
            {
                var cts = _cancellationTokenSource;
                var tasks = _workerTasks;

                cts.Cancel();
                try
                {
                    var pending = tasks?
                        .Where(t => t != null && !t.IsCompleted)
                        .ToArray() ?? Array.Empty<Task>();

                    if (pending.Length > 0)
                    {
                        var all = Task.WhenAll(pending);
                        var finished = await Task.WhenAny(all, Task.Delay(3000));
                        if (finished != all)
                        {
                            Console.WriteLine("[关闭] 等待任务超时（>3s），后续让任务自行收尾。");
                        }
                        else
                        {
                            await all;
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (AggregateException aex)
                {
                    foreach (var inner in aex.Flatten().InnerExceptions)
                        Console.WriteLine($"[线程终止异常] {inner.Message}");
                }
                finally
                {
                    cts.Dispose();
                    _cancellationTokenSource = null;
                    _workerTasks = Array.Empty<Task>();
                }
            }
            #endregion

            #region —— 状态复位/计时器复位 —— 
            _hasAppliedFilter = false;
            _isCaptureStarted = false;
            label_SettingTip.Text = "00:00";
            timer_RefreshDpsTable.Enabled = false;
            monitor = false;
            timer_RefreshRunningTime.Stop();
            _combatWatch.Stop();
            #endregion
        }

        /// <summary>初始化统计/注册回调并启动</summary>
        private void InitStatsTimer()
        {
            if (statsTimer != null)
            {
                statsTimer.Stop();
                statsTimer.Dispose();
            }

            #region —— 丢包统计（保留注释块原样） —— 
            //statsTimer = new System.Timers.Timer(statsInterval);
            //statsTimer.AutoReset = true;  // 设置为自动重置，定期触发
            //lastStatsTime = DateTime.Now;
            //statsTimer.Elapsed += (s, e) => { ... };
            //statsTimer.Start();
            #endregion

            #region —— 注册抓包事件并启动 —— 
            selectedDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            selectedDevice.StartCapture();
         


            if (!_isCaptureStarted)
            {
                Console.WriteLine("开始抓包...");
                // 启动抓包的时候创建一次
                _packetAnalyzer = new PacketAnalyzer();
                _packetAnalyzer.Start();
                // Console.WriteLine($"已启动 {_workerCount} 个工作线程处理数据包");
                _isCaptureStarted = true;
            }
            #endregion
        }

        // 启动 N 个常驻 worker


        private PacketAnalyzer _packetAnalyzer;
        private volatile int _stopping = 0; // 0=运行,1=停止中

        /// <summary>数据包到达事件</summary>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            if (Volatile.Read(ref _stopping) == 1) return; // 停止中，丢包

            try
            {
                var dev = (ICaptureDevice)sender;
                _packetAnalyzer.Enqueue(dev, e.GetPacket()); // 只入队
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
            //try
            //{
            //    // 提取数据包并包装后添加到队列中，由工作线程处理
            //    _ = new PacketAnalyzer(selectedDevice, e.GetPacket()).Start();
            //}
            //catch (OperationCanceledException)
            //{
            //    // 预期的取消操作，不记录异常
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"[数据包入队异常] {ex}");
            //}
        }

 
        #endregion

        #region ========== 计时器Tick事件 ==========
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Task.Run(() => RefreshDpsTable());
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label_SettingTip.Text = _combatWatch.Elapsed.ToString(@"mm\:ss");
        }
        #endregion

        #region ========== 设置/对话框 ==========
        /// <summary>打开基础设置面板</summary>
        private void OpenSettingsDialog()
        {
            using (var form = new Setup(this))
            {
                form.inputNumber1.Value = (decimal)AppConfig.Transparency;
                form.colorPicker1.Value = AppConfig.DpsColor;
                string title = AntdUI.Localization.Get("systemset", "请选择网卡");
                AntdUI.Modal.open(new AntdUI.Modal.Config(this, title, form, TType.Info)
                {
                    CloseIcon = true,
                    BtnHeight = 0,
                });

                AppConfig.Transparency = (double)form.inputNumber1.Value;
                if (AppConfig.Transparency < 10)
                {
                    AppConfig.Transparency = 100;
                    MessageBox.Show("透明度不能低于10%，已自动设置为100%");
                }

                string labe = @$"{AppConfig.MouseThroughKey}：鼠标穿透 | {AppConfig.FormTransparencyKey}：窗体透明 | {AppConfig.WindowToggleKey}：开启/关闭 | {AppConfig.ClearDataKey}：清空数据 | {AppConfig.ClearHistoryKey}：清空历史";
                label_HotKeyTips.Text = labe;

                AppConfig.Reader.Load(AppConfig.ConfigIni);
                AppConfig.Reader.SaveValue("SetUp", "NetworkCard", AppConfig.NetworkCard.ToString());
                AppConfig.Reader.SaveValue("SetUp", "Transparency", form.inputNumber1.Value.ToString());
                AppConfig.Reader.SaveValue("SetUp", "DpsColor", AppConfig.DpsColor.ToString());
                AppConfig.Reader.SaveValue("SetKey", "MouseThroughKey", AppConfig.MouseThroughKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "FormTransparencyKey", AppConfig.FormTransparencyKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "WindowToggleKey", AppConfig.WindowToggleKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "ClearDataKey", AppConfig.ClearDataKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "ClearHistoryKey", AppConfig.ClearHistoryKey.ToString());
                AppConfig.Reader.Save(AppConfig.ConfigIni);

                label_SettingTip.Visible = false;
            }
        }

        private void dataDisplay()
        {
            using (var form = new DataDisplaySettings(this))
            {
                AppConfig.Reader.Load(AppConfig.ConfigIni);
                string title = Localization.Get("DataDisplaySettings", "请勾选需要显示的统计");
                AntdUI.Modal.open(new Modal.Config(this, title, form, TType.Info)
                {
                    CloseIcon = true,
                    BtnHeight = 0,
                });

                table_DpsDataTable.Columns = ColumnSettingsManager.BuildColumns(checkbox_PersentData.Checked);
                if (!checkbox_PersentData.Checked)
                {
                    table_DpsDataTable.StackedHeaderRows = ColumnSettingsManager.BuildStackedHeader();
                }
            }
        }
        #endregion
    }
}
