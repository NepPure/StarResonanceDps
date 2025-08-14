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
        private static readonly Dictionary<Quality, int> _animationFpsQuality = new()
        {
            { Quality.VeryLow, 10 },
            { Quality.Low, 20 },
            { Quality.Medium, 30 },
            { Quality.High, 60 },
            { Quality.VeryHigh, 120 },
            { Quality.Extreme, 160 },
            { Quality.AlmostAccurate, 1000 }
        };
        private PeriodicTimer _animationPeriodicTimer;
        private CubicBezier _moveAnimationCubicBezier;
        private CubicBezier _fadeAnimationCubicBezier;

        private readonly object _lock = new();
        private readonly Dictionary<long, GDI_ProgressBar> _gdiProgressBarDict = [];
        private readonly DrawInfo _drawInfo = new();

        private Stopwatch _animationWatch = new();
        private bool _animating = false;
        private List<long> _prevIdOrder = [];
        private List<SortAnimatingInfo> _animatingInfoBuffer = [];

        private CancellationTokenSource? _animationCancellation = null;

        public void Redraw(PaintEventArgs e)
        {
            if (Width == 0 || Height == 0) return;

            lock (_lock)
            {
                var g = e.Graphics;

                g.Clear(BackColor);

                var aniMs = _animationWatch.ElapsedMilliseconds;

                if (_animating && aniMs > AnimationDuration)
                {
                    _animating = false;
                    Console.WriteLine($"动画结束");
                }

                if (!_animating)
                {
                    var flag = Resort();
                    if (flag)
                    {
                        _prevIdOrder = [.. _animatingInfoBuffer.Select(e => e.ID)];

                        _animationWatch.Restart();

                        _animating = true;
                        Console.WriteLine($"动画启动");
                    }
                }

                var staticDraw = aniMs >= AnimationDuration;

                foreach (var data in _animatingInfoBuffer)
                {
                    var outOfHeightIndex = (Height / ProgressBarHeight) + 1;
                    var fromIndex = data.FromIndex == -1
                        ? outOfHeightIndex
                        : data.FromIndex;
                    var toIndex = data.ToIndex == -1
                        ? outOfHeightIndex
                        : data.ToIndex;
                    float top = fromIndex * ProgressBarHeight;

                    if (data.FromIndex == data.ToIndex || staticDraw)
                    {
                        DrawProgressBar(g, data.Data, top, 255);
                    }
                    else
                    {
                        var timePersent = 1f * aniMs / AnimationDuration;
                        var moveBezier = _moveAnimationCubicBezier.GetProximateBezierValue(timePersent);
                        var fadeBezier = _fadeAnimationCubicBezier.GetProximateBezierValue(timePersent);

                        var opacity = byte.MaxValue;

                        if (data.FromIndex == -1)
                        {
                            opacity = (byte)(opacity * fadeBezier);
                        }
                        else if (data.ToIndex == -1)
                        {
                            opacity = (byte)(255 - opacity * fadeBezier);
                        }

                        top += ProgressBarHeight * (toIndex - fromIndex) * moveBezier;

                        DrawProgressBar(g, data.Data, top, opacity);
                    }
                }

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

            // 这里会删除ToIndex = -1, TODO
            var tmpIndex = 0;
            _animatingInfoBuffer = [.. _animatingInfoBuffer
                .OrderByDescending(e => e.Data.ProgressBarValue)
                .Select(e =>
                {
                    if (e.ToIndex == -1) return e;

                    e.ToIndex = tmpIndex++;
                    return e;
                })];

            return result || CompareOrder();
        }
        private bool CompareOrder()
        {
            if (_animatingInfoBuffer.Count != _prevIdOrder.Count) return true;

            for (var i = 0; i < _prevIdOrder.Count; ++i)
            {
                if (_animatingInfoBuffer[i].ID != _prevIdOrder[i]) return true;
            }

            return false;
        }

        private void InitAnimation()
        {
            _animationCancellation?.Cancel();
            _animationCancellation = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (await _animationPeriodicTimer.WaitForNextTickAsync(_animationCancellation.Token).ConfigureAwait(false))
                {
                    Invalidate();
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
            public long ID { get; set; }
            public int FromIndex { get; set; }
            public int ToIndex { get; set; }
            public ProgressBarData Data { get; set; }
        }
    }
}
