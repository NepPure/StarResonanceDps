using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarResonanceDpsAnalysis.Control.GDI;
using StarResonanceDpsAnalysis.Extends;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class TextProgressBar
    {
        private readonly GDI_ProgressBar _gdiProgressBar = new();
        private readonly DrawInfo _drawInfo = new();

        private void DrawTextProgressBarControl(PaintEventArgs e) 
        {
            if (Width == 0 || Height == 0) return;

            _drawInfo.Width = Width;
            _drawInfo.Height = Height;
            _drawInfo.BackColor = BackColor;
            _drawInfo.ForeColor = ForeColor;
            _drawInfo.ProgressBarColor = ProgressBarColor;
            _drawInfo.ProgressBarValue = ProgressBarValue;
            _drawInfo.Text = Text;
            _drawInfo.Font = Font;
            _drawInfo.ProgressBarCornerRadius = ProgressBarCornerRadius;
            _drawInfo.TextPadding = TextPadding;

            e.Graphics.Clear(BackColor);

            _gdiProgressBar.Draw(e.Graphics, _drawInfo);
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
