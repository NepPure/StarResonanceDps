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
        // # 会话状态字段 —— 记录当前是否在录制，以及开始/结束时间点
        public static bool IsRecording { get; private set; }           // 正在记录的标记
        public static DateTime? StartedAt { get; private set; }        // 会话开始时间（首次 Start 时赋值）
        public static DateTime? EndedAt { get; private set; }          // 会话结束时间（Stop/自动停止时固定）

        // —— 新增：最近一次事件时间 & 空闲自动停止
        public static DateTime? LastEventAt { get; private set; }      // 最近一次记录事件（伤害/治疗/承伤）的时间戳

        /// <summary>空闲多少秒后自动停止（默认30，可由用户自定义）</summary>
        public static int InactivitySeconds { get; set; } = 30;        // 空闲超时阈值（秒），用户可配置

        private static System.Timers.Timer? _idleTimer;  // 1秒tick检查空闲（定时器：用于自动停止判断）

        // 持久累加存储：跨战斗的全程聚合
        private static readonly Dictionary<ulong, PlayerAcc> _players = new();

        // ★ 全程快照历史（Stop 或 自动停止时都会入栈）
        private static readonly List<FullSessionSnapshot> _sessionHistory = new();
        public static IReadOnlyList<FullSessionSnapshot> SessionHistory => _sessionHistory; // 只读暴露，便于 UI 历史查看

        // —— 新增：实时队伍 DPS（便于 UI 显示）
        public static double TeamRealtimeDps { get; private set; }     // 基于“有效会话秒数”的实时队伍DPS（只算伤害）

        // # 区域：控制（启动/停止/重置） ------------------------------------------------------
        #region 控制

        // # 外部调用
        /// <summary>
        /// 启动全程记录：
        /// - 若已在记录则直接返回；
        /// - 首次启动设置 StartedAt；
        /// - 开启空闲检测定时器，用于自动停止。
        /// </summary>
        public static void Start()
        {
            if (IsRecording) return;

            IsRecording = true;
            if (StartedAt is null) StartedAt = DateTime.Now; // 只在首次 Start 时设置开始时间
            EndedAt = null;
            LastEventAt = DateTime.Now;                      // 启动即刷新最近事件时间，避免立即被判空闲

            // 启动空闲检测定时器（1秒检查一次）
            _idleTimer ??= new System.Timers.Timer(1000);
            _idleTimer.Elapsed -= OnIdleTick;               // 确保不重复订阅
            _idleTimer.Elapsed += OnIdleTick;
            _idleTimer.AutoReset = true;
            _idleTimer.Enabled = true;
        }

        // # 外部调用
        /// <summary>
        /// 手动停止全程记录（不清空数据）：
        /// - 固化 EndedAt；
        /// - 生成并保存一次会话快照。
        /// </summary>
        public static void Stop()
        {
            if (!IsRecording) return;
            // 统一内部停止逻辑（手动停止，auto=false）
            StopInternal(auto: false);
        }

        // # 外部调用
        /// <summary>
        /// 重置当前会话：
        /// - 关闭定时器；
        /// - 清除状态与累计数据；
        /// - 可选择是否清空历史快照（此处清空，如需保留可删除相关行）。
        /// </summary>
        public static void Reset()
        {
            // 停掉计时器
            if (_idleTimer != null)
            {
                _idleTimer.Enabled = false;
                _idleTimer.Elapsed -= OnIdleTick;
            }

            IsRecording = false;
            StartedAt = null;
            EndedAt = null;
            LastEventAt = null;
            TeamRealtimeDps = 0;

            _players.Clear();
            _sessionHistory.Clear(); // 注意：如不想清历史，这行删掉
        }

        // # 内部调用
        /// <summary>
        /// 内部停止封装：
        /// - auto=true 表示由空闲超时触发；
        /// - 将 EndedAt 固定为 LastEventAt，排除空闲等待时间；
        /// - 生成快照并写入历史。
        /// </summary>
        private static void StopInternal(bool auto)
        {
            IsRecording = false;
            // 结束时间固定在最后一次战斗事件，避免把空闲等待时间算进DPS
            EndedAt = LastEventAt ?? DateTime.Now;

            if (_idleTimer != null)
            {
                _idleTimer.Enabled = false;
                _idleTimer.Elapsed -= OnIdleTick;
            }

            var snapshot = TakeSnapshot();   // 统一在停止时产出快照
            _sessionHistory.Add(snapshot);   // 保存至历史（支持查看过往会话）
        }

        // # 内部调用
        /// <summary>
        /// 获取“有效结束时间”：
        /// - 录制中：以 LastEventAt 为准（若还无事件，则退回到 StartedAt）；
        /// - 已停止：使用 EndedAt（StopInternal 已处理为 LastEventAt）。
        /// </summary>
        private static DateTime EffectiveEndTime()
        {
            if (IsRecording)
            {
                // 录制中：以最后事件时间为准；如果还没事件，就等于开始时间
                if (LastEventAt is not null) return LastEventAt.Value;
                return StartedAt ?? DateTime.Now;
            }
            // 已停止：StopInternal 已把 EndedAt 固定到 LastEventAt
            return EndedAt ?? DateTime.Now;
        }

        // # 内部调用（定时器回调）
        /// <summary>
        /// 空闲检测回调：
        /// - 每秒检查一次；
        /// - 若从 LastEventAt 至今超过 InactivitySeconds，则触发自动停止。
        /// </summary>
        private static void OnIdleTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsRecording) return;
            if (LastEventAt is null) return;

            var idle = DateTime.Now - LastEventAt.Value;
            if (idle.TotalSeconds >= InactivitySeconds)
            {
                // 自动停止：数据保持在此刻（EndedAt = now）
                StopInternal(auto: true);
            }
        }

        #endregion

        // # 区域：快照（留空位以便后续扩展，如需要将快照导出/序列化等） ------------------
        #region 快照


        #endregion

        // # 区域：内嵌写入点（由外部统计管线/钩子调用的一行接入） -------------------------
        #region 内嵌写入点会调用的 API（只加一行即可）

        // # 外部调用（写入点）
        /// <summary>
        /// 记录伤害事件：
        /// - 更新 LastEventAt（打点）；
        /// - 聚合到玩家总伤害与对应技能；
        /// - 更新实时 DPS（基于有效秒数）。
        /// </summary>
        public static void RecordDamage(
      ulong uid, ulong skillId, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
      string nickname, int combatPower, string profession)
        {
            if (!IsRecording || value == 0) return;

            LastEventAt = DateTime.Now; // —— 记录最近事件时间
            var p = GetOrCreate(uid, nickname, combatPower, profession);

            // 顶层聚合
            Accumulate(p.Damage, value, isCrit, isLucky, hpLessen);

            // 逐技能
            var s = p.DamageSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.DamageSkills[skillId] = new StatAcc());
            Accumulate(s, value, isCrit, isLucky, hpLessen);

            // —— 更新实时DPS
            UpdateRealtimeDps(p);
        }

        // # 外部调用（写入点）
        /// <summary>
        /// 记录治疗事件：
        /// - 更新 LastEventAt；
        /// - 聚合到玩家总治疗与对应技能；
        /// - 更新实时 DPS/HPS（HPS 以 Healing 为基，影响玩家侧实时显示）。
        /// </summary>
        public static void RecordHealing(
            ulong uid, ulong skillId, ulong value, bool isCrit, bool isLucky,
            string nickname, int combatPower, string profession)
        {
            if (!IsRecording || value == 0) return;

            LastEventAt = DateTime.Now;
            var p = GetOrCreate(uid, nickname, combatPower, profession);

            Accumulate(p.Healing, value, isCrit, isLucky, 0);
            var s = p.HealingSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.HealingSkills[skillId] = new StatAcc());
            Accumulate(s, value, isCrit, isLucky, 0);

            UpdateRealtimeDps(p);
        }

        // # 外部调用（写入点）
        /// <summary>
        /// 记录承伤事件：
        /// - 更新 LastEventAt；
        /// - 聚合承伤与逐技能承伤（承伤不分暴击/幸运，hpLessen 取总值）；
        /// - 不计入队伍/玩家DPS（如需“受伤DPS”，可扩展）。
        /// </summary>
        public static void RecordTakenDamage(
            ulong uid, ulong skillId, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
            string nickname, int combatPower, string profession)
        {
            if (!IsRecording || value == 0) return;

            LastEventAt = DateTime.Now;
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


        // # 内部调用
        /// <summary>
        /// 更新玩家与队伍的实时 DPS：
        /// - 秒数取 SessionSeconds()（进行中：Now 与 StartedAt；已停：EndedAt 与 StartedAt）；
        /// - 玩家总伤害/治疗与逐技能实时值均更新；
        /// - 队伍实时 DPS = 所有玩家总伤害 / 有效秒数。
        /// </summary>
        private static void UpdateRealtimeDps(PlayerAcc p, bool includeHealing = true)
        {
            var secs = SessionSeconds(); // 会话秒数（进行中：用 Now；已结束：用 EndedAt）
            if (secs <= 0)
            {
                p.RealtimeDpsDamage = 0;
                p.RealtimeDpsHealing = 0;
                TeamRealtimeDps = 0;
                return;
            }

            // 玩家聚合
            p.RealtimeDpsDamage = p.Damage.Total / secs;
            if (includeHealing) p.RealtimeDpsHealing = p.Healing.Total / secs;

            // 逐技能
            foreach (var kv in p.DamageSkills) kv.Value.RealtimeDps = kv.Value.Total / secs;
            if (includeHealing) foreach (var kv in p.HealingSkills) kv.Value.RealtimeDps = kv.Value.Total / secs;

            // 队伍实时DPS
            ulong teamTotal = 0;
            foreach (var pp in _players.Values) teamTotal += pp.Damage.Total;
            TeamRealtimeDps = teamTotal / secs;
        }

        #endregion

        // # 区域：快照 & 秒伤计算（对外查询接口 + 快照产出） -------------------------------
        #region 快照 & 秒伤

        // # 外部调用
        /// <summary>
        /// 生成一次全程快照：
        /// - 使用 EffectiveEndTime() 保证不包含空闲等待；
        /// - 汇总队伍总伤害/治疗；
        /// - 逐玩家构建 SnapshotPlayer，并附带按伤害/治疗降序的技能汇总。
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
                    TotalDps = duration.TotalSeconds > 0 ? p.Damage.Total / duration.TotalSeconds : 0,
                    TotalHealing = p.Healing.Total,
                    TotalHps = duration.TotalSeconds > 0 ? p.Healing.Total / duration.TotalSeconds : 0,
                    TakenDamage = p.TakenDamage,
                    LastRecordTime = null,

                    DamageSkills = damageSkills,
                    HealingSkills = healingSkills,
                    TakenSkills = takenSkills          // ★ 新增

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

        // # 外部调用
        /// <summary>
        /// 获取队伍当前全程 DPS（只计算伤害）：
        /// - 进行中：以 Now 为有效结束；
        /// - 已停止：以 EndedAt（=LastEventAt）为有效结束。
        /// </summary>
        public static double GetTeamDps()
        {
            var secs = SessionSeconds();
            if (secs <= 0) return 0;
            ulong total = 0;
            foreach (var p in _players.Values) total += p.Damage.Total;
            return total / secs;
        }

        // # 外部调用
        /// <summary>
        /// 获取指定玩家当前全程 DPS（只计算伤害）。
        /// </summary>
        public static double GetPlayerDps(ulong uid)
        {
            var secs = SessionSeconds();
            if (secs <= 0) return 0;
            return _players.TryGetValue(uid, out var p) ? p.Damage.Total / secs : 0;
        }

        #region 查询（按快照时间检索）
        // # 外部调用
        /// <summary>
        /// 按快照的开始时间获取该快照中所有玩家数据。
        /// - 如果找不到对应快照，返回 null。
        /// </summary>
        public static IReadOnlyDictionary<ulong, SnapshotPlayer>? GetAllPlayersDataBySnapshotTime(DateTime snapshotStartTime)
        {
            var snapshot = SessionHistory.FirstOrDefault(s => s.StartedAt == snapshotStartTime);
            return snapshot?.Players;
        }

        // # 外部调用
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

        #endregion

        // # 区域：内部实现（工具方法与数据结构管理） ---------------------------------------
        #region 内部实现

        // # 内部调用
        /// <summary>
        /// 计算会话“有效秒数”：
        /// - 若尚未开始返回0；
        /// - 进行中：end=EffectiveEndTime()（通常等于 LastEventAt）；
        /// - 已停止：End=EndedAt（已固化至 LastEventAt）。
        /// </summary>
        private static double SessionSeconds()
        {
            if (StartedAt is null) return 0;
            var end = EffectiveEndTime();
            var sec = (end - StartedAt.Value).TotalSeconds;
            return sec > 0 ? sec : 0;
        }

        // # 内部调用
        /// <summary>
        /// 获取或创建玩家累计器，并同步其基础信息（昵称/战力/职业以最近一次为准）。
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

        // # 内部调用
        /// <summary>
        /// 将一次数值累加到统计器：
        /// - 区分普通/暴击/幸运/暴击+幸运四类数值；
        /// - 维护总和、hpLessen（承伤或额外指标）、次数与最大/最小单次值。
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
        }

        // # 内部调用
        /// <summary>
        /// 将内部技能统计转为快照中的技能汇总项（含DPS、命中均值、暴击/幸运率等）。
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
                AvgPerHit = s.CountTotal > 0 ? (double)s.Total / s.CountTotal : 0.0,
                CritRate = s.CountTotal > 0 ? (double)s.CountCritical / s.CountTotal : 0.0,
                LuckyRate = s.CountTotal > 0 ? (double)s.CountLucky / s.CountTotal : 0.0,
                MaxSingleHit = s.MaxSingleHit,
                MinSingleHit = s.MinSingleHit,
                RealtimeValue = 0,          // 快照为历史静态值，这里不赋实时
                RealtimeMax = 0,            // 同上
                TotalDps = duration.TotalSeconds > 0 ? s.Total / duration.TotalSeconds : 0,
                LastTime = null,            // 可按需扩展：记录技能最后出现时间
                ShareOfTotal = 0            // 可按需扩展：占比（由外部渲染时计算亦可）
            };
        }

        // ===== 内部数据结构 =====
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

        private sealed class StatAcc
        {
            public ulong Normal, Critical, Lucky, CritLucky, HpLessen, Total;
            public ulong MaxSingleHit, MinSingleHit; // Min=0 表示未赋值
            public int CountNormal, CountCritical, CountLucky, CountTotal;

            // —— 新增：实时DPS（逐技能/逐类）
            public double RealtimeDps { get; set; }
        }
        #endregion
    }

    /// <summary>全程快照结构（与 BattleSnapshot 类似，但跨战斗）</summary>
    public sealed class FullSessionSnapshot
    {
        public DateTime StartedAt { get; init; }
        public DateTime EndedAt { get; init; }
        public TimeSpan Duration { get; init; }
        public ulong TeamTotalDamage { get; init; }
        public ulong TeamTotalHealing { get; init; }
        public Dictionary<ulong, SnapshotPlayer> Players { get; init; } = new();
        public ulong TeamTotalTakenDamage { get; init; }   // ★ 新增

    }
}
