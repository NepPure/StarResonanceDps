using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 星痕共鸣DPS统计
{
    /// <summary>
    /// 表示一个玩家的战斗统计信息
    /// </summary>
    public class PlayerStat
    {
        /// <summary>
        /// 玩家 UID（唯一标识）
        /// </summary>
        public ulong Uid { get; set; }

        /// <summary>
        /// 职业
        /// </summary>
        public string Profession { get; set; }

        /// <summary>
        /// 总伤害值（所有类型伤害的总和）
        /// </summary>
        public ulong TotalDamage { get; set; }

        /// <summary>
        /// 普通伤害（非暴击、非幸运）
        /// </summary>
        public ulong NormalDamage { get; set; }

        /// <summary>
        /// 纯暴击伤害（非幸运）
        /// </summary>
        public ulong CriticalDamage { get; set; }

        /// <summary>
        /// 纯幸运伤害（非暴击）
        /// </summary>
        public ulong LuckyDamage { get; set; }

        /// <summary>
        /// 同时为暴击 + 幸运的伤害值
        /// </summary>
        public ulong CritLuckyDamage { get; set; }

        /// <summary>
        /// 命中次数：普通命中
        /// </summary>
        public int NormalCount { get; set; }

        /// <summary>
        /// 命中次数：暴击
        /// </summary>
        public int CriticalCount { get; set; }

        /// <summary>
        /// 命中次数：幸运
        /// </summary>
        public int LuckyCount { get; set; }

        /// <summary>
        /// 总命中次数（普通 + 暴击 + 幸运）
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总减少的HP量（扣血）
        /// </summary>
        public ulong HpLessenTotal { get; set; }

        /// <summary>
        /// 首次造成伤害的时间（用于计算总DPS）
        /// </summary>
        public DateTime FirstHitTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 最后一次造成伤害的时间
        /// </summary>
        public DateTime LastHitTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 最近的命中记录（用于计算瞬时DPS）
        /// </summary>
        public List<(DateTime Time, ulong Damage)> RecentHits { get; set; } = new();

        /// <summary>
        /// 暴击率（百分比：暴击次数 / 总次数）
        /// </summary>
        public double CritRate => TotalCount > 0 ? (double)CriticalCount / TotalCount * 100 : 0;

        /// <summary>
        /// 幸运率（百分比：幸运次数 / 总次数）
        /// </summary>
        public double LuckyRate => TotalCount > 0 ? (double)LuckyCount / TotalCount * 100 : 0;

        /// <summary>
        /// 1秒内造成的伤害总和，用于展示“瞬时DPS”
        /// </summary>
        public ulong InstantDPS
        {
            get
            {
                var now = DateTime.Now;
                // 移除 1 秒前的记录
                RecentHits.RemoveAll(hit => (now - hit.Time).TotalSeconds > 1);
                return (ulong)RecentHits.Sum(hit => (long)hit.Damage);
            }
        }

        /// <summary>
        /// 总DPS（总伤害除以战斗时长秒数）
        /// </summary>
        public double TotalDPS
        {
            get
            {
                if (FirstHitTime == DateTime.MinValue || LastHitTime == DateTime.MinValue || FirstHitTime == LastHitTime)
                    return 0;

                var duration = (LastHitTime - FirstHitTime).TotalSeconds;
                return duration > 0 ? TotalDamage / duration : 0;
            }
        }

        /// <summary>
        /// 瞬时DPS的最高值（用于追踪爆发伤害）
        /// </summary>
        public ulong MaxInstantDPS { get; set; } = 0;

        /// <summary>
        /// 记录一次命中（伤害统计入口）
        /// </summary>
        /// <param name="damage">造成的伤害值</param>
        /// <param name="isCrit">是否为暴击</param>
        /// <param name="isLucky">是否为幸运</param>
        /// <param name="hpLessen">是否减少了目标HP（可能为0）</param>
        public void RecordHit(ulong damage, bool isCrit, bool isLucky, ulong hpLessen)
        {
            TotalDamage += damage;
            HpLessenTotal += hpLessen;
            TotalCount++;

            if (isCrit && isLucky)
            {
                CritLuckyDamage += damage;
                CriticalCount++;
                LuckyCount++;
            }
            else if (isCrit)
            {
                CriticalDamage += damage;
                CriticalCount++;
            }
            else if (isLucky)
            {
                LuckyDamage += damage;
                LuckyCount++;
            }
            else
            {
                NormalDamage += damage;
                NormalCount++;
            }

            var now = DateTime.Now;

            RecentHits.Add((now, damage));
            MaxInstantDPS = Math.Max(MaxInstantDPS, InstantDPS);

            if (FirstHitTime == DateTime.MinValue)
                FirstHitTime = now;

            LastHitTime = now;
        }
    }


}
