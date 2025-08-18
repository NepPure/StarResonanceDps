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
            input1 = new AntdUI.Input();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.ShowButton = true;
            pageHeader1.Size = new Size(727, 45);
            pageHeader1.TabIndex = 0;
            pageHeader1.Text = "技能日记";
            // 
            // input1
            // 
            input1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            input1.AutoScroll = true;
            input1.Location = new Point(12, 63);
            input1.Multiline = true;
            input1.Name = "input1";
            input1.Size = new Size(701, 600);
            input1.TabIndex = 1;
            // 
            // SkillDiary
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(727, 676);
            Controls.Add(input1);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SkillDiary";
            Text = "SkillDiary";
            Load += SkillDiary_Load;
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Input input1;
    }
}