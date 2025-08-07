namespace StarResonanceDpsAnalysis.Control
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
            input1 = new AntdUI.Input();
            input2 = new AntdUI.Input();
            input3 = new AntdUI.Input();
            input4 = new AntdUI.Input();
            input5 = new AntdUI.Input();
            label2 = new AntdUI.Label();
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
            inputNumber1.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            inputNumber1.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            inputNumber1.Name = "inputNumber1";
            inputNumber1.PrefixText = "窗体透明度：";
            inputNumber1.SelectionStart = 2;
            inputNumber1.Size = new Size(500, 56);
            inputNumber1.SuffixText = "%";
            inputNumber1.TabIndex = 12;
            inputNumber1.Text = "10";
            inputNumber1.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // colorPicker1
            // 
            colorPicker1.Location = new Point(163, 392);
            colorPicker1.Name = "colorPicker1";
            colorPicker1.ShowText = true;
            colorPicker1.Size = new Size(173, 57);
            colorPicker1.TabIndex = 13;
            colorPicker1.Value = Color.FromArgb(252, 227, 138);
            colorPicker1.ValueChanged += colorPicker1_ValueChanged;
            // 
            // label1
            // 
            label1.Location = new Point(32, 400);
            label1.Name = "label1";
            label1.Size = new Size(152, 41);
            label1.TabIndex = 14;
            label1.Text = "DPS占比条颜色：";
            // 
            // input1
            // 
            input1.Location = new Point(20, 149);
            input1.Name = "input1";
            input1.PrefixText = "鼠标穿透键位：";
            input1.ReadOnly = true;
            input1.Size = new Size(252, 65);
            input1.TabIndex = 15;
            input1.PreviewKeyDown += input1_PreviewKeyDown;
            // 
            // input2
            // 
            input2.Location = new Point(285, 149);
            input2.Name = "input2";
            input2.PrefixText = "窗体透明键位：";
            input2.ReadOnly = true;
            input2.Size = new Size(237, 65);
            input2.TabIndex = 16;
            input2.PreviewKeyDown += input2_PreviewKeyDown;
            // 
            // input3
            // 
            input3.Location = new Point(20, 220);
            input3.Name = "input3";
            input3.PrefixText = "开关键位：";
            input3.ReadOnly = true;
            input3.Size = new Size(252, 65);
            input3.TabIndex = 17;
            input3.PreviewKeyDown += input3_PreviewKeyDown;
            // 
            // input4
            // 
            input4.Location = new Point(285, 220);
            input4.Name = "input4";
            input4.PrefixText = "清空数据键位：";
            input4.ReadOnly = true;
            input4.Size = new Size(237, 65);
            input4.TabIndex = 18;
            input4.PreviewKeyDown += input4_PreviewKeyDown;
            // 
            // input5
            // 
            input5.Location = new Point(20, 291);
            input5.Name = "input5";
            input5.PrefixText = "清空历史键位：";
            input5.ReadOnly = true;
            input5.Size = new Size(252, 65);
            input5.TabIndex = 19;
            input5.PreviewKeyDown += input5_PreviewKeyDown;
            // 
            // label2
            // 
            label2.Dock = DockStyle.Bottom;
            label2.ForeColor = Color.Red;
            label2.Location = new Point(0, 460);
            label2.Name = "label2";
            label2.Size = new Size(550, 41);
            label2.TabIndex = 20;
            label2.Text = "Delete删除当前键位";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Setup
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(colorPicker1);
            Controls.Add(label2);
            Controls.Add(input5);
            Controls.Add(input4);
            Controls.Add(input3);
            Controls.Add(input2);
            Controls.Add(input1);
            Controls.Add(label1);
            Controls.Add(inputNumber1);
            Controls.Add(InterfaceComboBox);
            ForeColor = Color.Transparent;
            Name = "Setup";
            Size = new Size(550, 501);
            Load += Setup_Load;
            ResumeLayout(false);
        }

        #endregion

        public AntdUI.Select InterfaceComboBox;
        public AntdUI.InputNumber inputNumber1;
        private AntdUI.Label label1;
        public AntdUI.ColorPicker colorPicker1;
        private AntdUI.Input input1;
        private AntdUI.Input input2;
        private AntdUI.Input input3;
        private AntdUI.Input input4;
        private AntdUI.Input input5;
        private AntdUI.Label label2;
    }
}
