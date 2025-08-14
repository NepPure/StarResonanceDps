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
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Extends;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class TestForm : Form
    {
        readonly List<ProgressBarData> list =
        [
            new() { ID = 1, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 2, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 3, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 4, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 5, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 6, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 7, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 8, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 9, Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 10, Text = "0", ProgressBarCornerRadius = 99 },

            //new() { ID = 11, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 12, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 13, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 14, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 15, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 16, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 17, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 18, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 19, Text = "0", ProgressBarCornerRadius = 99 },
            //new() { ID = 20, Text = "0", ProgressBarCornerRadius = 99 },
        ];
        List<int> vList = [];

        public TestForm()
        {
            InitializeComponent();

            textProgressBar1.Padding = new Padding(3, 3, 3, 3);
            textProgressBar1.TextPadding = new Padding(3, 3, 3, 3);
            textProgressBar1.ProgressBarCornerRadius = 999;

            sortedProgressBarList1.Data = list;
            sortedProgressBarList1.ProgressBarHeight = 30;
            sortedProgressBarList1.AnimationDuration = 1000;
            sortedProgressBarList1.AnimationQuality = Quality.Low;

            BindTaskTest1();

            //BindClickTest1();

            vList = [.. list.Select(e => 0)];

            BindListTest();

            //BindClickListTest();

            TestBezier();
        }

        private void BindTaskTest1()
        {
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
        }

        private void BindClickTest1()
        {
            Click += (s, e) =>
            {
                var now = DateTime.Now;

                var value = (now.Second % 5 * 1000 + now.Millisecond) / 5000d;
                textProgressBar1.ProgressBarValue = value;
                textProgressBar1.Text = value.ToString();
            };
        }

        private void BindListTest()
        {
            Task.Run(() =>
            {
                var rd = new Random();

                while (true)
                {
                    for (var i = 0; i < 10; i++)
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
        }

        private void BindClickListTest()
        {
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

        private void TestBezier()
        {
            var rd = new Random();

            button1.Click += (s, e) =>
            {
                var bezier = new CubicBezier(
                    (float)numericUpDown1.Value,
                    (float)numericUpDown2.Value,
                    (float)numericUpDown3.Value,
                    (float)numericUpDown4.Value,
                    (Quality)numericUpDown5.Value);

                var bm = new Bitmap(panel1.Width, panel1.Height);
                using var g = Graphics.FromImage(bm);
                var prevPoint = new PointF();
                for (var i = 0; i < bm.Width; i++)
                {
                    var t = i / (float)bm.Width;
                    var bezierPersent = bezier.GetProximateBezierValue(t);
                    var nowPoint = new PointF(i, bezierPersent * bm.Height);
                    g.DrawLine(new Pen(Color.FromArgb(rd.Next() * int.MaxValue)), prevPoint, nowPoint);

                    prevPoint = nowPoint;
                }

                var prevImg = panel1.BackgroundImage;
                panel1.BackgroundImage = bm;
                prevImg?.Dispose();
            };
        }

    }
}
