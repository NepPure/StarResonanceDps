namespace StarResonanceDpsAnalysis.Forms
{
    partial class SettingsForm
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
            pageHeader1 = new AntdUI.PageHeader();
            panel1 = new AntdUI.Panel();
            panel2 = new AntdUI.Panel();
            label1 = new AntdUI.Label();
            panel3 = new AntdUI.Panel();
            panel4 = new AntdUI.Panel();
            label2 = new AntdUI.Label();
            button1 = new AntdUI.Button();
            divider3 = new AntdUI.Divider();
            InterfaceComboBox = new AntdUI.Select();
            panel5 = new AntdUI.Panel();
            divider1 = new AntdUI.Divider();
            button2 = new AntdUI.Button();
            label3 = new AntdUI.Label();
            input5 = new AntdUI.Input();
            input4 = new AntdUI.Input();
            input3 = new AntdUI.Input();
            input2 = new AntdUI.Input();
            input1 = new AntdUI.Input();
            panel6 = new AntdUI.Panel();
            divider2 = new AntdUI.Divider();
            button3 = new AntdUI.Button();
            label4 = new AntdUI.Label();
            input6 = new AntdUI.Input();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(1064, 38);
            pageHeader1.TabIndex = 29;
            pageHeader1.Text = "";
            // 
            // panel1
            // 
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 38);
            panel1.Name = "panel1";
            panel1.Size = new Size(265, 1082);
            panel1.TabIndex = 30;
            panel1.Text = "panel1";
            // 
            // panel2
            // 
            panel2.Back = Color.FromArgb(34, 151, 244);
            panel2.Controls.Add(label1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(265, 38);
            panel2.Name = "panel2";
            panel2.Radius = 0;
            panel2.Size = new Size(799, 51);
            panel2.TabIndex = 31;
            panel2.Text = "panel2";
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("阿里妈妈数黑体", 10F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label1.ForeColor = Color.White;
            label1.Location = new Point(19, 7);
            label1.Name = "label1";
            label1.Size = new Size(363, 31);
            label1.TabIndex = 32;
            label1.Text = "设置";
            // 
            // panel3
            // 
            panel3.Back = Color.FromArgb(239, 239, 239);
            panel3.Controls.Add(panel6);
            panel3.Controls.Add(panel5);
            panel3.Controls.Add(panel4);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(265, 89);
            panel3.Name = "panel3";
            panel3.Size = new Size(799, 1031);
            panel3.TabIndex = 32;
            panel3.Text = "panel3";
            // 
            // panel4
            // 
            panel4.Back = Color.White;
            panel4.Controls.Add(InterfaceComboBox);
            panel4.Controls.Add(divider3);
            panel4.Controls.Add(button1);
            panel4.Controls.Add(label2);
            panel4.Location = new Point(46, 45);
            panel4.Name = "panel4";
            panel4.Size = new Size(718, 211);
            panel4.TabIndex = 0;
            panel4.Text = "panel4";
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("HarmonyOS Sans SC", 9.999999F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label2.ForeColor = Color.FromArgb(34, 151, 244);
            label2.Location = new Point(36, 26);
            label2.Name = "label2";
            label2.Size = new Size(105, 31);
            label2.TabIndex = 33;
            label2.Text = "网卡设置";
            // 
            // button1
            // 
            button1.DefaultBack = Color.FromArgb(34, 151, 244);
            button1.Location = new Point(-9, 7);
            button1.Name = "button1";
            button1.Radius = 0;
            button1.Size = new Size(21, 70);
            button1.TabIndex = 34;
            // 
            // divider3
            // 
            divider3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider3.BackColor = Color.Transparent;
            divider3.Location = new Point(36, 78);
            divider3.Name = "divider3";
            divider3.OrientationMargin = 0F;
            divider3.Size = new Size(650, 14);
            divider3.TabIndex = 37;
            divider3.Text = "";
            // 
            // InterfaceComboBox
            // 
            InterfaceComboBox.Anchor = AnchorStyles.Top;
            InterfaceComboBox.Font = new Font("HarmonyOS Sans SC", 8F);
            InterfaceComboBox.List = true;
            InterfaceComboBox.Location = new Point(36, 114);
            InterfaceComboBox.Name = "InterfaceComboBox";
            InterfaceComboBox.PrefixText = "请选择网卡：";
            InterfaceComboBox.Radius = 3;
            InterfaceComboBox.Size = new Size(650, 56);
            InterfaceComboBox.TabIndex = 38;
            // 
            // panel5
            // 
            panel5.Back = Color.White;
            panel5.Controls.Add(input5);
            panel5.Controls.Add(input4);
            panel5.Controls.Add(input3);
            panel5.Controls.Add(input2);
            panel5.Controls.Add(input1);
            panel5.Controls.Add(divider1);
            panel5.Controls.Add(button2);
            panel5.Controls.Add(label3);
            panel5.Location = new Point(46, 295);
            panel5.Name = "panel5";
            panel5.Size = new Size(718, 303);
            panel5.TabIndex = 1;
            panel5.Text = "panel5";
            // 
            // divider1
            // 
            divider1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider1.BackColor = Color.Transparent;
            divider1.Location = new Point(36, 78);
            divider1.Name = "divider1";
            divider1.OrientationMargin = 0F;
            divider1.Size = new Size(650, 14);
            divider1.TabIndex = 37;
            divider1.Text = "";
            // 
            // button2
            // 
            button2.DefaultBack = Color.FromArgb(34, 151, 244);
            button2.Location = new Point(-9, 7);
            button2.Name = "button2";
            button2.Radius = 0;
            button2.Size = new Size(21, 70);
            button2.TabIndex = 34;
            // 
            // label3
            // 
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("HarmonyOS Sans SC", 9.999999F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label3.ForeColor = Color.FromArgb(34, 151, 244);
            label3.Location = new Point(36, 26);
            label3.Name = "label3";
            label3.Size = new Size(105, 31);
            label3.TabIndex = 33;
            label3.Text = "按键设置";
            // 
            // input5
            // 
            input5.Font = new Font("HarmonyOS Sans SC", 8F);
            input5.Location = new Point(36, 198);
            input5.Name = "input5";
            input5.PrefixText = "清空历史键位：";
            input5.Radius = 3;
            input5.ReadOnly = true;
            input5.Size = new Size(192, 65);
            input5.TabIndex = 42;
            // 
            // input4
            // 
            input4.Font = new Font("HarmonyOS Sans SC", 8F);
            input4.Location = new Point(265, 198);
            input4.Name = "input4";
            input4.PrefixText = "清空数据键位：";
            input4.Radius = 3;
            input4.ReadOnly = true;
            input4.Size = new Size(192, 65);
            input4.TabIndex = 41;
            // 
            // input3
            // 
            input3.Font = new Font("HarmonyOS Sans SC", 8F);
            input3.Location = new Point(494, 112);
            input3.Name = "input3";
            input3.PrefixText = "开关键位：";
            input3.Radius = 3;
            input3.ReadOnly = true;
            input3.Size = new Size(192, 65);
            input3.TabIndex = 40;
            // 
            // input2
            // 
            input2.Font = new Font("HarmonyOS Sans SC", 8F);
            input2.Location = new Point(265, 112);
            input2.Name = "input2";
            input2.PrefixText = "窗体透明键位：";
            input2.Radius = 3;
            input2.ReadOnly = true;
            input2.Size = new Size(192, 65);
            input2.TabIndex = 39;
            // 
            // input1
            // 
            input1.Font = new Font("HarmonyOS Sans SC", 8F);
            input1.Location = new Point(36, 112);
            input1.Name = "input1";
            input1.PrefixText = "鼠标穿透键位：";
            input1.Radius = 3;
            input1.ReadOnly = true;
            input1.Size = new Size(192, 65);
            input1.TabIndex = 38;
            // 
            // panel6
            // 
            panel6.Back = Color.White;
            panel6.Controls.Add(input6);
            panel6.Controls.Add(divider2);
            panel6.Controls.Add(button3);
            panel6.Controls.Add(label4);
            panel6.Location = new Point(46, 635);
            panel6.Name = "panel6";
            panel6.Size = new Size(718, 211);
            panel6.TabIndex = 2;
            panel6.Text = "panel6";
            // 
            // divider2
            // 
            divider2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider2.BackColor = Color.Transparent;
            divider2.Location = new Point(36, 78);
            divider2.Name = "divider2";
            divider2.OrientationMargin = 0F;
            divider2.Size = new Size(650, 14);
            divider2.TabIndex = 37;
            divider2.Text = "";
            // 
            // button3
            // 
            button3.DefaultBack = Color.FromArgb(34, 151, 244);
            button3.Location = new Point(-9, 7);
            button3.Name = "button3";
            button3.Radius = 0;
            button3.Size = new Size(21, 70);
            button3.TabIndex = 34;
            // 
            // label4
            // 
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("HarmonyOS Sans SC", 9.999999F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label4.ForeColor = Color.FromArgb(34, 151, 244);
            label4.Location = new Point(36, 26);
            label4.Name = "label4";
            label4.Size = new Size(105, 31);
            label4.TabIndex = 33;
            label4.Text = "战斗设置";
            // 
            // input6
            // 
            input6.Font = new Font("HarmonyOS Sans SC", 8F);
            input6.Location = new Point(36, 116);
            input6.Name = "input6";
            input6.PrefixText = "脱战";
            input6.Radius = 3;
            input6.ReadOnly = true;
            input6.Size = new Size(222, 65);
            input6.SuffixText = "/秒后清除数据";
            input6.TabIndex = 39;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1064, 1120);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(pageHeader1);
            Name = "SettingsForm";
            Text = "SettingsForm";
            Load += SettingsForm_Load;
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Panel panel1;
        private AntdUI.Panel panel2;
        private AntdUI.Label label1;
        private AntdUI.Panel panel3;
        private AntdUI.Panel panel4;
        private AntdUI.Label label2;
        private AntdUI.Button button1;
        private AntdUI.Divider divider3;
        public AntdUI.Select InterfaceComboBox;
        private AntdUI.Panel panel5;
        private AntdUI.Divider divider1;
        private AntdUI.Button button2;
        private AntdUI.Label label3;
        private AntdUI.Input input5;
        private AntdUI.Input input4;
        private AntdUI.Input input3;
        private AntdUI.Input input2;
        private AntdUI.Input input1;
        private AntdUI.Panel panel6;
        private AntdUI.Divider divider2;
        private AntdUI.Button button3;
        private AntdUI.Label label4;
        private AntdUI.Input input6;
    }
}