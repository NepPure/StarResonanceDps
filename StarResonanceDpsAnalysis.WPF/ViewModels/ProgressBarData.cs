using StarResonanceDpsAnalysis.Core.Models;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public class ProgressBarData : BaseViewModel
{
    public long ID { get; set; }
    public double ProgressBarValue { get; set; }
    public Classes Classes { get; set; } = Classes.FrostMage;
    public OrderingDataViewModel? Data { get; set; }
}