using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using StarResonanceDpsAnalysis.WPF.Controls;
using StarResonanceDpsAnalysis.WPF.ViewModels;

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
            Task.Run(async () =>
            {
                var rd = new Random();
                var arrs = new long[10];

                while (true)
                {
                    var data = new ObservableCollection<ProgressBarData>();
                    for (var i = 0; i < 10; ++i)
                    {
                        arrs[i] += rd.Next(10, 20);
                    }

                    var max = arrs.Max();
                    for (var i = 0; i < 10; ++i)
                    {
                        var v = 1d * arrs[i] / max;
                        data.Add(new()
                        {
                            ID = i,
                            ProgressBarValue = v,
                            Data = new Data() { Order = 0 },
                        });
                    }
                    ProgressBarList.Dispatcher.Invoke(() =>
                    {
                        ProgressBarList.Data = data;
                    });

                    await Task.Delay(100);
                }
            });
        }

        public class Data : OrderingDataViewModel
        {
            public double Value { get; set; }
            public string Name => $"{Order.ToString().PadLeft(2, '0')}: {Math.Round(Value, 2)}";
        }
    }
}
