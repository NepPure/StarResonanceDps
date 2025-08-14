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
        int id = 1;
        List<ProgressBarData> data = [];
        public TestForm()
        {
            InitializeComponent();

            sortedProgressBarList1.AnimationDuration = 1000;
            sortedProgressBarList1.AnimationQuality = Quality.High;

            numericUpDown1.Minimum = -1;
            numericUpDown2.Minimum = -1;

            button1.Click += (s, e) =>
            {
                if (numericUpDown1.Value > data.Count) return;

                if (numericUpDown1.Value <= 0)
                {
                    data.Add(new ProgressBarData
                    {
                        ID = id,
                        ProgressBarValue = (double)numericUpDown2.Value / 100d,
                        Text = $"{id++}: {numericUpDown2.Value}"
                    });
                }
                else
                {
                    var index = (int)numericUpDown1.Value - 1;
                    data[index].ProgressBarValue = (double)numericUpDown2.Value / 100d;
                    data[index].Text = $"{index + 1}: {numericUpDown2.Value}";
                }

                sortedProgressBarList1.Data = data;
            };

            button2.Click += (s, e) =>
            {
                if (numericUpDown1.Value == numericUpDown2.Value && numericUpDown2.Value == -1) 
                {
                    data.Clear();
                    sortedProgressBarList1.Data = data;
                    return;
                }

                if (numericUpDown1.Value < 0) return;

                var index = data.FindIndex(e => e.ID == (int)numericUpDown1.Value);
                if (index < 0) return;

                data.RemoveAt(index);

                sortedProgressBarList1.Data = data;
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
