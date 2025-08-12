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
        public string sort = "Total";//排序
        public Func<SkillSummary, double> SkillOrderBySelector = s => s.Total;

        /// <summary>
        /// 刷新玩家技能数据
        /// </summary>
        public void UpdateSkillTable(ulong uid, bool isHeal = false)
        {
            SkillTableDatas.SkillTable.Clear();
            var skillType = isHeal
                ? StarResonanceDpsAnalysis.Core.SkillType.Heal
                : StarResonanceDpsAnalysis.Core.SkillType.Damage;

            var skills = StatisticData._manager
                .GetPlayerSkillSummaries(uid, topN: null, orderByTotalDesc: true, skillType)
                .OrderByDescending(SkillOrderBySelector)
                .ToList();



            foreach (var item in skills)
            {
                string critRateStr = $"{Math.Round(item.CritRate * 100, 1)}%";
                string luckyRateStr = $"{Math.Round(item.LuckyRate * 100, 1)}%";

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

            if (segmented1.SelectIndex == 0)
            {
                // ===== 伤害总览 =====
                TotalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalDps());
                CritRateText.Text = $"{(p.DamageStats.GetCritRate() * 100):0.#}%";
                LuckyRate.Text = $"{(p.DamageStats.GetLuckyRate() * 100):0.#}%";

                NormalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Normal);
                CritDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Critical);
                LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Lucky);
                AvgDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.GetAveragePerHit());

                // ===== 技能表（伤害）=====
                UpdateSkillTable(Uid, false);
            }
            else
            {
                // ===== 治疗总览 =====
                TotalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Total);
                TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalHps());
                CritRateText.Text = $"{(p.HealingStats.GetCritRate() * 100):0.#}%";
                LuckyRate.Text = $"{(p.HealingStats.GetLuckyRate() * 100):0.#}%";

                NormalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Normal);
                CritDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Critical);
                LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Lucky);
                AvgDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.GetAveragePerHit());

                // ===== 技能表（治疗）=====
                UpdateSkillTable(Uid, true);
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

                // 获取当前模式下的统计数据
                var stats = segmented1.SelectIndex == 0 ? p.DamageStats : p.HealingStats;

                var critRate = stats.GetCritRate() * 100;
                var luckyRate = stats.GetLuckyRate() * 100;
                var normalRate = 100 - critRate - luckyRate;

                var chartData = new List<(string, double)>();

                if (normalRate > 0)
                    chartData.Add(("普通", normalRate));
                if (critRate > 0)
                    chartData.Add(("暴击", critRate));
                if (luckyRate > 0)
                    chartData.Add(("幸运", luckyRate));

                _skillDistributionChart.SetData(chartData);
                // 移除Y轴标签设置，去掉右侧的百分比文字
                // _skillDistributionChart.YAxisLabel = "百分比(%)";
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
                // 获取当前模式下的技能数据
                bool isHeal = segmented1.SelectIndex != 0;
                var skillType = isHeal
                    ? StarResonanceDpsAnalysis.Core.SkillType.Heal
                    : StarResonanceDpsAnalysis.Core.SkillType.Damage;

                var skills = StatisticData._manager
                    .GetPlayerSkillSummaries(Uid, topN: 10, orderByTotalDesc: true, skillType)
                    .ToList();

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
