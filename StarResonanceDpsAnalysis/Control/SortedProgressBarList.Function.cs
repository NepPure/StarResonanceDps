using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarResonanceDpsAnalysis.Control.GDI;
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Effects.Enum;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SortedProgressBarList
    {
        private static readonly Dictionary<Quality, int> _animationDelayQuality = new()
        {
            { Quality.VeryLow, 100 },
            { Quality.Low, 50 },
            { Quality.Medium, 33 },
            { Quality.High, 16 },
            { Quality.VeryHigh, 8 },
            { Quality.Extreme, 6 },
            { Quality.AlmostAccurate, 1 }
        };
        private int _animationDelay = 33;
        private CubicBezier _moveAnimationCubicBezier;
        private CubicBezier _fadeAnimationCubicBezier;

        private readonly object _lock = new();
        private readonly Dictionary<int, GDI_ProgressBar> _gdiProgressBarDict = [];
        private readonly DrawInfo _drawInfo = new();

        private Stopwatch _animationWatch = new();
        private bool _animating = false;
        private List<int> _prevIdOrder = [];
        private List<SortAnimatingInfo> _animatingInfoBuffer = [];
        private Bitmap? _frameBuffer = null;

        public void Redraw(PaintEventArgs e)
        {
            if (Width == 0 || Height == 0) return;

            lock (_lock)
            {
                if (_frameBuffer == null || _frameBuffer.Width != Width || _frameBuffer.Height != Height)
                {
                    _frameBuffer?.Dispose();
                    _frameBuffer = new Bitmap(Width, Height);
                }

                using var g = Graphics.FromImage(_frameBuffer);

                g.Clear(BackColor);

                var aniMs = _animationWatch.ElapsedMilliseconds;

                if (_animating && aniMs > AnimationDuration)
                {
                    _animating = false;
                }

                if (!_animating)
                {
                    var flag = Resort();
                    if (!flag)
                    {
                        _prevIdOrder = [.. _animatingInfoBuffer.Select(e => e.ID)];

                        _animationWatch.Restart();

                        _animating = true;
                    }
                }

                var staticDraw = aniMs >= AnimationDuration;

                foreach (var data in _animatingInfoBuffer)
                {
                    float top = data.FromIndex * ProgressBarHeight;

                    if (data.FromIndex == data.ToIndex || staticDraw)
                    {
                        DrawProgressBar(g, data.Data, top, 255);
                    }
                    else
                    {
                        var timePersent = 1f * aniMs / AnimationDuration;
                        Console.WriteLine($"aniMs: {aniMs}, timePersent: {timePersent}");

                        var opacity = byte.MaxValue;
                        if (data.FromIndex == -1)
                        {
                            top = ProgressBarHeight * data.ToIndex * _moveAnimationCubicBezier.GetProximateBezierValue(timePersent);
                            Console.WriteLine($"text: {data.Data.Text}, top: {top}");
                            opacity = (byte)(opacity * _fadeAnimationCubicBezier.GetProximateBezierValue(timePersent));
                        }
                        else if (data.ToIndex == -1)
                        {
                            top += ProgressBarHeight * (_animatingInfoBuffer.Count - 1) * _moveAnimationCubicBezier.GetProximateBezierValue(timePersent);
                            Console.WriteLine($"text: {data.Data.Text}, top: {top}");
                            opacity = (byte)(255 - opacity * _fadeAnimationCubicBezier.GetProximateBezierValue(timePersent));
                        }
                        else
                        {
                            top += ProgressBarHeight * (data.ToIndex - data.FromIndex) * _moveAnimationCubicBezier.GetProximateBezierValue(timePersent);
                            Console.WriteLine($"text: {data.Data.Text}, top: {top}");
                        }

                        DrawProgressBar(g, data.Data, top, opacity);
                    }
                }

                e.Graphics.DrawImage(_frameBuffer, 0, 0, Width, Height);
            }
        }
        private bool Resort()
        {
            var result = false;

            // 已经经过消失动画的, 移除
            var removeCount = _animatingInfoBuffer.RemoveAll(a => a.ToIndex == -1);
            if (removeCount > 0) 
            {
                // 记录有变更
                result = true;
            }

            // 更新ToIndex
            for (var i = 0; i < _animatingInfoBuffer.Count; ++i)
            {
                var info = _animatingInfoBuffer[i];
                info.FromIndex = info.ToIndex;

                // 如果数据已经不存在, 则动画消失
                if (!_dataDict.ContainsKey(info.ID))
                {
                    info.ToIndex = -1;

                    // 记录有变更
                    result = true;
                }

                _animatingInfoBuffer[i] = info;
            }
            foreach (var data in _dataDict)
            {
                if (!_animatingInfoBuffer.Any(e => e.ID == data.Key))
                {
                    _animatingInfoBuffer.Add(new SortAnimatingInfo
                    {
                        ID = data.Key,
                        FromIndex = -1,
                        ToIndex = 0,
                        Data = data.Value
                    });

                    // 记录有变更
                    result = true;
                }
            }

            var tmpIndex = 0;
            _animatingInfoBuffer = [.. _animatingInfoBuffer
                .Where(e => e.ToIndex != -1)
                .OrderByDescending(e => e.Data.ProgressBarValue)
                .Select(e => // 潜在的问题
                {
                    e.ToIndex = ++tmpIndex;
                    return e;
                })];

            return result || CompareOrder();
        }
        private bool CompareOrder() 
        {
            if (_animatingInfoBuffer.Count != _prevIdOrder.Count) return false;

            for (var i = 0; i < _prevIdOrder.Count; ++i) 
            {
                if (_animatingInfoBuffer[i].ID != _prevIdOrder[i]) return false;
            }

            return true;
        }

        private void InitAnimation()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Invalidate();

                    Thread.Sleep(_animationDelay);
                }
            });
        }

        private void DrawProgressBar(Graphics g, ProgressBarData data, float top, byte opacity)
        {
            var flag = _gdiProgressBarDict.TryGetValue(data.ID, out var gdiProgressBar);
            if (!flag || gdiProgressBar == null)
            {
                gdiProgressBar = new GDI_ProgressBar();

                _gdiProgressBarDict[data.ID] = gdiProgressBar;
            }

            _drawInfo.Width = Width;
            _drawInfo.Height = ProgressBarHeight;
            _drawInfo.BackColor = BackColor;
            _drawInfo.ForeColor = data.ForeColor;
            _drawInfo.ProgressBarColor = Color.FromArgb(opacity, data.ProgressBarColor);
            _drawInfo.ProgressBarValue = data.ProgressBarValue;
            _drawInfo.Text = data.Text ?? string.Empty;
            _drawInfo.Font = Font;
            _drawInfo.ProgressBarCornerRadius = data.ProgressBarCornerRadius;
            _drawInfo.Padding = data.ProgressBarPadding;
            _drawInfo.TextPadding = data.TextPadding;
            _drawInfo.Top = top;

            gdiProgressBar.Draw(g, _drawInfo);
        }

        private struct SortAnimatingInfo
        {
            public int ID { get; set; }
            public int FromIndex { get; set; }
            public int ToIndex { get; set; }
            public ProgressBarData Data { get; set; }
        }
    }
}
