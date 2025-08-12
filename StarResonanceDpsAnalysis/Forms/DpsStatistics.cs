using AntdUI;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Properties;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatistics : BorderlessForm
    {
        public DpsStatistics()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
        }

        private void DpsStatistics_Load(object sender, EventArgs e)
        {

        }


        private void button_AlwaysOnTop_Click(object sender, EventArgs e)
        {
            if (this.TopMost)
            {
                this.TopMost = false;
                button_AlwaysOnTop.Toggle = false;
            }
            else
            {
                this.TopMost = true;
                button_AlwaysOnTop.Toggle = true;

            }
        }

        #region 切换显示类型（支持单次/全程联动）
        List<string> singleLabels = new() { "单次伤害", "单次治疗", "单次承伤" };
        List<string> totalLabels = new() { "全程伤害", "全程治疗", "全程承伤" };

        int currentIndex = 0;        // 当前类别：0伤害/1治疗/2承伤
        bool showTotal = false;      // false=单次；true=全程

        private void UpdateHeaderText()
        {
            pageHeader1.SubText = showTotal ? totalLabels[currentIndex]
                                            : singleLabels[currentIndex];
        }

        // 左切换
        private void LeftHandoffButton_Click(object sender, EventArgs e)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = singleLabels.Count - 1;
            UpdateHeaderText();
        }

        // 右切换
        private void RightHandoffButton_Click(object sender, EventArgs e)
        {
            currentIndex++;
            if (currentIndex >= singleLabels.Count) currentIndex = 0;
            UpdateHeaderText();
        }

        // 单次/全程切换
        private void button3_Click(object sender, EventArgs e)
        {
            showTotal = !showTotal;
            UpdateHeaderText();
        }
        #endregion


        /// <summary>
        /// 清空当前数据数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button_Settings_Click(object sender, EventArgs e)
        {
            var menulist = new IContextMenuStripItem[]
             {
                    new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up},
                    new ContextMenuStripItem("主窗体"){ IconSvg = Resources.data_display, },
                    new ContextMenuStripItem("用户UID设置"){ IconSvg = Resources.userUid, },
                    new ContextMenuStripItem("数据记录类型"){ IconSvg = Resources.data_display, },
             }
            ;

            AntdUI.ContextMenuStrip.open(this, it =>
            {
                switch (it.Text)
                {
                    case "基础设置":
                        OpenSettingsDialog();
                        break;
                    case "数据显示设置":
                        //dataDisplay(); 
                        break;
                    case "用户设置":
                        SetUserUid();

                        break;
                }
            }, menulist);
        }

        /// <summary>
        /// 打开基础设置面板
        /// </summary>
        private void OpenSettingsDialog()
        {
            if (FormManager.settingsForm == null || FormManager.settingsForm.IsDisposed)
            {
                FormManager.settingsForm = new SettingsForm();
            }
            FormManager.settingsForm.Show();

        }

        private void SetUserUid()
        {
            if (FormManager.userUidSetForm == null || FormManager.userUidSetForm.IsDisposed)
            {
                FormManager.userUidSetForm = new UserUidSetForm();
            }
            FormManager.userUidSetForm.Show();
        }

        private void button_AlwaysOnTop_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button_AlwaysOnTop, "置顶窗口");


        }

        private void ToolTip(System.Windows.Forms.Control control, string text)
        {

            AntdUI.TooltipComponent tooltip = new AntdUI.TooltipComponent()
            {
                Font = new Font("HarmonyOS Sans SC", 8, FontStyle.Regular),
            };
            tooltip.ArrowAlign = AntdUI.TAlign.TL;
            tooltip.SetTip(control, text);
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button1, "清空当前数据");
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button3, "点击切换：单次统计/全程统");
        }
    }
}
