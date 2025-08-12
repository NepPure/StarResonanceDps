using System.ComponentModel;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class TextProgressBar : UserControl
    {
        private bool _autoTextColor = true;
        private double _progressBarValue = 0.0d;
        private Color _progressBarColor = Color.FromArgb(0x56, 0x9C, 0xD6);
        private Color _foreColor = Color.Black;
        private Color? _autoForeColor = null;
        private int _progressBarCornerRadius = 3;
        private string _text = string.Empty;
        private Padding _textPadding = new();

        /// <summary>
        /// 自动设置进度条上方文字颜色
        /// </summary>
        /// <remarks>
        /// 启用后, 文字颜色默认设置为进度条颜色的反色; 为防止靠色, 灰色背景下会根据亮度仅调整为白色或黑色
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description("自动设置进度条上方文字颜色\r\n启用后, 文字颜色默认设置为进度条颜色的反色; 为防止靠色, 灰色背景下会根据亮度仅调整为白色或黑色")]
        [DefaultValue(true)]
        public bool AutoTextColor
        {
            get => _autoTextColor;
            set
            {
                if (_autoTextColor != value)
                {
                    _autoTextColor = value;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 进度条进度
        /// </summary>
        /// <remarks>
        /// 0.0d ~ 1.0d, 会自动限制在这个范围内
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条进度\r\n0.0d ~ 1.0d, 会自动限制在这个范围内")]
        [DefaultValue(1.0d)]
        public double ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                if (_progressBarValue != value)
                {
                    _progressBarValue = Math.Max(0.0d, Math.Min(1.0d, value));

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 进度条颜色
        /// </summary>
        /// <remarks>
        /// 默认色是从 VS public 关键字上复制的...
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条颜色")]
        public Color ProgressBarColor
        {
            get => _progressBarColor;
            set
            {
                if (_progressBarColor != value)
                {
                    // 重置 _autoForeColor, 下次 get 重新计算
                    _autoForeColor = null;
                    // 重置 ProgressBarBrush, 下次 Paint 重新创建
                    ProgressBarBrush?.Dispose();
                    ProgressBarBrush = null;

                    _progressBarColor = value;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 进度条文字颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条文字颜色")]
        public override Color ForeColor
        {
            get
            {
                // 如果不允许自动设置文字颜色, 则直接返回基类 ForeColor
                if (!AutoTextColor) return _foreColor;

                if (_autoForeColor == null)
                {
                    var max = Math.Max(ProgressBarColor.R, Math.Max(ProgressBarColor.G, ProgressBarColor.B));
                    var min = Math.Min(ProgressBarColor.R, Math.Min(ProgressBarColor.G, ProgressBarColor.B));
                    var delta = max - min;

                    // 饱和度过低 (发灰 -> 反色依然为灰 = 撞色)
                    if (delta < 20)
                    {
                        _autoForeColor = max + min / 2d > 127 ? Color.Black : Color.White;
                    }

                    _autoForeColor = Color.FromArgb(
                        255 - ProgressBarColor.R,
                        255 - ProgressBarColor.G,
                        255 - ProgressBarColor.B
                    );
                }

                return _autoForeColor.Value;
            }
            set
            {
                if (_foreColor != value)
                {
                    _foreColor = value;

                    // 只有不使用自动文本颜色的时候, 才要求重绘
                    if (!AutoTextColor)
                    {
                        // 重置 ProgressBarTextBrush, 下次 Paint 重新创建
                        ProgressBarTextBrush?.Dispose();
                        ProgressBarTextBrush = null;

                        Invalidate();
                    }
                }

            }
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("圆角半径")]
        [DefaultValue(1.0)]
        public int ProgressBarCornerRadius
        {
            get => _progressBarCornerRadius;
            set
            {
                // 此处不进行多余合法性判断, 绘制时会根据情况自适应
                if (_progressBarCornerRadius != value)
                {
                    _progressBarCornerRadius = value;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// 与控件关联的文本
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("与控件关联的文本")]
        [DefaultValue("")]
        public new string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 文字距离 ProgressBar 的距离
        /// </summary>
        /// <remarks>
        /// 右间距无效, 文字过长时会无视右边距设定超长(懒得修...)
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description("文字距离 ProgressBar 的距离\r\n右间距无效, 文字过长时会无视右边距设定超长")]
        [DefaultValue(typeof(Padding), "0,0,0,0")]
        public Padding TextPadding
        {
            get => _textPadding;
            set
            {
                if (_textPadding != value)
                {
                    _textPadding = value;

                    Invalidate();
                }
            }
        }

        public TextProgressBar()
        {
            InitializeComponent();
        }

        public void TextProgressBar_Paint(object sender, PaintEventArgs e)
        {
            DrawTextProgressBarControl(e);
        }

        public void TextProgressBar_Load(object sender, EventArgs e)
        {

        }
    }
}
