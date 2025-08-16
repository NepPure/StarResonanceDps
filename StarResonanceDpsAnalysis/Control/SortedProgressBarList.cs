using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StarResonanceDpsAnalysis.Control.GDI;
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Effects.Enum;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SortedProgressBarList : UserControl
    {
        public delegate void SelectionChanedEventHandler(SortedProgressBarList sender, int index, ProgressBarData? data);
        public event SelectionChanedEventHandler? SelectionChanged;

        private readonly Dictionary<long, ProgressBarData> _dataDict = [];
        private int _animationDuration = 300;
        private Quality _animationQuality = Quality.Medium;
        private int _progressBarHeight = 20;
        private Padding _progressBarPadding = new(3, 3, 3, 3);
        private RenderContent.ContentAlign _orderAlign = RenderContent.ContentAlign.MiddleLeft;
        private RenderContent.ContentOffset _orderOffset = new() { X = 0, Y = 0 };
        private Func<int, string>? _orderCallback = null;
        private Color _orderColor = Color.Black;
        private Font _orderFont = SystemFonts.DefaultFont;
        private int? _selectedIndex = null;
        private Color _seletedItemColor = Color.FromArgb(0x56, 0x9C, 0xD6);

        /// <summary>
        /// 数据源
        /// </summary>
        public List<ProgressBarData>? Data
        {
            get => _dataDict.Values.ToList();
            set
            {
                lock (_lock)
                {
                    if (value == null || value.Count == 0)
                    {
                        _dataDict.Clear();
                        _animatingInfoBuffer.Clear(); // 同步清空动画缓存，避免残影
                        _selectedIndex = null;        // 清空选择状态
                        SelectionChanged?.Invoke(this, -1, null);
                        Invalidate();
                        return;
                    }

                    // 预先构建目标 ID 集，避免 O(n^2)
                    var targetIds = value
                        .Where(item => item != null && item.ID >= 0)
                        .Select(item => item.ID)
                        .ToHashSet();

                    // 移除不存在的项
                    foreach (var key in _dataDict.Keys.ToList())
                    {
                        if (!targetIds.Contains(key))
                            _dataDict.Remove(key);
                    }

                    // 更新或新增项（后者覆盖同 ID）
                    foreach (var item in value)
                    {
                        if (item == null || item.ID < 0) continue;
                        _dataDict[item.ID] = item;
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 排序动画的持续时间
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("排序动画的持续时间")]
        [DefaultValue(300)]
        public int AnimationDuration
        {
            get => _animationDuration;
            set => _animationDuration = value;
        }

        /// <summary>
        /// 动画质量
        /// </summary>
        /// <remarks>
        /// Quality.VeryLow        =  10FPS; 贝塞尔精确段数 = 5
        /// Quality.Low            =  20FPS; 贝塞尔精确段数 = 7
        /// Quality.Medium         =  30FPS; 贝塞尔精确段数 = 13
        /// Quality.High           =  60FPS; 贝塞尔精确段数 = 25
        /// Quality.VeryHigh       = 120FPS; 贝塞尔精确段数 = 49
        /// Quality.Extreme        = 160FPS; 贝塞尔精确段数 = 499
        /// Quality.AlmostAccurate = 999FPS; 贝塞尔精确段数 = 2499
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description(@"""
            动画质量
            Quality.VeryLow        =  10FPS; 贝塞尔精确段数 = 5
            Quality.Low            =  20FPS; 贝塞尔精确段数 = 7
            Quality.Medium         =  30FPS; 贝塞尔精确段数 = 13
            Quality.High           =  60FPS; 贝塞尔精确段数 = 25
            Quality.VeryHigh       = 120FPS; 贝塞尔精确段数 = 49
            Quality.Extreme        = 160FPS; 贝塞尔精确段数 = 499
            Quality.AlmostAccurate = 999FPS; 贝塞尔精确段数 = 2499
            """)]
        [DefaultValue(Quality.Low)]
        public Quality AnimationQuality
        {
            get => _animationQuality;
            set
            {
                if (_animationQuality != value)
                {
                    var flag = _animationFpsQuality.TryGetValue(value, out var fps);
                    if (!flag)
                    {
                        throw new ArgumentException($"Invalid animation quality: {value}");
                    }
                    _animationPeriodicTimer.Period = TimeSpan.FromMilliseconds(1000d / fps);
                    _animationQuality = value;
                }
            }
        }

        /// <summary>
        /// 进度条高度
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条高度")]
        [DefaultValue(20)]
        public int ProgressBarHeight
        {
            get => _progressBarHeight;
            set => _progressBarHeight = value;
        }

        /// <summary>
        /// 进度条内边距
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条内边距")]
        [DefaultValue(typeof(Padding), "3,3,3,3")]
        public Padding ProgressBarPadding
        {
            get => _progressBarPadding;
            set => _progressBarPadding = value;
        }

        /// <summary>
        /// 排序序号对齐模式
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("排序序号对齐模式")]
        [DefaultValue(RenderContent.ContentAlign.MiddleLeft)]
        public RenderContent.ContentAlign OrderAlign
        {
            get => _orderAlign;
            set => _orderAlign = value;
        }

        /// <summary>
        /// 排序序号偏移量
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("排序序号偏移量")]
        public RenderContent.ContentOffset OrderOffset
        {
            get => _orderOffset;
            set => _orderOffset = value;
        }

        /// <summary>
        /// 序号文字重排回调
        /// </summary>
        /// <remarks>
        /// 会传递给函数一个从 1 开始的 int 序号, 
        /// 将其转为所需类型的 string 后返回即可;
        /// 如果函数为 null, 则不会显示序号
        /// </remarks>
        public Func<int, string>? OrderCallback
        {
            private get => _orderCallback;
            set => _orderCallback = value;
        }

        /// <summary>
        /// 序号文字颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("序号文字颜色")]
        public Color OrderColor
        {
            get => _orderColor;
            set => _orderColor = value;
        }

        /// <summary>
        /// 序号文字字体
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("序号文字字体")]
        public Font OrderFont
        {
            get => _orderFont;
            set => _orderFont = value;
        }

        /// <summary>
        /// 已选择的进度条项目索引
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("已选择的进度条项目索引")]
        [DefaultValue(null)]
        public int? SelectedIndex
        {
            get => _selectedIndex;
            set => _selectedIndex = value;
        }

        /// <summary>
        /// 已选择的进度条项目外框颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("已选择的进度条项目外框颜色")]
        public Color SeletedItemColor
        {
            get => _seletedItemColor;
            set => _seletedItemColor = value;
        }

        public SortedProgressBarList()
        {
            InitializeComponent();

            _animationPeriodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000d / _animationFpsQuality[Quality.Medium]));
            _moveAnimationCubicBezier = new CubicBezier(0.65f, 0f, 0.35f, 1f, AnimationQuality);
            _fadeAnimationCubicBezier = new CubicBezier(0.3f, 0.45f, 0.25f, 1f, AnimationQuality);

            InitAnimation();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Redraw(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            _animationCancellation?.Cancel();
            _animationPeriodicTimer?.Dispose();
        }

        private void SortedProgressBarList_MouseClick(object sender, MouseEventArgs e)
        {
            // 边界判断
            if (e.Location.X < Padding.Left || e.Location.Y < Padding.Top ||
                e.Location.X > Width - Padding.Right || e.Location.Y > Height - Padding.Bottom)
            {
                _selectedIndex = null;
                SelectionChanged?.Invoke(this, -1, null);
                return;
            }

            var index = e.Location.Y / ProgressBarHeight;
            if (index < 0 || index >= _animatingInfoBuffer.Count)
            {
                _selectedIndex = null;
                SelectionChanged?.Invoke(this, -1, null);
                return;
            }

            var progressBar = _animatingInfoBuffer[index].Data;
            var offsetY = e.Location.Y % ProgressBarHeight;

            if (e.Location.X < Padding.Left + progressBar.ProgressBarPadding.Left ||
                e.Location.X > Width - Padding.Right - progressBar.ProgressBarPadding.Right ||
                offsetY < progressBar.ProgressBarPadding.Top ||
                offsetY > ProgressBarHeight - progressBar.ProgressBarPadding.Bottom)
            {
                _selectedIndex = null;
                SelectionChanged?.Invoke(this, -1, null);
                return;
            }

            _selectedIndex = index;
            SelectionChanged?.Invoke(this, index, progressBar);
        }
    }
}
