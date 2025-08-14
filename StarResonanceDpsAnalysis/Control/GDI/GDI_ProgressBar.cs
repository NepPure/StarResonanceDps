using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace StarResonanceDpsAnalysis.Control.GDI
{
    public class GDI_ProgressBar : IDisposable
    {
        private static readonly StringFormat _strictStringFormat = StringFormat.GenericTypographic;
        private readonly object _lock = new();
        private Color? _prevProgressBarColor = null;
        private Brush? _progressBarBrush = null;
        private Color? _prevForeColor = null;
        private Brush? _progressBarTextBrush = null;
        private string? _prevText = null;
        private Font? _prevFont = null;
        private SizeF? _textSize = null;

        public void Draw(Graphics g, DrawInfo info)
        {
            /* 这里的 Graphics 不要使用 using, 也不要 Dispose, 
             * 因为双重缓冲机制, 在我们自己的绘制结束后, 系统会继续使用这个 Graphics 进行收尾工作,
             * 如果我们在这里 Dispose, 会导致报错: System.ArgumentException:“Parameter is not valid.”
             * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

            lock (_lock)
            {
                if (_progressBarBrush == null || info.ProgressBarColor != _prevProgressBarColor)
                {
                    _prevProgressBarColor = info.ProgressBarColor;
                    _progressBarBrush?.Dispose();
                    _progressBarBrush = new SolidBrush(info.ProgressBarColor);
                }
                if (_progressBarTextBrush == null || info.ForeColor != _prevForeColor)
                {
                    _prevForeColor = info.ForeColor;
                    _progressBarTextBrush?.Dispose();
                    _progressBarTextBrush = new SolidBrush(info.ForeColor);
                }
                if (_textSize == null || info.Text != _prevText || info.Font != _prevFont)
                {
                    _prevText = info.Text;
                    _prevFont = info.Font;
                    _textSize = TextRenderer.MeasureText(info.Text, info.Font);
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;

                var barWidth = (info.Width - info.Padding.Left - info.Padding.Right) * info.ProgressBarValue;
                var barHeight = info.Height - info.Padding.Top - info.Padding.Bottom;
                if (barWidth >= 1)
                {
                    var diameter = Math.Max(1, Math.Min(info.ProgressBarCornerRadius * 2, Math.Min(barWidth, barHeight)));
                    var rect = new RectangleF(0, 0, (float)diameter, (float)diameter);

                    using var path = new GraphicsPath();
                    // 左上角
                    rect.X = info.Padding.Left;
                    rect.Y = info.Top + info.Padding.Top;
                    path.AddArc(rect, 180, 90);
                    // 右上角
                    rect.X = (float)(info.Padding.Left + (barWidth - diameter));
                    path.AddArc(rect, 270, 90);
                    // 右下角
                    rect.Y = (float)(info.Top + info.Height - info.Padding.Bottom - diameter);
                    path.AddArc(rect, 0, 90);
                    // 左下角
                    rect.X = info.Padding.Left;
                    path.AddArc(rect, 90, 90);
                    // 闭合图形
                    path.CloseFigure();
                    g.FillPath(_progressBarBrush, path);

                }

                if (info.Text.Length != 0)
                {
                    var textLeft = info.Padding.Left + info.TextPadding.Left;
                    var textTop = info.Top + (info.Height - _textSize.Value.Height) / 2f + (info.Padding.Top + info.TextPadding.Top - (info.Padding.Bottom + info.TextPadding.Bottom)) / 2f;

                    g.DrawString(info.Text, info.Font, _progressBarTextBrush, new PointF(textLeft, (float)textTop), _strictStringFormat);
                }

            }
        }

        public void Dispose()
        {
            _strictStringFormat.Dispose();
            _progressBarBrush?.Dispose();
            _progressBarTextBrush?.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public class DrawInfo
    {
        public float Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte Opacity { get; set; }
        public Padding Padding { get; set; }
        public Color BackColor { get; set; }
        public double ProgressBarValue { get; set; }
        public Color ProgressBarColor { get; set; }
        public int ProgressBarCornerRadius { get; set; }
        public string Text { get; set; } = string.Empty;
        public Color ForeColor { get; set; }
        public Font Font { get; set; } = SystemFonts.DefaultFont;
        public Padding TextPadding { get; set; }
    }
}
