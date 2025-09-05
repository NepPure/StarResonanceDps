using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StarResonanceDpsAnalysis.WPF.Views
{
    /// <summary>
    /// SettingForm.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                var rd = new Random();
                while (true)
                {
                    Test.Dispatcher.Invoke(() =>
                    {
                        Test.Value = rd.NextDouble();
                    });

                    await Task.Delay(1000);
                }
            });
        }
    }
}
