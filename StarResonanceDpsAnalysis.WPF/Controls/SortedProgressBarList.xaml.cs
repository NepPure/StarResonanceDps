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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    /// <summary>
    /// SortedProgressBarList.xaml 的交互逻辑
    /// </summary>
    public partial class SortedProgressBarList : UserControl
    {
        private readonly Dictionary<long, ProgressBarData> _dataDict = [];

        public static readonly DependencyProperty ProgressBarSlotDataTemplateProperty =
            DependencyProperty.Register(
                nameof(ProgressBarSlotDataTemplate),
                typeof(DataTemplate),
                typeof(SortedProgressBarList),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public DataTemplate ProgressBarSlotDataTemplate
        {
            get { return (DataTemplate)GetValue(ProgressBarSlotDataTemplateProperty); }
            set { SetValue(ProgressBarSlotDataTemplateProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarHeightProperty =
            DependencyProperty.Register(
                nameof(ProgressBarHeight),
                typeof(double),
                typeof(SortedProgressBarList),
                new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.None));
        public double ProgressBarHeight
        {
            get { return (double)GetValue(ProgressBarHeightProperty); }
            set { SetValue(ProgressBarHeightProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarMarginProperty =
            DependencyProperty.Register(
                nameof(ProgressBarMargin),
                typeof(Thickness),
                typeof(SortedProgressBarList),
                new FrameworkPropertyMetadata(new Thickness(3), FrameworkPropertyMetadataOptions.None));
        public Thickness ProgressBarMargin
        {
            get { return (Thickness)GetValue(ProgressBarMarginProperty); }
            set { SetValue(ProgressBarMarginProperty, value); }
        }

        /// <summary>
        /// 数据源
        /// </summary>
        public List<ProgressBarData>? Data
        {
            get => [.. _dataDict.Values];
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

        public SortedProgressBarList()
        {
            InitializeComponent();
        }
    }
}
