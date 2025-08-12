namespace StarResonanceDpsAnalysis.Forms
{
    partial class DpsStatistics
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DpsStatistics));
            pageHeader1 = new AntdUI.PageHeader();
            checkbox1 = new AntdUI.Checkbox();
            button2 = new AntdUI.Button();
            button3 = new AntdUI.Button();
            RightHandoffButton = new AntdUI.Button();
            LeftHandoffButton = new AntdUI.Button();
            button_AlwaysOnTop = new AntdUI.Button();
            button1 = new AntdUI.Button();
            button_Settings = new AntdUI.Button();
            panel1 = new AntdUI.Panel();
            label1 = new AntdUI.Label();
            pageHeader1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(checkbox1);
            pageHeader1.Controls.Add(button2);
            pageHeader1.Controls.Add(button3);
            pageHeader1.Controls.Add(RightHandoffButton);
            pageHeader1.Controls.Add(LeftHandoffButton);
            pageHeader1.Controls.Add(button_AlwaysOnTop);
            pageHeader1.Controls.Add(button1);
            pageHeader1.Controls.Add(button_Settings);
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
            pageHeader1.Size = new Size(463, 33);
            pageHeader1.SubFont = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold);
            pageHeader1.SubGap = 0;
            pageHeader1.SubText = "单次伤害";
            pageHeader1.TabIndex = 16;
            pageHeader1.Text = "      ";
            // 
            // checkbox1
            // 
            checkbox1.BackColor = Color.Transparent;
            checkbox1.Dock = DockStyle.Right;
            checkbox1.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkbox1.ForeColor = Color.White;
            checkbox1.Location = new Point(244, 0);
            checkbox1.Name = "checkbox1";
            checkbox1.Size = new Size(114, 33);
            checkbox1.TabIndex = 17;
            checkbox1.Text = "打桩模式";
            checkbox1.Visible = false;
            // 
            // button2
            // 
            button2.ColorScheme = AntdUI.TAMode.Dark;
            button2.Dock = DockStyle.Right;
            button2.Ghost = true;
            button2.IconRatio = 0.8F;
            button2.IconSvg = resources.GetString("button2.IconSvg");
            button2.Location = new Point(358, 0);
            button2.Name = "button2";
            button2.Size = new Size(24, 33);
            button2.TabIndex = 20;
            button2.ToggleIconSvg = "";
            // 
            // button3
            // 
            button3.Dock = DockStyle.Left;
            button3.Ghost = true;
            button3.Icon = Properties.Resources.handoff_normal;
            button3.IconHover = Properties.Resources.handoff_hover;
            button3.IconRatio = 0.8F;
            button3.Location = new Point(134, 0);
            button3.Name = "button3";
            button3.Size = new Size(29, 33);
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
            RightHandoffButton.Location = new Point(105, 0);
            RightHandoffButton.Name = "RightHandoffButton";
            RightHandoffButton.Size = new Size(29, 33);
            RightHandoffButton.TabIndex = 18;
            RightHandoffButton.Click += RightHandoffButton_Click;
            // 
            // LeftHandoffButton
            // 
            LeftHandoffButton.Ghost = true;
            LeftHandoffButton.Icon = Properties.Resources.left_normal;
            LeftHandoffButton.IconHover = Properties.Resources.left_hover;
            LeftHandoffButton.IconRatio = 0.6F;
            LeftHandoffButton.Location = new Point(-3, 0);
            LeftHandoffButton.Name = "LeftHandoffButton";
            LeftHandoffButton.Size = new Size(45, 33);
            LeftHandoffButton.TabIndex = 17;
            LeftHandoffButton.Click += LeftHandoffButton_Click;
            // 
            // button_AlwaysOnTop
            // 
            button_AlwaysOnTop.ColorScheme = AntdUI.TAMode.Dark;
            button_AlwaysOnTop.Dock = DockStyle.Right;
            button_AlwaysOnTop.Ghost = true;
            button_AlwaysOnTop.IconRatio = 0.8F;
            button_AlwaysOnTop.IconSvg = resources.GetString("button_AlwaysOnTop.IconSvg");
            button_AlwaysOnTop.Location = new Point(382, 0);
            button_AlwaysOnTop.Name = "button_AlwaysOnTop";
            button_AlwaysOnTop.Size = new Size(26, 33);
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
            button1.Location = new Point(408, 0);
            button1.Name = "button1";
            button1.Size = new Size(24, 33);
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
            button_Settings.Location = new Point(432, 0);
            button_Settings.Name = "button_Settings";
            button_Settings.Size = new Size(31, 33);
            button_Settings.TabIndex = 3;
            button_Settings.ToggleIconSvg = "";
            button_Settings.Click += button_Settings_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 323);
            panel1.Name = "panel1";
            panel1.Radius = 3;
            panel1.Shadow = 2;
            panel1.ShadowAlign = AntdUI.TAlignMini.Top;
            panel1.Size = new Size(463, 51);
            panel1.TabIndex = 17;
            panel1.Text = "panel1";
            // 
            // label1
            // 
            label1.Dock = DockStyle.Right;
            label1.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label1.Location = new Point(375, 3);
            label1.Name = "label1";
            label1.Size = new Size(88, 48);
            label1.TabIndex = 18;
            label1.Text = "00:00:00";
            label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // DpsStatistics
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BorderWidth = 0;
            ClientSize = new Size(463, 374);
            Controls.Add(panel1);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "DpsStatistics";
            Opacity = 0.9D;
            Radius = 3;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DpsStatistics";
            Load += DpsStatistics_Load;
            pageHeader1.ResumeLayout(false);
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
        private AntdUI.Checkbox checkbox1;
        private AntdUI.Panel panel1;
        private AntdUI.Label label1;
    }
}