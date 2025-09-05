using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

            _prevIdOrder = [.. _animatingInfoBuffer.Select(e => e.ID)];

            foreach (var item in _animatingInfoBuffer) 
            {
                // TODO
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
                    var tmplate = SlotDataTemplate.LoadContent() as FrameworkElement;
                    tmplate!.DataContext = data.Value.Data;
                    ProgressBarListBox.Children.Add(tmplate);

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
                .OrderByDescending(e => e.Data.ProgressBarValue)
                .Select(e =>
                {
                    if (e.ToIndex == -1) return e;

                    e.ToIndex = tmpIndex++;
                    e.Data = _dataDict[e.ID];
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
