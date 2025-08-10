namespace StarResonanceDpsAnalysis.Control
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
            button2 = new AntdUI.Button();
            input2 = new AntdUI.Input();
            inputNumber1 = new AntdUI.InputNumber();
            SuspendLayout();
            // 
            // button2
            // 
            button2.BorderWidth = 1F;
            button2.DefaultBack = Color.FromArgb(153, 204, 255);
            button2.Location = new Point(49, 202);
            button2.Name = "button2";
            button2.Radius = 0;
            button2.Size = new Size(211, 52);
            button2.TabIndex = 5;
            button2.Text = "设置昵称";
            button2.Click += button2_Click;
            // 
            // input2
            // 
            input2.Font = new Font("SAO UI TT", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            input2.Location = new Point(22, 125);
            input2.Name = "input2";
            input2.PrefixText = "Name：";
            input2.SelectionColor = Color.FromArgb(143, 176, 229);
            input2.Size = new Size(264, 57);
            input2.TabIndex = 6;
            // 
            // inputNumber1
            // 
            inputNumber1.Font = new Font("SAO Welcome TT", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            inputNumber1.Location = new Point(22, 62);
            inputNumber1.MaxLength = 9999999;
            inputNumber1.Name = "inputNumber1";
            inputNumber1.PrefixText = "UID：";
            inputNumber1.SelectionColor = Color.FromArgb(143, 176, 229);
            inputNumber1.SelectionStart = 1;
            inputNumber1.Size = new Size(264, 57);
            inputNumber1.TabIndex = 7;
            inputNumber1.Text = "0";
            // 
            // UserUidSet
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(inputNumber1);
            Controls.Add(input2);
            Controls.Add(button2);
            Name = "UserUidSet";
            Size = new Size(358, 308);
            Load += UserUidSet_Load;
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.Button button2;
        private AntdUI.Input input2;
        private AntdUI.InputNumber inputNumber1;
    }
}