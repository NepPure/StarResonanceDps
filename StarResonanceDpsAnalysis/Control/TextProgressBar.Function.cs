using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class TextProgressBar
    {

        private Brush? ProgressBarBrush { get; set; } = null;
        private Brush? ProgressBarTextBrush { get; set; } = null;
        private void DrawTextProgressBarControl(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;

            var g = e.Graphics;
            g.Clear(BackColor);

            var barWidth = (Width - Padding.Left - Padding.Right) * ProgressBarValue;
            if (barWidth >= 1)
            {
                ProgressBarBrush ??= new SolidBrush(ProgressBarColor);

                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                var barHeight = Height - Padding.Top - Padding.Bottom;
                var diameter = Math.Min(ProgressBarCornerRadius * 2, Math.Min(barWidth, barHeight));
                var rect = new RectangleF(0, 0, (float)diameter, (float)diameter);

                using var path = new GraphicsPath();
                // 左上角
                rect.X = Padding.Left;
                rect.Y = Padding.Top;
                path.AddArc(rect, 180, 90);
                // 右上角
                rect.X = Padding.Left + (int)(barWidth - diameter);
                path.AddArc(rect, 270, 90);
                // 右下角
                rect.Y = (int)(Height - Padding.Bottom - diameter);
                path.AddArc(rect, 0, 90);
                // 左下角
                rect.X = Padding.Left;
                path.AddArc(rect, 90, 90);
                // 闭合图形
                path.CloseFigure();

                g.FillPath(ProgressBarBrush, path);
            }

            ProgressBarTextBrush ??= new SolidBrush(ForeColor);

            var textLeft = Padding.Left + TextPadding.Left;
            var textTop = (Height - Padding.Top - Padding.Bottom - TextPadding.Top - TextPadding.Bottom - Font.Size) / 2;

            g.DrawString(Text, Font, ProgressBarTextBrush, new PointF(textLeft, textTop));
        }

        private void RGB2HSL(Color color, out double h, out double s, out double l)
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));

            h = s = l = (max + min) / 2.0d;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                if (max == color.R)
                    h = (color.G - color.B) / d + (color.G < color.B ? 6 : 0);
                else if (max == color.G)
                    h = (color.B - color.R) / d + 2;
                else
                    h = (color.R - color.G) / d + 4;

                h /= 6.0;
            }
        }
    }
}
