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

        private ulong Uid;
        private string Profession;
        private ulong TotalDamage;
        private ulong CriticalDamage;
        private ulong LuckyDamage;
        private ulong CritLuckyDamage;
        private string CritRate;
        private string LuckyRate;
        private ulong InstantDPS;
        private ulong MaxInstantDPS;
        private double TotalDPS;
        private CellProgress progress;


        /// <summary>
        /// 构造一个用于展示伤害统计的 DpsTabel 实例（通常用于表格绑定）
        /// </summary>
        /// <param name="uid">角色 UID，唯一标识一个玩家</param>
        /// <param name="totalDamage">总伤害值</param>
        /// <param name="criticalDamage">通过暴击造成的伤害总量</param>
        /// <param name="luckyDamage">通过幸运造成的伤害总量</param>
        /// <param name="critLuckyDamage">同时满足暴击和幸运的伤害总量</param>
        /// <param name="critRate">暴击率，0-100%之间的小数</param>
        /// <param name="luckyRate">幸运率，0-100%之间的小数</param>
        /// <param name="instantDPS">最近 1 秒内的瞬时 DPS（伤害/秒）</param>
        /// <param name="maxInstantDPS">统计期间出现过的最大瞬时 DPS</param>
        /// <param name="totalDPS">总 DPS，即总伤害 / 战斗持续秒数</param>
        public DpsTabel(ulong uid, string profession, ulong totalDamage, ulong criticalDamage, ulong luckyDamage,
                            ulong critLuckyDamage, double critRate, double luckyRate,
                            ulong instantDPS, ulong maxInstantDPS, double totalDPS, CellProgress cellProgress = null)
        {

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
            CellProgress = cellProgress;
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
    }

}
