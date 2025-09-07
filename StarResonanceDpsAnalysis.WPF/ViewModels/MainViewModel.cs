using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.WPF.Views;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class MainViewModel : BaseViewModel
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
}