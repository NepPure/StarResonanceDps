using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using StarResonanceDpsAnalysis.WPF.Controls;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Views;

/// <summary>
///     DpsStatisticsForm.xaml 的交互逻辑
/// </summary>
public partial class DpsStatisticsView : Window
{
    public static readonly DependencyProperty MinimizeProperty =
        DependencyProperty.Register(
            nameof(Minimize),
            typeof(bool),
            typeof(DpsStatisticsView),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    private double _beforePilingHeight;

    public DpsStatisticsView(DpsStatisticsViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    public bool Minimize
    {
        get => (bool)GetValue(MinimizeProperty);
        set => SetValue(MinimizeProperty, value);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void PullButton_Click(object sender, RoutedEventArgs e)
    {
        Minimize = !Minimize;

        if (Minimize)
        {
            _beforePilingHeight = ActualHeight;
        }

        var sb = new Storyboard { FillBehavior = FillBehavior.HoldEnd };
        var duration = new Duration(TimeSpan.FromMilliseconds(300));
        var easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
        var animationHeight = new DoubleAnimation
        {
            From = ActualHeight,
            To = Minimize ? 25 : _beforePilingHeight,
            Duration = duration,
            EasingFunction = easingFunction
        };
        Storyboard.SetTarget(animationHeight, this);
        Storyboard.SetTargetProperty(animationHeight, new PropertyPath(HeightProperty));
        sb.Children.Add(animationHeight);

        var pullButtonTransformDA = new DoubleAnimation
        {
            To = Minimize ? 180 : 0,
            Duration = duration,
            EasingFunction = easingFunction
        };
        Storyboard.SetTarget(pullButtonTransformDA, PullButton);
        Storyboard.SetTargetProperty(pullButtonTransformDA,
            new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));
        sb.Children.Add(pullButtonTransformDA);

        sb.Begin();
    }

    /// <summary>
    /// 打桩模式选择
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PilingMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var me = (MenuItem)sender;
        var owner = ItemsControl.ItemsControlFromItemContainer(me);

        if (me.IsChecked)
        {
            // 这次点击后变成 true：把其它都关掉
            foreach (var obj in owner.Items)
            {
                if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                    mi.IsChecked = false;
            }
            // me 已经是 true，不用再设
        }

        // 这次点击后变成 false：允许“全不选”，什么也不做
        e.Handled = true;
    }

    /// <summary>
    ///     测伤模式
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AxisMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var me = (MenuItem)sender;
        var owner = ItemsControl.ItemsControlFromItemContainer(me);

        if (me.IsChecked)
        {
            // 这次点击后变成 true：把其它都关掉
            foreach (var obj in owner.Items)
            {
                if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                    mi.IsChecked = false;
            }
            // me 已经是 true，不用再设
        }

        // 这次点击后变成 false：允许“全不选”，什么也不做
        e.Handled = true;
    }
}