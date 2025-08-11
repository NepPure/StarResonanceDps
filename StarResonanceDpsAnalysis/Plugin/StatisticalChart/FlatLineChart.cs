using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Plugin.Charts
{
    /// <summary>
    /// 扁平化线性图控件 - 支持拖动和缩放功能
    /// </summary>
    public class FlatLineChart : UserControl
    {
        #region 字段和属性

        private readonly List<LineChartSeries> _series = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private string _xAxisLabel = "";
        private string _yAxisLabel = "";
        private bool _showLegend = true;
        private bool _showGrid = true;

        // 边距设置
        private const int PaddingLeft = 80;
        private const int PaddingRight = 30;
        private const int PaddingTop = 50;
        private const int PaddingBottom = 90;

        // 缩放和视图相关
        private float _timeScale = 1.0f;           // 时间轴缩放因子
        private float _viewOffset = 0.0f;          // 视图偏移（秒）
        private float _currentTimeSeconds = 0.0f;  // 当前时间（秒）
        
        // 数据持久化
        private readonly Dictionary<string, List<PointF>> _persistentData = new();

        // 鼠标交互相关
        private Point _lastMousePosition;
        private bool _isPanning = false;
        private ToolTip _tooltip;
        private bool _showTooltip = false;
        private string _tooltipText = "";

        // 颜色配置
        private readonly Color[] _colors = {
            Color.FromArgb(255, 99, 132),   // 红
            Color.FromArgb(54, 162, 235),   // 蓝
            Color.FromArgb(255, 206, 86),   // 黄
            Color.FromArgb(75, 192, 192),   // 青
            Color.FromArgb(153, 102, 255),  // 紫
            Color.FromArgb(255, 159, 64),   // 橙
            Color.FromArgb(199, 199, 199),  // 灰
            Color.FromArgb(83, 102, 255),   // 靛蓝
            Color.FromArgb(255, 99, 255),   // 品红
            Color.FromArgb(99, 255, 132),   // 绿
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

        public string XAxisLabel
        {
            get => _xAxisLabel;
            set
            {
                _xAxisLabel = value;
                Invalidate();
            }
        }

        public string YAxisLabel
        {
            get => _yAxisLabel;
            set
            {
                _yAxisLabel = value;
                Invalidate();
            }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set
            {
                _showLegend = value;
                Invalidate();
            }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                Invalidate();
            }
        }

        #endregion

        #region 构造函数

        public FlatLineChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw | 
                     ControlStyles.Selectable | ControlStyles.UserMouse, true);
            
            // 初始化工具提示
            _tooltip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 100,
                ReshowDelay = 500,
                ShowAlways = true,
                IsBalloon = true
            };

            ApplyTheme();
            
            // 注册鼠标事件
            MouseMove += OnChartMouseMove;
            MouseWheel += OnChartMouseWheel;
            MouseDown += OnChartMouseDown;
            MouseUp += OnChartMouseUp;
            MouseEnter += OnChartMouseEnter;
            KeyDown += OnChartKeyDown;
            
            // 允许控件接收焦点以处理键盘事件
            TabStop = true;
        }

        #endregion

        #region 数据管理

        public void AddSeries(string name, List<PointF> points)
        {
            // 持久化保存数据
            _persistentData[name] = new List<PointF>(points);
            
            var series = new LineChartSeries
            {
                Name = name,
                Points = new List<PointF>(points),
                Color = _colors[_series.Count % _colors.Length],
                LineWidth = 3.5f
            };
            
            _series.Add(series);
            
            // 更新当前时间
            if (points.Count > 0)
            {
                _currentTimeSeconds = Math.Max(_currentTimeSeconds, points.Max(p => p.X));
            }
            
            Invalidate();
        }

        public void ClearSeries()
        {
            _series.Clear();
            // 不清空持久化数据，防止数据丢失
            // _persistentData.Clear(); // 注释掉这行
            ResetViewToDefault();
            Invalidate();
        }

        public void UpdateSeries(string name, List<PointF> points)
        {
            // 更新持久化数据
            _persistentData[name] = new List<PointF>(points);
            
            var series = _series.FirstOrDefault(s => s.Name == name);
            if (series != null)
            {
                series.Points = new List<PointF>(points);
                
                // 更新当前时间
                if (points.Count > 0)
                {
                    _currentTimeSeconds = Math.Max(_currentTimeSeconds, points.Max(p => p.X));
                }
                
                Invalidate();
            }
        }

        /// <summary>
        /// 强制重新加载持久化数据（解决数据停止时线条消失问题）
        /// </summary>
        public void ReloadPersistentData()
        {
            _series.Clear();
            int colorIndex = 0;
            
            foreach (var kvp in _persistentData)
            {
                var series = new LineChartSeries
                {
                    Name = kvp.Key,
                    Points = new List<PointF>(kvp.Value),
                    Color = _colors[colorIndex % _colors.Length],
                    LineWidth = 3.5f
                };
                _series.Add(series);
                colorIndex++;
            }
            
            Invalidate();
        }

        #endregion

        #region 视图控制

        /// <summary>
        /// 设置时间轴缩放（以当前时间为中心）
        /// </summary>
        public void SetTimeScale(float scale)
        {
            var oldScale = _timeScale;
            _timeScale = Math.Max(0.1f, Math.Min(10.0f, scale));
            
            // 以当前时间为中心调整视图偏移
            var centerTime = _currentTimeSeconds;
            var oldViewWidth = GetViewTimeRange(oldScale);
            var newViewWidth = GetViewTimeRange(_timeScale);
            
            // 调整偏移以保持当前时间在视图中心
            _viewOffset = centerTime - newViewWidth / 2;
            
            // 限制视图不能超过当前时间
            ClampViewOffset();
            
            Invalidate();
        }

        /// <summary>
        /// 设置视图偏移（秒）
        /// </summary>
        public void SetViewOffset(float offset)
        {
            _viewOffset = offset;
            ClampViewOffset();
            Invalidate();
        }

        /// <summary>
        /// 重置视图到默认状态
        /// </summary>
        public void ResetViewToDefault()
        {
            _timeScale = 1.0f;
            _viewOffset = Math.Max(0, _currentTimeSeconds - 60); // 显示最近60秒
            ClampViewOffset();
            Invalidate();
        }

        /// <summary>
        /// 重置缩放和平移
        /// </summary>
        public void ResetZoomAndPan()
        {
            ResetViewToDefault();
        }

        /// <summary>
        /// 限制视图偏移不超过当前时间
        /// </summary>
        private void ClampViewOffset()
        {
            var viewWidth = GetViewTimeRange(_timeScale);
            var maxOffset = _currentTimeSeconds - viewWidth;
            var minOffset = Math.Max(0, _currentTimeSeconds - 300); // 最多回看5分钟
            
            _viewOffset = Math.Max(minOffset, Math.Min(maxOffset, _viewOffset));
        }

        /// <summary>
        /// 获取当前视图的时间范围
        /// </summary>
        private float GetViewTimeRange(float scale)
        {
            return 60.0f / scale; // 基础60秒范围
        }

        #endregion

        #region 主题管理

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

        #region 鼠标事件处理

        private void OnChartMouseEnter(object sender, EventArgs e)
        {
            // 鼠标进入时自动获取焦点
            if (!Focused)
            {
                Focus();
            }
        }

        private void OnChartMouseMove(object sender, MouseEventArgs e)
        {
            var chartRect = new Rectangle(PaddingLeft, PaddingTop, 
                                        Width - PaddingLeft - PaddingRight,
                                        Height - PaddingTop - PaddingBottom);

            if (chartRect.Contains(e.Location))
            {
                // 处理平移
                if (_isPanning && e.Button == MouseButtons.Left)
                {
                    var deltaX = e.X - _lastMousePosition.X;
                    var timeRange = GetViewTimeRange(_timeScale);
                    var timeDelta = -deltaX * timeRange / chartRect.Width;
                    
                    SetViewOffset(_viewOffset + timeDelta);
                    _lastMousePosition = e.Location;
                    return;
                }

                // 查找鼠标附近的数据点
                FindNearestDataPoint(e.Location, chartRect);
            }
            else
            {
                HideTooltip();
            }

            _lastMousePosition = e.Location;
        }

        private void OnChartMouseWheel(object sender, MouseEventArgs e)
        {
            // 确保控件有焦点
            if (!Focused)
            {
                Focus();
            }

            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                var chartRect = new Rectangle(PaddingLeft, PaddingTop, 
                                            Width - PaddingLeft - PaddingRight,
                                            Height - PaddingTop - PaddingBottom);

                if (chartRect.Contains(e.Location))
                {
                    var scaleDelta = e.Delta > 0 ? 1.1f : 0.9f;
                    SetTimeScale(_timeScale * scaleDelta);
                }
            }
        }

        private void OnChartMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 确保控件有焦点
                if (!Focused)
                {
                    Focus();
                }

                _isPanning = true;
                _lastMousePosition = e.Location;
                Cursor = Cursors.Hand;
            }
        }

        private void OnChartMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = false;
                Cursor = Cursors.Default;
            }
        }

        private void OnChartKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                ResetViewToDefault();
                e.Handled = true;
            }
        }

        #endregion

        #region 数据点查找和提示

        private void FindNearestDataPoint(Point mouseLocation, Rectangle chartRect)
        {
            if (_series.Count == 0) return;

            var viewRange = CalculateViewRange();
            if (viewRange.IsEmpty) return;

            var minDistance = double.MaxValue;
            string bestTooltip = "";
            var found = false;

            foreach (var series in _series)
            {
                if (series.Points.Count == 0) continue;

                foreach (var point in series.Points)
                {
                    // 检查点是否在当前视图范围内
                    if (point.X < viewRange.X || point.X > viewRange.X + viewRange.Width)
                        continue;

                    var screenX = chartRect.X + ((point.X - viewRange.X) / viewRange.Width) * chartRect.Width;
                    var screenY = chartRect.Bottom - (point.Y - viewRange.Y) / viewRange.Height * chartRect.Height;

                    var distance = Math.Sqrt(Math.Pow(mouseLocation.X - screenX, 2) + Math.Pow(mouseLocation.Y - screenY, 2));

                    if (distance < 15 && distance < minDistance)
                    {
                        minDistance = distance;
                        var timeText = FormatTimeLabel(point.X);
                        var dpsText = Common.FormatWithEnglishUnits(point.Y);
                        bestTooltip = $"{series.Name}\n时间: {timeText}\nDPS: {dpsText}";
                        found = true;
                    }
                }
            }

            if (found)
            {
                ShowTooltip(bestTooltip, mouseLocation);
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip(string text, Point location)
        {
            if (_tooltipText != text)
            {
                _tooltipText = text;
                _showTooltip = true;
                _tooltip.Show(text, this, location.X + 10, location.Y - 30, 3000);
            }
        }

        private void HideTooltip()
        {
            if (_showTooltip)
            {
                _showTooltip = false;
                _tooltip.Hide(this);
            }
        }

        #endregion

        #region 重写方法以确保焦点处理

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus(); // 点击时获取焦点
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // 处理键盘事件
            if (keyData == Keys.R)
            {
                ResetViewToDefault();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // 确保这些键被控件处理
            if (keyData == Keys.R)
                return true;
            return base.IsInputKey(keyData);
        }

        #endregion

        #region 绘制方法

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.Clear(BackColor);

            if (_series.Count == 0)
            {
                DrawNoDataMessage(g);
                return;
            }

            var viewRange = CalculateViewRange();
            if (viewRange.IsEmpty) return;

            var chartRect = new Rectangle(PaddingLeft, PaddingTop, 
                                        Width - PaddingLeft - PaddingRight,
                                        Height - PaddingTop - PaddingBottom);

            if (_showGrid)
            {
                DrawGrid(g, chartRect, viewRange);
            }

            DrawAxes(g, chartRect, viewRange);

            var clipRect = new Rectangle(chartRect.X - 1, chartRect.Y - 1, 
                                        chartRect.Width + 2, chartRect.Height + 2);
            g.SetClip(clipRect);

            DrawDataLines(g, chartRect, viewRange);
            g.ResetClip();

            DrawTitle(g);
            DrawViewInfo(g);

            if (_showLegend && _series.Count > 0)
            {
                DrawLegend(g);
            }
        }

        private void DrawNoDataMessage(Graphics g)
        {
            var message = "暂无数据\n\n使用方法：\n? Ctrl + 鼠标滚轮：缩放时间轴\n? 左键拖动：平移视图\n? R键：重置视图\n? 鼠标悬停：查看数据";
            using var font = new Font("Microsoft YaHei", 10, FontStyle.Regular);
            using var brush = new SolidBrush(_isDarkTheme ? Color.Gray : Color.DarkGray);
            
            var size = g.MeasureString(message, font);
            var x = (Width - size.Width) / 2;
            var y = (Height - size.Height) / 2;
            
            g.DrawString(message, font, brush, x, y);
        }

        private RectangleF CalculateViewRange()
        {
            if (_series.Count == 0) return RectangleF.Empty;

            var allPoints = _series.SelectMany(s => s.Points);
            if (!allPoints.Any()) return RectangleF.Empty;

            var minY = 0f;
            var maxY = allPoints.Max(p => p.Y);
            var rangeY = maxY - minY;
            if (rangeY == 0) rangeY = 1;

            var viewWidth = GetViewTimeRange(_timeScale);
            var viewMinX = _viewOffset;

            return new RectangleF(
                viewMinX,
                minY,
                viewWidth,
                rangeY * 1.15f
            );
        }

        private void DrawGrid(Graphics g, Rectangle chartRect, RectangleF viewRange)
        {
            var gridColor = _isDarkTheme ? Color.FromArgb(64, 64, 64) : Color.FromArgb(230, 230, 230);
            using var gridPen = new Pen(gridColor, 1);

            for (int i = 0; i <= 10; i++)
            {
                var x = chartRect.X + (float)chartRect.Width * i / 10;
                g.DrawLine(gridPen, x, chartRect.Y, x, chartRect.Bottom);
            }

            for (int i = 0; i <= 10; i++)
            {
                var y = chartRect.Y + (float)chartRect.Height * i / 10;
                g.DrawLine(gridPen, chartRect.X, y, chartRect.Right, y);
            }
        }

        private void DrawAxes(Graphics g, Rectangle chartRect, RectangleF viewRange)
        {
            var axisColor = _isDarkTheme ? Color.FromArgb(128, 128, 128) : Color.FromArgb(180, 180, 180);
            using var axisPen = new Pen(axisColor, 1);
            using var textBrush = new SolidBrush(ForeColor);
            using var font = new Font("Microsoft YaHei", 8);

            g.DrawLine(axisPen, chartRect.X, chartRect.Bottom, chartRect.Right, chartRect.Bottom);
            g.DrawLine(axisPen, chartRect.X, chartRect.Y, chartRect.X, chartRect.Bottom);

            // X轴标签 - 智能时间格式
            for (int i = 0; i <= 8; i++)
            {
                var x = chartRect.X + (float)chartRect.Width * i / 8;
                var timeValue = viewRange.X + viewRange.Width * i / 8;
                var text = FormatTimeLabel(timeValue);
                
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, x - size.Width / 2, chartRect.Bottom + 8);
            }

            // Y轴标签
            for (int i = 0; i <= 8; i++)
            {
                var y = chartRect.Bottom - (float)chartRect.Height * i / 8;
                var value = viewRange.Y + viewRange.Height * i / 8;
                var text = Common.FormatWithEnglishUnits(value);
                
                var size = g.MeasureString(text, font);
                var labelX = Math.Max(5, chartRect.X - size.Width - 8);
                g.DrawString(text, font, textBrush, labelX, y - size.Height / 2);
            }

            if (!string.IsNullOrEmpty(_xAxisLabel))
            {
                using var axisFont = new Font("Microsoft YaHei", 9);
                var size = g.MeasureString(_xAxisLabel, axisFont);
                var x = chartRect.X + (chartRect.Width - size.Width) / 2;
                var y = chartRect.Bottom + 45;
                g.DrawString(_xAxisLabel, axisFont, textBrush, x, y);
            }

            if (!string.IsNullOrEmpty(_yAxisLabel))
            {
                using var axisFont = new Font("Microsoft YaHei", 9);
                var size = g.MeasureString(_yAxisLabel, axisFont);
                using var matrix = new Matrix();
                matrix.RotateAt(-90, new PointF(20, chartRect.Y + (chartRect.Height + size.Width) / 2));
                g.Transform = matrix;
                g.DrawString(_yAxisLabel, axisFont, textBrush, 20, chartRect.Y + (chartRect.Height + size.Width) / 2);
                g.ResetTransform();
            }
        }

        /// <summary>
        /// 智能时间标签格式化
        /// </summary>
        private string FormatTimeLabel(float seconds)
        {
            if (seconds < 60)
            {
                return $"{seconds:F0}s";
            }
            else
            {
                var minutes = (int)(seconds / 60);
                var remainingSeconds = (int)(seconds % 60);
                return $"{minutes}m{remainingSeconds:D2}s";
            }
        }

        private void DrawDataLines(Graphics g, Rectangle chartRect, RectangleF viewRange)
        {
            foreach (var series in _series)
            {
                if (series.Points.Count < 2) continue;

                using var pen = new Pen(series.Color, series.LineWidth);
                pen.LineJoin = LineJoin.Round;
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                // 过滤视图范围内的点
                var visiblePoints = series.Points
                    .Where(p => p.X >= viewRange.X && p.X <= viewRange.X + viewRange.Width)
                    .Select(p => {
                        var screenX = chartRect.X + ((p.X - viewRange.X) / viewRange.Width) * chartRect.Width;
                        var screenY = chartRect.Bottom - (p.Y - viewRange.Y) / viewRange.Height * chartRect.Height;
                        
                        screenX = Math.Max(chartRect.X, Math.Min(chartRect.Right, screenX));
                        screenY = Math.Max(chartRect.Y, Math.Min(chartRect.Bottom, screenY));
                        
                        return new PointF(screenX, screenY);
                    }).ToArray();

                if (visiblePoints.Length < 2) continue;

                try
                {
                    if (visiblePoints.Length >= 3)
                    {
                        g.DrawCurve(pen, visiblePoints, 0.6f);
                    }
                    else
                    {
                        g.DrawLines(pen, visiblePoints);
                    }
                }
                catch
                {
                    for (int i = 0; i < visiblePoints.Length - 1; i++)
                    {
                        try
                        {
                            g.DrawLine(pen, visiblePoints[i], visiblePoints[i + 1]);
                        }
                        catch { }
                    }
                }
            }
        }

        private void DrawTitle(Graphics g)
        {
            if (string.IsNullOrEmpty(_titleText)) return;

            using var font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
            using var brush = new SolidBrush(ForeColor);
            
            var size = g.MeasureString(_titleText, font);
            var x = (Width - size.Width) / 2;
            var y = 15;
            
            g.DrawString(_titleText, font, brush, x, y);
        }

        private void DrawViewInfo(Graphics g)
        {
            var info = $"缩放: {_timeScale:F1}x | 当前时间: {FormatTimeLabel(_currentTimeSeconds)}";
            
            using var font = new Font("Microsoft YaHei", 8);
            using var brush = new SolidBrush(_isDarkTheme ? Color.LightGray : Color.DarkGray);
            
            var size = g.MeasureString(info, font);
            g.DrawString(info, font, brush, Width - size.Width - 10, Height - size.Height - 5);
        }

        private void DrawLegend(Graphics g)
        {
            using var font = new Font("Microsoft YaHei", 8);
            using var textBrush = new SolidBrush(ForeColor);
            
            var legendHeight = _series.Count * 18 + 10;
            var maxTextWidth = _series.Max(s => (int)g.MeasureString(s.Name, font).Width);
            var legendWidth = maxTextWidth + 35;
            var legendX = Width - legendWidth - 15;
            var legendY = PaddingTop + 15;

            var legendBg = _isDarkTheme ? Color.FromArgb(50, 50, 50) : Color.FromArgb(245, 245, 245);
            using var bgBrush = new SolidBrush(legendBg);
            using var borderPen = new Pen(_isDarkTheme ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200), 1);
            
            var legendRect = new Rectangle(legendX - 8, legendY - 8, legendWidth + 6, legendHeight + 6);
            g.FillRectangle(bgBrush, legendRect);
            g.DrawRectangle(borderPen, legendRect);

            for (int i = 0; i < _series.Count; i++)
            {
                var series = _series[i];
                var y = legendY + i * 18;

                using var colorPen = new Pen(series.Color, 3);
                g.DrawLine(colorPen, legendX, y + 7, legendX + 20, y + 7);
                g.DrawString(series.Name, font, textBrush, legendX + 25, y + 2);
            }
        }

        #endregion

        #region 资源清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    /// 线性图数据系列
    /// </summary>
    public class LineChartSeries
    {
        public string Name { get; set; } = "";
        public List<PointF> Points { get; set; } = new();
        public Color Color { get; set; }
        public float LineWidth { get; set; } = 3.5f;
    }
}