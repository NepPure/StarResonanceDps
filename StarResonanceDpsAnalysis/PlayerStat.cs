using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

using StarResonanceDpsAnalysis.Plugin;

namespace StarResonanceDpsAnalysis
{
    /// <summary>
    /// 通用统计类：用于伤害或治疗的数据累计、次数统计、实时窗口统计，以及总 DPS/HPS 计算
    /// </summary>
    public class StatisticData
    {
        #region 常量

        /// <summary>
        /// 实时统计的时间窗口（秒），用于计算实时值与峰值
        /// </summary>
        private const double 实时窗口秒数 = 1.0;

        #endregion

        #region 静态成员

        /// <summary>
        /// 全局玩家数据管理器（按你原代码保持不变）
        /// </summary>
        public static readonly PlayerDataManager _manager = new PlayerDataManager();

        #endregion

        #region 数值累计（只读属性，内部递增）

        /// <summary>普通命中数值总和</summary>
        public ulong Normal { get; private set; }

        /// <summary>暴击数值总和</summary>
        public ulong Critical { get; private set; }

        /// <summary>幸运命中数值总和</summary>
        public ulong Lucky { get; private set; }

        /// <summary>暴击且幸运数值总和</summary>
        public ulong CritLucky { get; private set; }

        /// <summary>HP 减少总和（伤害统计专用）</summary>
        public ulong HpLessen { get; private set; }

        /// <summary>所有命中数值总和</summary>
        public ulong Total { get; private set; }

        /// <summary>单次命中最大值</summary>
        public ulong MaxSingleHit { get; private set; }

        /// <summary>单次命中最小值（非 0）</summary>
        public ulong MinSingleHit { get; private set; } = ulong.MaxValue;

        #endregion

        #region 次数统计（只读属性，内部递增）

        /// <summary>普通命中次数</summary>
        public int CountNormal { get; private set; }

        /// <summary>暴击次数</summary>
        public int CountCritical { get; private set; }

        /// <summary>幸运命中次数</summary>
        public int CountLucky { get; private set; }

        /// <summary>总命中次数</summary>
        public int CountTotal { get; private set; }

        #endregion

        #region 实时统计窗口

        /// <summary>最近时间窗口内的记录（用于实时 DPS/HPS）</summary>
        private readonly List<(DateTime Time, ulong Value)> _realtimeWindow = new();

        /// <summary>窗口内实时累计值</summary>
        public ulong RealtimeValue { get; private set; }

        /// <summary>历史窗口最大峰值</summary>
        public ulong RealtimeMax { get; private set; }

        #endregion

        #region 时间范围（用于总平均每秒值）

        /// <summary>本次统计开始时间（首次 AddRecord 触发）</summary>
        private DateTime? _startTime;

        /// <summary>最近一次记录时间（用于总时长计算）</summary>
        private DateTime? _endTime;

        /// <summary>最后一次记录时间（只读暴露给外部）</summary>
        public DateTime? LastRecordTime => _endTime;

        #endregion

        #region 公开方法

        /// <summary>
        /// 添加一条新的统计记录（伤害或治疗）
        /// </summary>
        /// <param name="value">记录数值（伤害量或治疗量）</param>
        /// <param name="isCrit">是否暴击</param>
        /// <param name="isLucky">是否幸运</param>
        /// <param name="hpLessenValue">HP 减少值（仅伤害场景传入）</param>
        public void AddRecord(ulong value, bool isCrit, bool isLucky, ulong hpLessenValue = 0)
        {
            var now = DateTime.Now;

            // —— 数值累计 ——
            if (isCrit && isLucky) CritLucky += value;
            else if (isCrit) Critical += value;
            else if (isLucky) Lucky += value;
            else Normal += value;

            Total += value;
            HpLessen += hpLessenValue;

            // —— 次数统计 ——
            if (isCrit) CountCritical++;
            if (isLucky) CountLucky++;
            if (!isCrit && !isLucky) CountNormal++;
            CountTotal++;

            // —— 单次极值 ——
            if (value > 0)
            {
                if (value > MaxSingleHit) MaxSingleHit = value;
                if (value < MinSingleHit) MinSingleHit = value;
            }

            // —— 实时窗口 ——
            _realtimeWindow.Add((now, value));

            // —— 时间范围 ——
            _startTime ??= now;
            _endTime = now;
        }

        /// <summary>
        /// 刷新实时统计：剔除超过窗口期的数据，并计算实时值与峰值
        /// </summary>
        public void UpdateRealtimeStats()
        {
            var now = DateTime.Now;

            // 清理过期数据
            _realtimeWindow.RemoveAll(e => (now - e.Time).TotalSeconds > 实时窗口秒数);

            // 计算当前实时累计
            ulong sum = 0;
            foreach (var entry in _realtimeWindow) sum += entry.Value;
            RealtimeValue = sum;

            // 记录峰值
            if (RealtimeValue > RealtimeMax) RealtimeMax = RealtimeValue;
        }


        /// <summary>
        /// 获取总平均每秒值（Total / 总时长），用于总体 DPS 或 HPS
        /// </summary>
        public double GetTotalPerSecond()
        {
            if (_startTime == null || _endTime == null || _startTime == _endTime) return 0;
            var seconds = (_endTime.Value - _startTime.Value).TotalSeconds;
            return seconds > 0 ? Total / seconds : 0;
        }

        /// <summary>平均每次命中值（Total / CountTotal）</summary>
        public double GetAveragePerHit() => CountTotal > 0 ? (double)Total / CountTotal : 0.0;

        /// <summary>暴击率（包含“暴击且幸运”）</summary>
        public double GetCritRate() => CountTotal > 0 ? (double)CountCritical / CountTotal : 0.0;

        /// <summary>幸运率（包含“暴击且幸运”）</summary>
        public double GetLuckyRate() => CountTotal > 0 ? (double)CountLucky / CountTotal : 0.0;

        /// <summary>
        /// 重置所有统计数据与状态
        /// </summary>
        public void Reset()
        {
            Normal = Critical = Lucky = CritLucky = HpLessen = Total = 0;
            CountNormal = CountCritical = CountLucky = CountTotal = 0;

            MaxSingleHit = 0;
            MinSingleHit = ulong.MaxValue;

            _realtimeWindow.Clear();
            _startTime = _endTime = null;

            RealtimeValue = RealtimeMax = 0;
        }

        #endregion
    }

    // ------------------------------------------------------------
    // # 分类：技能元数据（静态信息，如名称/图标/是否DOT等）
    // ------------------------------------------------------------

    /// <summary>技能元数据（静态）</summary>
    public sealed class SkillMeta
    {
        /// <summary>技能 ID</summary>
        public ulong Id { get; init; }

        /// <summary>技能名称（可从资源/协议注入）</summary>
        public string Name { get; init; } = "未知技能";

        /// <summary>流派/元素系（可选）</summary>
        public string School { get; init; } = "";

        /// <summary>图标路径（可选）</summary>
        public string IconPath { get; init; } = "";

        // 新增
        public StarResonanceDpsAnalysis.Core.SkillType Type { get; init; } =
            StarResonanceDpsAnalysis.Core.SkillType.Unknown;
        public StarResonanceDpsAnalysis.Core.ElementType Element { get; init; } =
            StarResonanceDpsAnalysis.Core.ElementType.Unknown;

        /// <summary>是否为 DOT 技能（可选）</summary>
        public bool IsDoT { get; init; }

        /// <summary>是否为大招/终结技（可选）</summary>
        public bool IsUltimate { get; init; }
    }

    /// <summary>
    /// 技能注册表（进程级缓存）：按 ID 查询元数据；在解析数据时可随时补充/更新
    /// </summary>
    public static class SkillBook
    {
        private static readonly Dictionary<ulong, SkillMeta> _metas = new();

        /// <summary>整条更新/写入一个技能的元数据</summary>
        public static void SetOrUpdate(SkillMeta meta) => _metas[meta.Id] = meta;

        /// <summary>仅更新/设置技能名称（快速接口）</summary>
        public static void SetName(ulong id, string name)
        {
            if (_metas.TryGetValue(id, out var m))
                _metas[id] = new SkillMeta
                {
                    Id = id,
                    Name = name,
                    School = m.School,
                    IconPath = m.IconPath,
                    IsDoT = m.IsDoT,
                    IsUltimate = m.IsUltimate
                };
            else
                _metas[id] = new SkillMeta { Id = id, Name = name };
        }

        /// <summary>获取技能元数据；若不存在则返回带占位名的对象</summary>
        public static SkillMeta Get(ulong id) =>
            _metas.TryGetValue(id, out var m) ? m : new SkillMeta { Id = id, Name = $"技能[{id}]" };

        /// <summary>尝试获取技能元数据</summary>
        public static bool TryGet(ulong id, out SkillMeta meta) => _metas.TryGetValue(id, out meta);
    }

    // ------------------------------------------------------------
    // # 分类：技能摘要 DTO（给 UI/导出使用）
    // ------------------------------------------------------------

    /// <summary>单个玩家的技能摘要（合并统计与元数据）</summary>
    public sealed class SkillSummary
    {
        public ulong SkillId { get; init; }
        public string SkillName { get; init; } = "未知技能";

        public ulong Total { get; init; }
        public int HitCount { get; init; }
        public double AvgPerHit { get; init; }
        public double CritRate { get; init; }
        public double LuckyRate { get; init; }

        public ulong MaxSingleHit { get; init; }
        public ulong MinSingleHit { get; init; }
        public ulong RealtimeValue { get; init; }
        public ulong RealtimeMax { get; init; }

        public double TotalDps { get; init; }      // 该技能自身的平均每秒
        public DateTime? LastTime { get; init; }   // 该技能最后一次命中时间

        // # 分类：新增——历史总伤害占比（整数百分比 0-100）
        public double ShareOfTotal { get; init; }
    }

    /// <summary>全队聚合的技能摘要（跨玩家）</summary>
    public sealed class TeamSkillSummary
    {
        public ulong SkillId { get; init; }
        public string SkillName { get; init; } = "未知技能";
        public ulong Total { get; init; }
        public int HitCount { get; init; }
    }

    /// <summary>
    /// 单个玩家数据：包含伤害、治疗、承伤，以及按技能分组的统计
    /// </summary>
    public class PlayerData
    {
        #region 基本信息

        /// <summary>玩家唯一 UID</summary>
        public ulong Uid { get; }

        /// <summary>玩家昵称</summary>
        public string Nickname { get; set; } = "未知";

        /// <summary>战力</summary>
        public int CombatPower { get; set; } = 0;

        /// <summary>职业</summary>
        public string Profession { get; set; } = "未知";

        #endregion

        #region 统计对象与索引

        /// <summary>玩家伤害统计</summary>
        public StatisticData DamageStats { get; } = new();

        /// <summary>玩家治疗统计</summary>
        public StatisticData HealingStats { get; } = new();

        /// <summary>玩家承受总伤害</summary>
        public ulong TakenDamage { get; private set; }

        /// <summary>按技能分组的伤害/治疗统计（key=技能ID）</summary>
        public Dictionary<ulong, StatisticData> SkillUsage { get; } = new();

        /// <summary>按技能分组的承伤统计（key=技能ID）</summary>
        public Dictionary<ulong, StatisticData> TakenDamageBySkill { get; } = new();

        // # 分类：按技能分组的治疗统计
        public Dictionary<ulong, StatisticData> HealingBySkill { get; } = new();

        #endregion

        #region 构造

        public PlayerData(ulong uid) => Uid = uid;

        #endregion

        #region 添加记录（伤害/治疗/承伤）

        /// <summary>
        /// 添加伤害记录，并同步更新技能分组统计
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
        // 新增重载：带 skillId 的治疗写入
        public void AddHealing(ulong skillId, ulong healing, bool isCrit, bool isLucky)
        {
            HealingStats.AddRecord(healing, isCrit, isLucky);

            if (!HealingBySkill.TryGetValue(skillId, out var stat))
            {
                stat = new StatisticData();
                HealingBySkill[skillId] = stat;
            }
            stat.AddRecord(healing, isCrit, isLucky);
        }

        /// <summary>
        /// 添加承伤记录（不区分暴击/幸运），并按技能累计
        /// </summary>
        public void AddTakenDamage(ulong skillId, ulong damage)
        {
            TakenDamage += damage;

            if (!TakenDamageBySkill.TryGetValue(skillId, out var stat))
            {
                stat = new StatisticData();
                TakenDamageBySkill[skillId] = stat;
            }

            // 承伤不区分暴击/幸运；HpLessen 填伤害值便于总量对齐
            stat.AddRecord(damage, isCrit: false, isLucky: false, hpLessenValue: damage);
        }

        /// <summary>
        /// 设置玩家职业
        /// </summary>
        public void SetProfession(string profession) => Profession = profession;

        #endregion

        #region 实时刷新与聚合输出

        /// <summary>刷新玩家的实时 DPS/HPS（滚动窗口）</summary>
        public void UpdateRealtimeStats()
        {
            DamageStats.UpdateRealtimeStats();
            HealingStats.UpdateRealtimeStats();
        }

        /// <summary>获取总 DPS（总时长平均）</summary>
        public double GetTotalDps() => DamageStats.GetTotalPerSecond();

        /// <summary>获取总 HPS（总时长平均）</summary>
        public double GetTotalHps() => HealingStats.GetTotalPerSecond();

        /// <summary>获取合并后的命中次数统计（伤害+治疗）</summary>
        public (int Normal, int Critical, int Lucky, int Total) GetTotalCount()
            => (
                DamageStats.CountNormal + HealingStats.CountNormal,
                DamageStats.CountCritical + HealingStats.CountCritical,
                DamageStats.CountLucky + HealingStats.CountLucky,
                DamageStats.CountTotal + HealingStats.CountTotal
            );

        /// <summary>
    /// 获取技能统计汇总列表（可选排序和限制数量）
    /// </summary>
    /// <param name="topN">
    ///     仅返回前 N 条记录（按总伤害/治疗排序后取前 N 条）。
    ///     - 传 null 或 <= 0 表示返回全部技能。
    /// </param>
    /// <param name="orderByTotalDesc">
    ///     是否按总量降序排序（true = 从大到小，false = 按原顺序）。
    /// </param>
    /// <param name="filterType">
    ///     过滤技能类型：
    /// —     <list type = "bullet" >
    ///         <item><description><see cref="SkillType.Damage"/> = 仅统计伤害技能</description></item>
    ///         <item><description><see cref="SkillType.Heal"/>   = 仅统计治疗技能</description></item>
    ///         <item><description>null = 暂时等同于伤害技能（如需合并伤害+治疗可扩展）</description></item>
    ///     </list>
    /// </param>
    /// <returns>技能汇总信息列表，每项包含总量、次数、暴击率、幸运率、占比等数据</returns>
        public List<SkillSummary> GetSkillSummaries(
            int? topN = null,
            bool orderByTotalDesc = true,
            StarResonanceDpsAnalysis.Core.SkillType? filterType = StarResonanceDpsAnalysis.Core.SkillType.Damage)
        {
            // 1) 选择数据源
            IEnumerable<KeyValuePair<ulong, StatisticData>> source;
            if (filterType == StarResonanceDpsAnalysis.Core.SkillType.Damage)
                source = SkillUsage;                  // 按技能统计的伤害
            else if (filterType == StarResonanceDpsAnalysis.Core.SkillType.Heal)
                source = HealingBySkill;              // 按技能统计的治疗（你已增加该字典/写入）
            else
                source = SkillUsage;                  // 先用伤害；如需真的“合并伤害+治疗”，我再给你 Merge 版

            // 2) 分母：必须用“source”的 Total 求和（避免用错集合）
            ulong denom = 0;
            foreach (var kv in source) denom += kv.Value.Total;
            if (denom == 0) denom = 1; // 防止除0

            // 3) 生成列表
            var list = new List<SkillSummary>();
            foreach (var kv in source)
            {
                var id = kv.Key;
                var s = kv.Value;
                var meta = SkillBook.Get(id);

                list.Add(new SkillSummary
                {
                    SkillId = id,
                    SkillName = meta.Name,
                    Total = s.Total,
                    HitCount = s.CountTotal,
                    AvgPerHit = s.GetAveragePerHit(),
                    CritRate = s.GetCritRate(),
                    LuckyRate = s.GetLuckyRate(),
                    MaxSingleHit = s.MaxSingleHit,
                    MinSingleHit = s.MinSingleHit == ulong.MaxValue ? 0 : s.MinSingleHit,
                    RealtimeValue = s.RealtimeValue,
                    RealtimeMax = s.RealtimeMax,
                    TotalDps = s.GetTotalPerSecond(),
                    LastTime = s.LastRecordTime,     // 如果你没加这个属性，就先删这一行
                    ShareOfTotal = (double)s.Total / denom  // 0~1 占比（与 source 对齐）
                });
            }

            // 4) 排序/截断
            if (orderByTotalDesc) list = list.OrderByDescending(x => x.Total).ToList();
            if (topN.HasValue && topN.Value > 0 && list.Count > topN.Value)
                list = list.Take(topN.Value).ToList();

            return list;
        }


        /// <summary>
        /// 技能占比（实时窗口）：返回 TopN + 其他 的占比（用于饼图/环图）
        /// </summary>
        /// <param name="topN">Top N 技能</param>
        /// <param name="includeOthers">是否包含“其他”汇总</param>
        /// <returns>(SkillId, SkillName, Realtime, Percent)</returns>
        public List<(ulong SkillId, string SkillName, ulong Realtime, int Percent)> GetSkillDamageShareRealtime(int topN = 10, bool includeOthers = true)
        {
            if (SkillUsage.Count == 0) return new List<(ulong, string, ulong, int)>();

            // 分母：实时窗口内的伤害
            ulong denom = 0;
            foreach (var kv in SkillUsage) denom += kv.Value.RealtimeValue;
            if (denom == 0) return new List<(ulong, string, ulong, int)>();

            var top = SkillUsage
                .Select(kv => new { kv.Key, Val = kv.Value.RealtimeValue })
                .OrderByDescending(x => x.Val)
                .ToList();

            var chosen = top.Take(topN).ToList();
            ulong chosenSum = 0;
            foreach (var c in chosen) chosenSum += c.Val;

            var result = new List<(ulong, string, ulong, int)>(chosen.Count + 1);
            foreach (var c in chosen)
            {
                double r = (double)c.Val / denom;
                int p = (int)Math.Round(r * 100.0);
                var name = SkillBook.Get(c.Key).Name;
                result.Add((c.Key, name, c.Val, p));
            }

            if (includeOthers && top.Count > chosen.Count)
            {
                ulong others = denom - chosenSum;
                int p = (int)Math.Round((double)others / denom * 100.0);
                result.Add((0, "其他", others, p));
            }

            return result;
        }

        /// <summary>重置玩家所有统计与状态</summary>
        public void Reset()
        {
            DamageStats.Reset();
            HealingStats.Reset();
            TakenDamage = 0;
            Profession = "未知";
            SkillUsage.Clear();
            TakenDamageBySkill.Clear();
        }

        #endregion

        #region 技能占比
        // ================================
        // # 分类：技能占比（整场总伤害）- 单玩家
        // ================================
        public List<(ulong SkillId, string SkillName, ulong Total, int Percent)>
            GetSkillDamageShareTotal(int topN = 10, bool includeOthers = true)
        {
            if (SkillUsage.Count == 0) return new();

            // 1) 分母：该玩家所有技能的【总伤害】求和
            ulong denom = 0;
            foreach (var kv in SkillUsage) denom += kv.Value.Total;
            if (denom == 0) return new();

            // 2) 按总伤害降序取 TopN
            var top = SkillUsage
                .Select(kv => new { kv.Key, Val = kv.Value.Total })
                .OrderByDescending(x => x.Val)
                .ToList();

            var chosen = top.Take(topN).ToList();
            ulong chosenSum = 0;
            foreach (var c in chosen) chosenSum += c.Val;

            // 3) 组装结果（百分比四舍五入为整数）
            var result = new List<(ulong SkillId, string SkillName, ulong Total, int Percent)>(chosen.Count + 1);
            foreach (var c in chosen)
            {
                double r = (double)c.Val / denom;
                int p = (int)Math.Round(r * 100.0);
                var name = SkillBook.Get(c.Key).Name;
                result.Add((c.Key, name, c.Val, p));
            }

            // 4) 其余汇总为“其他”
            if (includeOthers && top.Count > chosen.Count)
            {
                ulong others = denom - chosenSum;
                int p = (int)Math.Round((double)others / denom * 100.0);
                result.Add((0, "其他", others, p));
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// 玩家数据管理器：负责玩家对象创建/缓存、批量实时刷新与外部属性同步
    /// </summary>
    public class PlayerDataManager
    {
        #region 存储

        /// <summary>UID → 玩家数据</summary>
        private readonly Dictionary<ulong, PlayerData> _players = new();

        /// <summary>UID → 昵称（外部同步缓存）</summary>
        private static readonly Dictionary<ulong, string> _nicknameRequestedUids = new();

        /// <summary>UID → 战力（外部同步缓存）</summary>
        private static readonly Dictionary<ulong, int> _combatPowerByUid = new();

        /// <summary>UID → 职业（外部同步缓存）</summary>
        private static readonly Dictionary<ulong, string> _professionByUid = new();

        /// <summary>整场战斗开始时间（第一次出现战斗事件时赋值）</summary>
        private DateTime? _combatStart;

        /// <summary>整场战斗结束时间（手动结束后赋值；进行中则为 null）</summary>
        private DateTime? _combatEnd;

        /// <summary>是否处于战斗中</summary>
        public bool IsInCombat => _combatStart.HasValue && !_combatEnd.HasValue;
        #endregion

        #region 定时器

        /// <summary>用于周期性刷新实时统计的计时器（默认 1 秒）</summary>
        private readonly System.Timers.Timer _checkTimer;

        /// <summary>最近一次新增玩家时间（用于快速跳过无数据场景）</summary>
        private DateTime _lastAddTime = DateTime.MinValue;

        #endregion
        #region 全局战斗时间
        /// <summary>
        /// 标记一次战斗活动（任一伤害/治疗/承伤写入时调用）：
        /// - 若尚未开始：设置开始时间为当前
        /// - 若已结束：视为新一场，重置并重新开始
        /// </summary>
        private void MarkCombatActivity()
        {
            var now = DateTime.Now;
            if (!_combatStart.HasValue || _combatEnd.HasValue)
            {
                // 重新开一场
                _combatStart = now;
                _combatEnd = null;
            }
            // 进行中则不动结束时间
        }

        /// <summary>
        /// 手动结束整场战斗（设置结束时间）
        /// </summary>
        public void EndCombat()
        {
            if (_combatStart.HasValue && !_combatEnd.HasValue)
                _combatEnd = DateTime.Now;
        }

        /// <summary>
        /// 清除战斗时间（仅计时，不清玩家数据）
        /// </summary>
        public void ResetCombatClock()
        {
            _combatStart = null;
            _combatEnd = null;
        }

        /// <summary>
        /// 获取整场战斗持续时间：
        /// - 未开始：00:00:00
        /// - 进行中：now - start
        /// - 已结束：end - start
        /// </summary>
        public TimeSpan GetCombatDuration()
        {
            if (!_combatStart.HasValue) return TimeSpan.Zero;
            if (_combatEnd.HasValue) return _combatEnd.Value - _combatStart.Value;
            return DateTime.Now - _combatStart.Value;
        }

        #endregion

        #region 构造

        /// <summary>
        /// 构造函数：启动定时器，按固定频率刷新实时统计
        /// </summary>
        public PlayerDataManager()
        {
            _checkTimer = new System.Timers.Timer(1000)
            {
                AutoReset = true,
                Enabled = true
            };
            _checkTimer.Elapsed += CheckTimerElapsed;
        }

        #endregion

        #region 获取或创建

        /// <summary>
        /// 获取或创建指定 UID 的玩家数据，并套用已缓存的昵称/战力/职业
        /// </summary>
        public PlayerData GetOrCreate(ulong uid)
        {
            if (!_players.TryGetValue(uid, out var data))
            {
                data = new PlayerData(uid);
                _players[uid] = data;

                _lastAddTime = DateTime.Now;

                // 预填缓存信息（若已存在）
                if (_nicknameRequestedUids.TryGetValue(uid, out var cachedName) &&
                    !string.IsNullOrWhiteSpace(cachedName))
                {
                    data.Nickname = cachedName;
                }

                if (_combatPowerByUid.TryGetValue(uid, out var power) && power > 0)
                {
                    data.CombatPower = power;
                }

                if (_professionByUid.TryGetValue(uid, out var profession) &&
                    !string.IsNullOrWhiteSpace(profession))
                {
                    data.Profession = profession;
                }
            }

            return data;
        }

        #endregion

        #region 定时循环

        /// <summary>
        /// 定时器回调：刷新所有玩家的实时统计（保持轻量）
        /// </summary>
        private async void CheckTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_lastAddTime == DateTime.MinValue || _players.Count == 0)
                return;

            UpdateAllRealtimeStats();
            await System.Threading.Tasks.Task.CompletedTask;
        }

        #endregion

        #region 全局写入（转发至 PlayerData）

        /// <summary>添加全局伤害记录</summary>
        public void AddDamage(ulong uid, ulong skillId, ulong damage, bool isCrit, bool isLucky, ulong hpLessen = 0)
        {
            // # 分类：进入战斗（自动）
            MarkCombatActivity();
            GetOrCreate(uid).AddDamage(skillId, damage, isCrit, isLucky, hpLessen);
        }

        /// <summary>添加全局治疗记录</summary>
        public void AddHealing(ulong uid, ulong skillId, ulong healing, bool isCrit, bool isLucky)
        => GetOrCreate(uid).AddHealing(skillId, healing, isCrit, isLucky);


        /// <summary>添加全局承伤记录</summary>
        public void AddTakenDamage(ulong uid, ulong skillId, ulong damage)
        {
            // # 分类：进入战斗（自动）
            MarkCombatActivity();
            GetOrCreate(uid).AddTakenDamage(skillId, damage);
        }

        /// <summary>设置玩家职业（缓存 + 实例）</summary>
        public void SetProfession(ulong uid, string profession)
        {
            _professionByUid[uid] = profession;
            GetOrCreate(uid).SetProfession(profession);
        }

        /// <summary>设置玩家战力（缓存 + 实例）</summary>
        public void SetCombatPower(ulong uid, int combatPower)
        {
            _combatPowerByUid[uid] = combatPower;
            GetOrCreate(uid).CombatPower = combatPower;
        }

        /// <summary>设置玩家昵称（缓存 + 实例）</summary>
        public void SetNickname(ulong uid, string nickname)
        {
            _nicknameRequestedUids[uid] = nickname;
            GetOrCreate(uid).Nickname = nickname;
        }

        #endregion

        #region 批量与查询

        /// <summary>刷新所有玩家的实时统计（滚动窗口）</summary>
        public void UpdateAllRealtimeStats()
        {
            if (_players.Count == 0) return;
            foreach (var player in _players.Values)
                player?.UpdateRealtimeStats();
        }

        /// <summary>获取所有玩家数据对象</summary>
        public IEnumerable<PlayerData> GetAllPlayers() => _players.Values;

        /// <summary>清空所有玩家数据（缓存仍保留）</summary>
        public void ClearAll()
        {

            _players.Clear();
             ResetCombatClock();

        }

        /// <summary>获取所有玩家 UID</summary>
        public IEnumerable<ulong> GetAllUids() => _players.Keys;

        /// <summary>
        /// 获取“全队 Top 技能”（按总伤害聚合）
        /// </summary>
        public List<TeamSkillSummary> GetTeamTopSkillsByTotal(int topN = 20)
        {
            // skillId -> (total, count)
            var agg = new Dictionary<ulong, (ulong total, int count)>();

            foreach (var p in _players.Values)
            {
                foreach (var kv in p.SkillUsage)
                {
                    if (!agg.TryGetValue(kv.Key, out var a))
                        agg[kv.Key] = (kv.Value.Total, kv.Value.CountTotal);
                    else
                        agg[kv.Key] = (a.total + kv.Value.Total, a.count + kv.Value.CountTotal);
                }
            }

            return agg
                .OrderByDescending(x => x.Value.total)
                .Take(topN)
                .Select(x => new TeamSkillSummary
                {
                    SkillId = x.Key,
                    SkillName = SkillBook.Get(x.Key).Name,
                    Total = x.Value.total,
                    HitCount = x.Value.count
                })
                .ToList();
        }

        /// <summary>
        /// # 分类：按玩家获取技能明细列表
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="topN"></param>
        /// <param name="orderByTotalDesc"></param>
        /// <returns></returns>
        /// <summary>
        /// 按玩家获取技能明细列表（支持按技能类型过滤）
        /// </summary>
        public List<SkillSummary> GetPlayerSkillSummaries(
            ulong uid,
            int? topN = null,
            bool orderByTotalDesc = true,
            StarResonanceDpsAnalysis.Core.SkillType? filterType = StarResonanceDpsAnalysis.Core.SkillType.Damage)
        {
            var p = GetOrCreate(uid);
            return p.GetSkillSummaries(topN, orderByTotalDesc, filterType);
        }


        /// <summary>
        /// 分类：按玩家获取实时技能占比（TopN + 其他）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="topN"></param>
        /// <param name="includeOthers"></param>
        /// <returns></returns>
        public List<(ulong SkillId, string SkillName, ulong Realtime, int Percent)>
            GetPlayerSkillShareRealtime(ulong uid, int topN = 10, bool includeOthers = true)
        {
            var p = GetOrCreate(uid);
            return p.GetSkillDamageShareRealtime(topN, includeOthers);
        }


        /// <summary>
        /// 分类：按玩家 + 技能ID 获取单条技能详情
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public SkillSummary? GetPlayerSkillDetail(ulong uid, ulong skillId)
        {
            var p = GetOrCreate(uid);
            if (!p.SkillUsage.TryGetValue(skillId, out var stat))
                return null;

            var meta = SkillBook.Get(skillId);
            return new SkillSummary
            {
                SkillId = skillId,
                SkillName = meta.Name,
                Total = stat.Total,
                HitCount = stat.CountTotal,
                AvgPerHit = stat.GetAveragePerHit(),
                CritRate = stat.GetCritRate(),
                LuckyRate = stat.GetLuckyRate(),
                MaxSingleHit = stat.MaxSingleHit,
                MinSingleHit = stat.MinSingleHit == ulong.MaxValue ? 0 : stat.MinSingleHit,
                RealtimeValue = stat.RealtimeValue,
                RealtimeMax = stat.RealtimeMax,
                TotalDps = stat.GetTotalPerSecond(),
                LastTime = stat.LastRecordTime
            };
        }


        /// <summary>
        /// # 分类：技能占比（整场总伤害）- 全队聚合
        /// </summary>
        /// <param name="topN"></param>
        /// <param name="includeOthers"></param>
        /// <returns></returns>
        public List<(ulong SkillId, string SkillName, ulong Total, int Percent)>
            GetTeamSkillDamageShareTotal(int topN = 10, bool includeOthers = true)
        {
            // 1) 聚合全队：skillId -> totalDamage
            var agg = new Dictionary<ulong, ulong>();
            foreach (var p in _players.Values)
            {
                foreach (var kv in p.SkillUsage)
                {
                    ulong sid = kv.Key;
                    ulong val = kv.Value.Total;
                    if (val == 0) continue;

                    if (agg.TryGetValue(sid, out var old))
                        agg[sid] = old + val;
                    else
                        agg[sid] = val;
                }
            }
            if (agg.Count == 0) return new();

            // 2) 分母：全队总伤害
            ulong denom = 0;
            foreach (var v in agg.Values) denom += v;
            if (denom == 0) return new();

            // 3) 排序并取 TopN
            var top = agg
                .Select(kv => new { kv.Key, Val = kv.Value })
                .OrderByDescending(x => x.Val)
                .ToList();

            var chosen = top.Take(topN).ToList();
            ulong chosenSum = 0;
            foreach (var c in chosen) chosenSum += c.Val;

            // 4) 组装结果
            var result = new List<(ulong SkillId, string SkillName, ulong Total, int Percent)>(chosen.Count + 1);
            foreach (var c in chosen)
            {
                double r = (double)c.Val / denom;
                int p = (int)Math.Round(r * 100.0);
                var name = SkillBook.Get(c.Key).Name;
                result.Add((c.Key, name, c.Val, p));
            }

            if (includeOthers && top.Count > chosen.Count)
            {
                ulong others = denom - chosenSum;
                int p = (int)Math.Round((double)others / denom * 100.0);
                result.Add((0, "其他", others, p));
            }

            return result;
        }


        /// <summary>
        /// 根据UID获取玩家昵称 战力 职业
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public (string Nickname, int CombatPower, string Profession) GetPlayerBasicInfo(ulong uid)
        {
            // 先查已创建的 PlayerData
            if (_players.TryGetValue(uid, out var player))
            {
                return (player.Nickname, player.CombatPower, player.Profession);
            }

            // 没有 PlayerData，则用缓存字典
            string nickname = _nicknameRequestedUids.TryGetValue(uid, out var name) ? name : "未知";
            int combatPower = _combatPowerByUid.TryGetValue(uid, out var power) ? power : 0;
            string profession = _professionByUid.TryGetValue(uid, out var prof) ? prof : "未知";

            return (nickname, combatPower, profession);
        }

        /// <summary>
        /// 根据玩家UID获取完整统计信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public (ulong Uid, string Nickname, int CombatPower, string Profession,
        ulong TotalDamage, double CritRate, double LuckyRate,
        ulong MaxSingleHit, ulong MinSingleHit,
        ulong RealtimeDps, ulong RealtimeDpsMax,
        double TotalDps, ulong TotalHealing, double TotalHps,
        ulong TakenDamage, DateTime? LastRecordTime)
        GetPlayerFullStats(ulong uid)
        {
            if (!_players.TryGetValue(uid, out var p))
                return (uid, "未知", 0, "未知", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null);

            var dmg = p.DamageStats;
            var heal = p.HealingStats;

            return (
                p.Uid,
                p.Nickname,
                p.CombatPower,
                p.Profession,

                dmg.Total,
                dmg.GetCritRate(),
                dmg.GetLuckyRate(),

                dmg.MaxSingleHit,
                dmg.MinSingleHit == ulong.MaxValue ? 0 : dmg.MinSingleHit,

                dmg.RealtimeValue,
                dmg.RealtimeMax,

                p.GetTotalDps(),
                heal.Total,
                p.GetTotalHps(),

                p.TakenDamage,
                dmg.LastRecordTime
            );
        }


        #endregion
    }
}
