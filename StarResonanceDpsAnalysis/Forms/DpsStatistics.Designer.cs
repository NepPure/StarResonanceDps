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
            button2 = new AntdUI.Button();
            button_AlwaysOnTop = new AntdUI.Button();
            button1 = new AntdUI.Button();
            button_Settings = new AntdUI.Button();
            pageHeader1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = SystemColors.ActiveBorder;
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(button2);
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
            pageHeader1.ShowBack = true;
            pageHeader1.Size = new Size(691, 45);
            pageHeader1.SubFont = new Font("Microsoft YaHei UI", 7F, FontStyle.Bold);
            pageHeader1.SubText = "秒伤";
            pageHeader1.TabIndex = 16;
            pageHeader1.Text = "Damage Per Second";
            // 
            // button2
            // 
            button2.ColorScheme = AntdUI.TAMode.Dark;
            button2.Dock = DockStyle.Right;
            button2.Ghost = true;
            button2.IconRatio = 1F;
            button2.IconSvg = resources.GetString("button2.IconSvg");
            button2.Location = new Point(495, 0);
            button2.Name = "button2";
            button2.Size = new Size(49, 45);
            button2.TabIndex = 6;
            button2.ToggleIconSvg = "";
            // 
            // button_AlwaysOnTop
            // 
            button_AlwaysOnTop.ColorScheme = AntdUI.TAMode.Dark;
            button_AlwaysOnTop.Dock = DockStyle.Right;
            button_AlwaysOnTop.Ghost = true;
            button_AlwaysOnTop.IconSvg = resources.GetString("button_AlwaysOnTop.IconSvg");
            button_AlwaysOnTop.Location = new Point(544, 0);
            button_AlwaysOnTop.Name = "button_AlwaysOnTop";
            button_AlwaysOnTop.Size = new Size(49, 45);
            button_AlwaysOnTop.TabIndex = 5;
            button_AlwaysOnTop.ToggleIconSvg = resources.GetString("button_AlwaysOnTop.ToggleIconSvg");
            // 
            // button1
            // 
            button1.ColorScheme = AntdUI.TAMode.Dark;
            button1.Dock = DockStyle.Right;
            button1.Ghost = true;
            button1.IconRatio = 1F;
            button1.IconSvg = resources.GetString("button1.IconSvg");
            button1.Location = new Point(593, 0);
            button1.Name = "button1";
            button1.Size = new Size(49, 45);
            button1.TabIndex = 4;
            button1.ToggleIconSvg = "";
            // 
            // button_Settings
            // 
            button_Settings.ColorScheme = AntdUI.TAMode.Dark;
            button_Settings.Dock = DockStyle.Right;
            button_Settings.Ghost = true;
            button_Settings.IconRatio = 1F;
            button_Settings.IconSvg = resources.GetString("button_Settings.IconSvg");
            button_Settings.Location = new Point(642, 0);
            button_Settings.Name = "button_Settings";
            button_Settings.Size = new Size(49, 45);
            button_Settings.TabIndex = 3;
            button_Settings.ToggleIconSvg = "";
            // 
            // DpsStatistics
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(230, 230, 230);
            BorderWidth = 0;
            ClientSize = new Size(691, 364);
            Controls.Add(pageHeader1);
            Name = "DpsStatistics";
            Opacity = 0.9D;
            Text = "DpsStatistics";
            Load += DpsStatistics_Load;
            pageHeader1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Button button_Settings;
        private AntdUI.Button button1;
        private AntdUI.Button button_AlwaysOnTop;
        private AntdUI.Button button2;
    }
}