namespace StarResonanceDpsAnalysis
{
    partial class SkillDiary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillDiary));
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            select2 = new AntdUI.Select();
            button3 = new AntdUI.Button();
            button4 = new AntdUI.Button();
            richTextBox1 = new RichTextBox();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(TitleText);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(727, 52);
            pageHeader1.TabIndex = 16;
            pageHeader1.Text = "";
            // 
            // TitleText
            // 
            TitleText.BackColor = Color.Transparent;
            TitleText.ColorScheme = AntdUI.TAMode.Dark;
            TitleText.Dock = DockStyle.Fill;
            TitleText.Font = new Font("SAO Welcome TT", 12F, FontStyle.Bold);
            TitleText.Location = new Point(0, 0);
            TitleText.Name = "TitleText";
            TitleText.Size = new Size(727, 52);
            TitleText.TabIndex = 26;
            TitleText.Text = "Skill History";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
            // 
            // panel6
            // 
            panel6.Controls.Add(select2);
            panel6.Controls.Add(button3);
            panel6.Controls.Add(button4);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 789);
            panel6.Name = "panel6";
            panel6.Radius = 3;
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(727, 86);
            panel6.TabIndex = 33;
            panel6.Text = "panel6";
            // 
            // select2
            // 
            select2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select2.DropDownTextAlign = AntdUI.TAlign.Top;
            select2.List = true;
            select2.Location = new Point(3369, 44);
            select2.Name = "select2";
            select2.Radius = 0;
            select2.Size = new Size(204, 47);
            select2.TabIndex = 27;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom;
            button3.Ghost = true;
            button3.Icon = Properties.Resources.cancel_normal;
            button3.IconHover = Properties.Resources.cancel_hover;
            button3.IconPosition = AntdUI.TAlignMini.None;
            button3.IconRatio = 1.5F;
            button3.Location = new Point(400, 25);
            button3.Name = "button3";
            button3.Size = new Size(57, 49);
            button3.TabIndex = 1;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom;
            button4.Ghost = true;
            button4.Icon = Properties.Resources.flushed_normal;
            button4.IconHover = Properties.Resources.flushed_hover;
            button4.IconPosition = AntdUI.TAlignMini.None;
            button4.IconRatio = 1.5F;
            button4.Location = new Point(265, 25);
            button4.Name = "button4";
            button4.Size = new Size(57, 49);
            button4.TabIndex = 0;
            button4.Click += button4_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = Color.White;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 52);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(727, 737);
            richTextBox1.TabIndex = 34;
            richTextBox1.Text = "";
            // 
            // SkillDiary
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(727, 875);
            Controls.Add(richTextBox1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SkillDiary";
            Text = "SkillDiary";
            Load += SkillDiary_Load;
            ForeColorChanged += SkillDiary_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel6;
        private AntdUI.Select select2;
        private AntdUI.Button button3;
        private AntdUI.Button button4;
        private RichTextBox richTextBox1;
    }
}