using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StarResonanceDpsAnalysis.WPF.Form
{
    /// <summary>
    /// DpsStatisticsForm.xaml 的交互逻辑
    /// </summary>
    public partial class DpsStatisticsForm : Window
    {
       
        public DpsStatisticsForm()
        {
            InitializeComponent();


        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("测试");
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("测试");
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

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PullButton_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
