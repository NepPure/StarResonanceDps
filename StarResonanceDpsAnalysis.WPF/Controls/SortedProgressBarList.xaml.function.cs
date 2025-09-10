using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

using StarResonanceDpsAnalysis.Core.Extends.System.Windows;
using StarResonanceDpsAnalysis.WPF.Controls.Models;

namespace StarResonanceDpsAnalysis.WPF.Controls;

public partial class SortedProgressBarList
{
    private readonly object _dataLock = new();
    private bool _animating;
    private List<SortAnimatingInfo> _animatingInfoBuffer = [];
    private List<long> _prevIdOrder = [];

    private void UpdateAnimation()
    {
        if (_animating)
        {
            return;
        }

        var flag = Resort();
        if (!flag)
        {
            return;
        }

        _animating = true;

        _prevIdOrder = [.. _animatingInfoBuffer.Select(e => e.ID)];

        var hasAnimation = false;
        foreach (var item in _animatingInfoBuffer)
        {
            if (item.FromIndex == item.ToIndex) continue;

            var progressBar = item.UIElement;
            var mta = new ThicknessAnimation
            {
                To = new Thickness
                {
                    Top = ProgressBarMargin.Top +
                          (ProgressBarHeight + ProgressBarMargin.Top + ProgressBarMargin.Bottom) * item.ToIndex,
                    Bottom = 0,
                    Left = ProgressBarMargin.Left,
                    Right = ProgressBarMargin.Right
                },
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            var mtaClock = mta.CreateClock();
            mtaClock.Completed += (s, e) =>
            {
                if (item.ToIndex == -1)
                {
                    ProgressBarListBox.Children.Remove(progressBar);
                }

                lock (_dataLock)
                {
                    _animating = false;
                    Debug.WriteLine("Animation End");

                    UpdateAnimation();
                }
            };
            progressBar.ApplyAnimationClock(MarginProperty, mtaClock);

            hasAnimation = true;
        }

        if (!hasAnimation)
        {
            _animating = false;
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
            if (_animatingInfoBuffer.Any(e => e.ID == data.Key)) continue;

            // 如果新增数据, 则动画出现
            var template = ProgressBarSlotDataTemplate.LoadContent() as FrameworkElement;
            template!.DataContext = data.Value.Data;

            var progressBarMargin = new Thickness(0, ProgressBarListBox.ActualHeight, 0, 0).Add(ProgressBarMargin);
            var progressBar = new CustomizeProgressBar
            {
                VerticalAlignment = VerticalAlignment.Top,
                Height = ProgressBarHeight,
                Data = data.Value,
                Value = data.Value.ProgressBarValue,
                Slot = template
            };
            progressBar.MouseEnter += ProgressBar_MouseEnter;
            progressBar.MouseMove += ProgressBar_MouseMove;
            progressBar.MouseLeave += ProgressBar_MouseLeave;
            progressBar.MouseDown += ProgressBar_MouseDown;

            ProgressBarListBox.Children.Add(progressBar);

            _animatingInfoBuffer.Add(new SortAnimatingInfo
            {
                ID = data.Key,
                FromIndex = -1,
                ToIndex = 0,
                Data = data.Value,
                UIElement = progressBar
            });

            // 记录有变更
            result = true;
        }

        // 重新排序
        var tmpIndex = 0;
        _animatingInfoBuffer =
        [
            .. _animatingInfoBuffer
                .OrderByDescending(e => e.Data.ProgressBarValue)
                .Select(info =>
                {
                    // 即将消失的项目, 不参与排序
                    if (info.ToIndex == -1) return info;

                    info.ToIndex = tmpIndex++;

                    var ele = (CustomizeProgressBar)info.UIElement;
                    var pbData = _dataDict[info.ID];

                    if (pbData.Data != null)
                    {
                        pbData.Data.Order = info.ToIndex + 1;
                    }

                    ((FrameworkElement)ele.Slot).DataContext = pbData.Data;
                    ele.Value = pbData.ProgressBarValue;
                    info.Data = pbData;
                    return info;
                })
        ];

        return result || CompareOrder();
    }

    private void ProgressBar_MouseEnter(object sender, MouseEventArgs e)
    {
        var progressBar = (CustomizeProgressBar)sender;
        ProgressBarMouseEnter?.Invoke(progressBar, e, progressBar.Data);
    }

    private void ProgressBar_MouseMove(object sender, MouseEventArgs e)
    {
        var progressBar = (CustomizeProgressBar)sender;
        ProgressBarMouseMove?.Invoke(progressBar, e, progressBar.Data);
    }

    private void ProgressBar_MouseLeave(object sender, MouseEventArgs e)
    {
        var progressBar = (CustomizeProgressBar)sender;
        ProgressBarMouseLeave?.Invoke(progressBar, e, progressBar.Data);
    }

    private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var progressBar = (CustomizeProgressBar)sender;
        ProgressBarMouseDown?.Invoke(progressBar, e, progressBar.Data);
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

    private struct SortAnimatingInfo
    {
        public long ID { get; init; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }

        public FrameworkElement UIElement { get; init; }
        public ProgressBarData Data { get; set; }
    }
}