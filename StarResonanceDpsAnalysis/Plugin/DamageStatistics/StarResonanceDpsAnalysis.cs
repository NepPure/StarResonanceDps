namespace StarResonanceDpsAnalysis.Plugin.DamageStatistics
{
    /// <summary>
    /// 全程记录（跨战斗，会话级）：通过在 AddDamage/AddHealing/AddTakenDamage 内部打点，实时累加
    /// - Start(): 开启记录（不会被 ClearAll 清掉）
    /// - Stop(): 关闭记录（保留数据，可随时快照）
    /// - Reset(): 手动清空本会话
    /// - TakeSnapshot(): 生成“全程快照”（含玩家聚合与技能明细）
    /// - GetTeamDps()/GetPlayerDps(): 全程秒伤
    /// </summary>
    public static class FullRecord
    {
        // # 导航 / 分类索引（仅注释，不影响代码）：
        // #   1) 通用工具与数值格式: R2()
        // #   2) Shim 只读外观（与 StatisticData 口径对齐）: Shim.StatsLike / Shim.PlayerLike / Shim.TakenOverviewLike
        // #   3) UI 视图投影: StatView / ToView() / MergeStats()
        // #   4) 对外统计查询（与 StatisticData 一致口径）: GetPlayerDamageStats/HealingStats/TakenStats
        // #   5) 会话状态与控制: IsRecording/StartedAt/EndedAt + Start/Stop/Reset/GetSessionTotalTimeSpan
        // #   6) 快照入口与历史: TakeSnapshot / SessionHistory / 内部 StopInternal/EffectiveEndTime
        // #   7) 写入点（由解码管线调用）: RecordDamage/RecordHealing/RecordTakenDamage + UpdateRealtimeDps
        // #   8) 快照 & 秒伤对外接口: GetPlayersWithTotals/GetPlayersWithTotalsArray/GetTeamDps/GetPlayerDps 等
        // #   9) 快照时间检索: GetAllPlayersDataBySnapshotTime/GetPlayerSkillsBySnapshotTime
        // #  10) 内部实现工具: SessionSeconds/GetOrCreate/Accumulate/ToSkillSummary
        // #  11) 内部数据结构: PlayerAcc / StatAcc

        // ======================================================================
        // # 分类 1：通用工具与数值格式
        // ======================================================================

        // # 通用：两位小数四舍五入（远离零）
        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        // ======================================================================
        // # 分类 2：Shim 只读外观（对齐 StatisticData 接口口径，便于 UI 复用）
        // ======================================================================
        public static class Shim
        {
            // # —— 与 PlayerData.*Stats 口径一致的“只读统计对象” —— 
            // # 用于向上层提供只读统计视图，避免直接暴露内部累加器
            public sealed class StatsLike
            {
                public ulong Total, Normal, Critical, Lucky;
                public int CountTotal, CountNormal, CountCritical, CountLucky;
                public ulong MaxSingleHit, MinSingleHit; // Min=0 表示无记录
                public double ActiveSeconds;             // 用于计算 Dps/Hps

                public double GetAveragePerHit() => CountTotal > 0 ? R2((double)Total / CountTotal) : 0.0;
                public double GetCritRate() => CountTotal > 0 ? R2((double)CountCritical * 100.0 / CountTotal) : 0.0;
                public double GetLuckyRate() => CountTotal > 0 ? R2((double)CountLucky * 100.0 / CountTotal) : 0.0;
            }

            // # —— 与 StatisticData._manager.GetOrCreate(uid) 返回的“p”相似的外观 —— 
            // # 上层可像使用当前战斗统计那样读“全程统计”
            public sealed class PlayerLike
            {
                public StatsLike DamageStats { get; init; } = new();
                public StatsLike HealingStats { get; init; } = new();
                public StatsLike TakenStats { get; init; } = new();

                public double GetTotalDps() => DamageStats.ActiveSeconds > 0 ? R2(DamageStats.Total / DamageStats.ActiveSeconds) : 0.0;
                public double GetTotalHps() => HealingStats.ActiveSeconds > 0 ? R2(HealingStats.Total / HealingStats.ActiveSeconds) : 0.0;
            }

            // # 承伤总览（用于 UI 顶部概览）
            public sealed class TakenOverviewLike
            {
                public ulong Total { get; init; }
                public double AvgTakenPerSec { get; init; }
                public ulong MaxSingleHit { get; init; }
                public ulong MinSingleHit { get; init; }
            }

            // # 内部：将累加器转换为只读 StatsLike
            private static StatsLike From(StatAcc s)
            {
                // # 将内部累加器 StatAcc 投影为只读 StatsLike，供 UI/外部展示
                return new StatsLike
                {
                    Total = s.Total,
                    Normal = s.Normal,
                    Critical = s.Critical,
                    Lucky = s.Lucky,
                    CountTotal = s.CountTotal,
                    CountNormal = s.CountNormal,
                    CountCritical = s.CountCritical,
                    CountLucky = s.CountLucky,
                    MaxSingleHit = s.MaxSingleHit,
                    MinSingleHit = s.MinSingleHit, // 0 代表没记录
                    ActiveSeconds = s.ActiveSeconds
                };
            }

            // # 内部：聚合一组统计（常用于承伤按技能 → 玩家层合并）
            private static StatAcc MergeStats(IEnumerable<StatAcc> items)
            {
                // # 聚合多项 StatAcc：用于把逐技能合并为玩家层（承伤等）
                var acc = new StatAcc();
                ulong min = 0; bool hasMin = false;
                double maxActiveSecs = 0;

                foreach (var s in items)
                {
                    acc.Total += s.Total;
                    acc.Normal += s.Normal;
                    acc.Critical += s.Critical;
                    acc.Lucky += s.Lucky;
                    acc.CritLucky += s.CritLucky;
                    acc.HpLessen += s.HpLessen;

                    acc.CountNormal += s.CountNormal;
                    acc.CountCritical += s.CountCritical;
                    acc.CountLucky += s.CountLucky;
                    acc.CountTotal += s.CountTotal;

                    if (s.MaxSingleHit > acc.MaxSingleHit) acc.MaxSingleHit = s.MaxSingleHit;
                    if (s.MinSingleHit > 0 && (!hasMin || s.MinSingleHit < min)) { min = s.MinSingleHit; hasMin = true; }
                    if (s.ActiveSeconds > maxActiveSecs) maxActiveSecs = s.ActiveSeconds;
                }

                acc.MinSingleHit = hasMin ? min : 0;
                acc.ActiveSeconds = maxActiveSecs; // 不相加，取最大活跃时长，避免夸大分母
                return acc;
            }

            /// <summary>
            /// # 获取“只读玩家视图”：从 FullRecord 内部数据投影为与 StatisticData 类似的结构
            /// </summary>
            public static PlayerLike GetOrCreate(ulong uid)
            {
                // # 以 FullRecord 的内部累加为来源，返回近似 StatisticData 的“只读外观”
                lock (_sync)
                {
                    if (!_players.TryGetValue(uid, out var p))
                        return new PlayerLike();

                    // Damage / Healing 直接来自 FullRecord 的玩家聚合器
                    var dmg = From(p.Damage);
                    var heal = From(p.Healing);

                    // Taken：按技能合并（若没有按技能承伤，则用 TakenDamage + 会话秒数兜底）
                    StatAcc takenAcc;
                    if (p.TakenSkills != null && p.TakenSkills.Count > 0)
                        takenAcc = MergeStats(p.TakenSkills.Values);
                    else
                        takenAcc = new StatAcc
                        {
                            Total = p.TakenDamage,
                            ActiveSeconds = Math.Max(0.0, GetSessionTotalTimeSpan().TotalSeconds)
                        };
                    var taken = From(takenAcc);

                    return new PlayerLike
                    {
                        DamageStats = dmg,
                        HealingStats = heal,
                        TakenStats = taken
                    };
                }
            }

            /// <summary>
            /// # 承伤总览（合计、每秒均值、最大/最小单次）
            /// </summary>
            public static TakenOverviewLike GetPlayerTakenOverview(ulong uid)
            {
                // # 承伤总览：总量/每秒均值/单击最大最小
                var p = GetOrCreate(uid);
                var t = p.TakenStats;
                double perSec = t.ActiveSeconds > 0 ? R2(t.Total / t.ActiveSeconds) : 0.0;

                return new TakenOverviewLike
                {
                    Total = t.Total,
                    AvgTakenPerSec = perSec,
                    MaxSingleHit = t.MaxSingleHit,
                    MinSingleHit = t.MinSingleHit
                };
            }
        }

        // ======================================================================
        // # 分类 3：UI 只读统计视图（StatView 映射与合并工具）
        // ======================================================================

        // # === UI 只读统计视图 ===
        public readonly record struct StatView(
            ulong Total,
            ulong Normal,
            ulong Critical,
            ulong Lucky,
            int CountTotal,
            int CountNormal,
            int CountCritical,
            int CountLucky,
            ulong MaxSingleHit,
            ulong MinSingleHit,
            double PerSecond,      // = Total / ActiveSeconds(>0 ?)
            double AveragePerHit,  // = Total / CountTotal(>0 ?)
            double CritRate,       // %，两位小数
            double LuckyRate       // %
        );

        // # 将内部累加器 → UI 展示用视图
        private static StatView ToView(StatAcc s)
        {
            // # 将内部累加器映射为 UI 展示用视图（带每秒/均伤/暴击率/幸运率）
            int ct = s.CountTotal;
            double secs = s.ActiveSeconds > 0 ? s.ActiveSeconds : 0;
            double perSec = secs > 0 ? R2(s.Total / secs) : 0;
            double avg = ct > 0 ? R2((double)s.Total / ct) : 0;
            double crit = ct > 0 ? R2((double)s.CountCritical * 100.0 / ct) : 0.0;
            double lucky = ct > 0 ? R2((double)s.CountLucky * 100.0 / ct) : 0.0;

            ulong min = s.MinSingleHit; // StatAcc 里 Min=0 表示未赋值，直接返回 0 即可

            return new StatView(
                Total: s.Total,
                Normal: s.Normal,
                Critical: s.Critical,
                Lucky: s.Lucky,
                CountTotal: s.CountTotal,
                CountNormal: s.CountNormal,
                CountCritical: s.CountCritical,
                CountLucky: s.CountLucky,
                MaxSingleHit: s.MaxSingleHit,
                MinSingleHit: min,
                PerSecond: perSec,
                AveragePerHit: avg,
                CritRate: crit,
                LuckyRate: lucky
            );
        }

        // # 合并一组 StatAcc（用于 Taken：把各技能承伤合成玩家总承伤视图）
        private static StatAcc MergeStats(IEnumerable<StatAcc> items)
        {
            var acc = new StatAcc();
            ulong min = 0;
            bool hasMin = false;
            double maxActiveSecs = 0;

            foreach (var s in items)
            {
                acc.Total += s.Total;
                acc.Normal += s.Normal;
                acc.Critical += s.Critical;
                acc.Lucky += s.Lucky;
                acc.CritLucky += s.CritLucky;
                acc.HpLessen += s.HpLessen;

                acc.CountNormal += s.CountNormal;
                acc.CountCritical += s.CountCritical;
                acc.CountLucky += s.CountLucky;
                acc.CountTotal += s.CountTotal;

                if (s.MaxSingleHit > acc.MaxSingleHit) acc.MaxSingleHit = s.MaxSingleHit;
                if (s.MinSingleHit > 0 && (!hasMin || s.MinSingleHit < min)) { min = s.MinSingleHit; hasMin = true; }

                if (s.ActiveSeconds > maxActiveSecs) maxActiveSecs = s.ActiveSeconds;
            }

            acc.MinSingleHit = hasMin ? min : 0;
            acc.ActiveSeconds = maxActiveSecs; // 取最大活跃秒数，避免相加放大
            return acc;
        }

        // ======================================================================
        // # 分类 4：对外统计查询（口径对齐 StatisticData）
        // ======================================================================

        /// <summary>获取玩家全程伤害统计（UI 视图口径）。</summary>
        public static StatView GetPlayerDamageStats(ulong uid)
        {
            lock (_sync)
            {
                if (_players.TryGetValue(uid, out var p))
                    return ToView(p.Damage);
                return default;
            }
        }

        /// <summary>获取玩家全程治疗统计（UI 视图口径）。</summary>
        public static StatView GetPlayerHealingStats(ulong uid)
        {
            lock (_sync)
            {
                if (_players.TryGetValue(uid, out var p))
                    return ToView(p.Healing);
                return default;
            }
        }

        /// <summary>获取玩家全程承伤统计（UI 视图口径）。</summary>
        public static StatView GetPlayerTakenStats(ulong uid)
        {
            lock (_sync)
            {
                if (_players.TryGetValue(uid, out var p))
                {
                    if (p.TakenSkills.Count > 0)
                        return ToView(MergeStats(p.TakenSkills.Values));

                    // 没有逐技能承伤明细时，至少返回 Total；秒数兜底用会话时长
                    var secs = GetSessionTotalTimeSpan().TotalSeconds; // 你已实现的会话秒数API
                    var fake = new StatAcc { Total = p.TakenDamage, ActiveSeconds = secs > 0 ? secs : 0 };
                    return ToView(fake);
                }
                return default;
            }
        }

        // ======================================================================
        // # 分类 5：对外绑定的行结构（列表项定义）
        // ======================================================================

        // # 用于对外绑定的行结构（可按需增删字段）
        public sealed record FullPlayerTotal(
                ulong Uid,
                string Nickname,
                int CombatPower,
                string Profession,
                ulong TotalDamage,
                ulong TotalHealing,
                ulong TakenDamage,
                double Dps,   // 全程秒伤（只算伤害）
                double Hps    // 全程秒疗
            );

        // ======================================================================
        // # 分类 6：会话状态与控制（启动/停止/重置/时长）
        // ======================================================================

        // # 会话状态字段 —— 记录当前是否在录制，以及开始/结束时间点
        public static bool IsRecording { get; private set; }
        public static DateTime? StartedAt { get; private set; }
        public static DateTime? EndedAt { get; private set; }

        // # 彻底取消“事件空闲期自动停止”机制：不再跟踪 LastEventAt / 不再使用定时器
        // # 保留占位但不再使用（如需可直接删除字段与引用）
        private static readonly bool DisableIdleAutoStop = true;

        // # 持久累加存储：跨战斗的全程聚合
        private static readonly Dictionary<ulong, PlayerAcc> _players = new();

        // # ★ 全程快照历史（Stop 或 自动停止时都会入栈）
        private static readonly List<FullSessionSnapshot> _sessionHistory = new();
        public static IReadOnlyList<FullSessionSnapshot> SessionHistory => _sessionHistory; // 只读暴露，便于 UI 历史查看

        // # —— 新增：实时队伍 DPS（便于 UI 显示）
        public static double TeamRealtimeDps { get; private set; }     // 基于“有效会话秒数”的实时队伍DPS（只算伤害）

        // # 区域：控制（启动/停止/重置） ------------------------------------------------------
        #region 控制

        /// <summary>
        /// 启动全程记录：
        /// - 若已在记录则直接返回；
        /// - 首次启动设置 StartedAt；
        /// - EndedAt 清空以表示进行中。
        /// </summary>
        public static void Start()
        {
            if (IsRecording) return;

            IsRecording = true;
            if (StartedAt is null) StartedAt = DateTime.Now; // 记录首次启动时间
            EndedAt = null;
        }

        private static readonly object _sync = new();

        /// <summary>
        /// 手动停止全程记录（不清空数据）：
        /// - 如在录制，生成一次会话快照；
        /// - 然后清空“当前会话”累计数据（历史快照保留）；
        /// - 重置时间基，准备新会话。
        /// </summary>
        public static void Stop()
        {
            lock (_sync)
            {
                // 1) 若在录制中，先入快照（保留历史）
                if (IsRecording)
                    StopInternal(auto: false);

                // 2) 清【当前会话】累计（不动历史）
                _players.Clear();
                TeamRealtimeDps = 0;

                // 3) 重置时间基，准备新会话
                StartedAt = null;
                EndedAt = null;
            }
        }

        /// <summary>
        /// 清空快照
        /// </summary>
        public static void ClearSessionHistory()
        {
            lock (_sync)
            {
                _sessionHistory.Clear();
            }
        }



        /// <summary>
        /// 重置当前会话：
        /// - 如有进行中的或已有数据的会话，先保存一次快照；
        /// - 清除当前会话累计与时间基；
        /// - IsRecording 置为 true（进入新会话录制状态）。
        /// </summary>
        public static void Reset(bool preserveHistory = true)
        {

            if (AppConfig.ClearPicture == 0 && preserveHistory) return;//如果是0，则不清除数据
            lock (_sync)
            {
                // 1) 如有进行中的或已有数据的会话，先入一条快照（不影响历史）
                bool hasData = _players.Count > 0 || StartedAt != null;
                if (hasData)
                {
                    // StopInternal: 固定 EndedAt，生成快照，加入 _sessionHistory
                    StopInternal(auto: false);
                }

                // 2) 清【当前会话】累计（不动历史，除非显式要求清）
                _players.Clear();
                TeamRealtimeDps = 0;
                // 3) 清时间基与录制状态
                StartedAt = DateTime.Now;   // 原来是 null
                EndedAt = null;
                IsRecording = true;

                // 4) 可选：清历史（当前保留）

            }
        }

        /// <summary>
        /// 获取当前会话总时长（TimeSpan）。
        /// - 进行中：Now - StartedAt
        /// - 已停止：EndedAt - StartedAt
        /// </summary>
        public static TimeSpan GetSessionTotalTimeSpan()
        {
            if (StartedAt is null) return TimeSpan.Zero;
            DateTime end = IsRecording ? DateTime.Now : (EndedAt ?? DateTime.Now);
            var duration = end - StartedAt.Value;
            return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        }

        /// <summary>
        /// 获取指定玩家的全程技能统计（基于当前时刻快照）。
        /// 返回三个只读列表（伤害技能/治疗技能/承伤技能）。
        /// </summary>
        public static (IReadOnlyList<SkillSummary> DamageSkills,
                       IReadOnlyList<SkillSummary> HealingSkills,
                       IReadOnlyList<SkillSummary> TakenSkills)
            GetPlayerSkills(ulong uid)
        {
            var snap = TakeSnapshot();
            if (snap.Players.TryGetValue(uid, out var p))
            {
                return (p.DamageSkills, p.HealingSkills, p.TakenSkills);
            }
            return (Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>());
        }

        #endregion

        // ======================================================================
        // # 分类 7：列表数据获取 / 汇总：用于 UI 绑定与展示
        // ======================================================================

        /// <summary>
        /// 获取“此刻”的全程逐玩家总量清单（默认按总伤害降序）。
        /// includeZero=false 时会过滤掉三项全为 0 的玩家。
        /// Dps/Hps 分母为各自“有效活跃秒数”（无活跃时回退会话时长）。
        /// </summary>
        public static List<FullPlayerTotal> GetPlayersWithTotals(bool includeZero = false)
        {
            var snap = TakeSnapshot();

            // 不再用 snap.Duration 作为统一分母
            var list = new List<FullPlayerTotal>(snap.Players.Count);
            foreach (var kv in snap.Players)
            {
                var p = kv.Value;

                // 各自有效分母（回退到会话时长以兜底）
                var secsDmg = p.ActiveSecondsDamage > 0 ? p.ActiveSecondsDamage : snap.Duration.TotalSeconds;
                var secsHeal = p.ActiveSecondsHealing > 0 ? p.ActiveSecondsHealing : snap.Duration.TotalSeconds;

                // includeZero 过滤逻辑保持不变
                if (!includeZero && p.TotalDamage == 0 && p.TotalHealing == 0 && p.TakenDamage == 0)
                    continue;

                list.Add(new FullPlayerTotal(
                    Uid: p.Uid,
                    Nickname: p.Nickname,
                    CombatPower: p.CombatPower,
                    Profession: p.Profession,
                    TotalDamage: p.TotalDamage,
                    TotalHealing: p.TotalHealing,
                    TakenDamage: p.TakenDamage,
                    Dps: secsDmg > 0 ? R2(p.TotalDamage / secsDmg) : 0,
                    Hps: secsHeal > 0 ? R2(p.TotalHealing / secsHeal) : 0
                ));
            }

            return list.OrderByDescending(r => r.TotalDamage).ToList();
        }

        /// <summary>
        /// 查看全程战斗时间（HH:mm:ss，基于 Damage 有效时长最大值）
        /// 用于 UI 文本显示。
        /// </summary>
        public static string GetEffectiveDurationString()
        {
            double activeSeconds = 0;

            // 取全队最大有效时长（Damage为主）
            foreach (var p in _players.Values)
            {
                if (p.Damage.ActiveSeconds > activeSeconds)
                    activeSeconds = p.Damage.ActiveSeconds;
            }

            var ts = TimeSpan.FromSeconds(activeSeconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        /// <summary>
        /// 便于外层直接 ToArray 绑定（示例对齐 StatisticData 用法）。
        /// </summary>
        public static FullPlayerTotal[] GetPlayersWithTotalsArray(bool includeZero = false)
            => GetPlayersWithTotals(includeZero).ToArray();

        // ======================================================================
        // # 分类 8：内部停止与有效结束时间（快照辅助）
        // ======================================================================

        // # 内部调用
        /// <summary>
        /// 内部停止封装：
        /// - auto=true 表示由空闲超时触发；
        /// - 固化 EndedAt；
        /// - 生成快照并写入历史。
        /// </summary>
        private static void StopInternal(bool auto)
        {
            IsRecording = false;
            EndedAt = DateTime.Now;

            // 有任何有效数据才写历史
            bool hasAnyData = _players.Values.Any(p =>
                p.Damage.Total > 0 || p.Healing.Total > 0 || p.TakenDamage > 0);

            if (!hasAnyData) return;            // <- 关键：没有数据就不入历史

            var snapshot = TakeSnapshot();
            // 也可以再加一道时长阈值
            if (snapshot.Duration.TotalSeconds >= 1 || hasAnyData)
                _sessionHistory.Add(snapshot);
        }

        // # 内部调用
        /// <summary>
        /// 获取“有效结束时间”：
        /// - 录制中：以 Now 为结束点；
        /// - 已停止：使用 EndedAt。
        /// </summary>
        private static DateTime EffectiveEndTime()
        {
            if (StartedAt is null) return DateTime.Now;
            return IsRecording ? DateTime.Now : (EndedAt ?? DateTime.Now);
        }

        // ======================================================================
        // # 分类 9：写入点（由外部解码/事件管线调用）
        // ======================================================================

        #region 内嵌写入点会调用的 API（只加一行即可）

        /// <summary>
        /// 记录伤害事件：
        /// - 聚合到玩家总伤害与对应技能；
        /// - 更新实时 DPS（基于伤害侧有效秒数）；
        /// - 忽略 0 值。
        /// </summary>
        public static void RecordDamage(
            ulong uid, ulong skillId, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
            string nickname, int combatPower, string profession)
        {
            if (!IsRecording || value == 0) return;

            var p = GetOrCreate(uid, nickname, combatPower, profession);

            // 顶层聚合
            Accumulate(p.Damage, value, isCrit, isLucky, hpLessen);

            // 逐技能
            var s = p.DamageSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.DamageSkills[skillId] = new StatAcc());
            Accumulate(s, value, isCrit, isLucky, hpLessen);

            // —— 更新实时DPS
            UpdateRealtimeDps(p);
        }

        /// <summary>
        /// 记录治疗事件：
        /// - 聚合到玩家总治疗与对应技能；
        /// - 更新“治疗侧”实时指标（RealtimeDpsHealing）；
        /// - 忽略 0 值。
        /// </summary>
        public static void RecordHealing(
            ulong uid, ulong skillId, ulong value, bool isCrit, bool isLucky,
            string nickname, int combatPower, string profession)
        {
            if (!IsRecording || value == 0) return;

            var p = GetOrCreate(uid, nickname, combatPower, profession);

            Accumulate(p.Healing, value, isCrit, isLucky, 0);
            var s = p.HealingSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.HealingSkills[skillId] = new StatAcc());
            Accumulate(s, value, isCrit, isLucky, 0);

            UpdateRealtimeDps(p);
        }

        /// <summary>
        /// 记录承伤事件：
        /// - 聚合承伤总量与逐技能承伤（hpLessen 写入）；
        /// - 不计入队伍/玩家DPS（仅用于受击统计与 UI 展示）；
        /// - 忽略 0 值。
        /// </summary>
        public static void RecordTakenDamage(
            ulong uid, ulong skillId, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
            string nickname, int combatPower, string profession)
        {
            if (!IsRecording || value == 0) return;

            var p = GetOrCreate(uid, nickname, combatPower, profession);

            // hpLessen 兜底：未传或为0时，用 value
            var lessen = hpLessen > 0 ? hpLessen : value;
            p.TakenDamage += lessen;

            var s = p.TakenSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.TakenSkills[skillId] = new StatAcc());
            // 承伤也记录暴击/幸运，并把 hpLessen 写入累加器
            Accumulate(s, value, isCrit: isCrit, isLucky: isLucky, hpLessen: lessen);

            // 承伤不参与队伍/玩家DPS（如需“受伤DPS”，可在此扩展）
            UpdateRealtimeDps(p, includeHealing: false);
        }

        /// <summary>
        /// 更新玩家与队伍的实时 DPS：
        /// - 玩家侧：Damage/Healing 分别按自身 ActiveSeconds 计算；
        /// - 技能侧：逐技能 RealtimeDps 按各自 ActiveSeconds；
        /// - 队伍侧：总伤害 / 全队最大“伤害活跃秒数”。
        /// </summary>
        private static void UpdateRealtimeDps(PlayerAcc p, bool includeHealing = true)
        {
            // 玩家聚合：按事件有效时长计算
            var dmgSecs = p.Damage.ActiveSeconds;
            p.RealtimeDpsDamage = dmgSecs > 0 ? R2(p.Damage.Total / dmgSecs) : 0;

            if (includeHealing)
            {
                var healSecs = p.Healing.ActiveSeconds;
                p.RealtimeDpsHealing = healSecs > 0 ? R2(p.Healing.Total / healSecs) : 0;
            }

            // 逐技能（可选：也按各自有效时长计算）
            foreach (var kv in p.DamageSkills)
            {
                var s = kv.Value;
                var secs = s.ActiveSeconds;
                s.RealtimeDps = secs > 0 ? R2(s.Total / secs) : 0;
            }
            if (includeHealing)
            {
                foreach (var kv in p.HealingSkills)
                {
                    var s = kv.Value;
                    var secs = s.ActiveSeconds;
                    s.RealtimeDps = secs > 0 ? R2(s.Total / secs) : 0;
                }
            }

            // 队伍实时DPS：用“全队有效时长（取最大）”更贴近“团队在打的时间”
            double teamActiveSecs = 0;
            foreach (var pp in _players.Values)
                teamActiveSecs = Math.Max(teamActiveSecs, pp.Damage.ActiveSeconds);

            ulong teamTotal = 0;
            foreach (var pp in _players.Values) teamTotal += pp.Damage.Total;

            TeamRealtimeDps = teamActiveSecs > 0 ? R2(teamTotal / teamActiveSecs) : 0;
        }

        #endregion

        // ======================================================================
        // # 分类 10：快照 & 秒伤（对外接口 / 快照产出）
        // ======================================================================

        /// <summary>
        /// 生成一次全程快照（当前时刻）：
        /// - 结束时间使用 EffectiveEndTime()；
        /// - 汇总队伍总伤害/治疗/承伤；
        /// - 逐玩家构建 SnapshotPlayer（含按伤害/治疗/承伤降序的技能汇总）。
        /// </summary>
        public static FullSessionSnapshot TakeSnapshot()
        {
            var end = EffectiveEndTime();
            var start = StartedAt ?? end;
            var duration = end - start;
            if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;

            ulong teamDmg = 0, teamHeal = 0, teamTaken = 0;   // ★ teamTaken 新增
            var players = new Dictionary<ulong, SnapshotPlayer>(_players.Count);

            foreach (var p in _players.Values)
            {
                teamDmg += p.Damage.Total;
                teamHeal += p.Healing.Total;
                teamTaken += p.TakenDamage;                   // ★ 新增

                var damageSkills = p.DamageSkills
                    .Select(kv => ToSkillSummary(kv.Key, kv.Value, duration))
                    .OrderByDescending(x => x.Total).ToList();

                var healingSkills = p.HealingSkills
                    .Select(kv => ToSkillSummary(kv.Key, kv.Value, duration))
                    .OrderByDescending(x => x.Total).ToList();

                // ★ 新增：承伤按技能
                var takenSkills = p.TakenSkills
                    .Select(kv => ToSkillSummary(kv.Key, kv.Value, duration))
                    .OrderByDescending(x => x.Total).ToList();

                players[p.Uid] = new SnapshotPlayer
                {
                    Uid = p.Uid,
                    Nickname = p.Nickname,
                    CombatPower = p.CombatPower,
                    Profession = p.Profession,

                    TotalDamage = p.Damage.Total,
                    TotalDps = p.Damage.ActiveSeconds > 0 ? R2(p.Damage.Total / p.Damage.ActiveSeconds) : 0,
                    TotalHps = p.Healing.ActiveSeconds > 0 ? R2(p.Healing.Total / p.Healing.ActiveSeconds) : 0,

                    TotalHealing = p.Healing.Total,
                    TakenDamage = p.TakenDamage,
                    LastRecordTime = null,
                    ActiveSecondsDamage = p.Damage.ActiveSeconds,
                    ActiveSecondsHealing = p.Healing.ActiveSeconds,
                    DamageSkills = damageSkills,
                    HealingSkills = healingSkills,
                    TakenSkills = takenSkills,          // ★ 新增
                    // 在 FullRecord.TakeSnapshot 的 players[p.Uid] = new SnapshotPlayer { ... } 中补充：
                    RealtimeDps = (ulong)Math.Round(p.RealtimeDpsDamage),

                    // —— 伤害侧细分与比率 —— 
                    CriticalDamage = p.Damage.Critical,
                    LuckyDamage = p.Damage.Lucky,
                    CritLuckyDamage = p.Damage.CritLucky,
                    MaxSingleHit = p.Damage.MaxSingleHit,
                    CritRate = p.Damage.CountTotal > 0 ? R2((double)p.Damage.CountCritical * 100.0 / p.Damage.CountTotal) : 0.0,
                    LuckyRate = p.Damage.CountTotal > 0 ? R2((double)p.Damage.CountLucky * 100.0 / p.Damage.CountTotal) : 0.0,

                    // —— 治疗侧细分与实时值 —— 
                    HealingCritical = p.Healing.Critical,
                    HealingLucky = p.Healing.Lucky,
                    HealingCritLucky = p.Healing.CritLucky,
                    HealingRealtime = (ulong)Math.Round(p.RealtimeDpsHealing),

                    // （可选）如果你想给“最大瞬时”也来个近似，可用逐技能的 RealtimeDps 最大值做个 UI 友好兜底：
                     RealtimeDpsMax     = (ulong)Math.Round(p.DamageSkills.Values.Select(s => s.RealtimeDps).DefaultIfEmpty(0).Max()),
                     HealingRealtimeMax = (ulong)Math.Round(p.HealingSkills.Values.Select(s => s.RealtimeDps).DefaultIfEmpty(0).Max()),

                };
            }

            return new FullSessionSnapshot
            {
                StartedAt = start,
                EndedAt = end,
                Duration = duration,
                TeamTotalDamage = teamDmg,
                TeamTotalHealing = teamHeal,
                TeamTotalTakenDamage = teamTaken,   // ★ 新增
                Players = players
            };
        }

        /// <summary>
        /// 获取队伍当前全程 DPS（只计算伤害）。
        /// - 取全队最大 Damage.ActiveSeconds 为分母；
        /// - 汇总总伤害为分子。
        /// </summary>
        public static double GetTeamDps()
        {
            lock (_sync)
            {
                double teamActiveSecs = 0;
                foreach (var p in _players.Values)
                    if (p.Damage.ActiveSeconds > teamActiveSecs)
                        teamActiveSecs = p.Damage.ActiveSeconds;

                if (teamActiveSecs <= 0) return 0.0;

                ulong total = 0;
                foreach (var p in _players.Values) total += p.Damage.Total;

                return R2(total / teamActiveSecs);
            }
        }

        /// <summary>
        /// 获取指定玩家当前全程 DPS（只计算伤害）。
        /// - 分母：整个会话秒数（SessionSeconds）；
        /// - 若尚未开始或分母为 0，返回 0。
        /// </summary>
        public static double GetPlayerDps(ulong uid)
        {
            var secs = SessionSeconds();
            if (secs <= 0) return 0;
            return _players.TryGetValue(uid, out var p) ? R2(p.Damage.Total / secs) : 0;
        }

        // using StarResonanceDpsAnalysis.Plugin.DamageStatistics; // 确保命名空间可见

        public static (IReadOnlyList<SkillSummary> DamageSkills,
                      IReadOnlyList<SkillSummary> HealingSkills,
                      IReadOnlyList<SkillSummary> TakenSkills)
        GetPlayerSkillsBySnapshotTimeEx(DateTime snapshotStartTime, ulong uid, double toleranceSeconds = 2.0)
        {
            // —— 先查【全程历史】——
            var session = SessionHistory?.FirstOrDefault(s =>
                s.StartedAt == snapshotStartTime ||
                Math.Abs((s.StartedAt - snapshotStartTime).TotalSeconds) <= toleranceSeconds);

            if (session != null && session.Players != null &&
                session.Players.TryGetValue(uid, out var sp1))
            {
                return (sp1.DamageSkills ?? new List<SkillSummary>(),
                        sp1.HealingSkills ?? new List<SkillSummary>(),
                        sp1.TakenSkills ?? new List<SkillSummary>());
            }

            // —— 再查【单场历史】——
            var battles = StatisticData._manager.History;
            var battle = battles?.FirstOrDefault(s =>
                s.StartedAt == snapshotStartTime ||
                Math.Abs((s.StartedAt - snapshotStartTime).TotalSeconds) <= toleranceSeconds);

            if (battle != null && battle.Players != null &&
                battle.Players.TryGetValue(uid, out var sp2))
            {
                return (sp2.DamageSkills ?? new List<SkillSummary>(),
                        sp2.HealingSkills ?? new List<SkillSummary>(),
                        sp2.TakenSkills ?? new List<SkillSummary>());
            }

            // —— 都没找到：返回空 —— 
            return (Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>());
        }



        // ======================================================================
        // # 分类 11：快照时间检索（历史查询）
        // ======================================================================

        #region 查询（按快照时间检索）
        /// <summary>
        /// 按快照的开始时间获取该快照中所有玩家数据。
        /// - 如果找不到对应快照，返回 null。
        /// </summary>
        public static IReadOnlyDictionary<ulong, SnapshotPlayer>? GetAllPlayersDataBySnapshotTime(DateTime snapshotStartTime)
        {
            var snapshot = SessionHistory.FirstOrDefault(s => s.StartedAt == snapshotStartTime);
            return snapshot?.Players;
        }

        /// <summary>
        /// 按快照的开始时间和玩家 UID 获取该玩家的技能数据。
        /// - 返回 (伤害技能, 治疗技能)，若找不到则返回两个空列表。
        /// </summary>
        public static (IReadOnlyList<SkillSummary> DamageSkills, IReadOnlyList<SkillSummary> HealingSkills)
            GetPlayerSkillsBySnapshotTime(DateTime snapshotStartTime, ulong uid)
        {
            var snapshot = SessionHistory.FirstOrDefault(s => s.StartedAt == snapshotStartTime);
            if (snapshot != null && snapshot.Players.TryGetValue(uid, out var player))
            {
                return (player.DamageSkills, player.HealingSkills);
            }
            return (Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>());
        }
        #endregion

        // ======================================================================
        // # 分类 12：内部实现工具（时长/取或建/累加/投影）
        // ======================================================================

        #region 内部实现

        /// <summary>
        /// 计算会话“有效秒数”：
        /// - 若尚未开始返回 0；
        /// - 进行中：StartedAt → Now；
        /// - 已停止：StartedAt → EndedAt。
        /// </summary>
        private static double SessionSeconds()
        {
            if (StartedAt is null) return 0;

            DateTime end = IsRecording
                ? DateTime.Now           // 进行中：用 Now 做临时结束点
                : (EndedAt ?? DateTime.Now);

            var sec = (end - StartedAt.Value).TotalSeconds;
            return sec > 0 ? sec : 0;
        }

        /// <summary>
        /// 获取或创建玩家累计器，并同步基础信息（昵称/战力/职业以最近一次为准）。
        /// </summary>
        private static PlayerAcc GetOrCreate(ulong uid, string nickname, int combatPower, string profession)
        {
            if (!_players.TryGetValue(uid, out var p))
            {
                p = new PlayerAcc(uid);
                _players[uid] = p;
            }
            // 以最近一次为准同步基础信息
            p.Nickname = nickname;
            p.CombatPower = combatPower;
            p.Profession = profession;
            return p;
        }

        /// <summary>
        /// 将一次数值累加到统计器：
        /// - 区分普通/暴击/幸运/暴击+幸运；
        /// - 维护总和、hpLessen、次数与最大/最小单次值；
        /// - 通过 FirstAt/LastAt 与时间差累加 ActiveSeconds（含空档封顶）。
        /// </summary>
        private static void Accumulate(StatAcc acc, ulong value, bool isCrit, bool isLucky, ulong hpLessen)
        {
            // 数值累计
            if (isCrit && isLucky) acc.CritLucky += value;
            else if (isCrit) acc.Critical += value;
            else if (isLucky) acc.Lucky += value;
            else acc.Normal += value;

            acc.Total += value;
            acc.HpLessen += hpLessen;

            // 次数
            if (isCrit) acc.CountCritical++;
            if (isLucky) acc.CountLucky++;
            if (!isCrit && !isLucky) acc.CountNormal++;
            acc.CountTotal++;

            // 极值
            if (value > 0)
            {
                if (value > acc.MaxSingleHit) acc.MaxSingleHit = value;
                if (acc.MinSingleHit == 0 || value < acc.MinSingleHit) acc.MinSingleHit = value;
            }
            var now = DateTime.Now;
            if (acc.FirstAt is null)
            {
                acc.FirstAt = now;
                // 第一条事件不增加时长
            }
            else
            {
                // 防止长空档把分母拉大：可按需要调整 1~5 秒；不想封顶就删掉这两行
                const double GAP_CAP_SECONDS = 3.0;

                var gap = (now - (acc.LastAt ?? acc.FirstAt.Value)).TotalSeconds;
                if (gap < 0) gap = 0;
                if (gap > GAP_CAP_SECONDS) gap = GAP_CAP_SECONDS;

                acc.ActiveSeconds += gap;
            }
            acc.LastAt = now;
        }

        /// <summary>
        /// 将内部技能统计转为快照中的技能汇总项（含DPS、命中均值、暴击/幸运率等）。
        /// - Realtime 字段快照中不赋值（保持 0）。
        /// </summary>
        private static SkillSummary ToSkillSummary(ulong skillId, StatAcc s, TimeSpan duration)
        {
            var meta = SkillBook.Get(skillId);
            return new SkillSummary
            {
                SkillId = skillId,
                SkillName = meta.Name,
                Total = s.Total,
                HitCount = s.CountTotal,
                AvgPerHit = s.CountTotal > 0 ? R2((double)s.Total / s.CountTotal) : 0.0,
                CritRate = s.CountTotal > 0 ? R2((double)s.CountCritical * 100.0 / s.CountTotal) : 0.0,
                LuckyRate = s.CountTotal > 0 ? R2((double)s.CountLucky * 100.0 / s.CountTotal) : 0.0,
                MaxSingleHit = s.MaxSingleHit,
                MinSingleHit = s.MinSingleHit,
                RealtimeValue = 0,          // 快照为历史静态值，这里不赋实时
                RealtimeMax = 0,            // 同上
                TotalDps = s.ActiveSeconds > 0 ? R2(s.Total / s.ActiveSeconds) : 0,
                LastTime = null,            // 可按需扩展：记录技能最后出现时间
                ShareOfTotal = 0            // 可按需扩展：占比（由外部渲染时计算亦可）
            };
        }

        // ===== 内部数据结构 =====

        /// <summary>
        /// 玩家聚合器（会话内持续累加）。
        /// - 含基础信息（昵称/战力/职业）与三类统计（伤害/治疗/承伤）；
        /// - DamageSkills/HealingSkills/TakenSkills 逐技能累加；
        /// - RealtimeDps* 用于 UI 实时显示。
        /// </summary>
        private sealed class PlayerAcc
        {
            public ulong Uid { get; }
            public string Nickname { get; set; } = "未知";
            public int CombatPower { get; set; }
            public string Profession { get; set; } = "未知";

            public StatAcc Damage { get; } = new();
            public StatAcc Healing { get; } = new();
            public ulong TakenDamage { get; set; }

            public Dictionary<ulong, StatAcc> DamageSkills { get; } = new();
            public Dictionary<ulong, StatAcc> HealingSkills { get; } = new();
            public Dictionary<ulong, StatAcc> TakenSkills { get; } = new();

            // —— 新增：实时总DPS（聚合）
            public double RealtimeDpsDamage { get; set; }
            public double RealtimeDpsHealing { get; set; }

            public PlayerAcc(ulong uid) => Uid = uid;
        }

        /// <summary>
        /// 统计累加器（通用结构）：
        /// - 四象限数值：Normal/Critical/Lucky/CritLucky；
        /// - 计数：Count*；
        /// - 极值：MaxSingleHit/MinSingleHit；
        /// - 时序：FirstAt/LastAt/ActiveSeconds；
        /// - RealtimeDps：便于 UI 实时显示（逐技能/逐类）。
        /// </summary>
        private sealed class StatAcc
        {
            public ulong Normal, Critical, Lucky, CritLucky, HpLessen, Total;
            public ulong MaxSingleHit, MinSingleHit; // Min=0 表示未赋值
            public int CountNormal, CountCritical, CountLucky, CountTotal;
            public DateTime? FirstAt;     // 第一条记录时间
            public DateTime? LastAt;      // 最近一条记录时间
            public double ActiveSeconds;  // 事件间隔累加（单位：秒）
            // —— 新增：实时DPS（逐技能/逐类）
            public double RealtimeDps { get; set; }
        }
        #endregion
    }

    // ======================================================================
    // # 分类 13：快照结构定义（跨战斗的全程快照）
    // ======================================================================

    /// <summary>全程快照结构（与 BattleSnapshot 类似，但跨战斗）。用于历史/统计展示。</summary>
    public sealed class FullSessionSnapshot
    {
        public DateTime StartedAt { get; init; }          // 快照起始时间
        public DateTime EndedAt { get; init; }            // 快照结束时间
        public TimeSpan Duration { get; init; }           // 持续时长
        public ulong TeamTotalDamage { get; init; }       // 队伍总伤害
        public ulong TeamTotalHealing { get; init; }      // 队伍总治疗
        public Dictionary<ulong, SnapshotPlayer> Players { get; init; } = new(); // 逐玩家详情
        public ulong TeamTotalTakenDamage { get; init; }  // ★ 队伍总承伤
    }
}
