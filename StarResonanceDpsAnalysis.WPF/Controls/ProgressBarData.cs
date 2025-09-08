using System.Windows.Media;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Controls;

public class ProgressBarData
{
    public long ID { get; set; }
    public double ProgressBarValue { get; set; }
    public SolidColorBrush ProgressBarBrush { get; set; } = new(Color.FromRgb(0x56, 0x9C, 0xD6));
    public double ProgressBarCornerRadius { get; set; }
    public OrderingDataViewModel? Data { get; set; }
}