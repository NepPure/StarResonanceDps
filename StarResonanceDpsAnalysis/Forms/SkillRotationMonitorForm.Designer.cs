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
            panel_Main = new Panel();
            panel_SkillRotation = new AntdUI.VirtualPanel();
            panel_Controls = new Panel();
            button_Close = new AntdUI.Button();
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
            panel_Main.SuspendLayout();
            panel_Controls.SuspendLayout();
            panel_Stats.SuspendLayout();
            SuspendLayout();
            // 
            // panel_Main
            // 
            panel_Main.Controls.Add(panel_SkillRotation);
            panel_Main.Controls.Add(panel_Controls);
            panel_Main.Controls.Add(panel_Stats);
            panel_Main.Dock = DockStyle.Fill;
            panel_Main.Location = new Point(0, 0);
            panel_Main.Margin = new Padding(5, 4, 5, 4);
            panel_Main.Name = "panel_Main";
            panel_Main.Padding = new Padding(16, 14, 16, 14);
            panel_Main.Size = new Size(1414, 847);
            panel_Main.TabIndex = 1;
            // 
            // panel_SkillRotation
            // 
            panel_SkillRotation.Location = new Point(14, 163);
            panel_SkillRotation.Margin = new Padding(5, 4, 5, 4);
            panel_SkillRotation.Name = "panel_SkillRotation";
            panel_SkillRotation.Size = new Size(1385, 131);
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
            panel_Controls.Location = new Point(16, 14);
            panel_Controls.Margin = new Padding(5, 4, 5, 4);
            panel_Controls.Name = "panel_Controls";
            panel_Controls.Size = new Size(1383, 117);
            panel_Controls.TabIndex = 1;
            // 
            // button_Close
            // 
            button_Close.Location = new Point(1210, 49);
            button_Close.Margin = new Padding(5, 4, 5, 4);
            button_Close.Name = "button_Close";
            button_Close.Size = new Size(126, 42);
            button_Close.TabIndex = 5;
            button_Close.Text = "关闭窗口";
            button_Close.Type = AntdUI.TTypeMini.Error;
            button_Close.Click += button_Close_Click;
            // 
            // button_RefreshPlayers
            // 
            button_RefreshPlayers.Location = new Point(1069, 49);
            button_RefreshPlayers.Margin = new Padding(5, 4, 5, 4);
            button_RefreshPlayers.Name = "button_RefreshPlayers";
            button_RefreshPlayers.Size = new Size(126, 42);
            button_RefreshPlayers.TabIndex = 4;
            button_RefreshPlayers.Text = "刷新列表";
            button_RefreshPlayers.Click += button_RefreshPlayers_Click;
            // 
            // button_Clear
            // 
            button_Clear.Location = new Point(927, 49);
            button_Clear.Margin = new Padding(5, 4, 5, 4);
            button_Clear.Name = "button_Clear";
            button_Clear.Size = new Size(126, 42);
            button_Clear.TabIndex = 3;
            button_Clear.Text = "清空数据";
            button_Clear.Click += button_Clear_Click;
            // 
            // button_StartStop
            // 
            button_StartStop.Location = new Point(786, 49);
            button_StartStop.Margin = new Padding(5, 4, 5, 4);
            button_StartStop.Name = "button_StartStop";
            button_StartStop.Size = new Size(126, 42);
            button_StartStop.TabIndex = 2;
            button_StartStop.Text = "开始监控";
            button_StartStop.Type = AntdUI.TTypeMini.Primary;
            button_StartStop.Click += button_StartStop_Click;
            // 
            // dropdown_PlayerSelect
            // 
            dropdown_PlayerSelect.Location = new Point(126, 49);
            dropdown_PlayerSelect.Margin = new Padding(5, 4, 5, 4);
            dropdown_PlayerSelect.Name = "dropdown_PlayerSelect";
            dropdown_PlayerSelect.Size = new Size(629, 42);
            dropdown_PlayerSelect.TabIndex = 1;
            dropdown_PlayerSelect.SelectedValueChanged += dropdown_PlayerSelect_SelectedValueChanged;
            // 
            // label_SelectPlayer
            // 
            label_SelectPlayer.Location = new Point(0, 49);
            label_SelectPlayer.Margin = new Padding(5, 4, 5, 4);
            label_SelectPlayer.Name = "label_SelectPlayer";
            label_SelectPlayer.Size = new Size(126, 42);
            label_SelectPlayer.TabIndex = 0;
            label_SelectPlayer.Text = "选择玩家:";
            // 
            // panel_Stats
            // 
            panel_Stats.Controls.Add(label_AvgInterval);
            panel_Stats.Controls.Add(label_LastSkillTime);
            panel_Stats.Controls.Add(label_TotalSkills);
            panel_Stats.Controls.Add(label_PlayerName);
            panel_Stats.Location = new Point(21, 728);
            panel_Stats.Margin = new Padding(5, 4, 5, 4);
            panel_Stats.Name = "panel_Stats";
            panel_Stats.Size = new Size(1383, 85);
            panel_Stats.TabIndex = 0;
            // 
            // label_AvgInterval
            // 
            label_AvgInterval.Anchor = AnchorStyles.Bottom;
            label_AvgInterval.Location = new Point(1037, 8);
            label_AvgInterval.Margin = new Padding(5, 4, 5, 4);
            label_AvgInterval.Name = "label_AvgInterval";
            label_AvgInterval.Size = new Size(314, 71);
            label_AvgInterval.TabIndex = 3;
            label_AvgInterval.Text = "平均间隔: 无";
            // 
            // label_LastSkillTime
            // 
            label_LastSkillTime.Anchor = AnchorStyles.Bottom;
            label_LastSkillTime.Location = new Point(691, 8);
            label_LastSkillTime.Margin = new Padding(5, 4, 5, 4);
            label_LastSkillTime.Name = "label_LastSkillTime";
            label_LastSkillTime.Size = new Size(314, 71);
            label_LastSkillTime.TabIndex = 2;
            label_LastSkillTime.Text = "最后技能: 无";
            // 
            // label_TotalSkills
            // 
            label_TotalSkills.Anchor = AnchorStyles.Bottom;
            label_TotalSkills.Location = new Point(346, 8);
            label_TotalSkills.Margin = new Padding(5, 4, 5, 4);
            label_TotalSkills.Name = "label_TotalSkills";
            label_TotalSkills.Size = new Size(314, 71);
            label_TotalSkills.TabIndex = 1;
            label_TotalSkills.Text = "技能总数: 0";
            // 
            // label_PlayerName
            // 
            label_PlayerName.Anchor = AnchorStyles.Bottom;
            label_PlayerName.Location = new Point(0, 8);
            label_PlayerName.Margin = new Padding(5, 4, 5, 4);
            label_PlayerName.Name = "label_PlayerName";
            label_PlayerName.Size = new Size(314, 71);
            label_PlayerName.TabIndex = 0;
            label_PlayerName.Text = "玩家: 未选择";
            // 
            // SkillRotationMonitorForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1414, 847);
            Controls.Add(panel_Main);
            Margin = new Padding(5, 4, 5, 4);
            MaximizeBox = false;
            Name = "SkillRotationMonitorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "技能释放循环监测";
            Load += SkillRotationMonitorForm_Load;
            panel_Main.ResumeLayout(false);
            panel_Controls.ResumeLayout(false);
            panel_Stats.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel_Main;
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