using AntdUI;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using static StarResonanceDpsAnalysis.Forms.DpsStatisticsForm;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SkillDetailForm
    {
        public void ToggleTableView()
        {

            table_DpsDetailDataTable.Columns.Clear();

            table_DpsDetailDataTable.Columns = new AntdUI.ColumnCollection
            {
                 //new AntdUI.Column("SkillId","技能id"),
                new AntdUI.Column("Name","技能名"),
                new AntdUI.Column("Damage","伤害"),
                new AntdUI.Column("AvgPerHit","DPS/秒"),
                new AntdUI.Column("HitCount","命中次数"),
                new AntdUI.Column("CritRate","暴击率"),
                new AntdUI.Column("LuckyRate","幸运率"),
               // new AntdUI.Column("Share","占比"),
                new AntdUI.Column("Percentage","百分比"),
            };

            table_DpsDetailDataTable.Binding(SkillTableDatas.SkillTable);


        }

        public ulong Uid;//存放用户uid
        public string Nickname;//存放用户昵称
        public int Power;//战力
        public string Profession;//职业
      
        public Func<SkillSummary, double> SkillOrderBySelector = s => s.Total;

        /// <summary>
        /// 刷新玩家技能数据
        /// </summary>
        public void UpdateSkillTable(ulong uid, SourceType source, MetricType metric)
        {
            SkillTableDatas.SkillTable.Clear();

            // 取技能清单（统一成同样的结构）
            List<StarResonanceDpsAnalysis.Plugin.DamageStatistics.SkillSummary> skills;
            if (source == SourceType.Current)
            {
                if (metric == MetricType.Taken)
                {
                    skills = StatisticData._manager
                                .GetPlayerTakenDamageSummaries(uid, null, true)
                                .OrderByDescending(SkillOrderBySelector)
                                .ToList();
                }
                else
                {
                    var skillType = metric == MetricType.Healing
                        ? StarResonanceDpsAnalysis.Core.SkillType.Heal
                        : StarResonanceDpsAnalysis.Core.SkillType.Damage;

                    skills = StatisticData._manager
                                .GetPlayerSkillSummaries(uid, topN: null, orderByTotalDesc: true, skillType)
                                .OrderByDescending(SkillOrderBySelector)
                                .ToList();
                }
            }
            else
            {
                var (damageSkills, healingSkills, takenSkills) = FullRecord.GetPlayerSkills(uid); // 
                skills = metric switch
                {
                    MetricType.Healing => healingSkills.OrderByDescending(SkillOrderBySelector).ToList(),
                    MetricType.Taken => takenSkills.OrderByDescending(SkillOrderBySelector).ToList(),
                    _ => damageSkills.OrderByDescending(SkillOrderBySelector).ToList()
                };
            }

            // 计算 ShareOfTotal（全程/单次统一口径）
            double grandTotal = skills.Sum(s => (double)s.Total);
            foreach (var item in skills)
            {
                double share = grandTotal > 0 ? (double)item.Total / grandTotal : 0.0;

                string critRateStr = $"{item.CritRate}%";
                string luckyRateStr = $"{item.LuckyRate}%";
                // 承伤时，如果你不想显示百分号，可按需改成纯数值字符串

                var existing = SkillTableDatas.SkillTable.FirstOrDefault(s => s.SkillId == item.SkillId);
                if (existing == null)
                {
                    SkillTableDatas.SkillTable.Add(new SkillData(
                        item.SkillId,
                        item.SkillName,
                        null,
                        item.Total,
                        item.HitCount,
                        critRateStr,
                        luckyRateStr,
                        share,
                        item.AvgPerHit,
                        item.TotalDps
                    )
                    {
                        Share = new CellProgress((float)share)
                        {
                            Fill = AppConfig.DpsColor,
                            Size = new Size(200, 10)
                        }
                    });
                }
                else
                {
                    existing.SkillId = item.SkillId;
                    existing.Name = item.SkillName;
                    existing.Damage = new CellText(item.Total.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.HitCount = new CellText(item.HitCount.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.CritRate = new CellText(critRateStr) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.LuckyRate = new CellText(luckyRateStr) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.AvgPerHit = new CellText(item.AvgPerHit.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.TotalDps = new CellText(item.TotalDps.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.Percentage = new CellText(share.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.Share = new CellProgress((float)share)
                    {
                        Fill = AppConfig.DpsColor,
                        Size = new Size(200, 10),
                    };
                }
            }
        }


        /// <summary>
        /// 更新技能表
        /// </summary>
        public void SelectDataType()
        {
            var source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;
            var metric = segmented1.SelectIndex switch
            {
                1 => MetricType.Healing,
                2 => MetricType.Taken,
                _ => MetricType.Damage
            };

            // 同步折线图数据源（不清历史，避免频繁闪烁）
            ChartVisualizationService.SetDataSource(FormManager.showTotal ? ChartDataSource.FullRecord : ChartDataSource.Current, clearHistory: false);

            // === 顶部总览 ===
            if (source == SourceType.Current)
            {
                var p = StatisticData._manager.GetOrCreate(Uid);

                if (metric == MetricType.Damage)
                {
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalDps());
                    CritRateText.Text = $"{p.DamageStats.GetCritRate()}%";
                    LuckyRate.Text = $"{p.DamageStats.GetLuckyRate()}%";


                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Normal);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.GetAveragePerHit());

                    NumberHitsLabel.Text = Common.FormatWithEnglishUnits(p.DamageStats.CountTotal);//命中次数
                    NumberCriticalHitsLabel.Text = Common.FormatWithEnglishUnits(p.DamageStats.CountCritical);//暴击次数
                    LuckyTimesLabel.Text = Common.FormatWithEnglishUnits(p.DamageStats.CountLucky);//幸运次数
                    BeatenLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountTotal);//挨打次数
                }
                else if (metric == MetricType.Healing)
                {
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalHps());
                    CritRateText.Text = $"{p.HealingStats.GetCritRate()}%";
                    LuckyRate.Text = $"{p.HealingStats.GetLuckyRate()}%";

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Normal);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.GetAveragePerHit());

                    NumberHitsLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountTotal);//命中次数
                    NumberCriticalHitsLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountCritical);//暴击次数
                    LuckyTimesLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountLucky);//幸运次数
                    BeatenLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountTotal);//挨打次数
                }
                else // Taken
                {
                    var taken = StatisticData._manager.GetPlayerTakenOverview(Uid);
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(taken.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(taken.AvgTakenPerSec);
                    CritRateText.Text = Common.FormatWithEnglishUnits(taken.MaxSingleHit);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(taken.MinSingleHit);

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Total);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Critical);//承伤
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.GetAveragePerHit());
                    LuckyRate.Text = "0";

                   NumberHitsLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountTotal);//命中次数
                    NumberCriticalHitsLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountCritical);//暴击次数
                    LuckyTimesLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountLucky);//幸运次数
                    BeatenLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountTotal);//挨打次数
                }
            }
            else // === 全程 FullRecord ===
            {
                var p = FullRecord.Shim.GetOrCreate(Uid);

                if (metric == MetricType.Damage)
                {
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalDps());
                    CritRateText.Text = $"{p.DamageStats.GetCritRate()}%";
                    LuckyRate.Text = $"{p.DamageStats.GetLuckyRate()}%";

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Normal);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.GetAveragePerHit());


                    NumberHitsLabel.Text = Common.FormatWithEnglishUnits(p.DamageStats.CountTotal);//命中次数
                    NumberCriticalHitsLabel.Text = Common.FormatWithEnglishUnits(p.DamageStats.CountCritical);//暴击次数
                    LuckyTimesLabel.Text = Common.FormatWithEnglishUnits(p.DamageStats.CountLucky);//幸运次数
                    BeatenLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountTotal);//挨打次数
                }
                else if (metric == MetricType.Healing)
                {
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalHps());
                    CritRateText.Text = $"{p.HealingStats.GetCritRate()}%";
                    LuckyRate.Text = $"{p.HealingStats.GetLuckyRate()}%";

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Normal);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.GetAveragePerHit());

                    NumberHitsLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountTotal);//命中次数
                    NumberCriticalHitsLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountCritical);//暴击次数
                    LuckyTimesLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountLucky);//幸运次数
                    BeatenLabel.Text = Common.FormatWithEnglishUnits(p.HealingStats.CountTotal);//挨打次数
                }
                else // Taken
                {
                    var taken = FullRecord.Shim.GetPlayerTakenOverview(Uid);
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(taken.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(taken.AvgTakenPerSec);
                    CritRateText.Text = Common.FormatWithEnglishUnits(taken.MaxSingleHit); // 你原本就把这里用来显示“单次最大”
                    CritDamageText.Text = Common.FormatWithEnglishUnits(taken.MinSingleHit); // 你原本就把这里用来显示“单次最小”

                    LuckyRate.Text = "0";
                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Total);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.GetAveragePerHit());

                    NumberHitsLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountTotal);//命中次数
                    NumberCriticalHitsLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountCritical);//暴击次数
                    LuckyTimesLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountLucky);//幸运次数
                    BeatenLabel.Text = Common.FormatWithEnglishUnits(p.TakenStats.CountTotal);//挨打次数
                }
            }

            // === 技能表 ===
            UpdateSkillTable(Uid, source, metric);

            // 图表刷新
            if (_dpsTrendChart != null)
            {
                try { RefreshDpsTrendChart(); } catch (Exception ex) { Console.WriteLine($"更新图表数据时出错: {ex.Message}"); }
            }
            UpdateSkillDistributionChart();
            UpdateCritLuckyChart();
        }

        /// <summary>
        /// 更新暴击率与幸运率条形图数据（根据单次/全程与指标切换）
        /// </summary>
        private void UpdateSkillDistributionChart()
        {
            if (_skillDistributionChart == null) return;

            try
            {
                var source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;
                var metric = segmented1.SelectIndex switch
                {
                    1 => MetricType.Healing,
                    2 => MetricType.Taken,
                    _ => MetricType.Damage
                };

                double critRate, luckyRate;
                if (source == SourceType.Current)
                {
                    var p = StatisticData._manager.GetOrCreate(Uid);
                    var stats = metric switch
                    {
                        MetricType.Healing => p.HealingStats,
                        MetricType.Taken => p.TakenStats,
                        _ => p.DamageStats
                    };
                    critRate = stats.GetCritRate();
                    luckyRate = stats.GetLuckyRate();
                }
                else
                {
                    var p = FullRecord.Shim.GetOrCreate(Uid);
                    var stats = metric switch
                    {
                        MetricType.Healing => p.HealingStats,
                        MetricType.Taken => p.TakenStats,
                        _ => p.DamageStats
                    };
                    critRate = stats.GetCritRate();
                    luckyRate = stats.GetLuckyRate();
                }

                var normalRate = Math.Max(0, 100 - critRate - luckyRate);

                var chartData = new List<(string, double)>();

                if (normalRate > 0)
                    chartData.Add(("普通", normalRate));
                if (critRate > 0)
                    chartData.Add(("暴击", critRate));
                if (luckyRate > 0)
                    chartData.Add(("幸运", luckyRate));

                _skillDistributionChart.SetData(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新暴击率与幸运率图表时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新技能占比饼图数据（根据单次/全程与指标切换）
        /// </summary>
        private void UpdateCritLuckyChart()
        {
            if (_critLuckyChart == null) return;

            try
            {
                var source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;
                var metric = segmented1.SelectIndex switch
                {
                    1 => MetricType.Healing,
                    2 => MetricType.Taken,
                    _ => MetricType.Damage
                };

                List<SkillSummary> skills;
                if (source == SourceType.Current)
                {
                    // 当前战斗
                    switch (metric)
                    {
                        case MetricType.Healing:
                            skills = StatisticData._manager
                                .GetPlayerSkillSummaries(Uid, topN: 10, orderByTotalDesc: true, StarResonanceDpsAnalysis.Core.SkillType.Heal)
                                .ToList();
                            break;
                        case MetricType.Taken:
                            skills = StatisticData._manager
                                .GetPlayerTakenDamageSummaries(Uid, topN: 10, orderByTotalDesc: true)
                                .ToList();
                            break;
                        default:
                            skills = StatisticData._manager
                                .GetPlayerSkillSummaries(Uid, topN: 10, orderByTotalDesc: true, StarResonanceDpsAnalysis.Core.SkillType.Damage)
                                .ToList();
                            break;
                    }
                }
                else
                {
                    // 全程
                    var (damageSkills, healingSkills, takenSkills) = FullRecord.GetPlayerSkills(Uid);
                    skills = metric switch
                    {
                        MetricType.Healing => healingSkills.Take(10).ToList(),
                        MetricType.Taken => takenSkills.Take(10).ToList(),
                        _ => damageSkills.Take(10).ToList()
                    };
                }

                var chartData = skills.Select(skill =>
                    (skill.SkillName, (double)skill.Total)
                ).ToList();

                _critLuckyChart.SetData(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新技能占比图表时出错: {ex.Message}");
            }
        }
    }
}
