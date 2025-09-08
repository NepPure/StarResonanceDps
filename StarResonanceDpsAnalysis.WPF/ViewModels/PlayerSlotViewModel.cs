using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

/// <summary>
/// 用于 DataTemplate 绑定的数据载体（挂到 ProgressBarData.Data 上）
/// </summary>
public partial class PlayerSlotViewModel : OrderingDataViewModel
{
    [ObservableProperty] private BitmapImage? _icon;

    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private string _nickname = string.Empty;

    [ObservableProperty] private string _profession = string.Empty;

    [ObservableProperty] private string _valueText = string.Empty;

    public string OrderText => $"{Order:00}.";
}