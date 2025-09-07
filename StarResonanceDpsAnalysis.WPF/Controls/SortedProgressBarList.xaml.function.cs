using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

using StarResonanceDpsAnalysis.Core.Extends.System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    public partial class SortedProgressBarList
    {
        private readonly object _dataLock = new();
        private bool _animating = false;
        private List<long> _prevIdOrder = [];
        private List<SortAnimatingInfo> _animatingInfoBuffer = [];

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
            Debug.WriteLine($"Ainmation Start");

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
                        Top = (ProgressBarHeight + ProgressBarMargin.Top + ProgressBarMargin.Bottom) * item.ToIndex,
                        Bottom = 0,
                        Left = 0,
                        Right = 0
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
                        Debug.WriteLine($"Ainmation End");

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
                // 如果新增数据, 则动画出现
                if (!_animatingInfoBuffer.Any(e => e.ID == data.Key))
                {
                    var template = ProgressBarSlotDataTemplate.LoadContent() as FrameworkElement;
                    template!.DataContext = data.Value.Data;

                    var progressBarMargin = new Thickness(0, ProgressBarListBox.ActualHeight, 0, 0).Add(ProgressBarMargin);
                    var progressBar = new CustomizeProgressBar
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Height = ProgressBarHeight,
                        Value = data.Value.ProgressBarValue,
                        Slot = template
                    };

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
            }

            var tmpIndex = 0;
            _animatingInfoBuffer = [.. _animatingInfoBuffer
                .OrderByDescending(e => e.Data.ProgressBarValue)
                .Select(e =>
                {
                    if (e.ToIndex == -1) return e;

                    e.ToIndex = tmpIndex++;

                    var ele = (CustomizeProgressBar)e.UIElement;
                    var data = _dataDict[e.ID];
                    ((FrameworkElement)ele.Slot).DataContext = data.Data;
                    ele.Value = data.ProgressBarValue;
                    e.Data = data;
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

        private struct SortAnimatingInfo
        {
            public long ID { get; set; }
            public int FromIndex { get; set; }
            public int ToIndex { get; set; }

            public FrameworkElement UIElement { get; set; }
            public ProgressBarData Data { get; set; }
        }
    }

    public class ProgressBarData
    {
        public long ID { get; set; }
        public double ProgressBarValue { get; set; }
        public SolidColorBrush ProgressBarBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x56, 0x9C, 0xD6));
        public double ProgressBarCornerRadius { get; set; }
        public object? Data { get; set; }
    }
}
