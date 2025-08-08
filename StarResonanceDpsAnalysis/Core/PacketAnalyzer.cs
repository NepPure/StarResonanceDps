using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PacketDotNet;
using SharpPcap;
using StarResonanceDpsAnalysis.Plugin;
using ZstdNet;

namespace StarResonanceDpsAnalysis.Core
{
    public class PacketAnalyzer()
    {
        #region ====== 字段定义 ======
        private readonly BlockingCollection<(ICaptureDevice dev, RawCapture raw)> _queue
        = new(8192);
        // 启动 N 个工作线程（放到 PacketAnalyzer 里）
        private readonly List<Thread> _threads = new();

        // worker 管理
        private CancellationTokenSource _cts = new();
        #endregion

        #region ====== 公共属性与状态 ======
        public AnalyzerState State { get; private set; } = AnalyzerState.NeverStarted;

        // TCP 分片缓存：Key 是 TCP 序列号，Value 是对应的分片数据
        // 用于重组多段 TCP 数据流（比如一个完整的 protobuf 消息被拆分在多个包里）
        private static ConcurrentDictionary<uint, byte[]> TcpCache { get; } = new();

        /// <summary>当前已缓存的数据缓冲区，用于拼接多个 TCP 包的数据</summary>
        private byte[] TcpDataBuffer { get; set; } = [];

        /// <summary>期望的下一个 TCP 序列号</summary>
        private uint TcpNextSeq { get; set; } = 0;

        /// <summary>上一次接收到数据包的时间</summary>
        public static DateTime LastPacketTime { get; private set; } = DateTime.MinValue;

        // private ICaptureDevice? SelectedDevice { get; init; } = null;
        // private RawCapture Raw { get; init; } = null;
        private bool HasAppliedFilter { get; set; } = false;
        public static string CurrentServer { get; set; } = string.Empty;
        private ulong UserUid { get; set; } = 0;
        private ConcurrentDictionary<uint, DateTime> TcpCacheTime { get; } = new();
        private MemoryStream TcpStream { get; } = new();
        public Exception? LastException { get; private set; } = null;

        private int workerCount;
        #endregion

        #region ====== 静态辅助方法 ======
        public static void Recaptured()
        {
            LastPacketTime = DateTime.Now;
        }
        #endregion

        #region ========== Public API: Start / Enqueue / Stop ==========
        public void Start(int workerCount = 5)
        {
            if (_threads.Count > 0) return; // 已经启动，防重复
            if (workerCount <= 0) workerCount = Math.Max(2, Environment.ProcessorCount / 2);

            _cts = new CancellationTokenSource();

            for (int i = 0; i < workerCount; i++)
            {
                var t = new Thread(() => AnalyzerAction(_cts.Token))
                {
                    IsBackground = true,
                    Name = $"PA-Worker-{i}"
                };
                _threads.Add(t);
                t.Start();
            }

          
            Console.WriteLine($"线程创建: {workerCount}");
        }

        public void Enqueue(ICaptureDevice dev, RawCapture raw)
        {
            _queue.Add((dev, raw));
      
           // int count = _queue.Count;
           // Console.WriteLine($"当前队列长度: {count}");
        }

        public void Stop(bool drain = true, int maxWaitMs = 10000)
        {
            if (drain)
            {
                // 让消费者知道没有新数据了，消费到空即可退出
                _queue.CompleteAdding();
            }
            else
            {
                // 立即取消阻塞中的 Take()
                _cts?.Cancel();
            }

            foreach (var t in _threads)
            {
                try { t.Join(2000); } catch { }
            }
            _threads.Clear();

            // 兜底清空
            while (_queue.TryTake(out _)) { }

        }
        #endregion


        #region 看门

        #endregion

        #region ====== 工作线程主循环 ======
        public void AnalyzerAction(CancellationToken token)
        {
            const int batchMax = 12000;
            var batch = new List<(ICaptureDevice device, RawCapture raw)>(1024);

            try
            {
                while (true)
                {
                    var first = _queue.Take(token); // 阻塞取1个，支持 Cancel/CompleteAdding
                    batch.Add(first);

                    while (batch.Count < batchMax && _queue.TryTake(out var more))
                        batch.Add(more);

                    for (int i = 0; i < batch.Count; i++)
                    {
                        var (device, raw) = batch[i];
                        HandleRaw(device, raw);      // ⚠️ 确保线程安全地更新共享状态
                        token.ThrowIfCancellationRequested();
                    }
                    batch.Clear();
                }
            }
            catch (OperationCanceledException) { /* 取消退出 */ }
            catch (InvalidOperationException) { /* CompleteAdding + 取空退出 */ }
        }
        #endregion

        #region ========== Stage 1: 逐包解析入口 ==========
        /// <summary>
        /// 处理单个数据包
        /// </summary>
        /// <param name="packetObj">数据包对象</param>
        private void HandleRaw(ICaptureDevice? device, RawCapture raw)
        {
            try
            {
                // 使用 PacketDotNet 解析为通用数据包对象（包含以太网/IP/TCP 等）
                var packet = Packet.ParsePacket(raw.LinkLayerType, raw.Data);

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
                if (!HasAppliedFilter && VerifyServer(srcServer, payload))
                {
                    ApplyDynamicFilter(device, srcServer);
                    HasAppliedFilter = true;
                }


                // 如果距离上次收到包的时间超过 30 秒，认为可能断线，清除缓存
                if (LastPacketTime != DateTime.MinValue && (DateTime.Now - LastPacketTime).TotalSeconds > 30)
                {
                    Console.WriteLine("长时间未收到包，可能已断线");
                    CurrentServer = "";
                    ClearTcpCache(); // 清除 TCP 缓存数据
                }

                //如果当前服务器（源地址）发生变化，进行验证
                if (CurrentServer != srcServer)
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
        #endregion

        #region ========== Stage 1.1: 服务器识别 / 过滤器应用 ==========
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
                if (CurrentServer == srcServer)
                {
                    return false;
                }


                // 第 5 字节不为 0，则不是“小包”格式（协议约定）
                if (buf.Length >= 10 && buf[4] == 0)
                {
                    // 提取去掉前10字节后的内容（通常前面是包头或控制信息）
                    var data = buf.AsSpan(10);
                    if (data.Length > 0)
                    {
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
                            // var body = global::Blueprotobuf.Decode(data1.AsSpan(18).ToArray());
                            if (CurrentServer != srcServer)
                            {
                                // 成功识别服务器，设置当前 server
                                CurrentServer = srcServer;
                                Console.WriteLine($"识别到场景服务器地址: {srcServer}");

                                // 清除之前可能未完成的 TCP 拼包缓存
                                ClearTcpCache();
                                return true; // ✅ 成功识别，立即返回
                            }
                            //// 从结构中尝试获取玩家 UID（字段[1]->[5]）
                            //if (data1.Length >= 17 && data1[17] == 0x2E) // 特定标志字节 0x2E
                            //{
                            //    var b1 = ProtoFieldHelper.TryGetDecoded(body, 1); // 获取字段 1 的子结构
                            //    if (b1 != null && ProtoFieldHelper.TryGetU64(b1, 5) is ulong rawUid && rawUid != 0)
                            //    {
                            //        UserUid = rawUid >> 16; // 截断低16位，仅保留玩家 UID 主体
                            //        Console.WriteLine($"识别玩家UID: {UserUid}");

                            //    }
                            //}

                        }

                    }

                }
                if (buf.Length == 0x62)
                {
                    byte[] signature_ = new byte[]
                    {
                                0x00, 0x00, 0x00, 0x62,
                                0x00, 0x03,
                                0x00, 0x00, 0x00, 0x01,
                                0x00, 0x11, 0x45, 0x14, // seq?
                                0x00, 0x00, 0x00, 0x00,
                                0x0a, 0x4e, 0x08, 0x01, 0x22, 0x24
                    };

                    bool firstPartMatch = buf.AsSpan(0, 10).SequenceEqual(signature_.AsSpan(0, 10));
                    bool secondPartMatch = buf.AsSpan(14, 6).SequenceEqual(signature_.AsSpan(14, 6));

                    if (firstPartMatch && secondPartMatch)
                    {
                        if (CurrentServer != srcServer)
                        {
                            CurrentServer = srcServer;
                            ClearTcpCache();
                            Console.WriteLine("Got Scene Server Address by Login Return Packet: " + srcServer);
                            return true; // ✅ 成功识别，立即返回
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                // 捕获所有异常，避免崩溃
                //Console.WriteLine($"[VerifyServer 异常] {ex}");
            }

            return false; // 无法识别为目标服务器
        }

        private void ApplyDynamicFilter(ICaptureDevice? device, string srcServer)
        {
            if (device == null)
            {
                Console.WriteLine("[错误] selectedDevice为null，无法应用过滤器");
                return;
            }

            // 拆出服务器那半
            var serverPart = srcServer.Split("->")[0].Trim();       // "36.152.0.122:16125"
            var parts = serverPart.Split(':');
            var serverIp = parts[0];
            var serverPort = parts[1];

            device.Filter = $"tcp and host {serverIp}";
            // Console.WriteLine($"【Filter 已更新】 tcp and host {serverIp} and port {serverPort}");
        }
        #endregion

        #region ========== Stage 2: TCP 重组 ==========
        private void ReassembleTcp(TcpPacket tcpPacket, byte[] payload)
        {
            var seq = tcpPacket.SequenceNumber;

            lock (TcpCache)
            {
                // 尝试初始化 tcpNextSeq（只有第一次设置）
                if (TcpNextSeq == 0 && payload.Length > 4)
                {
                    int len = (payload[0] << 24) | (payload[1] << 16) | (payload[2] << 8) | payload[3];
                    if (len < 999999) // 判断为合理包头
                    {
                        TcpNextSeq = seq;
                    }
                }

                // 缓存当前段
                TcpCache[seq] = payload;
                TcpCacheTime[seq] = DateTime.Now;

                // 清理超时缓存（超过10秒）
                var expired = TcpCacheTime
                    .Where(kv => (DateTime.Now - kv.Value).TotalSeconds > 10)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var key in expired)
                {
                    TcpCache.Remove(key, out var v1);
                    TcpCacheTime.Remove(key, out var v2);


                }

                // —— 关键：处理拼接 —— 
                int skipTimeoutMs = 200;

                // 找到当前可用的最小 Seq
                var sortedSeqs = TcpCache.Keys.OrderBy(k => k).ToList();

                while (true)
                {
                    if (TcpCache.TryGetValue(TcpNextSeq, out var chunk))
                    {
                        // 找到了当前需要的段，正常拼接
                        TcpStream.Seek(0, SeekOrigin.End);
                        TcpStream.Write(chunk, 0, chunk.Length);

                        TcpCache.Remove(TcpNextSeq, out var v1);
                        TcpCacheTime.Remove(TcpNextSeq, out var v2);
                        TcpNextSeq += (uint)chunk.Length;
                        LastPacketTime = DateTime.Now;
                    }
                    else
                    {
                        // 当前段还没到，判断是否跳过等待
                        var delayed = sortedSeqs
                            .Where(k => k > TcpNextSeq && TcpCacheTime.ContainsKey(k))
                            .FirstOrDefault(k => (DateTime.Now - TcpCacheTime[k]).TotalMilliseconds > skipTimeoutMs);

                        if (delayed != 0)
                        {
                            // 跳过迟迟不到的 tcpNextSeq，跳到新的 delayed
                            TcpNextSeq = delayed;
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
            TcpStream.Seek(0, SeekOrigin.Begin);

            while (TcpStream.Length - TcpStream.Position >= 4)
            {
                byte[] lenBytes = new byte[4];
                TcpStream.Read(lenBytes, 0, 4);
                int len = (lenBytes[0] << 24) | (lenBytes[1] << 16) | (lenBytes[2] << 8) | lenBytes[3];

                if (len > 999999)
                {
                    // 非法长度，清空整个缓冲区
                    TcpStream.SetLength(0);
                    return;
                }

                long remain = TcpStream.Length - TcpStream.Position;
                if (remain >= len - 4)  // 剩余是否够“包体”的长度
                {
                    // 数据够一个完整包
                    byte[] packet = new byte[len];
                    Array.Copy(lenBytes, 0, packet, 0, 4);
                    TcpStream.Read(packet, 4, len - 4);
                    临时字段测试.process(packet);
                    // 异步处理，防止 UI 卡顿
                    //AnalyzePacket(packet);
                }
                else
                {
                    // 不够，回退4字节
                    TcpStream.Position -= 4;
                    break;
                }
            }

            // 剩余未读部分保留到新缓冲区
            long unread = TcpStream.Length - TcpStream.Position;
            if (unread > 0)
            {
                byte[] remain = new byte[unread];
                TcpStream.Read(remain, 0, (int)unread);
                TcpStream.SetLength(0);
                TcpStream.Write(remain, 0, remain.Length);
            }
            else
            {
                TcpStream.SetLength(0);
            }
        }
        #endregion

        #region ========== Stage 3: 业务解析（解压/拆包/统计） ==========
        /// <summary>
        /// 处理完整的 TCP 数据包，包括解压缩、解包、protobuf 解析、技能伤害提取与统计
        /// </summary>
        /// <param name="buf">一个完整的 TCP 数据包，前4字节为长度</param>
        private void AnalyzePacket(byte[] buf)
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
                    var body = global::Blueprotobuf.Decode(data1.AsSpan(18).ToArray());

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
                            if (UserUid != uid)
                            {
                                UserUid = uid;
                                Console.WriteLine("识别玩家UID: " + UserUid);
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
                            if (damage == 0) continue;

                            string extra = isCrit ? "暴击" : luckyValue != 0 ? "幸运" : isMiss ? "Miss" : "普通";

                            Console.WriteLine($"玩家 {operatorUid} 使用技能 {skill} 造成伤害 {damage} 扣血 {hpLessen} 标记 {extra}");
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
                            MainForm.RefreshDpsTable();


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
        #endregion

        #region ====== TCP 缓存清理 ======
        /// <summary>
        /// 清除 TCP 缓存，用于断线、错误重组等情况的重置操作
        /// </summary>
        private void ClearTcpCache()
        {
            // 清空当前拼接的数据缓冲区（通常是多段 TCP 包拼接中间状态）
            TcpDataBuffer = new byte[0];

            // 重置下一个期望的 TCP 序列号为 0（重新开始拼接）
            TcpNextSeq = 0;
            HasAppliedFilter = false;
            // 清空缓存的 TCP 分片数据（已收到但未处理的包）
            TcpCache.Clear();
        }
        #endregion
    }

    #region ====== 枚举定义 ======
    public enum AnalyzerState
    {
        Errored = -1,

        NeverStarted = 0,
        Running = 1,
        Completed = 2,
    }
    #endregion
}
