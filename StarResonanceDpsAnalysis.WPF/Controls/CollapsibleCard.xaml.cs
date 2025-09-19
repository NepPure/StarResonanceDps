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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    /// <summary>
    /// CollapsibleCard.xaml 的交互逻辑
    /// </summary>
    [ContentProperty(nameof(CardContent))]
    public partial class CollapsibleCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(CollapsibleCard),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(CollapsibleCard),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(
                nameof(CardContent),
                typeof(object),
                typeof(CollapsibleCard),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool IsExpanded 
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        public CollapsibleCard()
        {
            InitializeComponent();
        }
    }
}
