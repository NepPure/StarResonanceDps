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

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class RankingsForm : BorderlessForm
    {
        public RankingsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
        }

        private void RankingsForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
