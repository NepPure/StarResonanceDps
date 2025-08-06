

using AntdUI;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using ZstdNet;
using 星痕共鸣DPS统计.Control;
using 星痕共鸣DPS统计.Plugin;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static 星痕共鸣DPS统计.Plugin.Common;
using SharpPcap.LibPcap;


namespace 星痕共鸣DPS统计
{

    public partial class Form1 : BorderlessForm
    {
        // 当前选中的网络抓包设备（例如某个网卡）
        private ICaptureDevice selectedDevice;
      

        // TCP 分片缓存：Key 是 TCP 序列号，Value 是对应的分片数据
        // 用于重组多段 TCP 数据流（比如一个完整的 protobuf 消息被拆分在多个包里）
        private Dictionary<uint, byte[]> tcpCache = new Dictionary<uint, byte[]>();

        // 期望的下一个 TCP 序列号，用于判断数据是否连续（是否需要缓存/拼接）
        private uint tcpNextSeq = 0;

        // 当前已缓存的数据缓冲区，用于拼接多个 TCP 包的数据
        private byte[] tcpDataBuffer = new byte[0];

        // 上一次接收到数据包的时间，用于判断是否超时（如超时则可能清理缓存）
        private DateTime lastPacketTime = DateTime.MinValue;
        KeyboardHook gg;


        public Form1()
        {
            InitializeComponent();
            FormGui.GUI(this);
            tabel_load();


        }


        private void tabel_load()
        {
            table1.Columns.Clear();
            table1.Columns = new ColumnCollection
            {
                new Column("","序号"){
                    Width = "50",
                    Render = (value,record,rowindex)=>{return (rowindex+1); },
                    Fixed = true,//冻结列
                },
                new AntdUI.Column("uid", "角色ID",ColumnAlign.Center){ SortOrder=true},
                new AntdUI.Column("profession", "职业",ColumnAlign.Center),
                new AntdUI.Column("totalDamage", "总伤害/总治疗",ColumnAlign.Center){ SortOrder=true},


            };


            if (!checkbox1.Checked)
            {
                var extraColumns = new[]
                {
                    new AntdUI.Column("criticalDamage", "纯暴击"){ SortOrder=true},
                    new AntdUI.Column("luckyDamage", "纯幸运"){ SortOrder=true},
                    new AntdUI.Column("critLuckyDamage", "暴击幸运"){ SortOrder=true},
                    new AntdUI.Column("critRate", "暴击率"){ SortOrder=true},
                    new AntdUI.Column("luckyRate", "幸运率"){ SortOrder=true},
                    new AntdUI.Column("instantDps", "瞬时DPS"){ SortOrder=true},
                    new AntdUI.Column("maxInstantDps", "最大瞬时"){ SortOrder=true}
                };
                // 添加到现有 Columns 中
                foreach (var col in extraColumns)
                {
                    if (!table1.Columns.Any(c => c.Key == col.Key))
                        table1.Columns.Add(col);
                }

            }
            else
            {

                table1.Columns.Add(new Column("CellProgress", "团队总伤害占比", ColumnAlign.Center));
            }
            table1.Columns.Add(new Column("totalDps", "总DPS/治疗", ColumnAlign.Center) { SortOrder = true });
            table1.Binding(tabel.dps_tabel);
        }
        private void RefreshDpsTabel()
        {
            // 先解绑（或冻结 UI）

            var statsList = playerStats.Values.ToList(); // 拷贝一份，避免并发修改

            // 计算最大 DPS
            float totalDps = statsList.Sum(s => (float)s.TotalDamage);
            if (totalDps <= 0f) totalDps = 1f; // 避免除以 0

            foreach (var stat in statsList)
            {
                float percent = (float)stat.TotalDamage / totalDps;  // 0 ~ 1，所有人的加起来是1


                var item = tabel.dps_tabel
                                  .FirstOrDefault(x => x != null && x.uid == stat.Uid); if (item == null)
                {
                    tabel.dps_tabel.Add(new DpsTabel(
                        stat.Uid,
                        stat.Profession,
                        stat.TotalDamage,
                        stat.CriticalDamage,
                        stat.LuckyDamage,
                        stat.CritLuckyDamage,
                        Math.Round(stat.CritRate, 1),
                        Math.Round(stat.LuckyRate, 1),
                        stat.InstantDPS,
                        stat.MaxInstantDPS,
                        Math.Round(stat.TotalDPS, 1),
                        new CellProgress(percent) { Size = new Size(300, 10), Fill = config.dpscolor }
                    ));
                }
                else
                {
                    item.totalDamage = stat.TotalDamage;
                    item.profession = stat.Profession;
                    item.criticalDamage = stat.CriticalDamage;
                    item.luckyDamage = stat.LuckyDamage;
                    item.critLuckyDamage = stat.CritLuckyDamage;
                    item.critRate = Math.Round(stat.CritRate, 1).ToString();
                    item.luckyRate = Math.Round(stat.LuckyRate, 1).ToString();
                    item.instantDps = stat.InstantDPS;
                    item.maxInstantDps = stat.MaxInstantDPS;
                    item.totalDps = Math.Round(stat.TotalDPS, 1);
                    if (item.CellProgress is not CellProgress cp)
                    {
                        cp = new CellProgress(percent) { Size = new Size(300, 10), Fill = config.dpscolor };
                        item.CellProgress = cp;
                    }
                    else
                    {
                        cp.Value = percent; // ✅ 仅更新数值，不重新创建
                    }


                }
            }

        }


        private Dictionary<ulong, PlayerStat> playerStats = new();







        bool penetrate = false;//鼠标穿透
        bool monitor = false;//监控开关
        bool hyaline = false;//是否开启透明

        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;


        async void kh_OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            //F6 鼠标穿透
            if (e.KeyData == Keys.F6)
            {
                var exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);

                if (penetrate)
                {
                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);


                    penetrate = false;
                }
                else
                {

                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
                    penetrate = true;
                }

            }
            //F7 窗体透明
            if (e.KeyData == Keys.F7)
            {
                if (hyaline)
                {
                    this.Opacity = 100 / 100;

                    hyaline = false;
                }
                else
                {

                    this.Opacity = config.transparency / 100;

                    hyaline = true;
                }


            }
            //F8 开启/关闭 监控
            if (e.KeyData == Keys.F8)
            {
                if (monitor == false)
                {

                    //开始监控
                    StartCapture();

                    if (config.network_card == -1)
                    {
                        return;
                    }
                    timer1.Enabled = true;
                    pageHeader1.SubText = "监控已开启";
                    monitor = true;


                    //开始监控的时候清空数据
                    if (tabel.dps_tabel.Count > 0)
                    {

                        SaveCurrentDpsSnapshot();
                    }

                    tabel.dps_tabel.Clear();
                    playerStats.Clear();

                }
                else
                {
                    pageHeader1.SubText = "监控已关闭";
              
                    //关闭监控
                    StopCapture();
                    timer1.Enabled = false;
                    monitor = false;

    


                }
            }
            //F9 清空数据
            if (e.KeyData == Keys.F9)
            {
                if (tabel.dps_tabel.Count >= 0)
                {

                    SaveCurrentDpsSnapshot();
                }
                tabel.dps_tabel.Clear();
                playerStats.Clear();

            }//F10 清空历史记录
            if(e.KeyData==Keys.F10)
            {
                dropdown1.Items.Clear();
                HistoricalRecords.Clear();
            
            }
        }


        Dictionary<string, AntdUI.AntList<DpsTabel>> HistoricalRecords = new Dictionary<string, AntdUI.AntList<DpsTabel>>();

        private void SaveCurrentDpsSnapshot()
        {
            if (tabel.dps_tabel.Count == 0)
                return;

            string timeOnly = @$"结束时间：{DateTime.Now:HH:mm:ss}";

            var snapshot = new AntdUI.AntList<DpsTabel>();
            foreach (var item in tabel.dps_tabel)
            {
                snapshot.Add(new DpsTabel(
                    item.uid,
                    item.profession,
                    item.totalDamage,
                    item.criticalDamage,
                    item.luckyDamage,
                    item.critLuckyDamage,
                    double.TryParse(item.critRate.ToString(), out var cr) ? cr : 0,
                    double.TryParse(item.luckyRate.ToString(), out var lr) ? lr : 0,
                    item.instantDps,
                    item.maxInstantDps,
                    item.totalDps,
                    new CellProgress(item.CellProgress?.Value ?? 0)
                    {
                        Size = new Size(300, 10),
                        Fill = config.dpscolor
                    }
                ));
            }

            HistoricalRecords[timeOnly] = snapshot;
            dropdown1.Items.Add(timeOnly);
            dropdown1.SelectedValue = -1;
        }




        #region tcp抓包

        /// <summary>
        /// 开始抓包
        /// </summary>
        private void StartCapture()
        {
            // 如果用户没有选择任何网卡，弹出提示并返回
            if (config.network_card < 0)
            {
                MessageBox.Show("请选择一个网卡设备");

                pageHeader1.SubText = "监控已关闭";
                return;
            }

       
            // 获取所有可用的抓包设备（网卡）
            var devices = CaptureDeviceList.Instance;

            // 根据用户在下拉框中选择的索引获取对应设备
            selectedDevice = devices[config.network_card];

            // 打开设备，设置为混杂模式（能接收所有经过的包），超时设置为 1000 毫秒
            selectedDevice.Open(DeviceModes.Promiscuous, 1000);
            // selectedDevice.Open(DeviceModes.Promiscuous, 8 * 1024 * 1024);

            // 设置过滤器，只抓取 IP 层和 TCP 协议的数据包（避免抓取无关的 UDP、ARP、ICMP 等）
           // selectedDevice.Filter = "tcp and (net 36.152.0.0/24) and (port 16125 or port 16126)";


            // 注册数据包到达时的事件处理函数（回调）

            // selectedDevice.OnPacketArrival += Device_OnPacketArrival;
            selectedDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            // 开始
            selectedDevice.StartCapture();
          
            // 控制台打印提示信息
            Console.WriteLine("开始抓包...");
        }

        private void ApplyDynamicFilter(string srcServer)
        {
            // 拆出服务器那半
            var serverPart = srcServer.Split("->")[0].Trim();       // "36.152.0.122:16125"
            var parts = serverPart.Split(':');
            var serverIp = parts[0];
            var serverPort = parts[1];

            selectedDevice.Filter = $"tcp and host {serverIp} and port {serverPort}";
           // Console.WriteLine($"【Filter 已更新】 tcp and host {serverIp} and port {serverPort}");
        }
        private bool _hasAppliedFilter = false;

        /// <summary>
        /// 停止抓包
        /// </summary>
        private void StopCapture()
        {
            if (selectedDevice != null)
            {
                try
                {
                    selectedDevice.StopCapture();
                    selectedDevice.OnPacketArrival -= Device_OnPacketArrival;
                    currentServer = "";
                    // 4. 重置动态过滤标志（下次再 StartCapture 时，会重新抓全流量以便重新 VerifyServer）
                    _hasAppliedFilter = false;
                    ClearTcpCache();
                    selectedDevice.Close();
                  
                    Console.WriteLine("停止抓包");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"停止抓包异常: {ex.Message}");
                }
            }
        }

    
        private readonly Dictionary<uint, DateTime> tcpCacheTime = new();
        private readonly MemoryStream tcpStream = new();


        /// <summary>
        /// 网络设备捕获到 TCP 数据包后的处理回调函数
        /// </summary>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {

                // 获取原始数据包
                var rawPacket = e.GetPacket();

                // 使用 PacketDotNet 解析为通用数据包对象（包含以太网/IP/TCP 等）
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data.ToArray());

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
                // ... 前面做 VerifyServer 的逻辑 ...
                if (!_hasAppliedFilter && VerifyServer(srcServer, payload))
                {
                    ApplyDynamicFilter(srcServer);
                    _hasAppliedFilter = true;
                }


                // 如果距离上次收到包的时间超过 30 秒，认为可能断线，清除缓存
                if (lastPacketTime != DateTime.MinValue && (DateTime.Now - lastPacketTime).TotalSeconds > 30)
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
                    tcpCache.Remove(key);
                    tcpCacheTime.Remove(key);
                }

                // 按顺序拼接数据
                while (tcpCache.TryGetValue(tcpNextSeq, out var chunk))
                {
                    tcpStream.Seek(0, SeekOrigin.End);
                    tcpStream.Write(chunk, 0, chunk.Length);
                    tcpNextSeq += (uint)chunk.Length;

                    tcpCache.Remove(tcpNextSeq - (uint)chunk.Length);
                    tcpCacheTime.Remove(tcpNextSeq - (uint)chunk.Length);

                    lastPacketTime = DateTime.Now;
                }

                // 解析完整包
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
                            var hpLessen = ProtoFieldHelper.TryGetU64(hit, 9);       // 实际扣血

                            // 施法者 UID：字段 21 优先，其次 11
                            var operatorRaw = ProtoFieldHelper.TryGetU64(hit, 21);
                            if (operatorRaw == 0) operatorRaw = ProtoFieldHelper.TryGetU64(hit, 11);
                            var operatorUid = operatorRaw >> 16;
                            var operatorTail = operatorRaw & 0xFFFF;

                            // 仅统计玩家（尾部不是 640 说明是怪物或NPC）
                            if (operatorUid == 0 || operatorTail != 640) continue;

                            // 伤害数值：优先普通，如果没有用幸运值（注意0值可能表示治疗）
                            var damage = value != 0 ? value : luckyValue;

                            string extra = isCrit ? "暴击" : luckyValue != 0 ? "幸运" : isMiss ? "Miss" : "普通";

                            //Console.WriteLine($"玩家 {operatorUid} 使用技能 {skill} 造成伤害 {damage} 扣血 {hpLessen} 标记 {extra}");


                            // 加入统计
                            if (!playerStats.TryGetValue(operatorUid, out var stat))
                            {
                                stat = new PlayerStat { Uid = operatorUid };
                                playerStats[operatorUid] = stat;
                            }

                            // 总是尝试识别职业（只要当前没值）
                            if (string.IsNullOrWhiteSpace(stat.Profession))
                            {
                                var profession = Common.GetProfessionBySkill(skill);
                                if (!string.IsNullOrWhiteSpace(profession))
                                {
                                    stat.Profession = profession;
                                    // Console.WriteLine($"[职业识别成功] 玩家 {operatorUid} → {profession}");
                                }
                            }

                           

                            stat.RecordHit(damage, isCrit, luckyValue != 0, hpLessen);
                           

                        }



                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析异常: {ex}");
            }
        }







        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshDpsTabel();
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {
            //注册钩子
            gg = new KeyboardHook();
            gg.SetHook();
            //绑定事件
            gg.OnKeyDownEvent += kh_OnKeyDownEvent;

            config.reader.Load(config.config_ini);//加载配置文件
            //config.network_card = Convert.ToInt32(config.reader.GetValue("SetUp", "NetworkCard"));
            if (!string.IsNullOrEmpty(config.reader.GetValue("SetUp", "NetworkCard")))
            {
                config.network_card = Convert.ToInt32(Convert.ToInt32(config.reader.GetValue("SetUp", "NetworkCard")));
                label2.Visible = false;
            }
            else
            {
                config.network_card = -1;

            }



            if (!string.IsNullOrEmpty(config.reader.GetValue("SetUp", "Transparency")))
                config.transparency = Convert.ToInt32(config.reader.GetValue("SetUp", "Transparency"));
            else
                config.transparency = 100; // 或者其他表示"未配置"的值




            var colorStr = config.reader.GetValue("SetUp", "DpsColor");
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
                    config.dpscolor = Color.FromArgb(a, r, g, b);
                }
                else
                {
                    // 解析失败用默认色
                    config.dpscolor = Color.FromArgb(252, 227, 138);
                }
            }
            else
            {
                config.dpscolor = Color.FromArgb(252, 227, 138);
            }



        }

        private bool isLight = true;
        private bool Top = false;
        private void button2_Click(object sender, EventArgs e)
        {
            isLight = !isLight;
            //这里使用了Toggle属性切换图标
            button2.Toggle = !isLight;
            FormGui.SetColorMode(this, isLight);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Top)
            {
                Top = false;

            }
            else
            {
                Top = true;

            }

            button3.Toggle = Top;
            this.TopMost = Top;


        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var form = new Setup(this))
            {


                form.inputNumber1.Value = (decimal)config.transparency;
                form.colorPicker1.Value = config.dpscolor;
                string title = AntdUI.Localization.Get("systemset", "请选择网卡");
                AntdUI.Modal.open(new AntdUI.Modal.Config(this, title, form, TType.Info)
                {

                    CloseIcon = true,
                    BtnHeight = 0,

                });
                config.transparency = (double)form.inputNumber1.Value;
                if (config.transparency < 10)
                {
                    config.transparency = 100;
                    MessageBox.Show("透明度不能低于10%，已自动设置为100%");
                }
                config.reader.Load(config.config_ini);//加载配置文件
                config.reader.SaveValue("SetUp", "NetworkCard", config.network_card.ToString());
                config.reader.SaveValue("SetUp", "Transparency", form.inputNumber1.Value.ToString());
                config.reader.SaveValue("SetUp", "DpsColor", config.dpscolor.ToString());
                config.reader.Save(config.config_ini);
                label2.Visible = false;


            }
        }

        private void checkbox1_CheckedChanged(object sender, BoolEventArgs e)
        {
            tabel_load();
        }

        private void dropdown1_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {

            if(monitor)
            {
                MessageBox.Show("请先停止监控后再查看历史数据");
                return;
            
            }

            tabel.dps_tabel.Clear();
            playerStats.Clear();
            ShowHistoricalDps(e.Value.ToString());

            dropdown1.SelectedValue = -1;
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
                tabel.dps_tabel.Add(new DpsTabel(
                    item.uid,
                    item.profession,
                    item.totalDamage,
                    item.criticalDamage,
                    item.luckyDamage,
                    item.critLuckyDamage,
                     double.TryParse(item.critRate, out var cr) ? cr : 0,
                    double.TryParse(item.luckyRate, out var lr) ? lr : 0,
                    item.instantDps,
                    item.maxInstantDps,
                    item.totalDps,
                    new CellProgress(item.CellProgress?.Value ?? 0)
                    {
                        Size = new Size(300, 10),
                        Fill = config.dpscolor
                    }
                ));
            }
        }
    }
}
