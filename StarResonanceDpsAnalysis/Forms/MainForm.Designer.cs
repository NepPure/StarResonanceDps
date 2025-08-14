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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            pageHeader_MainHeader = new AntdUI.PageHeader();
            button_SkillDiary = new AntdUI.Button();
            dropdown_History = new AntdUI.Dropdown();
            button_AlwaysOnTop = new AntdUI.Button();
            button_ThemeSwitch = new AntdUI.Button();
            button_Settings = new AntdUI.Button();
            label_SettingTip = new AntdUI.Label();
            table_DpsDataTable = new AntdUI.Table();
            timer_RefreshDpsTable = new System.Windows.Forms.Timer(components);
            label_HotKeyTips = new AntdUI.Label();
            timer_RefreshRunningTime = new System.Windows.Forms.Timer(components);
            switch_IsMonitoring = new AntdUI.Switch();
            panel_FooterPanel = new AntdUI.Panel();
            pageHeader_MainHeader.SuspendLayout();
            panel_FooterPanel.SuspendLayout();
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
            // label_SettingTip
            // 
            label_SettingTip.AutoSizeMode = AntdUI.TAutoSize.Width;
            label_SettingTip.Dock = DockStyle.Right;
            label_SettingTip.ForeColor = Color.Red;
            label_SettingTip.Location = new Point(826, 0);
            label_SettingTip.Margin = new Padding(2, 1, 2, 1);
            label_SettingTip.Name = "label_SettingTip";
            label_SettingTip.Size = new Size(252, 30);
            label_SettingTip.TabIndex = 16;
            label_SettingTip.Text = "请先右上角设置网卡在启动哟！";
            label_SettingTip.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // table_DpsDataTable
            // 
            table_DpsDataTable.Dock = DockStyle.Fill;
            table_DpsDataTable.FixedHeader = false;
            table_DpsDataTable.Gap = 8;
            table_DpsDataTable.Gaps = new Size(8, 8);
            table_DpsDataTable.Location = new Point(0, 43);
            table_DpsDataTable.Name = "table_DpsDataTable";
            table_DpsDataTable.Size = new Size(1191, 619);
            table_DpsDataTable.TabIndex = 13;
            table_DpsDataTable.Text = "table1";
            table_DpsDataTable.CellClick += table_DpsDataTable_CellClick;
            table_DpsDataTable.SortRows += table_DpsDataTable_SortRows;
            // 
            // timer_RefreshDpsTable
            // 
            timer_RefreshDpsTable.Interval = 600;
            timer_RefreshDpsTable.Tick += timer_RefreshDpsTable_Tick;
            // 
            // label_HotKeyTips
            // 
            label_HotKeyTips.AutoSizeMode = AntdUI.TAutoSize.Width;
            label_HotKeyTips.Dock = DockStyle.Left;
            label_HotKeyTips.Location = new Point(9, 0);
            label_HotKeyTips.Margin = new Padding(2, 1, 2, 1);
            label_HotKeyTips.Name = "label_HotKeyTips";
            label_HotKeyTips.Size = new Size(631, 30);
            label_HotKeyTips.TabIndex = 14;
            label_HotKeyTips.Text = "F6：鼠标穿透 | F7：窗体透明 | F8：开启/关闭 | F9：清空数据 | F10：清空历史";
            // 
            // timer_RefreshRunningTime
            // 
            timer_RefreshRunningTime.Tick += timer_RefreshRunningTime_Tick;
            // 
            // switch_IsMonitoring
            // 
            switch_IsMonitoring.CheckedText = "开启中";
            switch_IsMonitoring.Dock = DockStyle.Right;
            switch_IsMonitoring.Location = new Point(1078, 0);
            switch_IsMonitoring.Name = "switch_IsMonitoring";
            switch_IsMonitoring.Size = new Size(113, 30);
            switch_IsMonitoring.TabIndex = 18;
            switch_IsMonitoring.UnCheckedText = "关闭中";
            switch_IsMonitoring.CheckedChanged += switch_IsMonitoring_CheckedChanged;
            // 
            // panel_FooterPanel
            // 
            panel_FooterPanel.Back = Color.Transparent;
            panel_FooterPanel.BackColor = Color.Transparent;
            panel_FooterPanel.Controls.Add(label_HotKeyTips);
            panel_FooterPanel.Controls.Add(label_SettingTip);
            panel_FooterPanel.Controls.Add(switch_IsMonitoring);
            panel_FooterPanel.Dock = DockStyle.Bottom;
            panel_FooterPanel.Location = new Point(0, 632);
            panel_FooterPanel.Name = "panel_FooterPanel";
            panel_FooterPanel.Padding = new Padding(9, 0, 0, 0);
            panel_FooterPanel.Size = new Size(1191, 30);
            panel_FooterPanel.TabIndex = 19;
            panel_FooterPanel.Text = "panel1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1191, 662);
            Controls.Add(panel_FooterPanel);
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
            panel_FooterPanel.ResumeLayout(false);
            panel_FooterPanel.PerformLayout();
            ResumeLayout(false);
        }
        private AntdUI.PageHeader pageHeader_MainHeader;
        private AntdUI.Table table_DpsDataTable;
        private System.Windows.Forms.Timer timer_RefreshDpsTable;
        private AntdUI.Button button_ThemeSwitch;
        private AntdUI.Button button_AlwaysOnTop;
        private AntdUI.Button button_Settings;
        private AntdUI.Label label_HotKeyTips;
        public AntdUI.Label label_SettingTip;
        private AntdUI.Dropdown dropdown_History;
        private AntdUI.Button button_SkillDiary;
        private System.Windows.Forms.Timer timer_RefreshRunningTime;
        private AntdUI.Switch switch_IsMonitoring;
        private AntdUI.Panel panel_FooterPanel;
    }
}
