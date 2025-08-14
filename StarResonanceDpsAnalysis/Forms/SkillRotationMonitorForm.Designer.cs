namespace StarResonanceDpsAnalysis.Forms
{
    partial class SkillRotationMonitorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pageHeader_MainHeader = new AntdUI.PageHeader();
            panel_Main = new Panel();
            panel_Content = new Panel();
            panel_SkillRotation = new AntdUI.VirtualPanel();
            panel_Controls = new Panel();
            button_RefreshPlayers = new AntdUI.Button();
            button_Clear = new AntdUI.Button();
            button_StartStop = new AntdUI.Button();
            dropdown_PlayerSelect = new AntdUI.Dropdown();
            label_SelectPlayer = new AntdUI.Label();
            panel_Stats = new Panel();
            label_AvgInterval = new AntdUI.Label();
            label_LastSkillTime = new AntdUI.Label();
            label_TotalSkills = new AntdUI.Label();
            label_PlayerName = new AntdUI.Label();
            button_Close = new AntdUI.Button();
            panel_Main.SuspendLayout();
            panel_Content.SuspendLayout();
            panel_Controls.SuspendLayout();
            panel_Stats.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader_MainHeader
            // 
            pageHeader_MainHeader.Dock = DockStyle.Top;
            pageHeader_MainHeader.Location = new Point(0, 0);
            pageHeader_MainHeader.Name = "pageHeader_MainHeader";
            pageHeader_MainHeader.ShowBack = false;
            pageHeader_MainHeader.Size = new Size(900, 60);
            pageHeader_MainHeader.TabIndex = 0;
            pageHeader_MainHeader.Text = "技能释放循环监测";
            pageHeader_MainHeader.SubText = "监测选择玩家的技能释放顺序和间隔";
            pageHeader_MainHeader.MouseDown += TitleText_MouseDown;
            // 
            // panel_Main
            // 
            panel_Main.Controls.Add(panel_Content);
            panel_Main.Controls.Add(panel_Controls);
            panel_Main.Controls.Add(panel_Stats);
            panel_Main.Dock = DockStyle.Fill;
            panel_Main.Location = new Point(0, 60);
            panel_Main.Name = "panel_Main";
            panel_Main.Padding = new Padding(10);
            panel_Main.Size = new Size(900, 540);
            panel_Main.TabIndex = 1;
            // 
            // panel_Content
            // 
            panel_Content.Controls.Add(panel_SkillRotation);
            panel_Content.Dock = DockStyle.Fill;
            panel_Content.Location = new Point(10, 80);
            panel_Content.Name = "panel_Content";
            panel_Content.Size = new Size(880, 370);
            panel_Content.TabIndex = 2;
            // 
            // panel_SkillRotation
            // 
            panel_SkillRotation.Dock = DockStyle.Fill;
            panel_SkillRotation.Location = new Point(0, 0);
            panel_SkillRotation.Name = "panel_SkillRotation";
            panel_SkillRotation.Size = new Size(880, 370);
            panel_SkillRotation.TabIndex = 0;
            // 
            // panel_Controls
            // 
            panel_Controls.Controls.Add(button_Close);
            panel_Controls.Controls.Add(button_RefreshPlayers);
            panel_Controls.Controls.Add(button_Clear);
            panel_Controls.Controls.Add(button_StartStop);
            panel_Controls.Controls.Add(dropdown_PlayerSelect);
            panel_Controls.Controls.Add(label_SelectPlayer);
            panel_Controls.Dock = DockStyle.Top;
            panel_Controls.Location = new Point(10, 10);
            panel_Controls.Name = "panel_Controls";
            panel_Controls.Size = new Size(880, 70);
            panel_Controls.TabIndex = 1;
            // 
            // button_Close
            // 
            button_Close.Location = new Point(770, 35);
            button_Close.Name = "button_Close";
            button_Close.Size = new Size(80, 30);
            button_Close.TabIndex = 5;
            button_Close.Text = "关闭窗口";
            button_Close.Type = AntdUI.TTypeMini.Error;
            button_Close.Click += button_Close_Click;
            // 
            // button_RefreshPlayers
            // 
            button_RefreshPlayers.Location = new Point(680, 35);
            button_RefreshPlayers.Name = "button_RefreshPlayers";
            button_RefreshPlayers.Size = new Size(80, 30);
            button_RefreshPlayers.TabIndex = 4;
            button_RefreshPlayers.Text = "刷新列表";
            button_RefreshPlayers.Type = AntdUI.TTypeMini.Default;
            button_RefreshPlayers.Click += button_RefreshPlayers_Click;
            // 
            // button_Clear
            // 
            button_Clear.Location = new Point(590, 35);
            button_Clear.Name = "button_Clear";
            button_Clear.Size = new Size(80, 30);
            button_Clear.TabIndex = 3;
            button_Clear.Text = "清空数据";
            button_Clear.Type = AntdUI.TTypeMini.Default;
            button_Clear.Click += button_Clear_Click;
            // 
            // button_StartStop
            // 
            button_StartStop.Location = new Point(500, 35);
            button_StartStop.Name = "button_StartStop";
            button_StartStop.Size = new Size(80, 30);
            button_StartStop.TabIndex = 2;
            button_StartStop.Text = "开始监控";
            button_StartStop.Type = AntdUI.TTypeMini.Primary;
            button_StartStop.Click += button_StartStop_Click;
            // 
            // dropdown_PlayerSelect
            // 
            dropdown_PlayerSelect.Location = new Point(80, 35);
            dropdown_PlayerSelect.Name = "dropdown_PlayerSelect";
            dropdown_PlayerSelect.Size = new Size(400, 30);
            dropdown_PlayerSelect.TabIndex = 1;
            dropdown_PlayerSelect.SelectedValueChanged += dropdown_PlayerSelect_SelectedValueChanged;
            // 
            // label_SelectPlayer
            // 
            label_SelectPlayer.Location = new Point(0, 35);
            label_SelectPlayer.Name = "label_SelectPlayer";
            label_SelectPlayer.Size = new Size(80, 30);
            label_SelectPlayer.TabIndex = 0;
            label_SelectPlayer.Text = "选择玩家:";
            label_SelectPlayer.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel_Stats
            // 
            panel_Stats.Controls.Add(label_AvgInterval);
            panel_Stats.Controls.Add(label_LastSkillTime);
            panel_Stats.Controls.Add(label_TotalSkills);
            panel_Stats.Controls.Add(label_PlayerName);
            panel_Stats.Dock = DockStyle.Bottom;
            panel_Stats.Location = new Point(10, 450);
            panel_Stats.Name = "panel_Stats";
            panel_Stats.Size = new Size(880, 80);
            panel_Stats.TabIndex = 0;
            // 
            // label_AvgInterval
            // 
            label_AvgInterval.Location = new Point(660, 20);
            label_AvgInterval.Name = "label_AvgInterval";
            label_AvgInterval.Size = new Size(200, 50);
            label_AvgInterval.TabIndex = 3;
            label_AvgInterval.Text = "平均间隔: 无";
            label_AvgInterval.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label_LastSkillTime
            // 
            label_LastSkillTime.Location = new Point(440, 20);
            label_LastSkillTime.Name = "label_LastSkillTime";
            label_LastSkillTime.Size = new Size(200, 50);
            label_LastSkillTime.TabIndex = 2;
            label_LastSkillTime.Text = "最后技能: 无";
            label_LastSkillTime.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label_TotalSkills
            // 
            label_TotalSkills.Location = new Point(220, 20);
            label_TotalSkills.Name = "label_TotalSkills";
            label_TotalSkills.Size = new Size(200, 50);
            label_TotalSkills.TabIndex = 1;
            label_TotalSkills.Text = "技能总数: 0";
            label_TotalSkills.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label_PlayerName
            // 
            label_PlayerName.Location = new Point(0, 20);
            label_PlayerName.Name = "label_PlayerName";
            label_PlayerName.Size = new Size(200, 50);
            label_PlayerName.TabIndex = 0;
            label_PlayerName.Text = "玩家: 未选择";
            label_PlayerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SkillRotationMonitorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.Add(panel_Main);
            Controls.Add(pageHeader_MainHeader);
            MaximizeBox = false;
            Name = "SkillRotationMonitorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "技能释放循环监测";
            Load += SkillRotationMonitorForm_Load;
            panel_Main.ResumeLayout(false);
            panel_Content.ResumeLayout(false);
            panel_Controls.ResumeLayout(false);
            panel_Stats.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader_MainHeader;
        private Panel panel_Main;
        private Panel panel_Content;
        private AntdUI.VirtualPanel panel_SkillRotation;
        private Panel panel_Controls;
        private AntdUI.Button button_Close;
        private AntdUI.Button button_RefreshPlayers;
        private AntdUI.Button button_Clear;
        private AntdUI.Button button_StartStop;
        private AntdUI.Dropdown dropdown_PlayerSelect;
        private AntdUI.Label label_SelectPlayer;
        private Panel panel_Stats;
        private AntdUI.Label label_AvgInterval;
        private AntdUI.Label label_LastSkillTime;
        private AntdUI.Label label_TotalSkills;
        private AntdUI.Label label_PlayerName;
    }
}