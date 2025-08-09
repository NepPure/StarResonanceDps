using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core
{
    public static class SkillConverter
    {
        /// <summary>
        /// 记录了技能 ID 对应的技能名称
        /// </summary>
        public static readonly Dictionary<ulong, string> SkillIdNames = new()
        {
            // 神射
            { 1700826, "狂野呼唤" }, // 第一段(召唤出来后的第一次伤害)
            { 1700827, "狂野呼唤" }, // 消失前的召唤物攻击(召唤后的后续伤害)

            // 神射 -> 鹰弓
            { 2233, "聚能射击" },
            { 2289, "箭雨" },
            { 2295, "锐眼 · 光能巨箭" },
            { 220101, "普通攻击 · 弹无虚发" }, // 第一段(1发), 第二段(1发), 第三段(2发), 一共 4 个包都是 220101
            { 220102, "怒涛射击" },
            { 220103, "普通攻击 · 弹无虚发" }, // 第四段(1发), 独立 ID
            { 220106, "二连矢" },
            
        };

        /// <summary>
        /// 记录了技能 ID 对应的相同技能的主技能 ID
        /// </summary>
        /// <remarks>
        /// 有些技能带有多段伤害, 或者不同的转职, 相同的技能会有不同的 ID, 通过这个字典可以将它们统一到一个主技能 ID 上
        /// </remarks>
        public static readonly Dictionary<ulong, ulong> SameSkillMainIds = new()
        {
            // 神射 -> 狂野呼唤
            { 1700826, 1700827 },

            // 神射 -> 鹰弓 -> 普通攻击 · 弹无虚发
            { 220103, 220101 }
        };
    }
}
