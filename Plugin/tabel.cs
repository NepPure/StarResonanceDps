using AntdUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 星痕共鸣DPS统计.Plugin
{

    public class tabel
    {
        /// <summary>
        /// 表格数据绑定
        /// </summary>
        public static AntdUI.AntList<DpsTabel> dps_tabel = new AntdUI.AntList<DpsTabel>();
        
    }
  

    public class DpsTabel: AntdUI.NotifyProperty
    {

        // —— DPS 相关私有字段 —— 

        /// <summary>玩家的唯一标识 UID</summary>
        private ulong Uid;
        private string NickName;

        /// <summary>玩家的职业/角色名称</summary>
        private string Profession;

        /// <summary>该玩家造成的总伤害</summary>
        private ulong TotalDamage;

        /// <summary>该玩家通过暴击造成的伤害总量</summary>
        private ulong CriticalDamage;

        /// <summary>该玩家通过幸运造成的伤害总量</summary>
        private ulong LuckyDamage;

        /// <summary>同时满足暴击和幸运条件的伤害总量</summary>
        private ulong CritLuckyDamage;

        /// <summary>格式化后的暴击率（字符串，带“%”）</summary>
        private string CritRate;

        /// <summary>格式化后的幸运率（字符串，带“%”）</summary>
        private string LuckyRate;

        /// <summary>最近 1 秒内的瞬时 DPS（伤害/秒）</summary>
        private ulong InstantDPS;

        /// <summary>统计期间出现过的最大瞬时 DPS</summary>
        private ulong MaxInstantDPS;

        /// <summary>平均总 DPS（总伤害 ÷ 战斗持续秒数）</summary>
        private double TotalDPS;

        /// <summary>用于在 UI 中展示进度条的 CellProgress 对象</summary>
        private CellProgress progress;


        // —— HPS 相关私有字段 —— 

        /// <summary>累计受到的伤害（该玩家受到的总伤害）</summary>
        private ulong DamageTaken;

        /// <summary>该玩家提供的总治疗量</summary>
        private ulong TotalHealingDone;

        /// <summary>该玩家通过暴击造成的治疗总量</summary>
        private ulong CriticalHealingDone;

        /// <summary>该玩家通过幸运造成的治疗总量</summary>
        private ulong LuckyHealingDone;

        /// <summary>同时满足暴击和幸运条件的治疗总量</summary>
        private ulong CritLuckyHealingDone;

        /// <summary>最近 1 秒内的瞬时 HPS（治疗/秒）</summary>
        private ulong InstantHps;

        /// <summary>统计期间出现过的最大瞬时 HPS</summary>
        private ulong MaxInstantHps;

        /// <summary>平均总 HPS（总治疗 ÷ 战斗持续秒数）</summary>
        private double TotalHps;



        /// <summary>
        /// 构造一个用于展示伤害（DPS）和治疗（HPS）统计的 DpsTabel 实例（通常用于表格绑定）。
        /// </summary>
        /// <param name="uid">角色 UID，唯一标识一个玩家。</param>
        /// <param name="takenDamage">该玩家受到的总伤害。</param>
        /// <param name="totalHealing">该玩家提供的总治疗量。</param>
        /// <param name="totalCriticalHealing">该玩家暴击治疗的总量。</param>
        /// <param name="totalLuckyHealing">该玩家幸运治疗的总量。</param>
        /// <param name="totalCritLuckyHealing">同时满足暴击和幸运条件的治疗总量。</param>
        /// <param name="totalInstantHps">最近 1 秒内的瞬时 HPS（治疗/秒）。</param>
        /// <param name="totalMaxInstantHps">统计期间出现过的最大瞬时 HPS。</param>
        /// <param name="profession">玩家的职业/角色名称。</param>
        /// <param name="totalDamage">该玩家造成的总伤害。</param>
        /// <param name="criticalDamage">该玩家通过暴击造成的伤害总量。</param>
        /// <param name="luckyDamage">该玩家通过幸运造成的伤害总量。</param>
        /// <param name="critLuckyDamage">同时满足暴击和幸运的伤害总量。</param>
        /// <param name="critRate">暴击命中率（0～100 之间的小数，构造后会格式化为字符串并附带“%”）。</param>
        /// <param name="luckyRate">幸运命中率（0～100 之间的小数，构造后会格式化为字符串并附带“%”）。</param>
        /// <param name="instantDPS">最近 1 秒内的瞬时 DPS（伤害/秒）。</param>
        /// <param name="maxInstantDPS">统计期间出现过的最大瞬时 DPS。</param>
        /// <param name="totalDPS">平均总 DPS（总伤害 ÷ 战斗持续秒数）。</param>
        /// <param name="totalHps">平均总 HPS（总治疗 ÷ 战斗持续秒数）。</param>
        /// <param name="cellProgress">用于在 UI 中展示进度条的 CellProgress 对象。</param>
        public DpsTabel(
            ulong uid,
            string nickname,
            ulong takenDamage,
            ulong totalHealing,
            ulong totalCriticalHealing,
            ulong totalLuckyHealing,
            ulong totalCritLuckyHealing,
            ulong totalInstantHps,
            ulong totalMaxInstantHps,
            string profession,
            ulong totalDamage,
            ulong criticalDamage,
            ulong luckyDamage,
            ulong critLuckyDamage,
            double critRate,
            double luckyRate,
            ulong instantDPS,
            ulong maxInstantDPS,
            double totalDPS,
            double totalHps,
            CellProgress cellProgress = null)
        {
            // DPS 相关
            this.Uid = uid;
            this.Profession = profession;
            this.TotalDamage = totalDamage;
            this.CriticalDamage = criticalDamage;
            this.LuckyDamage = luckyDamage;
            this.CritLuckyDamage = critLuckyDamage;
            this.CritRate = @$"{critRate}%";
            this.LuckyRate = @$"{luckyRate}%";
            this.InstantDPS = instantDPS;
            this.MaxInstantDPS = maxInstantDPS;
            this.TotalDPS = totalDPS;

            // HPS 相关
            this.DamageTaken = takenDamage;
            this.TotalHealingDone = totalHealing;
            this.CriticalHealingDone = totalCriticalHealing;
            this.LuckyHealingDone = totalLuckyHealing;
            this.CritLuckyHealingDone = totalCritLuckyHealing;
            this.InstantHps = totalInstantHps;
            this.MaxInstantHps = totalMaxInstantHps;
            this.TotalHps = totalHps;

            // UI 控件
            this.CellProgress = cellProgress;
        }


        public CellProgress CellProgress
        {
            get { return progress; }
            set
            {
                if (progress == value) return;
                progress = value;
                OnPropertyChanged(nameof(CellProgress));
            }
        }


        // 属性封装（包含通知）

        /// <summary>角色 ID</summary>
        public ulong uid
        {
            get => Uid;
            set
            {
                if (Uid == value) return;
                Uid = value;
                OnPropertyChanged(nameof(uid));
            }
        }

        public string nickname
        {
            get => NickName;
            set
            {
                if (NickName == value) return;
                NickName = value;
                OnPropertyChanged(nameof(nickname));
            }
        }

        /// <summary>
        /// 职业
        /// </summary>
        public string profession
        {
            get => Profession;
            set
            {
                if (Profession == value) return;
                Profession = value;
                OnPropertyChanged(nameof(Profession));
            }
        }

        /// <summary>总伤害</summary>
        public ulong totalDamage
        {
            get => TotalDamage;
            set
            {
                if (TotalDamage == value) return;
                TotalDamage = value;
                OnPropertyChanged(nameof(totalDamage));
            }
        }

        /// <summary>纯暴击伤害</summary>
        public ulong criticalDamage
        {
            get => CriticalDamage;
            set
            {
                if (CriticalDamage == value) return;
                CriticalDamage = value;
                OnPropertyChanged(nameof(criticalDamage));
            }
        }

        /// <summary>纯幸运伤害</summary>
        public ulong luckyDamage
        {
            get => LuckyDamage;
            set
            {
                if (LuckyDamage == value) return;
                LuckyDamage = value;
                OnPropertyChanged(nameof(luckyDamage));
            }
        }

        /// <summary>暴击+幸运伤害</summary>
        public ulong critLuckyDamage
        {
            get => CritLuckyDamage;
            set
            {
                if (CritLuckyDamage == value) return;
                CritLuckyDamage = value;
                OnPropertyChanged(nameof(critLuckyDamage));
            }
        }

        /// <summary>暴击率（百分比）</summary>
        public string critRate
        {
            get => CritRate;
            set
            {
                if (CritRate == value) return;
                CritRate = value;
                OnPropertyChanged(nameof(critRate));
            }
        }

        /// <summary>幸运率（百分比）</summary>
        public string luckyRate
        {
            get => LuckyRate;
            set
            {
                if (LuckyRate == value) return;
                LuckyRate = value;
                OnPropertyChanged(nameof(luckyRate));
            }
        }

        /// <summary>瞬时 DPS（最近1秒）</summary>
        public ulong instantDps
        {
            get => InstantDPS;
            set
            {
                if (InstantDPS == value) return;
                InstantDPS = value;
                OnPropertyChanged(nameof(instantDps));
            }
        }

        /// <summary>最大瞬时 DPS</summary>
        public ulong maxInstantDps
        {
            get => MaxInstantDPS;
            set
            {
                if (MaxInstantDPS == value) return;
                MaxInstantDPS = value;
                OnPropertyChanged(nameof(maxInstantDps));
            }
        }

        /// <summary>平均总 DPS</summary>
        public double totalDps
        {
            get => TotalDPS;
            set
            {
                if (TotalDPS == value) return;
                TotalDPS = value;
                OnPropertyChanged(nameof(totalDps));
            }
        }

        // —— 公开属性（包含通知） —— 

        /// <summary>累计受到的伤害（该玩家受到的总伤害）</summary>
        public ulong damageTaken
        {
            get => DamageTaken;
            set
            {
                if (DamageTaken == value) return;
                DamageTaken = value;
                OnPropertyChanged(nameof(damageTaken));
            }
        }

        /// <summary>总治疗量（该玩家提供的治疗总量）</summary>
        public ulong totalHealingDone
        {
            get => TotalHealingDone;
            set
            {
                if (TotalHealingDone == value) return;
                TotalHealingDone = value;
                OnPropertyChanged(nameof(totalHealingDone));
            }
        }

        /// <summary>暴击治疗量（该玩家通过暴击造成的治疗）</summary>
        public ulong criticalHealingDone
        {
            get => CriticalHealingDone;
            set
            {
                if (CriticalHealingDone == value) return;
                CriticalHealingDone = value;
                OnPropertyChanged(nameof(criticalHealingDone));
            }
        }

        /// <summary>幸运治疗量（该玩家通过幸运造成的治疗）</summary>
        public ulong luckyHealingDone
        {
            get => LuckyHealingDone;
            set
            {
                if (LuckyHealingDone == value) return;
                LuckyHealingDone = value;
                OnPropertyChanged(nameof(luckyHealingDone));
            }
        }

        /// <summary>暴击+幸运治疗量（同时满足暴击和幸运条件的治疗）</summary>
        public ulong critLuckyHealingDone
        {
            get => CritLuckyHealingDone;
            set
            {
                if (CritLuckyHealingDone == value) return;
                CritLuckyHealingDone = value;
                OnPropertyChanged(nameof(critLuckyHealingDone));
            }
        }

        /// <summary>瞬时 HPS（最近 1 秒内的治疗/秒）</summary>
        public ulong instantHps
        {
            get => InstantHps;
            set
            {
                if (InstantHps == value) return;
                InstantHps = value;
                OnPropertyChanged(nameof(instantHps));
            }
        }

        /// <summary>最大瞬时 HPS（统计期间出现过的最大治疗/秒）</summary>
        public ulong maxInstantHps
        {
            get => MaxInstantHps;
            set
            {
                if (MaxInstantHps == value) return;
                MaxInstantHps = value;
                OnPropertyChanged(nameof(maxInstantHps));
            }
        }

        /// <summary>平均总 HPS（总治疗 ÷ 战斗持续秒数）</summary>
        public double totalHps
        {
            get => TotalHps;
            set
            {
                if (TotalHps == value) return;
                TotalHps = value;
                OnPropertyChanged(nameof(totalHps));
            }
        }
    }

}
