namespace StarResonanceDpsAnalysis.Forms.ModuleForm
{
    partial class ModuleCalculationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleCalculationForm));
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            select2 = new AntdUI.Select();
            button3 = new AntdUI.Button();
            button4 = new AntdUI.Button();
            select1 = new AntdUI.Select();
            chkStrengthBoost = new AntdUI.Checkbox();
            chkAgilityBoost = new AntdUI.Checkbox();
            chkIntelligenceBoost = new AntdUI.Checkbox();
            chkSpecialAttackDamage = new AntdUI.Checkbox();
            chkSpecialHealingBoost = new AntdUI.Checkbox();
            chkExpertHealingBoost = new AntdUI.Checkbox();
            chkCastingFocus = new AntdUI.Checkbox();
            chkAttackSpeedFocus = new AntdUI.Checkbox();
            chkCriticalFocus = new AntdUI.Checkbox();
            chkLuckFocus = new AntdUI.Checkbox();
            chkMagicResistance = new AntdUI.Checkbox();
            chkPhysicalResistance = new AntdUI.Checkbox();
            groupBox1 = new GroupBox();
            chkEliteStrike = new AntdUI.Checkbox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            groupBox4 = new GroupBox();
            button1 = new AntdUI.Button();
            virtualPanel1 = new AntdUI.VirtualPanel();
            label1 = new AntdUI.Label();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
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
            pageHeader1.Size = new Size(1259, 38);
            pageHeader1.TabIndex = 31;
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
            TitleText.Size = new Size(1259, 38);
            TitleText.TabIndex = 27;
            TitleText.Text = "Death Statistics";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
            // 
            // panel6
            // 
            panel6.Controls.Add(select2);
            panel6.Controls.Add(button3);
            panel6.Controls.Add(button4);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 1013);
            panel6.Name = "panel6";
            panel6.Radius = 3;
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1259, 86);
            panel6.TabIndex = 34;
            panel6.Text = "panel6";
            // 
            // select2
            // 
            select2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select2.DropDownTextAlign = AntdUI.TAlign.Top;
            select2.List = true;
            select2.Location = new Point(4450, 53);
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
            button3.Location = new Point(699, 25);
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
            button4.Location = new Point(502, 25);
            button4.Name = "button4";
            button4.Size = new Size(57, 49);
            button4.TabIndex = 0;
            button4.Click += button4_Click;
            // 
            // select1
            // 
            select1.Font = new Font("HarmonyOS Sans SC", 9F);
            select1.Items.AddRange(new object[] { "攻击", "辅助", "守护" });
            select1.List = true;
            select1.Location = new Point(18, 60);
            select1.Name = "select1";
            select1.PrefixText = "选择模组类型：";
            select1.Radius = 3;
            select1.SelectedIndex = 0;
            select1.SelectedValue = "攻击";
            select1.SelectionStart = 2;
            select1.Size = new Size(600, 83);
            select1.TabIndex = 35;
            select1.Text = "攻击";
            select1.SelectedIndexChanged += select1_SelectedIndexChanged;
            // 
            // chkStrengthBoost
            // 
            chkStrengthBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkStrengthBoost.Location = new Point(17, 41);
            chkStrengthBoost.Name = "chkStrengthBoost";
            chkStrengthBoost.Size = new Size(126, 54);
            chkStrengthBoost.TabIndex = 36;
            chkStrengthBoost.Text = "力量加持";
            chkStrengthBoost.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkAgilityBoost
            // 
            chkAgilityBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkAgilityBoost.Location = new Point(147, 41);
            chkAgilityBoost.Name = "chkAgilityBoost";
            chkAgilityBoost.Size = new Size(126, 54);
            chkAgilityBoost.TabIndex = 37;
            chkAgilityBoost.Text = "敏捷加持";
            chkAgilityBoost.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkIntelligenceBoost
            // 
            chkIntelligenceBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkIntelligenceBoost.Location = new Point(17, 109);
            chkIntelligenceBoost.Name = "chkIntelligenceBoost";
            chkIntelligenceBoost.Size = new Size(126, 54);
            chkIntelligenceBoost.TabIndex = 38;
            chkIntelligenceBoost.Text = "智力加持";
            chkIntelligenceBoost.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkSpecialAttackDamage
            // 
            chkSpecialAttackDamage.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkSpecialAttackDamage.Location = new Point(165, 41);
            chkSpecialAttackDamage.Name = "chkSpecialAttackDamage";
            chkSpecialAttackDamage.Size = new Size(126, 54);
            chkSpecialAttackDamage.TabIndex = 39;
            chkSpecialAttackDamage.Text = "特攻伤害";
            chkSpecialAttackDamage.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkSpecialHealingBoost
            // 
            chkSpecialHealingBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkSpecialHealingBoost.Location = new Point(17, 33);
            chkSpecialHealingBoost.Name = "chkSpecialHealingBoost";
            chkSpecialHealingBoost.Size = new Size(162, 54);
            chkSpecialHealingBoost.TabIndex = 40;
            chkSpecialHealingBoost.Text = "特攻治疗加持";
            chkSpecialHealingBoost.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkExpertHealingBoost
            // 
            chkExpertHealingBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExpertHealingBoost.Location = new Point(17, 102);
            chkExpertHealingBoost.Name = "chkExpertHealingBoost";
            chkExpertHealingBoost.Size = new Size(162, 54);
            chkExpertHealingBoost.TabIndex = 41;
            chkExpertHealingBoost.Text = "专精治疗加持";
            chkExpertHealingBoost.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkCastingFocus
            // 
            chkCastingFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkCastingFocus.Location = new Point(148, 109);
            chkCastingFocus.Name = "chkCastingFocus";
            chkCastingFocus.Size = new Size(126, 54);
            chkCastingFocus.TabIndex = 42;
            chkCastingFocus.Text = "施法专注";
            chkCastingFocus.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkAttackSpeedFocus
            // 
            chkAttackSpeedFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkAttackSpeedFocus.Location = new Point(16, 41);
            chkAttackSpeedFocus.Name = "chkAttackSpeedFocus";
            chkAttackSpeedFocus.Size = new Size(126, 54);
            chkAttackSpeedFocus.TabIndex = 43;
            chkAttackSpeedFocus.Text = "攻速专注";
            chkAttackSpeedFocus.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkCriticalFocus
            // 
            chkCriticalFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkCriticalFocus.Location = new Point(16, 109);
            chkCriticalFocus.Name = "chkCriticalFocus";
            chkCriticalFocus.Size = new Size(126, 54);
            chkCriticalFocus.TabIndex = 44;
            chkCriticalFocus.Text = "暴击专注";
            chkCriticalFocus.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkLuckFocus
            // 
            chkLuckFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkLuckFocus.Location = new Point(165, 109);
            chkLuckFocus.Name = "chkLuckFocus";
            chkLuckFocus.Size = new Size(126, 54);
            chkLuckFocus.TabIndex = 45;
            chkLuckFocus.Text = "幸运专注";
            chkLuckFocus.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkMagicResistance
            // 
            chkMagicResistance.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkMagicResistance.Location = new Point(148, 30);
            chkMagicResistance.Name = "chkMagicResistance";
            chkMagicResistance.Size = new Size(126, 54);
            chkMagicResistance.TabIndex = 46;
            chkMagicResistance.Text = "抵御魔法";
            chkMagicResistance.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // chkPhysicalResistance
            // 
            chkPhysicalResistance.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkPhysicalResistance.Location = new Point(16, 29);
            chkPhysicalResistance.Name = "chkPhysicalResistance";
            chkPhysicalResistance.Size = new Size(126, 54);
            chkPhysicalResistance.TabIndex = 47;
            chkPhysicalResistance.Text = "抵御物理";
            chkPhysicalResistance.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkEliteStrike);
            groupBox1.Controls.Add(chkAttackSpeedFocus);
            groupBox1.Controls.Add(chkSpecialAttackDamage);
            groupBox1.Controls.Add(chkCriticalFocus);
            groupBox1.Controls.Add(chkLuckFocus);
            groupBox1.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox1.Location = new Point(18, 149);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(304, 253);
            groupBox1.TabIndex = 48;
            groupBox1.TabStop = false;
            groupBox1.Text = "攻击类";
            // 
            // chkEliteStrike
            // 
            chkEliteStrike.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkEliteStrike.Location = new Point(16, 180);
            chkEliteStrike.Name = "chkEliteStrike";
            chkEliteStrike.Size = new Size(126, 54);
            chkEliteStrike.TabIndex = 46;
            chkEliteStrike.Text = "精英打击";
            chkEliteStrike.CheckedChanged += chkAttackSpeedFocus_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkSpecialHealingBoost);
            groupBox2.Controls.Add(chkExpertHealingBoost);
            groupBox2.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox2.Location = new Point(18, 800);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(304, 173);
            groupBox2.TabIndex = 49;
            groupBox2.TabStop = false;
            groupBox2.Text = "治疗类";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(chkPhysicalResistance);
            groupBox3.Controls.Add(chkMagicResistance);
            groupBox3.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox3.Location = new Point(18, 678);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(304, 95);
            groupBox3.TabIndex = 50;
            groupBox3.TabStop = false;
            groupBox3.Text = "防御类";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(chkStrengthBoost);
            groupBox4.Controls.Add(chkAgilityBoost);
            groupBox4.Controls.Add(chkIntelligenceBoost);
            groupBox4.Controls.Add(chkCastingFocus);
            groupBox4.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox4.Location = new Point(18, 425);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(304, 222);
            groupBox4.TabIndex = 51;
            groupBox4.TabStop = false;
            groupBox4.Text = "通用类";
            // 
            // button1
            // 
            button1.Font = new Font("HarmonyOS Sans SC", 9F);
            button1.Location = new Point(624, 60);
            button1.Name = "button1";
            button1.Size = new Size(347, 83);
            button1.TabIndex = 52;
            button1.Text = "分析模组";
            button1.Type = AntdUI.TTypeMini.Primary;
            button1.Click += button1_Click;
            // 
            // virtualPanel1
            // 
            virtualPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            virtualPanel1.BackgroundImageLayout = ImageLayout.Zoom;
            virtualPanel1.Gap = 6;
            virtualPanel1.Location = new Point(346, 149);
            virtualPanel1.Margin = new Padding(0);
            virtualPanel1.Name = "virtualPanel1";
            virtualPanel1.Shadow = 3;
            virtualPanel1.Size = new Size(903, 824);
            virtualPanel1.TabIndex = 53;
            virtualPanel1.Text = "virtualPanel1";
            // 
            // label1
            // 
            label1.Dock = DockStyle.Bottom;
            label1.Location = new Point(0, 979);
            label1.Name = "label1";
            label1.Size = new Size(1259, 34);
            label1.TabIndex = 54;
            label1.Text = "打开此界面后需要过图或者重新登录一次才能进行分析";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ModuleCalculationForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1259, 1099);
            Controls.Add(label1);
            Controls.Add(virtualPanel1);
            Controls.Add(button1);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(select1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ModuleCalculationForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "模组分析";
            Load += ModuleCalculationForm_Load;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel6;
        private AntdUI.Select select2;
        private AntdUI.Button button3;
        private AntdUI.Button button4;
        private AntdUI.Select select1;
        private AntdUI.Checkbox chkStrengthBoost;
        private AntdUI.Checkbox chkAgilityBoost;
        private AntdUI.Checkbox chkIntelligenceBoost;
        private AntdUI.Checkbox chkSpecialAttackDamage;
        private AntdUI.Checkbox chkSpecialHealingBoost;
        private AntdUI.Checkbox chkExpertHealingBoost;
        private AntdUI.Checkbox chkCastingFocus;
        private AntdUI.Checkbox chkAttackSpeedFocus;
        private AntdUI.Checkbox chkCriticalFocus;
        private AntdUI.Checkbox chkLuckFocus;
        private AntdUI.Checkbox chkMagicResistance;
        private AntdUI.Checkbox chkPhysicalResistance;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private AntdUI.Button button1;
        public AntdUI.VirtualPanel virtualPanel1;
        private AntdUI.Checkbox chkEliteStrike;
        private AntdUI.Label label1;
    }
}