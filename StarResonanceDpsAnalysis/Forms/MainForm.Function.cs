using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Extends;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System.Runtime.InteropServices;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class MainForm
    {
     



        #region RefreshDpsTable() 刷新DPS信息

        public static void RefreshDpsTable()
        {
            // 1) 拷贝一份所有玩家数据，避免并发修改
            List<PlayerData> statsList = StatisticData._manager
                .GetPlayersWithCombatData()
                .ToList();

            if (statsList.Count <= 0) return;
            // 2) 计算最大总伤害，用于归一化进度条
            float totalDamageSum = statsList
            .Where(p => p?.DamageStats != null)
            .Sum(p => (float)p.DamageStats.Total);

            if (totalDamageSum <= 0f) totalDamageSum = 1f;

            // 3) 遍历，新增或更新行
            foreach (var stat in statsList)
            {
                if (stat == null) continue;
                // 3.1 计算进度条比例
                float percent = (float)stat.DamageStats.Total / totalDamageSum;

                // 3.2 按 UID 查找已有行
                var row = DpsTableDatas.DpsTable
                    .FirstOrDefault(x => x.Uid == stat.Uid);

                // 3.3 计算暴击率/幸运率（以 % 计）
                double critRate = stat.DamageStats.CountTotal > 0
                                 ? (double)stat.DamageStats.CountCritical / stat.DamageStats.CountTotal * 100
                                 : 0.0;
                double luckyRate = stat.DamageStats.CountTotal > 0
                                 ? (double)stat.DamageStats.CountLucky / stat.DamageStats.CountTotal * 100
                                 : 0.0;


                if (row == null)
                {
                    // 新增一行
                    DpsTableDatas.DpsTable.Add(new DpsTable(
                         stat.Uid,
                         stat.Nickname,
                         stat.TakenDamage,
                         stat.HealingStats.Total,
                         stat.HealingStats.Critical,
                        stat.HealingStats.Lucky,
                        stat.HealingStats.CritLucky,
                        stat.HealingStats.RealtimeValue,
                        stat.HealingStats.RealtimeMax,
                         stat.Profession,
                        stat.DamageStats.Total,
                        stat.DamageStats.Critical,
                         stat.DamageStats.Lucky,
                        stat.DamageStats.CritLucky,
                        Math.Round(critRate, 1),
                        Math.Round(luckyRate, 1),
                       stat.DamageStats.RealtimeValue,     // 即时 DPS
                        stat.DamageStats.RealtimeMax,       // 峰值 DPS
                        Math.Round(stat.DamageStats.GetTotalPerSecond(), 1), // 总平均 DPS
                        Math.Round(stat.HealingStats.GetTotalPerSecond(), 1), // 总平均 HPS
                    new CellProgress(percent)
                    {
                        Size = new Size(200, 10),
                        Fill = AppConfig.DpsColor
                    },
                    stat.CombatPower

                    ));
                }
                else
                {
                    // —— 更新 DPS 部分 —— 
                    row.Profession = stat.Profession;
                    row.NickName = stat.Nickname;
                    row.TotalDamage = stat.DamageStats.Total.ToString();
                    row.CriticalDamage = stat.DamageStats.Critical.ToString();
                    row.LuckyDamage = stat.DamageStats.Lucky.ToString();
                    row.CritLuckyDamage = stat.DamageStats.CritLucky.ToString();
                    row.CritRate = Math.Round(critRate, 1).ToString();
                    row.LuckyRate = Math.Round(luckyRate, 1).ToString();
                    row.InstantDps = stat.DamageStats.RealtimeValue.ToString();
                    row.MaxInstantDps = stat.DamageStats.RealtimeMax.ToString();
                    row.TotalDps = Math.Round(stat.DamageStats.GetTotalPerSecond(), 1).ToString();

                    // —— 更新 HPS（治疗）部分 —— 
                    row.DamageTaken = stat.TakenDamage.ToString();
                    row.TotalHealingDone = stat.HealingStats.Total.ToString();
                    row.CriticalHealingDone = stat.HealingStats.Critical.ToString();
                    row.LuckyHealingDone = stat.HealingStats.Lucky.ToString();
                    row.CritLuckyHealingDone = stat.HealingStats.CritLucky.ToString();

                    row.InstantHps = stat.HealingStats.RealtimeValue.ToString();
                    row.MaxInstantHps = stat.HealingStats.RealtimeMax.ToString();
                    row.TotalHps = Math.Round(stat.HealingStats.GetTotalPerSecond(), 1).ToString();

                    row.CombatPower = stat.CombatPower;
                    // —— 更新进度条 —— 
                    if (row.CellProgress is CellProgress cp)
                    {
                        cp.Value = percent;  // 仅更新进度值
                    }
                    else
                    {
                        // 兼容：如果之前没创建过，就新建一个
                        row.CellProgress = new CellProgress(percent)
                        {
                            Size = new Size(200, 10),
                            Fill = AppConfig.DpsColor
                        };
                    }
                }

            }
        }

        #endregion






    }
}
