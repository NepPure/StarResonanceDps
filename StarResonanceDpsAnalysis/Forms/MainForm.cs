using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

using AntdUI;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Properties;
using ZstdNet;

namespace StarResonanceDpsAnalysis
{
    public partial class MainForm : BorderlessForm
    {
        // 当前选中的网络抓包设备（例如某个网卡）
        private ICaptureDevice? selectedDevice;

        // TCP 分片缓存：Key 是 TCP 序列号，Value 是对应的分片数据
        // 用于重组多段 TCP 数据流（比如一个完整的 protobuf 消息被拆分在多个包里）
        private readonly ConcurrentDictionary<uint, byte[]> tcpCache = new();

        // 期望的下一个 TCP 序列号，用于判断数据是否连续（是否需要缓存/拼接）
        private uint tcpNextSeq = 0;

        // 当前已缓存的数据缓冲区，用于拼接多个 TCP 包的数据
        private byte[] tcpDataBuffer = [];

        // 上一次接收到数据包的时间，用于判断是否超时（如超时则可能清理缓存）
        private DateTime lastPacketTime = DateTime.MinValue;


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
               
                //string nickname = item.nickname;
                // 先把 critRate / luckyRate 的 "%" 去掉再解析成 double
                double.TryParse(item.critRate.TrimEnd('%'), out var cr);
                double.TryParse(item.luckyRate.TrimEnd('%'), out var lr);
                snapshot.Add(new DpsTable(
                    // —— 受伤 & 治疗 —— 
                    item.uid,
                    nickname : item.nickname,
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

            switch_IsMonitoring.Checked = true;
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

            // 初始化工作线程
            _cancellationTokenSource = new CancellationTokenSource();
            _workerTasks = new Task[_workerCount];
            for (int i = 0; i < _workerCount; i++)
            {
                _workerTasks[i] = Task.Run(() => WorkerLoop(_cancellationTokenSource.Token));
            }

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

        private void ApplyDynamicFilter(string srcServer)
        {
            if (selectedDevice == null)
            {
                Console.WriteLine("[错误] selectedDevice为null，无法应用过滤器");
                return;
            }

            // 拆出服务器那半
            var serverPart = srcServer.Split("->")[0].Trim();       // "36.152.0.122:16125"
            var parts = serverPart.Split(':');
            var serverIp = parts[0];
            var serverPort = parts[1];

            selectedDevice.Filter = $"tcp and host {serverIp} and port {serverPort}";
            // Console.WriteLine($"【Filter 已更新】 tcp and host {serverIp} and port {serverPort}");
        }
        private bool _hasAppliedFilter = false;
        private bool _isCaptureStarted = false;

        /// <summary>
        /// 网络设备捕获到 TCP 数据包后的处理回调函数
        /// </summary>
        /// <summary>
        /// 工作线程循环，从队列中取出数据包并处理
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        private void WorkerLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // 从队列中获取数据包，如果队列为空则阻塞
                        PacketData packetData = _packetQueue.Take(cancellationToken);
                       
                        ProcessPacket(packetData.Packet);
                    }
                    catch (OperationCanceledException)
                    {
                        // 预期的取消操作，不记录异常
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[工作线程异常] {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[工作线程致命异常] {ex}");
            }
        }
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
            currentServer = "";
            _hasAppliedFilter = false;
            ClearTcpCache();
            #region 清空记录
            //开始监控的时候清空数据
          
            label_SettingTip.Text = "00:00";
            switch_IsMonitoring.Checked = false;
            //_hasAppliedFilter = false;//需要测试
            timer_RefreshDpsTable.Enabled = false;
            monitor = false;
            timer_RefreshRunningTime.Stop();
            _combatWatch.Stop();
            #endregion
            // 清空数据包队列
            while (_packetQueue.TryTake(out _)) ;

        }

        private readonly ConcurrentDictionary<uint, DateTime> tcpCacheTime = new();
        private readonly MemoryStream tcpStream = new();

        // 多线程处理相关成员
        private BlockingCollection<PacketData> _packetQueue = new BlockingCollection<PacketData>(new ConcurrentQueue<PacketData>());
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
                var packet = e.GetPacket();
                _packetQueue.Add(new PacketData(packet), _cancellationTokenSource.Token);
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
            if (lastPacketTime == DateTime.MinValue)
                return; // 还没有收到过包

            if ((DateTime.Now - lastPacketTime).TotalSeconds > NO_PACKET_TIMEOUT_SECONDS)
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
                    lastPacketTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{timestamp}] [重启抓包失败] {ex.Message}");
                }
            }
        }



        /// <summary>
        /// 处理单个数据包
        /// </summary>
        /// <param name="packetObj">数据包对象</param>
        private void ProcessPacket(object packetObj)
        {
            try
            {

                // 获取原始数据包
                dynamic rawPacket = packetObj;

                // 使用 PacketDotNet 解析为通用数据包对象（包含以太网/IP/TCP 等）
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
              
                // 提取 TCP 数据包（如果不是 TCP，会返回 null）
                var tcpPacket = packet.Extract<TcpPacket>();

                // 提取 IPv4 数据包（如果不是 IPv4，也会返回 null）
                var ipPacket = packet.Extract<IPv4Packet>();

                // 如果不是 TCP 或 IP 包，忽略
                if (tcpPacket == null || ipPacket == null) return;

                // 获取 TCP 负载（即应用层数据）
                var payload = tcpPacket.PayloadData;
                if (payload == null || payload.Length == 0) return;


                // 构造当前包的源 -> 目的 IP 和端口的字符串，作为唯一标识
                var srcServer = $"{ipPacket.SourceAddress}:{tcpPacket.SourcePort} -> {ipPacket.DestinationAddress}:{tcpPacket.DestinationPort}";
                // 更新数据包来源
                if (!_hasAppliedFilter && VerifyServer(srcServer, payload))
                {
                    ApplyDynamicFilter(srcServer);
                    _hasAppliedFilter = true;
                }


                // 如果距离上次收到包的时间超过 30 秒，认为可能断线，清除缓存
                if (lastPacketTime != DateTime.MinValue && (DateTime.Now - lastPacketTime).TotalSeconds > 10)
                {
                    Console.WriteLine("长时间未收到包，可能已断线");
                    currentServer = "";
                    ClearTcpCache(); // 清除 TCP 缓存数据
                }

                // 如果当前服务器（源地址）发生变化，进行验证
                if (currentServer != srcServer)
                {

                    // 尝试验证是否为目标服务器（例如游戏服务器）
                    if (!VerifyServer(srcServer, payload))
                        return; // 如果验证失败，丢弃该包
                }

                // 通过序列号和数据进行 TCP 数据流重组（例如多个 TCP 包拼接成完整的 protobuf）
                ReassembleTcp(tcpPacket, payload);
            }
            catch (Exception ex)
            {
                // 捕获异常，避免程序崩溃，同时打印异常信息
                Console.WriteLine("抓包异常: " + ex.Message);
            }
        }

        /// <summary>
        /// 清除 TCP 缓存，用于断线、错误重组等情况的重置操作
        /// </summary>
        private void ClearTcpCache()
        {
            // 清空当前拼接的数据缓冲区（通常是多段 TCP 包拼接中间状态）
            tcpDataBuffer = new byte[0];

            // 重置下一个期望的 TCP 序列号为 0（重新开始拼接）
            tcpNextSeq = 0;
            _hasAppliedFilter = false;
            // 清空缓存的 TCP 分片数据（已收到但未处理的包）
            tcpCache.Clear();
        }



        private void ReassembleTcp(TcpPacket tcpPacket, byte[] payload)
        {
            uint seq = (uint)tcpPacket.SequenceNumber;

            lock (tcpCache)
            {
                // 尝试初始化 tcpNextSeq（只有第一次设置）
                if (tcpNextSeq == 0 && payload.Length > 4)
                {
                    int len = (payload[0] << 24) | (payload[1] << 16) | (payload[2] << 8) | payload[3];
                    if (len < 999999) // 判断为合理包头
                    {
                        tcpNextSeq = seq;
                    }
                }

                // 缓存当前段
                tcpCache[seq] = payload;
                tcpCacheTime[seq] = DateTime.Now;

                // 清理超时缓存（超过10秒）
                var expired = tcpCacheTime
                    .Where(kv => (DateTime.Now - kv.Value).TotalSeconds > 10)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var key in expired)
                {
                    tcpCache.Remove(key, out var v1);
                    tcpCacheTime.Remove(key, out var v2);


                }

                // —— 关键：处理拼接 —— 
                int skipTimeoutMs = 200;

                // 找到当前可用的最小 Seq
                var sortedSeqs = tcpCache.Keys.OrderBy(k => k).ToList();

                while (true)
                {
                    if (tcpCache.TryGetValue(tcpNextSeq, out var chunk))
                    {
                        // 找到了当前需要的段，正常拼接
                        tcpStream.Seek(0, SeekOrigin.End);
                        tcpStream.Write(chunk, 0, chunk.Length);

                        tcpCache.Remove(tcpNextSeq, out var v1);
                        tcpCacheTime.Remove(tcpNextSeq, out var v2);
                        tcpNextSeq += (uint)chunk.Length;
                        lastPacketTime = DateTime.Now;
                    }
                    else
                    {
                        // 当前段还没到，判断是否跳过等待
                        var delayed = sortedSeqs
                            .Where(k => k > tcpNextSeq && tcpCacheTime.ContainsKey(k))
                            .FirstOrDefault(k => (DateTime.Now - tcpCacheTime[k]).TotalMilliseconds > skipTimeoutMs);

                        if (delayed != 0)
                        {
                            // 跳过迟迟不到的 tcpNextSeq，跳到新的 delayed
                            tcpNextSeq = delayed;
                            continue;
                        }
                        break;
                    }
                }

                TryParseTcpStream();
            }
        }

        private void TryParseTcpStream()
        {
            tcpStream.Seek(0, SeekOrigin.Begin);

            while (tcpStream.Length - tcpStream.Position >= 4)
            {
                byte[] lenBytes = new byte[4];
                tcpStream.Read(lenBytes, 0, 4);
                int len = (lenBytes[0] << 24) | (lenBytes[1] << 16) | (lenBytes[2] << 8) | lenBytes[3];

                if (len > 999999)
                {
                    // 非法长度，清空整个缓冲区
                    tcpStream.SetLength(0);
                    return;
                }

                long remain = tcpStream.Length - tcpStream.Position;
                if (remain + 4 >= len)
                {
                    // 数据够一个完整包
                    byte[] packet = new byte[len];
                    Array.Copy(lenBytes, 0, packet, 0, 4);
                    tcpStream.Read(packet, 4, len - 4);

                    临时字段测试.process(packet);//测试
                    // 异步处理，防止 UI 卡顿
                    ProcessPacket(packet);
                   
                }
                else
                {
                    // 不够，回退4字节
                    tcpStream.Position -= 4;
                    break;
                }
            }

            // 剩余未读部分保留到新缓冲区
            long unread = tcpStream.Length - tcpStream.Position;
            if (unread > 0)
            {
                byte[] remain = new byte[unread];
                tcpStream.Read(remain, 0, (int)unread);
                tcpStream.SetLength(0);
                tcpStream.Write(remain, 0, remain.Length);
            }
            else
            {
                tcpStream.SetLength(0);
            }
        }
        #endregion


        private string currentServer = "";
        private ulong userUid = 0;


        /// <summary>
        /// 尝试识别当前 TCP 包是否来自目标服务器，并提取玩家 UID（只在首次识别时调用）
        /// </summary>
        /// <param name="srcServer">格式为“IP:端口 -> IP:端口”的源描述</param>
        /// <param name="buf">TCP 包的 Payload 内容</param>
        /// <returns>如果识别成功，返回 true；否则返回 false</returns>
        private bool VerifyServer(string srcServer, byte[] buf)
        {
            try
            {
                // 如果当前服务器已经是这个，不需要重复识别
                if (currentServer == srcServer)
                {

                    return false;
                }


                // 第 5 字节不为 0，则不是“小包”格式（协议约定）

                if (buf[4] != 0)
                    return false;

                // 提取去掉前10字节后的内容（通常前面是包头或控制信息）
                var data = buf.AsSpan(10);
                if (data.Length == 0)
                    return false;

                // 将数据包装为流，逐个解析其中的 protobuf 消息结构
                using var ms = new MemoryStream(data.ToArray());
                while (ms.Position < ms.Length)
                {
                    // 读取消息长度（4字节大端序）
                    byte[] lenBuf = new byte[4];
                    if (ms.Read(lenBuf, 0, 4) != 4)
                        break;

                    int len = ProtoFieldHelper.ReadInt32BigEndian(lenBuf);
                    if (len < 4 || len > 1024 * 1024) // 防止异常长度（小于最小值或超大）
                        break;

                    // 读取真正的数据体（减去4字节长度头）
                    byte[] data1 = new byte[len - 4];
                    if (ms.Read(data1, 0, data1.Length) != data1.Length)
                        break;

                    // 签名校验，确认是目标游戏协议的数据包
                    byte[] signature = new byte[] { 0x00, 0x63, 0x33, 0x53, 0x42, 0x00 };
                    //Console.WriteLine(!data1.Skip(5).Take(signature.Length).SequenceEqual(signature));
                    if (!data1.Skip(5).Take(signature.Length).SequenceEqual(signature))
                        break;

                    // 解码主体结构（跳过前18字节，通常是头部结构）
                    var body = ProtoDynamic.Decode(data1.AsSpan(18).ToArray());

                    // 从结构中尝试获取玩家 UID（字段[1]->[5]）
                    if (data1.Length >= 17 && data1[17] == 0x2E) // 特定标志字节 0x2E
                    {
                        var b1 = ProtoFieldHelper.TryGetDecoded(body, 1); // 获取字段 1 的子结构
                        if (b1 != null && ProtoFieldHelper.TryGetU64(b1, 5) is ulong rawUid && rawUid != 0)
                        {
                            userUid = rawUid >> 16; // 截断低16位，仅保留玩家 UID 主体
                            Console.WriteLine($"识别玩家UID: {userUid}");
                        }
                    }

                    // 成功识别服务器，设置当前 server
                    currentServer = srcServer;

                    // 清除之前可能未完成的 TCP 拼包缓存
                    ClearTcpCache();

                    Console.WriteLine($"识别到场景服务器地址: {srcServer}");

                    return true; // ✅ 成功识别，立即返回
                }
            }
            catch (Exception ex)
            {
                // 捕获所有异常，避免崩溃
                //Console.WriteLine($"[VerifyServer 异常] {ex}");
            }

            return false; // 无法识别为目标服务器
        }




        /// <summary>
        /// 处理完整的 TCP 数据包，包括解压缩、解包、protobuf 解析、技能伤害提取与统计
        /// </summary>
        /// <param name="buf">一个完整的 TCP 数据包，前4字节为长度</param>
        private void ProcessPacket(byte[] buf)
        {
            try
            {
               
                // 如果包长度小于 32 字节，认为是无效或空包
                if (buf.Length < 32)
                    return;

                // Step 1: 检查是否需要 ZSTD 解压缩
                // 如果第5字节（buf[4]）最高位为 1，则需要解压缩
                if ((buf[4] & 0x80) != 0)
                {
                    try
                    {
                        // 从第10字节开始是压缩数据，前10字节为协议头
                        using var inputStream = new MemoryStream(buf, 10, buf.Length - 10);
                        using var decompressionStream = new DecompressionStream(inputStream);
                        using var outputStream = new MemoryStream();

                        // 解压到 outputStream
                        decompressionStream.CopyTo(outputStream);

                        var decompressed = outputStream.ToArray();

                        // 把原始前10字节头部和解压后的数据拼接成一个新的完整包
                        buf = buf.Take(10).Concat(decompressed).ToArray();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[zstd 解压失败] {ex.Message}，跳过该包");
                        return;
                    }
                }

                // buf 是解压完成的数据


                // Step 2: 开始拆包（跳过前10字节协议头）
                using var ms = new MemoryStream(buf, 10, buf.Length - 10);
                #region
                while (ms.Position < ms.Length)
                {
                    // 每个内部数据块前4字节是长度（大端）
                    byte[] lenBuf = new byte[4];
                    if (ms.Read(lenBuf, 0, 4) != 4) break;

                    int len = ProtoFieldHelper.ReadInt32BigEndian(lenBuf);

                    // 如果长度不合法，直接跳过当前包
                    if (len < 4 || len > 4 * 1024 * 1024 || len - 4 > ms.Length - ms.Position)
                    {
                        Console.WriteLine($"[跳过] 非法包长度 len = {len}，流剩余 = {ms.Length - ms.Position}");
                        break;
                    }

                    // 读取实际内容（减去4字节长度）
                    byte[] data1 = new byte[len - 4];
                    if (ms.Read(data1, 0, data1.Length) != data1.Length) break;

                    if (data1.Length < 18) continue;

                    // 解码：从第18字节开始是 protobuf 数据结构
                    var body = ProtoDynamic.Decode(data1.AsSpan(18).ToArray());

                    // 如果 data1[17] == 0x2E（.），意味着 body[1] 是真正的结构
                    if (data1.Length >= 17 && data1[17] == 0x2E)
                    {
                        var temp = ProtoFieldHelper.TryGetDecoded(body, 1);
                        if (temp != null)
                            body = temp;

                        // 尝试读取玩家 UID（字段 5）
                        if (ProtoFieldHelper.TryGetU64(body, 5) is ulong rawUid && rawUid != 0)
                        {
                            var uid = rawUid >> 16;
                            if (userUid != uid)
                            {
                                userUid = uid;
                                Console.WriteLine("识别玩家UID: " + userUid);
                            }
                        }
                    }

                    // 提取 body[1] 作为数据列表（伤害记录容器）
                    var body1Raw = ProtoFieldHelper.TryGetRaw(body, 1);
                    if (body1Raw == null) return;

                    List<object> body1List = new();

                    // 如果是单个对象（非列表）
                    if (body1Raw is ProtoValue pv && pv.Decoded != null)
                    {
                        body1List.Add(pv);
                    }
                    // 如果是多个结构（列表）
                    else if (body1Raw is List<object> list)
                    {
                        foreach (var item in list)
                        {
                            if (item is ProtoValue p && p.Decoded != null)
                                body1List.Add(p);
                        }
                    }
                    #region 
                    // 遍历每个伤害记录结构体
                    foreach (var bRaw in body1List)
                    {
                        if (bRaw is not ProtoValue bVal || bVal.Decoded == null) continue;

                        var b = bVal.Decoded;

                        // 查找字段 7，结构体中包含伤害信息
                        if (!b.TryGetValue(7, out var n7) || n7 is not ProtoValue pv7 || pv7.Decoded == null)
                            continue;

                        var d7 = pv7.Decoded;

                        // 字段 2：包含 hit（伤害）数组或单个结构
                        if (!d7.TryGetValue(2, out var hitsRaw)) continue;

                        List<Dictionary<int, object>> hits = new();

                        if (hitsRaw is ProtoValue pvHit && pvHit.Decoded != null)
                        {
                            hits.Add(pvHit.Decoded);
                        }
                        else if (hitsRaw is List<object> hitList)
                        {
                            foreach (var item in hitList)
                            {
                                if (item is ProtoValue p && p.Decoded != null)
                                    hits.Add(p.Decoded);
                            }
                        }

                        //Console.WriteLine("========== Hits ==========");

                        //for (int i = 0; i < hits.Count; i++)
                        //{
                        //    Console.WriteLine($"--- Hit #{i} ---");
                        //    var hit = hits[i];
                        //    foreach (var kv in hit)
                        //    {
                        //        Console.WriteLine($"字段 {kv.Key}: {ProtoDynamic.FormatProtoValue(kv.Value)}");
                        //    }
                        //}


                        foreach (var hit in hits)
                        {
                            var skill = ProtoFieldHelper.TryGetU64(hit, 12);
                            if (skill == 0) continue;

                            var value = ProtoFieldHelper.TryGetU64(hit, 6);          // 普通伤害
                            var luckyValue = ProtoFieldHelper.TryGetU64(hit, 8);     // 幸运伤害
                            var isMiss = ProtoFieldHelper.TryGetBool(hit, 2);        // 是否Miss
                            var isCrit = ProtoFieldHelper.TryGetBool(hit, 5);        // 是否暴击
                            bool isLucky = luckyValue != 0;

                            bool isHeal = ProtoFieldHelper.TryGetU64(hit, 4) == 2;
                            var isDead = ProtoFieldHelper.TryGetBool(hit, 17);// 是否造成死亡
                            var hpLessen = ProtoFieldHelper.TryGetU64(hit, 9);       // 实际扣血

                            // 施法者 UID：字段 21 优先，其次 11
                            var operatorRaw = ProtoFieldHelper.TryGetU64(hit, 21); //操作者 ID

                            ulong targetRaw = ProtoFieldHelper.TryGetU64(b, 1);//目标UID


                            if (operatorRaw == 0) operatorRaw = ProtoFieldHelper.TryGetU64(hit, 11);


                            bool operator_is_player = (operatorRaw & 0xFFFFUL) == 640UL;

                            bool target_is_player = (targetRaw & 0xFFFFUL) == 640UL;


                            var operatorUid = operatorRaw >> 16;

                            ulong targetUid = targetRaw >> 16;



                            // 仅统计玩家（尾部不是 640 说明是怪物或NPC）
                            if (operatorUid == 0) continue;


                            // 伤害数值：优先普通，如果没有用幸运值（注意0值可能表示治疗）
                            var damage = value != 0 ? value : luckyValue;

                            string extra = isCrit ? "暴击" : luckyValue != 0 ? "幸运" : isMiss ? "Miss" : "普通";

                            //Console.WriteLine($"玩家 {operatorUid} 使用技能 {skill} 造成伤害 {damage} 扣血 {hpLessen} 标记 {extra}");
                            if (Common.skillDiary != null && !Common.skillDiary.IsDisposed)
                            {
                                Task.Run(() =>
                                {
                                    Common.skillDiary.AppendDiaryLine($"玩家 {operatorUid} 使用技能 {skill} 造成伤害 {damage} 扣血 {hpLessen} 标记 {extra}");

                                });


                            }
                            
                            //根据目标类型，处理数据
                            if (target_is_player)
                            {
                                if (isHeal)
                                {
                                    if (operator_is_player)
                                    {
                                        var healer = StatisticData._manager.GetOrCreate(operatorUid);

                                        //记录玩家造成的治疗
                                        healer.AddHealing(damage, isCrit, isLucky);
                                    }


                                }
                                else
                                {
                                    // 拿到“受伤者” 对象
                                    var victim = StatisticData._manager.GetOrCreate(targetUid);

                                    victim.AddTakenDamage(skill, damage);
                                    //记录玩家受到伤害
                                }


                            }
                            else
                            {
                                if (!isHeal && operator_is_player)
                                {
                                    var healer = StatisticData._manager.GetOrCreate(operatorUid);

                                    //记录输出
                                    healer.AddDamage(skill, damage, isCrit, isLucky, hpLessen);

                                }
                            }

                            if (operator_is_player)
                            {
                                string roleName = null;

                                switch (skill)
                                {
                                    case 1241:
                                        roleName = "射线";
                                        break;
                                    case 55302:
                                        roleName = "协奏";
                                        break;
                                    case 20301:
                                        roleName = "愈合";
                                        break;
                                    case 1518:
                                        roleName = "惩戒";
                                        break;
                                    case 2306:
                                        roleName = "狂音";
                                        break;
                                    case 120902:
                                        roleName = "冰矛";
                                        break;
                                    case 1714:
                                        roleName = "居合";
                                        break;
                                    case 44701:
                                        roleName = "月刃";
                                        break;
                                    case 220112:
                                    case 2203622:
                                        roleName = "鹰弓";
                                        break;
                                    case 1700827:
                                        roleName = "狼弓";
                                        break;
                                    case 1419:
                                        roleName = "空枪";
                                        break;
                                    case 1418:
                                        roleName = "重装";
                                        break;
                                    case 2405:
                                        roleName = "防盾";
                                        break;
                                    case 2406:
                                        roleName = "光盾";
                                        break;
                                    case 199902:
                                        roleName = "岩盾";
                                        break;
                                    default:
                                        // 未匹配到任何职业，不做处理
                                        roleName = Common.GetProfessionBySkill(skill);
                                        break;
                                }


                                if (!string.IsNullOrEmpty(roleName))
                                {
                                    StatisticData._manager.SetProfession(operatorUid, roleName);
                                }
                            }



                        }
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析异常: {ex}");
            }
        }







        private void timer1_Tick(object sender, EventArgs e)
        {
            Task.Run(() => RefreshDpsTable());

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
            switch(e.Value)
            {
                case "基础设置":

                    break;

            }
        }
    }
}
