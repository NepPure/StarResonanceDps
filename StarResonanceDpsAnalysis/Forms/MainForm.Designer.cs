namespace StarResonanceDpsAnalysis.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            pageHeader_MainHeader = new AntdUI.PageHeader();
            button1 = new AntdUI.Button();
            button_SkillDiary = new AntdUI.Button();
            dropdown_History = new AntdUI.Dropdown();
            button_AlwaysOnTop = new AntdUI.Button();
            button_ThemeSwitch = new AntdUI.Button();
            button_Settings = new AntdUI.Button();
            pageHeader_MainHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader_MainHeader
            // 
            pageHeader_MainHeader.Controls.Add(button1);
            pageHeader_MainHeader.Controls.Add(button_SkillDiary);
            pageHeader_MainHeader.Controls.Add(dropdown_History);
            pageHeader_MainHeader.Controls.Add(button_AlwaysOnTop);
            pageHeader_MainHeader.Controls.Add(button_ThemeSwitch);
            pageHeader_MainHeader.Controls.Add(button_Settings);
            pageHeader_MainHeader.DividerShow = true;
            pageHeader_MainHeader.DividerThickness = 2F;
            pageHeader_MainHeader.Dock = DockStyle.Top;
            pageHeader_MainHeader.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            pageHeader_MainHeader.Icon = (Image)resources.GetObject("pageHeader_MainHeader.Icon");
            pageHeader_MainHeader.Location = new Point(0, 0);
            pageHeader_MainHeader.MaximizeBox = false;
            pageHeader_MainHeader.Name = "pageHeader_MainHeader";
            pageHeader_MainHeader.ShowButton = true;
            pageHeader_MainHeader.Size = new Size(1191, 43);
            pageHeader_MainHeader.SubText = "2.0";
            pageHeader_MainHeader.TabIndex = 8;
            pageHeader_MainHeader.Text = "别查我DPS";
            // 
            // button1
            // 
            button1.Dock = DockStyle.Right;
            button1.Ghost = true;
            button1.IconRatio = 1F;
            button1.IconSvg = resources.GetString("button1.IconSvg");
            button1.Location = new Point(763, 0);
            button1.Name = "button1";
            button1.Size = new Size(49, 43);
            button1.TabIndex = 18;
            button1.ToggleIconSvg = "";
            button1.Click += button1_Click;
            // 
            // button_SkillDiary
            // 
            button_SkillDiary.Dock = DockStyle.Right;
            button_SkillDiary.Ghost = true;
            button_SkillDiary.IconRatio = 1F;
            button_SkillDiary.IconSvg = resources.GetString("button_SkillDiary.IconSvg");
            button_SkillDiary.Location = new Point(812, 0);
            button_SkillDiary.Name = "button_SkillDiary";
            button_SkillDiary.Size = new Size(49, 43);
            button_SkillDiary.TabIndex = 17;
            button_SkillDiary.ToggleIconSvg = "";
            button_SkillDiary.Click += button_SkillDiary_Click;
            // 
            // dropdown_History
            // 
            dropdown_History.Dock = DockStyle.Right;
            dropdown_History.IconRatio = 1F;
            dropdown_History.IconSvg = resources.GetString("dropdown_History.IconSvg");
            dropdown_History.Location = new Point(861, 0);
            dropdown_History.MaxCount = 100;
            dropdown_History.Name = "dropdown_History";
            dropdown_History.Size = new Size(39, 43);
            dropdown_History.TabIndex = 16;
            dropdown_History.Trigger = AntdUI.Trigger.Hover;
            dropdown_History.SelectedValueChanged += dropdown_History_SelectedValueChanged;
            // 
            // button_AlwaysOnTop
            // 
            button_AlwaysOnTop.Dock = DockStyle.Right;
            button_AlwaysOnTop.Ghost = true;
            button_AlwaysOnTop.IconSvg = resources.GetString("button_AlwaysOnTop.IconSvg");
            button_AlwaysOnTop.Location = new Point(900, 0);
            button_AlwaysOnTop.Name = "button_AlwaysOnTop";
            button_AlwaysOnTop.Size = new Size(49, 43);
            button_AlwaysOnTop.TabIndex = 1;
            button_AlwaysOnTop.ToggleIconSvg = resources.GetString("button_AlwaysOnTop.ToggleIconSvg");
            button_AlwaysOnTop.Click += button_AlwaysOnTop_Click;
            // 
            // button_ThemeSwitch
            // 
            button_ThemeSwitch.Dock = DockStyle.Right;
            button_ThemeSwitch.Ghost = true;
            button_ThemeSwitch.IconSvg = "SunOutlined";
            button_ThemeSwitch.Location = new Point(949, 0);
            button_ThemeSwitch.Name = "button_ThemeSwitch";
            button_ThemeSwitch.Size = new Size(49, 43);
            button_ThemeSwitch.TabIndex = 0;
            button_ThemeSwitch.ToggleIconSvg = "MoonOutlined";
            button_ThemeSwitch.Click += button_ThemeSwitch_Click;
            // 
            // button_Settings
            // 
            button_Settings.Dock = DockStyle.Right;
            button_Settings.Ghost = true;
            button_Settings.IconRatio = 1F;
            button_Settings.IconSvg = resources.GetString("button_Settings.IconSvg");
            button_Settings.Location = new Point(998, 0);
            button_Settings.Name = "button_Settings";
            button_Settings.Size = new Size(49, 43);
            button_Settings.TabIndex = 2;
            button_Settings.ToggleIconSvg = "";
            button_Settings.MouseClick += button_Settings_MouseClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1191, 662);
            Controls.Add(pageHeader_MainHeader);
            Dark = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(6);
            MaximizeBox = false;
            Mode = AntdUI.TAMode.Dark;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DPS 统计工具";
            Load += MainForm_Load;
            pageHeader_MainHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
        private AntdUI.PageHeader pageHeader_MainHeader;
        private AntdUI.Button button_ThemeSwitch;
        private AntdUI.Button button_AlwaysOnTop;
        private AntdUI.Button button_Settings;
        private AntdUI.Dropdown dropdown_History;
        private AntdUI.Button button_SkillDiary;
        private AntdUI.Button button1;
    }
}
