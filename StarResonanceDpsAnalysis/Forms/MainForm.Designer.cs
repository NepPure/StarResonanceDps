namespace StarResonanceDpsAnalysis
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            pageHeader_MainHeader = new AntdUI.PageHeader();
            button_SkillDiary = new AntdUI.Button();
            dropdown_History = new AntdUI.Dropdown();
            button_AlwaysOnTop = new AntdUI.Button();
            button_ThemeSwitch = new AntdUI.Button();
            button_Settings = new AntdUI.Button();
            label_SettingTip = new AntdUI.Label();
            checkbox_PersentData = new AntdUI.Checkbox();
            table_DpsDataTable = new AntdUI.Table();
            timer_RefreshDpsTable = new System.Windows.Forms.Timer(components);
            label_HotKeyTips = new AntdUI.Label();
            timer_RefreshRunningTime = new System.Windows.Forms.Timer(components);
            switch1 = new AntdUI.Switch();
            panel1 = new AntdUI.Panel();
            pageHeader_MainHeader.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader_MainHeader
            // 
            pageHeader_MainHeader.Controls.Add(button_SkillDiary);
            pageHeader_MainHeader.Controls.Add(dropdown_History);
            pageHeader_MainHeader.Controls.Add(button_AlwaysOnTop);
            pageHeader_MainHeader.Controls.Add(button_ThemeSwitch);
            pageHeader_MainHeader.Controls.Add(button_Settings);
            pageHeader_MainHeader.DividerShow = true;
            pageHeader_MainHeader.DividerThickness = 2F;
            pageHeader_MainHeader.Dock = DockStyle.Top;
            pageHeader_MainHeader.Icon = (Image)resources.GetObject("pageHeader_MainHeader.Icon");
            pageHeader_MainHeader.Location = new Point(0, 0);
            pageHeader_MainHeader.MaximizeBox = false;
            pageHeader_MainHeader.Name = "pageHeader_MainHeader";
            pageHeader_MainHeader.ShowButton = true;
            pageHeader_MainHeader.Size = new Size(1127, 46);
            pageHeader_MainHeader.SubText = "";
            pageHeader_MainHeader.TabIndex = 8;
            pageHeader_MainHeader.Text = "DPS统计 - by: 惊奇猫猫盒 [别查我DPS] v1.0.2";
            // 
            // button_SkillDiary
            // 
            button_SkillDiary.Dock = DockStyle.Right;
            button_SkillDiary.Ghost = true;
            button_SkillDiary.IconRatio = 1F;
            button_SkillDiary.IconSvg = resources.GetString("button_SkillDiary.IconSvg");
            button_SkillDiary.Location = new Point(748, 0);
            button_SkillDiary.Name = "button_SkillDiary";
            button_SkillDiary.Size = new Size(49, 46);
            button_SkillDiary.TabIndex = 17;
            button_SkillDiary.ToggleIconSvg = "";
            button_SkillDiary.Click += button1_Click;
            // 
            // dropdown_History
            // 
            dropdown_History.Dock = DockStyle.Right;
            dropdown_History.IconRatio = 1F;
            dropdown_History.IconSvg = resources.GetString("dropdown_History.IconSvg");
            dropdown_History.Location = new Point(797, 0);
            dropdown_History.MaxCount = 100;
            dropdown_History.Name = "dropdown_History";
            dropdown_History.Size = new Size(39, 46);
            dropdown_History.TabIndex = 16;
            dropdown_History.Trigger = AntdUI.Trigger.Hover;
            dropdown_History.SelectedValueChanged += dropdown1_SelectedValueChanged;
            // 
            // button_AlwaysOnTop
            // 
            button_AlwaysOnTop.Dock = DockStyle.Right;
            button_AlwaysOnTop.Ghost = true;
            button_AlwaysOnTop.IconSvg = resources.GetString("button_AlwaysOnTop.IconSvg");
            button_AlwaysOnTop.Location = new Point(836, 0);
            button_AlwaysOnTop.Name = "button_AlwaysOnTop";
            button_AlwaysOnTop.Size = new Size(49, 46);
            button_AlwaysOnTop.TabIndex = 1;
            button_AlwaysOnTop.ToggleIconSvg = resources.GetString("button_AlwaysOnTop.ToggleIconSvg");
            button_AlwaysOnTop.Click += button3_Click;
            // 
            // button_ThemeSwitch
            // 
            button_ThemeSwitch.Dock = DockStyle.Right;
            button_ThemeSwitch.Ghost = true;
            button_ThemeSwitch.IconSvg = "SunOutlined";
            button_ThemeSwitch.Location = new Point(885, 0);
            button_ThemeSwitch.Name = "button_ThemeSwitch";
            button_ThemeSwitch.Size = new Size(49, 46);
            button_ThemeSwitch.TabIndex = 0;
            button_ThemeSwitch.ToggleIconSvg = "MoonOutlined";
            button_ThemeSwitch.Click += button2_Click;
            // 
            // button_Settings
            // 
            button_Settings.Dock = DockStyle.Right;
            button_Settings.Ghost = true;
            button_Settings.IconRatio = 1F;
            button_Settings.IconSvg = resources.GetString("button_Settings.IconSvg");
            button_Settings.Location = new Point(934, 0);
            button_Settings.Name = "button_Settings";
            button_Settings.Size = new Size(49, 46);
            button_Settings.TabIndex = 2;
            button_Settings.ToggleIconSvg = "";
            button_Settings.MouseClick += button4_MouseClick;
            // 
            // label_SettingTip
            // 
            label_SettingTip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label_SettingTip.ForeColor = Color.Red;
            label_SettingTip.Location = new Point(654, 0);
            label_SettingTip.Name = "label_SettingTip";
            label_SettingTip.Size = new Size(231, 42);
            label_SettingTip.TabIndex = 16;
            label_SettingTip.Text = "请先右上角设置网卡在启动哟！";
            label_SettingTip.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // checkbox_PersentData
            // 
            checkbox_PersentData.Dock = DockStyle.Right;
            checkbox_PersentData.Location = new Point(1003, 0);
            checkbox_PersentData.Name = "checkbox_PersentData";
            checkbox_PersentData.Size = new Size(124, 42);
            checkbox_PersentData.TabIndex = 15;
            checkbox_PersentData.Text = "占比数据";
            checkbox_PersentData.CheckedChanged += checkbox1_CheckedChanged;
            // 
            // table_DpsDataTable
            // 
            table_DpsDataTable.Dock = DockStyle.Fill;
            table_DpsDataTable.FixedHeader = false;
            table_DpsDataTable.Gap = 8;
            table_DpsDataTable.Gaps = new Size(8, 8);
            table_DpsDataTable.Location = new Point(0, 46);
            table_DpsDataTable.Name = "table_DpsDataTable";
            table_DpsDataTable.Size = new Size(1127, 616);
            table_DpsDataTable.TabIndex = 13;
            table_DpsDataTable.Text = "table1";
            // 
            // timer_RefreshDpsTable
            // 
            timer_RefreshDpsTable.Interval = 600;
            timer_RefreshDpsTable.Tick += timer1_Tick;
            // 
            // label_HotKeyTips
            // 
            label_HotKeyTips.Dock = DockStyle.Left;
            label_HotKeyTips.Location = new Point(0, 0);
            label_HotKeyTips.Name = "label_HotKeyTips";
            label_HotKeyTips.Size = new Size(654, 42);
            label_HotKeyTips.TabIndex = 14;
            label_HotKeyTips.Text = "F6：鼠标穿透 | F7：窗体透明 | F8：开启/关闭 | F9：清空数据 | F10：清空历史";
            // 
            // timer_RefreshRunningTime
            // 
            timer_RefreshRunningTime.Tick += timer2_Tick;
            // 
            // switch1
            // 
            switch1.CheckedText = "开启中";
            switch1.Dock = DockStyle.Right;
            switch1.Location = new Point(888, 0);
            switch1.Name = "switch1";
            switch1.Size = new Size(115, 42);
            switch1.TabIndex = 18;
            switch1.UnCheckedText = "关闭中";
            switch1.CheckedChanged += switch1_CheckedChanged;
            // 
            // panel1
            // 
            panel1.Back = Color.Transparent;
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(label_HotKeyTips);
            panel1.Controls.Add(label_SettingTip);
            panel1.Controls.Add(switch1);
            panel1.Controls.Add(checkbox_PersentData);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 620);
            panel1.Name = "panel1";
            panel1.Size = new Size(1127, 42);
            panel1.TabIndex = 19;
            panel1.Text = "panel1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1127, 662);
            Controls.Add(panel1);
            Controls.Add(table_DpsDataTable);
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
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }
        private AntdUI.PageHeader pageHeader_MainHeader;
        private AntdUI.Table table_DpsDataTable;
        private System.Windows.Forms.Timer timer_RefreshDpsTable;
        private AntdUI.Button button_ThemeSwitch;
        private AntdUI.Button button_AlwaysOnTop;
        private AntdUI.Button button_Settings;
        private AntdUI.Label label_HotKeyTips;
        private AntdUI.Checkbox checkbox_PersentData;
        public AntdUI.Label label_SettingTip;
        private AntdUI.Dropdown dropdown_History;
        private AntdUI.Button button_SkillDiary;
        private System.Windows.Forms.Timer timer_RefreshRunningTime;
        private AntdUI.Switch switch1;
        private AntdUI.Panel panel1;
    }
}
