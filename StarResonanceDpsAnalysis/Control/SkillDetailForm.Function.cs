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
            };

            table_DpsDetailDataTable.Binding(SkillTableDatas.SkillTable);
            LoadPlayerSkillsToTable();

        }

        public ulong Uid;//存放用户uid
        public string Nickname;//存放用户昵称
        public int Power;//战力
        public string Profession;//职业

        /// <summary>
        /// 刷新玩家技能数据
        /// </summary>
        public void LoadPlayerSkillsToTable()
        {
            var p = StatisticData._manager.GetOrCreate(Uid);

            // 伤害信息
            TotalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Total);                 // 总伤害
            TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalDps());                      // 秒伤
            CritRateText.Text= $"{(p.DamageStats.GetCritRate() * 100):0.#}%";       // 暴击率
            LuckyRate.Text = $"{(p.DamageStats.GetLuckyRate() * 100):0.#}%";      // 幸运率

            // 伤害分布
            NormalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Normal);                // 普通伤害
            CritDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Critical);              // 暴击伤害
            LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Lucky);                  // 幸运伤害
            AvgDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.GetAveragePerHit());    // 平均伤害


            // 先取原始数据并按伤害降序
            var playerSkill = StatisticData._manager
                .GetPlayerSkillSummaries(Uid)
                .OrderByDescending(s => s.Total) // 直接用原始 ulong
                .ToList();

            foreach (var item in playerSkill)
            {
                string critRateStr = $"{Math.Round(item.CritRate * 100, 1)}%";
                string luckyRateStr = $"{Math.Round(item.LuckyRate * 100, 1)}%";

                var existing = SkillTableDatas.SkillTable.FirstOrDefault(s => s.Name == item.SkillName);
                if (existing != null)
                {
                    existing.Damage =item.Total.ToString();
                    existing.HitCount = item.HitCount;
                    existing.CritRate = critRateStr;
                    existing.LuckyRate = luckyRateStr;
                    existing.Share = new CellProgress((float)item.ShareOfTotal) { Fill = AppConfig.DpsColor, Size = new Size(200, 10) };
                    existing.AvgPerHit =item.AvgPerHit.ToString();
                    existing.TotalDps =item.TotalDps.ToString();
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
                    ));
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
            ProfessionText.Text = profession;
            UidText.Text = Uid.ToString();
        }

    }
}
