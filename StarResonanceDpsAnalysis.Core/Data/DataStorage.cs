using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StarResonanceDpsAnalysis.Core.Data.Models;

namespace StarResonanceDpsAnalysis.Core.Data
{
    public static class DataStorage
    {
        public static PlayerInfo CurrentPlayerInfo { get; } = new();
        private static Dictionary<long, PlayerInfo> PlayerInfoDatas { get; } = [];

        private static List<BattleLog> BattleLogs { get; } = [];
        private static Dictionary<long, List<BattleLog>> PlayerBattleLogs { get; } = [];
        private static List<BattleLogSection> BattleLogSections { get; } = [];
        public static TimeSpan SectionTimeout { get; set; } = TimeSpan.FromMicroseconds(5000);

        public delegate void PlayerInfoUpdatedEventHandler(PlayerInfo info);
        public delegate void BattleLogUpdatedEventHandler(BattleLog battleLog);
        public delegate void DataUpdatedEventHandler();

        public static event PlayerInfoUpdatedEventHandler? PlayerInfoUpdated;
        public static event BattleLogUpdatedEventHandler? BattleLogUpdated;
        public static event DataUpdatedEventHandler? DataUpdated;

        public static bool TestCreatePlayerInfoByUID(long uid)
        {
            if (PlayerInfoDatas.ContainsKey(uid))
            {
                return true;
            }

            PlayerInfoDatas[uid] = new PlayerInfo() { UID = uid };

            TriggerPlayerInfoUpdated(uid);

            return false;
        }

        public static void SetPlayerName(long uid, string name) 
        {
            PlayerInfoDatas[uid].Name = name;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerProfessionID(long uid, int professionId)
        {
            PlayerInfoDatas[uid].ProfessionID = professionId;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerFightPoint(long uid, int fightPoint)
        {
            PlayerInfoDatas[uid].FightPoint = fightPoint;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerLevel(long uid, int level)
        {
            PlayerInfoDatas[uid].Level = level;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerRankLevel(long uid, int rankLevel)
        {
            PlayerInfoDatas[uid].RankLevel = rankLevel;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerCritical(long uid, int critical)
        {
            PlayerInfoDatas[uid].Critical = critical;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerLucky(long uid, int lucky)
        {
            PlayerInfoDatas[uid].Lucky = lucky;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerHP(long uid, long hp)
        {
            PlayerInfoDatas[uid].HP = hp;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void SetPlayerMaxHP(long uid, long maxHp) 
        {
            PlayerInfoDatas[uid].MaxHP = maxHp;

            TriggerPlayerInfoUpdated(uid);
        }

        public static void TriggerPlayerInfoUpdated(long uid)
        {
            PlayerInfoUpdated?.Invoke(PlayerInfoDatas[uid]);
            DataUpdated?.Invoke();
        }

        public static bool TestCreateBattleLogByUID(long uid)
        {
            if (PlayerBattleLogs.ContainsKey(uid))
            {
                return true;
            }

            PlayerBattleLogs[uid] = [];
            return false;
        }

        public static void AddBattleLog(long uid, BattleLog log)
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
            }

            BattleLogUpdated?.Invoke(log);
            DataUpdated?.Invoke();
        }

        private static void CloseBattleSection()
        {
            var lastSection = BattleLogSections.Last();
            lastSection.EndIndex = BattleLogs.Count - 1;
            BattleLogSections[^1] = lastSection;
        }

        private static void StartNewBattleSection()
        {
            BattleLogSections.Add(new() { StartIndex = BattleLogs.Count - 1 });
        }

        /// <summary>
        /// 职业ID映射为职业名称
        /// </summary>
        public static string GetProfessionNameFromId(int professionId) => professionId switch
        {
            1 => "雷影剑士",
            2 => "冰魔导师",
            3 => "涤罪恶火_战斧",
            4 => "青岚骑士",
            5 => "森语者",
            9 => "巨刃守护者",
            11 => "神射手",
            12 => "神盾骑士",
            8 => "雷霆一闪_手炮",
            10 => "暗灵祈舞_仪刀_仪仗",
            13 => "灵魂乐手",
            _ => string.Empty,
        };
    }
}
