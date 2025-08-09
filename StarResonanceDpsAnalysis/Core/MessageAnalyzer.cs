using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;
using BlueProto;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using PacketDotNet;
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
                        //Console.WriteLine($"Ignore packet with message type {msgTypeId}.");
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
            { 0x0000002EU, ProcessSyncToMeDeltaInfo },

            // NotifyMethod.SyncNearDeltaInfo
            { 0x0000002DU, ProcessSyncNearDeltaInfo },
        };

        public static void ProcessNotifyMsg(ByteReader packet, bool isZstdCompressed)
        {
            var serviceUuid = packet.ReadUInt64BE();
            var stubId = packet.ReadUInt32BE();
            var methodId = packet.ReadUInt32BE();
          
            if (serviceUuid != 0x0000000063335342UL)
            {
               // Console.WriteLine($"Skipping NotifyMsg with serviceId {serviceUuid}");
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
               // Console.WriteLine($"Skipping NotifyMsg with methodId {methodId}");
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

        /// <summary>
        /// 获取人物属性（名字、职业、战力）
        /// </summary>
        /// <param name="payloadBuffer"></param>
        public static void ProcessSyncNearEntities(byte[] payloadBuffer)
        {
       
            var syncNearEntities = SyncNearEntities.Parser.ParseFrom(payloadBuffer);

           

            if (syncNearEntities.Appear == null || syncNearEntities.Appear.Count == 0)
            {
                return;
            }


            foreach (var entity in syncNearEntities.Appear)
            {
            
                // 仅关心“角色（玩家）实体”
                if (entity.EntType != EEntityType.EntChar) continue;

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


        public static void ProcessSyncNearDeltaInfo(byte[] payloadBuffer)
        {
            //var syncNearEntities = SyncNearEntities.Parser.ParseFrom(payloadBuffer);
            var syncNearDeltaInfo = SyncNearDeltaInfo.Parser.ParseFrom(payloadBuffer);


            if (syncNearDeltaInfo.DeltaInfos == null || syncNearDeltaInfo.DeltaInfos.Count == 0)
            {
                return;
            }


            foreach (var aoiSyncDelta in syncNearDeltaInfo.DeltaInfos)
            {
                
                ProcessAoiSyncDelta(aoiSyncDelta);
            }
        }
        /// <summary>
        /// 解析 SyncNearDeltaInfo 并统计玩家的伤害/治疗/被打量。
        /// 关键点：
        /// 1) 全程避免早退 return（用 continue），以免丢后续数据包导致“解析不出来/延迟”
        /// 2) UUID 相关计算全部使用 ulong，并在“未右移的原始值”上判断玩家身份
        /// 3) UI 刷新放在循环外（仍建议你做节流）
        /// </summary>
        public static void ProcessAoiSyncDelta(AoiSyncDelta delta)
        {
          
            // 遍历 AOI 的增量信息
        
                if (delta == null) return; // ← 避免早退，跳过空项

                // 目标 UUID（原始 64 位数值，不做任何位移）
                // 用 ulong 是为了无符号右移 >>16 时不发生算术右移（C# long >> 是算术右移）
                ulong targetUuidRaw = (ulong)delta.Uuid;
                if (targetUuidRaw == 0) return; // ← 跳过无效 UUID

                // 在“未右移的原值”上判断是否为玩家（等价 JS 的 isUuidPlayer）
                bool isTargetPlayer = IsUuidPlayerRaw(targetUuidRaw);

                // 与 JS: shiftRight(16) 对齐 —— 取实体真正 ID（去掉低 16 位类型/分片信息）
                ulong targetUuid = Shr16(targetUuidRaw);

                // 技能效果段判空（Damages 列表为空就跳过）
                var se = delta.SkillEffects;
      

                if (se?.Damages == null || se.Damages.Count == 0) return;

                // 遍历所有伤害/治疗记录
                foreach (var d in se.Damages)
                {
                    // 技能 ID（统计输出用）
                    long skillId = d.OwnerId;
                    if (skillId == 0) continue;

                    // 施法者/伤害来源：优先使用 TopSummonerId（顶层召唤者），否则用 AttackerUuid
                    ulong attackerRaw = (ulong)(d.TopSummonerId != 0 ? d.TopSummonerId : d.AttackerUuid);
                    if (attackerRaw == 0) continue;

                    // 同样在“原始 attackerRaw”上做玩家判断
                    bool isAttackerPlayer = IsUuidPlayerRaw(attackerRaw);

                    // 再把 UUID 无符号右移 16 位，得到最终用于统计的 ID
                    ulong attackerUuid = Shr16(attackerRaw);

                    // 伤害值：优先 Value，其次 LuckyValue（JS 是 value ?? luckyValue ?? 0）
                    long damageSigned = d.HasValue ? d.Value : (d.HasLuckyValue ? d.LuckyValue : 0L);
                    if (damageSigned == 0) continue;

                    // 保险起见转为正的 ulong（有些服务端字段可能出现负数占位）
                    ulong damage = (ulong)(damageSigned < 0 ? -damageSigned : damageSigned);

                    // 暴击判断：JS 用 TypeFlag 的第 1 位（& 1）
                    bool isCrit = d.HasTypeFlag && ((d.TypeFlag & 1L) == 1L);

                    // 是否治疗：直接对齐枚举
                    bool isHeal = d.Type == EDamageType.Heal;

                    // 幸运（JS: !!luckyValue，只要存在就 true）
                    bool isLucky = d.HasLuckyValue;

                    // 血量压制/减益（JS: HpLessenValue?.toNumber()）
                    ulong hpLessen = d.HasHpLessenValue ? (ulong)d.HpLessenValue : 0UL;

                    if (isTargetPlayer)
                    {
                        // 目标是玩家
                        if (isHeal)
                        {
                            // “玩家被治疗”场景下，只记录“玩家造成的治疗”（奶妈是玩家时才记）
                            if (isAttackerPlayer)
                            {
                                var attacker = StatisticData._manager.GetOrCreate(attackerUuid);
                                attacker.AddHealing(damage, isCrit, isLucky);
                            }
                        }
                        else
                        {
                     
                            // 玩家受到伤害 → 统计“被打量”
                            var victim = StatisticData._manager.GetOrCreate(targetUuid);

                            // NOTE: JS 的 addTakenDamage 只传伤害值。如果你的 C# 方法签名是 (ulong damage)，
                            // 请改为：victim.AddTakenDamage(damage);
                            // 目前按你代码保留 (skillId, damage)，确保签名匹配你自己的实现。
                            victim.AddTakenDamage((ulong)skillId, damage);
                        }
                    }
                    else
                    {
                        // 目标不是玩家
                        if (!isHeal && isAttackerPlayer)
                        {
                            // 只记录“玩家造成的输出伤害”，治疗对非玩家一般不计
                            var attacker = StatisticData._manager.GetOrCreate(attackerUuid);
                            attacker.AddDamage((ulong)skillId, damage, isCrit, isLucky, hpLessen);
                        }
                    }
                }
            

            // UI 刷新放在循环外，避免高频阻塞消息处理线程
            // NOTE: 强烈建议节流，例如 100–200ms 合并一次刷新（BeginInvoke + 计时器）
            MainForm.RefreshDpsTable();
        }
        public static void ProcessSyncToMeDeltaInfo(byte[] payloadBuffer)
        {
            // 1) 反序列化：把网络收到的一段二进制，解成 SyncToMeDeltaInfo（“与我相关”的增量同步包）
            var syncToMeDeltaInfo = SyncToMeDeltaInfo.Parser.ParseFrom(payloadBuffer);
            
            // 2) 取出里面的 AoiSyncToMeDelta（包含：我的Uuid、BaseDelta、以及可选的技能CD/资源CD等）
            var aoiSyncToMeDelta = syncToMeDeltaInfo.DeltaInfo;

            // 3) 我的实体 Uuid（64 位）。常用于：
            //    - 记录/缓存“当前玩家”的 Uuid，便于后续做归属、过滤（例如把与我无关的事件忽略）
            //    - 可按项目规则右移16位得到“短ID”用于显示或作为字典Key
            long uuid = aoiSyncToMeDelta.Uuid; // ← 这里取UID是用于做缓存？是的，通常会缓存当前玩家Uuid。

            // 4) BaseDelta 是一条通用的 AoiSyncDelta，里面才有战斗/治疗/BUFF 等具体增量数据
            var aoiSyncDelta = aoiSyncToMeDelta.BaseDelta;
            if (aoiSyncDelta == null) return;  // 没有具体增量就直接返回

            // 5) 交给统一的增量处理逻辑：在这里面解析 SkillEffects.Damages（伤害/治疗）、
            //    BuffInfos/BuffEffect（增益变更）、EventDataList 等，并更新你的统计表
            ProcessAoiSyncDelta(aoiSyncDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsUuidPlayerRaw(ulong uuidRaw) => (uuidRaw & 0xFFFFUL) == 640UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Shr16(ulong v) => v >> 16;

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
