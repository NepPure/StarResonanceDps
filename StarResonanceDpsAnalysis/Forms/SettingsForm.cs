using AntdUI;
using DocumentFormat.OpenXml.Math;
using StarResonanceDpsAnalysis.Plugin;
using System.Runtime.InteropServices;

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

            slider1.Value = (int)AppConfig.Transparency;
            inputNumber2.Value = (decimal)AppConfig.CombatTimeClearDelaySeconds;
        }

        private void TransparencyKnob_ValueChanged(object sender, int value)
        {
            // 实时更新透明度（用于实时预览效果）
            try
            {
                // 获取MainForm实例
                var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                if (mainForm != null)
                {
                    // 检查透明度值的有效性（10-100之间）
                    if (value >= 10 && value <= 100)
                    {
                        // 实时更新MainForm的透明度进行预览
                        mainForm.Opacity = value / 100.0;
                        Console.WriteLine($"实时透明度预览: {value}% (Opacity: {mainForm.Opacity})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新透明度预览时出错: {ex.Message}");
            }
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

                // 通知MainForm更新网卡设置提示
                try
                {
                    var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                    mainForm?.RefreshNetworkCardSettingTip();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"更新MainForm网卡设置提示时出错: {ex.Message}");
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {


            // 保存到配置
            AppConfig.Transparency = slider1.Value;
            AppConfig.CombatTimeClearDelaySeconds = (int)inputNumber2.Value;



            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
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
                stackPanel1.Back = ColorTranslator.FromHtml("#EFEFEF");
                //transparencyKnob1.IsDarkMode = false;
            }
            else
            {
                BasicSetupPanel.Back = KeySettingsPanel.Back = CombatSettingsPanel.Back = ColorTranslator.FromHtml("#282828");
                stackPanel1.Back = ColorTranslator.FromHtml("#1E1E1E");
                //transparencyKnob1.IsDarkMode = true;
            }
        }




        private void slider1_ValueChanged(object sender, IntEventArgs e)
        {

            this.Opacity = (double)e.Value / 100;
            //this.BackColor = Color.Transparent;
            //this.TransparencyKey = Color.Transparent;

        }

        private void KeySettingsPanel_Click(object sender, EventArgs e)
        {

        }
    }
}
