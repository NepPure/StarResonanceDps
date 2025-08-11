using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Plugin.Charts
{
    /// <summary>
    /// 扁平化饼图控件
    /// </summary>
    public class FlatPieChart : UserControl
    {
        #region 字段和属性

        private readonly List<PieChartData> _data = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private bool _showLabels = true;
        private bool _showPercentages = true;

        // 现代化扁平配色
        private readonly Color[] _colors = {
            Color.FromArgb(255, 107, 107),  // 红
            Color.FromArgb(78, 205, 196),   // 青
            Color.FromArgb(69, 183, 209),   // 蓝
            Color.FromArgb(150, 206, 180),  // 绿
            Color.FromArgb(255, 234, 167),  // 黄
            Color.FromArgb(221, 160, 221),  // 紫
            Color.FromArgb(152, 216, 200),  // 薄荷
            Color.FromArgb(247, 220, 111),  // 金
            Color.FromArgb(187, 143, 206),  // 淡紫
            Color.FromArgb(133, 193, 233)   // 天蓝
        };

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                _isDarkTheme = value;
                ApplyTheme();
                Invalidate();
            }
        }

        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                Invalidate();
            }
        }

        public bool ShowLabels
        {
            get => _showLabels;
            set
            {
                _showLabels = value;
                Invalidate();
            }
        }

        public bool ShowPercentages
        {
            get => _showPercentages;
            set
            {
                _showPercentages = value;
                Invalidate();
            }
        }

        #endregion

        #region 构造函数

        public FlatPieChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            
            ApplyTheme();
        }

        #endregion

        #region 数据管理

        public void SetData(List<(string Label, double Value)> data)
        {
            _data.Clear();
            
            var total = data.Sum(d => d.Value);
            if (total <= 0) return;

            for (int i = 0; i < data.Count; i++)
            {
                var percentage = data[i].Value / total * 100;
                _data.Add(new PieChartData
                {
                    Label = data[i].Label,
                    Value = data[i].Value,
                    Percentage = percentage,
                    Color = _colors[i % _colors.Length]
                });
            }
            
            Invalidate();
        }

        public void ClearData()
        {
            _data.Clear();
            Invalidate();
        }

        #endregion

        #region 主题设置

        private void ApplyTheme()
        {
            if (_isDarkTheme)
            {
                BackColor = Color.FromArgb(31, 31, 31);
                ForeColor = Color.White;
            }
            else
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
            }
        }

        #endregion

        #region 绘制方法

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 清除背景
            g.Clear(BackColor);

            if (_data.Count == 0)
            {
                DrawNoDataMessage(g);
                return;
            }

            // 绘制标题
            DrawTitle(g);

            // 计算饼图区域
            var titleHeight = string.IsNullOrEmpty(_titleText) ? 0 : 40;
            var pieSize = Math.Min(Width - 40, Height - titleHeight - 40);
            var pieRect = new Rectangle(
                (Width - pieSize) / 2,
                titleHeight + (Height - titleHeight - pieSize) / 2,
                pieSize,
                pieSize
            );

            // 绘制饼图
            DrawPieSlices(g, pieRect);

            // 绘制标签
            if (_showLabels)
            {
                DrawLabels(g, pieRect);
            }
        }

        private void DrawNoDataMessage(Graphics g)
        {
            var message = "暂无数据";
            var font = new Font("Microsoft YaHei", 12, FontStyle.Regular);
            var brush = new SolidBrush(_isDarkTheme ? Color.Gray : Color.DarkGray);
            
            var size = g.MeasureString(message, font);
            var x = (Width - size.Width) / 2;
            var y = (Height - size.Height) / 2;
            
            g.DrawString(message, font, brush, x, y);
            
            font.Dispose();
            brush.Dispose();
        }

        private void DrawTitle(Graphics g)
        {
            if (string.IsNullOrEmpty(_titleText)) return;

            using var font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
            using var brush = new SolidBrush(ForeColor);
            
            var size = g.MeasureString(_titleText, font);
            var x = (Width - size.Width) / 2;
            var y = 10;
            
            g.DrawString(_titleText, font, brush, x, y);
        }

        private void DrawPieSlices(Graphics g, Rectangle pieRect)
        {
            float startAngle = 0;
            
            foreach (var data in _data)
            {
                var sweepAngle = (float)(data.Percentage * 360 / 100);
                
                // 绘制饼片 - 扁平化设计（无边框）
                using var brush = new SolidBrush(data.Color);
                g.FillPie(brush, pieRect, startAngle, sweepAngle);
                
                startAngle += sweepAngle;
            }
        }

        private void DrawLabels(Graphics g, Rectangle pieRect)
        {
            using var font = new Font("Microsoft YaHei", 9);
            using var brush = new SolidBrush(ForeColor);
            
            float startAngle = 0;
            var centerX = pieRect.X + pieRect.Width / 2f;
            var centerY = pieRect.Y + pieRect.Height / 2f;
            var radius = pieRect.Width / 2f;
            
            foreach (var data in _data)
            {
                var sweepAngle = (float)(data.Percentage * 360 / 100);
                var labelAngle = startAngle + sweepAngle / 2;
                
                // 计算标签位置
                var labelRadius = radius * 0.7f; // 标签在饼图内部
                var labelX = centerX + labelRadius * (float)Math.Cos(labelAngle * Math.PI / 180);
                var labelY = centerY + labelRadius * (float)Math.Sin(labelAngle * Math.PI / 180);
                
                // 构建标签文本
                var labelText = "";
                if (_showLabels && _showPercentages)
                {
                    labelText = $"{data.Label}\n{data.Percentage:F1}%";
                }
                else if (_showLabels)
                {
                    labelText = data.Label;
                }
                else if (_showPercentages)
                {
                    labelText = $"{data.Percentage:F1}%";
                }
                
                if (!string.IsNullOrEmpty(labelText))
                {
                    var textSize = g.MeasureString(labelText, font);
                    var textX = labelX - textSize.Width / 2;
                    var textY = labelY - textSize.Height / 2;
                    
                    // 绘制半透明背景
                    var bgColor = _isDarkTheme ? Color.FromArgb(180, 0, 0, 0) : Color.FromArgb(180, 255, 255, 255);
                    using var bgBrush = new SolidBrush(bgColor);
                    g.FillRectangle(bgBrush, textX - 2, textY - 2, textSize.Width + 4, textSize.Height + 4);
                    
                    // 绘制文本
                    g.DrawString(labelText, font, brush, textX, textY);
                }
                
                startAngle += sweepAngle;
            }
        }

        #endregion
    }

    /// <summary>
    /// 饼图数据项
    /// </summary>
    public class PieChartData
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public double Percentage { get; set; }
        public Color Color { get; set; }
    }
}