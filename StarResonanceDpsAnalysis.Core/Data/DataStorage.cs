using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using StarResonanceDpsAnalysis.Core.Data.Models;

namespace StarResonanceDpsAnalysis.Core.Data
{
    /// <summary>
    /// 数据存储
    /// </summary>
    public static class DataStorage
    {
        /// <summary>
        /// 当前玩家信息
        /// </summary>
        public static PlayerInfo CurrentPlayerInfo { get; private set; } = new();
        /// <summary>
        /// 玩家信息字典 (Key: UID)
        /// </summary>
        private static Dictionary<long, PlayerInfo> PlayerInfoDatas { get; } = [];
        /// <summary>
        /// 只读玩家信息字典 (Key: UID)
        /// </summary>
        public static ReadOnlyDictionary<long, PlayerInfo> ReadOnlyPlayerInfoDatas { get => PlayerInfoDatas.AsReadOnly(); }

        /// <summary>
        /// 战斗日志列表
        /// </summary>
        private static List<BattleLog> BattleLogs { get; } = new(262144);
        /// <summary>
        /// 只读战斗日志列表
        /// </summary>
        public static IReadOnlyList<BattleLog> ReadOnlyBattleLogs { get => BattleLogs.AsReadOnly(); }
        /// <summary>
        /// 玩家战斗日志列表字典 (Key: UID)
        /// </summary>
        private static Dictionary<long, List<BattleLog>> PlayerBattleLogs { get; } = [];
        /// <summary>
        /// 只读玩家战斗日志列表字典 (Key: UID)
        /// </summary>
        public static ReadOnlyDictionary<long, List<BattleLog>> ReadOnlyPlayerBattleLogs { get => PlayerBattleLogs.AsReadOnly(); }
        /// <summary>
        /// 战斗日志分段列表
        /// </summary>
        private static List<BattleLogSection> BattleLogSections { get; } = [];
        /// <summary>
        /// 只读战斗日志分段列表
        /// </summary>
        public static IReadOnlyList<BattleLogSection> ReadOnlyBattleLogSections { get => BattleLogSections.AsReadOnly(); }
        /// <summary>
        /// 战斗日志分段超时时间 (默认: 5000ms)
        /// </summary>
        public static TimeSpan SectionTimeout { get; set; } = TimeSpan.FromMilliseconds(5000);
        /// <summary>
        /// 强制新分段标记
        /// </summary>
        /// <remarks>
        /// 设置为 true 后将在下一次添加战斗日志时, 强制创建一个新的分段之后重置为 false
        /// </remarks>
        private static bool ForceNewBattleSection { get; set; } = false;

        /// <summary>
        /// 全程玩家DPS字典 (Key: UID)
        /// </summary>
        private static Dictionary<long, DpsData> FullDpsData { get; } = [];
        /// <summary>
        /// 只读全程玩家DPS字典 (Key: UID)
        /// </summary>
        public static ReadOnlyDictionary<long, DpsData> ReadOnlyFullDpsData { get => FullDpsData.AsReadOnly(); }
        /// <summary>
        /// 只读全程玩家DPS列表; 注意! 频繁读取该属性可能会导致性能问题!
        /// </summary>
        public static IReadOnlyList<DpsData> ReadOnlyFullDpsDataList { get => FullDpsData.Values.ToList().AsReadOnly(); }
        /// <summary>
        /// 阶段性玩家DPS字典 (Key: UID)
        /// </summary>
        private static Dictionary<long, DpsData> SectionedDpsDatas { get; } = [];
        /// <summary>
        /// 阶段性只读玩家DPS字典 (Key: UID)
        /// </summary>
        public static ReadOnlyDictionary<long, DpsData> ReadOnlySectionedDpsDatas { get => SectionedDpsDatas.AsReadOnly(); }
        /// <summary>
        /// 阶段性只读玩家DPS列表; 注意! 频繁读取该属性可能会导致性能问题!
        /// </summary>
        public static IReadOnlyList<DpsData> ReadOnlySectionedDpsDataList { get => SectionedDpsDatas.Values.ToList().AsReadOnly(); }

        public delegate void PlayerInfoUpdatedEventHandler(PlayerInfo info);
        public delegate void BattleLogNewSectionCreatedEventHandler();
        public delegate void BattleLogUpdatedEventHandler(BattleLog battleLog);
        public delegate void DataUpdatedEventHandler();

        /// <summary>
        /// 玩家信息更新事件
        /// </summary>
        public static event PlayerInfoUpdatedEventHandler? PlayerInfoUpdated;
        /// <summary>
        /// 战斗日志新分段创建事件
        /// </summary>
        public static event BattleLogNewSectionCreatedEventHandler? BattleLogNewSectionCreated;
        /// <summary>
        /// 战斗日志更新事件
        /// </summary>
        public static event BattleLogUpdatedEventHandler? BattleLogUpdated;
        /// <summary>
        /// 数据更新事件 (玩家信息或战斗日志更新时触发)
        /// </summary>
        public static event DataUpdatedEventHandler? DataUpdated;

        /// <summary>
        /// 检查或创建玩家信息
        /// </summary>
        /// <param name="uid"UID></param>
        /// <returns>是否已经存在; 是: true, 否: false</returns>
        /// <remarks>
        /// 如果传入的 UID 已存在, 则不会进行任何操作;
        /// 否则会创建一个新的 PlayerInfo 并触发 PlayerInfoUpdated 事件
        /// </remarks>
        internal static bool TestCreatePlayerInfoByUID(long uid)
        {
            if (PlayerInfoDatas.ContainsKey(uid))
            {
                return true;
            }

            PlayerInfoDatas[uid] = new PlayerInfo() { UID = uid };

            TriggerPlayerInfoUpdated(uid);

            return false;
        }

        /// <summary>
        /// 设置玩家名称
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="name">玩家名称</param>
        internal static void SetPlayerName(long uid, string name)
        {
            PlayerInfoDatas[uid].Name = name;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家职业ID
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="professionId">职业ID</param>
        internal static void SetPlayerProfessionID(long uid, int professionId)
        {
            PlayerInfoDatas[uid].ProfessionID = professionId;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家战力
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="fightPoint">战力</param>
        internal static void SetPlayerCombatPower(long uid, int combatPower)
        {
            PlayerInfoDatas[uid].CombatPower = combatPower;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家等级
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="level">等级</param>
        internal static void SetPlayerLevel(long uid, int level)
        {
            PlayerInfoDatas[uid].Level = level;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家 RankLevel
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="rankLevel">RankLevel</param>
        /// <remarks>
        /// 暂不清楚 RankLevel 的具体含义...
        /// </remarks>
        internal static void SetPlayerRankLevel(long uid, int rankLevel)
        {
            PlayerInfoDatas[uid].RankLevel = rankLevel;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家暴击
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="critical">暴击值</param>
        internal static void SetPlayerCritical(long uid, int critical)
        {
            PlayerInfoDatas[uid].Critical = critical;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家幸运
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="lucky">幸运值</param>
        internal static void SetPlayerLucky(long uid, int lucky)
        {
            PlayerInfoDatas[uid].Lucky = lucky;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家当前HP
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="hp">当前HP</param>
        internal static void SetPlayerHP(long uid, long hp)
        {
            PlayerInfoDatas[uid].HP = hp;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 设置玩家最大HP
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="maxHp">最大HP</param>
        internal static void SetPlayerMaxHP(long uid, long maxHp)
        {
            PlayerInfoDatas[uid].MaxHP = maxHp;

            TriggerPlayerInfoUpdated(uid);
        }

        /// <summary>
        /// 触发玩家信息更新事件
        /// </summary>
        /// <param name="uid">UID</param>
        private static void TriggerPlayerInfoUpdated(long uid)
        {
            PlayerInfoUpdated?.Invoke(PlayerInfoDatas[uid]);
            DataUpdated?.Invoke();
        }

        /// <summary>
        /// 检查或创建玩家战斗日志列表
        /// </summary>
        /// <param name="uid">UID</param>
        /// <returns>是否已经存在; 是: true, 否: false</returns>
        /// <remarks>
        /// 如果传入的 UID 已存在, 则不会进行任何操作;
        /// 否则会创建一个新的对应 UID 的 List<BattleLog>
        /// </remarks>
        internal static bool TestCreateBattleLogAndDpsDataByUID(long uid)
        {
            var battleLogFlag = PlayerBattleLogs.ContainsKey(uid);
            if (!battleLogFlag)
            {
                PlayerBattleLogs[uid] = new(16384);
            }

            var sectionedDpsDataFlag = SectionedDpsDatas.TryGetValue(uid, out var sectionedDpsData);
            if (!sectionedDpsDataFlag)
            {
                sectionedDpsData = new() { UID = uid };
            }

            var fullDpsDataFlag = FullDpsData.TryGetValue(uid, out var fullDpsData);
            if (!fullDpsDataFlag)
            {
                fullDpsData = new() { UID = uid };
            }

            SectionedDpsDatas[uid] = sectionedDpsData;
            FullDpsData[uid] = fullDpsData;

            return battleLogFlag && sectionedDpsDataFlag;
        }

        /// <summary>
        /// 添加战斗日志 (会自动创建日志分段)
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="log">战斗日志</param>
        internal static void AddBattleLog(BattleLog log)
        {
            var tt = new TimeSpan(log.TimeTicks);
            var sectionFlag = BattleLogs.Count == 0;
            if (!sectionFlag)
            {
                // 如果超时或强制创建新战斗阶段时, 关闭上一分段, 最后创建新分段
                var prevTt = new TimeSpan(BattleLogs[^1].TimeTicks);
                if (tt - prevTt > SectionTimeout || ForceNewBattleSection)
                {
                    CloseBattleSection();

                    sectionFlag = true;

                    ForceNewBattleSection = false;
                }
            }

            BattleLogs.Add(log);

            if (log.IsTargetPlayer)
            {
                if (log.IsHeal)
                {
                    TestCreatePlayerInfoByUID(log.AttackerUuid);
                    TestCreateBattleLogAndDpsDataByUID(log.AttackerUuid);

                    var data = SectionedDpsDatas[log.AttackerUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalHeal += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    SectionedDpsDatas[log.AttackerUuid] = data;

                    data = FullDpsData[log.AttackerUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalHeal += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    FullDpsData[log.AttackerUuid] = data;

                    PlayerBattleLogs[log.AttackerUuid].Add(log);
                }
                else
                {
                    TestCreatePlayerInfoByUID(log.TargetUuid);
                    TestCreateBattleLogAndDpsDataByUID(log.TargetUuid);

                    var data = SectionedDpsDatas[log.TargetUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalTakenDamage += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    SectionedDpsDatas[log.TargetUuid] = data;

                    data = FullDpsData[log.TargetUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalTakenDamage += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    FullDpsData[log.TargetUuid] = data;

                    PlayerBattleLogs[log.TargetUuid].Add(log);
                }
            }
            else
            {
                if (!log.IsHeal && log.IsAttackerPlayer)
                {
                    TestCreatePlayerInfoByUID(log.AttackerUuid);
                    TestCreateBattleLogAndDpsDataByUID(log.AttackerUuid);

                    var data = SectionedDpsDatas[log.AttackerUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalAttackDamage += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    SectionedDpsDatas[log.AttackerUuid] = data;

                    data = FullDpsData[log.AttackerUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalAttackDamage += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    FullDpsData[log.AttackerUuid] = data;

                    PlayerBattleLogs[log.AttackerUuid].Add(log);
                }

                // 提升局部, 统一局部变量名
                {
                    TestCreatePlayerInfoByUID(log.TargetUuid);
                    TestCreateBattleLogAndDpsDataByUID(log.TargetUuid);

                    var data = SectionedDpsDatas[log.TargetUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalTakenDamage += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    data.IsNpcData = true;
                    SectionedDpsDatas[log.TargetUuid] = data;

                    data = FullDpsData[log.TargetUuid];
                    data.StartLoggedTick ??= log.TimeTicks;
                    data.TotalTakenDamage += log.Value;
                    data.LastLoggedTick = log.TimeTicks;
                    FullDpsData[log.TargetUuid] = data;
                    data.IsNpcData = true;
                    PlayerBattleLogs[log.TargetUuid].Add(log);
                }
            }

            if (sectionFlag)
            {
                StartNewBattleSection();

                BattleLogNewSectionCreated?.Invoke();
            }

            BattleLogUpdated?.Invoke(log);
            DataUpdated?.Invoke();
        }

        /// <summary>
        /// 关闭当前战斗日志分段
        /// </summary>
        private static void CloseBattleSection()
        {
            var lastSection = BattleLogSections.Last();
            lastSection.EndIndex = BattleLogs.Count - 1;
            BattleLogSections[^1] = lastSection;
        }

        /// <summary>
        /// 开始新的战斗日志分段
        /// </summary>
        private static void StartNewBattleSection()
        {
            BattleLogSections.Add(new() { StartIndex = BattleLogs.Count - 1 });
        }

        /// <summary>
        /// 清除所有DPS数据 (包括全程和阶段性)
        /// </summary>
        public static void ClearAllDpsData()
        {
            SectionedDpsDatas.Clear();
            FullDpsData.Clear();
        }

        /// <summary>
        /// 标记新的战斗日志分段 (清空阶段性Dps数据)
        /// </summary>
        public static void MarkNewBattleSection()
        {
            ForceNewBattleSection = true;
            SectionedDpsDatas.Clear();
        }

        /// <summary>
        /// 清除当前玩家信息
        /// </summary>
        public static void ClearCurrentPlayerInfo()
        {
            CurrentPlayerInfo = new();
        }

        /// <summary>
        /// 清除所有玩家信息
        /// </summary>
        public static void ClearPlayerInfos()
        {
            PlayerInfoDatas.Clear();
        }

        /// <summary>
        /// 清除所有数据 (包括缓存历史)
        /// </summary>
        public static void ClearAllData()
        {
            ClearCurrentPlayerInfo();
            ClearPlayerInfos();
        }

    }
}
