using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SkillDetailForm
    {
        public void ToggleTableView()
        {

            table_DpsDetailDataTable.Columns.Clear();

            table_DpsDetailDataTable.Columns = new AntdUI.ColumnCollection
            {
                new AntdUI.Column("Name","技能名"),
                new AntdUI.Column("Damage","伤害"),
                new AntdUI.Column("AvgPerHit","DPS/秒"),
                new AntdUI.Column("HitCount","命中次数"),
                new AntdUI.Column("CritRate","暴击率"),
                new AntdUI.Column("LuckyRate","幸运率"),
                new AntdUI.Column("Share","占比"),
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
        public void UpdateSkillTable(ulong uid, bool isHeal = false,bool AcceptInjury=false)
        {
            SkillTableDatas.SkillTable.Clear();
            var skillType = isHeal
                ? StarResonanceDpsAnalysis.Core.SkillType.Heal
                : StarResonanceDpsAnalysis.Core.SkillType.Damage;

            var skills = StatisticData._manager
                .GetPlayerSkillSummaries(uid, topN: null, orderByTotalDesc: true, skillType)
                .OrderByDescending(SkillOrderBySelector)
                .ToList();
            //是否属于承伤数据
            if(AcceptInjury)
            {
                skills = StatisticData._manager.GetPlayerTakenDamageSummaries(uid,null,true)
                .OrderByDescending(SkillOrderBySelector)
                .ToList();
            }


            foreach (var item in skills)
            {

                string critRateStr = AcceptInjury ? item.CritRate.ToString() :$"{item.CritRate}%";
                string luckyRateStr = AcceptInjury? item.LuckyRate.ToString() :$"{item.LuckyRate}%";

                var existing = SkillTableDatas.SkillTable.FirstOrDefault(s => s.Name == item.SkillName);
                if (existing != null)
                {
                    existing.Damage = new CellText(item.Total.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.HitCount = new CellText(item.HitCount.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.CritRate = new CellText(critRateStr) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.LuckyRate = new CellText(luckyRateStr) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.AvgPerHit = new CellText(item.AvgPerHit.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.TotalDps = new CellText(item.TotalDps.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.Percentage = new CellText((item.ShareOfTotal).ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.Share = new CellProgress((float)item.ShareOfTotal)
                    {
                        Fill = AppConfig.DpsColor,
                        Size = new Size(200, 10),
                    };
                }
                else
                {
                    SkillTableDatas.SkillTable.Add(new SkillData(
                        item.SkillName,
                        null,
                        item.Total,
                        item.HitCount,
                        critRateStr,
                        luckyRateStr,
                        item.ShareOfTotal,
                        item.AvgPerHit,
                        item.TotalDps
                    )
                    {
                        Share = new CellProgress((float)item.ShareOfTotal)
                        {
                            Fill = AppConfig.DpsColor,
                            Size = new Size(200, 10)
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 更新技能表
        /// </summary>
        public void SelectDataType()
        {
            var p = StatisticData._manager.GetOrCreate(Uid);

            switch(segmented1.SelectIndex)
            {
                case 0:
                    // ===== 伤害总览 =====
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalDps());
                    CritRateText.Text = $"{p.DamageStats.GetCritRate()}%";
                    LuckyRate.Text = $"{p.DamageStats.GetLuckyRate()}%";

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Normal);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.GetAveragePerHit());

                    // ===== 技能表（伤害）=====
                    UpdateSkillTable(Uid, false);
                    break;

                case 1:
                    // ===== 治疗总览 =====
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Total);
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalHps());
                    CritRateText.Text = $"{p.HealingStats.GetCritRate()}%";
                    LuckyRate.Text = $"{p.HealingStats.GetLuckyRate()}%";

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Normal);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.GetAveragePerHit());

                    // ===== 技能表（治疗）=====
                    UpdateSkillTable(Uid, true);
                    break;
                    case 2:
                    var takenDamages = StatisticData._manager.GetPlayerTakenOverview(Uid);
                    TotalDamageText.Text = Common.FormatWithEnglishUnits(takenDamages.Total);//总承伤
                    TotalDpsText.Text = Common.FormatWithEnglishUnits(takenDamages.AvgTakenPerSec);//平均每秒承伤
                    CritRateText.Text = Common.FormatWithEnglishUnits(takenDamages.MaxSingleHit);//单次最大承伤
                    CritDamageText.Text = Common.FormatWithEnglishUnits(takenDamages.MinSingleHit);//单次最小承伤

                    NormalDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Total);
                    CritDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Critical);
                    LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.Lucky);
                    AvgDamageText.Text = Common.FormatWithEnglishUnits(p.TakenStats.GetAveragePerHit());
                    UpdateSkillTable(Uid, false,true);

                    break;
            }


            // 更新图表数据以反映当前选择的数据类型（伤害/治疗）
            if (_dpsTrendChart != null)
            {
                try
                {
                    // 刷新图表数据，确保显示当前玩家的最新数据
                    RefreshDpsTrendChart();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"更新图表数据时出错: {ex.Message}");
                }
            }

            // 更新条形图和饼图数据
            UpdateSkillDistributionChart();
            UpdateCritLuckyChart();
        }

        /// <summary>
        /// 更新暴击率与幸运率条形图数据（现在条形图显示暴击率数据）
        /// </summary>
        private void UpdateSkillDistributionChart()
        {
            if (_skillDistributionChart == null) return;

            try
            {
                var p = StatisticData._manager.GetOrCreate(Uid);

                // 根据当前模式获取对应的统计数据
                var stats = segmented1.SelectIndex switch
                {
                    0 => p.DamageStats,      // 伤害数据
                    1 => p.HealingStats,     // 治疗数据  
                    2 => p.TakenStats,       // 承伤数据
                    _ => p.DamageStats       // 默认伤害数据
                };

                var critRate = stats.GetCritRate();
                var luckyRate = stats.GetLuckyRate();
                var normalRate = 100 - critRate - luckyRate;

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
        /// 更新技能占比饼图数据（现在饼图显示技能分布数据）
        /// </summary>
        private void UpdateCritLuckyChart()
        {
            if (_critLuckyChart == null) return;

            try
            {
                List<SkillSummary> skills;

                // 根据当前模式获取相应的技能数据
                switch (segmented1.SelectIndex)
                {
                    case 0: // 伤害分析
                        skills = StatisticData._manager
                            .GetPlayerSkillSummaries(Uid, topN: 10, orderByTotalDesc: true, StarResonanceDpsAnalysis.Core.SkillType.Damage)
                            .ToList();
                        break;

                    case 1: // 治疗分析
                        skills = StatisticData._manager
                            .GetPlayerSkillSummaries(Uid, topN: 10, orderByTotalDesc: true, StarResonanceDpsAnalysis.Core.SkillType.Heal)
                            .ToList();
                        break;

                    case 2: // 承伤分析
                        skills = StatisticData._manager
                            .GetPlayerTakenDamageSummaries(Uid, topN: 10, orderByTotalDesc: true)
                            .ToList();
                        break;

                    default:
                        skills = new List<SkillSummary>();
                        break;
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
