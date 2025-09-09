namespace StarResonanceDpsAnalysis.WPF.Config;

public class Configuration
{
    public Setup SetUp { get; set; } = new();
    public Shortcut SetKey { get; set; } = new();

    public class Setup
    {
        public int NetworkCard { get; set; } = 9;
        public string StartUpState { get; set; } = "2262,580,526,441";
        public int ClearPicture { get; set; } = 1;
        public string NickName { get; set; } = "未知";
        public long Uid { get; set; } = 0;
        public string Profession { get; set; } = "未知";
        public long CombatPower { get; set; } = 0;
        public int Transparency { get; set; } = 100;
        public int CombatTimeClearDelaySeconds { get; set; } = 5;
        public DamageDisplayType DamageDisplayType { get; set; } = DamageDisplayType.KMB;
    }

    public class Shortcut
    {
        public int MouseThroughKey { get; set; } = 131189;
        public int ClearDataKey { get; set; } = 131190;
        public int ClearHistoryKey { get; set; } = 131193;
    }
}

/// <summary>
/// 伤害显示类型
/// </summary>
public enum DamageDisplayType
{
    /// <summary>
    /// Kilo Mega Billion
    /// </summary>
    KMB, // KMB
    /// <summary>
    /// 万 亿
    /// </summary>
    Wan, 
}