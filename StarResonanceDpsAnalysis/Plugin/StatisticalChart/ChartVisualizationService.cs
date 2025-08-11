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

            // 只有当DPS值有意义时才添加数据点
            if (dps >= 0) // 允许0值，但过滤负值
            {
                history.Add((now, dps));
            }

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

            // 只有当HPS值有意义时才添加数据点
            if (hps >= 0) // 允许0值，但过滤负值
            {
                history.Add((now, hps));
            }

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
            
            foreach (var player in players)
            {
                // 使用总DPS而不是实时DPS，以获得更平滑的曲线
                var dps = player.GetTotalDps();
                var hps = player.GetTotalHps();

                // 总是添加数据点，即使是0，这样能保持连续性
                AddDpsDataPoint(player.Uid, dps);
                if (hps > 0) AddHpsDataPoint(player.Uid, hps);
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
        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400)
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

            RefreshDpsTrendChart(chart);
            return chart;
        }

        /// <summary>
        /// 刷新DPS趋势图数据
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart)
        {
            chart.ClearSeries();

            if (_dpsHistory.Count == 0 || _combatStartTime == null)
            {
                return;
            }

            var startTime = _combatStartTime.Value;

            // 按玩家ID排序，确保数据加载的一致性
            var sortedHistory = _dpsHistory.OrderBy(x => x.Key);

            foreach (var kvp in sortedHistory)
            {
                var playerId = kvp.Key;
                var history = kvp.Value;

                if (history.Count == 0) continue;

                // 获取玩家信息
                var playerInfo = StatisticData._manager.GetPlayerBasicInfo(playerId);
                var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{playerId}" : playerInfo.Nickname;

                // 转换为相对时间（秒）和DPS值的点集合
                var points = history.Select(h => new PointF(
                    (float)(h.Time - startTime).TotalSeconds,
                    (float)h.Dps
                )).ToList();

                if (points.Count > 0)
                {
                    chart.AddSeries(playerName, points);
                }
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