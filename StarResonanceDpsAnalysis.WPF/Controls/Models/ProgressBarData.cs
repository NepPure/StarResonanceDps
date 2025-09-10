using System.Windows.Media;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Controls.Models;

public class ProgressBarData
{
    public long ID { get; set; }
    public double ProgressBarValue { get; set; }
    public OrderingData? Data { get; set; }
}