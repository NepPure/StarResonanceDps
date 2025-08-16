using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BlueProto;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using ZstdNet;
using StarResonanceDpsAnalysis.Plugin.Database; // 数据库同步

namespace StarResonanceDpsAnalysis.Core
{
    public class MessageAnalyzer
    {
        private static readonly Dictionary<int, Action<ByteReader, bool>> MessageHandlers = new()
        {
            { 2, ProcessNotifyMsg },   // MessageType.Notify
            { 6, ProcessFrameDown }    // MessageType.FrameDown
        };

        public static void Process(byte[] packets)
        {
            try
            {
                var packetsReader = new ByteReader(packets);
                while (packetsReader.Remaining > 0)
                {
                    if (!packetsReader.TryPeekUInt32BE(out uint packetSize)) break;
                    if (packetSize < 6) break;
                    if (packetSize > packetsReader.Remaining) break;

                    var packetReader = new ByteReader(packetsReader.ReadBytes((int)packetSize));
                    uint sizeAgain = packetReader.ReadUInt32BE();
                    if (sizeAgain != packetSize) continue;

                    var packetType = packetReader.ReadUInt16BE();
                    var isZstdCompressed = (packetType & 0x8000) != 0; // bit15
                    var msgTypeId = packetType & 0x7FFF;                // 低15位

                    if (!MessageHandlers.TryGetValue(msgTypeId, out var handler))
                    {
                        // 未识别类型：跳过本包，继续后续解析
                        continue;
                    }
                    handler(packetReader, isZstdCompressed);
                }
            }
            catch { }
        }

        public enum AttrType
        {
            AttrName = 0x01,
            AttrProfessionId = 0xDC,
            AttrFightPoint = 0x272E,
            AttrLevel = 0x2710,
            AttrRankLevel = 0x274C,
            AttrCri = 0x2B66,
            AttrLucky = 0x2B7A,
            AttrHp = 0x2C2E,
            AttrMaxHp = 0x2C38,
            AttrElementFlag = 0x646D6C,
            AttrReductionLevel = 0x64696D,
            AttrReduntionId = 0x6F6C65,
            AttrEnergyFlag = 0x543CD3C6
        }

        private static readonly Dictionary<uint, Action<byte[]>> ProcessMethods = new()
        {
            { 0x00000006U, ProcessSyncNearEntities },        // SyncNearEntities
            { 0x00000015U, ProcessSyncContainerData },       // SyncContainerData
            { 0x00000016U, ProcessSyncContainerDirtyData },  // SyncContainerDirtyData
            { 0x0000002EU, ProcessSyncToMeDeltaInfo },       // SyncToMeDeltaInfo
            { 0x0000002DU, ProcessSyncNearDeltaInfo }        // SyncNearDeltaInfo
        };

        public static void ProcessNotifyMsg(ByteReader packet, bool isZstdCompressed)
        {
            var serviceUuid = packet.ReadUInt64BE();
            _ = packet.ReadUInt32BE(); // stubId
            var methodId = packet.ReadUInt32BE();
            if (serviceUuid != 0x0000000063335342UL) return;

            byte[] msgPayload = packet.ReadRemaining();
            if (isZstdCompressed) msgPayload = DecompressZstdIfNeeded(msgPayload);

            if (!ProcessMethods.TryGetValue(methodId, out var processMethod)) return;
            processMethod(msgPayload);
        }

        public static void ProcessFrameDown(ByteReader reader, bool isZstdCompressed)
        {
            _ = reader.ReadUInt32BE(); // serverSequenceId
            if (reader.Remaining == 0) return;
            var nestedPacket = reader.ReadRemaining();
            if (isZstdCompressed) nestedPacket = DecompressZstdIfNeeded(nestedPacket);
            Process(nestedPacket);
        }

        private static readonly uint ZSTD_MAGIC = 0xFD2FB528;
        private static readonly uint SKIPPABLE_MAGIC_MIN = 0x184D2A50;
        private static readonly uint SKIPPABLE_MAGIC_MAX = 0x184D2A5F;
        private static byte[] DecompressZstdIfNeeded(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 4) return Array.Empty<byte>();
            int off = 0;
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
                off++;
            }
            if (off + 4 > buffer.Length) return buffer;

            using var input = new MemoryStream(buffer, off, buffer.Length - off, writable: false);
            using var decoder = new DecompressionStream(input);
            using var output = new MemoryStream();

            const long MAX_OUT = 32L * 1024 * 1024;
            var temp = new byte[8192];
            long total = 0;
            int read;
            while ((read = decoder.Read(temp, 0, temp.Length)) > 0)
            {
                total += read;
                if (total > MAX_OUT) throw new InvalidDataException("Zstd frame exceeds safety limit");
                output.Write(temp, 0, read);
            }
            return output.ToArray();
        }

        public static void ProcessSyncNearEntities(byte[] payloadBuffer)
        {
            var syncNearEntities = SyncNearEntities.Parser.ParseFrom(payloadBuffer);
            if (syncNearEntities.Appear == null || syncNearEntities.Appear.Count == 0) return;

            foreach (var entity in syncNearEntities.Appear)
            {
                if (entity.EntType != EEntityType.EntChar) continue;
                ulong playerUid = Shr16((ulong)entity.Uuid);
                if (playerUid == 0) continue;

                PlayerDbSyncService.TryFillFromDbOnce(playerUid);

                var attrCollection = entity.Attrs;
                if (attrCollection?.Attrs == null) continue;

                bool updated = false;
                foreach (var attr in attrCollection.Attrs)
                {
                    if (attr.Id == 0 || attr.RawData == null || attr.RawData.Length == 0) continue;
                    var reader = new Google.Protobuf.CodedInputStream(attr.RawData.ToByteArray());

                    switch (attr.Id)
                    {
                        case (int)AttrType.AttrName:
                            StatisticData._manager.SetNickname(playerUid, reader.ReadString());
                            updated = true;
                            break;
                        case (int)AttrType.AttrProfessionId:
                            StatisticData._manager.SetProfession(playerUid, GetProfessionNameFromId(reader.ReadInt32()));
                            updated = true;
                            break;
                        case (int)AttrType.AttrFightPoint:
                            StatisticData._manager.SetCombatPower(playerUid, reader.ReadInt32());
                            updated = true;
                            break;
                        case (int)AttrType.AttrLevel:
                            StatisticData._manager.SetAttrKV(playerUid, "level", reader.ReadInt32());
                            break;
                        case (int)AttrType.AttrRankLevel:
                            StatisticData._manager.SetAttrKV(playerUid, "rank_level", reader.ReadInt32());
                            break;
                        case (int)AttrType.AttrCri:
                            StatisticData._manager.SetAttrKV(playerUid, "cri", reader.ReadInt32());
                            break;
                        case (int)AttrType.AttrLucky:
                            StatisticData._manager.SetAttrKV(playerUid, "lucky", reader.ReadInt32());
                            break;
                        case (int)AttrType.AttrHp:
                            StatisticData._manager.SetAttrKV(playerUid, "hp", reader.ReadInt32());
                            break;
                        case (int)AttrType.AttrMaxHp:
                            _ = reader.ReadInt32();
                            break;
                        case (int)AttrType.AttrElementFlag:
                            _ = reader.ReadInt32();
                            break;
                        case (int)AttrType.AttrEnergyFlag:
                            _ = reader.ReadInt32();
                            break;
                        case (int)AttrType.AttrReductionLevel:
                            _ = reader.ReadInt32();
                            break;
                        default:
                            break;
                    }
                }
                if (updated) Task.Run(() => PlayerDbSyncService.UpsertCurrentAsync(playerUid));
            }
        }

        public static void ProcessSyncNearDeltaInfo(byte[] payloadBuffer)
        {
            var syncNearDeltaInfo = SyncNearDeltaInfo.Parser.ParseFrom(payloadBuffer);
            if (syncNearDeltaInfo.DeltaInfos == null || syncNearDeltaInfo.DeltaInfos.Count == 0) return;
            foreach (var aoiSyncDelta in syncNearDeltaInfo.DeltaInfos) ProcessAoiSyncDelta(aoiSyncDelta);
        }

        private static bool IsUnknownString(string? s) => string.IsNullOrWhiteSpace(s) || s == "未知" || s == "未知昵称" || s == "未知职业" || s == "Unknown";

        public static void ProcessAoiSyncDelta(AoiSyncDelta delta)
        {
            if (delta == null) return;
            ulong targetUuidRaw = (ulong)delta.Uuid;
            if (targetUuidRaw == 0) return;
            bool isTargetPlayer = IsUuidPlayerRaw(targetUuidRaw);
            ulong targetUuid = Shr16(targetUuidRaw);

            var se = delta.SkillEffects;
            if (se?.Damages == null || se.Damages.Count == 0) return;

            foreach (var d in se.Damages)
            {
                long skillId = d.OwnerId;
                if (skillId == 0) continue;

                ulong attackerRaw = (ulong)(d.TopSummonerId != 0 ? d.TopSummonerId : d.AttackerUuid);
                if (attackerRaw == 0) continue;
                bool isAttackerPlayer = IsUuidPlayerRaw(attackerRaw);
                ulong attackerUuid = Shr16(attackerRaw);

                if (isAttackerPlayer && attackerUuid != 0)
                {
                    var info = StatisticData._manager.GetPlayerBasicInfo(attackerUuid);
                    if (IsUnknownString(info.Nickname) || IsUnknownString(info.Profession) || info.CombatPower <= 0)
                        PlayerDbSyncService.TryFillFromDbOnce(attackerUuid);
                }

                long damageSigned = d.HasValue ? d.Value : (d.HasLuckyValue ? d.LuckyValue : 0L);
                if (damageSigned == 0) continue;
                ulong damage = (ulong)(damageSigned < 0 ? -damageSigned : damageSigned);

                bool isCrit = d.TypeFlag != null && ((d.TypeFlag & 1) == 1);
                bool isHeal = d.Type == EDamageType.Heal;
                var luckyValue = d.LuckyValue;
                bool isLucky = luckyValue != null && luckyValue != 0;
                ulong hpLessen = d.HasHpLessenValue ? (ulong)d.HpLessenValue : 0UL;

                if (AppConfig.PilingMode)
                {
                    if (attackerUuid != AppConfig.Uid) continue;
                    if (targetUuid != 75) continue;
                }

                if (isTargetPlayer)
                {
                    if (isHeal)
                    {
                        if (isAttackerPlayer) StatisticData._manager.AddHealing(attackerUuid, (ulong)skillId, hpLessen, isCrit, isLucky);
                    }
                    else
                    {
                        StatisticData._manager.AddTakenDamage(targetUuid, (ulong)skillId, damage, isCrit, isLucky, hpLessen);
                    }
                }
                else
                {
                    if (!isHeal && isAttackerPlayer)
                    {
                        StatisticData._manager.AddDamage(attackerUuid, (ulong)skillId, damage, isCrit, isLucky, hpLessen);
                    }
                    if (AppConfig.NpcsTakeDamage)
                    {
                        StatisticData._manager.AddTakenDamage(targetUuid, (ulong)skillId, damage, isCrit, isLucky, hpLessen);
                        Console.WriteLine(@$"怪物ID：{targetUuid}受到伤害{damage},来自{attackerUuid}的技能{skillId}");
                    }
                }
            }
        }

        public static long currentUserUuid = 0;
        public static void ProcessSyncToMeDeltaInfo(byte[] payloadBuffer)
        {
            var syncToMeDeltaInfo = SyncToMeDeltaInfo.Parser.ParseFrom(payloadBuffer);
            var aoiSyncToMeDelta = syncToMeDeltaInfo.DeltaInfo;
            long uuid = aoiSyncToMeDelta.Uuid;
            if (uuid != 0 && currentUserUuid != uuid)
            {
                currentUserUuid = uuid;
                PlayerDbSyncService.TryFillFromDbOnce((ulong)currentUserUuid >> 16);
            }
            var aoiSyncDelta = aoiSyncToMeDelta.BaseDelta;
            if (aoiSyncDelta == null) return;
            ProcessAoiSyncDelta(aoiSyncDelta);
        }

        public static void ProcessSyncContainerData(byte[] payloadBuffer)
        {
            var syncContainerData = SyncContainerData.Parser.ParseFrom(payloadBuffer);
            if (syncContainerData?.VData == null) return;

            var vData = syncContainerData.VData;
            if (vData.CharId == null || vData.CharId == 0) return;

            ulong playerUid = vData.CharId;
            if (playerUid != 0) { try { AppConfig.Uid = playerUid; } catch { } }
            PlayerDbSyncService.TryFillFromDbOnce(playerUid);

            bool updated = false;

            if (vData.RoleLevel?.Level != 0)
                StatisticData._manager.SetAttrKV(playerUid, "level", vData.RoleLevel.Level);

            if (vData.Attr?.CurHp != 0)
                StatisticData._manager.SetAttrKV(playerUid, "hp", (int)vData.Attr.CurHp);

            if (vData.Attr?.MaxHp != 0)
                StatisticData._manager.SetAttrKV(playerUid, "max_hp", (int)vData.Attr.MaxHp);

            if (vData.CharBase != null)
            {
                if (!string.IsNullOrEmpty(vData.CharBase.Name))
                {
                    StatisticData._manager.SetNickname(playerUid, vData.CharBase.Name);
                    AppConfig.NickName = vData.CharBase.Name;
                    updated = true;
                }
                if (vData.CharBase.FightPoint != 0)
                {
                    StatisticData._manager.SetCombatPower(playerUid, vData.CharBase.FightPoint);
                    AppConfig.CombatPower = vData.CharBase.FightPoint;
                    updated = true;
                }
            }

            var professionList = vData.ProfessionList;
            if (professionList != null && professionList.CurProfessionId != 0)
            {
                var professionName = GetProfessionNameFromId(professionList.CurProfessionId);
                AppConfig.Profession = professionName;
                updated = true;
            }
            if (updated) Task.Run(() => PlayerDbSyncService.UpsertCurrentAsync(playerUid));
        }

        public static void ProcessSyncContainerDirtyData(byte[] payloadBuffer)
        {
            try
            {
                if (currentUserUuid == 0) return;
                var dirty = SyncContainerDirtyData.Parser.ParseFrom(payloadBuffer);
                if (dirty?.VData?.BufferS == null || dirty.VData.BufferS.Length == 0) return;

                var buf = dirty.VData.BufferS.ToByteArray();
                using var ms = new MemoryStream(buf, writable: false);
                using var br = new BinaryReader(ms);

                if (!DoesStreamHaveIdentifier(br)) return;

                uint fieldIndex = br.ReadUInt32();
                _ = br.ReadInt32();

                ulong playerUid = (ulong)currentUserUuid >> 16;
                bool updated = false;

                switch (fieldIndex)
                {
                    case 2:
                    {
                        if (!DoesStreamHaveIdentifier(br)) break;
                        fieldIndex = br.ReadUInt32();
                        _ = br.ReadInt32();
                        switch (fieldIndex)
                        {
                            case 5:
                            {
                                string playerName = StreamReadString(br);
                                if (!string.IsNullOrEmpty(playerName))
                                {
                                    StatisticData._manager.SetNickname(playerUid, playerName);
                                    AppConfig.NickName = playerName;
                                    updated = true;
                                }
                                break;
                            }
                            case 35:
                            {
                                uint fightPoint = br.ReadUInt32();
                                _ = br.ReadInt32();
                                if (fightPoint != 0)
                                {
                                    StatisticData._manager.SetCombatPower(playerUid, (int)fightPoint);
                                    AppConfig.CombatPower = (int)fightPoint;
                                    updated = true;
                                }
                                break;
                            }
                        }
                        break;
                    }
                    case 16:
                    {
                        if (!DoesStreamHaveIdentifier(br)) break;
                        fieldIndex = br.ReadUInt32();
                        _ = br.ReadInt32();
                        switch (fieldIndex)
                        {
                            case 1:
                            {
                                uint curHp = br.ReadUInt32();
                                StatisticData._manager.SetAttrKV(playerUid, "hp", (int)curHp);
                                break;
                            }
                            case 2:
                            {
                                uint maxHp = br.ReadUInt32();
                                StatisticData._manager.SetAttrKV(playerUid, "max_hp", (int)maxHp);
                                break;
                            }
                        }
                        break;
                    }
                    case 61:
                    {
                        if (!DoesStreamHaveIdentifier(br)) break;
                        fieldIndex = br.ReadUInt32();
                        _ = br.ReadInt32();
                        if (fieldIndex == 1)
                        {
                            uint curProfessionId = br.ReadUInt32();
                            _ = br.ReadInt32();
                            if (curProfessionId != 0)
                            {
                                var professionName = GetProfessionNameFromId((int)curProfessionId);
                                AppConfig.Profession = professionName;
                                StatisticData._manager.SetProfession(playerUid, professionName);
                                updated = true;
                            }
                        }
                        break;
                    }
                }

                if (updated) Task.Run(() => PlayerDbSyncService.UpsertCurrentAsync(playerUid));
            }
            catch { }
        }

        private static bool DoesStreamHaveIdentifier(BinaryReader br)
        {
            var s = br.BaseStream;
            if (s.Position + 8 > s.Length) return false;
            _ = br.ReadUInt64();
            return true;
        }

        private static string StreamReadString(BinaryReader br)
        {
            uint len = br.ReadUInt32();
            if (len == 0) return string.Empty;
            var bytes = br.ReadBytes((int)len);
            int pad = (int)((4 - (len % 4)) % 4);
            if (pad > 0) _ = br.ReadBytes(pad);
            return Encoding.UTF8.GetString(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsUuidPlayerRaw(ulong uuidRaw) => (uuidRaw & 0xFFFFUL) == 640UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Shr16(ulong v) => v >> 16;

        public static string GetProfessionNameFromId(int professionId) => professionId switch
        {
            1 => "雷影剑士",
            2 => "冰魔导师",
            3 => "青岚骑士",
            4 => "森语者",
            5 => "巨刃守护者",
            6 => "神射手",
            7 => "神盾骑士",
            8 => "灵魂乐手",
            _ => string.Empty,
        };
    }
}
