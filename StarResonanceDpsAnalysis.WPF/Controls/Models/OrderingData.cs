using CommunityToolkit.Mvvm.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.Controls.Models;

public abstract partial class OrderingData : ObservableObject
{
    [ObservableProperty] protected int _order;
}