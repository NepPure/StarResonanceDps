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
        }

        private void RankingsForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}
