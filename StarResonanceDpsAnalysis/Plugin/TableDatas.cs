using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntdUI;

namespace StarResonanceDpsAnalysis.Plugin
{

    public class TableDatas
    {
        /// <summary>
        /// 表格数据绑定
        /// </summary>
        public static BindingList<DpsTable> DpsTable = [];
        public static readonly object DpsTableLock = new();

    }


    public class DpsTable : NotifyProperty
    {

        // —— DPS 相关私有字段 —— 

        /// <summary>玩家的唯一标识 UID</summary>
        private ulong Uid;
        private string NickName;

        /// <summary>玩家的职业/角色名称</summary>
        private string Profession;

        /// <summary>该玩家造成的总伤害</summary>
        private string TotalDamage;

        /// <summary>该玩家通过暴击造成的伤害总量</summary>
        private string CriticalDamage;

        /// <summary>该玩家通过幸运造成的伤害总量</summary>
        private string LuckyDamage;

        /// <summary>同时满足暴击和幸运条件的伤害总量</summary>
        private string CritLuckyDamage;

        /// <summary>格式化后的暴击率（字符串，带“%”）</summary>
        private string CritRate;

        /// <summary>格式化后的幸运率（字符串，带“%”）</summary>
        private string LuckyRate;

        /// <summary>最近 1 秒内的瞬时 DPS（伤害/秒）</summary>
        private string InstantDPS;

        /// <summary>统计期间出现过的最大瞬时 DPS</summary>
        private string MaxInstantDPS;

        /// <summary>平均总 DPS（总伤害 ÷ 战斗持续秒数）</summary>
        private string TotalDPS;

        /// <summary>用于在 UI 中展示进度条的 CellProgress 对象</summary>
        private CellProgress progress;


        // —— HPS 相关私有字段 —— 

        /// <summary>累计受到的伤害（该玩家受到的总伤害）</summary>
        private string DamageTaken;

        /// <summary>该玩家提供的总治疗量</summary>
        private string TotalHealingDone;

        /// <summary>该玩家通过暴击造成的治疗总量</summary>
        private string CriticalHealingDone;

        /// <summary>该玩家通过幸运造成的治疗总量</summary>
        private string LuckyHealingDone;

        /// <summary>同时满足暴击和幸运条件的治疗总量</summary>
        private string CritLuckyHealingDone;

        /// <summary>最近 1 秒内的瞬时 HPS（治疗/秒）</summary>
        private string InstantHps;

        /// <summary>统计期间出现过的最大瞬时 HPS</summary>
        private string MaxInstantHps;

        /// <summary>平均总 HPS（总治疗 ÷ 战斗持续秒数）</summary>
        private string TotalHps;



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
        public DpsTable(
            ulong uid,
            string nickname,
            string takenDamage,
            string totalHealing,
            string totalCriticalHealing,
            string totalLuckyHealing,
            string totalCritLuckyHealing,
            string totalInstantHps,
            string totalMaxInstantHps,
            string profession,
            string totalDamage,
            string criticalDamage,
            string luckyDamage,
            string critLuckyDamage,
            string critRate,
            string luckyRate,
            string instantDPS,
            string maxInstantDPS,
            string totalDPS,
            string totalHps,
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
                OnPropertyChanged(nameof(Uid));
            }
        }

        public string nickname
        {
            get => NickName;
            set
            {
                if (NickName == value) return;
                NickName = value;
                OnPropertyChanged(nameof(NickName));
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
        public string totalDamage
        {
            get => TotalDamage;
            set
            {
                if (TotalDamage == value) return;
                TotalDamage = value;
                OnPropertyChanged(nameof(TotalDamage));
            }
        }

        /// <summary>纯暴击伤害</summary>
        public string criticalDamage
        {
            get => CriticalDamage;
            set
            {
                if (CriticalDamage == value) return;
                CriticalDamage = value;
                OnPropertyChanged(nameof(CriticalDamage));
            }
        }

        /// <summary>纯幸运伤害</summary>
        public string luckyDamage
        {
            get => LuckyDamage;
            set
            {
                if (LuckyDamage == value) return;
                LuckyDamage = value;
                OnPropertyChanged(nameof(LuckyDamage));
            }
        }

        /// <summary>暴击+幸运伤害</summary>
        public string critLuckyDamage
        {
            get => CritLuckyDamage;
            set
            {
                if (CritLuckyDamage == value) return;
                CritLuckyDamage = value;
                OnPropertyChanged(nameof(CritLuckyDamage));
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
                OnPropertyChanged(nameof(CritRate));
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
                OnPropertyChanged(nameof(LuckyRate));
            }
        }

        /// <summary>瞬时 DPS（最近1秒）</summary>
        public string instantDps
        {
            get => InstantDPS;
            set
            {
                if (InstantDPS == value) return;
                InstantDPS = value;
                OnPropertyChanged(nameof(InstantDPS));
            }
        }

        /// <summary>最大瞬时 DPS</summary>
        public string maxInstantDps
        {
            get => MaxInstantDPS;
            set
            {
                if (MaxInstantDPS == value) return;
                MaxInstantDPS = value;
                OnPropertyChanged(nameof(MaxInstantDPS));
            }
        }

        /// <summary>平均总 DPS</summary>
        public string totalDps
        {
            get => TotalDPS;
            set
            {
                if (TotalDPS == value) return;
                TotalDPS = value;
                OnPropertyChanged(nameof(TotalDPS));
            }
        }

        // —— 公开属性（包含通知） —— 

        /// <summary>累计受到的伤害（该玩家受到的总伤害）</summary>
        public string damageTaken
        {
            get => DamageTaken;
            set
            {
                if (DamageTaken == value) return;
                DamageTaken = value;
                OnPropertyChanged(nameof(DamageTaken));
            }
        }

        /// <summary>总治疗量（该玩家提供的治疗总量）</summary>
        public string totalHealingDone
        {
            get => TotalHealingDone;
            set
            {
                if (TotalHealingDone == value) return;
                TotalHealingDone = value;
                OnPropertyChanged(nameof(TotalHealingDone));
            }
        }

        /// <summary>暴击治疗量（该玩家通过暴击造成的治疗）</summary>
        public string criticalHealingDone
        {
            get => CriticalHealingDone;
            set
            {
                if (CriticalHealingDone == value) return;
                CriticalHealingDone = value;
                OnPropertyChanged(nameof(CriticalHealingDone));
            }
        }

        /// <summary>幸运治疗量（该玩家通过幸运造成的治疗）</summary>
        public string luckyHealingDone
        {
            get => LuckyHealingDone;
            set
            {
                if (LuckyHealingDone == value) return;
                LuckyHealingDone = value;
                OnPropertyChanged(nameof(LuckyHealingDone));
            }
        }

        /// <summary>暴击+幸运治疗量（同时满足暴击和幸运条件的治疗）</summary>
        public string critLuckyHealingDone
        {
            get => CritLuckyHealingDone;
            set
            {
                if (CritLuckyHealingDone == value) return;
                CritLuckyHealingDone = value;
                OnPropertyChanged(nameof(CritLuckyHealingDone));
            }
        }

        /// <summary>瞬时 HPS（最近 1 秒内的治疗/秒）</summary>
        public string instantHps
        {
            get => InstantHps;
            set
            {
                if (InstantHps == value) return;
                InstantHps = value;
                OnPropertyChanged(nameof(InstantHps));
            }
        }

        /// <summary>最大瞬时 HPS（统计期间出现过的最大治疗/秒）</summary>
        public string maxInstantHps
        {
            get => MaxInstantHps;
            set
            {
                if (MaxInstantHps == value) return;
                MaxInstantHps = value;
                OnPropertyChanged(nameof(MaxInstantHps));
            }
        }

        /// <summary>平均总 HPS（总治疗 ÷ 战斗持续秒数）</summary>
        public string totalHps
        {
            get => TotalHps;
            set
            {
                if (TotalHps == value) return;
                TotalHps = value;
                OnPropertyChanged(nameof(TotalHps));
            }
        }
    }

}
