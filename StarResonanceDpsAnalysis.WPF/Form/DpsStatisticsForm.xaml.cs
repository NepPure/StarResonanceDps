using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            // 捕获整个窗口上的左键按下（包括已处理的事件）
            AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler((s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed) DragMove();
            }), /*handledEventsToo:*/ true);
        }
    }
}
