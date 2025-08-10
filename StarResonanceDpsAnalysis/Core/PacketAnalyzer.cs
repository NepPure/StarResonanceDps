using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PacketDotNet;
using SharpPcap;
using StarResonanceDpsAnalysis.Extends;
using StarResonanceDpsAnalysis.Plugin;
using ZstdNet;
using static System.Runtime.InteropServices.JavaScript.JSType;

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


        private ulong UserUid { get; set; } = 0;

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
                        if (payload.Length > 4 && payload.ReadUInt32BigEndian() < 0x0fffff)
                        {
                            TcpNextSeq = tcpPacket.SequenceNumber;
                        }
                    }
                    if (TcpNextSeq == null || (int)(TcpNextSeq - tcpPacket.SequenceNumber) <= 0)
                    {
                        TcpCache[tcpPacket.SequenceNumber] = payload;
                    }

                    using var messageMs = new MemoryStream();
                    while (TcpNextSeq != null && TcpCache.Remove(TcpNextSeq.Value, out var cachedTcpData))
                    {
                        messageMs.Write(cachedTcpData, 0, cachedTcpData.Length);
                        TcpNextSeq += (uint)cachedTcpData.Length;
                        TcpLastTime = DateTime.Now;
                    }

                    messageMs.Position = 0;
                    messageMs.CopyTo(TcpStream);
                    TcpStream.Position = 0;

                    while (true)
                    {
                        if (TcpStream.Length - TcpStream.Position < 4) break;

                        var packetSizeBytes = new byte[4];
                        TcpStream.Read(packetSizeBytes, 0, 4);

                        TcpStream.Position -= 4;
                        
                        var packetSize = packetSizeBytes.ReadInt32BigEndian();
                        if (packetSize <= 0 || packetSize > 0x0fffff)
                        {
                            // Console.WriteLine($"Invalid Length!! TcpNextSeq({TcpNextSeq}): size={packetSize}");
                            break;
                        }

                        if (TcpStream.Length - TcpStream.Position < packetSize)
                        {
                            TcpStream.Position -= 4;
                            break;
                        }

                        var messageBytes = new byte[packetSize];
                        TcpStream.Read(messageBytes, 0, packetSize);

                        Console.WriteLine($"<Buffer {string.Join(' ', messageBytes.Select(e => e.ToString("X2")))}>");

                        MessageAnalyzer.Process(messageBytes);

                    }

                    if (TcpStream.Position > 0)
                    {
                        var remain = (int)(TcpStream.Length - TcpStream.Position);
                        var tmp = new byte[remain];
                        TcpStream.Read(tmp, 0, remain);
                        TcpStream.SetLength(0);
                        TcpStream.Position = 0;
                        TcpStream.Write(tmp, 0, remain);
                    }
                }
            }
            catch (Exception ex)
            {
                // 捕获异常，避免程序崩溃，同时打印异常信息
                Console.WriteLine($"包处理异常: {ex.Message}\r\n{ex.StackTrace}");
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
