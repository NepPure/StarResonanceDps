using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Effects.Enum;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SortedProgressBarList : UserControl
    {
        private readonly Dictionary<int, ProgressBarData> _dataDict = [];
        private int _animationDuration = 300;
        private Quality _animationQuality = Quality.Medium;
        private int _progressBarHeight = 20;
        private Padding _progressBarPadding = new(3, 3, 3, 3);

        public List<ProgressBarData>? Data
        {
            get => [.. _dataDict.Select(e => e.Value)];
            set
            {
                // 清空列表
                if (value == null || value.Count == 0)
                {
                    _dataDict.Clear();
                    return;
                }
                // 现有表中多出来的数据移除
                foreach (var item in _dataDict)
                {
                    if (value.Any(e => e.ID == item.Key)) continue;
                    _dataDict.Remove(item.Key);
                }
                // 将新数据更新或添加到表中
                foreach (var item in value)
                {
                    if (item == null || item.ID < 0) continue;
                    if (!_dataDict.TryAdd(item.ID, item))
                    {
                        _dataDict[item.ID] = item;
                    }
                }
            }
        }

        public new int Height
        {
            get => base.Height;
        }

        public int AnimationDuration
        {
            get => _animationDuration;
            set => _animationDuration = value;
        }

        public Quality AnimationQuality
        {
            get => _animationQuality;
            set
            {
                if (_animationQuality != value)
                {
                    var flag = _animationDelayQuality.TryGetValue(value, out var delay);
                    if (!flag)
                    {
                        throw new ArgumentException($"Invalid animation quality: {value}");
                    }
                    _animationDelay = delay;
                    _animationQuality = value;
                }
            }
        }

        public int ProgressBarHeight
        {
            get => _progressBarHeight;
            set => _progressBarHeight = value;
        }
        public Padding ProgressBarPadding
        {
            get => _progressBarPadding;
            set => _progressBarPadding = value;
        }

        public SortedProgressBarList()
        {
            InitializeComponent();

            _moveAnimationCubicBezier = new CubicBezier(0.65f, 0f, 0.35f, 1f, AnimationQuality);
            _fadeAnimationCubicBezier = new CubicBezier(0.3f, 0.45f, 0.25f, 1f, AnimationQuality);

            InitAnimation();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Redraw(e);
        }
    }

    public class ProgressBarData
    {
        public int ID { get; set; }
        public bool AutoTextColor { get; set; } = false;
        public double ProgressBarValue { get; set; }
        public Color ProgressBarColor { get; set; } = Color.FromArgb(0x56, 0x9C, 0xD6);
        public Color ForeColor { get; set; } = Color.Black;
        public int ProgressBarCornerRadius { get; set; }
        public string? Text { get; set; }
        public Padding ProgressBarPadding { get; set; } = new(3, 3, 3, 3);
        public Padding TextPadding { get; set; } = new(3, 3, 3, 3);
    }
}
