namespace StarResonanceDpsAnalysis.Forms
{
    partial class DpsStatisticsForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DpsStatisticsForm));
            pageHeader1 = new AntdUI.PageHeader();
            PilingModeCheckbox = new AntdUI.Checkbox();
            button2 = new AntdUI.Button();
            button3 = new AntdUI.Button();
            RightHandoffButton = new AntdUI.Button();
            button_AlwaysOnTop = new AntdUI.Button();
            button1 = new AntdUI.Button();
            button_Settings = new AntdUI.Button();
            DamageModeLabel = new AntdUI.Label();
            LeftHandoffButton = new AntdUI.Button();
            panel1 = new AntdUI.Panel();
            BattleTimeText = new AntdUI.Label();
            timer_RefreshRunningTime = new System.Windows.Forms.Timer(components);
            timer1 = new System.Windows.Forms.Timer(components);
            sortedProgressBarList1 = new StarResonanceDpsAnalysis.Control.SortedProgressBarList();
            pageHeader1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(PilingModeCheckbox);
            pageHeader1.Controls.Add(button2);
            pageHeader1.Controls.Add(button3);
            pageHeader1.Controls.Add(RightHandoffButton);
            pageHeader1.Controls.Add(button_AlwaysOnTop);
            pageHeader1.Controls.Add(button1);
            pageHeader1.Controls.Add(button_Settings);
            pageHeader1.Controls.Add(DamageModeLabel);
            pageHeader1.Controls.Add(LeftHandoffButton);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Font = new Font("SAO Welcome TT", 9F);
            pageHeader1.ForeColor = Color.White;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.MinimizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(471, 35);
            pageHeader1.SubFont = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold);
            pageHeader1.SubGap = 0;
            pageHeader1.SubText = "";
            pageHeader1.TabIndex = 16;
            pageHeader1.Text = "";
            // 
            // PilingModeCheckbox
            // 
            PilingModeCheckbox.AutoSizeMode = AntdUI.TAutoSize.Width;
            PilingModeCheckbox.BackColor = Color.Transparent;
            PilingModeCheckbox.Dock = DockStyle.Right;
            PilingModeCheckbox.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            PilingModeCheckbox.ForeColor = Color.White;
            PilingModeCheckbox.Location = new Point(246, 0);
            PilingModeCheckbox.Name = "PilingModeCheckbox";
            PilingModeCheckbox.Size = new Size(120, 35);
            PilingModeCheckbox.TabIndex = 17;
            PilingModeCheckbox.Text = "打桩模式";
            PilingModeCheckbox.Visible = false;
            PilingModeCheckbox.CheckedChanged += PilingModeCheckbox_CheckedChanged;
            // 
            // button2
            // 
            button2.ColorScheme = AntdUI.TAMode.Dark;
            button2.Dock = DockStyle.Right;
            button2.Ghost = true;
            button2.IconRatio = 0.8F;
            button2.IconSvg = resources.GetString("button2.IconSvg");
            button2.Location = new Point(366, 0);
            button2.Name = "button2";
            button2.Size = new Size(24, 35);
            button2.TabIndex = 20;
            button2.ToggleIconSvg = "";
            button2.Click += button2_Click_1;
            // 
            // button3
            // 
            button3.Dock = DockStyle.Left;
            button3.Ghost = true;
            button3.Icon = Properties.Resources.handoff_normal;
            button3.IconHover = Properties.Resources.handoff_hover;
            button3.IconRatio = 0.8F;
            button3.Location = new Point(163, 0);
            button3.Name = "button3";
            button3.Size = new Size(29, 35);
            button3.TabIndex = 19;
            button3.Click += button3_Click;
            button3.MouseEnter += button3_MouseEnter;
            // 
            // RightHandoffButton
            // 
            RightHandoffButton.Dock = DockStyle.Left;
            RightHandoffButton.Ghost = true;
            RightHandoffButton.Icon = Properties.Resources.right_normal;
            RightHandoffButton.IconHover = Properties.Resources.right_hover;
            RightHandoffButton.IconRatio = 0.6F;
            RightHandoffButton.Location = new Point(134, 0);
            RightHandoffButton.Name = "RightHandoffButton";
            RightHandoffButton.Size = new Size(29, 35);
            RightHandoffButton.TabIndex = 18;
            RightHandoffButton.Click += RightHandoffButton_Click;
            // 
            // button_AlwaysOnTop
            // 
            button_AlwaysOnTop.ColorScheme = AntdUI.TAMode.Dark;
            button_AlwaysOnTop.Dock = DockStyle.Right;
            button_AlwaysOnTop.Ghost = true;
            button_AlwaysOnTop.IconRatio = 0.8F;
            button_AlwaysOnTop.IconSvg = resources.GetString("button_AlwaysOnTop.IconSvg");
            button_AlwaysOnTop.Location = new Point(390, 0);
            button_AlwaysOnTop.Name = "button_AlwaysOnTop";
            button_AlwaysOnTop.Size = new Size(26, 35);
            button_AlwaysOnTop.TabIndex = 5;
            button_AlwaysOnTop.ToggleIconSvg = resources.GetString("button_AlwaysOnTop.ToggleIconSvg");
            button_AlwaysOnTop.Click += button_AlwaysOnTop_Click;
            button_AlwaysOnTop.MouseEnter += button_AlwaysOnTop_MouseEnter;
            // 
            // button1
            // 
            button1.ColorScheme = AntdUI.TAMode.Dark;
            button1.Dock = DockStyle.Right;
            button1.Ghost = true;
            button1.IconRatio = 0.8F;
            button1.IconSvg = resources.GetString("button1.IconSvg");
            button1.Location = new Point(416, 0);
            button1.Name = "button1";
            button1.Size = new Size(24, 35);
            button1.TabIndex = 4;
            button1.ToggleIconSvg = "";
            button1.Click += button1_Click;
            button1.MouseEnter += button1_MouseEnter;
            // 
            // button_Settings
            // 
            button_Settings.BackActive = Color.Transparent;
            button_Settings.BackColor = Color.Transparent;
            button_Settings.ColorScheme = AntdUI.TAMode.Dark;
            button_Settings.DefaultBack = Color.Transparent;
            button_Settings.Dock = DockStyle.Right;
            button_Settings.Ghost = true;
            button_Settings.Icon = Properties.Resources.setting_hover;
            button_Settings.IconRatio = 1F;
            button_Settings.IconSvg = "";
            button_Settings.Location = new Point(440, 0);
            button_Settings.Name = "button_Settings";
            button_Settings.Size = new Size(31, 35);
            button_Settings.TabIndex = 3;
            button_Settings.ToggleIconSvg = "";
            button_Settings.Click += button_Settings_Click;
            // 
            // DamageModeLabel
            // 
            DamageModeLabel.BackColor = Color.Transparent;
            DamageModeLabel.Dock = DockStyle.Left;
            DamageModeLabel.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            DamageModeLabel.ForeColor = Color.White;
            DamageModeLabel.Location = new Point(45, 0);
            DamageModeLabel.Name = "DamageModeLabel";
            DamageModeLabel.Size = new Size(89, 35);
            DamageModeLabel.TabIndex = 18;
            DamageModeLabel.Text = "单次伤害";
            // 
            // LeftHandoffButton
            // 
            LeftHandoffButton.Dock = DockStyle.Left;
            LeftHandoffButton.Ghost = true;
            LeftHandoffButton.Icon = Properties.Resources.left_normal;
            LeftHandoffButton.IconHover = Properties.Resources.left_hover;
            LeftHandoffButton.IconRatio = 0.6F;
            LeftHandoffButton.Location = new Point(0, 0);
            LeftHandoffButton.Name = "LeftHandoffButton";
            LeftHandoffButton.Size = new Size(45, 35);
            LeftHandoffButton.TabIndex = 17;
            LeftHandoffButton.Click += LeftHandoffButton_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(BattleTimeText);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 419);
            panel1.Name = "panel1";
            panel1.Radius = 3;
            panel1.Size = new Size(471, 46);
            panel1.TabIndex = 17;
            panel1.Text = "panel1";
            // 
            // BattleTimeText
            // 
            BattleTimeText.Dock = DockStyle.Right;
            BattleTimeText.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            BattleTimeText.Location = new Point(383, 0);
            BattleTimeText.Name = "BattleTimeText";
            BattleTimeText.Size = new Size(88, 46);
            BattleTimeText.TabIndex = 18;
            BattleTimeText.Text = "00:00:00";
            // 
            // timer_RefreshRunningTime
            // 
            timer_RefreshRunningTime.Enabled = true;
            timer_RefreshRunningTime.Interval = 10;
            timer_RefreshRunningTime.Tick += timer_RefreshRunningTime_Tick;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // sortedProgressBarList1
            // 
            sortedProgressBarList1.AnimationQuality = Effects.Enum.Quality.Medium;
            sortedProgressBarList1.BackColor = Color.FromArgb(224, 224, 224);
            sortedProgressBarList1.Dock = DockStyle.Fill;
            sortedProgressBarList1.Location = new Point(0, 35);
            sortedProgressBarList1.Margin = new Padding(5, 4, 5, 4);
            sortedProgressBarList1.Name = "sortedProgressBarList1";
            sortedProgressBarList1.OrderColor = Color.Black;
            sortedProgressBarList1.OrderFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            sortedProgressBarList1.SeletedItemColor = Color.FromArgb(86, 156, 214);
            sortedProgressBarList1.Size = new Size(471, 384);
            sortedProgressBarList1.TabIndex = 18;
            // 
            // DpsStatisticsForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BorderWidth = 0;
            ClientSize = new Size(471, 465);
            Controls.Add(sortedProgressBarList1);
            Controls.Add(panel1);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "DpsStatisticsForm";
            Opacity = 0.85D;
            Radius = 3;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "别查我DPS";
            Load += DpsStatistics_Load;
            ForeColorChanged += DpsStatisticsForm_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            pageHeader1.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Button button_Settings;
        private AntdUI.Button button1;
        private AntdUI.Button button_AlwaysOnTop;
        private AntdUI.Button LeftHandoffButton;
        private AntdUI.Button RightHandoffButton;
        private AntdUI.Button button3;
        private AntdUI.Button button2;
        private AntdUI.Checkbox PilingModeCheckbox;
        private AntdUI.Panel panel1;
        private AntdUI.Label BattleTimeText;
        private System.Windows.Forms.Timer timer_RefreshRunningTime;
        private System.Windows.Forms.Timer timer1;
        private AntdUI.Label DamageModeLabel;
        private Control.SortedProgressBarList sortedProgressBarList1;
    }
}