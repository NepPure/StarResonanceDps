using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using StarResonanceDpsAnalysis.Plugin.Charts;

namespace StarResonanceDpsAnalysis.Plugin
{
    /// <summary>
    /// 实时图表可视化服务 - 基于自定义扁平化图表控件
    /// </summary>
    public static class ChartVisualizationService
    {
        #region 数据点存储

        /// <summary>DPS趋势数据点</summary>
        private static readonly Dictionary<ulong, List<(DateTime Time, double Dps)>> _dpsHistory = new();

        /// <summary>HPS趋势数据点</summary>
        private static readonly Dictionary<ulong, List<(DateTime Time, double Hps)>> _hpsHistory = new();

        /// <summary>战斗开始时间（用于X轴时间计算）</summary>
        private static DateTime? _combatStartTime;

        /// <summary>最大历史数据点数</summary>
        private const int MaxHistoryPoints = 500; // 增加到500个点以支持更长时间的历史

        /// <summary>是否正在进行数据捕获（用于图表服务检查）</summary>
        public static bool IsCapturing { get; private set; } = false;

        #endregion

        #region 数据更新方法

        /// <summary>
        /// 添加DPS数据点
        /// </summary>
        public static void AddDpsDataPoint(ulong playerId, double dps)
        {
            var now = DateTime.Now;
            _combatStartTime ??= now;

            if (!_dpsHistory.TryGetValue(playerId, out var history))
            {
                history = new List<(DateTime, double)>();
                _dpsHistory[playerId] = history;
            }

            // 总是添加数据点，包括0值，这样能保持图表的连续性
            history.Add((now, Math.Max(0, dps))); // 确保不会有负值，但允许0值

            // 限制历史数据点数量
            if (history.Count > MaxHistoryPoints)
            {
                history.RemoveAt(0);
            }
        }

        /// <summary>
        /// 添加HPS数据点
        /// </summary>
        public static void AddHpsDataPoint(ulong playerId, double hps)
        {
            var now = DateTime.Now;
            _combatStartTime ??= now;

            if (!_hpsHistory.TryGetValue(playerId, out var history))
            {
                history = new List<(DateTime, double)>();
                _hpsHistory[playerId] = history;
            }

            // 总是添加数据点，包括0值，这样能保持图表的连续性
            history.Add((now, Math.Max(0, hps))); // 确保不会有负值，但允许0值

            // 限制历史数据点数量
            if (history.Count > MaxHistoryPoints)
            {
                history.RemoveAt(0);
            }
        }

        /// <summary>
        /// 批量更新所有玩家数据点
        /// </summary>
        public static void UpdateAllDataPoints()
        {
            var players = StatisticData._manager.GetPlayersWithCombatData();
            
            // 首先刷新所有玩家的实时统计数据
            foreach (var player in players)
            {
                player.UpdateRealtimeStats();
            }
            
            foreach (var player in players)
            {
                // 使用实时DPS而不是总平均DPS，这样当没有伤害时会正确显示为0
                var dps = player.DamageStats.RealtimeValue; // 改为使用实时DPS
                var hps = player.HealingStats.RealtimeValue; // 改为使用实时HPS

                // 总是添加DPS和HPS数据点，即使是0，这样能保持连续性
                AddDpsDataPoint(player.Uid, dps);
                AddHpsDataPoint(player.Uid, hps);
            }
            
            // 为了确保在战斗结束后显示0值，我们需要检查是否有玩家的DPS/HPS变为0
            // 并确保这些0值也被记录到历史中
            CheckAndAddZeroValuesForInactivePlayers();
        }

        /// <summary>
        /// 检查并为不活跃的玩家添加0值数据点
        /// </summary>
        private static void CheckAndAddZeroValuesForInactivePlayers()
        {
            var activePlayers = StatisticData._manager.GetPlayersWithCombatData();
            var activePlayerIds = activePlayers.Select(p => p.Uid).ToHashSet();
            
            // 获取所有历史记录中的玩家ID
            var allDpsPlayerIds = _dpsHistory.Keys.ToList();
            var allHpsPlayerIds = _hpsHistory.Keys.ToList();
            
            var now = DateTime.Now;
            
            // 为DPS历史中但当前不活跃的玩家添加0值
            foreach (var playerId in allDpsPlayerIds)
            {
                if (!activePlayerIds.Contains(playerId))
                {
                    // 检查最后一条记录的时间，如果距离现在超过一定时间且不为0，则添加0值
                    var history = _dpsHistory[playerId];
                    if (history.Count > 0)
                    {
                        var lastRecord = history.Last();
                        var timeSinceLastRecord = (now - lastRecord.Time).TotalSeconds;
                        
                        // 如果最后一条记录距离现在超过2秒且不为0，添加0值数据点
                        if (timeSinceLastRecord > 2.0 && lastRecord.Dps > 0)
                        {
                            AddDpsDataPoint(playerId, 0);
                        }
                    }
                }
            }
            
            // 为HPS历史中但当前不活跃的玩家添加0值
            foreach (var playerId in allHpsPlayerIds)
            {
                if (!activePlayerIds.Contains(playerId))
                {
                    var history = _hpsHistory[playerId];
                    if (history.Count > 0)
                    {
                        var lastRecord = history.Last();
                        var timeSinceLastRecord = (now - lastRecord.Time).TotalSeconds;
                        
                        // 如果最后一条记录距离现在超过2秒且不为0，添加0值数据点
                        if (timeSinceLastRecord > 2.0 && lastRecord.Hps > 0)
                        {
                            AddHpsDataPoint(playerId, 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清空所有历史数据
        /// </summary>
        public static void ClearAllHistory()
        {
            _dpsHistory.Clear();
            _hpsHistory.Clear();
            _combatStartTime = null;
        }
        
        /// <summary>
        /// 完全重置所有图表（用于F9清空数据）
        /// </summary>
        public static void FullResetAllCharts()
        {
            // 首先清空历史数据
            ClearAllHistory();
            
            // 重置所有注册的图表
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try
                        {
                            chart.FullReset();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"重置图表时出错: {ex.Message}");
                        }
                    }
                }
                
                // 清理失效的引用
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }

        /// <summary>
        /// 当战斗结束时，为所有有历史记录的玩家添加最终的0值数据点
        /// </summary>
        public static void OnCombatEnd()
        {
            var now = DateTime.Now;
            
            // 为所有DPS历史记录中的玩家添加0值终点
            foreach (var playerId in _dpsHistory.Keys.ToList())
            {
                var history = _dpsHistory[playerId];
                if (history.Count > 0 && history.Last().Dps > 0)
                {
                    AddDpsDataPoint(playerId, 0);
                }
            }
            
            // 为所有HPS历史记录中的玩家添加0值终点
            foreach (var playerId in _hpsHistory.Keys.ToList())
            {
                var history = _hpsHistory[playerId];
                if (history.Count > 0 && history.Last().Hps > 0)
                {
                    AddHpsDataPoint(playerId, 0);
                }
            }
        }

        /// <summary>
        /// 获取战斗持续时间（秒）
        /// </summary>
        public static double GetCombatDurationSeconds()
        {
            if (_combatStartTime == null) return 0;
            return (DateTime.Now - _combatStartTime.Value).TotalSeconds;
        }

        #endregion

        #region DPS趋势图

        /// <summary>
        /// 创建DPS趋势图
        /// </summary>
        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400, ulong? specificPlayerId = null)
        {
            var chart = new FlatLineChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "实时DPS趋势图",
                XAxisLabel = "时间",
                YAxisLabel = "DPS",
                ShowLegend = true,
                ShowGrid = true,
                IsDarkTheme = !AppConfig.IsLight
            };

            // 注册图表到全局管理
            RegisterChart(chart);

            // 如果当前正在捕获数据，立即启动图表的自动刷新
            if (IsCapturing)
            {
                chart.StartAutoRefresh(1000);
            }

            RefreshDpsTrendChart(chart, specificPlayerId);
            return chart;
        }

        /// <summary>
        /// 刷新DPS趋势图数据
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart, ulong? specificPlayerId = null, bool showHps = false)
        {
            // 保存当前的视图状态，避免被ClearSeries重置
            var currentTimeScale = chart.GetTimeScale();
            var currentViewOffset = chart.GetViewOffset();
            var hadPreviousData = chart.HasData();
            
            chart.ClearSeries();

            // 根据显示类型选择合适的历史数据
            var historyData = showHps ? _hpsHistory : _dpsHistory;
            var dataTypeName = showHps ? "HPS" : "DPS";

            if (historyData.Count == 0 || _combatStartTime == null)
            {
                return;
            }

            var startTime = _combatStartTime.Value;

            // 如果指定了特定玩家ID，只显示该玩家的数据
            if (specificPlayerId.HasValue)
            {
                if (historyData.TryGetValue(specificPlayerId.Value, out var playerHistory) && playerHistory.Count > 0)
                {
                    // 获取玩家信息
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(specificPlayerId.Value);
                    var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{specificPlayerId.Value}" : playerInfo.Nickname;

                    // 转换为相对时间（秒）和数值的点集合
                    List<PointF> points;
                    if (showHps)
                    {
                        points = ((List<(DateTime Time, double Hps)>)playerHistory).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Hps
                        )).ToList();
                    }
                    else
                    {
                        points = ((List<(DateTime Time, double Dps)>)playerHistory).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Dps
                        )).ToList();
                    }

                    if (points.Count > 0)
                    {
                        chart.AddSeries($"{playerName} - {dataTypeName}趋势", points);
                        
                        // 更新图表标题显示当前玩家和数据类型
                        chart.TitleText = $"{playerName} - 实时{dataTypeName}趋势";
                    }
                }
                else
                {
                    // 没有找到指定玩家的数据
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(specificPlayerId.Value);
                    var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{specificPlayerId.Value}" : playerInfo.Nickname;
                    chart.TitleText = $"{playerName} - 暂无{dataTypeName}数据";
                }
            }
            else
            {
                // 显示所有玩家数据（原有逻辑）
                // 按玩家ID排序确保数据加载的一致性
                var sortedHistory = historyData.OrderBy(x => x.Key);

                foreach (var kvp in sortedHistory)
                {
                    var playerId = kvp.Key;
                    var history = kvp.Value;

                    if (history.Count == 0) continue;

                    // 获取玩家信息
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(playerId);
                    var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{playerId}" : playerInfo.Nickname;

                    // 转换为相对时间（秒）和数值的点集合
                    List<PointF> points;
                    if (showHps)
                    {
                        points = ((List<(DateTime Time, double Hps)>)history).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Hps
                        )).ToList();
                    }
                    else
                    {
                        points = ((List<(DateTime Time, double Dps)>)history).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Dps
                        )).ToList();
                    }

                    if (points.Count > 0)
                    {
                        chart.AddSeries(playerName, points);
                    }
                }
                
                chart.TitleText = $"实时{dataTypeName}趋势图";
            }
            
            // 如果之前有数据且用户有过交互，恢复视图状态
            if (hadPreviousData && chart.HasUserInteracted())
            {
                chart.SetTimeScale(currentTimeScale);
                chart.SetViewOffset(currentViewOffset);
            }
        }

        #endregion

        #region 技能伤害饼图

        /// <summary>
        /// 创建技能伤害占比饼图
        /// </summary>
        public static FlatPieChart CreateSkillDamagePieChart(ulong playerId, int width = 400, int height = 400)
        {
            var chart = new FlatPieChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                ShowLabels = true,
                ShowPercentages = true,
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshSkillDamagePieChart(chart, playerId);
            return chart;
        }

        /// <summary>
        /// 刷新技能伤害饼图
        /// </summary>
        public static void RefreshSkillDamagePieChart(FlatPieChart chart, ulong playerId)
        {
            chart.ClearData();

            try
            {
                // 获取玩家技能数据
                var skillData = StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true);
                
                if (skillData.Count == 0)
                {
                    chart.TitleText = "技能伤害占比 - 暂无数据";
                    return;
                }

                // 获取玩家信息
                var playerInfo = StatisticData._manager.GetPlayerBasicInfo(playerId);
                var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{playerId}" : playerInfo.Nickname;
                chart.TitleText = $"{playerName} - 技能伤害占比";

                // 准备饼图数据
                var pieData = skillData.Select(s => (
                    Label: $"{s.SkillName}: {Common.FormatWithEnglishUnits(s.Total)}",
                    Value: (double)s.Total
                )).ToList();

                chart.SetData(pieData);
            }
            catch (Exception ex)
            {
                chart.TitleText = $"技能伤害占比 - 数据加载错误: {ex.Message}";
            }
        }

        #endregion

        #region 团队DPS对比条形图

        /// <summary>
        /// 创建团队DPS对比条形图
        /// </summary>
        public static FlatBarChart CreateTeamDpsBarChart(int width = 600, int height = 400)
        {
            var chart = new FlatBarChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "团队DPS对比",
                XAxisLabel = "玩家",
                YAxisLabel = "DPS",
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshTeamDpsBarChart(chart);
            return chart;
        }

        /// <summary>
        /// 刷新团队DPS对比条形图
        /// </summary>
        public static void RefreshTeamDpsBarChart(FlatBarChart chart)
        {
            chart.ClearData();

            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
            
            if (players.Count == 0)
            {
                chart.TitleText = "团队DPS对比 - 暂无数据";
                return;
            }

            // 按总DPS排序
            players = players.OrderByDescending(p => p.GetTotalDps()).ToList();

            // 准备条形图数据
            var barData = players.Select(p => (
                Label: string.IsNullOrEmpty(p.Nickname) ? $"玩家{p.Uid}" : p.Nickname,
                Value: p.GetTotalDps()
            )).ToList();

            chart.SetData(barData);
            chart.TitleText = "团队DPS对比";
        }

        #endregion

        #region 多维度散点图

        /// <summary>
        /// 创建多维度散点图
        /// </summary>
        public static FlatScatterChart CreateDpsRadarChart(int width = 400, int height = 400)
        {
            var chart = new FlatScatterChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "DPS与暴击率对比",
                XAxisLabel = "暴击率 (%)",
                YAxisLabel = "总DPS",
                ShowLegend = true,
                ShowGrid = true,
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshDpsRadarChart(chart);
            return chart;
        }

        /// <summary>
        /// 刷新多维度散点图
        /// </summary>
        public static void RefreshDpsRadarChart(FlatScatterChart chart)
        {
            chart.ClearSeries();

            var players = StatisticData._manager.GetPlayersWithCombatData().Take(5).ToList();
            
            if (players.Count == 0)
            {
                chart.TitleText = "DPS与暴击率对比 - 暂无数据";
                return;
            }

            foreach (var player in players)
            {
                var totalDps = player.GetTotalDps();
                var critRate = player.DamageStats.GetCritRate() * 100;
                
                var playerName = string.IsNullOrEmpty(player.Nickname) ? $"玩家{player.Uid}" : player.Nickname;
                var points = new List<PointF> { new PointF((float)critRate, (float)totalDps) };
                
                chart.AddSeries(playerName, points);
            }

            chart.TitleText = "DPS与暴击率对比";
        }

        #endregion

        #region 伤害类型分布条形图

        /// <summary>
        /// 创建伤害类型分布条形图
        /// </summary>
        public static FlatBarChart CreateDamageTypeStackedChart(int width = 600, int height = 400)
        {
            var chart = new FlatBarChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "玩家伤害类型分布",
                XAxisLabel = "玩家",
                YAxisLabel = "伤害值",
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshDamageTypeStackedChart(chart);
            return chart;
        }

        /// <summary>
        /// 刷新伤害类型分布条形图
        /// </summary>
        public static void RefreshDamageTypeStackedChart(FlatBarChart chart)
        {
            chart.ClearData();

            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
            
            if (players.Count == 0)
            {
                chart.TitleText = "玩家伤害类型分布 - 暂无数据";
                return;
            }

            // 按总伤害排序并限制显示数量
            players = players.OrderByDescending(p => p.DamageStats.Total).Take(6).ToList();

            // 准备数据：使用总伤害作为主要对比指标
            var barData = players.Select(p => (
                Label: string.IsNullOrEmpty(p.Nickname) ? $"玩家{p.Uid}" : p.Nickname,
                Value: (double)p.DamageStats.Total
            )).ToList();

            chart.SetData(barData);
            chart.TitleText = "玩家伤害类型分布";
        }

        #endregion

        #region 全局图表管理

        /// <summary>
        /// 注册的图表实例列表（用于全局控制）
        /// </summary>
        private static readonly List<WeakReference> _registeredCharts = new();

        /// <summary>
        /// 注册图表实例以便全局管理
        /// </summary>
        public static void RegisterChart(FlatLineChart chart)
        {
            lock (_registeredCharts)
            {
                // 清理已失效的弱引用
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
                
                // 添加新的弱引用
                _registeredCharts.Add(new WeakReference(chart));
            }
        }

        /// <summary>
        /// 停止所有注册的图表自动刷新
        /// </summary>
        public static void StopAllChartsAutoRefresh()
        {
            IsCapturing = false; // 清除捕获状态
            
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try
                        {
                            chart.StopAutoRefresh();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"停止图表自动刷新时出错: {ex.Message}");
                        }
                    }
                }
                
                // 清理失效的引用
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }

        /// <summary>
        /// 启动所有注册的图表自动刷新
        /// </summary>
        public static void StartAllChartsAutoRefresh(int intervalMs = 1000)
        {
            IsCapturing = true; // 设置捕获状态
            
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try
                        {
                            chart.StartAutoRefresh(intervalMs);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"启动图表自动刷新时出错: {ex.Message}");
                        }
                    }
                }
                
                // 清理失效的引用
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 检查是否有数据可显示
        /// </summary>
        public static bool HasDataToVisualize()
        {
            return StatisticData._manager.GetPlayersWithCombatData().Any();
        }

        /// <summary>
        /// 刷新所有打开的图表主题
        /// </summary>
        public static void RefreshAllChartThemes()
        {
            // 如果图表窗口打开着，刷新它的主题
            //if (Common.realtimeChartsForm != null && !Common.realtimeChartsForm.IsDisposed)
            //{
            //    Common.realtimeChartsForm.RefreshChartsTheme();
            //}
        }

        /// <summary>
        /// 获取DPS历史数据点数量（用于调试）
        /// </summary>
        public static int GetDpsHistoryPointCount()
        {
            return _dpsHistory.Sum(kvp => kvp.Value.Count);
        }

        #endregion
    }
}