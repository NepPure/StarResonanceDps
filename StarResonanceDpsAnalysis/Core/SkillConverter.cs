namespace StarResonanceDpsAnalysis.Core
{
    // Auto-generated from skill_config.json (v2.0.1)
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
        Physics,   // ← 新增
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
        public static readonly string Version = "2.0.1";      // ← 更新
        public static readonly string LastUpdated = "2025-01-20"; // ← 更新

        // 与 skill_config.json 的 elements 完全一致
        public static readonly Dictionary<ElementType, ElementInfo> Elements = new()
        {
            [ElementType.Fire] = new ElementInfo { Color = "#ff6b6b", Icon = "🔥" },
            [ElementType.Ice] = new ElementInfo { Color = "#74c0fc", Icon = "❄️" },
            [ElementType.Thunder] = new ElementInfo { Color = "#ffd43b", Icon = "⚡" },
            [ElementType.Earth] = new ElementInfo { Color = "#8ce99a", Icon = "🍀" }, // ← 图标从🌍改为🍀
            [ElementType.Wind] = new ElementInfo { Color = "#91a7ff", Icon = "💨" },
            [ElementType.Light] = new ElementInfo { Color = "#fff3bf", Icon = "✨" },
            [ElementType.Dark] = new ElementInfo { Color = "#9775fa", Icon = "🌙" },
            [ElementType.Physics] = new ElementInfo { Color = "#91a7ff", Icon = "⚔️" }  // ← 新增
        };

        // 与 skill_config.json 的 skills 完全一致（覆盖之前的列表）
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
            ["2205450"] = new SkillDefinition { Name = "勇气风环吸血", Type = SkillType.Damage, Element = ElementType.Wind, Description = "勇气风环吸血" },
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

            ["1501"] = new SkillDefinition { Name = "掌控藤曼", Type = SkillType.Damage, Element = ElementType.Earth, Description = "掌控藤曼" },
            ["1502"] = new SkillDefinition { Name = "掌控藤曼", Type = SkillType.Damage, Element = ElementType.Earth, Description = "掌控藤曼" },
            ["1503"] = new SkillDefinition { Name = "掌控藤曼", Type = SkillType.Damage, Element = ElementType.Earth, Description = "掌控藤曼" },
            ["1504"] = new SkillDefinition { Name = "掌控藤曼", Type = SkillType.Damage, Element = ElementType.Earth, Description = "掌控藤曼" },
            ["1509"] = new SkillDefinition { Name = "希望结界伤害", Type = SkillType.Damage, Element = ElementType.Earth, Description = "希望结界伤害" },
            ["1518"] = new SkillDefinition { Name = "狂野绽放", Type = SkillType.Damage, Element = ElementType.Earth, Description = "狂野绽放" },
            ["1529"] = new SkillDefinition { Name = "盛放注能(包含伤害和治疗)", Type = SkillType.Damage, Element = ElementType.Earth, Description = "盛放注能(包含伤害和治疗)" },
            ["1550"] = new SkillDefinition { Name = "不羁之种", Type = SkillType.Damage, Element = ElementType.Earth, Description = "不羁之种" },
            ["1551"] = new SkillDefinition { Name = "狂野之种", Type = SkillType.Damage, Element = ElementType.Earth, Description = "狂野之种" },
            ["1560"] = new SkillDefinition { Name = "再生脉冲", Type = SkillType.Damage, Element = ElementType.Earth, Description = "再生脉冲" },
            ["20301"] = new SkillDefinition { Name = "生命绽放", Type = SkillType.Heal, Element = ElementType.Earth, Description = "生命绽放" },
            ["21402"] = new SkillDefinition { Name = "狂野绽放治疗", Type = SkillType.Heal, Element = ElementType.Earth, Description = "狂野绽放治疗" },
            ["21404"] = new SkillDefinition { Name = "滋养", Type = SkillType.Heal, Element = ElementType.Earth, Description = "滋养" },
            ["21406"] = new SkillDefinition { Name = "森之祈愿", Type = SkillType.Heal, Element = ElementType.Earth, Description = "森之祈愿" },
            ["21414"] = new SkillDefinition { Name = "希望结界持续(包含伤害和治疗)", Type = SkillType.Heal, Element = ElementType.Earth, Description = "希望结界持续(包含伤害和治疗)" },
            ["21427"] = new SkillDefinition { Name = "休止的治愈", Type = SkillType.Heal, Element = ElementType.Earth, Description = "休止的治愈" },
            ["21428"] = new SkillDefinition { Name = "休止的治愈", Type = SkillType.Heal, Element = ElementType.Earth, Description = "休止的治愈" },
            ["21429"] = new SkillDefinition { Name = "休止的治愈", Type = SkillType.Heal, Element = ElementType.Earth, Description = "休止的治愈" },
            ["21430"] = new SkillDefinition { Name = "休止的治愈", Type = SkillType.Heal, Element = ElementType.Earth, Description = "休止的治愈" },
            ["150103"] = new SkillDefinition { Name = "不羁之种", Type = SkillType.Damage, Element = ElementType.Earth, Description = "不羁之种" },
            ["150104"] = new SkillDefinition { Name = "不羁之种", Type = SkillType.Damage, Element = ElementType.Earth, Description = "不羁之种" },
            ["150106"] = new SkillDefinition { Name = "灌注", Type = SkillType.Heal, Element = ElementType.Earth, Description = "灌注" },
            ["150107"] = new SkillDefinition { Name = "灌注", Type = SkillType.Heal, Element = ElementType.Earth, Description = "灌注" },
            ["2031005"] = new SkillDefinition { Name = "幸运一击(森语者)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(森语者)" },
            ["2202091"] = new SkillDefinition { Name = "治疗链接", Type = SkillType.Heal, Element = ElementType.Earth, Description = "治疗链接" },
            ["2202311"] = new SkillDefinition { Name = "滋养之种", Type = SkillType.Heal, Element = ElementType.Earth, Description = "滋养之种" },
            ["1541"] = new SkillDefinition { Name = "狂野绽放", Type = SkillType.Damage, Element = ElementType.Earth, Description = "狂野绽放" },
            ["1561"] = new SkillDefinition { Name = "再生脉冲(包含伤害和治疗)", Type = SkillType.Damage, Element = ElementType.Earth, Description = "再生脉冲(包含伤害和治疗)" },
            ["21423"] = new SkillDefinition { Name = "共生印记", Type = SkillType.Heal, Element = ElementType.Earth, Description = "共生印记" },
            ["21424"] = new SkillDefinition { Name = "荆棘", Type = SkillType.Damage, Element = ElementType.Earth, Description = "荆棘" },
            ["150101"] = new SkillDefinition { Name = "鹿之奔袭", Type = SkillType.Damage, Element = ElementType.Earth, Description = "鹿之奔袭" },
            ["150110"] = new SkillDefinition { Name = "灌注", Type = SkillType.Heal, Element = ElementType.Earth, Description = "灌注" },

            ["2031105"] = new SkillDefinition { Name = "幸运一击(惩戒)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(惩戒)" },
            ["220101"] = new SkillDefinition { Name = "弹无虚发", Type = SkillType.Damage, Element = ElementType.Earth, Description = "弹无虚发" },
            ["220103"] = new SkillDefinition { Name = "弹无虚发", Type = SkillType.Damage, Element = ElementType.Earth, Description = "弹无虚发" },
            ["220104"] = new SkillDefinition { Name = "暴风箭矢", Type = SkillType.Damage, Element = ElementType.Wind, Description = "暴风箭矢" },
            ["2295"] = new SkillDefinition { Name = "锐眼·光能巨箭", Type = SkillType.Damage, Element = ElementType.Light, Description = "锐眼·光能巨箭" },
            ["2291"] = new SkillDefinition { Name = "锐眼·光能巨箭(天赋)", Type = SkillType.Damage, Element = ElementType.Light, Description = "锐眼·光能巨箭(天赋)" },
            ["2289"] = new SkillDefinition { Name = "箭雨", Type = SkillType.Damage, Element = ElementType.Earth, Description = "箭雨" },
            ["2233"] = new SkillDefinition { Name = "聚能射击", Type = SkillType.Damage, Element = ElementType.Light, Description = "聚能射击" },
            ["2288"] = new SkillDefinition { Name = "光能轰炸", Type = SkillType.Damage, Element = ElementType.Light, Description = "光能轰炸" },
            ["220102"] = new SkillDefinition { Name = "怒涛射击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "怒涛射击" },
            ["220108"] = new SkillDefinition { Name = "爆炸箭矢", Type = SkillType.Damage, Element = ElementType.Fire, Description = "爆炸箭矢" },
            ["220109"] = new SkillDefinition { Name = "威慑射击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "威慑射击" },
            ["1700820"] = new SkillDefinition { Name = "狼协同攻击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "狼协同攻击" },
            ["1700827"] = new SkillDefinition { Name = "狼普攻", Type = SkillType.Damage, Element = ElementType.Earth, Description = "狼普攻" },
            ["2292"] = new SkillDefinition { Name = "扑咬", Type = SkillType.Damage, Element = ElementType.Earth, Description = "扑咬" },
            ["2203512"] = new SkillDefinition { Name = "践踏", Type = SkillType.Damage, Element = ElementType.Earth, Description = "践踏" },
            ["55231"] = new SkillDefinition { Name = "爆炸射击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "爆炸射击" },
            ["220110"] = new SkillDefinition { Name = "爆炸箭引爆", Type = SkillType.Damage, Element = ElementType.Fire, Description = "爆炸箭引爆" },
            ["2203291"] = new SkillDefinition { Name = "猎鹰出击", Type = SkillType.Damage, Element = ElementType.Light, Description = "猎鹰出击" },
            ["220113"] = new SkillDefinition { Name = "幻影猎鹰", Type = SkillType.Damage, Element = ElementType.Light, Description = "幻影猎鹰" },
            ["2203621"] = new SkillDefinition { Name = "光棱", Type = SkillType.Damage, Element = ElementType.Light, Description = "光棱" },
            ["2203622"] = new SkillDefinition { Name = "光棱溅射", Type = SkillType.Damage, Element = ElementType.Light, Description = "光棱溅射" },
            ["220112"] = new SkillDefinition { Name = "光能裂隙", Type = SkillType.Damage, Element = ElementType.Light, Description = "光能裂隙" },
            ["220106"] = new SkillDefinition { Name = "二连矢", Type = SkillType.Damage, Element = ElementType.Earth, Description = "二连矢" },
            ["2203521"] = new SkillDefinition { Name = "内爆(钢制喙触发)", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "内爆(钢制喙触发)" },
            ["2203181"] = new SkillDefinition { Name = "闪电冲击", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "闪电冲击" },
            ["2294"] = new SkillDefinition { Name = "光意·四连矢", Type = SkillType.Damage, Element = ElementType.Light, Description = "光意·四连矢" },
            ["220111"] = new SkillDefinition { Name = "光意·四连矢", Type = SkillType.Damage, Element = ElementType.Light, Description = "光意·四连矢" },
            ["220203"] = new SkillDefinition { Name = "光意·四连矢", Type = SkillType.Damage, Element = ElementType.Light, Description = "光意·四连矢" },
            ["2031109"] = new SkillDefinition { Name = "幸运一击(弓箭手)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(弓箭手)" },
            ["220301"] = new SkillDefinition { Name = "锐眼·光能巨箭", Type = SkillType.Damage, Element = ElementType.Light, Description = "锐眼·光能巨箭" },
            ["2352"] = new SkillDefinition { Name = "天界雄鹰", Type = SkillType.Damage, Element = ElementType.Light, Description = "天界雄鹰" },

            ["120401"] = new SkillDefinition { Name = "雨打潮生", Type = SkillType.Damage, Element = ElementType.Ice, Description = "雨打潮生" },
            ["1203"] = new SkillDefinition { Name = "雨打潮生", Type = SkillType.Damage, Element = ElementType.Ice, Description = "雨打潮生" },
            ["120501"] = new SkillDefinition { Name = "雨打潮生", Type = SkillType.Damage, Element = ElementType.Ice, Description = "雨打潮生" },
            ["120201"] = new SkillDefinition { Name = "雨打潮生", Type = SkillType.Damage, Element = ElementType.Ice, Description = "雨打潮生" },
            ["120301"] = new SkillDefinition { Name = "雨打潮生", Type = SkillType.Damage, Element = ElementType.Ice, Description = "雨打潮生" },
            ["2031102"] = new SkillDefinition { Name = "幸运一击(冰法)", Type = SkillType.Damage, Element = ElementType.Light, Description = "幸运一击(冰法)" },
            ["1248"] = new SkillDefinition { Name = "极寒·冰雪颂歌", Type = SkillType.Damage, Element = ElementType.Ice, Description = "极寒·冰雪颂歌" },
            ["1263"] = new SkillDefinition { Name = "极寒·冰雪颂歌", Type = SkillType.Damage, Element = ElementType.Ice, Description = "极寒·冰雪颂歌" },
            ["120902"] = new SkillDefinition { Name = "冰矛", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰矛" },
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
            ["1261"] = new SkillDefinition { Name = "瞬发寒冰风暴", Type = SkillType.Damage, Element = ElementType.Ice, Description = "瞬发寒冰风暴" },

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
            ["3210081"] = new SkillDefinition { Name = "蜥蜴人猎手（被动）", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "蜥蜴人猎手（被动）" },
            ["391007"] = new SkillDefinition { Name = "哥布林弩手（被动）", Type = SkillType.Damage, Element = ElementType.Physics, Description = "哥布林弩手（被动）" },
            ["391008"] = new SkillDefinition { Name = "变异峰（被动）", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "变异峰弩手（被动）" },
            ["1700440"] = new SkillDefinition { Name = "慕课头目", Type = SkillType.Damage, Element = ElementType.Dark, Description = "慕课头目" },

            ["1222"] = new SkillDefinition { Name = "幻影冲锋", Type = SkillType.Damage, Element = ElementType.Light, Description = "幻影冲锋" },
            ["1241"] = new SkillDefinition { Name = "射线", Type = SkillType.Damage, Element = ElementType.Ice, Description = "射线" },
            ["199902"] = new SkillDefinition { Name = "岩盾", Type = SkillType.Damage, Element = ElementType.Earth, Description = "岩盾" },


            // Ice 系
            ["1240"] = new SkillDefinition { Name = "冻结寒风", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冻结寒风" }, // ← 新增
            ["1242"] = new SkillDefinition { Name = "冰霜之矛", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰霜之矛" }, // ← 新增
            ["1243"] = new SkillDefinition { Name = "冰之灌注", Type = SkillType.Damage, Element = ElementType.Ice, Description = "冰之灌注" }, // ← 新增
            ["1245"] = new SkillDefinition { Name = "寒冰庇护", Type = SkillType.Heal, Element = ElementType.Ice, Description = "寒冰庇护" }, // ← 新增
            ["1246"] = new SkillDefinition { Name = "浪潮汇聚-水龙卷", Type = SkillType.Damage, Element = ElementType.Ice, Description = "浪潮汇聚-水龙卷" }, // ← 新增
            ["1247"] = new SkillDefinition { Name = "天赋触发彗星", Type = SkillType.Damage, Element = ElementType.Ice, Description = "天赋触发彗星" }, // ← 新增
            ["1249"] = new SkillDefinition { Name = "协同冰晶", Type = SkillType.Damage, Element = ElementType.Ice, Description = "协同冰晶" }, // ← 新增

            // Wind 系
            ["1405"] = new SkillDefinition { Name = "疾风刺", Type = SkillType.Damage, Element = ElementType.Wind, Description = "疾风刺" }, // ← 新增
            ["1406"] = new SkillDefinition { Name = "风华翔舞", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风华翔舞" }, // ← 新增
            ["1407"] = new SkillDefinition { Name = "风神", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风神" }, // ← 新增
            ["1410"] = new SkillDefinition { Name = "风神·破阵之风", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风神·破阵之风" }, // ← 新增
            ["1426"] = new SkillDefinition { Name = "风神·破阵之风", Type = SkillType.Damage, Element = ElementType.Wind, Description = "风神·破阵之风" }, // ← 新增
            ["1430"] = new SkillDefinition { Name = "翔返", Type = SkillType.Damage, Element = ElementType.Wind, Description = "翔返(额外版本)" }, // ← 新增

            // Earth 系
            ["1517"] = new SkillDefinition { Name = "掌控藤蔓-红光反制", Type = SkillType.Damage, Element = ElementType.Earth, Description = "掌控藤蔓-红光反制" }, // ← 新增
            ["1527"] = new SkillDefinition { Name = "花语回升", Type = SkillType.Heal, Element = ElementType.Earth, Description = "花语回升" }, // ← 新增
            ["1556"] = new SkillDefinition { Name = "花语回升", Type = SkillType.Heal, Element = ElementType.Earth, Description = "花语回升" }, // ← 新增
            ["1562"] = new SkillDefinition { Name = "再生脉冲-扩散治疗", Type = SkillType.Heal, Element = ElementType.Earth, Description = "再生脉冲-扩散治疗" }, // ← 新增

            // Thunder 系
            ["1711"] = new SkillDefinition { Name = "我流秘刀法", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流秘刀法" }, // ← 新增
            ["1712"] = new SkillDefinition { Name = "我流秘刀法2", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流秘刀法2" }, // ← 新增
            ["1716"] = new SkillDefinition { Name = "超高出力", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "超高出力" }, // ← 新增
            ["1720"] = new SkillDefinition { Name = "心眼", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "心眼" }, // ← 新增
            ["1721"] = new SkillDefinition { Name = "我流秘刀法·壹之型·改", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流秘刀法·壹之型·改" }, // ← 新增
            ["1722"] = new SkillDefinition { Name = "我流秘刀法·贰之型·改", Type = SkillType.Damage, Element = ElementType.Thunder, Description = "我流秘刀法·贰之型·改" }, // ← 新增

            // Rock / Shield 系
            ["1905"] = new SkillDefinition { Name = "斩龙式", Type = SkillType.Damage, Element = ElementType.Earth, Description = "斩龙式" }, // ← 新增
            ["1906"] = new SkillDefinition { Name = "后撤斩", Type = SkillType.Damage, Element = ElementType.Earth, Description = "后撤斩" }, // ← 新增
            ["1907"] = new SkillDefinition { Name = "岩御·崩裂回环", Type = SkillType.Damage, Element = ElementType.Earth, Description = "岩御·崩裂回环" }, // ← 新增
            ["1917"] = new SkillDefinition { Name = "巨岩撞击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "巨岩撞击" }, // ← 新增
            ["1922"] = new SkillDefinition { Name = "护盾猛击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "护盾猛击" }, // ← 新增
            ["1925"] = new SkillDefinition { Name = "怒爆", Type = SkillType.Damage, Element = ElementType.Earth, Description = "怒爆" }, // ← 新增
            ["1926"] = new SkillDefinition { Name = "砂岩之握", Type = SkillType.Damage, Element = ElementType.Earth, Description = "砂岩之握" }, // ← 新增
            ["1928"] = new SkillDefinition { Name = "岩之守护", Type = SkillType.Heal, Element = ElementType.Earth, Description = "岩之守护" }, // ← 新增
            ["1929"] = new SkillDefinition { Name = "不动如山", Type = SkillType.Heal, Element = ElementType.Earth, Description = "不动如山" }, // ← 新增
            ["1936"] = new SkillDefinition { Name = "巨岩躯体", Type = SkillType.Heal, Element = ElementType.Earth, Description = "巨岩躯体" }, // ← 新增
            ["1938"] = new SkillDefinition { Name = "勇者壁垒", Type = SkillType.Heal, Element = ElementType.Earth, Description = "勇者壁垒" }, // ← 新增
            ["1941"] = new SkillDefinition { Name = "碎星崩裂", Type = SkillType.Damage, Element = ElementType.Earth, Description = "碎星崩裂" }, // ← 新增
            ["1943"] = new SkillDefinition { Name = "巨岩轰击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "巨岩轰击" }, // ← 新增

            // 其他 (选摘)
            ["2220"] = new SkillDefinition { Name = "暴风箭矢", Type = SkillType.Damage, Element = ElementType.Wind, Description = "暴风箭矢" }, // ← 新增
            ["2221"] = new SkillDefinition { Name = "暴风箭矢SP", Type = SkillType.Damage, Element = ElementType.Wind, Description = "暴风箭矢SP" }, // ← 新增
            ["2230"] = new SkillDefinition { Name = "怒涛射击", Type = SkillType.Damage, Element = ElementType.Earth, Description = "怒涛射击" }, // ← 新增
            ["2231"] = new SkillDefinition { Name = "精神凝聚", Type = SkillType.Damage, Element = ElementType.Light, Description = "精神凝聚" }, // ← 新增
            ["2232"] = new SkillDefinition { Name = "箭雨", Type = SkillType.Damage, Element = ElementType.Wind, Description = "箭雨" }, // ← 新增
            ["2234"] = new SkillDefinition { Name = "光能轰炸", Type = SkillType.Damage, Element = ElementType.Light, Description = "光能轰炸" }, // ← 新增
            ["2237"] = new SkillDefinition { Name = "狂野呼唤", Type = SkillType.Damage, Element = ElementType.Wind, Description = "狂野呼唤" }, // ← 新增
            ["2238"] = new SkillDefinition { Name = "爆炸射击", Type = SkillType.Damage, Element = ElementType.Fire, Description = "爆炸射击" }, // ← 新增


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
