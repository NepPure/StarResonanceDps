using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;
using Google.Protobuf;   // Parser.ParseFrom 在这
using Blue;           // Blueprotobuf.cs 里的命名空间


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
        public bool TryPeekUInt32BE(out uint value)
        {
            if (Remaining < 4) { value = 0; return false; }
            value = (uint)(_buffer[_offset] << 24 |
                           _buffer[_offset + 1] << 16 |
                           _buffer[_offset + 2] << 8 |
                           _buffer[_offset + 3]);
            return true;
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


        private static byte[] DecompressZstdIfNeeded(byte[] body)
        {
            using var input = new MemoryStream(body);
            using var z = new DecompressionStream(input);
            using var output = new MemoryStream();
            z.CopyTo(output);
            return output.ToArray();
        }





        public static void process(byte[] packets)
        {
            try
            {
                // 外层：处理一次输入里可能包含的多个完整包
                var packetsReader = new ByteReader(packets);

                while (packetsReader.Remaining >= 4) // 至少读得到一个 packetSize
                {
                    // —— 预读长度，不前进指针 —— 
                    if (!packetsReader.TryPeekUInt32BE(out uint packetSize)) return;

                    // 合法包最短：size(4) + type(2) = 6
                    if (packetSize < 6)
                    {
                        Console.WriteLine($"Received invalid packet size (<6): {packetSize}. Drop the rest.");
                        return; // 丢掉剩余，避免死循环
                    }

                    // 不够一个完整包，直接返回等下一批（防止 ReadBytes 越界）
                    if (packetSize > (uint)packetsReader.Remaining)
                    {
                        Console.WriteLine($"Truncated packet: need {packetSize}, have {packetsReader.Remaining}. Stop here.");
                        return;
                    }

                    // —— 切出一个完整包，交给独立 reader 解析 —— 
                    var packetReader = new ByteReader(packetsReader.ReadBytes((int)packetSize));

                    // 包内逐项读取
                    uint sizeAgain = packetReader.ReadUInt32BE(); // 与 peek 相同
                    if (sizeAgain != packetSize)
                    {
                        Console.WriteLine($"Size mismatch: peek={packetSize}, inner={sizeAgain}. Skip this packet.");
                        continue;
                    }

                    ushort packetType = packetReader.ReadUInt16BE();
                    bool isZstdCompressed = (packetType & 0x8000) != 0; // bit15
                    int msgTypeId = packetType & 0x7FFF;                // 低15位

                    // 调试输出（按需保留）
                    //Console.WriteLine($"MsgType={msgTypeId}, Size={packetSize}, RemainInPacket={packetReader.Remaining}");

                    switch (msgTypeId)
                    {
                        case 2: // Notify
                            _processNotifyMsg(packetReader, isZstdCompressed);
                            break;

                        case 3: // Return
                            //_processReturnMsg(packetReader, isZstdCompressed);
                            break;

                        case 6: // FrameDown（注意：这里才读“服务器序列号”）
                            {
                                if (packetReader.Remaining < 4)
                                {
                                    Console.WriteLine("FrameDown missing serverSequenceId, skip.");
                                    break;
                                }

                                uint serverSequenceId = packetReader.ReadUInt32BE(); // 仅读取，不使用
                                if (packetReader.Remaining == 0) break; // 空 FrameDown

                                // 剩余即为嵌套包（通常自身也是“完整下行包”：size + type + body）
                                byte[] nestedPacket = packetReader.ReadRemaining();

                                if (isZstdCompressed)
                                {
                                    // 你的解压函数：输入压缩体 -> 输出解压后的原始字节
                                    nestedPacket = DecompressZstdIfNeeded(nestedPacket);
                                }

                                // 递归处理内部包
                                process(nestedPacket);
                                break;
                            }

                        default:
                            Console.WriteLine($"Ignore packet with message type {msgTypeId}.");
                            break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                // 统一吞掉越界异常，便于持续解析
                Console.WriteLine("Unexpected end of buffer while reading a packet.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public enum NotifyMethod : uint
        {
            SyncNearEntities = 0x00000006,
            SyncNearDeltaInfo = 0x0000002d,
            SyncToMeDeltaInfo = 0x0000002e
        }

        /// <summary>
        /// 属性类型（AttrId）— 仅列出当前关心的字段
        /// </summary>
        public enum AttrType
        {
            AttrName = 0x01,          // 玩家名字（string）
            AttrProfessionId = 0xDC,  // 职业 ID（int32）
            AttrFightPoint = 0x272E   // 战力（int32）
        }

        public enum ProfessionType
        {
            雷影剑士 = 1,
            冰魔导师 = 2,
            涤罪恶火_战斧 = 3,
            青岚骑士 = 4,
            森语者 = 5,
            雷霆一闪_手炮 = 6,
            巨刃守护者 = 7,
            暗灵祈舞_仪刀_仪仗 = 8,
            神射手 = 9,
            神盾骑士 = 10,
            灵魂乐手 = 11
        }
        public static string GetProfessionNameFromId(int professionId)
        {
            switch ((ProfessionType)professionId)
            {
                case ProfessionType.雷影剑士: return "雷影剑士";
                case ProfessionType.冰魔导师: return "冰魔导师";
                case ProfessionType.涤罪恶火_战斧: return "涤罪恶火·战斧";
                case ProfessionType.青岚骑士: return "青岚骑士";
                case ProfessionType.森语者: return "森语者";
                case ProfessionType.雷霆一闪_手炮: return "雷霆一闪·手炮";
                case ProfessionType.巨刃守护者: return "巨刃守护者";
                case ProfessionType.暗灵祈舞_仪刀_仪仗: return "暗灵祈舞·仪刀/仪仗";
                case ProfessionType.神射手: return "神射手";
                case ProfessionType.神盾骑士: return "神盾骑士";
                case ProfessionType.灵魂乐手: return "灵魂乐手";
                default: return ""; // 未知职业
            }
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
            byte[] protoBody = msgPayload; // 纯 protobuf 部分，初始化为原 payload
            if (isZstdCompressed)
            {
                msgPayload = DecompressZstdIfNeeded(msgPayload);
            }

            #region
            //if (isZstdCompressed && msgPayload.Length >= 4)
            //{
            //    bool looksZstdAt0 = msgPayload.Length >= 4 &&
            //                        msgPayload[0] == 0x28 && msgPayload[1] == 0xB5 &&
            //                        msgPayload[2] == 0x2F && msgPayload[3] == 0xFD;

            //    bool looksZstdAt10 = msgPayload.Length >= 14 &&
            //                         msgPayload[10] == 0x28 && msgPayload[11] == 0xB5 &&
            //                         msgPayload[12] == 0x2F && msgPayload[13] == 0xFD;

            //    try
            //    {
            //        if (looksZstdAt0)
            //        {
            //            using var in0 = new MemoryStream(msgPayload, writable: false);
            //            using var z0 = new ZstdNet.DecompressionStream(in0);
            //            using var out0 = new MemoryStream();
            //            z0.CopyTo(out0);
            //            protoBody = out0.ToArray(); // 解压后的 protobuf
            //            msgPayload = protoBody;     // 如果下游用这个变量
            //        }
            //        else if (looksZstdAt10)
            //        {
            //            using var in10 = new MemoryStream(msgPayload, 10, msgPayload.Length - 10, writable: false);
            //            using var z10 = new ZstdNet.DecompressionStream(in10);
            //            using var out10 = new MemoryStream();
            //            z10.CopyTo(out10);
            //            var decompressed = out10.ToArray();

            //            // 保留 10 字节协议头的版本
            //            msgPayload = msgPayload.AsSpan(0, 10).ToArray().Concat(decompressed).ToArray();

            //            // 纯 protobuf 部分去掉前 10 字节
            //            protoBody = decompressed;
            //        }
            //        else
            //        {
            //            Console.WriteLine("Notify flagged compressed, but no ZSTD magic at 0 or +10. Treat as plain.");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("ZSTD decompress failed: " + ex.Message + " — treat as plain.");
            //    }
            //}
            #endregion
            // ====== 打印区 ======
            //Console.WriteLine($"[PROTO][svc={serviceUuid:X16}][stub={stubId}][mid={methodId}] len={protoBody.Length}");
            //Console.WriteLine(BitConverter.ToString(protoBody));
            //ProtoPrinter.Print(protoBody); // 这里假设你有 ProtoPrinter

            // ====== 原有路由逻辑 ======
            switch (methodId)
            {
                case (uint)NotifyMethod.SyncNearEntities:
                    _processSyncNearEntities(msgPayload);
                    break;
                case (uint)NotifyMethod.SyncToMeDeltaInfo:
                    // _processSyncToMeDeltaInfo(msgPayload);
                    break;
                case (uint)NotifyMethod.SyncNearDeltaInfo:
                    // _processSyncNearDeltaInfo(msgPayload);
                    break;
                default:
                    Console.WriteLine($"Skipping NotifyMsg with methodId {methodId}");
                    break;
            }
        }


        public static void _processSyncNearEntities(byte[] payloadBuffer)
        {
            var syncNearEntities = SyncNearEntities.Parser.ParseFrom(payloadBuffer);

            if (syncNearEntities.Appear == null || syncNearEntities.Appear.Count == 0)
            {
                return;
            }
           

            foreach (var entity in syncNearEntities.Appear)
            {
                Console.WriteLine(entity.EntType);
                Console.WriteLine((int)EEntityType.EntChar);
                // 仅关心“角色（玩家）实体”
                if (entity.EntType != (int)EEntityType.EntChar) continue;

                // 玩家完整 UUID（含类型位），右移 16 位得到 UID
                long playerUuid = entity.Uuid;
                if (playerUuid == 0) continue;
                playerUuid >>= 16;

                var attrCollection = entity.Attrs;
                if (attrCollection == null || attrCollection.Attrs == null) continue;

                // Attr 是“(Id, RawData)”这样的 Key-Value 对
                foreach (var attr in attrCollection.Attrs)
                {
                    if (attr.Id == 0 || attr.RawData == null || attr.RawData.Length == 0) continue;

                    // 用 C# Protobuf 的 CodedInputStream 读取原始 wire 格式
                    var reader = new Google.Protobuf.CodedInputStream(attr.RawData.ToByteArray());

                    switch (attr.Id)
                    {
                        case (int)AttrType.AttrName:
                            {
                                // protobuf string: 先读长度（varint），后读 UTF-8 bytes
                                string playerName = reader.ReadString();
                                //this.userDataManager.setName((long)playerUuid, playerName);
                                Console.WriteLine($"Found player name {playerName} for uuid {playerUuid}");
                                break;
                            }
                        case (int)AttrType.AttrProfessionId:
                            {
                                int professionId = reader.ReadInt32();
                                string professionName = GetProfessionNameFromId(professionId);
                                //this.userDataManager.setProfession((long)playerUuid, professionName);
                                Console.WriteLine($"Found profession {professionName} for uuid {playerUuid}");
                                break;
                            }
                        case (int)AttrType.AttrFightPoint:
                            {
                                int playerFightPoint = reader.ReadInt32();
                                //this.userDataManager.setFightPoint((long)playerUuid, playerFightPoint);
                                Console.WriteLine($"Found player fight point {playerFightPoint} for uuid {playerUuid}");
                                break;
                            }
                        default:
                            // 其他属性先跳过（可按需扩展）
                            break;
                    }
                }
            }

        }
    }



}
