using CommunityToolkit.Mvvm.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public abstract partial class OrderingDataViewModel : ObservableObject
{
    [ObservableProperty] protected int _order;
}