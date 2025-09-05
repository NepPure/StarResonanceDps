using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    public partial class CustomizeProgressBar
    {
        private void UpdateProgressBarAnimation() 
        {
            Debug.WriteLine($"[{DateTime.Now:T}] To: {ActualWidth * Value}");

            ProgressBarBox.BeginAnimation(WidthProperty, new DoubleAnimation
            {
                To = ActualWidth * Value,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            });
        }
    }
}
