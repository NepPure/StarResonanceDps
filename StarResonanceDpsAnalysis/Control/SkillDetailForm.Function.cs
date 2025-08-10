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
            UpdateSkillTable(Uid, false);

        }

        public ulong Uid;//存放用户uid
        public string Nickname;//存放用户昵称
        public int Power;//战力
        public string Profession;//职业

        /// <summary>
        /// 刷新玩家技能数据
        /// </summary>
        private void UpdateSkillTable(ulong uid, bool isHeal)
        {
            var skillType = isHeal
                ? StarResonanceDpsAnalysis.Core.SkillType.Heal
                : StarResonanceDpsAnalysis.Core.SkillType.Damage;

            var skills = StatisticData._manager
                .GetPlayerSkillSummaries(uid, topN: null, orderByTotalDesc: true, skillType)
                .ToList();

            foreach (var item in skills)
            {
                string critRateStr = $"{Math.Round(item.CritRate * 100, 1)}%";
                string luckyRateStr = $"{Math.Round(item.LuckyRate * 100, 1)}%";

                var existing = SkillTableDatas.SkillTable.FirstOrDefault(s => s.Name == item.SkillName);
                if (existing != null)
                {
                    existing.Damage = item.Total.ToString();
                    existing.HitCount = item.HitCount;
                    existing.CritRate = critRateStr;
                    existing.LuckyRate = luckyRateStr;
                    existing.AvgPerHit = item.AvgPerHit.ToString();
                    existing.TotalDps = item.TotalDps.ToString();
                    existing.Share = new CellProgress((float)item.ShareOfTotal)
                    {
                        Fill = AppConfig.DpsColor,
                        Size = new Size(200, 10)
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
