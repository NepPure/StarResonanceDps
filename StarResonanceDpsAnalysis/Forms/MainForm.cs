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
        // 当前选中的网络抓包设备（例如某个网卡）
        private ICaptureDevice? selectedDevice;

        // 用于检测长时间无数据包并重启抓包的定时器
        private System.Timers.Timer? _restartCaptureTimer;
        private const double NO_PACKET_TIMEOUT_SECONDS = 5; // 无数据包超时时间（秒）

        // 键盘钩子
        private KeyboardHook? kbHook;

        public MainForm()
        {
            InitializeComponent();

            FormGui.SetDefaultGUI(this);

            /* Application.ProductVersion 默认会被 MSBuild 附加 Git 哈希, 
             * 如: "1.0.0+123456789acbdef", 
             * 将 + 后面去掉就是项目属性的版本号,
             * 这样可以让生成文件的版本号与标题版本号一致
             * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            pageHeader_MainHeader.Text += $" v{Application.ProductVersion.Split('+')[0]}";

            LoadTableColumnVisibilitySettings();
            ToggleTableView();
            LoadNetworkDevices();
        }

        /// <summary>
        /// 启动时加载网卡设备
        /// </summary>
        private void LoadNetworkDevices()
        {
            Console.WriteLine("应用程序启动时加载网卡...");



            // 如果配置中指定了网卡索引，直接使用
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
                // 创建临时Setup对象用于调用LoadDevices方法
                using (var setup = new Setup(this))
                {
                    setup.LoadDevices();
                }
            }
        }



        /// <summary>
        /// 用于加载数据记录表格列名
        /// </summary>
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



        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e)
        {
            // TODO: 后续会改成 Dictionary 触发, 维护热键与行为列表就可以了

            //F6 鼠标穿透
            if (e.KeyData == AppConfig.MouseThroughKey)
            {
                HandleMouseThrough();
            }
            //F7 窗体透明
            if (e.KeyData == AppConfig.FormTransparencyKey)
            {
                HandleFormTransparency();
            }
            //F8 开启/关闭 监控
            if (e.KeyData == AppConfig.WindowToggleKey)
            {
                
                HandleSwitchMonitoring();
            }
            //F9 清空数据
            if (e.KeyData == AppConfig.ClearDataKey)
            {
                HandleClearData();

            }//F10 清空历史记录
            if (e.KeyData == AppConfig.ClearHistoryKey)
            {
                HandleClearHistory();
            }
        }


        Dictionary<string, BindingList<DpsTable>> HistoricalRecords = [];

        private void SaveCurrentDpsSnapshot()
        {
            if (TableDatas.DpsTable.Count == 0)
            {
                return;
            }

            string timeOnly = @$"结束时间：{DateTime.Now:HH:mm:ss}";

            var snapshot = new BindingList<DpsTable>();
            foreach (var item in TableDatas.DpsTable)
            {

                string nickname = item.nickname;
                // 先把 critRate / luckyRate 的 "%" 去掉再解析成 double
                double.TryParse(item.critRate.TrimEnd('%'), out var cr);
                double.TryParse(item.luckyRate.TrimEnd('%'), out var lr);
                snapshot.Add(new DpsTable(
                    // —— 受伤 & 治疗 —— 
                    item.uid,
                    nickname: nickname,
                    item.damageTaken,             // 累计受到的伤害
                    item.totalHealingDone,        // 总治疗量
                    item.criticalHealingDone,     // 暴击治疗量
                    item.luckyHealingDone,        // 幸运治疗量
                    item.critLuckyHealingDone,    // 暴击+幸运治疗量
                    item.instantHps,              // 瞬时 HPS
                    item.maxInstantHps,           // 峰值 HPS

                    // —— 职业 & DPS —— 
                    item.profession,              // 职业
                    item.totalDamage,             // 总伤害
                    item.criticalDamage,          // 暴击伤害
                    item.luckyDamage,             // 幸运伤害
                    item.critLuckyDamage,         // 暴击+幸运伤害
                    cr.ToString(),                           // 暴击率（0～100）
                    lr.ToString(),                           // 幸运率（0～100）
                    item.instantDps,              // 瞬时 DPS
                    item.maxInstantDps,           // 峰值 DPS
                    item.totalDps,                // 平均 DPS

                    // —— 平均 HPS & 进度 —— 
                    item.totalHps,                // 平均 HPS
                    new CellProgress(item.CellProgress?.Value ?? 0)
                    {
                        Size = new Size(300, 10),
                        Fill = AppConfig.DpsColor
                    }
                ));



            }
            //foreach (var item in snapshot)
            //{
            //    Console.WriteLine("dict nickname = " + item.nickname);
            //}

            HistoricalRecords[timeOnly] = snapshot;
            //Console.WriteLine("dict readback nickname = " + HistoricalRecords[timeOnly].Last().nickname);

            dropdown_History.Items.Add(timeOnly);
            dropdown_History.SelectedValue = -1;
        }




        #region tcp抓包


        /// <summary>
        /// 开始抓包
        /// </summary>
        private void StartCapture()
        {
            // 如果用户没有选择任何网卡，弹出提示并返回
            if (AppConfig.NetworkCard < 0)
            {
                MessageBox.Show("请选择一个网卡设备");

                pageHeader_MainHeader.SubText = "监控已关闭";
                return;
            }


            // 获取所有可用的抓包设备（网卡）
            var devices = CaptureDeviceList.Instance;

            // 检查设备列表是否为空
            if (devices == null || devices.Count == 0)
            {
                throw new InvalidOperationException("没有找到可用的网络抓包设备");
            }

            // 检查索引是否有效
            if (AppConfig.NetworkCard < 0 || AppConfig.NetworkCard >= devices.Count)
            {
                throw new InvalidOperationException($"无效的网络设备索引: {AppConfig.NetworkCard}");
            }

            // 根据用户在下拉框中选择的索引获取对应设备
            selectedDevice = devices[AppConfig.NetworkCard];

            // 检查获取的设备是否为null
            if (selectedDevice == null)
            {
                throw new InvalidOperationException($"无法获取网络设备，索引: {AppConfig.NetworkCard}");
            }

            // 打开设备，设置为混杂模式（能接收所有经过的包），超时设置为 1000 毫秒
            selectedDevice.Open(DeviceModes.Promiscuous, 1000);

            // selectedDevice.OnPacketArrival += Device_OnPacketArrival;
            //selectedDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            // 开始
            //selectedDevice.StartCapture();

            _lastStats = selectedDevice.Statistics!;  // 使用!操作符告诉编译器Statistics不会为null

            //selectedDevice.StartCapture();
            InitStatsTimer();


            // 初始化无数据包检测定时器
            //_restartCaptureTimer = new System.Timers.Timer(1000); // 每秒检查一次
            //_restartCaptureTimer.AutoReset = true;
            //_restartCaptureTimer.Elapsed += RestartCaptureTimer_Elapsed;
            //_restartCaptureTimer.Start();

            // 控制台打印提示信息
            // Console.WriteLine("开始抓包...");

            //switch_IsMonitoring.Checked = true;
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



            TableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
        }


        // 统计计时器间隔 (毫秒)
        private int statsInterval = 1000;
        // 统计计时器
        private System.Timers.Timer? statsTimer;
        // 用于同步统计任务的互斥锁
        private object statsLock = new object();
        // 标记统计任务是否正在处理中
        private bool isProcessing = false;
        // 上次统计时间
        private DateTime lastStatsTime = DateTime.Now;



        // 初始化统计计时器
        private void InitStatsTimer()
        {
            if (statsTimer != null)
            {
                statsTimer.Stop();
                statsTimer.Dispose();
            }
            #region 丢包统计
            //statsTimer = new System.Timers.Timer(statsInterval);
            //statsTimer.AutoReset = true;  // 设置为自动重置，定期触发
            //lastStatsTime = DateTime.Now;

            //statsTimer.Elapsed += (s, e) =>
            //{
            //    var currentTime = DateTime.Now;
            //    var elapsedSinceLast = currentTime - lastStatsTime;
            //    lastStatsTime = currentTime;

            //    // 使用互斥锁确保同一时间只有一个统计任务在执行
            //    if (!Monitor.TryEnter(statsLock, 100))  // 设置100ms超时
            //    {
            //        var timestamp = currentTime.ToString("HH:mm:ss.fff");
            //        Console.WriteLine($"[{timestamp}] [丢包统计] 警告: 上一个统计任务仍在执行({elapsedSinceLast.TotalMilliseconds:F2}ms)，跳过本次统计");
            //        return;
            //    }

            //    try
            //    {
            //        if (isProcessing)
            //        {
            //            var timestamp = currentTime.ToString("HH:mm:ss.fff");
            //            Console.WriteLine($"[{timestamp}] [丢包统计] 警告: 统计任务重入，跳过本次统计");
            //            return;
            //        }

            //        isProcessing = true;
            //        var startTime = currentTime;

            //        // 取当前统计
            //        var cur = selectedDevice.Statistics!;  // 使用!操作符告诉编译器Statistics不会为null

            //        // 计算差值
            //        long recvDelta = cur.ReceivedPackets - _lastStats.ReceivedPackets;
            //        long dropDelta = cur.DroppedPackets - _lastStats.DroppedPackets;
            //        long ifDropDelta = cur.InterfaceDroppedPackets - _lastStats.InterfaceDroppedPackets;

            //        // 应用层丢包率 = dropped / (received + dropped)
            //        double appLossRate = (recvDelta + dropDelta) > 0
            //            ? dropDelta / (double)(recvDelta + dropDelta) * 100
            //            : 0;
            //        // 网卡层丢包率 = ifDrop / (received + dropped + ifDrop)
            //        double driverLossRate = (recvDelta + dropDelta + ifDropDelta) > 0
            //            ? ifDropDelta / (double)(recvDelta + dropDelta + ifDropDelta) * 100
            //            : 0;


            //        // 只在丢包率不为0或有较多数据包接收时才打印
            //        if (appLossRate > 0 || driverLossRate > 0 || recvDelta > 10)  // 增加阈值，减少无意义的打印
            //        {
            //            // 输出丢包率信息，添加时间戳以便观察实时性
            //            var timestamp = currentTime.ToString("HH:mm:ss.fff");
            //            Console.WriteLine($"[{timestamp}] [丢包统计] (间隔: {elapsedSinceLast.TotalMilliseconds:F0}ms) 应用层丢包率: {appLossRate:F2}%, 网卡层丢包率: {driverLossRate:F2}%");
            //            Console.WriteLine($"[{timestamp}] [丢包统计] 接收包数: {recvDelta}, 丢弃包数: {dropDelta}, 网卡丢弃: {ifDropDelta}");
            //        }

            //        // 更新上次统计
            //        _lastStats = cur;

            //        var endTime = DateTime.Now;
            //        var processingTime = endTime - startTime;
            //        if (processingTime.TotalMilliseconds > statsInterval / 2)
            //        {
            //            var timestamp = endTime.ToString("HH:mm:ss.fff");
            //            Console.WriteLine($"[{timestamp}] [丢包统计] 警告: 统计任务执行时间过长 ({processingTime.TotalMilliseconds:F2}ms)");
            //        }
            //    }
            //    finally
            //    {
            //        isProcessing = false;
            //        Monitor.Exit(statsLock);
            //    }
            //};
            //statsTimer.Start();
            #endregion
            // 注册数据包到达时的事件处理函数（回调）

            // selectedDevice.OnPacketArrival += Device_OnPacketArrival;
            selectedDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            // 开始
            selectedDevice.StartCapture();

            // 控制台打印提示信息
            // 控制台打印提示信息
            if (!_isCaptureStarted)
            {
                Console.WriteLine("开始抓包...");
                Console.WriteLine($"已启动 {_workerCount} 个工作线程处理数据包");
                _isCaptureStarted = true;
            }
        }
        private bool _hasAppliedFilter = false;
        private bool _isCaptureStarted = false;

        /// <summary>
        /// 停止抓包
        /// </summary>
        private async void StopCapture()
        {
            if (TableDatas.DpsTable.Count > 0)
            {

                SaveCurrentDpsSnapshot();
            }

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

            // 停止统计定时器
            if (statsTimer != null)
            {
                statsTimer.Stop();
                statsTimer.Dispose();
                statsTimer = null;
            }

            // 停止工作线程
            // 在 async 方法中调用
            if (_cancellationTokenSource != null)
            {
                var cts = _cancellationTokenSource;      // 捕获本地引用，避免竞态
                var tasks = _workerTasks;                // 同上

                cts.Cancel();

                try
                {
                    // 过滤掉 null 和已完成的任务，避免无意义等待和异常
                    var pending = tasks?
                        .Where(t => t != null && !t.IsCompleted)
                        .ToArray() ?? Array.Empty<Task>();

                    if (pending.Length > 0)
                    {
                        var all = Task.WhenAll(pending);             // 汇总任务
                        var finished = await Task.WhenAny(all, Task.Delay(3000)); // 最多等3秒

                        if (finished != all)
                        {
                            // 超时：此处可记日志，但不要阻塞
                            Console.WriteLine("[关闭] 等待任务超时（>3s），后续让任务自行收尾。");
                        }
                        else
                        {
                            // 如果里头有异常，这里会把 AggregateException 展开抛出
                            await all;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常：任务响应取消
                }
                catch (AggregateException aex)
                {
                    foreach (var inner in aex.Flatten().InnerExceptions)
                        Console.WriteLine($"[线程终止异常] {inner.Message}");
                }
                finally
                {
                    cts.Dispose();
                    _cancellationTokenSource = null;
                    _workerTasks = Array.Empty<Task>(); // 避免设 null 引发后续判空失误
                }
            }


            // 清空缓存状态
            _hasAppliedFilter = false;
            #region 清空记录
            //开始监控的时候清空数据

            label_SettingTip.Text = "00:00";
           // switch_IsMonitoring.Checked = false;
            //_hasAppliedFilter = false;//需要测试
            timer_RefreshDpsTable.Enabled = false;
            monitor = false;
            timer_RefreshRunningTime.Stop();
            _combatWatch.Stop();
            #endregion
        }


        // 多线程处理相关成员
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task[] _workerTasks = Array.Empty<Task>();
        private readonly int _workerCount = Math.Max(2, Environment.ProcessorCount / 2);

        // 用于存储数据包数据的包装类
        private class PacketData
        {
            public object Packet { get; set; }

            public PacketData(object packet)
            {
                Packet = packet;
            }
        }

        // 类成员，保存上一时刻的统计数据
        private ICaptureStatistics _lastStats;

        /// <summary>
        /// 网络设备捕获到 TCP 数据包后的处理回调函数
        /// </summary>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                // 提取数据包并包装后添加到队列中，由工作线程处理
                _ = new PacketAnalyzer(selectedDevice, e.GetPacket()).Start();
            }
            catch (OperationCanceledException)
            {
                // 预期的取消操作，不记录异常
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[数据包入队异常] {ex}");
            }


        }


        /// <summary>
        /// 无数据包超时检测定时器事件处理
        /// </summary>
        private async void RestartCaptureTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (PacketAnalyzer.LastPacketTime == DateTime.MinValue)
                return; // 还没有收到过包

            if ((DateTime.Now - PacketAnalyzer.LastPacketTime).TotalSeconds > NO_PACKET_TIMEOUT_SECONDS)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                Console.WriteLine($"[{timestamp}] [场景切换] 检测到场景切换重连数据中...");

                // 执行重启抓包操作
                try
                {
                    // 停止当前抓包
                    StopCapture();
                    // 非阻塞等待一小段时间
                    await Task.Delay(3000);
                    // 重新开始抓包
                    StartCapture();
                    // 重置最后数据包时间
                    PacketAnalyzer.Recaptured();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{timestamp}] [重启抓包失败] {ex.Message}");
                }
            }
        }


        #endregion



        private void timer1_Tick(object sender, EventArgs e)
        {
           // Task.Run(() => RefreshDpsTable());

        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            //注册钩子
            kbHook = new KeyboardHook();
            kbHook.SetHook();
            //绑定事件
            kbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent;

            AppConfig.Reader.Load(AppConfig.ConfigIni);//加载配置文件
            //config.network_card = Convert.ToInt32(config.reader.GetValue("SetUp", "NetworkCard"));
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
                AppConfig.Transparency = 100; // 或者其他表示"未配置"的值




            var colorStr = AppConfig.Reader.GetValue("SetUp", "DpsColor");
            if (!string.IsNullOrWhiteSpace(colorStr))
            {
                // 尝试解析 Color [A=255, R=238, G=67, B=98]
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
                    // 解析失败用默认色
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
        }


        private bool Top = false;
        private void button2_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight;
            //这里使用了Toggle属性切换图标
            button_ThemeSwitch.Toggle = !AppConfig.IsLight;
            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(Common.skillDiary, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(Common.userUidSet, AppConfig.IsLight);//设置窗体颜色


            AppConfig.Reader.Load(AppConfig.ConfigIni);//加载配置文件
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

        private void ShowHistoricalDps(string timeKey)
        {
            if (!HistoricalRecords.TryGetValue(timeKey, out var recordList))
            {
                MessageBox.Show($"未找到时间 {timeKey} 的历史记录");
                return;
            }


            // 深拷贝每一项（防止修改历史数据）
            foreach (var item in recordList)
            {
                // 先把 critRate / luckyRate 的 "%" 去掉再解析成 double
                double.TryParse(item.critRate.TrimEnd('%'), out var cr);
                double.TryParse(item.luckyRate.TrimEnd('%'), out var lr);

                Plugin.TableDatas.DpsTable.Add(new DpsTable(
                    // —— 受伤 & 治疗 —— 
                    item.uid,
                    item.nickname,
                    item.damageTaken,             // 累计受到的伤害
                    item.totalHealingDone,        // 总治疗量
                    item.criticalHealingDone,     // 暴击治疗量
                    item.luckyHealingDone,        // 幸运治疗量
                    item.critLuckyHealingDone,    // 暴击+幸运治疗量
                    item.instantHps,              // 瞬时 HPS
                    item.maxInstantHps,           // 峰值 HPS

                    // —— 职业 & DPS —— 
                    item.profession,              // 职业
                    item.totalDamage,             // 总伤害
                    item.criticalDamage,          // 暴击伤害
                    item.luckyDamage,             // 幸运伤害
                    item.critLuckyDamage,         // 暴击+幸运伤害
                    cr.ToString(),                           // 暴击率（0～100）
                    lr.ToString(),                           // 幸运率（0～100）
                    item.instantDps,              // 瞬时 DPS
                    item.maxInstantDps,           // 峰值 DPS
                    item.totalDps,                // 平均 DPS

                    // —— 平均 HPS & 进度 —— 
                    item.totalHps,                // 平均 HPS
                    new CellProgress(item.CellProgress?.Value ?? 0)
                    {
                        Size = new Size(300, 10),
                        Fill = AppConfig.DpsColor
                    }
                ));
            }
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


        private readonly Stopwatch _combatWatch = new Stopwatch();

        private void timer2_Tick(object sender, EventArgs e)
        {
            label_SettingTip.Text = _combatWatch.Elapsed.ToString(@"mm\:ss");
        }
    
        private void switch1_CheckedChanged(object sender, BoolEventArgs e)
        {
           
            if (monitor == false)
            {

                //开始监控
                StartCapture();

                if (AppConfig.NetworkCard == -1)
                {
                    return;
                }

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

                //关闭监控
                StopCapture();
                label_SettingTip.Text = "00:00";
                //_hasAppliedFilter = false;//需要测试
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
                    new ContextMenuStripItem("基础设置")
                    {
                        IconSvg = Resources.set_up,

                    },

                    new ContextMenuStripItem("数据显示设置")
                    {
                        IconSvg = Resources.data_display,

                    },
                    new ContextMenuStripItem("用户UID设置")
                        {
                            IconSvg = Resources.userUid,
                        },

            };
            AntdUI.ContextMenuStrip.open(this, async it =>
            {
                switch (it.Text)
                {
                    case "基础设置":
                        OpenSettingsDialog();
                        break;
                    case "数据显示设置":
                        dataDisplay();
                        break;
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

        /// <summary>
        /// 打开基础设置面板
        /// </summary>
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
                AppConfig.Reader.Load(AppConfig.ConfigIni);//加载配置文件
                AppConfig.Reader.SaveValue("SetUp", "NetworkCard", AppConfig.NetworkCard.ToString());
                AppConfig.Reader.SaveValue("SetUp", "Transparency", form.inputNumber1.Value.ToString());
                AppConfig.Reader.SaveValue("SetUp", "DpsColor", AppConfig.DpsColor.ToString());
                //键位存储
                AppConfig.Reader.SaveValue("SetKey", "MouseThroughKey", AppConfig.MouseThroughKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "FormTransparencyKey", AppConfig.FormTransparencyKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "WindowToggleKey", AppConfig.WindowToggleKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "ClearDataKey", AppConfig.ClearDataKey.ToString());
                AppConfig.Reader.SaveValue("SetKey", "ClearHistoryKey", AppConfig.ClearHistoryKey.ToString());
                //保存配置文件
                AppConfig.Reader.Save(AppConfig.ConfigIni);
                label_SettingTip.Visible = false;


            }
        }

        private void dataDisplay()
        {
            using (var form = new DataDisplaySettings(this))
            {
                AppConfig.Reader.Load(AppConfig.ConfigIni);//加载配置文件
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

        private void dropdown1_SelectedValueChanged_1(object sender, ObjectNEventArgs e)
        {
            switch (e.Value)
            {
                case "基础设置":

                    break;

            }
        }
    }
}
