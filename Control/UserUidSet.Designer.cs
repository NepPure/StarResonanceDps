namespace 星痕共鸣DPS统计.Control
{
    partial class UserUidSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserUidSet));
            pageHeader1 = new AntdUI.PageHeader();
            table1 = new AntdUI.Table();
            button1 = new AntdUI.Button();
            button2 = new AntdUI.Button();
            input2 = new AntdUI.Input();
            inputNumber1 = new AntdUI.InputNumber();
            SuspendLayout();
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
            pageHeader1.Size = new Size(823, 45);
            pageHeader1.TabIndex = 1;
            pageHeader1.Text = "UID昵称设置";
            // 
            // table1
            // 
            table1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            table1.Bordered = true;
            table1.EditMode = AntdUI.TEditMode.Click;
            table1.Gap = 12;
            table1.Location = new Point(12, 123);
            table1.Name = "table1";
            table1.Size = new Size(799, 577);
            table1.TabIndex = 2;
            table1.Text = "table1";
            table1.CellEndEdit += table1_CellEndEdit;
            table1.CellEditComplete += table1_CellEditComplete;
            // 
            // button1
            // 
            button1.Location = new Point(12, 53);
            button1.Name = "button1";
            button1.Size = new Size(295, 57);
            button1.TabIndex = 3;
            button1.Text = "从当前统计数据中获取用户UID";
            button1.Type = AntdUI.TTypeMini.Primary;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom;
            button2.Location = new Point(598, 709);
            button2.Name = "button2";
            button2.Size = new Size(167, 57);
            button2.TabIndex = 5;
            button2.Text = "手动添加";
            button2.Type = AntdUI.TTypeMini.Primary;
            button2.Click += button2_Click;
            // 
            // input2
            // 
            input2.Anchor = AnchorStyles.Bottom;
            input2.Location = new Point(328, 709);
            input2.Name = "input2";
            input2.PrefixText = "昵称：";
            input2.Size = new Size(264, 57);
            input2.TabIndex = 6;
            // 
            // inputNumber1
            // 
            inputNumber1.Location = new Point(58, 709);
            inputNumber1.MaxLength = 9999999;
            inputNumber1.Name = "inputNumber1";
            inputNumber1.PrefixText = "用户UID：";
            inputNumber1.SelectionStart = 1;
            inputNumber1.Size = new Size(264, 57);
            inputNumber1.TabIndex = 7;
            inputNumber1.Text = "0";
            // 
            // UserUidSet
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(823, 775);
            Controls.Add(inputNumber1);
            Controls.Add(input2);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(table1);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "UserUidSet";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "UserUidSet";
            Load += UserUidSet_Load;
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Table table1;
        private AntdUI.Button button1;
        private AntdUI.Button button2;
        private AntdUI.Input input2;
        private AntdUI.InputNumber inputNumber1;
    }
}