using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StarResonanceDpsAnalysis.WPF.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace StarResonanceDpsAnalysis.WPF.Views
{
    /// <summary>
    /// DpsStatisticsForm.xaml 的交互逻辑
    /// </summary>
    public partial class DpsStatisticsView : Window
    {
        private double _beforePilingHeight = 0;

        public static readonly DependencyProperty MinimizeProperty =
            DependencyProperty.Register(
                nameof(Minimize),
                typeof(bool),
                typeof(DpsStatisticsView),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool Minimize
        {
            get { return (bool)GetValue(MinimizeProperty); }
            set { SetValue(MinimizeProperty, value); }
        }


        public DpsStatisticsView()
        {
            InitializeComponent();

            InitDemoProgressBars();

        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private int metricIndex = 0; // 0: 伤害, 1: 治疗, 2: 承伤
        private bool isFull = false; // 是否全程显示

        private static readonly string[] names = ["伤害", "治疗", "承伤"];

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            metricIndex = (metricIndex - 1 + 3) % 3;
            UpdateLabel();
        }


        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            metricIndex = ++metricIndex % 3;
            UpdateLabel();
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            isFull = !isFull;
            UpdateLabel();
        }
        private void UpdateLabel()
        {
            StatisticTypeLabel.Text = (isFull ? "全程" : "当前") + names[metricIndex];
        }


        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var cm = btn.ContextMenu;
            if (cm == null) return;

            // 保证 DataContext（如果菜单里有绑定）
            cm.DataContext = btn.DataContext;

            // 控制弹出位置
            cm.PlacementTarget = btn;
            cm.Placement = PlacementMode.Right; // 或 MousePoint / Right / Left / Top
            cm.HorizontalOffset = 0;
            cm.VerticalOffset = 0;

            // 打开（可切换）
            cm.IsOpen = !cm.IsOpen;
        }



        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {


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
            var heightDA = new DoubleAnimation
            {
                From = ActualHeight,
                To = Minimize ? 25 : _beforePilingHeight,
                Duration = duration,
                EasingFunction = easingFunction,
            };
            Storyboard.SetTarget(heightDA, this);
            Storyboard.SetTargetProperty(heightDA, new PropertyPath(HeightProperty));
            sb.Children.Add(heightDA);

            var pullButtonTransformDA = new DoubleAnimation
            {
                To = Minimize ? 180 : 0,
                Duration = duration,
                EasingFunction = easingFunction
            };
            Storyboard.SetTarget(pullButtonTransformDA, PullButton);
            Storyboard.SetTargetProperty(pullButtonTransformDA, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));
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

            if (me.IsChecked == true)
            {
                // 这次点击后变成 true：把其它都关掉
                foreach (var obj in owner.Items)
                {
                    if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                        mi.IsChecked = false;
                }
                // me 已经是 true，不用再设
            }
            else
            {
                // 这次点击后变成 false：允许“全不选”，什么也不做
            }

            e.Handled = true;
        }
        /// <summary>
        /// 设置选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var me = (MenuItem)sender;
            var owner = ItemsControl.ItemsControlFromItemContainer(me);
            foreach (var obj in owner.Items)
            {
                if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                    mi.IsChecked = false;
            }
            me.IsChecked = true; // 保证当前为选中
            e.Handled = true;
        }

        /// <summary>
        /// 测伤模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AxisMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var me = (MenuItem)sender;
            var owner = ItemsControl.ItemsControlFromItemContainer(me);

            if (me.IsChecked == true)
            {
                // 这次点击后变成 true：把其它都关掉
                foreach (var obj in owner.Items)
                {
                    if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                        mi.IsChecked = false;
                }
                // me 已经是 true，不用再设
            }
            else
            {
                // 这次点击后变成 false：允许“全不选”，什么也不做
            }

            e.Handled = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshButton_MouseEnter(object sender, MouseEventArgs e)
        {
            // 模拟技能数据
            var skills = new List<SkillItem>
            {
                new SkillItem { SkillName = "技能A", TotalDamage = "939.1万", HitCount = 4, CritCount = 121, AvgDamage = 121 },
                new SkillItem { SkillName = "技能B", TotalDamage = "88.6万", HitCount = 8, CritCount = 23,  AvgDamage = 11 },
                new SkillItem { SkillName = "技能C", TotalDamage = "123.4万", HitCount = 3, CritCount = 45,  AvgDamage = 233 },
            };

            SkillList.ItemsSource = skills;
            SkillPopup.IsOpen = true;
        }

        private void RefreshButton_MouseLeave(object sender, MouseEventArgs e)
        {

            SkillList.ItemsSource = null;
            SkillPopup.IsOpen = false;
        }
    }
}
