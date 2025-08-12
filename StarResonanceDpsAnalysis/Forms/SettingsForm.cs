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
        /// <summary>
        /// 保存打开设置窗口时的透明度值，用于取消时恢复
        /// </summary>
        private double _originalTransparency;
        
        public SettingsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            LoadDevices();
            
            // 保存当前透明度设置
            _originalTransparency = AppConfig.Transparency;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色

            // 设置旋钮初始值
            transparencyKnob1.Value = (int)AppConfig.Transparency;
            transparencyKnob1.ValueChanged += TransparencyKnob_ValueChanged;

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
            // 保存设置按钮
            int newTransparency = transparencyKnob1.Value;
            
            // 验证透明度范围
            if (newTransparency < 10)
            {
                newTransparency = 95;
                MessageBox.Show("透明度不能低于10%，已自动设置为95%");
                transparencyKnob1.Value = newTransparency;
            }
            
            // 保存到配置
            AppConfig.Transparency = newTransparency;
            AppConfig.CombatTimeClearDelaySeconds = (int)inputNumber2.Value;
            
            // 更新MainForm的透明度到最终值（确保一致性）
            try
            {
                var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                if (mainForm != null)
                {
                    // 只有在非鼠标穿透状态下才设置透明度
                    if (!IsMainFormInMousePenetrateMode(mainForm))
                    {
                        mainForm.Opacity = newTransparency / 100.0;
                        Console.WriteLine($"透明度已保存并应用: {newTransparency}% (Opacity: {mainForm.Opacity})");
                    }
                    else
                    {
                        Console.WriteLine($"透明度已保存: {newTransparency}%，但当前处于鼠标穿透模式，透明度将在退出穿透模式时应用");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用最终透明度时出错: {ex.Message}");
            }
            
            // 标记为按钮关闭，避免OnFormClosed中恢复透明度
            _isClosingByButton = true;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // 取消按钮 - 恢复原始透明度
            try
            {
                var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                if (mainForm != null)
                {
                    // 只有在非鼠标穿透状态下才恢复透明度
                    if (!IsMainFormInMousePenetrateMode(mainForm))
                    {
                        mainForm.Opacity = _originalTransparency / 100.0;
                        Console.WriteLine($"透明度已恢复: {_originalTransparency}% (Opacity: {mainForm.Opacity})");
                    }
                    else
                    {
                        Console.WriteLine($"当前处于鼠标穿透模式，透明度将在退出穿透模式时恢复到原始值");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"恢复透明度时出错: {ex.Message}");
            }
            
            // 标记为按钮关闭，避免OnFormClosed中重复处理
            _isClosingByButton = true;
            this.Close();
        }
        
        /// <summary>
        /// 检查MainForm是否处于鼠标穿透模式
        /// </summary>
        /// <param name="mainForm">MainForm实例</param>
        /// <returns>是否处于鼠标穿透模式</returns>
        private bool IsMainFormInMousePenetrateMode(MainForm mainForm)
        {
            try
            {
                // 通过检查标题是否包含"[鼠标穿透]"来判断是否处于穿透模式
                // 这里使用反射或者添加公共属性来获取IsMousePenetrate状态
                // 简单的方法是检查透明度是否为0.4（穿透模式的固定值）
                return Math.Abs(mainForm.Opacity - 0.4) < 0.01;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查鼠标穿透状态时出错: {ex.Message}");
                return false;
            }
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
                transparencyKnob1.IsDarkMode = false;
            }
            else
            {
                BasicSetupPanel.Back = KeySettingsPanel.Back = CombatSettingsPanel.Back = ColorTranslator.FromHtml("#282828");
                BackgroundPanel.Back = ColorTranslator.FromHtml("#1E1E1E");
                transparencyKnob1.IsDarkMode = true;
            }
        }

        /// <summary>
        /// 窗体关闭时的处理（用于处理用户点击X按钮的情况）
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 如果不是通过按钮关闭的，则执行取消逻辑
            if (!_isClosingByButton && e.CloseReason == CloseReason.UserClosing)
            {
                try
                {
                    var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                    if (mainForm != null)
                    {
                        // 直接关闭窗体时，恢复到原始设置（相当于取消操作）
                        if (!IsMainFormInMousePenetrateMode(mainForm))
                        {
                            mainForm.Opacity = _originalTransparency / 100.0;
                            Console.WriteLine($"窗体关闭：透明度已恢复到原始值 {_originalTransparency}%");
                        }
                        else
                        {
                            Console.WriteLine($"窗体关闭：当前处于鼠标穿透模式，透明度将在退出穿透模式时恢复到原始值");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"窗体关闭时处理透明度出错: {ex.Message}");
                }
            }
            
            base.OnFormClosed(e);
        }
        
        /// <summary>
        /// 添加一个标记变量来跟踪是否是通过按钮关闭的
        /// </summary>
        private bool _isClosingByButton = false;
    }
}
