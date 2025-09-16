namespace StarResonanceDpsAnalysis.WPF.Config;

public interface IConfigManager
{
    Task SaveAsync(AppConfig newConfig);
    event EventHandler<AppConfig>? ConfigurationUpdated;
    AppConfig CurrentConfig { get; }
}