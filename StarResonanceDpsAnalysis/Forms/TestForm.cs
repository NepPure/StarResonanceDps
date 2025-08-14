using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// 下面这些命名空间来自你的项目：自定义控件、效果、扩展
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Extends;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class TestForm : Form
    {
        // 演示用的数据源：10 个进度条条目
        // ProgressBarData 是你项目里的数据模型（含 ID、Text、ProgressBarValue、圆角等）
        readonly List<ProgressBarData> list =
        [
            new() { ID = 1,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 2,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 3,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 4,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 5,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 6,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 7,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 8,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 9,  Text = "0", ProgressBarCornerRadius = 99 },
            new() { ID = 10, Text = "0", ProgressBarCornerRadius = 99 },
        ];

        // 用于累积每个条目的“原始值”；UI 显示会用 “当前值 / 最大值” 做归一化
        List<int> vList = [];

        public TestForm()
        {
            InitializeComponent();

            // ======= 单个进度条（textProgressBar1）的外观设置 =======
            textProgressBar1.Padding = new Padding(3, 3, 3, 3);
            textProgressBar1.TextPadding = new Padding(3, 3, 3, 3);
            textProgressBar1.ProgressBarCornerRadius = 999; // 超大圆角

            sortedProgressBarList1.Data = list;
            sortedProgressBarList1.ProgressBarHeight = 30;
            sortedProgressBarList1.AnimationDuration = 1000;
            sortedProgressBarList1.AnimationQuality = Quality.Low;

            // ======= 绑定演示逻辑 =======
            BindTaskTest1();    // 周期更新单个进度条
            //BindClickTest1();  // 可切换为点击再更新

            // vList 初值：与 list 相同长度，全部从 0 开始
            vList = [.. list.Select(_ => 0)];

            BindListTest();     // 周期更新列表进度条
            //BindClickListTest(); // 可切换为点击按钮后再更新

            TestBezier();       // 按钮点击 -> 在 panel1 上绘制贝塞尔曲线
        }

        /// <summary>
        /// 每 ~33ms 更新一次 textProgressBar1 的值（0~1），并显示文本
        /// ⚠️ 注意：这里用 Task.Run + while(true) 会在“线程池线程”上更新控件属性，
        ///    WinForms 默认是“不允许跨线程更改 UI”的（可能抛异常或有隐患）。
        ///    如果你没关闭检查，就需要用 this.BeginInvoke(...) 回到 UI 线程。
        ///    更推荐用 System.Windows.Forms.Timer。
        /// </summary>
        private void BindTaskTest1()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var now = DateTime.Now;

                    // value：以 5 秒为一个循环（0 -> 1）
                    var value = (now.Second % 5 * 1000 + now.Millisecond) / 5000d;

                    // ⚠️ 跨线程更新控件：最好封送回 UI 线程
                    if (!IsDisposed && textProgressBar1.IsHandleCreated)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            textProgressBar1.ProgressBarValue = value;     // 0~1 之间
                            textProgressBar1.Text = value.ToString("0.000"); // 文本显示更友好
                        }));
                    }

                    Thread.Sleep(33); // ~30 FPS
                }
            });
        }

        /// <summary>
        /// 可选：改成“点击窗体时计算一次值并更新进度条”
        /// </summary>
        private void BindClickTest1()
        {
            Click += (s, e) =>
            {
                var now = DateTime.Now;
                var value = (now.Second % 5 * 1000 + now.Millisecond) / 5000d;

                // 这里在 UI 线程，所以可直接更新
                textProgressBar1.ProgressBarValue = value;
                textProgressBar1.Text = value.ToString("0.000");
            };
        }

        /// <summary>
        /// 每 ~33ms 随机累加 10 个条目的原始值，然后用最大值归一化到 0~1，再更新列表控件
        /// ⚠️ 同样涉及跨线程更新，使用 BeginInvoke 包一层比较稳妥。
        /// </summary>
        private void BindListTest()
        {
            Task.Run(() =>
            {
                var rd = new Random();

                while (true)
                {
                    // 1) 原始值随机增加
                    for (var i = 0; i < 10; i++)
                    {
                        vList[i] += rd.Next(0, 10);
                    }

                    // 2) 找出最大值用于归一化（避免除 0）
                    var max = Math.Max(1, vList.Max());

                    // 3) 回到 UI 线程更新 list[i]（控件监听 Data 或单项属性变化）
                    if (!IsDisposed && IsHandleCreated)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            for (var i = 0; i < list.Count; i++)
                            {
                                var value = vList[i] / (double)max; // 归一化到 0~1
                                list[i].ProgressBarValue = value;
                                list[i].Text = $"{list[i].ID}: {vList[i]}";
                            }

                            // 如果你的列表控件需要手动刷新，可调用：
                            // sortedProgressBarList1.Invalidate();
                        }));
                    }

                    Thread.Sleep(33);
                }
            });
        }

        /// <summary>
        /// 可选：用按钮 + 10 个 NumericUpDown 手动输入各项数值，再统一刷新列表
        /// 这里更新发生在 UI 线程（按钮点击事件），可直接操作。
        /// </summary>
        private void BindClickListTest()
        {
            button1.Click += (s, e) =>
            {
                // 1) 读取 10 个输入值
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

                // 2) 归一化
                var max = Math.Max(1, vList.Max());

                // 3) 更新 list（控件可据此重绘动画）
                for (int i = 0; i < 10; i++)
                {
                    list[i].ProgressBarValue = vList[i] / (double)max;
                    list[i].Text = $"{list[i].ID}: {vList[i]}";
                }
            };
        }

        /// <summary>
        /// 点击按钮后，根据四个控制点 (x1,y1,x2,y2) 与品质枚举，绘制一条三次贝塞尔曲线到 panel1 的背景
        /// 这里是典型的 GDI 一次性离屏绘制（绘到 Bitmap 再设为 BackgroundImage）
        /// </summary>
        private void TestBezier()
        {
            var rd = new Random();

            button1.Click += (s, e) =>
            {
                // 1) 从 NumericUpDown 读取贝塞尔参数（假设都是 0~1 的小数，或已约定范围）
                var bezier = new CubicBezier(
                    (float)numericUpDown1.Value, // x1
                    (float)numericUpDown2.Value, // y1
                    (float)numericUpDown3.Value, // x2
                    (float)numericUpDown4.Value, // y2
                    (Quality)numericUpDown5.Value); // 品质（采样精度或近似算法开销）

                // 2) 用目标 Panel 的尺寸创建位图，并在位图上绘图（离屏）
                var bm = new Bitmap(panel1.Width, panel1.Height);
                using var g = Graphics.FromImage(bm);

                // 建议：提高线条质量（抗锯齿）
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(panel1.BackColor); // 背景与 Panel 一致

                // 3) 连续取样 t = [0,1) 上的点，逐段连线
                var prevPoint = new PointF(0, 0);
                // ⚠️ 预先创建 Pen，避免循环中频繁 new 造成 GDI 资源开销
                using var pen = new Pen(Color.Black, 2f); // 统一颜色/线宽更易辨识

                for (var i = 0; i < bm.Width; i++)
                {
                    var t = i / (float)(bm.Width - 1); // 让 t 覆盖到 1
                    var bezierPercent = bezier.GetProximateBezierValue(t); // 0~1

                    // WinForms 坐标 y 轴向下：若希望曲线“向上”，可用 (1 - bezierPercent)
                    var nowPoint = new PointF(i, (1f - bezierPercent) * bm.Height);

                    if (i > 0)
                    {
                        g.DrawLine(pen, prevPoint, nowPoint);
                    }
                    prevPoint = nowPoint;
                }

                // 4) 替换 Panel 背景图（注意释放旧图，避免内存泄漏）
                var prevImg = panel1.BackgroundImage;
                panel1.BackgroundImage = bm;
                prevImg?.Dispose();
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 设计器生成的空事件，可删除或保留
        }

        private void TestForm_Load(object sender, EventArgs e)
        {

        }
    }
}
