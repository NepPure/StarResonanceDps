namespace StarResonanceDpsAnalysis.Forms
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            sortedProgressBarList1 = new StarResonanceDpsAnalysis.Control.SortedProgressBarList();
            numericUpDown1 = new NumericUpDown();
            numericUpDown2 = new NumericUpDown();
            button1 = new Button();
            button2 = new Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            SuspendLayout();
            // 
            // sortedProgressBarList1
            // 
            sortedProgressBarList1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            sortedProgressBarList1.AnimationDuration = 300;
            sortedProgressBarList1.AnimationQuality = Effects.Enum.Quality.Medium;
            sortedProgressBarList1.BackColor = Color.White;
            sortedProgressBarList1.Location = new Point(12, 12);
            sortedProgressBarList1.Name = "sortedProgressBarList1";
            sortedProgressBarList1.ProgressBarHeight = 20;
            sortedProgressBarList1.ProgressBarPadding = new Padding(3);
            sortedProgressBarList1.Size = new Size(848, 524);
            sortedProgressBarList1.TabIndex = 1;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(525, 482);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(67, 23);
            numericUpDown1.TabIndex = 2;
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(598, 483);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(67, 23);
            numericUpDown2.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(671, 482);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 3;
            button1.Text = "新增/变更";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(752, 482);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 4;
            button2.Text = "删除";
            button2.UseVisualStyleBackColor = true;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(872, 548);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(numericUpDown2);
            Controls.Add(numericUpDown1);
            Controls.Add(sortedProgressBarList1);
            Margin = new Padding(2);
            Name = "TestForm";
            Text = "TestForm";
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Control.SortedProgressBarList sortedProgressBarList1;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private Button button1;
        private Button button2;
    }
}