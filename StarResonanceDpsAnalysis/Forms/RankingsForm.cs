using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using System.Runtime.InteropServices;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class RankingsForm : BorderlessForm
    {
        public RankingsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            ToggleTableView();
        }

        private void RankingsForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }



        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            divider3.Text = "伤害榜";
            get_dps_rank();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            divider3.Text = "战力榜";
            get_dps_rank();
        }

        private void segmented1_SelectIndexChanged(object sender, IntEventArgs e)
        {
            get_dps_rank();
        }



    }
}
