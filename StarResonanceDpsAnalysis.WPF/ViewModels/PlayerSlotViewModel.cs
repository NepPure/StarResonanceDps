using System;

using CommunityToolkit.Mvvm.ComponentModel;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.Controls.Models;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

/// <summary>
/// 用于 DataTemplate 绑定的数据载体（挂到 ProgressBarData.Data 上）
/// </summary>
public partial class PlayerSlotViewModel : OrderingData
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private string _nickname = string.Empty;

    [ObservableProperty] private Classes _class = Classes.Unknown;

    [ObservableProperty] private string _valueText = string.Empty;

    public string OrderText => $"{Order:00}.";
}