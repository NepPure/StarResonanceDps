using System;
using System.Diagnostics;

using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.WinForm.Control;
using StarResonanceDpsAnalysis.WinForm.Core;
using StarResonanceDpsAnalysis.WinForm.Effects;
using StarResonanceDpsAnalysis.WinForm.Plugin;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class MainForm : BorderlessForm
    {

        #region ========== 构造与启动加载 ==========

        public MainForm()
        {

            InitializeComponent(); // # WinForms 初始化
            FormGui.SetDefaultGUI(this); // # UI 样式：统一默认外观


            pageHeader_MainHeader.Text = Text = $"{FormManager.APP_NAME} {FormManager.AppVersion}";
            label_NowVersionNumber.Text = FormManager.AppVersion;

            //InitTableColumnsConfigAtFirstRun(); // # 列显隐初始化：首次运行建立列配置
            //LoadTableColumnVisibilitySettings(); // # 读取列显隐配置
            //ToggleTableView(); // # 表格视图切换（依配置）
            //LoadFromEmbeddedSkillConfig(); // # 预装载技能元数据 → SkillBook

            FormGui.SetColorMode(this, AppConfig.IsLight); // # 主题：主窗体明暗模式

            var alimamaFont_Size12Bold = HandledAssets.HarmonyOS_Sans(12, FontStyle.Bold);
            var alimamaFont_Size9 = HandledAssets.HarmonyOS_Sans(9);

            var size12BoldControlList = new List<System.Windows.Forms.Control>()
            {
                groupBox_About, label_AppName, label_NowVersionTip, label_NowVersionDevelopersTip
            };
            foreach (var c in size12BoldControlList)
            {
                c.Font = alimamaFont_Size12Bold;
            }

            var size9ControlList = new List<System.Windows.Forms.Control>
            {
                label_SelfIntroduce, label_NowVersionNumber, label_NowVersionDevelopers,
                label_OpenSourceTip_1, linkLabel_GitHub, label_OpenSourceTip_2, linkLabel_QQGroup,
                label_ThankHelpFromTip_1, linkLabel_NodeJsProject, label_ThankHelpFromTip_2,
                label_Copyright
            };
            foreach (var c in size9ControlList)
            {
                c.Font = alimamaFont_Size9;
            }

            label_AppName.Text = $"{FormManager.APP_NAME}";

            pictureBox_AppIcon.Image = HandledAssets.ApplicationIcon_256x256;
        }


        #endregion

        #region —— 按钮/复选框/下拉事件 —— 
        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight; // # 状态翻转：明/暗

            button_ThemeSwitch.Toggle = !AppConfig.IsLight; // # UI同步：按钮切换状态

            FormGui.SetColorMode(this, AppConfig.IsLight);

            FormGui.SetColorMode(FormManager.SettingsForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.DpsStatistics, AppConfig.IsLight);//设置窗体颜色

            // # 注意：部分窗体可能为 null 或已释放，SetColorMode 内部应做空值与IsDisposed判断方可安全调用
        }

        #endregion

        private void linkLabel_GitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/anying1073/StarResonanceDps",
                UseShellExecute = true
            });

            linkLabel_GitHub.LinkVisited = true;
        }

        private void linkLabel_QQGroup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://qm.qq.com/cgi-bin/qm/qr?k=QeIozXvLSRH9dL_CTZeisQ1Ae4CZpiSc&jump_from=webapi&authKey=HNr5BrrIhqRPyGs2R54NucKsg7Pb9/c0a03gih69PekWfSNLh9MIi/ClXXnaMzHK",
                UseShellExecute = true
            });

            linkLabel_QQGroup.LinkVisited = true;
        }

        private void linkLabel_NodeJsProject_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/dmlgzs/StarResonanceDamageCounter",
                UseShellExecute = true
            });

            linkLabel_NodeJsProject.LinkVisited = true;
        }

        private void MainForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                groupBox_About.ForeColor = Color.Black;
                linkLabel_GitHub.LinkColor = linkLabel_QQGroup.LinkColor = linkLabel_NodeJsProject.LinkColor = Color.Blue;
                linkLabel_GitHub.VisitedLinkColor = linkLabel_QQGroup.VisitedLinkColor = linkLabel_NodeJsProject.VisitedLinkColor = Color.Purple;
            }
            else
            {
                groupBox_About.ForeColor = Color.White;
                linkLabel_GitHub.LinkColor = linkLabel_QQGroup.LinkColor = linkLabel_NodeJsProject.LinkColor = Color.LightSkyBlue;
                linkLabel_GitHub.VisitedLinkColor = linkLabel_QQGroup.VisitedLinkColor = linkLabel_NodeJsProject.VisitedLinkColor = Color.MediumPurple;
            }
        }
    }
}
