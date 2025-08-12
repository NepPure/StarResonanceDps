namespace StarResonanceDpsAnalysis.Core
{
    // Auto-generated from skill_config.json, with string/int keys
    public enum SkillType
    {
        Damage,
        Heal,
        Unknown
    }

    public enum ElementType
    {
        Dark,
        Earth,
        Fire,
        Ice,
        Light,
        Thunder,
        Wind,
        Unknown
    }

    public sealed class SkillDefinition
    {
        public string Name { get; set; } = "";
        public SkillType Type { get; set; } = SkillType.Unknown;
        public ElementType Element { get; set; } = ElementType.Unknown;
        public string Description { get; set; } = "";
    }

    public sealed class ElementInfo
    {
        public string Color { get; set; } = "#FFFFFF";
        public string Icon { get; set; } = "";
    }

    public static class EmbeddedSkillConfig
    {
        public static readonly string Version = "2.0.0";
        public static readonly string LastUpdated = "2025-01-19";

        public static readonly Dictionary<ElementType, ElementInfo> Elements = new()
        {
            [ElementType.Fire] = new ElementInfo { Color = "#ff6b6b", Icon = "🔥" },
            [ElementType.Ice] = new ElementInfo { Color = "#74c0fc", Icon = "❄️" },
            [ElementType.Thunder] = new ElementInfo { Color = "#ffd43b", Icon = "⚡" },
            [ElementType.Earth] = new ElementInfo { Color = "#8ce99a", Icon = "🌍" },
            [ElementType.Wind] = new ElementInfo { Color = "#91a7ff", Icon = "💨" },
            [ElementType.Light] = new ElementInfo { Color = "#fff3bf", Icon = "✨" },
            [ElementType.Dark] = new ElementInfo { Color = "#9775fa", Icon = "🌙" }
        };

        public static readonly Dictionary<string, SkillDefinition> SkillsByString = new()
        {
            ["1401"] = new SkillDefinition { Name = "风华翔舞", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风华翔舞" },
            ["1402"] = new SkillDefinition { Name = "风华翔舞", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风华翔舞" },
            ["1403"] = new SkillDefinition { Name = "风华翔舞", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风华翔舞" },
            ["1404"] = new SkillDefinition { Name = "风华翔舞", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风华翔舞" },
            ["1409"] = new SkillDefinition { Name = "风神·破阵之风", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风神·破阵之风" },
            ["1420"] = new SkillDefinition { Name = "风姿卓绝", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风姿卓绝" },
            ["2031104"] = new SkillDefinition { Name = "幸运一击(长枪)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(长枪)" },
            ["1418"] = new SkillDefinition { Name = "疾风刺", Type = SkillType.Damage, Element = ElementType.Wind, Description = "疾风刺" },
            ["1421"] = new SkillDefinition { Name = "螺旋击刺", Type = SkillType.Damage, Element = ElementType.Wind, Description = "螺旋击刺" },
            ["1434"] = new SkillDefinition { Name = "神影螺旋", Type = SkillType.Damage, Element = ElementType.Wind, Description = "神影螺旋" },
            ["140301"] = new SkillDefinition { Name = "神影螺旋", Type = SkillType.Damage, Element = ElementType.Wind, Description = "神影螺旋" },
            ["1422"] = new SkillDefinition { Name = "破追", Type = SkillType.Damage, Element = ElementType.Wind, Description = "破追" },
            ["1427"] = new SkillDefinition { Name = "破追", Type = SkillType.Damage, Element = ElementType.Wind, Description = "破追" },
            ["31901"] = new SkillDefinition { Name = "勇气风环", Type = SkillType.Damage, Element = ElementType.Wind, Description = "勇气风环" },
            ["1411"] = new SkillDefinition { Name = "疾驰锋刃", Type = SkillType.Damage, Element = ElementType.Wind, Description = "疾驰锋刃" },
            ["1435"] = new SkillDefinition { Name = "龙击炮", Type = SkillType.Damage, Element = ElementType.Wind, Description = "龙击炮" },
            ["140401"] = new SkillDefinition { Name = "龙击炮", Type = SkillType.Damage, Element = ElementType.Wind, Description = "龙击炮" },
            ["2205071"] = new SkillDefinition { Name = "撕裂", Type = SkillType.Damage, Element = ElementType.Wind, Description = "撕裂" },
            ["149901"] = new SkillDefinition { Name = "风螺旋/螺旋引爆", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风螺旋/螺旋引爆" },
            ["1419"] = new SkillDefinition { Name = "翔返", Type = SkillType.Damage, Element = ElementType.Wind, Description = "翔返" },
            ["1424"] = new SkillDefinition { Name = "刹那", Type = SkillType.Damage, Element = ElementType.Wind, Description = "刹那" },
            ["1425"] = new SkillDefinition { Name = "飞鸟投", Type = SkillType.Damage, Element = ElementType.Wind, Description = "飞鸟投" },
            ["149905"] = new SkillDefinition { Name = "飞鸟投", Type = SkillType.Damage, Element = ElementType.Wind, Description = "飞鸟投" },
            ["1433"] = new SkillDefinition { Name = "极·岚切", Type = SkillType.Damage, Element = ElementType.Wind, Description = "极·岚切" },
            ["149906"] = new SkillDefinition { Name = "极·岚切", Type = SkillType.Damage, Element = ElementType.Wind, Description = "极·岚切" },
            ["149907"] = new SkillDefinition { Name = "锐利冲击(风神)", Type = SkillType.Damage, Element = ElementType.Wind, Description = "锐利冲击(风神)" },
            ["1431"] = new SkillDefinition { Name = "锐利冲击(风神)", Type = SkillType.Damage, Element = ElementType.Wind, Description = "锐利冲击(风神)" },
            ["149902"] = new SkillDefinition { Name = "长矛贯穿", Type = SkillType.Damage, Element = ElementType.Wind, Description = "长矛贯穿" },
            ["140501"] = new SkillDefinition { Name = "龙卷风", Type = SkillType.Damage, Element = ElementType.Wind, Description = "龙卷风" },
            ["1701"] = new SkillDefinition { Name = "我流刀法·诛恶", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流刀法·诛恶" },
            ["1702"] = new SkillDefinition { Name = "我流刀法·诛恶", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流刀法·诛恶" },
            ["1703"] = new SkillDefinition { Name = "我流刀法·诛恶", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流刀法·诛恶" },
            ["1704"] = new SkillDefinition { Name = "我流刀法·诛恶", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流刀法·诛恶" },
            ["1713"] = new SkillDefinition { Name = "极诣·大破灭连斩", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "极诣·大破灭连斩" },
            ["1728"] = new SkillDefinition { Name = "极诣·大破灭连斩(天赋)", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "极诣·大破灭连斩(天赋)" },
            ["1714"] = new SkillDefinition { Name = "居合", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "居合" },
            ["1717"] = new SkillDefinition { Name = "一闪", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "一闪" },
            ["1718"] = new SkillDefinition { Name = "飞雷神", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "飞雷神" },
            ["1735"] = new SkillDefinition { Name = "坠龙闪", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "坠龙闪" },
            ["1736"] = new SkillDefinition { Name = "神影斩", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "神影斩" },
            ["155101"] = new SkillDefinition { Name = "雷切", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "雷切" },
            ["1715"] = new SkillDefinition { Name = "月影", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "月影" },
            ["1719"] = new SkillDefinition { Name = "镰车", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "镰车" },
            ["1724"] = new SkillDefinition { Name = "霹雳连斩", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "霹雳连斩" },
            ["1705"] = new SkillDefinition { Name = "超高出力", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "超高出力" },
            ["1732"] = new SkillDefinition { Name = "千雷闪影之意", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "千雷闪影之意" },
            ["1737"] = new SkillDefinition { Name = "神罚之镰", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "神罚之镰" },
            ["1738"] = new SkillDefinition { Name = "缭乱兜割", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "缭乱兜割" },
            ["1739"] = new SkillDefinition { Name = "看破斩", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "看破斩" },
            ["1740"] = new SkillDefinition { Name = "雷霆之镰(触发霹雳升龙斩时)", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "雷霆之镰(触发霹雳升龙斩时)" },
            ["1741"] = new SkillDefinition { Name = "雷霆之镰", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "雷霆之镰" },
            ["1742"] = new SkillDefinition { Name = "霹雳升龙斩", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "霹雳升龙斩" },
            ["44701"] = new SkillDefinition { Name = "月刃", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "月刃" },
            ["179908"] = new SkillDefinition { Name = "雷击", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "雷击" },
            ["179906"] = new SkillDefinition { Name = "月刃回旋", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "月刃回旋" },
            ["2031101"] = new SkillDefinition { Name = "幸运一击(太刀)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(太刀)" },
            ["2330"] = new SkillDefinition { Name = "火柱冲击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "火柱冲击" },
            ["55314"] = new SkillDefinition { Name = "安可治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "安可治疗" },
            ["230101"] = new SkillDefinition { Name = "聚合乐章/安可治疗相关", Type = SkillType.Heal, Element = ElementType.Fire, Description = "聚合乐章/安可治疗相关" },
            ["230401"] = new SkillDefinition { Name = "安可伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "安可伤害" },
            ["230501"] = new SkillDefinition { Name = "无限连奏安可伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "无限连奏安可伤害" },
            ["2031111"] = new SkillDefinition { Name = "幸运一击(灵魂乐手)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(灵魂乐手)" },
            ["2306"] = new SkillDefinition { Name = "增幅节拍", Type = SkillType.Damage, Element = ElementType.Fire, Description = "增幅节拍" },
            ["2317"] = new SkillDefinition { Name = "猛烈挥击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "猛烈挥击" },
            ["2321"] = new SkillDefinition { Name = "琴弦叩击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦叩击" },
            ["2322"] = new SkillDefinition { Name = "琴弦叩击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦叩击" },
            ["2323"] = new SkillDefinition { Name = "琴弦叩击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦叩击" },
            ["2324"] = new SkillDefinition { Name = "琴弦叩击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦叩击" },
            ["2331"] = new SkillDefinition { Name = "音浪", Type = SkillType.Damage, Element = ElementType.Fire, Description = "音浪" },
            ["2335"] = new SkillDefinition { Name = "无限狂想伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "无限狂想伤害" },
            ["230102"] = new SkillDefinition { Name = "聚合乐章", Type = SkillType.Damage, Element = ElementType.Fire, Description = "聚合乐章" },
            ["230103"] = new SkillDefinition { Name = "聚合乐章", Type = SkillType.Damage, Element = ElementType.Fire, Description = "聚合乐章" },
            ["230104"] = new SkillDefinition { Name = "聚合乐章", Type = SkillType.Damage, Element = ElementType.Fire, Description = "聚合乐章" },
            ["230105"] = new SkillDefinition { Name = "炎律狂踏伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "炎律狂踏伤害" },
            ["230106"] = new SkillDefinition { Name = "烈焰音符伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "烈焰音符伤害" },
            ["231001"] = new SkillDefinition { Name = "烈焰狂想伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "烈焰狂想伤害" },
            ["55301"] = new SkillDefinition { Name = "烈焰狂想治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "烈焰狂想治疗" },
            ["55311"] = new SkillDefinition { Name = "安可曲转化", Type = SkillType.Heal, Element = ElementType.Fire, Description = "安可曲转化" },
            ["55341"] = new SkillDefinition { Name = "英勇乐章治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "英勇乐章治疗" },
            ["55346"] = new SkillDefinition { Name = "无限狂想治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "无限狂想治疗" },
            ["55355"] = new SkillDefinition { Name = "休止的治愈", Type = SkillType.Heal, Element = ElementType.Fire, Description = "休止的治愈" },
            ["2207141"] = new SkillDefinition { Name = "音符", Type = SkillType.Heal, Element = ElementType.Fire, Description = "音符" },
            ["2207151"] = new SkillDefinition { Name = "炽焰治愈", Type = SkillType.Heal, Element = ElementType.Fire, Description = "炽焰治愈" },
            ["2207431"] = new SkillDefinition { Name = "炎律狂踏治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "炎律狂踏治疗" },
            ["2301"] = new SkillDefinition { Name = "琴弦撩拨", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦撩拨" },
            ["2302"] = new SkillDefinition { Name = "琴弦撩拨", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦撩拨" },
            ["2303"] = new SkillDefinition { Name = "琴弦撩拨", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦撩拨" },
            ["2304"] = new SkillDefinition { Name = "琴弦撩拨", Type = SkillType.Damage, Element = ElementType.Fire, Description = "琴弦撩拨" },
            ["2312"] = new SkillDefinition { Name = "激涌五重奏伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "激涌五重奏伤害" },
            ["2313"] = new SkillDefinition { Name = "热情挥洒", Type = SkillType.Damage, Element = ElementType.Fire, Description = "热情挥洒" },
            ["2332"] = new SkillDefinition { Name = "强化热情挥洒", Type = SkillType.Damage, Element = ElementType.Fire, Description = "强化热情挥洒" },
            ["2336"] = new SkillDefinition { Name = "巡演曲伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "巡演曲伤害" },
            ["2366"] = new SkillDefinition { Name = "巡演曲伤害", Type = SkillType.Damage, Element = ElementType.Fire, Description = "巡演曲伤害(音箱复读的)" },
            ["55302"] = new SkillDefinition { Name = "愈合节拍", Type = SkillType.Heal, Element = ElementType.Fire, Description = "愈合节拍" },
            ["55304"] = new SkillDefinition { Name = "激涌五重奏治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "激涌五重奏治疗" },
            ["55339"] = new SkillDefinition { Name = "巡演曲治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "巡演曲治疗" },
            ["55342"] = new SkillDefinition { Name = "愈合乐章治疗", Type = SkillType.Heal, Element = ElementType.Fire, Description = "愈合乐章治疗" },
            ["2207620"] = new SkillDefinition { Name = "活力解放", Type = SkillType.Heal, Element = ElementType.Fire, Description = "活力解放" },
            ["220101"] = new SkillDefinition { Name = "弓箭手普攻", Type = SkillType.Damage, Element = ElementType.Earth, Description = "弓箭手普攻" },
            ["220103"] = new SkillDefinition { Name = "弓箭手普攻", Type = SkillType.Damage, Element = ElementType.Earth, Description = "弓箭手普攻" },
            ["220104"] = new SkillDefinition { Name = "暴风箭矢", Type = SkillType.Damage, Element = ElementType.Wind, Description = "暴风箭矢" },
            ["2295"] = new SkillDefinition { Name = "锐眼·光能巨箭", Type = SkillType.Damage, Element = ElementType.Light, Description = "锐眼·光能巨箭" },
            ["2289"] = new SkillDefinition { Name = "箭雨", Type = SkillType.Damage, Element = ElementType.Earth, Description = "箭雨" },
            ["2233"] = new SkillDefinition { Name = "聚能射击", Type = SkillType.Damage, Element = ElementType.Light, Description = "聚能射击" },
            ["2288"] = new SkillDefinition { Name = "光能轰炸", Type = SkillType.Damage, Element = ElementType.Light, Description = "光能轰炸" },
            ["220102"] = new SkillDefinition { Name = "怒涛射击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "怒涛射击" },
            ["220108"] = new SkillDefinition { Name = "爆炸箭矢", Type = SkillType.Damage, Element = ElementType.Fire, Description = "爆炸箭矢" },
            ["55231"] = new SkillDefinition { Name = "爆炸射击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "爆炸射击" },
            ["220109"] = new SkillDefinition { Name = "威慑射击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "威慑射击" },
            ["1700820"] = new SkillDefinition { Name = "协同攻击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "协同攻击" },
            ["1700827"] = new SkillDefinition { Name = "狼普攻", Type = SkillType.Damage, Element = ElementType.Earth, Description = "狼普攻" },
            ["2292"] = new SkillDefinition { Name = "扑咬", Type = SkillType.Damage, Element = ElementType.Earth, Description = "扑咬" },
            ["2203512"] = new SkillDefinition { Name = "践踏", Type = SkillType.Damage, Element = ElementType.Earth, Description = "践踏" },
            ["120401"] = new SkillDefinition { Name = "冰法普攻", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰法普攻" },
            ["1203"] = new SkillDefinition { Name = "冰法普攻", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰法普攻" },
            ["120501"] = new SkillDefinition { Name = "冰法普攻", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰法普攻" },
            ["120201"] = new SkillDefinition { Name = "冰法普攻", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰法普攻" },
            ["120301"] = new SkillDefinition { Name = "冰法普攻", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰法普攻" },
            ["2031102"] = new SkillDefinition { Name = "幸运一击(冰法)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(冰法)" },
            ["120902"] = new SkillDefinition { Name = "冰矛", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰矛" },
            ["1248"] = new SkillDefinition { Name = "极寒·冰雪颂歌", Type = SkillType.Damage, Element = ElementType.Ice, Description = "极寒·冰雪颂歌" },
            ["1263"] = new SkillDefinition { Name = "极寒·冰雪颂歌", Type = SkillType.Damage, Element = ElementType.Ice, Description = "极寒·冰雪颂歌" },
            ["1262"] = new SkillDefinition { Name = "陨星风暴", Type = SkillType.Damage, Element = ElementType.Ice, Description = "陨星风暴" },
            ["121501"] = new SkillDefinition { Name = "清淹绕珠", Type = SkillType.Damage, Element = ElementType.Ice, Description = "清淹绕珠" },
            ["1216"] = new SkillDefinition { Name = "强化清淹绕珠", Type = SkillType.Damage, Element = ElementType.Ice, Description = "强化清淹绕珠" },
            ["1257"] = new SkillDefinition { Name = "寒冰风暴", Type = SkillType.Damage, Element = ElementType.Ice, Description = "寒冰风暴" },
            ["1250"] = new SkillDefinition { Name = "水之涡流", Type = SkillType.Damage, Element = ElementType.Ice, Description = "水之涡流" },
            ["2204081"] = new SkillDefinition { Name = "冰箭爆炸", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰箭爆炸" },
            ["121302"] = new SkillDefinition { Name = "冰箭", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰箭" },
            ["1259"] = new SkillDefinition { Name = "冰霜彗星", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰霜彗星" },
            ["120901"] = new SkillDefinition { Name = "贯穿冰矛", Type = SkillType.Damage, Element = ElementType.Ice, Description = "贯穿冰矛" },
            ["2204241"] = new SkillDefinition { Name = "冰霜冲击", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰霜冲击" },
            ["2401"] = new SkillDefinition { Name = "公正之剑", Type = SkillType.Damage, Element = ElementType.Light, Description = "公正之剑" },
            ["2402"] = new SkillDefinition { Name = "公正之剑", Type = SkillType.Damage, Element = ElementType.Light, Description = "公正之剑" },
            ["2403"] = new SkillDefinition { Name = "公正之剑", Type = SkillType.Damage, Element = ElementType.Light, Description = "公正之剑" },
            ["2404"] = new SkillDefinition { Name = "公正之剑", Type = SkillType.Damage, Element = ElementType.Light, Description = "公正之剑" },
            ["2416"] = new SkillDefinition { Name = "断罪", Type = SkillType.Damage, Element = ElementType.Light, Description = "断罪" },
            ["2417"] = new SkillDefinition { Name = "断罪", Type = SkillType.Damage, Element = ElementType.Light, Description = "断罪" },
            ["2407"] = new SkillDefinition { Name = "凛威·圣光灌注", Type = SkillType.Damage, Element = ElementType.Light, Description = "凛威·圣光灌注" },
            ["2031110"] = new SkillDefinition { Name = "幸运一击(剑盾)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(剑盾)" },
            ["2405"] = new SkillDefinition { Name = "英勇盾击", Type = SkillType.Damage, Element = ElementType.Light, Description = "英勇盾击" },
            ["2450"] = new SkillDefinition { Name = "光明冲击", Type = SkillType.Damage, Element = ElementType.Light, Description = "光明冲击" },
            ["2410"] = new SkillDefinition { Name = "裁决", Type = SkillType.Damage, Element = ElementType.Light, Description = "裁决" },
            ["2451"] = new SkillDefinition { Name = "裁决(神圣触发)", Type = SkillType.Damage, Element = ElementType.Light, Description = "裁决(神圣触发)" },
            ["2452"] = new SkillDefinition { Name = "灼热裁决", Type = SkillType.Damage, Element = ElementType.Fire, Description = "灼热裁决" },
            ["2412"] = new SkillDefinition { Name = "清算", Type = SkillType.Damage, Element = ElementType.Light, Description = "清算" },
            ["2413"] = new SkillDefinition { Name = "炽热清算", Type = SkillType.Damage, Element = ElementType.Fire, Description = "炽热清算" },
            ["240101"] = new SkillDefinition { Name = "投掷盾牌", Type = SkillType.Damage, Element = ElementType.Light, Description = "投掷盾牌" },
            ["2206401"] = new SkillDefinition { Name = "神圣之击", Type = SkillType.Damage, Element = ElementType.Light, Description = "神圣之击" },
            ["55421"] = new SkillDefinition { Name = "裁决治疗", Type = SkillType.Heal, Element = ElementType.Light, Description = "裁决治疗" },
            ["55404"] = new SkillDefinition { Name = "圣环伤害/治疗(相同编号)", Type = SkillType.Heal, Element = ElementType.Light, Description = "圣环伤害/治疗(相同编号)" },
            ["2406"] = new SkillDefinition { Name = "先锋打击/先锋追击", Type = SkillType.Damage, Element = ElementType.Light, Description = "先锋打击/先锋追击" },
            ["2421"] = new SkillDefinition { Name = "圣剑", Type = SkillType.Damage, Element = ElementType.Light, Description = "圣剑" },
            ["240102"] = new SkillDefinition { Name = "光明决心", Type = SkillType.Damage, Element = ElementType.Light, Description = "光明决心" },
            ["55412"] = new SkillDefinition { Name = "冷酷征伐", Type = SkillType.Damage, Element = ElementType.Light, Description = "冷酷征伐" },
            ["2206241"] = new SkillDefinition { Name = "神圣印记", Type = SkillType.Damage, Element = ElementType.Light, Description = "神圣印记" },
            ["2206552"] = new SkillDefinition { Name = "光明核心", Type = SkillType.Damage, Element = ElementType.Light, Description = "光明核心" },
            ["1005240"] = new SkillDefinition { Name = "绝技! 追猎猛斩(尖兵)", Type = SkillType.Damage, Element = ElementType.Dark, Description = "绝技! 追猎猛斩(尖兵)" },
            ["1006940"] = new SkillDefinition { Name = "奥义! 茧房术(蜘蛛)", Type = SkillType.Damage, Element = ElementType.Dark, Description = "奥义! 茧房术(蜘蛛)" },
            ["391006"] = new SkillDefinition { Name = "绝技! 纷乱飞弹(虚食人魔)", Type = SkillType.Damage, Element = ElementType.Dark, Description = "绝技! 纷乱飞弹(虚食人魔)" },
            ["1008440"] = new SkillDefinition { Name = "奥义! 沧澜风啸(飞鱼)", Type = SkillType.Damage, Element = ElementType.Wind, Description = "奥义! 沧澜风啸(飞鱼)" },
            ["391301"] = new SkillDefinition { Name = "绝技! 电磁爆弹(枪手)", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "绝技! 电磁爆弹(枪手)" },
            ["3913001"] = new SkillDefinition { Name = "绝技! 电磁爆弹(枪手)", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "绝技! 电磁爆弹(枪手)" },
            ["1008641"] = new SkillDefinition { Name = "飓风哥布林战士", Type = SkillType.Damage, Element = ElementType.Wind, Description = "飓风哥布林战士" },
            ["3210021"] = new SkillDefinition { Name = "奥义！ 流星陨落", Type = SkillType.Damage, Element = ElementType.Wind, Description = "飓风哥布林王" },
            ["2002853"] = new SkillDefinition { Name = "绝技！ 碎星陨落", Type = SkillType.Damage, Element = ElementType.Wind, Description = "火焰哥布林巫师" },
            ["1222"] = new SkillDefinition { Name = "幻影冲锋", Type = SkillType.Damage, Element = ElementType.Light, Description = "幻影冲锋" },
            ["2031105"] = new SkillDefinition { Name = "幸运伤害", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运伤害" }
        };

        public static readonly Dictionary<int, SkillDefinition> SkillsByInt = new();

        static EmbeddedSkillConfig()
        {
            foreach (var kv in SkillsByString)
            {
                if (int.TryParse(kv.Key, out var id))
                    SkillsByInt[id] = kv.Value;
            }
        }

        public static bool TryGet(string id, out SkillDefinition def) => SkillsByString.TryGetValue(id, out def!);
        public static bool TryGet(int id, out SkillDefinition def) => SkillsByInt.TryGetValue(id, out def!);

        public static string GetName(string id) => TryGet(id, out var d) ? d.Name : id;
        public static string GetName(int id) => TryGet(id, out var d) ? d.Name : id.ToString();

        public static SkillType GetTypeOf(string id) => TryGet(id, out var d) ? d.Type : SkillType.Unknown;
        public static SkillType GetTypeOf(int id) => TryGet(id, out var d) ? d.Type : SkillType.Unknown;

        public static ElementType GetElementOf(string id) => TryGet(id, out var d) ? d.Element : ElementType.Unknown;
        public static ElementType GetElementOf(int id) => TryGet(id, out var d) ? d.Element : ElementType.Unknown;

        public static IReadOnlyDictionary<string, SkillDefinition> AllByString => SkillsByString;
        public static IReadOnlyDictionary<int, SkillDefinition> AllByInt => SkillsByInt;
    }
}