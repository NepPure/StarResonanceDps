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
            AntdUI.SegmentedItem segmentedItem3 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem4 = new AntdUI.SegmentedItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillDetailForm));
            table_DpsDetailDataTable = new AntdUI.Table();
            pageHeader1 = new AntdUI.PageHeader();
            segmented1 = new AntdUI.Segmented();
            NickNameText = new AntdUI.Label();
            button_AlwaysOnTop = new AntdUI.Button();
            ProfessionText = new AntdUI.Label();
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
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // table_DpsDetailDataTable
            // 
            table_DpsDetailDataTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            table_DpsDetailDataTable.FixedHeader = false;
            table_DpsDetailDataTable.Gap = 8;
            table_DpsDetailDataTable.Gaps = new Size(8, 8);
            table_DpsDetailDataTable.Location = new Point(0, 366);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.Size = new Size(1247, 771);
            table_DpsDetailDataTable.TabIndex = 14;
            table_DpsDetailDataTable.Text = "table1";
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
            pageHeader1.Size = new Size(1247, 45);
            pageHeader1.TabIndex = 15;
            pageHeader1.Text = "技能详情占比";
            // 
            // segmented1
            // 
            segmented1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            segmentedItem3.Text = "技能伤害分析";
            segmentedItem4.Text = "技能治疗分析";
            segmented1.Items.Add(segmentedItem3);
            segmented1.Items.Add(segmentedItem4);
            segmented1.Location = new Point(911, 82);
            segmented1.Name = "segmented1";
            segmented1.SelectIndex = 0;
            segmented1.Size = new Size(277, 53);
            segmented1.TabIndex = 16;
            segmented1.Text = "segmented1";
            segmented1.SelectIndexChanged += segmented1_SelectIndexChanged;
            // 
            // NickNameText
            // 
            NickNameText.BackColor = Color.Transparent;
            NickNameText.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point, 134);
            NickNameText.Location = new Point(20, 58);
            NickNameText.Name = "NickNameText";
            NickNameText.Size = new Size(247, 45);
            NickNameText.TabIndex = 17;
            NickNameText.Text = "NickName";
            // 
            // button_AlwaysOnTop
            // 
            button_AlwaysOnTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button_AlwaysOnTop.BackColor = Color.Transparent;
            button_AlwaysOnTop.Ghost = true;
            button_AlwaysOnTop.IconSvg = resources.GetString("button_AlwaysOnTop.IconSvg");
            button_AlwaysOnTop.Location = new Point(1194, 85);
            button_AlwaysOnTop.Name = "button_AlwaysOnTop";
            button_AlwaysOnTop.Size = new Size(49, 47);
            button_AlwaysOnTop.TabIndex = 18;
            button_AlwaysOnTop.ToggleIconSvg = "";
            // 
            // ProfessionText
            // 
            ProfessionText.BackColor = Color.Transparent;
            ProfessionText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            ProfessionText.Location = new Point(20, 109);
            ProfessionText.Name = "ProfessionText";
            ProfessionText.Prefix = "职业：";
            ProfessionText.Size = new Size(162, 45);
            ProfessionText.TabIndex = 19;
            ProfessionText.Text = "神射手";
            // 
            // divider1
            // 
            divider1.BackColor = Color.Transparent;
            divider1.Location = new Point(281, 58);
            divider1.Name = "divider1";
            divider1.OrientationMargin = 0F;
            divider1.Size = new Size(59, 86);
            divider1.TabIndex = 20;
            divider1.Text = "";
            divider1.Vertical = true;
            // 
            // panel1
            // 
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
            panel1.Location = new Point(0, 172);
            panel1.Name = "panel1";
            panel1.Shadow = 4;
            panel1.Size = new Size(608, 172);
            panel1.TabIndex = 21;
            panel1.Text = "panel1";
            // 
            // LuckyRate
            // 
            LuckyRate.BackColor = Color.Transparent;
            LuckyRate.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            LuckyRate.Location = new Point(481, 114);
            LuckyRate.Name = "LuckyRate";
            LuckyRate.Size = new Size(96, 30);
            LuckyRate.TabIndex = 25;
            LuckyRate.Text = "8.9万";
            LuckyRate.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TotalDpsText
            // 
            TotalDpsText.BackColor = Color.Transparent;
            TotalDpsText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            TotalDpsText.Location = new Point(189, 114);
            TotalDpsText.Name = "TotalDpsText";
            TotalDpsText.Size = new Size(96, 30);
            TotalDpsText.TabIndex = 25;
            TotalDpsText.Text = "8.9万";
            TotalDpsText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CritRateText
            // 
            CritRateText.BackColor = Color.Transparent;
            CritRateText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            CritRateText.Location = new Point(481, 65);
            CritRateText.Name = "CritRateText";
            CritRateText.Size = new Size(96, 30);
            CritRateText.TabIndex = 23;
            CritRateText.Text = "2288万";
            CritRateText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label5.Location = new Point(329, 107);
            label5.Name = "label5";
            label5.Size = new Size(73, 45);
            label5.TabIndex = 24;
            label5.Text = "幸运率";
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label2.Location = new Point(21, 107);
            label2.Name = "label2";
            label2.Size = new Size(73, 45);
            label2.TabIndex = 24;
            label2.Text = "秒伤";
            // 
            // TotalDamageText
            // 
            TotalDamageText.BackColor = Color.Transparent;
            TotalDamageText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            TotalDamageText.Location = new Point(189, 65);
            TotalDamageText.Name = "TotalDamageText";
            TotalDamageText.Size = new Size(96, 30);
            TotalDamageText.TabIndex = 23;
            TotalDamageText.Text = "2288万";
            TotalDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label4.Location = new Point(329, 58);
            label4.Name = "label4";
            label4.Size = new Size(73, 45);
            label4.TabIndex = 22;
            label4.Text = "暴击率";
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label1.Location = new Point(21, 58);
            label1.Name = "label1";
            label1.Size = new Size(73, 45);
            label1.TabIndex = 22;
            label1.Text = "总伤害";
            // 
            // label3
            // 
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label3.Location = new Point(219, 19);
            label3.Name = "label3";
            label3.Size = new Size(183, 30);
            label3.TabIndex = 22;
            label3.Text = "伤害信息";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
            panel2.Location = new Point(634, 172);
            panel2.Name = "panel2";
            panel2.Shadow = 4;
            panel2.Size = new Size(613, 172);
            panel2.TabIndex = 22;
            panel2.Text = "panel2";
            // 
            // AvgDamageText
            // 
            AvgDamageText.BackColor = Color.Transparent;
            AvgDamageText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            AvgDamageText.Location = new Point(481, 114);
            AvgDamageText.Name = "AvgDamageText";
            AvgDamageText.Size = new Size(96, 30);
            AvgDamageText.TabIndex = 25;
            AvgDamageText.Text = "8.9万";
            AvgDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CritDamageText
            // 
            CritDamageText.BackColor = Color.Transparent;
            CritDamageText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            CritDamageText.Location = new Point(189, 114);
            CritDamageText.Name = "CritDamageText";
            CritDamageText.Size = new Size(96, 30);
            CritDamageText.TabIndex = 25;
            CritDamageText.Text = "8.9万";
            CritDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // LuckyDamageText
            // 
            LuckyDamageText.BackColor = Color.Transparent;
            LuckyDamageText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            LuckyDamageText.Location = new Point(481, 65);
            LuckyDamageText.Name = "LuckyDamageText";
            LuckyDamageText.Size = new Size(96, 30);
            LuckyDamageText.TabIndex = 23;
            LuckyDamageText.Text = "2288万";
            LuckyDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            label9.BackColor = Color.Transparent;
            label9.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label9.Location = new Point(329, 107);
            label9.Name = "label9";
            label9.Size = new Size(73, 45);
            label9.TabIndex = 24;
            label9.Text = "平均伤害";
            // 
            // label7
            // 
            label7.BackColor = Color.Transparent;
            label7.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label7.Location = new Point(22, 107);
            label7.Name = "label7";
            label7.Size = new Size(73, 45);
            label7.TabIndex = 24;
            label7.Text = "暴击伤害";
            // 
            // NormalDamageText
            // 
            NormalDamageText.BackColor = Color.Transparent;
            NormalDamageText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            NormalDamageText.Location = new Point(189, 65);
            NormalDamageText.Name = "NormalDamageText";
            NormalDamageText.Size = new Size(96, 30);
            NormalDamageText.TabIndex = 23;
            NormalDamageText.Text = "2288万";
            NormalDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            label8.BackColor = Color.Transparent;
            label8.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label8.Location = new Point(329, 58);
            label8.Name = "label8";
            label8.Size = new Size(73, 45);
            label8.TabIndex = 22;
            label8.Text = "幸运伤害";
            // 
            // label6
            // 
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label6.Location = new Point(22, 58);
            label6.Name = "label6";
            label6.Size = new Size(73, 45);
            label6.TabIndex = 22;
            label6.Text = "普通伤害";
            // 
            // divider2
            // 
            divider2.BackColor = Color.Transparent;
            divider2.Location = new Point(281, 58);
            divider2.Name = "divider2";
            divider2.OrientationMargin = 0F;
            divider2.Size = new Size(59, 86);
            divider2.TabIndex = 20;
            divider2.Text = "";
            divider2.Vertical = true;
            // 
            // label19
            // 
            label19.BackColor = Color.Transparent;
            label19.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label19.Location = new Point(219, 19);
            label19.Name = "label19";
            label19.Size = new Size(183, 30);
            label19.TabIndex = 22;
            label19.Text = "伤害分布";
            label19.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // UidText
            // 
            UidText.BackColor = Color.Transparent;
            UidText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            UidText.Location = new Point(197, 109);
            UidText.Name = "UidText";
            UidText.Prefix = "UID:";
            UidText.Size = new Size(157, 45);
            UidText.TabIndex = 23;
            UidText.Text = "123";
            // 
            // PowerText
            // 
            PowerText.BackColor = Color.Transparent;
            PowerText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            PowerText.Location = new Point(369, 109);
            PowerText.Name = "PowerText";
            PowerText.Prefix = "战力：";
            PowerText.Size = new Size(168, 45);
            PowerText.TabIndex = 24;
            PowerText.Text = "123";
            // 
            // divider3
            // 
            divider3.BackColor = Color.Transparent;
            divider3.Location = new Point(-1, 346);
            divider3.Name = "divider3";
            divider3.OrientationMargin = 0F;
            divider3.Size = new Size(1247, 14);
            divider3.TabIndex = 25;
            divider3.Text = "";
            // 
            // SkillDetailForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1247, 1138);
            Controls.Add(divider3);
            Controls.Add(PowerText);
            Controls.Add(UidText);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(ProfessionText);
            Controls.Add(button_AlwaysOnTop);
            Controls.Add(NickNameText);
            Controls.Add(segmented1);
            Controls.Add(pageHeader1);
            Controls.Add(table_DpsDetailDataTable);
            Name = "SkillDetailForm";
            Resizable = false;
            Text = "SkillDetailForm";
            Load += SkillDetailForm_Load;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Segmented segmented1;
        private AntdUI.Label NickNameText;
        private AntdUI.Button button_AlwaysOnTop;
        private AntdUI.Label ProfessionText;
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
    }
}