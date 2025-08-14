using System;
using System.Runtime.CompilerServices;
using System.Text;

using BlueProto;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using ZstdNet;

namespace StarResonanceDpsAnalysis.Core
{
    public class MessageAnalyzer
    {
        private static readonly Dictionary<int, Action<ByteReader, bool>> MessageHandlers = new()
        {
            //{1, }// RPC 请求
            // MessageType.Notify
            { 2, ProcessNotifyMsg },

            // MessageType.Return
            //{ 3, ProcessReturnMsg }, // 目前不处理

            //{4,} // 心跳/回显
            //{5, }// 客户端->服务器帧
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

                    if (packetSize > packetsReader.Remaining)
                    {
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
            catch (EndOfStreamException ex)
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
            AttrName = 0x01,               // 玩家名字（string）
            AttrProfessionId = 0xDC,       // 职业 ID（int32）
            AttrFightPoint = 0x272E,       // 战力（int32）
            AttrLevel = 0x2710,            // 等级（int32）
            AttrRankLevel = 0x274C,        // 段位/阶位（int32）
            AttrCri = 0x2B66,               // 暴击（int32）
            AttrLucky = 0x2B7A,             // 幸运（int32）
            AttrHp = 0x2C2E,                // 当前 HP（int32）
            AttrMaxHp = 0x2C38,             // 最大 HP（int32）
            AttrElementFlag = 0x646D6C,     // 元素标记（int32）
            AttrReductionLevel = 0x64696D,  // 减伤等级（int32）
            AttrReduntionId = 0x6F6C65,     // 减免 Id（int32，拼写看似服务端内部命名）
            AttrEnergyFlag = 0x543CD3C6     // 能量标志（int32）
        }

        private static Dictionary<uint, Action<byte[]>> ProcessMethods = new()
        {
            // NotifyMethod.SyncNearEntities
            { 0x00000006U, ProcessSyncNearEntities },//// 附近实体出现/更新（AOI 进入）

            //进场景的时候推的大包
            {0x00000015U,ProcessSyncContainerData }, // 容器（玩家）完整数据（如登录后下发）
            //在场景内有变化时候推的包
            {0x00000016U,ProcessSyncContainerDirtyData },// 容器（玩家）增量更新（Dirty）

            // NotifyMethod.SyncToMeDeltaInfo
            { 0x0000002EU, ProcessSyncToMeDeltaInfo },//// 同步给“我”的 AOI 增量（含自身 UUID）

            // NotifyMethod.SyncNearDeltaInfo
            { 0x0000002DU, ProcessSyncNearDeltaInfo },//// AOI 增量（附近实体状态变化：伤害/治疗等）
        };

        public static void ProcessNotifyMsg(ByteReader packet, bool isZstdCompressed)
        {
            var serviceUuid = packet.ReadUInt64BE();
            var stubId = packet.ReadUInt32BE();
            var methodId = packet.ReadUInt32BE();

            if (serviceUuid != 0x0000000063335342UL)
            {
                //Console.WriteLine($"Skipping NotifyMsg with serviceId {serviceUuid}");
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
        private static readonly uint ZSTD_MAGIC = 0xFD2FB528;          // 小端: 28 B5 2F FD
        private static readonly uint SKIPPABLE_MAGIC_MIN = 0x184D2A50; // 小端: 50 2A 4D 18
        private static readonly uint SKIPPABLE_MAGIC_MAX = 0x184D2A5F;
        private static byte[] DecompressZstdIfNeeded(byte[] buffer)
        {
            //using var input = new MemoryStream(body);
            //using var z = new DecompressionStream(input);
            //using var output = new MemoryStream();

            //z.CopyTo(output);

            //return output.ToArray();
            if (buffer == null || buffer.Length < 4) return Array.Empty<byte>();

            int off = 0;

            // 1) 找到真正的 zstd 帧开头，同时跳过 skippable 帧
            while (off + 4 <= buffer.Length)
            {
                uint magic = BitConverter.ToUInt32(buffer, off);
                if (magic == ZSTD_MAGIC) break;

                if (magic >= SKIPPABLE_MAGIC_MIN && magic <= SKIPPABLE_MAGIC_MAX)
                {
                    if (off + 8 > buffer.Length) throw new InvalidDataException("Incomplete skippable frame header");
                    uint size = BitConverter.ToUInt32(buffer, off + 4);
                    if (off + 8 + size > buffer.Length) throw new InvalidDataException("Incomplete skippable frame payload");
                    off += 8 + (int)size;
                    continue;
                }

                off++; // 继续扫描
            }

            if (off + 4 > buffer.Length)
                return buffer; // 没有 zstd 魔数，当作未压缩返回

            // 2) 用解码流读取“第一帧”，不要传不存在的 MaxFrameSize
            using var input = new MemoryStream(buffer, off, buffer.Length - off, writable: false);
            using var decoder = new DecompressionStream(input); // <- 不要 DecompressionOptions.MaxFrameSize
            using var output = new MemoryStream();

            // 可选：手动限制最大解压大小（例如 32MB）
            const long MAX_OUT = 32L * 1024 * 1024;
            var temp = new byte[8192];
            long total = 0;
            int read;
            while ((read = decoder.Read(temp, 0, temp.Length)) > 0)
            {
                total += read;
                if (total > MAX_OUT)
                    throw new InvalidDataException("Zstd frame exceeds safety limit");
                output.Write(temp, 0, read);
            }

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
                        //昵称
                        case (int)AttrType.AttrName:
                            // protobuf string: 先读长度（varint），后读 UTF-8 bytes
                            string playerName = reader.ReadString();
                            StatisticData._manager.SetNickname((ulong)playerUuid, playerName);

                            //this.userDataManager.setName((long)playerUuid, playerName);
                            //Console.WriteLine($"昵称： {playerName}UID：{playerUuid}");
                            break;

                        //职业
                        case (int)AttrType.AttrProfessionId:
                            int professionId = reader.ReadInt32();
                            string professionName = GetProfessionNameFromId(professionId);
                            //this.userDataManager.setProfession((long)playerUuid, professionName);
                            StatisticData._manager.SetProfession((ulong)playerUuid, professionName);

                            //Console.WriteLine($"职业ID：{professionId}职业： {professionName} UID： {playerUuid}");
                            break;
                            //战力
                        case (int)AttrType.AttrFightPoint:
                            int playerFightPoint = reader.ReadInt32();
                            //this.userDataManager.setFightPoint((long)playerUuid, playerFightPoint);
                            StatisticData._manager.SetCombatPower((ulong)playerUuid, playerFightPoint);
                            //Console.WriteLine($"战力： {playerFightPoint} UID：{playerUuid}");
                            break;
                            //等级
                        case (int)AttrType.AttrLevel:
                            int playerLevel = reader.ReadInt32();
                            StatisticData._manager.SetAttrKV((ulong)playerUuid, "level", playerLevel);
                            //Console.WriteLine($"等级： {playerLevel} UID：{playerUuid}");
                            break;
                        case (int)AttrType.AttrRankLevel:
                            // 段位/阶位
                            int playerRankLevel = reader.ReadInt32();
                            StatisticData._manager.SetAttrKV((ulong)playerUuid, "rank_level", playerRankLevel);
                            //Console.WriteLine($"段位/阶位： {playerRankLevel} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrCri:
                            // 暴击
                            int playerCri = reader.ReadInt32();
                            StatisticData._manager.SetAttrKV((ulong)playerUuid, "cri", playerCri);
                            //Console.WriteLine($"暴击： {playerCri} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrLucky:
                            // 幸运
                            int playerLucky = reader.ReadInt32();
                            StatisticData._manager.SetAttrKV((ulong)playerUuid, "lucky", playerLucky);
                            //Console.WriteLine($"幸运： {playerLucky} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrHp:
                            // 当前 HP
                            int playerHp = reader.ReadInt32();
                            StatisticData._manager.SetAttrKV((ulong)playerUuid, "hp", playerHp);
                            //Console.WriteLine($"当前 HP： {playerHp} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrMaxHp:
                            // 最大 HP
                            int playerMaxHp = reader.ReadInt32();
                            //StatisticData._manager.SetAttrKV((ulong)playerUuid, "max_hp", playerMaxHp);
                            //Console.WriteLine($"最大 HP： {playerMaxHp} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrElementFlag:
                            // 元素标记（火/冰/雷等）
                            int playerElementFlag = reader.ReadInt32();
                            //StatisticData._manager.SetAttrKV((ulong)playerUuid, "element_flag", playerElementFlag);
                            //Console.WriteLine($"元素标记： {playerElementFlag} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrEnergyFlag:
                            // 能量标志
                            int playerEnergyFlag = reader.ReadInt32();
                            //StatisticData._manager.SetAttrKV((ulong)playerUuid, "energy_flag", playerEnergyFlag);
                            //Console.WriteLine($"能量标志： {playerEnergyFlag} UID：{playerUuid}");
                            break;

                        case (int)AttrType.AttrReductionLevel:
                            // 减伤等级
                            int playerReductionLevel = reader.ReadInt32();
                            //StatisticData._manager.SetAttrKV((ulong)playerUuid, "reduction_level", playerReductionLevel);
                            //Console.WriteLine($"减伤等级： {playerReductionLevel} UID：{playerUuid}");
                            break;

                        default:
                            // 其他属性先跳过（可按需扩展）
                            // Console.WriteLine($"昵称： {reader.ReadString()}UID：{playerUuid}");
                            break;
                    }
                }
            }

        }


        /// <summary>
        ///  获取战斗信息DPS
        /// </summary>
        /// <param name="payloadBuffer"></param>
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
                //bool isCrit = d.HasTypeFlag && ((d.TypeFlag & 1L) == 1L);
                bool isCrit = d.TypeFlag != null
                  ? ((d.TypeFlag & 1) == 1)
                  : false;

                // 是否治疗：直接对齐枚举
                bool isHeal = d.Type == EDamageType.Heal;

                // 幸运（JS: !!luckyValue，只要存在就 true）

                var luckyValue = d.LuckyValue;
                // JS 中 "!!" 是把值转成布尔，这里相当于判断 luckyValue 是否非空且非 0
                bool isLucky = luckyValue != null && luckyValue != 0;


                // 血量压制/减益（JS: HpLessenValue?.toNumber()）
                ulong hpLessen = d.HasHpLessenValue ? (ulong)d.HpLessenValue : 0UL;
               
                //如果开启了打桩模式，且攻击者不是自己，且目标不是最右侧的木桩则跳过
                if(AppConfig.PilingMode)
                {
                    if ( attackerUuid != AppConfig.Uid)
                    {
                        continue;
                    }
                    else
                    {
                        if (targetUuid != 75)
                        {
                            continue;
                        }
                    }
                }
               


                if (isTargetPlayer)
                {
                    // 目标是玩家
                    if (isHeal)
                    {
                        // “玩家被治疗”场景下，只记录“玩家造成的治疗”（奶妈是玩家时才记）
                        if (isAttackerPlayer)
                        {

                            StatisticData._manager.AddHealing(attackerUuid, (ulong)skillId, hpLessen, isCrit, isLucky);
                        }
                    }
                    else
                    {



                        StatisticData._manager.AddTakenDamage(targetUuid, (ulong)skillId, damage,isCrit,isLucky,hpLessen);
                    }

                }
                else
                {
                    // 目标不是玩家
                    if (!isHeal && isAttackerPlayer)
                    {
                        // 只记录“玩家造成的输出伤害”，治疗对非玩家一般不计
                        //Console.WriteLine(@$"玩家{attackerUuid} 使用技能{skillId}对非玩家{targetUuid}造成伤害{damage}");
                        //最右侧木桩ID为75
                        StatisticData._manager.AddDamage(attackerUuid, (ulong)skillId, damage, isCrit, isLucky, hpLessen);

                    }
                    if (AppConfig.NpcsTakeDamage)
                    {
                        //添加怪物承伤记录
                        StatisticData._manager.AddTakenDamage(targetUuid, (ulong)skillId, damage,isCrit,isLucky,hpLessen);

                        
                        Console.WriteLine(@$"怪物ID：{targetUuid}受到伤害{damage},来自{attackerUuid}的技能{skillId}");
                    }


                }
            }


            // UI 刷新放在循环外，避免高频阻塞消息处理线程
            // NOTE: 强烈建议节流，例如 100–200ms 合并一次刷新（BeginInvoke + 计时器）
           // MainForm.RefreshDpsTable();
        }
        public static long currentUserUuid = 0;
        //获取承伤信息DPS
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
            if (uuid != 0 && currentUserUuid != uuid)
            {
                currentUserUuid = uuid;
                Console.WriteLine($"Got player UUID! UUID: {currentUserUuid} UID: {currentUserUuid >> 16}");
            }

            // 4) BaseDelta 是一条通用的 AoiSyncDelta，里面才有战斗/治疗/BUFF 等具体增量数据
            var aoiSyncDelta = aoiSyncToMeDelta.BaseDelta;
            if (aoiSyncDelta == null) return;  // 没有具体增量就直接返回

            // 5) 交给统一的增量处理逻辑：在这里面解析 SkillEffects.Damages（伤害/治疗）、
            //    BuffInfos/BuffEffect（增益变更）、EventDataList 等，并更新你的统计表
            ProcessAoiSyncDelta(aoiSyncDelta);
        }

        public static void ProcessSyncContainerData(byte[] payloadBuffer)
        {
            // 解析 protobuf 数据，得到 SyncContainerData 对象
            var syncContainerData = SyncContainerData.Parser.ParseFrom(payloadBuffer);

            // 如果 VData 不存在，直接结束
            if (syncContainerData == null || syncContainerData.VData == null)
                return;

            var vData = syncContainerData.VData;

            // 如果没有 CharId（玩家 UID），直接结束
            if (vData.CharId==null|| vData.CharId==0)
                return;

            // 玩家 UID
            ulong playerUid = vData.CharId;


            // 如果存在角色等级数据
            if (vData.RoleLevel != null && vData.RoleLevel.Level != 0)
            {
                StatisticData._manager.SetAttrKV(playerUid, "level", vData.RoleLevel.Level);

                //Console.WriteLine($"[解析] 玩家 {playerUid} 等级: {vData.RoleLevel.Level}");
                
            }

            // 如果存在当前 HP
            if (vData.Attr != null && vData.Attr.CurHp != 0)
            {
                StatisticData._manager.SetAttrKV(playerUid, "hp", vData.RoleLevel.Level);
                //Console.WriteLine($"[解析] 玩家 {playerUid} 当前HP: {vData.Attr.CurHp}");
       
            }

            // 如果存在最大 HP
            if (vData.Attr != null && vData.Attr.MaxHp != 0)
            {
                StatisticData._manager.SetAttrKV(playerUid, "max_hp", vData.RoleLevel.Level);
                //Console.WriteLine($"[解析] 玩家 {playerUid} 最大HP: {vData.Attr.MaxHp}");
              
            }

            // 如果没有 CharBase（角色基础信息），直接结束
            if (vData.CharBase == null)
                return;

            var charBase = vData.CharBase;

            // 如果有角色名字
            if (!string.IsNullOrEmpty(charBase.Name))
            {
                StatisticData._manager.SetNickname(playerUid, charBase.Name);
                AppConfig.NickName = charBase.Name;
                //Console.WriteLine($"[解析] 玩家 {playerUid} 名字: {charBase.Name}");

            }

            // 如果有战力值
            if (charBase.FightPoint != 0)
            {
                StatisticData._manager.SetCombatPower(playerUid, charBase.FightPoint);
                //Console.WriteLine($"[解析] 玩家 {playerUid} 战力值: {charBase.FightPoint}");
                AppConfig.CombatPower = charBase.FightPoint;
            }

            // 如果没有职业信息列表，直接结束
            if (vData.ProfessionList == null)
                return;

            var professionList = vData.ProfessionList;

            // 如果有当前职业 ID
            if (professionList.CurProfessionId != 0)
            {
                var professionName = GetProfessionNameFromId(professionList.CurProfessionId);
                AppConfig.Profession = professionName;
                //Console.WriteLine($"[解析] 玩家 {playerUid} 职业ID: {professionList.CurProfessionId} => 职业名: {professionName}");

            }
        }

        public static void ProcessSyncContainerDirtyData(byte[] payloadBuffer)
        {
            try
            {
                // 1) 必须先拿到当前玩家 UUID；否则无法归属到具体玩家
                if (currentUserUuid == 0) return;

                // 2) decode 脏数据
                var dirty = SyncContainerDirtyData.Parser.ParseFrom(payloadBuffer);
                if (dirty == null || dirty.VData == null || dirty.VData.BufferS == null || dirty.VData.BufferS.Length == 0)
                    return;

                var buf = dirty.VData.BufferS.ToByteArray();

                // 可选调试：输出十六进制
                // Console.WriteLine(BitConverter.ToString(buf).Replace("-", ""));

                using var ms = new MemoryStream(buf, writable: false);
                using var br = new BinaryReader(ms); // BinaryReader 默认小端 LE

                // 3) 顶层先读“标识头”（JS: doesStreamHaveIdentifier）
                if (!DoesStreamHaveIdentifier(br)) return;

                // 读取顶层字段索引（LE UInt32 + 4 字节对齐，JS: readUInt32LE + readInt32）
                uint fieldIndex = br.ReadUInt32(); 
                _ = br.ReadInt32(); // 对齐

                // 归属用 UID（右移 16）
                ulong playerUid = (ulong)currentUserUuid >> 16;

                switch (fieldIndex)
                {
                    // ===== CharBase（名字/战力） =====
                    case 2:
                    {
                        if (!DoesStreamHaveIdentifier(br)) break;

                        fieldIndex = br.ReadUInt32();
                        _ = br.ReadInt32(); // 对齐

                        switch (fieldIndex)
                        {
                            case 5: // Name (string)
                            {
                                string playerName = StreamReadString(br);
                                if (!string.IsNullOrEmpty(playerName))
                                {
                                    StatisticData._manager.SetNickname(playerUid, playerName);
                                    AppConfig.NickName = playerName;
                                    Console.WriteLine($"[Dirty] 名字: {playerName}, UID: {playerUid}");
                                }
                                break;
                            }

                            case 35: // FightPoint (uint32)
                            {
                                uint fightPoint = br.ReadUInt32();
                                _ = br.ReadInt32(); // 对齐
                                if (fightPoint != 0)
                                {
                                    StatisticData._manager.SetCombatPower(playerUid, (int)fightPoint);
                                    AppConfig.CombatPower = (int)fightPoint;
                                    Console.WriteLine($"[Dirty] 战力: {fightPoint}, UID: {playerUid}");
                                }
                                break;
                            }

                            default:
                                // 未处理的 CharBase 子字段
                                break;
                        }
                        break;
                    }

                    // ===== UserFightAttr（CurHp/MaxHp）=====
                    case 16:
                    {
                        if (!DoesStreamHaveIdentifier(br)) break;

                        fieldIndex = br.ReadUInt32();
                        _ = br.ReadInt32(); // 对齐

                        switch (fieldIndex)
                        {
                            case 1: // CurHp
                            {
                                uint curHp = br.ReadUInt32();
                                StatisticData._manager.SetAttrKV(playerUid, "hp", (int)curHp);
                                Console.WriteLine($"[Dirty] 当前HP: {curHp}, UID: {playerUid}");
                                break;
                            }
                            case 2: // MaxHp
                            {
                                uint maxHp = br.ReadUInt32();
                                StatisticData._manager.SetAttrKV(playerUid, "max_hp", (int)maxHp);
                                Console.WriteLine($"[Dirty] 最大HP: {maxHp}, UID: {playerUid}");
                                break;
                            }
                            default:
                                // 未处理的 UserFightAttr 子字段
                                break;
                        }
                        break;
                    }

                    // ===== ProfessionList（CurProfessionId）=====
                    case 61:
                    {
                        if (!DoesStreamHaveIdentifier(br)) break;

                        fieldIndex = br.ReadUInt32();
                        _ = br.ReadInt32(); // 对齐

                        switch (fieldIndex)
                        {
                            case 1: // CurProfessionId
                            {
                                uint curProfessionId = br.ReadUInt32();
                                _ = br.ReadInt32(); // 对齐
                                if (curProfessionId != 0)
                                {
                                    var professionName = GetProfessionNameFromId((int)curProfessionId);
                                    AppConfig.Profession = professionName;
                                    // 如果需要也写入到 StatisticData：
                                    StatisticData._manager.SetProfession(playerUid, professionName);
                                    Console.WriteLine($"[Dirty] 职业ID: {curProfessionId} => {professionName}, UID: {playerUid}");
                                }
                                break;
                            }
                            default:
                                // 未处理的 ProfessionList 子字段
                                break;
                        }
                        break;
                    }

                    default:
                        // 未处理的顶层字段
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                // 数据不完整时的保护
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// JS: doesStreamHaveIdentifier(reader)
        /// 语义：检查并“吃掉”一段标识头。这里按 8 字节跳过（具体值不校验，只要有足够数据）。
        /// </summary>
        private static bool DoesStreamHaveIdentifier(BinaryReader br)
        {
            var s = br.BaseStream;
            if (s.Position + 8 > s.Length) return false;
            _ = br.ReadUInt64(); // 跳过标识
            return true;
        }

        /// <summary>
        /// JS: streamReadString(reader)
        /// 语义：先读一个 LE UInt32 作为长度，然后读该长度的 UTF-8 字节；最后做 4 字节对齐填充跳过。
        /// </summary>
        private static string StreamReadString(BinaryReader br)
        {
            uint len = br.ReadUInt32(); // 字符串长度（LE）
            if (len == 0) return string.Empty;

            var bytes = br.ReadBytes((int)len);
            // 4 字节对齐（padding 0~3）
            int pad = (int)((4 - (len % 4)) % 4);
            if (pad > 0) _ = br.ReadBytes(pad);

            return Encoding.UTF8.GetString(bytes);
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
                12 => "神盾骑士",
                13 => "灵魂乐手",
                _ => string.Empty,// 未知职业
            };
        }



    }
}
