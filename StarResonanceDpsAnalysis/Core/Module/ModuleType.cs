using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Module
{
    public enum ModuleType : int
    {
        BASIC_ATTACK = 5500101,
        HIGH_PERFORMANCE_ATTACK = 5500102,
        OUTSTANDING_ATTACK = 5500103,
        BASIC_HEALING = 5500201,
        HIGH_PERFORMANCE_HEALING = 5500202,
        OUTSTANDING_HEALING = 5500203,
        BASIC_PROTECTION = 5500301,
        HIGH_PERFORMANCE_PROTECTION = 5500302,
        OUTSTANDING_PROTECTION = 5500303,
    }

    public enum ModuleAttrType : int
    {
        STRENGTH_BOOST = 1110,
        AGILITY_BOOST = 1111,
        INTELLIGENCE_BOOST = 1112,
        SPECIAL_ATTACK_DAMAGE = 1113,
        ELITE_STRIKE = 1114,
        SPECIAL_HEALING_BOOST = 1205,
        EXPERT_HEALING_BOOST = 1206,
        CASTING_FOCUS = 1407,
        ATTACK_SPEED_FOCUS = 1408,
        CRITICAL_FOCUS = 1409,
        LUCK_FOCUS = 1410,
        MAGIC_RESISTANCE = 1307,
        PHYSICAL_RESISTANCE = 1308,
    }

    public enum ModuleCategory
    {
        ATTACK,
        GUARDIAN,
        SUPPORT
    }
    /// <summary>模组信息</summary>
    public class ModuleInfo
    {
        public string Name { get; set; }
        public int ConfigId { get; set; }
        public long Uuid { get; set; }
        public int Quality { get; set; }
        public List<ModulePart> Parts { get; set; } = new();
    }

    public class ModulePart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public static class ModuleMaps
    {
        // 模组名称映射（按 Python 的 .value：用 int 作键）
        public static readonly Dictionary<int, string> MODULE_NAMES = new()
    {
        { (int)ModuleType.BASIC_ATTACK, "基础攻击" },
        { (int)ModuleType.HIGH_PERFORMANCE_ATTACK, "高性能攻击" },
        { (int)ModuleType.OUTSTANDING_ATTACK, "卓越攻击" },
        { (int)ModuleType.BASIC_HEALING, "基础治疗" },
        { (int)ModuleType.HIGH_PERFORMANCE_HEALING, "高性能治疗" },
        { (int)ModuleType.OUTSTANDING_HEALING, "卓越治疗" },
        { (int)ModuleType.BASIC_PROTECTION, "基础防护" },
        { (int)ModuleType.HIGH_PERFORMANCE_PROTECTION, "高性能守护" },
        { (int)ModuleType.OUTSTANDING_PROTECTION, "卓越防护" },
    };

        // 模组属性名称映射
        public static readonly Dictionary<int, string> MODULE_ATTR_NAMES = new()
    {
        { (int)ModuleAttrType.STRENGTH_BOOST, "力量加持" },
        { (int)ModuleAttrType.AGILITY_BOOST, "敏捷加持" },
        { (int)ModuleAttrType.INTELLIGENCE_BOOST, "智力加持" },
        { (int)ModuleAttrType.SPECIAL_ATTACK_DAMAGE, "特攻伤害" },
        { (int)ModuleAttrType.ELITE_STRIKE, "精英打击" },
        { (int)ModuleAttrType.SPECIAL_HEALING_BOOST, "特攻治疗加持" },
        { (int)ModuleAttrType.EXPERT_HEALING_BOOST, "专精治疗加持" },
        { (int)ModuleAttrType.CASTING_FOCUS, "施法专注" },
        { (int)ModuleAttrType.ATTACK_SPEED_FOCUS, "攻速专注" },
        { (int)ModuleAttrType.CRITICAL_FOCUS, "暴击专注" },
        { (int)ModuleAttrType.LUCK_FOCUS, "幸运专注" },
        { (int)ModuleAttrType.MAGIC_RESISTANCE, "抵御魔法" },
        { (int)ModuleAttrType.PHYSICAL_RESISTANCE, "抵御物理" },
    };

        // 模组类型到分类的映射（键仍为 int，值为分类枚举）
        public static readonly Dictionary<int, ModuleCategory> MODULE_CATEGORY_MAP = new()
    {
        { (int)ModuleType.BASIC_ATTACK, ModuleCategory.ATTACK },
        { (int)ModuleType.HIGH_PERFORMANCE_ATTACK, ModuleCategory.ATTACK },
        { (int)ModuleType.OUTSTANDING_ATTACK, ModuleCategory.ATTACK },
        { (int)ModuleType.BASIC_PROTECTION, ModuleCategory.GUARDIAN },
        { (int)ModuleType.HIGH_PERFORMANCE_PROTECTION, ModuleCategory.GUARDIAN },
        { (int)ModuleType.OUTSTANDING_PROTECTION, ModuleCategory.GUARDIAN },
        { (int)ModuleType.BASIC_HEALING, ModuleCategory.SUPPORT },
        { (int)ModuleType.HIGH_PERFORMANCE_HEALING, ModuleCategory.SUPPORT },
        { (int)ModuleType.OUTSTANDING_HEALING, ModuleCategory.SUPPORT },
    };

        // 分类中文名（C# 枚举不能直接持有字符串值）
        public static readonly Dictionary<ModuleCategory, string> MODULE_CATEGORY_NAMES = new()
        {
            { ModuleCategory.ATTACK, "攻击" },
            { ModuleCategory.GUARDIAN, "守护" },
            { ModuleCategory.SUPPORT, "辅助" },
        };

        // 属性阈值和效果等级
        public static readonly int[] ATTR_THRESHOLDS = new[] { 1, 4, 8, 12, 16, 20 };
    }
}
