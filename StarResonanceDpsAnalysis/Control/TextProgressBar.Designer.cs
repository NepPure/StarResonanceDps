namespace StarResonanceDpsAnalysis.Control
{
    partial class TextProgressBar
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
            SuspendLayout();
            // 
            // TextProgressBar
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            DoubleBuffered = true;
            Margin = new Padding(5, 4, 5, 4);
            Name = "TextProgressBar";
            Padding = new Padding(5, 4, 5, 4);
            Size = new Size(440, 42);
            Load += TextProgressBar_Load;
            Paint += TextProgressBar_Paint;
            ResumeLayout(false);
        }

        #endregion
    }
}
