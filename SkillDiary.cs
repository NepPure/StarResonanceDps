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
using 星痕共鸣DPS统计.Plugin;

namespace 星痕共鸣DPS统计
{
    public partial class SkillDiary : BorderlessForm
    {
        public SkillDiary()
        {
            InitializeComponent();
            FormGui.GUI(this);

        }

        private void SkillDiary_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(Common.skillDiary, config.isLight);//设置窗体颜色
        }
    }
}
