using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using 星痕共鸣DPS统计.Plugin;

namespace 星痕共鸣DPS统计
{
    /// <summary>
    /// 通用统计类，用于处理伤害或治疗数据，包括数值统计、次数统计、实时窗口和DPS/HPS计算
    /// </summary>
    public class StatisticData
    {
        public static readonly PlayerDataManager _manager = new PlayerDataManager();

        // —— 数值统计 ——
        /// <summary>普通命中数值总和</summary>
        public ulong Normal { get; private set; }
        /// <summary>暴击数值总和</summary>
        public ulong Critical { get; private set; }
        /// <summary>幸运命中数值总和</summary>
        public ulong Lucky { get; private set; }
        /// <summary>既暴击又幸运数值总和</summary>
        public ulong CritLucky { get; private set; }
        /// <summary>造成的伤害（HP减少）总和，仅用于伤害统计</summary>
        public ulong HpLessen { get; private set; }
        /// <summary>所有命中数值总和</summary>
        public ulong Total { get; private set; }

        // —— 次数统计 ——
        /// <summary>普通命中次数</summary>
        public int CountNormal { get; private set; }
        /// <summary>暴击次数</summary>
        public int CountCritical { get; private set; }
        /// <summary>幸运命中次数</summary>
        public int CountLucky { get; private set; }
        /// <summary>所有命中次数</summary>
        public int CountTotal { get; private set; }

        // —— 实时统计窗口 ——
        // 存储过去一段时间（1秒）内的所有记录，用于计算实时DPS/HPS
        private readonly List<(DateTime Time, ulong Value)> _realtimeWindow = new();

        // —— 时间范围 ——
        private DateTime? _startTime;  // 统计开始时间
        private DateTime? _endTime;    // 最近一次记录时间

        // —— 实时结果 ——
        /// <summary>当前1秒内的累计数值（实时DPS/HPS）</summary>
        public ulong RealtimeValue { get; private set; }
        /// <summary>历史最大1秒峰值</summary>
        public ulong RealtimeMax { get; private set; }

        /// <summary>
        /// 添加一条新记录（普通/暴击/幸运等），并更新统计
        /// </summary>
        /// <param name="value">记录数值（伤害或治疗量）</param>
        /// <param name="isCrit">是否为暴击</param>
        /// <param name="isLucky">是否为幸运</param>
        /// <param name="hpLessenValue">HP减少值，仅伤害调用时传入</param>
        public void AddRecord(ulong value, bool isCrit, bool isLucky, ulong hpLessenValue = 0)
        {
            var now = DateTime.Now;

            // —— 更新数值统计 ——
            if (isCrit && isLucky)
            {
                CritLucky += value;  // 暴击且幸运
            }
            else if (isCrit)
            {
                Critical += value;    // 仅暴击
            }
            else if (isLucky)
            {
                Lucky += value;       // 仅幸运
            }
            else
            {
                Normal += value;      // 普通命中
            }
            Total += value;           // 总和
            HpLessen += hpLessenValue; // 伤害专用

            // —— 更新次数统计 ——
            if (isCrit) CountCritical++;
            if (isLucky) CountLucky++;
            if (!isCrit && !isLucky) CountNormal++;
            CountTotal++;

            // —— 记录至实时窗口 ——
            _realtimeWindow.Add((now, value));

            // —— 更新时间范围 ——
            if (_startTime == null)
                _startTime = now;  // 首次记录设为开始时间
            _endTime = now;        // 更新时间范围结尾
        }

        /// <summary>
        /// 更新实时统计，剔除超过1秒的旧记录，并计算当前1秒内数值
        /// </summary>
        public void UpdateRealtimeStats()
        {
            var now = DateTime.Now;
            // 移除时间差超过1秒的记录
            _realtimeWindow.RemoveAll(e => (now - e.Time).TotalSeconds > 1);

            // 累加当前窗口所有数值
            ulong sum = 0;
            foreach (var entry in _realtimeWindow)
                sum += entry.Value;
            RealtimeValue = sum;

            // 更新峰值
            if (RealtimeValue > RealtimeMax)
                RealtimeMax = RealtimeValue;
        }

        /// <summary>
        /// 计算从开始到结束的总平均每秒值（用于总DPS或HPS）
        /// </summary>
        public double GetTotalPerSecond()
        {
            // 时间范围无效或长度为0时返回0
            if (_startTime == null || _endTime == null || _startTime == _endTime)
                return 0;

            var duration = (_endTime.Value - _startTime.Value).TotalSeconds;
            return duration > 0 ? Total / duration : 0;
        }

        /// <summary>
        /// 清空所有统计数据，重置状态
        /// </summary>
        public void Reset()
        {
            Normal = Critical = Lucky = CritLucky = HpLessen = Total = 0;
            CountNormal = CountCritical = CountLucky = CountTotal = 0;
            _realtimeWindow.Clear();
            _startTime = _endTime = null;
            RealtimeValue = RealtimeMax = 0;
        }
    }

    /// <summary>
    /// 单个玩家数据类，包含伤害、治疗及承伤统计，并支持按技能分类统计
    /// </summary>
    public class PlayerData
    {
        /// <summary>玩家唯一标识 UID</summary>
        public ulong Uid { get; }

        public string Nickname { get; set; } = "未知";

        /// <summary>玩家伤害统计</summary>
        public StatisticData DamageStats { get; } = new();
        /// <summary>玩家治疗统计</summary>
        public StatisticData HealingStats { get; } = new();
        /// <summary>玩家承受总伤害（伤害吸收、护盾等）</summary>
        public ulong TakenDamage { get; private set; }
        /// <summary>玩家职业名称</summary>
        public string Profession { get; private set; } = "未知";

        /// <summary>按技能ID分组的统计数据</summary>
        public Dictionary<ulong, StatisticData> SkillUsage { get; } = new();

        /// <summary>按技能ID分组的承伤统计</summary>
        public Dictionary<ulong, StatisticData> TakenDamageBySkill { get; }
            = new Dictionary<ulong, StatisticData>();

        /// <summary>
        /// 构造函数，初始化玩家UID
        /// </summary>
        public PlayerData(ulong uid)
        {
            Uid = uid;
        }

        /// <summary>
        /// 添加伤害记录，并同步按技能统计
        /// </summary>
        public void AddDamage(ulong skillId, ulong damage, bool isCrit, bool isLucky, ulong hpLessen = 0)
        {
            DamageStats.AddRecord(damage, isCrit, isLucky, hpLessen);
            if (!SkillUsage.TryGetValue(skillId, out var stat))
            {
                stat = new StatisticData();
                SkillUsage[skillId] = stat;
            }
            stat.AddRecord(damage, isCrit, isLucky, hpLessen);
        }

        /// <summary>
        /// 添加治疗记录
        /// </summary>
        public void AddHealing(ulong healing, bool isCrit, bool isLucky)
        {
            HealingStats.AddRecord(healing, isCrit, isLucky);
        }

        /// <summary>
        /// 添加承伤记录（受到的伤害量累加）
        /// </summary>
        public void AddTakenDamage(ulong skillId, ulong damage)
        {
            // 全局累加
            TakenDamage += damage;

            // 按技能累加
            if (!TakenDamageBySkill.TryGetValue(skillId, out var stat))
            {
                stat = new StatisticData();
                TakenDamageBySkill[skillId] = stat;
            }
            // 对 “承伤” 这里不区分暴击/幸运，我们直接传 false, false
            stat.AddRecord(damage, isCrit: false, isLucky: false, hpLessenValue: damage);
        }

        /// <summary>
        /// 设置玩家职业
        /// </summary>
        public void SetProfession(string profession)
        {
            Profession = profession;
        }

        /// <summary>
        /// 更新玩家的实时DPS/HPS数据
        /// </summary>
        public void UpdateRealtimeStats()
        {
            DamageStats.UpdateRealtimeStats();
            HealingStats.UpdateRealtimeStats();
        }

        /// <summary>
        /// 获取总DPS（总平均每秒值）
        /// </summary>
        public double GetTotalDps() => DamageStats.GetTotalPerSecond();
        /// <summary>
        /// 获取总HPS（总平均每秒值）
        /// </summary>
        public double GetTotalHps() => HealingStats.GetTotalPerSecond();

        /// <summary>
        /// 获取合并后的命中次数统计
        /// </summary>
        public (int Normal, int Critical, int Lucky, int Total) GetTotalCount()
        {
            return (
                DamageStats.CountNormal + HealingStats.CountNormal,
                DamageStats.CountCritical + HealingStats.CountCritical,
                DamageStats.CountLucky + HealingStats.CountLucky,
                DamageStats.CountTotal + HealingStats.CountTotal
            );
        }

        /// <summary>
        /// 重置玩家所有数据，恢复初始状态
        /// </summary>
        public void Reset()
        {
            DamageStats.Reset();
            HealingStats.Reset();
            TakenDamage = 0;
            Profession = "未知";
            SkillUsage.Clear();
        }
    }

    /// <summary>
    /// 玩家数据管理器，负责创建、获取与清理所有玩家数据实例
    /// </summary>
    public class PlayerDataManager
    {
        // 存储所有玩家数据，key=UID
        private readonly Dictionary<ulong, PlayerData> _players = new();


        /// <summary>
        /// 获取或创建指定UID的玩家数据
        /// </summary>
        public PlayerData GetOrCreate(ulong uid)
        {
            if (!_players.TryGetValue(uid, out var data))
            {
                data = new PlayerData(uid);
                _players[uid] = data;
                
                _lastAddTime = DateTime.Now;

                _hasTriggeredFetch = false; // 有新玩家，允许重新触发

                // ✅ 如果缓存里已经有昵称，直接用
                if (_nicknameRequestedUids.TryGetValue(uid, out var cachedName) &&
                    !string.IsNullOrWhiteSpace(cachedName))
                {
                    data.Nickname = cachedName;
                }
            }
            return data;
        }
        private bool _hasTriggeredFetch = false;
        private DateTime _lastAddTime = DateTime.MinValue;

        private readonly System.Timers.Timer _checkTimer;

        public PlayerDataManager()
        {
            _checkTimer = new System.Timers.Timer(1000); // 每秒检查一次
            _checkTimer.AutoReset = true;

            _checkTimer.Elapsed += CheckTimerElapsed;
            _checkTimer.Start();
        }

        private static Dictionary<ulong,string> _nicknameRequestedUids = new Dictionary<ulong, string>();


        // 单独封装为 async void 方法（Timer支持）
        private async void CheckTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_lastAddTime == DateTime.MinValue || _players.Count == 0)
                return;

            var elapsed = (DateTime.Now - _lastAddTime).TotalSeconds;

            // Console.WriteLine($"[Timer] 距上次添加 {elapsed:F2}s, 触发状态: {_hasTriggeredFetch}");

            if (elapsed >= 5 && !_hasTriggeredFetch)
            {
                _hasTriggeredFetch = true;

                // 🔍 只挑出未请求过昵称的 UID
                var newUids = _players
                 .Where(p => !_nicknameRequestedUids.ContainsKey(p.Key)) // 没请求过
                 .Where(p => string.IsNullOrWhiteSpace(p.Value.Nickname) || p.Value.Nickname == "未知") // 昵称确实为空
                 .Select(p => p.Key)
                 .ToList();


                if (newUids.Count == 0)
                {
                    Console.WriteLine("✅ 所有 UID 昵称已请求，无需重复请求");
                    return;
                }

                var uidList = _players.Keys.Select(uid => uid.ToString()).ToList();

                await Common.player_uid_map(uidList);

                var data = await Common.player_uid_map(uidList);

                if (data != null && data["code"]?.ToString() == "200")
                {
                    var dict = data["data"]; // 如果是数组格式
                    if (dict != null)
                    {
                        foreach (var item in dict)
                        {
                            if (item["uid"] == null) continue;

                            var uid = Convert.ToUInt64(item["uid"]);
                            var nickname = item["name"]?.ToString();

                            if (!string.IsNullOrWhiteSpace(nickname))
                            {
                                var player = GetOrCreate(uid);
                                player.Nickname = nickname;

                                // ✅ 存入缓存
                                _nicknameRequestedUids[uid] = nickname;

                                Console.WriteLine($"✅ UID {uid} 昵称更新为：{nickname}");
                            }
                        }
                    }
                }



            }
        }

        /// <summary>
        /// 添加全局伤害记录
        /// </summary>
        public void AddDamage(ulong uid, ulong skillId, ulong damage, bool isCrit, bool isLucky, ulong hpLessen = 0)
        {
            GetOrCreate(uid).AddDamage(skillId, damage, isCrit, isLucky, hpLessen);
        }

        /// <summary>
        /// 添加全局治疗记录
        /// </summary>
        public void AddHealing(ulong uid, ulong healing, bool isCrit, bool isLucky)
        {
            GetOrCreate(uid).AddHealing(healing, isCrit, isLucky);
        }

        /// <summary>
        /// 添加全局承伤记录
        /// </summary>
        public void AddTakenDamage(ulong uid, ulong skillId, ulong damage)
        {
            GetOrCreate(uid).AddTakenDamage(skillId, damage);
        }

        /// <summary>
        /// 全局设置玩家职业
        /// </summary>
        public void SetProfession(ulong uid, string profession)
        {
            GetOrCreate(uid).SetProfession(profession);
        }

        /// <summary>
        /// 更新所有玩家的实时DPS/HPS
        /// </summary>
        public void UpdateAllRealtimeStats()
        {
            foreach (var player in _players.Values)
                player.UpdateRealtimeStats();
        }

        /// <summary>
        /// 获取所有玩家数据列表
        /// </summary>
        public IEnumerable<PlayerData> GetAllPlayers() => _players.Values;

        /// <summary>
        /// 清空所有玩家数据
        /// </summary>
        public void ClearAll() => _players.Clear();

        /// <summary>
        /// 获取所有玩家UID列表
        /// </summary>
        public IEnumerable<ulong> GetAllUids() => _players.Keys;
    }
}
