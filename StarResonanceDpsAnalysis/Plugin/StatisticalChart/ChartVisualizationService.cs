using StarResonanceDpsAnalysis.Plugin.Charts;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.Plugin
{
    /// <summary>
    /// 图表配置管理器 - 统一处理各类图表的默认设置
    /// </summary>
    public static class ChartConfigManager
    {
        // 统一的默认常量
        public const string EMPTY_TEXT = "";
        public const bool HIDE_LEGEND = false;
        public const bool SHOW_GRID = true;
        public const bool SHOW_VIEW_INFO = false;
        public const bool AUTO_SCALE_FONT = false;
        public const bool PRESERVE_VIEW = true;
        public const int REFRESH_INTERVAL = 1000;
        public const int MIN_WIDTH = 450;
        public const int MIN_HEIGHT = 150;

        public static readonly Font DefaultFont = new("微软雅黑", 10, FontStyle.Regular);

        /// <summary>
        /// 统一应用图表默认配置
        /// </summary>
        public static T ApplySettings<T>(T chart) where T : UserControl
        {
            // 通用控件设置
            chart.Dock = DockStyle.Fill;

            // 根据图表类型应用特定设置
            switch (chart)
            {
                case FlatLineChart lineChart:
                    ApplyLineChartSettings(lineChart);
                    break;
                case FlatBarChart barChart:
                    ApplyBarChartSettings(barChart);
                    break;
                case FlatPieChart pieChart:
                    ApplyPieChartSettings(pieChart);
                    break;
                case FlatScatterChart scatterChart:
                    ApplyScatterChartSettings(scatterChart);
                    break;
            }

            return chart;
        }

        private static void ApplyLineChartSettings(FlatLineChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.XAxisLabel = EMPTY_TEXT;
            chart.YAxisLabel = EMPTY_TEXT;
            chart.ShowLegend = HIDE_LEGEND;
            chart.ShowGrid = SHOW_GRID;
            chart.ShowViewInfo = SHOW_VIEW_INFO;
            chart.AutoScaleFont = AUTO_SCALE_FONT;
            chart.PreserveViewOnDataUpdate = PRESERVE_VIEW;
            chart.IsDarkTheme = !AppConfig.IsLight;
            chart.MinimumSize = new Size(MIN_WIDTH, MIN_HEIGHT);
            chart.Font = DefaultFont;
        }

        private static void ApplyBarChartSettings(FlatBarChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.IsDarkTheme = !AppConfig.IsLight;
        }

        private static void ApplyPieChartSettings(FlatPieChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.IsDarkTheme = !AppConfig.IsLight;
            chart.ShowLabels = true;
            chart.ShowPercentages = true;
        }

        private static void ApplyScatterChartSettings(FlatScatterChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.XAxisLabel = EMPTY_TEXT;
            chart.YAxisLabel = EMPTY_TEXT;
            chart.ShowLegend = true;
            chart.ShowGrid = SHOW_GRID;
            chart.IsDarkTheme = !AppConfig.IsLight;
        }
    }

    /// <summary>
    /// 图表数据来源
    /// </summary>
    public enum ChartDataSource
    {
        Current = 0,   // 当前战斗（单次）
        FullRecord = 1 // 全程（会话）
    }

    /// <summary>
    /// 图表数据类型
    /// </summary>
    public enum ChartDataType
    {
        Damage = 0,      // 伤害
        Healing = 1,     // 治疗 
        TakenDamage = 2  // 承伤
    }

    /// <summary>
    /// 实时图表可视化服务
    /// </summary>
    public static class ChartVisualizationService
    {
        #region 数据存储
        // 不同数据类型的历史存储
        private static readonly Dictionary<ulong, List<(DateTime Time, double Dps)>> _dpsHistory = new();
        private static readonly Dictionary<ulong, List<(DateTime Time, double Hps)>> _hpsHistory = new();
        private static readonly Dictionary<ulong, List<(DateTime Time, double TakenDps)>> _takenDpsHistory = new();
        
        private static DateTime? _combatStartTime;
        private static readonly List<WeakReference> _registeredCharts = new();

        private const int MAX_HISTORY_POINTS = 500;
        private const double INACTIVE_TIMEOUT_SECONDS = 2.0;

        public static bool IsCapturing { get; private set; } = false;

        // 新增：数据源模式（默认“当前战斗”）
        public static ChartDataSource DataSource { get; private set; } = ChartDataSource.Current;
        #endregion

        #region 数据更新
        /// <summary>
        /// 切换图表数据来源（会自动清空历史，避免数据混淆）。
        /// </summary>
        public static void SetDataSource(ChartDataSource source, bool clearHistory = true)
        {
            if (DataSource == source) return;
            DataSource = source;
            if (clearHistory)
            {
                ClearAllHistory();
            }
        }

        /// <summary>
        /// 添加数据点（通用方法）
        /// </summary>
        private static void AddDataPoint<T>(Dictionary<ulong, List<(DateTime, T)>> history, ulong playerId, T value)
        {
            var now = DateTime.Now;
            _combatStartTime ??= now;

            if (!history.TryGetValue(playerId, out var playerHistory))
            {
                playerHistory = new List<(DateTime, T)>();
                history[playerId] = playerHistory;
            }

            // 确保数值非负
            var safeValue = value is double d ? (T)(object)Math.Max(0, d) : value;
            playerHistory.Add((now, safeValue));

            // 控制历史长度
            if (playerHistory.Count > MAX_HISTORY_POINTS)
                playerHistory.RemoveAt(0);
        }

        public static void AddDpsDataPoint(ulong playerId, double dps) =>
            AddDataPoint(_dpsHistory, playerId, dps);

        public static void AddHpsDataPoint(ulong playerId, double hps) =>
            AddDataPoint(_hpsHistory, playerId, hps);

        public static void AddTakenDpsDataPoint(ulong playerId, double takenDps) =>
            AddDataPoint(_takenDpsHistory, playerId, takenDps);

        public static void UpdateAllDataPoints()
        {
            if (DataSource == ChartDataSource.Current)
            {
                var players = StatisticData._manager.GetPlayersWithCombatData();

                // 更新实时统计
                foreach (var player in players)
                    player.UpdateRealtimeStats();

                // 写入数据点
                foreach (var player in players)
                {
                    AddDpsDataPoint(player.Uid, player.DamageStats.RealtimeValue);
                    AddHpsDataPoint(player.Uid, player.HealingStats.RealtimeValue);

                    // 承伤也使用实时统计值
                    AddTakenDpsDataPoint(player.Uid, player.TakenStats.RealtimeValue);
                }
            }
            else // 全程数据源
            {
                // 使用全程统计的“当前时刻”快照计算 Dps/Hps/承伤每秒
                var totals = FullRecord.GetPlayersWithTotals(includeZero: false);
                foreach (var p in totals)
                {
                    // Dps/Hps 直接来自 FullRecord 的计算
                    AddDpsDataPoint(p.Uid, p.Dps);
                    AddHpsDataPoint(p.Uid, p.Hps);

                    // 承伤：通过 Shim 读取该玩家的承伤“有效时长均值”
                    var shim = FullRecord.Shim.GetOrCreate(p.Uid);
                    var t = shim.TakenStats;
                    double takenPerSec = t.ActiveSeconds > 0 ? Math.Round(t.Total / t.ActiveSeconds, 2, MidpointRounding.AwayFromZero) : 0.0;
                    AddTakenDpsDataPoint(p.Uid, takenPerSec);
                }
            }

            CheckAndAddZeroValues();
        }

        private static void CheckAndAddZeroValues()
        {
            HashSet<ulong> activePlayerIds;
            if (DataSource == ChartDataSource.Current)
                activePlayerIds = StatisticData._manager.GetPlayersWithCombatData().Select(p => p.Uid).ToHashSet();
            else
                activePlayerIds = FullRecord.GetPlayersWithTotals(includeZero: false).Select(p => p.Uid).ToHashSet();

            var now = DateTime.Now;

            // 为不活跃的玩家补 0 值
            CheckHistoryForZeroValues(_dpsHistory, activePlayerIds, now, AddDpsDataPoint);
            CheckHistoryForZeroValues(_hpsHistory, activePlayerIds, now, AddHpsDataPoint);
            CheckHistoryForZeroValues(_takenDpsHistory, activePlayerIds, now, AddTakenDpsDataPoint);
        }

        private static void CheckHistoryForZeroValues<T>(Dictionary<ulong, List<(DateTime Time, T Value)>> history,
            HashSet<ulong> activePlayerIds, DateTime now, Action<ulong, T> addZeroValue)
            where T : struct, IComparable<T>
        {
            var zero = default(T);
            foreach (var playerId in history.Keys.ToList())
            {
                if (activePlayerIds.Contains(playerId)) continue;

                var playerHistory = history[playerId];
                if (playerHistory.Count > 0)
                {
                    var lastRecord = playerHistory.Last();
                    var timeSinceLastRecord = (now - lastRecord.Time).TotalSeconds;

                    if (timeSinceLastRecord > INACTIVE_TIMEOUT_SECONDS && lastRecord.Value.CompareTo(zero) > 0)
                        addZeroValue(playerId, zero);
                }
            }
        }

        public static void ClearAllHistory()
        {
            _dpsHistory.Clear();
            _hpsHistory.Clear();
            _takenDpsHistory.Clear();
            _combatStartTime = null;
        }

        public static void OnCombatEnd()
        {
            foreach (var playerId in _dpsHistory.Keys.ToList())
            {
                var history = _dpsHistory[playerId];
                if (history.Count > 0 && history.Last().Dps > 0)
                    AddDpsDataPoint(playerId, 0);
            }

            foreach (var playerId in _hpsHistory.Keys.ToList())
            {
                var history = _hpsHistory[playerId];
                if (history.Count > 0 && history.Last().Hps > 0)
                    AddHpsDataPoint(playerId, 0);
            }

            foreach (var playerId in _takenDpsHistory.Keys.ToList())
            {
                var history = _takenDpsHistory[playerId];
                if (history.Count > 0 && history.Last().TakenDps > 0)
                    AddTakenDpsDataPoint(playerId, 0);
            }
        }
        #endregion

        #region 图表创建
        /// <summary>
        /// 通用创建方法
        /// </summary>
        /// <typeparam name="T">图表控件类型：继承自 UserControl</typeparam>
        /// <param name="size">图表的初始大小</param>
        /// <param name="customConfig">可选：自定义配置回调，可修改图表控件的各种参数</param>
        /// <returns>已创建并应用默认配置的图表实例</returns>
        private static T CreateChart<T>(Size size, Action<T> customConfig = null) where T : UserControl, new()
        {
            var chart = new T { Size = size };
            ChartConfigManager.ApplySettings(chart); // 应用统一的图表配置
            customConfig?.Invoke(chart); // 执行自定义配置
            return chart;
        }

        /// <summary>
        /// 创建 DPS 趋势折线图（FlatLineChart）
        /// </summary>
        /// <param name="width">图表宽，默认 800</param>
        /// <param name="height">图表高，默认 400</param>
        /// <param name="specificPlayerId">可选：指定玩家 ID（只显示该玩家曲线）</param>
        /// <returns>已创建并初始化的 DPS 趋势图控件</returns>
        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400, ulong? specificPlayerId = null)
        {
            var chart = CreateChart<FlatLineChart>(new Size(width, height));

            RegisterChart(chart); // 注册图表以便统一管理

            if (IsCapturing) // 若当前在捕获数据，则开启自动刷新
                chart.StartAutoRefresh(ChartConfigManager.REFRESH_INTERVAL);

            RefreshDpsTrendChart(chart, specificPlayerId); // 载入初始数据
            return chart;
        }

        /// <summary>
        /// 创建技能伤害占比饼图（FlatPieChart）
        /// </summary>
        public static FlatPieChart CreateSkillDamagePieChart(ulong playerId, int width = 400, int height = 400)
        {
            var chart = CreateChart<FlatPieChart>(new Size(width, height));
            RefreshSkillDamagePieChart(chart, playerId); // 初始刷新
            return chart;
        }

        /// <summary>
        /// 创建队伍 DPS 条形图（FlatBarChart）
        /// </summary>
        public static FlatBarChart CreateTeamDpsBarChart(int width = 600, int height = 400)
        {
            var chart = CreateChart<FlatBarChart>(new Size(width, height));
            RefreshTeamDpsBarChart(chart); // 初始刷新
            return chart;
        }

        /// <summary>
        /// 创建 DPS 散点图（FlatScatterChart）
        /// </summary>
        public static FlatScatterChart CreateDpsRadarChart(int width = 400, int height = 400)
        {
            var chart = CreateChart<FlatScatterChart>(new Size(width, height));
            RefreshDpsRadarChart(chart); // 初始刷新
            return chart;
        }

        /// <summary>
        /// 创建伤害类型堆叠条形图（FlatBarChart）
        /// </summary>
        public static FlatBarChart CreateDamageTypeStackedChart(int width = 600, int height = 400)
        {
            var chart = CreateChart<FlatBarChart>(new Size(width, height));
            RefreshDamageTypeStackedChart(chart); // 初始刷新
            return chart;
        }

        #endregion

        #region 图表刷新
        /// <summary>
        /// 刷新 DPS 趋势图数据，支持单人/多人以及不同数据类型
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart, ulong? specificPlayerId = null, ChartDataType dataType = ChartDataType.Damage)
        {
            // 记录与恢复视图状态
            var timeScale = chart.GetTimeScale();
            var viewOffset = chart.GetViewOffset();
            var hadData = chart.HasData();

            chart.ClearSeries();

            // 根据数据类型选择对应的历史数据
            Dictionary<ulong, List<(DateTime Time, double Value)>> historyData = dataType switch
            {
                ChartDataType.Healing => _hpsHistory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.Hps)).ToList()),
                ChartDataType.TakenDamage => _takenDpsHistory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.TakenDps)).ToList()),
                _ => _dpsHistory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.Dps)).ToList())
            };

            if (historyData.Count == 0 || _combatStartTime == null) return;

            var startTime = _combatStartTime.Value;

            if (specificPlayerId.HasValue)
            {
                RefreshSinglePlayerChart(chart, historyData, specificPlayerId.Value, startTime);
            }
            else
            {
                RefreshMultiPlayerChart(chart, historyData, startTime);
            }

            // 恢复视图状态（仅在用户交互过时）
            if (hadData && chart.HasUserInteracted())
            {
                chart.SetTimeScale(timeScale);
                chart.SetViewOffset(viewOffset);
            }
        }

        private static void RefreshSinglePlayerChart(FlatLineChart chart, Dictionary<ulong, List<(DateTime Time, double Value)>> historyData,
            ulong playerId, DateTime startTime)
        {
            if (historyData.TryGetValue(playerId, out var playerHistory) && playerHistory.Count > 0)
            {
                var points = ConvertToPoints(playerHistory, startTime);
                if (points.Count > 0)
                    chart.AddSeries("", points);
            }
        }

        private static void RefreshMultiPlayerChart(FlatLineChart chart, Dictionary<ulong, List<(DateTime Time, double Value)>> historyData,
            DateTime startTime)
        {
            foreach (var (playerId, history) in historyData.OrderBy(x => x.Key))
            {
                if (history.Count == 0) continue;

                var points = ConvertToPoints(history, startTime);
                if (points.Count > 0)
                    chart.AddSeries("", points);
            }
        }

        private static List<PointF> ConvertToPoints(List<(DateTime Time, double Value)> history, DateTime startTime)
        {
            return history.Select(h => new PointF(
                (float)(h.Time - startTime).TotalSeconds,
                (float)h.Value
            )).ToList();
        }

        public static void RefreshSkillDamagePieChart(FlatPieChart chart, ulong playerId, ChartDataType dataType = ChartDataType.Damage)
        {
            chart.ClearData();

            try
            {
                // 根据数据类型获取相应的技能数据
                var skillData = dataType switch
                {
                    ChartDataType.Healing => StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true, StarResonanceDpsAnalysis.Core.SkillType.Heal),
                    ChartDataType.TakenDamage => StatisticData._manager.GetPlayerTakenDamageSummaries(playerId, topN: 8, orderByTotalDesc: true),
                    _ => StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true, StarResonanceDpsAnalysis.Core.SkillType.Damage)
                };

                if (skillData.Count == 0) return;

                var pieData = skillData.Select(s => (
                    Label: $"{s.SkillName}: {Common.FormatWithEnglishUnits(s.Total)}",
                    Value: (double)s.Total
                )).ToList();

                chart.SetData(pieData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刷新技能伤害饼图时出错: {ex.Message}");
            }
        }

        public static void RefreshTeamDpsBarChart(FlatBarChart chart)
        {
            chart.ClearData();
            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
            if (players.Count == 0) return;

            var barData = players
                .OrderByDescending(p => p.GetTotalDps())
                .Select(p => (Label: "", Value: p.GetTotalDps()))
                .ToList();

            chart.SetData(barData);
        }

        public static void RefreshDpsRadarChart(FlatScatterChart chart)
        {
            chart.ClearSeries();
            var players = StatisticData._manager.GetPlayersWithCombatData().Take(5).ToList();
            if (players.Count == 0) return;

            foreach (var player in players)
            {
                var totalDps = player.GetTotalDps();
                var critRate = player.DamageStats.GetCritRate();
                var points = new List<PointF> { new((float)critRate, (float)totalDps) };
                chart.AddSeries("", points);
            }
        }

        public static void RefreshDamageTypeStackedChart(FlatBarChart chart)
        {
            chart.ClearData();
            var players = StatisticData._manager.GetPlayersWithCombatData()
                .OrderByDescending(p => p.DamageStats.Total)
                .Take(6)
                .ToList();

            if (players.Count == 0) return;

            var barData = players.Select(p => (Label: "", Value: (double)p.DamageStats.Total)).ToList();
            chart.SetData(barData);
        }
        #endregion

        #region 图表管理
        public static void RegisterChart(FlatLineChart chart)
        {
            lock (_registeredCharts)
            {
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
                _registeredCharts.Add(new WeakReference(chart));
            }
        }

        public static void StopAllChartsAutoRefresh()
        {
            IsCapturing = false;
            ExecuteOnRegisteredCharts(chart => chart.StopAutoRefresh());
        }

        public static void StartAllChartsAutoRefresh(int intervalMs = 1000)
        {
            IsCapturing = true;
            ExecuteOnRegisteredCharts(chart => chart.StartAutoRefresh(intervalMs));
        }

        public static void FullResetAllCharts()
        {
            ClearAllHistory();
            ExecuteOnRegisteredCharts(chart => chart.FullReset());
        }

        private static void ExecuteOnRegisteredCharts(Action<FlatLineChart> action)
        {
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try { action(chart); }
                        catch (Exception ex) { Console.WriteLine($"图表管理执行出错: {ex.Message}"); }
                    }
                }
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }
        #endregion

        #region 其它工具
        public static bool HasDataToVisualize() =>
            StatisticData._manager.GetPlayersWithCombatData().Any();

        public static double GetCombatDurationSeconds() =>
            _combatStartTime?.Let(start => (DateTime.Now - start).TotalSeconds) ?? 0;

        public static int GetDpsHistoryPointCount() =>
            _dpsHistory.Sum(kvp => kvp.Value.Count);
        #endregion
    }

    /// <summary>
    /// 扩展工具方法
    /// </summary>
    public static class Extensions
    {
        public static TResult Let<T, TResult>(this T obj, Func<T, TResult> func) => func(obj);
    }
}