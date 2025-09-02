using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Data.Models
{
    /// <summary>
    /// 战斗日志
    /// </summary>
    /// <remarks>
    /// 现阶段的字段顺序是经过设计的, 请勿随意更改
    /// 本段中, 后续的多个 bool 类型, 可以考虑合并为一个 Flag 以节省空间并提高效率, 但会降低可读性
    /// </remarks>
    public class BattleLog
    {
        /// <summary>
        /// 包ID
        /// </summary>
        public long PacketID { get;  set; }
        /// <summary>
        /// 时间戳 (Ticks)
        /// </summary>
        public long TimeTicks { get;  set; }
        /// <summary>
        /// 技能ID
        /// </summary>
        public long SkillID { get;  set; }
        /// <summary>
        /// 释放对象UUID (发出者)
        /// </summary>
        public long AttackerUuid { get;  set; }
        /// <summary>
        /// 目标对象UUID (目标者)
        /// </summary>
        public long TargetUuid { get;  set; }
        /// <summary>
        /// 具体数值 (伤害)
        /// </summary>
        public long Value { get;  set; }
        /// <summary>
        /// 数值元素类型
        /// </summary>
        public int ValueElementType { get;  set; }
        /// <summary>
        /// 伤害来源类型
        /// </summary>
        public int DamageSourceType { get;  set; }

        /// <summary>
        /// 释放对象 (发出者) 是否为玩家
        /// </summary>
        public bool IsAttackerPlayer { get;  set; }
        /// <summary>
        /// 目标对象 (目标者) 是否为玩家
        /// </summary>
        public bool IsTargetPlayer { get;  set; }
        /// <summary>
        /// 具体数值是否为幸运一击
        /// </summary>
        public bool IsLucky { get;  set; }
        /// <summary>
        /// 具体数值是否为暴击
        /// </summary>
        public bool IsCritical { get;  set; }
        /// <summary>
        /// 具体数值是否为治疗
        /// </summary>
        public bool IsHeal { get;  set; }
        /// <summary>
        /// 具体数值是否为闪避
        /// </summary>
        public bool IsMiss { get;  set; }
        /// <summary>
        /// 目标对象 (目标者) 是否阵亡
        /// </summary>
        public bool IsDead { get;  set; }
    }
}
