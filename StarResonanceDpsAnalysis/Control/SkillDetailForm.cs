using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SkillDetailForm : BorderlessForm
    {
        public SkillDetailForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            ToggleTableView();
        }

        private void SkillDetailForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadPlayerSkillsToTable();
        }
    }
}
