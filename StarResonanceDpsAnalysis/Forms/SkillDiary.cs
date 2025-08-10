using AntdUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StarResonanceDpsAnalysis.Plugin;

namespace StarResonanceDpsAnalysis
{
    public partial class SkillDiary : BorderlessForm
    {
        public SkillDiary()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);

        }


        private const int MaxLines = 100;

        public void AppendDiaryLine(string line)
        {
            if (input1.InvokeRequired)
            {
                input1.BeginInvoke(new Action<string>(AppendDiaryLine), line);
                return;
            }

            // 先计算当前行数：按 '\n' 分割（注意 Windows 换行是 "\r\n"，但这里拆出来也是一个 '\n'）
            int lineCount = input1.Text.Split('\n').Length;
            if (lineCount >= MaxLines)
                input1.Clear();

            // 再追加
            input1.AppendText(line + Environment.NewLine);
            input1.SelectionStart = input1.Text.Length;
            input1.ScrollToCaret();
        }



        private void SkillDiary_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }
    }
}
