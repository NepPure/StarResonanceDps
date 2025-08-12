using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public string sort= "Total";//排序
        public Func<SkillSummary, double> SkillOrderBySelector = s => s.Total;

        /// <summary>
        /// 刷新玩家技能数据
        /// </summary>
        public void UpdateSkillTable(ulong uid, bool isHeal=false)
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
                    existing.Damage =new CellText(item.Total.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) }; 
                    existing.HitCount =new CellText( item.HitCount.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.CritRate = new CellText(critRateStr) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.LuckyRate = new CellText(luckyRateStr) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.AvgPerHit = new CellText(item.AvgPerHit.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.TotalDps = new CellText(item.TotalDps.ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular) };
                    existing.Percentage = new CellText((item.ShareOfTotal).ToString()) { Font = new Font("SAO Welcome TT", 8, FontStyle.Regular)};
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
                    
                    // 根据当前选择的模式更新图表标题
                    if (segmented1.SelectIndex == 0)
                    {
                        //_dpsTrendChart.YAxisLabel = "DPS";
                        //var playerInfo = StatisticData._manager.GetPlayerBasicInfo(Uid);
                        //var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{Uid}" : playerInfo.Nickname;
                        //_dpsTrendChart.TitleText = $"{playerName} - 实时DPS趋势";
                    }
                    else
                    {
                        //_dpsTrendChart.YAxisLabel = "HPS";
                        //var playerInfo = StatisticData._manager.GetPlayerBasicInfo(Uid);
                        //var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"玩家{Uid}" : playerInfo.Nickname;
                        //_dpsTrendChart.TitleText = $"{playerName} - 实时HPS趋势";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"更新图表数据时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 更新玩家信息
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="power"></param>
        /// <param name="profession"></param>
        public void GetPlayerInfo(string nickname, int power, string profession)
        {
            NickNameText.Text = nickname;
            PowerText.Text = power.ToString();
           
            UidText.Text = Uid.ToString();

          
            object? resourceObj = Properties.Resources.ResourceManager.GetObject(profession);

            if (resourceObj is byte[] bytes)
            {
                using var ms = new MemoryStream(bytes);
                table_DpsDetailDataTable.BackgroundImage = Image.FromStream(ms);
            }
            else if (resourceObj is Image img)
            {
                table_DpsDetailDataTable.BackgroundImage = img;
            }
            else
            {
                table_DpsDetailDataTable.BackgroundImage = null; // 默认空白
            }
            
            // 更新玩家信息后，重新初始化图表以显示新玩家的数据
            if (_dpsTrendChart != null)
            {
                // 重新设置刷新回调以使用新的玩家ID
                _dpsTrendChart.SetRefreshCallback(() => {
                    try
                    {
                        // 只有在正在捕获数据时才更新数据点，避免停止抓包后继续显示虚假数据
                        if (ChartVisualizationService.IsCapturing)
                        {
                            ChartVisualizationService.UpdateAllDataPoints();
                        }
                        
                        // 根据当前选择的模式决定显示DPS还是HPS
                        bool showHps = segmented1.SelectIndex != 0; // 0是伤害，1是治疗
                        ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, showHps);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"图表刷新回调出错: {ex.Message}");
                    }
                });
                
                // 立即刷新图表数据
                RefreshDpsTrendChart();
            }
        }
    }
}
