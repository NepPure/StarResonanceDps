namespace StarResonanceDpsAnalysis.Control
{
    partial class SkillDetailForm
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
            AntdUI.SegmentedItem segmentedItem1 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem2 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem3 = new AntdUI.SegmentedItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillDetailForm));
            table_DpsDetailDataTable = new AntdUI.Table();
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            segmented1 = new AntdUI.Segmented();
            NickNameText = new AntdUI.Label();
            divider1 = new AntdUI.Divider();
            panel1 = new AntdUI.Panel();
            LuckyRate = new AntdUI.Label();
            TotalDpsText = new AntdUI.Label();
            CritRateText = new AntdUI.Label();
            label5 = new AntdUI.Label();
            label2 = new AntdUI.Label();
            TotalDamageText = new AntdUI.Label();
            label4 = new AntdUI.Label();
            label1 = new AntdUI.Label();
            label3 = new AntdUI.Label();
            panel2 = new AntdUI.Panel();
            AvgDamageText = new AntdUI.Label();
            CritDamageText = new AntdUI.Label();
            LuckyDamageText = new AntdUI.Label();
            label9 = new AntdUI.Label();
            label7 = new AntdUI.Label();
            NormalDamageText = new AntdUI.Label();
            label8 = new AntdUI.Label();
            label6 = new AntdUI.Label();
            divider2 = new AntdUI.Divider();
            label19 = new AntdUI.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            UidText = new AntdUI.Label();
            PowerText = new AntdUI.Label();
            divider3 = new AntdUI.Divider();
            panel3 = new AntdUI.Panel();
            panel5 = new AntdUI.Panel();
            select1 = new AntdUI.Select();
            panel6 = new AntdUI.Panel();
            button2 = new AntdUI.Button();
            button1 = new AntdUI.Button();
            splitter1 = new AntdUI.Splitter();
            collapse1 = new AntdUI.Collapse();
            collapseItem1 = new AntdUI.CollapseItem();
            collapseItem2 = new AntdUI.CollapseItem();
            collapseItem3 = new AntdUI.CollapseItem();
            pageHeader1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter1).BeginInit();
            splitter1.Panel1.SuspendLayout();
            splitter1.Panel2.SuspendLayout();
            splitter1.SuspendLayout();
            collapse1.SuspendLayout();
            SuspendLayout();
            // 
            // table_DpsDetailDataTable
            // 
            table_DpsDetailDataTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            table_DpsDetailDataTable.BackgroundImageLayout = ImageLayout.Zoom;
            table_DpsDetailDataTable.EmptyImage = Properties.Resources.cancel_hover;
            table_DpsDetailDataTable.FixedHeader = false;
            table_DpsDetailDataTable.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            table_DpsDetailDataTable.Gap = 8;
            table_DpsDetailDataTable.Gaps = new Size(8, 8);
            table_DpsDetailDataTable.Location = new Point(0, 235);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.RowSelectedBg = Color.FromArgb(174, 212, 251);
            table_DpsDetailDataTable.Size = new Size(1254, 987);
            table_DpsDetailDataTable.TabIndex = 14;
            table_DpsDetailDataTable.Text = "table1";
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
            pageHeader1.Size = new Size(1696, 52);
            pageHeader1.TabIndex = 15;
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
            TitleText.Size = new Size(1696, 52);
            TitleText.TabIndex = 26;
            TitleText.Text = "Skill Breakdown";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
            // 
            // segmented1
            // 
            segmented1.BarBg = true;
            segmented1.BarPosition = AntdUI.TAlignMini.Bottom;
            segmented1.BarSize = 0F;
            segmented1.Dock = DockStyle.Fill;
            segmented1.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            segmented1.Full = true;
            segmented1.IconGap = 0F;
            segmentedItem1.Text = "技能伤害分析";
            segmentedItem2.Text = "技能治疗分析";
            segmentedItem3.Text = "人物承伤分析";
            segmented1.Items.Add(segmentedItem1);
            segmented1.Items.Add(segmentedItem2);
            segmented1.Items.Add(segmentedItem3);
            segmented1.Location = new Point(9, 9);
            segmented1.Name = "segmented1";
            segmented1.Round = true;
            segmented1.SelectIndex = 0;
            segmented1.Size = new Size(478, 47);
            segmented1.TabIndex = 16;
            segmented1.Text = "segmented1";
            segmented1.SelectIndexChanged += segmented1_SelectIndexChanged;
            // 
            // NickNameText
            // 
            NickNameText.BackColor = Color.Transparent;
            NickNameText.Font = new Font("阿里妈妈数黑体", 12F);
            NickNameText.Location = new Point(27, 33);
            NickNameText.Name = "NickNameText";
            NickNameText.Size = new Size(181, 45);
            NickNameText.TabIndex = 17;
            NickNameText.Text = "惊奇猫猫盒";
            // 
            // divider1
            // 
            divider1.BackColor = Color.Transparent;
            divider1.ColorScheme = AntdUI.TAMode.Dark;
            divider1.ColorSplit = Color.White;
            divider1.Location = new Point(280, 71);
            divider1.Name = "divider1";
            divider1.OrientationMargin = 0F;
            divider1.Size = new Size(60, 86);
            divider1.TabIndex = 20;
            divider1.Text = "";
            divider1.Vertical = true;
            // 
            // panel1
            // 
            panel1.Back = Color.FromArgb(103, 174, 246);
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(LuckyRate);
            panel1.Controls.Add(TotalDpsText);
            panel1.Controls.Add(CritRateText);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(TotalDamageText);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(divider1);
            panel1.Controls.Add(label3);
            panel1.Location = new Point(7, 6);
            panel1.Name = "panel1";
            panel1.Shadow = 6;
            panel1.Size = new Size(608, 191);
            panel1.TabIndex = 21;
            panel1.Text = "panel1";
            // 
            // LuckyRate
            // 
            LuckyRate.BackColor = Color.Transparent;
            LuckyRate.ColorScheme = AntdUI.TAMode.Dark;
            LuckyRate.Font = new Font("SAO Welcome TT", 10.499999F);
            LuckyRate.Location = new Point(481, 122);
            LuckyRate.Name = "LuckyRate";
            LuckyRate.Size = new Size(96, 30);
            LuckyRate.TabIndex = 25;
            LuckyRate.Text = "0";
            LuckyRate.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TotalDpsText
            // 
            TotalDpsText.BackColor = Color.Transparent;
            TotalDpsText.ColorScheme = AntdUI.TAMode.Dark;
            TotalDpsText.Font = new Font("SAO Welcome TT", 10.499999F);
            TotalDpsText.Location = new Point(189, 122);
            TotalDpsText.Name = "TotalDpsText";
            TotalDpsText.Size = new Size(96, 30);
            TotalDpsText.TabIndex = 25;
            TotalDpsText.Text = "0";
            TotalDpsText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CritRateText
            // 
            CritRateText.BackColor = Color.Transparent;
            CritRateText.ColorScheme = AntdUI.TAMode.Dark;
            CritRateText.Font = new Font("SAO Welcome TT", 10.499999F);
            CritRateText.Location = new Point(481, 73);
            CritRateText.Name = "CritRateText";
            CritRateText.Size = new Size(96, 30);
            CritRateText.TabIndex = 23;
            CritRateText.Text = "0";
            CritRateText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.BackColor = Color.Transparent;
            label5.ColorScheme = AntdUI.TAMode.Dark;
            label5.Font = new Font("HarmonyOS Sans SC", 9F);
            label5.Location = new Point(329, 115);
            label5.Name = "label5";
            label5.Size = new Size(72, 45);
            label5.TabIndex = 24;
            label5.Text = "幸运率";
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.ColorScheme = AntdUI.TAMode.Dark;
            label2.Font = new Font("HarmonyOS Sans SC", 9F);
            label2.Location = new Point(21, 115);
            label2.Name = "label2";
            label2.Size = new Size(72, 45);
            label2.TabIndex = 24;
            label2.Text = "秒伤";
            // 
            // TotalDamageText
            // 
            TotalDamageText.BackColor = Color.Transparent;
            TotalDamageText.ColorScheme = AntdUI.TAMode.Dark;
            TotalDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            TotalDamageText.Location = new Point(189, 73);
            TotalDamageText.Name = "TotalDamageText";
            TotalDamageText.Size = new Size(96, 30);
            TotalDamageText.TabIndex = 23;
            TotalDamageText.Text = "0";
            TotalDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            label4.BackColor = Color.Transparent;
            label4.ColorScheme = AntdUI.TAMode.Dark;
            label4.Font = new Font("HarmonyOS Sans SC", 9F);
            label4.Location = new Point(329, 66);
            label4.Name = "label4";
            label4.Size = new Size(72, 45);
            label4.TabIndex = 22;
            label4.Text = "暴击率";
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.ColorScheme = AntdUI.TAMode.Dark;
            label1.Font = new Font("HarmonyOS Sans SC", 9F);
            label1.Location = new Point(21, 66);
            label1.Name = "label1";
            label1.Size = new Size(72, 45);
            label1.TabIndex = 22;
            label1.Text = "总伤害";
            // 
            // label3
            // 
            label3.BackColor = Color.Transparent;
            label3.ColorScheme = AntdUI.TAMode.Dark;
            label3.Font = new Font("HarmonyOS Sans SC Medium", 10.999999F, FontStyle.Bold);
            label3.Location = new Point(219, 21);
            label3.Name = "label3";
            label3.Size = new Size(182, 30);
            label3.TabIndex = 22;
            label3.Text = "伤害信息";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.Back = Color.FromArgb(103, 174, 246);
            panel2.BackColor = Color.Transparent;
            panel2.Controls.Add(AvgDamageText);
            panel2.Controls.Add(CritDamageText);
            panel2.Controls.Add(LuckyDamageText);
            panel2.Controls.Add(label9);
            panel2.Controls.Add(label7);
            panel2.Controls.Add(NormalDamageText);
            panel2.Controls.Add(label8);
            panel2.Controls.Add(label6);
            panel2.Controls.Add(divider2);
            panel2.Controls.Add(label19);
            panel2.Location = new Point(626, 6);
            panel2.Name = "panel2";
            panel2.Shadow = 6;
            panel2.Size = new Size(607, 191);
            panel2.TabIndex = 22;
            panel2.Text = "C";
            // 
            // AvgDamageText
            // 
            AvgDamageText.BackColor = Color.Transparent;
            AvgDamageText.ColorScheme = AntdUI.TAMode.Dark;
            AvgDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            AvgDamageText.Location = new Point(481, 122);
            AvgDamageText.Name = "AvgDamageText";
            AvgDamageText.Size = new Size(79, 30);
            AvgDamageText.TabIndex = 25;
            AvgDamageText.Text = "0";
            AvgDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CritDamageText
            // 
            CritDamageText.BackColor = Color.Transparent;
            CritDamageText.ColorScheme = AntdUI.TAMode.Dark;
            CritDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            CritDamageText.Location = new Point(189, 122);
            CritDamageText.Name = "CritDamageText";
            CritDamageText.Size = new Size(79, 30);
            CritDamageText.TabIndex = 25;
            CritDamageText.Text = "0";
            CritDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // LuckyDamageText
            // 
            LuckyDamageText.BackColor = Color.Transparent;
            LuckyDamageText.ColorScheme = AntdUI.TAMode.Dark;
            LuckyDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            LuckyDamageText.Location = new Point(481, 73);
            LuckyDamageText.Name = "LuckyDamageText";
            LuckyDamageText.Size = new Size(79, 30);
            LuckyDamageText.TabIndex = 23;
            LuckyDamageText.Text = "0";
            LuckyDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            label9.BackColor = Color.Transparent;
            label9.ColorScheme = AntdUI.TAMode.Dark;
            label9.Font = new Font("HarmonyOS Sans SC", 9F);
            label9.Location = new Point(329, 115);
            label9.Name = "label9";
            label9.Size = new Size(86, 45);
            label9.TabIndex = 24;
            label9.Text = "平均伤害";
            // 
            // label7
            // 
            label7.BackColor = Color.Transparent;
            label7.ColorScheme = AntdUI.TAMode.Dark;
            label7.Font = new Font("HarmonyOS Sans SC", 9F);
            label7.Location = new Point(22, 115);
            label7.Name = "label7";
            label7.Size = new Size(91, 45);
            label7.TabIndex = 24;
            label7.Text = "暴击伤害";
            // 
            // NormalDamageText
            // 
            NormalDamageText.BackColor = Color.Transparent;
            NormalDamageText.ColorScheme = AntdUI.TAMode.Dark;
            NormalDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            NormalDamageText.Location = new Point(189, 73);
            NormalDamageText.Name = "NormalDamageText";
            NormalDamageText.Size = new Size(79, 30);
            NormalDamageText.TabIndex = 23;
            NormalDamageText.Text = "0";
            NormalDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            label8.BackColor = Color.Transparent;
            label8.ColorScheme = AntdUI.TAMode.Dark;
            label8.Font = new Font("HarmonyOS Sans SC", 9F);
            label8.Location = new Point(329, 66);
            label8.Name = "label8";
            label8.Size = new Size(86, 45);
            label8.TabIndex = 22;
            label8.Text = "幸运伤害";
            // 
            // label6
            // 
            label6.BackColor = Color.Transparent;
            label6.ColorScheme = AntdUI.TAMode.Dark;
            label6.Font = new Font("HarmonyOS Sans SC", 9F);
            label6.Location = new Point(22, 66);
            label6.Name = "label6";
            label6.Size = new Size(91, 45);
            label6.TabIndex = 22;
            label6.Text = "普通伤害";
            // 
            // divider2
            // 
            divider2.BackColor = Color.Transparent;
            divider2.ColorScheme = AntdUI.TAMode.Dark;
            divider2.ColorSplit = Color.White;
            divider2.Location = new Point(299, 72);
            divider2.Name = "divider2";
            divider2.OrientationMargin = 0F;
            divider2.Size = new Size(8, 88);
            divider2.TabIndex = 20;
            divider2.Text = "";
            divider2.Vertical = true;
            // 
            // label19
            // 
            label19.BackColor = Color.Transparent;
            label19.ColorScheme = AntdUI.TAMode.Dark;
            label19.Font = new Font("HarmonyOS Sans SC Medium", 10.999999F, FontStyle.Bold);
            label19.Location = new Point(219, 21);
            label19.Name = "label19";
            label19.Size = new Size(182, 30);
            label19.TabIndex = 22;
            label19.Text = "伤害分布";
            label19.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 5000;
            timer1.Tick += timer1_Tick;
            // 
            // UidText
            // 
            UidText.BackColor = Color.Transparent;
            UidText.Font = new Font("HarmonyOS Sans SC", 9F);
            UidText.Location = new Point(213, 33);
            UidText.Name = "UidText";
            UidText.Prefix = "UID:";
            UidText.Size = new Size(157, 45);
            UidText.TabIndex = 23;
            UidText.Text = "123";
            // 
            // PowerText
            // 
            PowerText.BackColor = Color.Transparent;
            PowerText.Font = new Font("HarmonyOS Sans SC", 9F);
            PowerText.Location = new Point(376, 33);
            PowerText.Name = "PowerText";
            PowerText.Prefix = "战力：";
            PowerText.Size = new Size(168, 45);
            PowerText.TabIndex = 24;
            PowerText.Text = "123";
            // 
            // divider3
            // 
            divider3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider3.BackColor = Color.Transparent;
            divider3.Location = new Point(13, 203);
            divider3.Name = "divider3";
            divider3.OrientationMargin = 0F;
            divider3.Size = new Size(1204, 14);
            divider3.TabIndex = 25;
            divider3.Text = "";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel3.BackColor = Color.Transparent;
            panel3.Controls.Add(segmented1);
            panel3.Location = new Point(1189, 23);
            panel3.Name = "panel3";
            panel3.Radius = 500;
            panel3.Shadow = 6;
            panel3.ShadowOpacityHover = 0F;
            panel3.Size = new Size(496, 65);
            panel3.TabIndex = 26;
            panel3.Text = "panel3";
            // 
            // panel5
            // 
            panel5.BackColor = Color.Transparent;
            panel5.Controls.Add(panel3);
            panel5.Controls.Add(PowerText);
            panel5.Controls.Add(UidText);
            panel5.Controls.Add(NickNameText);
            panel5.Dock = DockStyle.Top;
            panel5.Location = new Point(0, 52);
            panel5.Name = "panel5";
            panel5.Radius = 0;
            panel5.Shadow = 6;
            panel5.ShadowAlign = AntdUI.TAlignMini.Bottom;
            panel5.Size = new Size(1696, 123);
            panel5.TabIndex = 29;
            panel5.Text = "panel5";
            // 
            // select1
            // 
            select1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            select1.DropDownTextAlign = AntdUI.TAlign.Top;
            select1.List = true;
            select1.Location = new Point(1489, 27);
            select1.Name = "select1";
            select1.Placement = AntdUI.TAlignFrom.Top;
            select1.Radius = 3;
            select1.Size = new Size(196, 47);
            select1.TabIndex = 27;
            select1.SelectedIndexChanged += select1_SelectedIndexChanged;
            // 
            // panel6
            // 
            panel6.Controls.Add(select1);
            panel6.Controls.Add(button2);
            panel6.Controls.Add(button1);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 1397);
            panel6.Name = "panel6";
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1696, 86);
            panel6.TabIndex = 30;
            panel6.Text = "panel6";
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom;
            button2.Ghost = true;
            button2.Icon = Properties.Resources.cancel_normal;
            button2.IconHover = Properties.Resources.cancel_hover;
            button2.IconPosition = AntdUI.TAlignMini.None;
            button2.IconRatio = 1.5F;
            button2.Location = new Point(930, 25);
            button2.Name = "button2";
            button2.Size = new Size(57, 49);
            button2.TabIndex = 1;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom;
            button1.Ghost = true;
            button1.Icon = Properties.Resources.flushed_normal;
            button1.IconHover = Properties.Resources.flushed_hover;
            button1.IconPosition = AntdUI.TAlignMini.None;
            button1.IconRatio = 1.5F;
            button1.Location = new Point(713, 25);
            button1.Name = "button1";
            button1.Size = new Size(57, 49);
            button1.TabIndex = 0;
            button1.Click += button1_Click;
            // 
            // splitter1
            // 
            splitter1.ArrowColor = SystemColors.ActiveCaption;
            splitter1.CollapsePanel = AntdUI.Splitter.ADCollapsePanel.Panel1;
            splitter1.Dock = DockStyle.Fill;
            splitter1.FixedPanel = FixedPanel.Panel2;
            splitter1.Location = new Point(0, 175);
            splitter1.Name = "splitter1";
            // 
            // splitter1.Panel1
            // 
            splitter1.Panel1.AutoScroll = true;
            splitter1.Panel1.Controls.Add(collapse1);
            // 
            // splitter1.Panel2
            // 
            splitter1.Panel2.Controls.Add(panel1);
            splitter1.Panel2.Controls.Add(table_DpsDetailDataTable);
            splitter1.Panel2.Controls.Add(divider3);
            splitter1.Panel2.Controls.Add(panel2);
            splitter1.Size = new Size(1696, 1222);
            splitter1.SplitterDistance = 432;
            splitter1.SplitterWidth = 6;
            splitter1.TabIndex = 27;
            // 
            // collapse1
            // 
            collapse1.Dock = DockStyle.Fill;
            collapse1.Font = new Font("HarmonyOS Sans SC", 10F, FontStyle.Bold);
            collapse1.FontExpand = new Font("HarmonyOS Sans SC", 10F, FontStyle.Bold);
            collapse1.ForeColor = Color.FromArgb(103, 174, 246);
            collapse1.Items.Add(collapseItem1);
            collapse1.Items.Add(collapseItem2);
            collapse1.Items.Add(collapseItem3);
            collapse1.Location = new Point(0, 0);
            collapse1.Name = "collapse1";
            collapse1.Size = new Size(432, 1222);
            collapse1.TabIndex = 28;
            collapse1.Text = "collapse1";
            // 
            // collapseItem1
            // 
            collapseItem1.Expand = true;
            collapseItem1.Font = new Font("HarmonyOS Sans SC", 10F, FontStyle.Bold);
            collapseItem1.Location = new Point(27, 90);
            collapseItem1.Name = "collapseItem1";
            collapseItem1.Size = new Size(378, 300);
            collapseItem1.TabIndex = 0;
            collapseItem1.Text = "Dps/Hps/DTps实时曲线图";
            // 
            // collapseItem2
            // 
            collapseItem2.Expand = true;
            collapseItem2.Location = new Point(27, 501);
            collapseItem2.Name = "collapseItem2";
            collapseItem2.Size = new Size(378, 300);
            collapseItem2.TabIndex = 1;
            collapseItem2.Text = "技能占比分布图";
            // 
            // collapseItem3
            // 
            collapseItem3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            collapseItem3.AutoScroll = true;
            collapseItem3.Expand = true;
            collapseItem3.Location = new Point(27, 912);
            collapseItem3.Name = "collapseItem3";
            collapseItem3.Size = new Size(378, 300);
            collapseItem3.TabIndex = 2;
            collapseItem3.Text = "伤害分布";
            // 
            // SkillDetailForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(251, 251, 251);
            ClientSize = new Size(1696, 1483);
            Controls.Add(splitter1);
            Controls.Add(panel6);
            Controls.Add(panel5);
            Controls.Add(pageHeader1);
            Dark = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Mode = AntdUI.TAMode.Dark;
            Name = "SkillDetailForm";
            Opacity = 0.95D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SkillDetailForm";
            Load += SkillDetailForm_Load;
            ForeColorChanged += SkillDetailForm_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel6.ResumeLayout(false);
            splitter1.Panel1.ResumeLayout(false);
            splitter1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter1).EndInit();
            splitter1.ResumeLayout(false);
            collapse1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Segmented segmented1;
        private AntdUI.Label NickNameText;
        private AntdUI.Divider divider1;
        private AntdUI.Panel panel1;
        private AntdUI.Label label3;
        private AntdUI.Label label1;
        private AntdUI.Label TotalDamageText;
        private AntdUI.Label TotalDpsText;
        private AntdUI.Label label2;
        private AntdUI.Label LuckyRate;
        private AntdUI.Label label5;
        private AntdUI.Label CritRateText;
        private AntdUI.Label label4;
        private AntdUI.Panel panel2;
        private AntdUI.Label AvgDamageText;
        private AntdUI.Label CritDamageText;
        private AntdUI.Label LuckyDamageText;
        private AntdUI.Label label9;
        private AntdUI.Label label7;
        private AntdUI.Label NormalDamageText;
        private AntdUI.Label label8;
        private AntdUI.Label label6;
        private AntdUI.Divider divider2;
        private AntdUI.Label label19;
        private AntdUI.Label UidText;
        private AntdUI.Label PowerText;
        public System.Windows.Forms.Timer timer1;
        private AntdUI.Divider divider3;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel3;
        private AntdUI.Panel panel5;
        private AntdUI.Panel panel6;
        private AntdUI.Button button1;
        private AntdUI.Button button2;
        private AntdUI.Select select1;
        private AntdUI.Splitter splitter1;
        private AntdUI.Collapse collapse1;
        private AntdUI.CollapseItem collapseItem1;
        private AntdUI.CollapseItem collapseItem2;
        private AntdUI.CollapseItem collapseItem3;
    }
}