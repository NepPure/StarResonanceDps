using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StarResonanceDpsAnalysis.WPF.ViewModels;

using StarResonanceDpsAnalysis.WPF.Controls.Models;

namespace StarResonanceDpsAnalysis.WPF.Controls;

/// <summary>
/// SortedProgressBarList.xaml 的交互逻辑
/// </summary>
public partial class SortedProgressBarList : UserControl
{
    public delegate void ProgressBarMouseEnterEventHandler(CustomizeProgressBar sender, MouseEventArgs e, ProgressBarData? data);
    public delegate void ProgressBarMouseMoveEventHandler(CustomizeProgressBar sender, MouseEventArgs e, ProgressBarData? data);
    public delegate void ProgressBarMouseLeaveEventHandler(CustomizeProgressBar sender, MouseEventArgs e, ProgressBarData? data);
    public delegate void ProgressBarMouseDownEventHandler(CustomizeProgressBar sender, MouseButtonEventArgs e, ProgressBarData? data);

    public event ProgressBarMouseEnterEventHandler? ProgressBarMouseEnter;
    public event ProgressBarMouseMoveEventHandler? ProgressBarMouseMove;
    public event ProgressBarMouseLeaveEventHandler? ProgressBarMouseLeave;
    public event ProgressBarMouseDownEventHandler? ProgressBarMouseDown;

    public static readonly DependencyProperty ProgressBarSlotDataTemplateProperty =
        DependencyProperty.Register(
            nameof(ProgressBarSlotDataTemplate),
            typeof(DataTemplate),
            typeof(SortedProgressBarList),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty ProgressBarHeightProperty =
        DependencyProperty.Register(
            nameof(ProgressBarHeight),
            typeof(double),
            typeof(SortedProgressBarList),
            new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty ProgressBarMarginProperty =
        DependencyProperty.Register(
            nameof(ProgressBarMargin),
            typeof(Thickness),
            typeof(SortedProgressBarList),
            new FrameworkPropertyMetadata(new Thickness(3), FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(ICollection<ProgressBarData>), typeof(SortedProgressBarList),
        new PropertyMetadata(default(ICollection<ProgressBarData>)));

    private readonly Dictionary<long, ProgressBarData> _dataDict = [];

    public SortedProgressBarList()
    {
        InitializeComponent();
    }

    public DataTemplate ProgressBarSlotDataTemplate
    {
        get => (DataTemplate)GetValue(ProgressBarSlotDataTemplateProperty);
        set => SetValue(ProgressBarSlotDataTemplateProperty, value);
    }

    public double ProgressBarHeight
    {
        get => (double)GetValue(ProgressBarHeightProperty);
        set => SetValue(ProgressBarHeightProperty, value);
    }

    public Thickness ProgressBarMargin
    {
        get => (Thickness)GetValue(ProgressBarMarginProperty);
        set => SetValue(ProgressBarMarginProperty, value);
    }

    /// <summary>
    /// 数据源
    /// </summary>
    public List<ProgressBarData>? Data
    {
        // 无需双向通知
        set
        {
            lock (_dataLock)
            {
                // 清除所有项目
                if (value == null || value.Count == 0)
                {
                    _dataDict.Clear();

                    return;
                }

                // 移除不存在的项
                foreach (var key in _dataDict.Keys.ToList())
                {
                    if (value.Exists(e => e.ID == key)) continue;

                    _dataDict.Remove(key);
                }

                // 更新或新增项（后者覆盖同 ID）
                foreach (var item in value)
                {
                    if (item == null || item.ID < 0) continue;

                    _dataDict[item.ID] = item;
                }

                UpdateAnimation();
            }
        }
    }
}