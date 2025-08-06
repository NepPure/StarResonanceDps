namespace 星痕共鸣DPS统计
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            pageHeader1 = new AntdUI.PageHeader();
            button1 = new AntdUI.Button();
            dropdown1 = new AntdUI.Dropdown();
            label2 = new AntdUI.Label();
            button3 = new AntdUI.Button();
            button2 = new AntdUI.Button();
            button4 = new AntdUI.Button();
            checkbox1 = new AntdUI.Checkbox();
            table1 = new AntdUI.Table();
            timer1 = new System.Windows.Forms.Timer(components);
            label1 = new AntdUI.Label();
            label3 = new AntdUI.Label();
            timer2 = new System.Windows.Forms.Timer(components);
            pageHeader1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.Controls.Add(button1);
            pageHeader1.Controls.Add(dropdown1);
            pageHeader1.Controls.Add(label2);
            pageHeader1.Controls.Add(button3);
            pageHeader1.Controls.Add(button2);
            pageHeader1.Controls.Add(button4);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Icon = (Image)resources.GetObject("pageHeader1.Icon");
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.ShowButton = true;
            pageHeader1.Size = new Size(1127, 46);
            pageHeader1.SubText = "";
            pageHeader1.TabIndex = 8;
            pageHeader1.Text = "DPS统计 - by: 惊奇猫猫盒 [别查我DPS] v1.0.1";
            // 
            // button1
            // 
            button1.Dock = DockStyle.Right;
            button1.Ghost = true;
            button1.IconRatio = 1F;
            button1.IconSvg = resources.GetString("button1.IconSvg");
            button1.Location = new Point(748, 0);
            button1.Name = "button1";
            button1.Size = new Size(49, 46);
            button1.TabIndex = 17;
            button1.ToggleIconSvg = "";
            button1.Click += button1_Click;
            // 
            // dropdown1
            // 
            dropdown1.Dock = DockStyle.Right;
            dropdown1.IconRatio = 1F;
            dropdown1.IconSvg = resources.GetString("dropdown1.IconSvg");
            dropdown1.Location = new Point(797, 0);
            dropdown1.MaxCount = 100;
            dropdown1.Name = "dropdown1";
            dropdown1.Size = new Size(39, 46);
            dropdown1.TabIndex = 16;
            dropdown1.Trigger = AntdUI.Trigger.Hover;
            dropdown1.SelectedValueChanged += dropdown1_SelectedValueChanged;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.ForeColor = Color.Red;
            label2.Location = new Point(532, 5);
            label2.Name = "label2";
            label2.Size = new Size(222, 31);
            label2.TabIndex = 16;
            label2.Text = "请先右边设置网卡在启动哟！";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button3
            // 
            button3.Dock = DockStyle.Right;
            button3.Ghost = true;
            button3.IconSvg = resources.GetString("button3.IconSvg");
            button3.Location = new Point(836, 0);
            button3.Name = "button3";
            button3.Size = new Size(49, 46);
            button3.TabIndex = 1;
            button3.ToggleIconSvg = resources.GetString("button3.ToggleIconSvg");
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Dock = DockStyle.Right;
            button2.Ghost = true;
            button2.IconSvg = "SunOutlined";
            button2.Location = new Point(885, 0);
            button2.Name = "button2";
            button2.Size = new Size(49, 46);
            button2.TabIndex = 0;
            button2.ToggleIconSvg = "MoonOutlined";
            button2.Click += button2_Click;
            // 
            // button4
            // 
            button4.Dock = DockStyle.Right;
            button4.Ghost = true;
            button4.IconRatio = 1F;
            button4.IconSvg = resources.GetString("button4.IconSvg");
            button4.Location = new Point(934, 0);
            button4.Name = "button4";
            button4.Size = new Size(49, 46);
            button4.TabIndex = 2;
            button4.ToggleIconSvg = "";
            button4.Click += button4_Click;
            // 
            // checkbox1
            // 
            checkbox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkbox1.Location = new Point(1002, 61);
            checkbox1.Name = "checkbox1";
            checkbox1.Size = new Size(124, 31);
            checkbox1.TabIndex = 15;
            checkbox1.Text = "占比数据";
            checkbox1.CheckedChanged += checkbox1_CheckedChanged;
            // 
            // table1
            // 
            table1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            table1.FixedHeader = false;
            table1.Gap = 8;
            table1.Gaps = new Size(8, 8);
            table1.Location = new Point(0, 105);
            table1.Name = "table1";
            table1.Size = new Size(1127, 555);
            table1.TabIndex = 13;
            table1.Text = "table1";
            // 
            // timer1
            // 
            timer1.Interval = 600;
            timer1.Tick += timer1_Tick;
            // 
            // label1
            // 
            label1.Location = new Point(12, 53);
            label1.Name = "label1";
            label1.Size = new Size(675, 45);
            label1.TabIndex = 14;
            label1.Text = "F6：鼠标穿透 | F7：窗体透明 | F8：开启/关闭 | F9：清空数据 | F10：清空历史";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.Location = new Point(761, 61);
            label3.Name = "label3";
            label3.Prefix = "战斗计时：";
            label3.Size = new Size(222, 31);
            label3.TabIndex = 17;
            label3.Text = "";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timer2
            // 
            timer2.Tick += timer2_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1127, 662);
            Controls.Add(label3);
            Controls.Add(checkbox1);
            Controls.Add(label1);
            Controls.Add(table1);
            Controls.Add(pageHeader1);
            Dark = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(6);
            MaximizeBox = false;
            Mode = AntdUI.TAMode.Dark;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DPS 统计工具";
            Load += Form1_Load_1;
            pageHeader1.ResumeLayout(false);
            ResumeLayout(false);
        }
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Table table1;
        private System.Windows.Forms.Timer timer1;
        private AntdUI.Button button2;
        private AntdUI.Button button3;
        private AntdUI.Button button4;
        private AntdUI.Label label1;
        private AntdUI.Checkbox checkbox1;
        public AntdUI.Label label2;
        private AntdUI.Dropdown dropdown1;
        private AntdUI.Button button1;
        public AntdUI.Label label3;
        private System.Windows.Forms.Timer timer2;
    }
}
