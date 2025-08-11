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
            textProgressBar1 = new StarResonanceDpsAnalysis.Control.TextProgressBar();
            SuspendLayout();
            // 
            // textProgressBar1
            // 
            textProgressBar1.BackColor = Color.White;
            textProgressBar1.Location = new Point(640, 244);
            textProgressBar1.Margin = new Padding(5, 4, 5, 4);
            textProgressBar1.Name = "textProgressBar1";
            textProgressBar1.Padding = new Padding(5, 4, 5, 4);
            textProgressBar1.ProgressBarColor = Color.FromArgb(86, 156, 214);
            textProgressBar1.ProgressBarCornerRadius = 3;
            textProgressBar1.ProgressBarValue = 0D;
            textProgressBar1.Size = new Size(502, 75);
            textProgressBar1.TabIndex = 0;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1370, 774);
            Controls.Add(textProgressBar1);
            Name = "TestForm";
            Text = "TestForm";
            ResumeLayout(false);
        }

        #endregion

        private Control.TextProgressBar textProgressBar1;
    }
}