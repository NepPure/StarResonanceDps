using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public static PlayerInfo CurrentPlayerInfo { get; } = new();
        /// <summary>
        /// 玩家信息字典 (Key: UID)
        /// </summary>
        internal static Dictionary<long, PlayerInfo> PlayerInfoDatas { get; } = [];
        /// <summary>
        /// 只读玩家信息字典 (Key: UID)
        /// </summary>
        public static ReadOnlyDictionary<long, PlayerInfo> ReadOnlyPlayerInfoDatas { get => PlayerInfoDatas.AsReadOnly(); }

        /// <summary>
        /// 战斗日志列表
        /// </summary>
        internal static List<BattleLog> BattleLogs { get; } = new(262144);
        /// <summary>
        /// 只读战斗日志列表
        /// </summary>
        public static IReadOnlyList<BattleLog> ReadOnlyBattleLogs { get => BattleLogs.AsReadOnly(); }
        /// <summary>
        /// 玩家战斗日志列表字典 (Key: UID)
        /// </summary>
        internal static Dictionary<long, List<BattleLog>> PlayerBattleLogs { get; } = [];
        /// <summary>
        /// 只读玩家战斗日志列表字典 (Key: UID)
        /// </summary>
        public static ReadOnlyDictionary<long, List<BattleLog>> ReadOnlyPlayerBattleLogs { get => PlayerBattleLogs.AsReadOnly(); }
        /// <summary>
        /// 战斗日志分段列表
        /// </summary>
        internal static List<BattleLogSection> BattleLogSections { get; } = [];
        /// <summary>
        /// 只读战斗日志分段列表
        /// </summary>
        public static IReadOnlyList<BattleLogSection> ReadOnlyBattleLogSections { get => BattleLogSections.AsReadOnly(); }
        /// <summary>
        /// 战斗日志分段超时时间 (默认: 5000ms)
        /// </summary>
        public static TimeSpan SectionTimeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        // TODO: 还没实现
        internal static Dictionary<long, DpsData> DpsDatas { get; } = [];
        public static ReadOnlyDictionary<long, DpsData> ReadOnlyDpsDatas { get => new(DpsDatas); }
        public static IReadOnlyList<DpsData> ReadOnlyDpsDataList { get => DpsDatas.Values.ToList().AsReadOnly(); }

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
        internal static void SetPlayerFightPoint(long uid, int fightPoint)
        {
            PlayerInfoDatas[uid].CombatPower = fightPoint;

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
        internal static void TriggerPlayerInfoUpdated(long uid)
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
        internal static bool TestCreateBattleLogByUID(long uid)
        {
            if (PlayerBattleLogs.ContainsKey(uid))
            {
                return true;
            }

            PlayerBattleLogs[uid] = new(16384);
            return false;
        }

        /// <summary>
        /// 添加战斗日志 (会自动创建日志分段)
        /// </summary>
        /// <param name="uid">UID</param>
        /// <param name="log">战斗日志</param>
        internal static void AddBattleLog(long uid, BattleLog log)
        {
            var tt = new TimeSpan(log.TimeTicks);
            var sectionFlag = BattleLogs.Count == 0;
            if (!sectionFlag)
            {
                var prevTt = new TimeSpan(BattleLogs[^1].TimeTicks);
                if (tt - prevTt > SectionTimeout)
                {
                    sectionFlag = true;

                    CloseBattleSection();
                }
            }

            BattleLogs.Add(log);
            PlayerBattleLogs[uid].Add(log);

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

    }
}
