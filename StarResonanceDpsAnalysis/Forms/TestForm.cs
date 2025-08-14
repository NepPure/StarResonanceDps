using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Extends;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();

            textProgressBar1.Padding = new Padding(3, 3, 3, 3);
            textProgressBar1.TextPadding = new Padding(3, 3, 3, 3);
            textProgressBar1.ProgressBarCornerRadius = 999;

            Task.Run(() =>
            {
                while (true)
                {
                    var now = DateTime.Now;

                    var value = (now.Second % 5 * 1000 + now.Millisecond) / 5000d;
                    textProgressBar1.ProgressBarValue = value;
                    textProgressBar1.Text = value.ToString();

                    Thread.Sleep(33);
                }
            });

            Click += (s, e) =>
            {
                var now = DateTime.Now;

                var value = (now.Second % 5 * 1000 + now.Millisecond) / 5000d;
                textProgressBar1.ProgressBarValue = value;
                textProgressBar1.Text = value.ToString();
            };

            var vList = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                vList.Add(0);
            }

            var list = new List<ProgressBarData>
            {
                new() { ID = 1, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 2, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 3, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 4, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 5, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 6, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 7, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 8, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 9, Text = "0", ProgressBarCornerRadius = 5 },
                new() { ID = 10, Text = "0", ProgressBarCornerRadius = 5 }
            };

            sortedProgressBarList1.Data = list;
            sortedProgressBarList1.AnimationDuration = 1000;
            sortedProgressBarList1.AnimationQuality = Quality.AlmostAccurate;

            Task.Run(() =>
            {
                var rd = new Random();

                while (true)
                {
                    for (var i = 0; i < vList.Count; i++)
                    {
                        vList[i] += rd.Next(0, 10);
                    }
                    var max = vList.Max();
                    for (var i = 0; i < list.Count; i++)
                    {
                        var value = vList[i] / (double)max;
                        list[i].ProgressBarValue = value;
                        list[i].Text = $"{list[i].ID}: {vList[i]}";
                    }

                    Thread.Sleep(33);
                }
            });

            //var data = new ProgressBarData() { ID = 1, Text = "0", ProgressBarCornerRadius = 5 };
            //var list = new List<ProgressBarData>
            //{
            //    data
            //};
            //sortedProgressBarList1.Data = list;
            //sortedProgressBarList1.AnimationDuration = 300;

            //Task.Run(() =>
            //{
            //    var rd = new Random();

            //    while (true)
            //    {
            //        var now = DateTime.Now;

            //        var value = (now.Second % 5 * 1000 + now.Millisecond) / 5000d;
            //        data.ProgressBarValue = value;
            //        data.Text = value.ToString();

            //        Thread.Sleep(33);
            //    }
            //});

            button1.Click += (s, e) =>
            {
                vList[0] = (int)numericUpDown1.Value;
                vList[1] = (int)numericUpDown2.Value;
                vList[2] = (int)numericUpDown3.Value;
                vList[3] = (int)numericUpDown4.Value;
                vList[4] = (int)numericUpDown5.Value;
                vList[5] = (int)numericUpDown6.Value;
                vList[6] = (int)numericUpDown7.Value;
                vList[7] = (int)numericUpDown8.Value;
                vList[8] = (int)numericUpDown9.Value;
                vList[9] = (int)numericUpDown10.Value;

                var max = vList.Max();

                list[0].ProgressBarValue = vList[0] / (double)max;
                list[0].Text = $"{list[0].ID}: {vList[0]}";
                list[1].ProgressBarValue = vList[1] / (double)max;
                list[1].Text = $"{list[1].ID}: {vList[1]}";
                list[2].ProgressBarValue = vList[2] / (double)max;
                list[2].Text = $"{list[2].ID}: {vList[2]}";
                list[3].ProgressBarValue = vList[3] / (double)max;
                list[3].Text = $"{list[3].ID}: {vList[3]}";
                list[4].ProgressBarValue = vList[4] / (double)max;
                list[4].Text = $"{list[4].ID}: {vList[4]}";
                list[5].ProgressBarValue = vList[5] / (double)max;
                list[5].Text = $"{list[5].ID}: {vList[5]}";
                list[6].ProgressBarValue = vList[6] / (double)max;
                list[6].Text = $"{list[6].ID}: {vList[6]}";
                list[7].ProgressBarValue = vList[7] / (double)max;
                list[7].Text = $"{list[7].ID}: {vList[7]}";
                list[8].ProgressBarValue = vList[8] / (double)max;
                list[8].Text = $"{list[8].ID}: {vList[8]}";
                list[9].ProgressBarValue = vList[9] / (double)max;
                list[9].Text = $"{list[9].ID}: {vList[9]}";

            };
        }
    }
}
