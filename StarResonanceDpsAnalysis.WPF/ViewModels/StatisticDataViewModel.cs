using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

[DebuggerDisplay("Name:{Player.Name};Value:{Value}")]
public partial class StatisticDataViewModel : BaseViewModel, IComparable<StatisticDataViewModel>
{
    [ObservableProperty] private long _index;
    [ObservableProperty] private ulong _value;
    [ObservableProperty] private ulong _duration;
    [ObservableProperty] private double _percentOfMax;
    [ObservableProperty] private double _percent;
    [ObservableProperty] private PlayerInfoViewModel _player = new();

    public int CompareTo(StatisticDataViewModel? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}