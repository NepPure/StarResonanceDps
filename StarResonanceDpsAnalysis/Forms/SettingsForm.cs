using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class SettingsForm : BorderlessForm
    {
        public SettingsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            LoadDevices();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色

            inputNumber1.Value = (decimal)AppConfig.Transparency;


        }

        /// <summary>
        /// 鼠标穿透键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input1.Text = string.Empty;
                return;
            }
            input1.Text = e.KeyCode.ToString();
            AppConfig.MouseThroughKey = e.KeyCode;
        }

        /// <summary>
        /// 窗体透明键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input2.Text = string.Empty;
                return;
            }
            input2.Text = e.KeyCode.ToString();
            AppConfig.FormTransparencyKey = e.KeyCode;
        }

        /// <summary>
        /// 开关键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input3.Text = string.Empty;
                return;
            }
            input3.Text = e.KeyCode.ToString();
            AppConfig.WindowToggleKey = e.KeyCode;
        }

        /// <summary>
        /// 清空数据键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input4_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input4.Text = string.Empty;
                return;
            }
            input4.Text = e.KeyCode.ToString();
            AppConfig.ClearDataKey = e.KeyCode;
        }

        /// <summary>
        /// 清空历史键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input5.Text = string.Empty;
                return;
            }
            input5.Text = e.KeyCode.ToString();
            AppConfig.ClearHistoryKey = e.KeyCode;
        }

        private void InterfaceComboBox_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (combox_changed)
            {
                AppConfig.NetworkCard = InterfaceComboBox.SelectedIndex;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AppConfig.Transparency = (double)inputNumber1.Value;
            AppConfig.CombatTimeClearDelaySeconds = (int)inputNumber2.Value;//保存战斗时间清除延迟
            if (AppConfig.Transparency < 10)
            {
                AppConfig.Transparency = 95;
                MessageBox.Show("透明度不能低于10%，已自动设置为默认值");
            }
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

        /// <summary>
        /// 检测窗体变动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                //浅色
                BasicSetupPanel.Back = KeySettingsPanel.Back = CombatSettingsPanel.Back = ColorTranslator.FromHtml("#FFFFFF");
                BackgroundPanel.Back = ColorTranslator.FromHtml("#EFEFEF");



            }
            else
            {
                BasicSetupPanel.Back = KeySettingsPanel.Back = CombatSettingsPanel.Back = ColorTranslator.FromHtml("#282828");
                BackgroundPanel.Back = ColorTranslator.FromHtml("#1E1E1E");

            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
