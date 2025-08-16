namespace StarResonanceDpsAnalysis.Forms
{
    partial class HistoricalBattlesForm
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
            AntdUI.SegmentedItem segmentedItem1 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem2 = new AntdUI.SegmentedItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistoricalBattlesForm));
            pageHeader1 = new AntdUI.PageHeader();
            label1 = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            select2 = new AntdUI.Select();
            button3 = new AntdUI.Button();
            select1 = new AntdUI.Select();
            panel3 = new AntdUI.Panel();
            segmented1 = new AntdUI.Segmented();
            table_DpsDetailDataTable = new AntdUI.Table();
            splitter1 = new AntdUI.Splitter();
            button1 = new AntdUI.Button();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter1).BeginInit();
            splitter1.Panel1.SuspendLayout();
            splitter1.Panel2.SuspendLayout();
            splitter1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(label1);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(1130, 52);
            pageHeader1.TabIndex = 29;
            pageHeader1.Text = "";
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.ColorScheme = AntdUI.TAMode.Dark;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("SAO UI TT", 12F);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(1130, 52);
            label1.TabIndex = 26;
            label1.Text = "HistoricalBattles";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel6
            // 
            panel6.Controls.Add(button1);
            panel6.Controls.Add(select2);
            panel6.Controls.Add(button3);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 958);
            panel6.Name = "panel6";
            panel6.Radius = 3;
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1130, 86);
            panel6.TabIndex = 33;
            panel6.Text = "panel6";
            // 
            // select2
            // 
            select2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select2.DropDownTextAlign = AntdUI.TAlign.Top;
            select2.List = true;
            select2.Location = new Point(3772, 44);
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
            button3.Location = new Point(621, 11);
            button3.Name = "button3";
            button3.Size = new Size(57, 74);
            button3.TabIndex = 1;
            // 
            // select1
            // 
            select1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select1.DropDownTextAlign = AntdUI.TAlign.Bottom;
            select1.List = true;
            select1.Location = new Point(536, 16);
            select1.Name = "select1";
            select1.Placement = AntdUI.TAlignFrom.Bottom;
            select1.Radius = 3;
            select1.Size = new Size(567, 56);
            select1.TabIndex = 28;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            panel3.Controls.Add(segmented1);
            panel3.Location = new Point(12, 10);
            panel3.Name = "panel3";
            panel3.Radius = 500;
            panel3.Shadow = 6;
            panel3.ShadowOpacityHover = 0F;
            panel3.Size = new Size(496, 65);
            panel3.TabIndex = 34;
            panel3.Text = "panel3";
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
            segmentedItem1.Text = "单次伤害记录";
            segmentedItem2.Text = "全程伤害记录";
            segmented1.Items.Add(segmentedItem1);
            segmented1.Items.Add(segmentedItem2);
            segmented1.Location = new Point(9, 9);
            segmented1.Name = "segmented1";
            segmented1.Round = true;
            segmented1.SelectIndex = 0;
            segmented1.Size = new Size(478, 47);
            segmented1.TabIndex = 16;
            segmented1.Text = "segmented1";
            // 
            // table_DpsDetailDataTable
            // 
            table_DpsDetailDataTable.BackgroundImageLayout = ImageLayout.Zoom;
            table_DpsDetailDataTable.Dock = DockStyle.Fill;
            table_DpsDetailDataTable.EmptyImage = Properties.Resources.cancel_hover;
            table_DpsDetailDataTable.FixedHeader = false;
            table_DpsDetailDataTable.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            table_DpsDetailDataTable.Gap = 8;
            table_DpsDetailDataTable.Gaps = new Size(8, 8);
            table_DpsDetailDataTable.Location = new Point(0, 0);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.RowSelectedBg = Color.FromArgb(174, 212, 251);
            table_DpsDetailDataTable.Size = new Size(1130, 816);
            table_DpsDetailDataTable.TabIndex = 35;
            table_DpsDetailDataTable.Text = "table1";
            // 
            // splitter1
            // 
            splitter1.Dock = DockStyle.Fill;
            splitter1.Location = new Point(0, 52);
            splitter1.Name = "splitter1";
            splitter1.Orientation = Orientation.Horizontal;
            // 
            // splitter1.Panel1
            // 
            splitter1.Panel1.Controls.Add(select1);
            splitter1.Panel1.Controls.Add(panel3);
            // 
            // splitter1.Panel2
            // 
            splitter1.Panel2.Controls.Add(table_DpsDetailDataTable);
            splitter1.Size = new Size(1130, 906);
            splitter1.SplitterDistance = 86;
            splitter1.TabIndex = 36;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom;
            button1.Ghost = true;
            button1.Icon = Properties.Resources.flushed_normal;
            button1.IconHover = Properties.Resources.flushed_hover;
            button1.IconPosition = AntdUI.TAlignMini.None;
            button1.IconRatio = 1.5F;
            button1.Location = new Point(450, 9);
            button1.Name = "button1";
            button1.Size = new Size(57, 78);
            button1.TabIndex = 28;
            // 
            // HistoricalBattlesForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1130, 1044);
            Controls.Add(splitter1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "HistoricalBattlesForm";
            Text = "HistoricalBattlesForm";
            Load += HistoricalBattlesForm_Load;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            panel3.ResumeLayout(false);
            splitter1.Panel1.ResumeLayout(false);
            splitter1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter1).EndInit();
            splitter1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label label1;
        private AntdUI.Panel panel6;
        private AntdUI.Select select2;
        private AntdUI.Button button3;
        private AntdUI.Select select1;
        private AntdUI.Panel panel3;
        private AntdUI.Segmented segmented1;
        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.Splitter splitter1;
        private AntdUI.Button button1;
    }
}