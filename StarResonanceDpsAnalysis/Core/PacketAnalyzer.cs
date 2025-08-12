using PacketDotNet;
using SharpPcap;
using StarResonanceDpsAnalysis.Extends;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace StarResonanceDpsAnalysis.Core
{
    public class PacketAnalyzer()
    {
        #region ====== 常量定义 ======

        /// <summary>
        /// 服务器签名
        /// </summary>
        /// <remarks>
        /// 不确定是不是服务器签名, StarResonanceDamageCounter 中, 后面还跟了 //c3SB?? 这样的注释
        /// </remarks>
        private readonly byte[] ServerSignature = [0x00, 0x63, 0x33, 0x53, 0x42, 0x00];

        /// <summary>
        /// 切换服务器时的登录返回包签名
        /// </summary>
        private readonly byte[] LoginReturnSignature =
        [
            0x00, 0x00, 0x00, 0x62,
            0x00, 0x03,
            0x00, 0x00, 0x00, 0x01,
            0x00, 0x11, 0x45, 0x14, // seq?
            0x00, 0x00, 0x00, 0x00,
            0x0a, 0x4e, 0x08, 0x01, 0x22, 0x24
        ];

        #endregion

        #region ====== 公共属性与状态 ======

        /// <summary>
        /// 当前连接的服务器地址
        /// </summary>
        public string CurrentServer { get; set; } = string.Empty;
        /// <summary>
        /// 期望的下一个 TCP 序列号
        /// </summary>
        private uint? TcpNextSeq { get; set; } = null;
        /// <summary>
        /// TCP 分片缓存
        /// </summary>
        /// <remarks>
        /// Key 是 TCP 序列号, Value 是对应的分片数据, 用于重组多段 TCP 数据流 (比如一个完整的 protobuf 消息被拆分在多个包里)
        /// </remarks>
        private ConcurrentDictionary<uint, byte[]> TcpCache { get; } = new();
        private DateTime TcpLastTime { get; set; } = DateTime.MinValue;
        private object TcpLock { get; } = new();

        private MemoryStream TcpStream { get; } = new();
        private ConcurrentDictionary<uint, DateTime> TcpCacheTime { get; } = new();


        #endregion

        #region ========== 启用新分析 ==========

        public void StartNewAnalyzer(ICaptureDevice device, RawCapture raw)
        {
            Task.Run(() =>
            {
                try
                {
                    HandleRaw(device, raw);
                }
                catch (Exception ex)
                {
                    var taskIdStr = (Task.CurrentId?.ToString() ?? "?") + ' ';
                    Console.WriteLine($"""

                        ==== ThreadID: {taskIdStr.PadRight(8, '=')}==============
                        封包分析时遇到关键性崩溃: {ex.Message}
                        {ex.StackTrace}
                        =======================

                        """);
                }
            });
        }

        #endregion

        #region ========== Stage 1: 逐包解析入口 ==========
        // 统一的 TCP 序号比较（考虑 32 位回绕）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int SeqCmp(uint a, uint b) => unchecked((int)(a - b)); // a>=b => 非负；a<b（跨回绕）=> 负


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
                if (tcpPacket == null) return;

                // 提取 IPv4 数据包（如果不是 IPv4，也会返回 null）
                var ipv4Packet = packet.Extract<IPv4Packet>();
                if (ipv4Packet == null) return;

                // 获取 TCP 负载（即应用层数据）
                var payload = tcpPacket.PayloadData;
                if (payload == null || payload.Length == 0) return;

                // 构造当前包的源 -> 目的 IP 和端口的字符串，作为唯一标识
                var srcServer = $"{ipv4Packet.SourceAddress}:{tcpPacket.SourcePort} -> {ipv4Packet.DestinationAddress}:{tcpPacket.DestinationPort}";

                lock (TcpLock)
                {
                    if (CurrentServer != srcServer)
                    {
                        try
                        {
                            // 尝试通过小包识别服务器
                            if (payload.Length > 10 && payload[4] == 0)
                            {
                                var data = payload.AsSpan(10);
                                if (data.Length > 0)
                                {
                                    using var payloadMs = new MemoryStream(data.ToArray());
                                    byte[] tmp;
                                    do
                                    {
                                        var lenBuffer = new byte[4];
                                        if (payloadMs.Read(lenBuffer, 0, 4) != 4)
                                            break;

                                        var len = lenBuffer.ReadInt32BigEndian();
                                        if (len < 4 || len > payloadMs.Length - 4)
                                        {
                                            break;
                                        }

                                        tmp = new byte[len - 4];
                                        if (payloadMs.Read(tmp, 0, tmp.Length) != tmp.Length)
                                        {
                                            break;
                                        }

                                        if (!tmp.Skip(5).Take(ServerSignature.Length).SequenceEqual(ServerSignature))
                                        {
                                            break;
                                        }

                                        try
                                        {
                                            if (CurrentServer != srcServer)
                                            {
                                                CurrentServer = srcServer;
                                                ClearTcpCache();
                                                TcpNextSeq = tcpPacket.SequenceNumber + (uint)payload.Length;
                                                Console.WriteLine($"Got Scene Server Address: {srcServer}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"""

                                                =======================
                                                HandleRaw 检测场景服务器时遇到关键性崩溃: {ex.Message}
                                                {ex.StackTrace}
                                                =======================

                                                """);
                                        }
                                    }
                                    while (tmp.Length > 0);
                                }
                            }

                            // 尝试通过登录返回包识别服务器(仍需测试)
                            if (payload.Length == 0x62)
                            {
                                if (payload.AsSpan(0, 10).SequenceEqual(LoginReturnSignature.AsSpan(0, 10)) &&
                                    payload.AsSpan(14, 6).SequenceEqual(LoginReturnSignature.AsSpan(14, 6)))
                                {
                                    if (CurrentServer != srcServer)
                                    {
                                        CurrentServer = srcServer;
                                        ClearTcpCache();
                                        TcpNextSeq = tcpPacket.SequenceNumber + (uint)payload.Length;

                                        Console.WriteLine($"Got Scene Server Address by Login Return Packet: {srcServer}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"""

                                =======================
                                HandleRaw 中遇到关键性崩溃: {ex.Message}
                                {ex.StackTrace}
                                =======================

                                """);
                        }

                        return;
                    }
                    // 这里已经是识别到的服务器的包了
                    if (TcpNextSeq == null)
                    {
                        Console.WriteLine("Unexpected TCP capture error! tcp_next_seq is NULL");
                        if (payload.Length > 4 && BinaryPrimitives.ReadUInt32BigEndian(payload) < 0x0fffff)
                        {
                            TcpNextSeq = tcpPacket.SequenceNumber;
                        }
                    }

                    // 缓存策略：只收“当前/未来”的段（避免重复旧段占用内存）
                    if (TcpNextSeq == null || SeqCmp(tcpPacket.SequenceNumber, TcpNextSeq.Value) >= 0)
                    {
                        TcpCache[tcpPacket.SequenceNumber] = payload.ToArray(); // 注意持有拷贝
                        //ScheduleEvictAfter(tcpPacket.SequenceNumber);
                    }

                    // 顺序拼接到一个临时缓冲（减少多次 ToArray）
                    using var messageMs = new MemoryStream(capacity: 4096);

                    while (TcpNextSeq != null && TcpCache.Remove(TcpNextSeq.Value, out var cachedTcpData))
                    {
                        messageMs.Write(cachedTcpData, 0, cachedTcpData.Length);
                        unchecked { TcpNextSeq += (uint)cachedTcpData.Length; }
                        TcpLastTime = DateTime.Now;
                    }

                    // 直接把本次拼好的字节“追加”到 TcpStream 末尾，避免 CopyTo 的中间拷贝
                    if (messageMs.Length > 0)
                    {
                        long endPos = TcpStream.Length;
                        TcpStream.Position = endPos;
                        messageMs.Position = 0;
                        messageMs.CopyTo(TcpStream);
                    }

                    // 解析：把 Position 设到当前未消费起点（即 0），循环取包
                    TcpStream.Position = 0;

                    Span<byte> lenBuf = stackalloc byte[4];
                    while (true)
                    {
                        long start = TcpStream.Position;
                        if (TcpStream.Length - start < 4) break; // 不足 4 字节长度头

                        int n = TcpStream.Read(lenBuf);
                        if (n < 4) { TcpStream.Position = start; break; }

                        int packetSize = BinaryPrimitives.ReadInt32BigEndian(lenBuf);

                        // 协议约定：长度为“总长（含 4B 头）”
                        if (packetSize <= 4 || packetSize > 0x0FFFFF)
                        {
                            TcpStream.Position = start; // 回滚，留待上层/后续处理
                            break;
                        }

                        if (TcpStream.Length - start < packetSize)
                        {
                            TcpStream.Position = start; // 包未齐，一个字节都不消费
                            break;
                        }

                        // 够完整包：读出 [4B长度头 + 负载]
                        TcpStream.Position = start;
                        byte[] messagePacket = new byte[packetSize];
                        int read = TcpStream.Read(messagePacket, 0, packetSize);
                        if (read != packetSize) { TcpStream.Position = start; break; }

                        MessageAnalyzer.Process(messagePacket);
                    }

                    // 压实剩余未解析数据到流头（零拷贝尽量减少临时数组）
                    if (TcpStream.Position > 0)
                    {
                        long remain = TcpStream.Length - TcpStream.Position;
                        if (remain > 0)
                        {
                            // 把剩余数据搬到前面
                            var buffer = TcpStream.GetBuffer(); // 需要确保 TcpStream 是可公开缓冲的 MemoryStream
                            Buffer.BlockCopy(buffer, (int)TcpStream.Position, buffer, 0, (int)remain);
                            TcpStream.Position = 0;
                            TcpStream.SetLength(remain);
                        }
                        else
                        {
                            TcpStream.SetLength(0);
                            TcpStream.Position = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 捕获异常，避免程序崩溃，同时打印异常信息
                Console.WriteLine($"包处理异常: {ex.Message}\r\n{ex.StackTrace}");
            }
        }


        public void ResetCaptureState()
        {
            lock (TcpLock)
            {
                CurrentServer = string.Empty;      // 或者 null，看你的判断逻辑
                TcpNextSeq = null;
                TcpLastTime = DateTime.MinValue;

                TcpCache.Clear();
                TcpCacheTime.Clear();

                // 如果上一轮流变得很大，直接丢弃换新更省内存
                if (TcpStream.Capacity > 1 << 20)  // >1MB 就换新，阈值自定
                {
                    TcpStream.Dispose();
                    // 如果需要 GetBuffer，确保用可公开缓冲的构造
                    typeof(MemoryStream)
                        .GetMethod("Dispose", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                        .Invoke(TcpStream, null); // 可忽略：只是确保释放
                }
                else
                {
                    TcpStream.Position = 0;
                    TcpStream.SetLength(0);
                }
            }
        }

        #endregion

        #region ====== TCP 缓存清理 ======
        /// <summary>
        /// 清除 TCP 缓存，用于断线、错误重组等情况的重置操作
        /// </summary>
        private void ClearTcpCache()
        {
            TcpNextSeq = null;
            TcpLastTime = DateTime.MinValue;
            TcpCache.Clear();
        }
        #endregion
    }

}
