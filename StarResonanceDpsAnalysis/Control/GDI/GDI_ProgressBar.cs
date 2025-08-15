using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Control.GDI
{
    public class GDI_ProgressBar : IDisposable
    {
        private static readonly StringFormat _strictStringFormat = StringFormat.GenericTypographic;
        private static readonly TextFormatFlags _textFormatFlags =
            TextFormatFlags.NoPadding
            | TextFormatFlags.SingleLine
            | TextFormatFlags.EndEllipsis;

        private readonly object _lock = new();
        private Color? _prevProgressBarColor = null;
        private Brush? _progressBarBrush = null;

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

                if (info.ContentList != null)
                {
                    foreach (var item in info.ContentList)
                    {
                        if (item.Type == RenderContent.ContentType.Text)
                        {
                            RenderText(g, info, item);
                        }
                        else if (item.Type == RenderContent.ContentType.Image)
                        {
                            RenderImage(g, info, item);
                        }
                    }
                }

            }
        }

        private static void RenderText(Graphics g, DrawInfo info, RenderContent content)
        {
            var textSize = TextRenderer.MeasureText(content.Text, content.Font);

            var (left, top) = GetContentPostion(info, content, textSize);

            TextRenderer.DrawText(g, content.Text, content.Font, new Point(left, top), content.ForeColor, _textFormatFlags);
        }

        private void RenderImage(Graphics g, DrawInfo info, RenderContent content)
        {
            var (left, top) = GetContentPostion(info, content, content.ImageRenderSize);

            g.DrawImage(content.Image!,new Rectangle(left, top, content.ImageRenderSize.Width, content.ImageRenderSize.Height));
        }

        private static (int left, int top) GetContentPostion(DrawInfo info, RenderContent content, Size contentSize)
        {
            var left = 0;
            var top = 0;

            if (((int)content.Align & (int)RenderContent.Direction.Left) > 0)
            {
                left = info.Padding.Left + content.Offset.X;
            }
            else if (((int)content.Align & (int)RenderContent.Direction.Center) > 0)
            {
                left = info.Padding.Left + (info.Width - info.Padding.Left - info.Padding.Right - contentSize.Width) / 2 + content.Offset.X;
            }
            else if (((int)content.Align & (int)RenderContent.Direction.Right) > 0)
            {
                left = info.Width - info.Padding.Right - contentSize.Width + content.Offset.X;
            }

            if (((int)content.Align & (int)RenderContent.Direction.Top) > 0)
            {
                top = (int)(info.Top + info.Padding.Top + content.Offset.Y);
            }
            else if (((int)content.Align & (int)RenderContent.Direction.Middle) > 0)
            {
                top = (int)(info.Top + info.Padding.Top + (info.Height - info.Padding.Top - info.Padding.Bottom - contentSize.Height) / 2 + content.Offset.Y);
            }
            else if (((int)content.Align & (int)RenderContent.Direction.Bottom) > 0)
            {
                top = (int)(info.Top + info.Height - info.Padding.Bottom - contentSize.Height + content.Offset.Y);
            }

            return (left, top);
        }

        public void Dispose()
        {
            _strictStringFormat.Dispose();
            _progressBarBrush?.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public class DrawInfo
    {
        public float Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Padding Padding { get; set; }
        public Color BackColor { get; set; }
        public double ProgressBarValue { get; set; }
        public Color ProgressBarColor { get; set; }
        public int ProgressBarCornerRadius { get; set; }
        public IEnumerable<RenderContent>? ContentList { get; set; }
    }

    public class RenderContent
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        public ContentType Type { get; set; } = ContentType.Text;
        /// <summary>
        /// 内容对齐方式
        /// </summary>
        public ContentAlign Align { get; set; } = ContentAlign.MiddleLeft;
        /// <summary>
        /// 偏移量, 相对于 Align 后的位置进行偏移
        /// </summary>
        /// <remarks>
        /// 无论 Align 如何设置, Offset 始终 - 为左, + 为右
        /// </remarks>
        public ContentOffset Offset { get; set; } = new ContentOffset { X = 0, Y = 0 };

        /// <summary>
        /// 文本内容
        /// </summary>
        /// <remarks>
        /// Type 为 ContentType.Text 时有效
        /// </remarks>
        public string? Text { get; set; }
        /// <summary>
        /// 文本颜色
        /// </summary>
        /// <remarks>
        /// AutoTextColor 为 true 时, 此属性无效
        /// </remarks>
        public Color ForeColor { get; set; } = Color.Black;
        /// <summary>
        /// 文本字体
        /// </summary>
        public Font Font { get; set; } = SystemFonts.DefaultFont;

        /// <summary>
        /// 图片内容
        /// </summary>
        /// <remarks>
        /// Type 为 ContentType.Image 时有效
        /// </remarks>
        public Image? Image { get; set; }
        /// <summary>
        /// 将要绘制的大小
        /// </summary>
        public Size ImageRenderSize { get; set; } = new Size(0, 0);


        public enum ContentType
        {
            Text = 0,
            Image = 1,
        }
        public enum Direction
        {
            Left = 1,
            Center = 2,
            Right = 4,

            Top = 8,
            Middle = 16,
            Bottom = 32,
        }
        public enum ContentAlign
        {
            TopLeft = Direction.Left | Direction.Top,
            TopCenter = Direction.Center | Direction.Top,
            TopRight = Direction.Right | Direction.Top,
            MiddleLeft = Direction.Left | Direction.Middle,
            MiddleCenter = Direction.Center | Direction.Middle,
            MiddleRight = Direction.Right | Direction.Middle,
            BottomLeft = Direction.Left | Direction.Bottom,
            BottomCenter = Direction.Center | Direction.Bottom,
            BottomRight = Direction.Right | Direction.Bottom,
        }
        public struct ContentOffset
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
