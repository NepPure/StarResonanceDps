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
using System.Windows.Navigation;
using System.Windows.Shapes;

using StarResonanceDpsAnalysis.Assets;

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    /// <summary>
    /// ControlBox.xaml 的交互逻辑
    /// </summary>
    public partial class ControlBox : UserControl
    {
        public static readonly DependencyProperty DefaultButtonWidth =
            DependencyProperty.Register(
                nameof(BUTTON_WIDTH),
                typeof(double),
                typeof(ControlBox),
                new FrameworkPropertyMetadata(BUTTON_WIDTH, FrameworkPropertyMetadataOptions.None));
        private const double BUTTON_WIDTH = 50;

        public static readonly DependencyProperty UseMinimizeButtonProperty =
            DependencyProperty.Register(
                nameof(UseMinimizeButton),
                typeof(bool),
                typeof(ControlBox),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public bool UseMinimizeButton
        {
            get { return (bool)GetValue(UseMinimizeButtonProperty); }
            set { SetValue(UseMinimizeButtonProperty, value); }
        }

        public static readonly DependencyProperty UseMaximizeButtonProperty =
            DependencyProperty.Register(
                nameof(UseMaximizeButton),
                typeof(bool),
                typeof(ControlBox),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public bool UseMaximizeButton
        {
            get { return (bool)GetValue(UseMaximizeButtonProperty); }
            set { SetValue(UseMaximizeButtonProperty, value); }
        }

        public ControlBox()
        {
            InitializeComponent();

            DisableWidthHeight();
        }

        private void DisableWidthHeight()
        {
            WidthProperty.OverrideMetadata(typeof(ControlBox),
                new FrameworkPropertyMetadata(double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    (_, _) => BUTTON_WIDTH * (1 + (UseMinimizeButton ? 1 : 0) + (UseMaximizeButton ? 1 : 0))));

            HeightProperty.OverrideMetadata(typeof(ControlBox),
                new FrameworkPropertyMetadata(double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    (_, _) => double.NaN));
        }
    }
}
