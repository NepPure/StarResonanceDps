using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using StarResonanceDpsAnalysis.Plugin.Charts;

namespace StarResonanceDpsAnalysis.Plugin
{
    /// <summary>
    /// 图表配置管理器 - 统一管理所有图表的配置和设置
    /// </summary>
    public static class ChartConfigManager
    {
        // 统一的默认配置常量
        public const string EMPTY_TEXT = "";
        public const bool HIDE_LEGEND = false;
        public const bool SHOW_GRID = true;
        public const bool SHOW_VIEW_INFO = false;
        public const bool AUTO_SCALE_FONT = false;
        public const bool PRESERVE_VIEW = true;
        public const int REFRESH_INTERVAL = 1000;
        public const int MIN_WIDTH = 450;
        public const int MIN_HEIGHT = 150;
        
        public static readonly Font DefaultFont = new("阿里妈妈数黑体", 10, FontStyle.Regular);

        /// <summary>
        /// 统一应用图表基础设置
        /// </summary>
        public static T ApplySettings<T>(T chart) where T : UserControl
        {
            // 通用属性设置
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
    /// 实时图表可视化服务
    /// </summary>
    public static class ChartVisualizationService
    {
        #region 数据存储
        private static readonly Dictionary<ulong, List<(DateTime Time, double Dps)>> _dpsHistory = new();
        private static readonly Dictionary<ulong, List<(DateTime Time, double Hps)>> _hpsHistory = new();
        private static DateTime? _combatStartTime;
        private static readonly List<WeakReference> _registeredCharts = new();
        
        private const int MAX_HISTORY_POINTS = 500;
        private const double INACTIVE_TIMEOUT_SECONDS = 2.0;

        public static bool IsCapturing { get; private set; } = false;
        #endregion

        #region 数据管理
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

            // 确保数值不为负
            var safeValue = value is double d ? (T)(object)Math.Max(0, d) : value;
            playerHistory.Add((now, safeValue));

            // 限制历史点数
            if (playerHistory.Count > MAX_HISTORY_POINTS)
                playerHistory.RemoveAt(0);
        }

        public static void AddDpsDataPoint(ulong playerId, double dps) => 
            AddDataPoint(_dpsHistory, playerId, dps);

        public static void AddHpsDataPoint(ulong playerId, double hps) => 
            AddDataPoint(_hpsHistory, playerId, hps);

        public static void UpdateAllDataPoints()
        {
            var players = StatisticData._manager.GetPlayersWithCombatData();
            
            // 更新实时统计
            foreach (var player in players)
                player.UpdateRealtimeStats();
            
            // 添加数据点
            foreach (var player in players)
            {
                AddDpsDataPoint(player.Uid, player.DamageStats.RealtimeValue);
                AddHpsDataPoint(player.Uid, player.HealingStats.RealtimeValue);
            }
            
            CheckAndAddZeroValues();
        }

        private static void CheckAndAddZeroValues()
        {
            var activePlayerIds = StatisticData._manager.GetPlayersWithCombatData().Select(p => p.Uid).ToHashSet();
            var now = DateTime.Now;

            // 为不活跃玩家添加0值
            CheckHistoryForZeroValues(_dpsHistory, activePlayerIds, now, AddDpsDataPoint);
            CheckHistoryForZeroValues(_hpsHistory, activePlayerIds, now, AddHpsDataPoint);
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
        }
        #endregion

        #region 图表创建
        /// <summary>
        /// 创建并配置图表（通用方法）
        /// </summary>
        private static T CreateChart<T>(Size size, Action<T> customConfig = null) where T : UserControl, new()
        {
            var chart = new T { Size = size };
            ChartConfigManager.ApplySettings(chart);
            customConfig?.Invoke(chart);
            return chart;
        }

        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400, ulong? specificPlayerId = null)
        {
            var chart = CreateChart<FlatLineChart>(new Size(width, height));
            
            RegisterChart(chart);
            
            if (IsCapturing)
                chart.StartAutoRefresh(ChartConfigManager.REFRESH_INTERVAL);
            
            RefreshDpsTrendChart(chart, specificPlayerId);
            return chart;
        }

        public static FlatPieChart CreateSkillDamagePieChart(ulong playerId, int width = 400, int height = 400)
        {
            var chart = CreateChart<FlatPieChart>(new Size(width, height));
            RefreshSkillDamagePieChart(chart, playerId);
            return chart;
        }

        public static FlatBarChart CreateTeamDpsBarChart(int width = 600, int height = 400)
        {
            var chart = CreateChart<FlatBarChart>(new Size(width, height));
            RefreshTeamDpsBarChart(chart);
            return chart;
        }

        public static FlatScatterChart CreateDpsRadarChart(int width = 400, int height = 400)
        {
            var chart = CreateChart<FlatScatterChart>(new Size(width, height));
            RefreshDpsRadarChart(chart);
            return chart;
        }

        public static FlatBarChart CreateDamageTypeStackedChart(int width = 600, int height = 400)
        {
            var chart = CreateChart<FlatBarChart>(new Size(width, height));
            RefreshDamageTypeStackedChart(chart);
            return chart;
        }
        #endregion

        #region 图表刷新
        public static void RefreshDpsTrendChart(FlatLineChart chart, ulong? specificPlayerId = null, bool showHps = false)
        {
            // 保存视图状态
            var timeScale = chart.GetTimeScale();
            var viewOffset = chart.GetViewOffset();
            var hadData = chart.HasData();
            
            chart.ClearSeries();

            var historyData = showHps ? _hpsHistory : _dpsHistory;
            if (historyData.Count == 0 || _combatStartTime == null) return;

            var startTime = _combatStartTime.Value;

            if (specificPlayerId.HasValue)
            {
                RefreshSinglePlayerChart(chart, historyData, specificPlayerId.Value, startTime, showHps);
            }
            else
            {
                RefreshMultiPlayerChart(chart, historyData, startTime, showHps);
            }
            
            // 恢复视图状态
            if (hadData && chart.HasUserInteracted())
            {
                chart.SetTimeScale(timeScale);
                chart.SetViewOffset(viewOffset);
            }
        }

        private static void RefreshSinglePlayerChart<T>(FlatLineChart chart, Dictionary<ulong, List<(DateTime Time, T Value)>> historyData,
            ulong playerId, DateTime startTime, bool showHps)
        {
            if (historyData.TryGetValue(playerId, out var playerHistory) && playerHistory.Count > 0)
            {
                var points = ConvertToPoints(playerHistory, startTime);
                if (points.Count > 0)
                    chart.AddSeries("", points);
            }
        }

        private static void RefreshMultiPlayerChart<T>(FlatLineChart chart, Dictionary<ulong, List<(DateTime Time, T Value)>> historyData,
            DateTime startTime, bool showHps)
        {
            foreach (var (playerId, history) in historyData.OrderBy(x => x.Key))
            {
                if (history.Count == 0) continue;
                
                var points = ConvertToPoints(history, startTime);
                if (points.Count > 0)
                    chart.AddSeries("", points);
            }
        }

        private static List<PointF> ConvertToPoints<T>(List<(DateTime Time, T Value)> history, DateTime startTime)
        {
            return history.Select(h => new PointF(
                (float)(h.Time - startTime).TotalSeconds,
                Convert.ToSingle(h.Value)
            )).ToList();
        }

        public static void RefreshSkillDamagePieChart(FlatPieChart chart, ulong playerId)
        {
            chart.ClearData();

            try
            {
                var skillData = StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true);
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
                var critRate = player.DamageStats.GetCritRate() * 100;
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
                        catch (Exception ex) { Console.WriteLine($"图表操作出错: {ex.Message}"); }
                    }
                }
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }
        #endregion

        #region 工具方法
        public static bool HasDataToVisualize() => 
            StatisticData._manager.GetPlayersWithCombatData().Any();

        public static double GetCombatDurationSeconds() => 
            _combatStartTime?.Let(start => (DateTime.Now - start).TotalSeconds) ?? 0;

        public static int GetDpsHistoryPointCount() => 
            _dpsHistory.Sum(kvp => kvp.Value.Count);
        #endregion
    }

    /// <summary>
    /// 扩展方法辅助类
    /// </summary>
    public static class Extensions
    {
        public static TResult Let<T, TResult>(this T obj, Func<T, TResult> func) => func(obj);
    }
}