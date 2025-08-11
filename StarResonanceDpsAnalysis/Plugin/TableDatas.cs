using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using AntdUI;

namespace StarResonanceDpsAnalysis.Plugin
{
    #region DpsTableDatas 类
    public class DpsTableDatas
    {
        /// <summary>
        /// 表格数据绑定
        /// </summary>
        public static BindingList<DpsTable> DpsTable = [];
        public static readonly object DpsTableLock = new();

    }
    #endregion
    #region DpsTable 类
    /// <summary>
    /// DPS 表格数据模型
    /// 用于绑定 UI 表格显示单个玩家的战斗统计信息（伤害、治疗、承伤等）
    /// 继承 NotifyProperty 以支持属性更改通知（UI 自动刷新）
    /// </summary>
    public class DpsTable : NotifyProperty
    {
        // —— DPS 相关私有字段（只在类内部使用） ——
        private ulong uid;                // 玩家唯一ID
        private string nickName;           // 玩家昵称
        private string profession;         // 职业
        private int combatPower;           // 战力
        private string totalDamage;        // 总伤害
        private string criticalDamage;     // 暴击伤害
        private string luckyDamage;        // 幸运伤害
        private string critLuckyDamage;    // 暴击且幸运的伤害
        private string critRate;           // 暴击率（百分比字符串）
        private string luckyRate;          // 幸运率（百分比字符串）
        private string instantDps;         // 实时 DPS
        private string maxInstantDps;      // 最大瞬时 DPS
        private string totalDps;           // 平均 DPS
        private CellProgress cellProgress; // 用于 UI 显示的伤害占比进度条

        // —— HPS 相关私有字段（治疗类数据） ——
        private string damageTaken;        // 承受伤害总量
        private string totalHealingDone;   // 总治疗量
        private string criticalHealingDone;// 暴击治疗量
        private string luckyHealingDone;   // 幸运治疗量
        private string critLuckyHealingDone;// 暴击且幸运的治疗量
        private string instantHps;         // 实时 HPS
        private string maxInstantHps;      // 最大瞬时 HPS
        private string totalHps;           // 平均 HPS

        /// <summary>
        /// 构造函数
        /// 初始化所有统计字段的值（UI 初次绑定时使用）
        /// </summary>
        public DpsTable(
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
            ulong instantDps,
            ulong maxInstantDps,
            double totalDps,
            double totalHps,
            CellProgress cellProgress = null,
            int combatPower = 0)
        {
            // —— 基础信息 ——
            Uid = uid;
            NickName = nickname;
            CombatPower = combatPower;
            Profession = profession;

            // —— 伤害相关 ——
            TotalDamage = totalDamage.ToString();
            CriticalDamage = criticalDamage.ToString();
            LuckyDamage = luckyDamage.ToString();
            CritLuckyDamage = critLuckyDamage.ToString();
            CritRate = $"{critRate}%";
            LuckyRate = $"{luckyRate}%";
            InstantDps = instantDps.ToString();
            MaxInstantDps = maxInstantDps.ToString();
            TotalDps = totalDps.ToString();

            // —— 承伤/治疗相关 ——
            DamageTaken = takenDamage.ToString();
            TotalHealingDone = totalHealing.ToString();
            CriticalHealingDone = totalCriticalHealing.ToString();
            LuckyHealingDone = totalLuckyHealing.ToString();
            CritLuckyHealingDone = totalCritLuckyHealing.ToString();
            InstantHps = totalInstantHps.ToString();
            MaxInstantHps = totalMaxInstantHps.ToString();
            TotalHps = totalHps.ToString();

            // —— UI 占比进度条 ——
            CellProgress = cellProgress;
        }

        // —— 属性封装（支持 UI 绑定通知） ——

        /// <summary>玩家唯一ID</summary>
        public ulong Uid
        {
            get => uid;
            set
            {
              
                if (uid == value) return;
                uid = value;
                OnPropertyChanged(nameof(Uid));
            }
        }

        /// <summary>玩家昵称</summary>
        public string NickName
        {
            get => nickName;
            set
            {
               
                if (nickName == value) return;
                nickName = value;
                OnPropertyChanged(nameof(NickName));
            }
        }



        /// <summary>战力</summary>
        public int CombatPower
        {
            get => combatPower;
            set
            {
                if (combatPower == value) return;
                combatPower = value;
                OnPropertyChanged(nameof(CombatPower));
            }
        }

        /// <summary>职业</summary>
        public string Profession
        {
            get => profession;
            set
            {
                if (profession == value) return;
                profession = value;
                OnPropertyChanged(nameof(Profession));
            }
        }


        /// <summary>
        /// 暴击率
        /// </summary>
        public string CritRate
        {
            get => critRate;
            set
            {
                if (critRate == value) return;
                critRate = value;
                OnPropertyChanged(nameof(CritRate));
            }
        }

        public string LuckyRate
        {
            get => luckyRate;
            set
            {
                if (luckyRate == value) return;
                luckyRate = value;
                OnPropertyChanged(nameof(LuckyRate));
            }
        }

        /// <summary>总伤害</summary>
        public string TotalDamage
        {
            get => totalDamage;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (totalDamage == formatted) return;
                totalDamage = formatted;
                OnPropertyChanged(nameof(TotalDamage));
            }
        }

        /// <summary>暴击伤害</summary>
        public string CriticalDamage
        {
            get => criticalDamage;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (criticalDamage == formatted) return;
                criticalDamage = formatted;
                OnPropertyChanged(nameof(CriticalDamage));
            }
        }

        /// <summary>幸运伤害</summary>
        public string LuckyDamage
        {
            get => luckyDamage;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (luckyDamage == formatted) return;
                luckyDamage = formatted;
                OnPropertyChanged(nameof(LuckyDamage));
            }
        }

        /// <summary>暴击且幸运伤害</summary>
        public string CritLuckyDamage
        {
            get => critLuckyDamage;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (critLuckyDamage == formatted) return;
                critLuckyDamage = formatted;
                OnPropertyChanged(nameof(CritLuckyDamage));
            }
        }

        /// <summary>实时 DPS</summary>
        public string InstantDps
        {
            get => instantDps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (instantDps == formatted) return;
                instantDps = formatted;
                OnPropertyChanged(nameof(InstantDps));
            }
        }

        /// <summary>最大瞬时 DPS</summary>
        public string MaxInstantDps
        {
            get => maxInstantDps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (maxInstantDps == formatted) return;
                maxInstantDps = formatted;
                OnPropertyChanged(nameof(MaxInstantDps));
            }
        }

        /// <summary>平均 DPS</summary>
        public string TotalDps
        {
            get => totalDps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
 
                if (totalDps == formatted) return;
                totalDps = formatted;
                OnPropertyChanged(nameof(TotalDps));
            }
        }

        /// <summary>承受伤害</summary>
        public string DamageTaken
        {
            get => damageTaken;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (damageTaken == formatted) return;
                damageTaken = formatted;
                OnPropertyChanged(nameof(DamageTaken));
            }
        }

        /// <summary>总治疗量</summary>
        public string TotalHealingDone
        {
            get => totalHealingDone;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (totalHealingDone == formatted) return;
                totalHealingDone = formatted;
                OnPropertyChanged(nameof(TotalHealingDone));
            }
        }

        /// <summary>暴击治疗量</summary>
        public string CriticalHealingDone
        {
            get => criticalHealingDone;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (criticalHealingDone == formatted) return;
                criticalHealingDone = formatted;
                OnPropertyChanged(nameof(CriticalHealingDone));
            }
        }

        /// <summary>幸运治疗量</summary>
        public string LuckyHealingDone
        {
            get => luckyHealingDone;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (luckyHealingDone == formatted) return;
                luckyHealingDone = formatted;
                OnPropertyChanged(nameof(LuckyHealingDone));
            }
        }

        /// <summary>暴击且幸运的治疗量</summary>
        public string CritLuckyHealingDone
        {
            get => critLuckyHealingDone;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (critLuckyHealingDone == formatted) return;
                critLuckyHealingDone = formatted;
                OnPropertyChanged(nameof(CritLuckyHealingDone));
            }
        }

        /// <summary>实时 HPS</summary>
        public string InstantHps
        {
            get => instantHps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (instantHps == formatted) return;
                instantHps = formatted;
                OnPropertyChanged(nameof(InstantHps));
            }
        }

        /// <summary>最大瞬时 HPS</summary>
        public string MaxInstantHps
        {
            get => maxInstantHps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (maxInstantHps == formatted) return;
                maxInstantHps = formatted;
                OnPropertyChanged(nameof(MaxInstantHps));
            }
        }

        /// <summary>平均 HPS</summary>
        public string TotalHps
        {
            get => totalHps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value));
                string formatted = Common.FormatWithEnglishUnits(val);
                if (totalHps == formatted) return;
                totalHps = formatted;
                OnPropertyChanged(nameof(TotalHps));
            }
        }


        /// <summary>伤害占比进度条</summary>
        public CellProgress CellProgress
        {
            get => cellProgress;
            set
            {
                if (cellProgress == value) return;
                cellProgress = value;
                OnPropertyChanged(nameof(CellProgress));
            }
        }
    }


    #endregion

    #region 技能数据

    public class SkillTableDatas
    {

        public static BindingList<SkillData> SkillTable = new();
        public static readonly object SkillTableLock = new();
    }
    public class SkillData : NotifyProperty
    {
        #region 字段（私有存储）
        private string name;       // 技能名称
        private string icon;       // 技能图标（文件路径或URL）
        private CellText damage;      // 技能总伤害
        private CellText hitCount;      // 技能命中次数
        private CellText critRate;   // 暴击率
        private CellText luckyRate;  // 幸运率
        private CellProgress share; // 占比（0~1）
        private CellText avgPerHit;  // 平均值
        private CellText totalDps;//秒伤
        private CellText percentage; //百分比

        private Font SaoFont = new Font("SAO Welcome TT", 10, FontStyle.Regular);
        #endregion

        #region 构造函数
        public SkillData(string name, string icon, ulong damage, int hitCount, string critRate, string luckyRate, double share, double avgPerHit,double totalDps)
        {
            Name = name;
            Icon = icon;
            Damage = new CellText(damage.ToString()) { Font = SaoFont };
            HitCount = new CellText(hitCount.ToString()) { Font = SaoFont };
            CritRate = new CellText(critRate) { Font = SaoFont };
            LuckyRate = new CellText(luckyRate){ Font = SaoFont };
            Share = new CellProgress((float)share) { Fill = AppConfig.DpsColor ,Size = new Size(200, 10) };
            this.AvgPerHit = new CellText(avgPerHit.ToString()) { Font = SaoFont };
            this.TotalDps =new CellText(totalDps.ToString()) { Font = SaoFont };
            this.Percentage = new CellText(share.ToString()) { Font = SaoFont };

        }
        #endregion

        #region 属性封装（包含通知）
        // —— 技能基础信息 —— 

        /// <summary>
        /// 技能名称（用于UI显示）
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// 技能图标（本地路径或URL）
        /// </summary>
        public string Icon
        {
            get => icon;
            set
            {
                if (icon == value) return;
                icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        // —— 技能统计数据 —— 

        /// <summary>
        /// 技能总伤害（累计值）
        /// </summary>
        public CellText Damage
        {
            get => damage;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value.Text));
                CellText formatted = new CellText(Common.FormatWithEnglishUnits(val)) {Font = SaoFont };

                if (damage == formatted) return;
                damage = formatted;
                OnPropertyChanged(nameof(Damage));
            }
        }

        /// <summary>
        /// 技能命中次数
        /// </summary>
        public CellText HitCount
        {
            get => hitCount;
            set
            {
                if (hitCount == value) return;
                hitCount = value;
                OnPropertyChanged(nameof(HitCount));
            }
        }

        /// <summary>
        /// 暴击率（百分比字符串）
        /// </summary>
        public CellText CritRate
        {
            get => critRate;
            set
            {
                if (critRate == value) return;
                critRate = value;
                OnPropertyChanged(nameof(CritRate));
            }
        }

        /// <summary>
        /// 幸运率（百分比字符串）
        /// </summary>
        public CellText LuckyRate
        {
            get => luckyRate;
            set
            {
                if (luckyRate == value) return;
                luckyRate = value;
                OnPropertyChanged(nameof(LuckyRate));
            }
        }

        /// <summary>
        /// 总伤害占比（0~1 之间的小数）
        /// </summary>
        public CellProgress Share
        {
            get => share;
            set
            {
                if (share == value) return;
                share = value;
                OnPropertyChanged(nameof(Share));
            }
        }

        public CellText AvgPerHit
        {
            get => avgPerHit;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value.Text));
                CellText formatted = new CellText(Common.FormatWithEnglishUnits(val)) { Font = SaoFont };

                if (avgPerHit == formatted) return;
                avgPerHit = formatted;
                OnPropertyChanged(nameof(AvgPerHit));
            }
        }

        public CellText TotalDps
        {
            get => totalDps;
            set
            {
                ulong val = (ulong)Math.Floor(double.Parse(value.Text));
                       CellText formatted = new CellText(Common.FormatWithEnglishUnits(val)) {Font = SaoFont };


                if (totalDps == formatted) return;
                totalDps = formatted;
                OnPropertyChanged(nameof(TotalDps));
            }
        }

        public CellText Percentage
        {
            get => percentage;
            set
            {
                string percentStr = Math.Round(double.Parse(value.Text)*100).ToString();
                CellText formatted = new CellText(@$"{percentStr}%") { Font = SaoFont };
                if (percentage == formatted) return;
                percentage = formatted;
                OnPropertyChanged(nameof(Percentage));
            }
        }
        #endregion
    }

    #endregion


}