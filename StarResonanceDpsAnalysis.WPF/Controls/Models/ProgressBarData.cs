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