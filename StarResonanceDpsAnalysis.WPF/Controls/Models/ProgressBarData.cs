using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Controls.Models;

public class ProgressBarData : BaseViewModel
{
    public long ID { get; set; }
    public double ProgressBarValue { get; set; }
    public Classes Classes { get; set; } = Classes.FrostMage;
    public OrderingDataViewModel? Data { get; set; }
}

[DebuggerDisplay("Name:{Name};Value:{Value}")]
public partial class StatisticDataViewModel : BaseViewModel, IComparable<StatisticDataViewModel>
{

    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private Classes _classes = Classes.FrostMage;
    [ObservableProperty] private ulong _value;
    [ObservableProperty] private double _percentOfMax;
    [ObservableProperty] private double _percent;

    public int CompareTo(StatisticDataViewModel? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}