using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blue;
using ZstdNet;

namespace StarResonanceDpsAnalysis.Core
{
    public class MessageAnalyzer
    {
        private static readonly Dictionary<int, Action<ByteReader, bool>> MessageHandlers = new()
        {
            // MessageType.Notify
            { 2, ProcessNotifyMsg },

            // MessageType.Return
            //{ 3, ProcessReturnMsg }, // 目前不处理

            // MessageType.FrameDown
            { 6, ProcessFrameDown }
        };

        public static void Process(byte[] packets)
        {
            try
            {
                // 外层：处理一次输入里可能包含的多个完整包
                var packetsReader = new ByteReader(packets);

                while (packetsReader.Remaining > 0) // 至少读得到一个 packetSize
                {
                    // —— 预读长度，不前进指针 —— 
                    if (!packetsReader.TryPeekUInt32BE(out uint packetSize)) break;

                    // 合法包最短：size(4) + type(2) = 6
                    if (packetSize < 6)
                    {
                        Console.WriteLine($"Received invalid packet size (<6): {packetSize}. Drop the rest.");
                        return; // 丢掉剩余，避免死循环
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

                    var packetType = packetReader.ReadUInt16BE();
                    var isZstdCompressed = (packetType & 0x8000) != 0; // bit15
                    var msgTypeId = packetType & 0x7FFF;                // 低15位

                    // 调试输出（按需保留）
                    //Console.WriteLine($"MsgType={msgTypeId}, Size={packetSize}, RemainInPacket={packetReader.Remaining}");

                    var flag = MessageHandlers.TryGetValue(msgTypeId, out var handler);
                    if (!flag)
                    {
                        Console.WriteLine($"Ignore packet with message type {msgTypeId}.");
                        return;
                    }

                    handler!(packetReader, isZstdCompressed);
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
            SyncNearEntities = 0x00000006U,
            SyncNearDeltaInfo = 0x0000002dU,
            SyncToMeDeltaInfo = 0x0000002eU
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

        private static Dictionary<uint, Action<byte[]>> ProcessMethods = new()
        {
            // NotifyMethod.SyncNearEntities
            { 0x00000006U, ProcessSyncNearEntities },

            // NotifyMethod.SyncToMeDeltaInfo
            //{ 0x0000002eU, ProcessSyncToMeDeltaInfo },

            // NotifyMethod.SyncNearDeltaInfo
            //{ 0x0000002dU, ProcessSyncNearDeltaInfo },
        };

        public static void ProcessNotifyMsg(ByteReader packet, bool isZstdCompressed)
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
            //protoBody = DecompressZstdIfNeeded(protoBody);

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
            //string result = ProtoPrettyPrinter.PrettyPrintProto(BitConverter.ToString(protoBody));

            var flag = ProcessMethods.TryGetValue(methodId, out var processMethod);
            if (!flag)
            {
                Console.WriteLine($"Skipping NotifyMsg with methodId {methodId}");
                return;
            }

            processMethod!(msgPayload);
        }

        public static void ProcessFrameDown(ByteReader reader, bool isZstdCompressed)
        {
            // 仅读取，不使用
            var serverSequenceId = reader.ReadUInt32BE();

            if (reader.Remaining != 0)
            {

                // 剩余即为嵌套包（通常自身也是“完整下行包”：size + type + body）
                var nestedPacket = reader.ReadRemaining();

                if (isZstdCompressed)
                {
                    nestedPacket = DecompressZstdIfNeeded(nestedPacket);
                }

                // 递归处理内部包
                Process(nestedPacket);
            }
        }

        private static byte[] DecompressZstdIfNeeded(byte[] body)
        {
            using var input = new MemoryStream(body);
            using var z = new DecompressionStream(input);
            using var output = new MemoryStream();

            z.CopyTo(output);

            return output.ToArray();
        }

        public static void ProcessSyncNearEntities(byte[] payloadBuffer)
        {
            var syncNearEntities = SyncNearEntities.Parser.ParseFrom(payloadBuffer);
            //var syncNearEntities = SyncNearEntitiesManual.Decode(payloadBuffer);

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
                            // protobuf string: 先读长度（varint），后读 UTF-8 bytes
                            string playerName = reader.ReadString();
                            //this.userDataManager.setName((long)playerUuid, playerName);
                            Console.WriteLine($"Found player name {playerName} for uuid {playerUuid}");
                            break;

                        case (int)AttrType.AttrProfessionId:
                            int professionId = reader.ReadInt32();
                            string professionName = GetProfessionNameFromId(professionId);
                            //this.userDataManager.setProfession((long)playerUuid, professionName);
                            Console.WriteLine($"Found profession {professionName} for uuid {playerUuid}");
                            break;

                        case (int)AttrType.AttrFightPoint:
                            int playerFightPoint = reader.ReadInt32();
                            //this.userDataManager.setFightPoint((long)playerUuid, playerFightPoint);
                            Console.WriteLine($"Found player fight point {playerFightPoint} for uuid {playerUuid}");
                            break;

                        default:
                            // 其他属性先跳过（可按需扩展）
                            break;
                    }
                }
            }

        }

        public static string GetProfessionNameFromId(int professionId)
        {
            return professionId switch
            {
                1 => "雷影剑士",
                2 => "冰魔导师",
                3 => "涤罪恶火·战斧",
                4 => "青岚骑士",
                5 => "森语者",
                6 => "雷霆一闪·手炮",
                7 => "巨刃守护者",
                8 => "暗灵祈舞·仪刀/仪仗",
                9 => "神射手",
                10 => "神盾骑士",
                11 => "灵魂乐手",
                _ => string.Empty,// 未知职业
            };
        }



    }
}
