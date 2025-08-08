using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;

namespace StarResonanceDpsAnalysis
{
    public sealed class ByteReader
    {
        private readonly byte[] _buffer;
        private int _offset;

        public ByteReader(byte[] buffer, int offset = 0)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _offset = offset;
        }

        public int Remaining => _buffer.Length - _offset;

        // ===== UInt64 (大端) =====
        public ulong ReadUInt64BE()
        {
            if (Remaining < 8) throw new EndOfStreamException();
            ulong value =
                ((ulong)_buffer[_offset] << 56) |
                ((ulong)_buffer[_offset + 1] << 48) |
                ((ulong)_buffer[_offset + 2] << 40) |
                ((ulong)_buffer[_offset + 3] << 32) |
                ((ulong)_buffer[_offset + 4] << 24) |
                ((ulong)_buffer[_offset + 5] << 16) |
                ((ulong)_buffer[_offset + 6] << 8) |
                _buffer[_offset + 7];
            _offset += 8;
            return value;
        }

        public ulong PeekUInt64BE()
        {
            if (Remaining < 8) throw new EndOfStreamException();
            return
                ((ulong)_buffer[_offset] << 56) |
                ((ulong)_buffer[_offset + 1] << 48) |
                ((ulong)_buffer[_offset + 2] << 40) |
                ((ulong)_buffer[_offset + 3] << 32) |
                ((ulong)_buffer[_offset + 4] << 24) |
                ((ulong)_buffer[_offset + 5] << 16) |
                ((ulong)_buffer[_offset + 6] << 8) |
                _buffer[_offset + 7];
        }

        // ===== UInt32 (大端) =====
        public uint ReadUInt32BE()
        {
            if (Remaining < 4) throw new EndOfStreamException();
            uint value =
                (uint)(_buffer[_offset] << 24 |
                       _buffer[_offset + 1] << 16 |
                       _buffer[_offset + 2] << 8 |
                       _buffer[_offset + 3]);
            _offset += 4;
            return value;
        }

        public uint PeekUInt32BE()
        {
            if (Remaining < 4) throw new EndOfStreamException();
            return (uint)(_buffer[_offset] << 24 |
                          _buffer[_offset + 1] << 16 |
                          _buffer[_offset + 2] << 8 |
                          _buffer[_offset + 3]);
        }

        // ===== UInt16 (大端) =====
        public ushort ReadUInt16BE()
        {
            if (Remaining < 2) throw new EndOfStreamException();
            ushort value = (ushort)(_buffer[_offset] << 8 | _buffer[_offset + 1]);
            _offset += 2;
            return value;
        }

        public ushort PeekUInt16BE()
        {
            if (Remaining < 2) throw new EndOfStreamException();
            return (ushort)(_buffer[_offset] << 8 | _buffer[_offset + 1]);
        }

        // ===== Bytes =====
        public byte[] ReadBytes(int length)
        {
            if (length < 0 || Remaining < length) throw new EndOfStreamException();
            var result = new byte[length];
            Buffer.BlockCopy(_buffer, _offset, result, 0, length);
            _offset += length;
            return result;
        }

        public byte[] PeekBytes(int length)
        {
            if (length < 0 || Remaining < length) throw new EndOfStreamException();
            var result = new byte[length];
            Buffer.BlockCopy(_buffer, _offset, result, 0, length);
            return result;
        }

        public byte[] ReadRemaining()
        {
            return ReadBytes(Remaining);
        }

    }

    public class 临时字段测试
    {
        // 简单的解压封装（Zstd）——用你项目里已有的 DecompressionStream
        private static byte[] DecompressPayload(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var zstd = new DecompressionStream(input); // 你的 Zstd 解压流类型
            using var output = new MemoryStream();
            zstd.CopyTo(output);
            return output.ToArray();
        }

        public static void process(byte[] packet)
        {
            var r = new ByteReader(packet);           // 用前面给你的大端 ByteReader
            uint packetSize = r.ReadUInt32BE();       // [0..3] 长度（含自身4字节）
            if (packetSize != packet.Length)
            {
                Console.WriteLine($"Invalid packet size: header={packetSize}, actual={packet.Length}");
                return;
            }

            ushort packetType = r.ReadUInt16BE();     // [4..5] 类型（高位压缩标志 / 低15位消息类型）
            bool isZstdCompressed = (packetType & 0x8000) != 0; // 最高位 bit15
            int msgTypeId = packetType & 0x7fff;        // 低15位
            Console.WriteLine(BitConverter.ToString(packet, 0, Math.Min(packet.Length, 16)));

  
            Console.WriteLine(msgTypeId);
            switch (msgTypeId)
            {
                case 2: // 推送消息：服务器主动下发
                    _processNotifyMsg(r, isZstdCompressed);
                    break;
                case 3:  // 调用结果：响应/返回值
                    break;
                case 6: // 载荷仍是一包“完整下行包”，需递归解析
                    break;
                default:
                    // FrameDown 特有的“服务器序列号”，可用于丢包/乱序检测（当前仅读取不使用）
                    uint serverSequenceId = r.ReadUInt32BE();

                    // 如果没有后续载荷，说明是空 FrameDown，直接返回
                    if (r.Remaining == 0) return;

                    // 剩余即为“嵌套包”（通常本身就是一个完整的下行包：含 size + type + body）
                    byte[] nestedPacket = r.ReadRemaining();
                    // 如果外层标记了压缩，这里需要先解压再继续
                    if (isZstdCompressed)
                    {
                        nestedPacket = DecompressPayload(nestedPacket);
                    }
                    // 递归处理内部包（内部包的格式与外层一致）
                    process(nestedPacket);
                    break;
            }
        }

        public enum NotifyMethod : uint
        {
            SyncNearEntities = 0x00000006,
            SyncNearDeltaInfo = 0x0000002d,
            SyncToMeDeltaInfo = 0x0000002e
        }


        public static void _processNotifyMsg(ByteReader packet, bool isZstdCompressed)
        {
            var serviceUuid = packet.ReadUInt64BE();
            var stubId = packet.ReadUInt32BE();
            var methodId = packet.ReadUInt32BE();

            if (serviceUuid != 0x0000000063335342UL)
            {
                Console.WriteLine($"Skipping NotifyMsg with serviceId {serviceUuid}");
                return;
            }
            byte[] msgPayload = packet.ReadRemaining();

            // 从第10字节开始是压缩数据，前10字节为协议头
            using var inputStream = new MemoryStream(msgPayload, 10, msgPayload.Length - 10);
            using var decompressionStream = new DecompressionStream(inputStream);
            using var outputStream = new MemoryStream();

            // 解压到 outputStream
            decompressionStream.CopyTo(outputStream);

            var decompressed = outputStream.ToArray();

            // 把原始前10字节头部和解压后的数据拼接成一个新的完整包

            msgPayload = msgPayload.Take(10).Concat(decompressed).ToArray();
            Console.WriteLine($"[stubId={stubId}, methodId={methodId}]  protobuf body size={decompressed.Length}");

            ProtoPrinter.Print(decompressed);            // ← 递归打印 tag / wireType / 值/子消息

            switch (methodId)
            {
                case (uint)NotifyMethod.SyncNearEntities:
                    _processSyncNearEntities(msgPayload);
                    break;

                case (uint)NotifyMethod.SyncToMeDeltaInfo:
                    //_processSyncToMeDeltaInfo(msgPayload);
                    break;

                case (uint)NotifyMethod.SyncNearDeltaInfo:
                    //_processSyncNearDeltaInfo(msgPayload);
                    break;

                default:
                    Console.WriteLine($"Skipping NotifyMsg with methodId {methodId}");
                    break;
            }


        }

        public static void _processSyncNearEntities(byte[] payloadBuffer)
        {
            var syncNearEntities = ProtoDynamic.Decode(payloadBuffer);
            //if (syncNearEntities.Appear == null || syncNearEntities.Appear.Count == 0)
            //    return;

        }
    }



}
