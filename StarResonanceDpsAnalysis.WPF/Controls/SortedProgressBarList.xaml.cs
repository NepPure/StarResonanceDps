using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StarResonanceDpsAnalysis.WPF.Controls.Models;

namespace StarResonanceDpsAnalysis.WPF.Controls;

/// <summary>
///     SortedProgressBarList.xaml 的交互逻辑
/// </summary>
public partial class SortedProgressBarList : UserControl
{
    public delegate void ProgressBarMouseDownEventHandler(CustomizeProgressBar sender, MouseButtonEventArgs e,
        ProgressBarData? data);

    public delegate void ProgressBarMouseEnterEventHandler(CustomizeProgressBar sender, MouseEventArgs e,
        ProgressBarData? data);

    public delegate void ProgressBarMouseLeaveEventHandler(CustomizeProgressBar sender, MouseEventArgs e,
        ProgressBarData? data);

    public delegate void ProgressBarMouseMoveEventHandler(CustomizeProgressBar sender, MouseEventArgs e,
        ProgressBarData? data);

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
        nameof(Data), typeof(IEnumerable<ProgressBarData>), typeof(SortedProgressBarList),
        new PropertyMetadata(default(IEnumerable<ProgressBarData>), OnDataPropertyChanged));

    // Use a standard Dictionary initializer
    private readonly Dictionary<long, ProgressBarData> _dataDict = new();

    private readonly object _dataLock = new();

    public SortedProgressBarList()
    {
        InitializeComponent();
    }

    public IEnumerable<ProgressBarData>? Data
    {
        get => (IEnumerable<ProgressBarData>?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
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

    public event ProgressBarMouseEnterEventHandler? ProgressBarMouseEnter;
    public event ProgressBarMouseMoveEventHandler? ProgressBarMouseMove;
    public event ProgressBarMouseLeaveEventHandler? ProgressBarMouseLeave;
    public event ProgressBarMouseDownEventHandler? ProgressBarMouseDown;

    private static void OnDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SortedProgressBarList)d;
        control.OnDataChanged((IEnumerable<ProgressBarData>?)e.OldValue, (IEnumerable<ProgressBarData>?)e.NewValue);
    }

    private void OnDataChanged(IEnumerable<ProgressBarData>? oldValue, IEnumerable<ProgressBarData>? newValue)
    {
        lock (_dataLock)
        {
            if (oldValue is INotifyCollectionChanged oldNotifying)
            {
                oldNotifying.CollectionChanged -= Data_CollectionChanged;
            }

            _dataDict.Clear();

            if (newValue != null)
            {
                foreach (var item in newValue)
                {
                    if (item == null || item.ID < 0) continue;
                    _dataDict[item.ID] = item;
                }

                if (newValue is INotifyCollectionChanged newNotifying)
                {
                    newNotifying.CollectionChanged += Data_CollectionChanged;
                }
            }

            UpdateAnimation();
        }
    }

    private void Data_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        lock (_dataLock)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _dataDict.Clear();
            }

            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems.Cast<ProgressBarData>())
                {
                    if (obj == null) continue;
                    _dataDict.Remove(obj.ID);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var obj in e.NewItems.Cast<ProgressBarData>())
                {
                    if (obj == null || obj.ID < 0) continue;
                    _dataDict[obj.ID] = obj;
                }
            }

            // For Move/Replace you can refine behavior if necessary.
            UpdateAnimation();
        }
    }
}