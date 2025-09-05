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

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    /// <summary>
    /// CustomizeProgressBar.xaml 的交互逻辑
    /// </summary>
    public partial class CustomizeProgressBar : UserControl
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(CustomizeProgressBar),
                new FrameworkPropertyMetadata(new CornerRadius(3), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarBackgroundProperty =
            DependencyProperty.Register(
                nameof(ProgressBarBackground),
                typeof(Brush),
                typeof(CustomizeProgressBar),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x56, 0x9C, 0xD6)), FrameworkPropertyMetadataOptions.None));
        public Brush ProgressBarBackground
        {
            get { return (Brush)GetValue(ProgressBarBackgroundProperty); }
            set { SetValue(ProgressBarBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(CustomizeProgressBar),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);

                UpdateProgressBarAnimation();
            }
        }

        public static readonly DependencyProperty SlotProperty =
            DependencyProperty.Register(
                nameof(Slot),
                typeof(object),
                typeof(CustomizeProgressBar),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public object Slot
        {
            get { return GetValue(SlotProperty); }
            set { SetValue(SlotProperty, value); }
        }

        public static readonly DependencyProperty SlotDataTemplateProperty =
            DependencyProperty.Register(
                nameof(SlotDataTemplate),
                typeof(DataTemplate),
                typeof(CustomizeProgressBar),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public DataTemplate SlotDataTemplate
        {
            get { return (DataTemplate)GetValue(SlotDataTemplateProperty); }
            set { SetValue(SlotDataTemplateProperty, value); }
        }

        public CustomizeProgressBar()
        {
            InitializeComponent();
        }
    }
}
