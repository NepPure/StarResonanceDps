using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.WPF.Themes;
using StarResonanceDpsAnalysis.WPF.Views;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class MainViewModel(ApplicationThemeManager themeManager, DpsStatisticsView dpsStatisticsView) : BaseViewModel
{
    public partial class DebugFunctions
    {
        [RelayCommand]
        private void CallDebugWindow()
        {
            var debugWindow = new DebugView();
            debugWindow.Show();
        }

    }

    public DebugFunctions Debug { get; } = new();

    [ObservableProperty] private ApplicationTheme _theme = themeManager.GetAppTheme();
    [ObservableProperty] private List<ApplicationTheme> _availableThemes = [ApplicationTheme.Light, ApplicationTheme.Dark];

    partial void OnThemeChanged(ApplicationTheme value)
    {
        themeManager.Apply(value);
    }
    [RelayCommand]
    private void CallDpsStatisticsView()
    {
        dpsStatisticsView.Show();
    }
}