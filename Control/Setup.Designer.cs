namespace 星痕共鸣DPS统计.Control
{
    partial class Setup
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            InterfaceComboBox = new AntdUI.Select();
            inputNumber1 = new AntdUI.InputNumber();
            colorPicker1 = new AntdUI.ColorPicker();
            label1 = new AntdUI.Label();
            SuspendLayout();
            // 
            // InterfaceComboBox
            // 
            InterfaceComboBox.Anchor = AnchorStyles.Top;
            InterfaceComboBox.List = true;
            InterfaceComboBox.Location = new Point(25, 26);
            InterfaceComboBox.Name = "InterfaceComboBox";
            InterfaceComboBox.PrefixText = "请选择网卡：";
            InterfaceComboBox.Size = new Size(500, 56);
            InterfaceComboBox.TabIndex = 10;
            InterfaceComboBox.SelectedIndexChanged += InterfaceComboBox_SelectedIndexChanged;
            // 
            // inputNumber1
            // 
            inputNumber1.Location = new Point(20, 87);
            inputNumber1.Name = "inputNumber1";
            inputNumber1.PrefixText = "窗体透明度：";
            inputNumber1.SelectionStart = 1;
            inputNumber1.Size = new Size(500, 56);
            inputNumber1.SuffixText = "%";
            inputNumber1.TabIndex = 12;
            inputNumber1.Text = "0";
            // 
            // colorPicker1
            // 
            colorPicker1.Location = new Point(195, 150);
            colorPicker1.Name = "colorPicker1";
            colorPicker1.ShowText = true;
            colorPicker1.Size = new Size(173, 57);
            colorPicker1.TabIndex = 13;
            colorPicker1.Value = Color.FromArgb(252, 227, 138);
            colorPicker1.ValueChanged += colorPicker1_ValueChanged;
            // 
            // label1
            // 
            label1.Location = new Point(37, 158);
            label1.Name = "label1";
            label1.Size = new Size(152, 41);
            label1.TabIndex = 14;
            label1.Text = "DPS占比条颜色：";
            // 
            // Setup
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(label1);
            Controls.Add(colorPicker1);
            Controls.Add(inputNumber1);
            Controls.Add(InterfaceComboBox);
            Name = "Setup";
            Size = new Size(550, 250);
            Load += Setup_Load;
            ResumeLayout(false);
        }

        #endregion

        public AntdUI.Select InterfaceComboBox;
        public AntdUI.InputNumber inputNumber1;
        private AntdUI.Label label1;
        public AntdUI.ColorPicker colorPicker1;
    }
}
