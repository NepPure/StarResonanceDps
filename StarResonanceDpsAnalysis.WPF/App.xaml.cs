using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static ILogger<App>? _logger;

    [STAThread]
    private static void Main(string[] args)
    {
        // 创建主机
        using var host = CreateHostBuilder(args).Build();
        _logger = host.Services.GetService<ILogger<App>>();

        App app = new()
        {
            MainWindow = host.Services.GetRequiredService<MainWindow>()
        };
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var ret =
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(AppContext.BaseDirectory);
                    // 读取配置文件
                    builder.AddConfiguration(new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("appsettings.Development.json", true)
                        .Build());
                })
                // 注册服务
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<MainWindow>();
                    // 注册主线程调度器
                    services.AddSingleton(_ => Current.Dispatcher);
                })
                .ConfigureLogging(config => { config.AddConsole(); });
        return ret;
    }
}