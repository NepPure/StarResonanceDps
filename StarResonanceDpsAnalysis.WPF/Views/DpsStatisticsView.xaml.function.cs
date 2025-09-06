using StarResonanceDpsAnalysis.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace StarResonanceDpsAnalysis.WPF.Views
{
    public partial class DpsStatisticsView
    {
        public class SkillItem
        {
            public string SkillName { get; set; }
            public string TotalDamage { get; set; }
            public int HitCount { get; set; }
            public int CritCount { get; set; }
            public int AvgDamage { get; set; }
        }

        // 用于 DataTemplate 绑定的数据载体（挂到 ProgressBarData.Data 上）
        public class PlayerSlot
        {
            public string Name { get; set; }        // 序号或排名显示，例如 "01."
            public string Nickname { get; set; }    // “惊奇猫猫盒-狼弓(23207)”
            public string Profession { get; set; }  // 职业（用于颜色映射）
            public string Icon { get; set; }        // 图标 pack:// 路径
            public string ValueText { get; set; }   // 右侧数值，例如 "2.24万(4603.4)"
        }

        private readonly Random _rd = new Random();
        private readonly long[] _totals = new long[6]; // 6位玩家示例
        private readonly List<ProgressBarData> _slots = new();
        private DispatcherTimer _timer;

        private void InitDemoProgressBars()
        {
            // 1) 职业 → 颜色映射（你可以按项目规范调整）
            var professionBrush = new Dictionary<string, Color>
            {
                ["弓手"] = Color.FromRgb(66, 133, 244), // 蓝
                ["战士"] =Color.FromRgb(234, 67, 53), // 红
                ["法师"] = Color.FromRgb(155, 81, 224), // 紫
                ["牧师"] = Color.FromRgb(15, 157, 88), // 绿
                ["刺客"] = Color.FromRgb(244, 180, 0), // 金
                ["骑士"] =Color.FromRgb(95, 99, 104), // 灰
            };

            // 2) 造几位玩家（随便举例，图标请换成你项目里存在的）
            var players = new (string Nick, string Profession, string Icon)[]
            {
        ("惊奇猫猫盒-狼弓(23207)", "弓手", "/Assets/Images/神射手.png"),
        ("无双重剑-测试(19876)",  "战士", "/Assets/Images/巨刃守护者.png"),
        ("奥术回响-测试(20111)",  "法师", "/Assets/Images/雷影剑士.png"),
        ("圣光之约-测试(18770)",  "牧师", "/Assets/Images/灵魂乐手.png"),
        ("影袭-测试(20990)",      "刺客", "/Assets/Images/森语者.png"),
        ("圣盾壁垒-测试(17654)",  "骑士", "/Assets/Images/神盾骑士.png"),
            };

            _slots.Clear();
            for (int i = 0; i < players.Length; i++)
            {
                var (nick, prof, icon) = players[i];

                // 初始化一点基础值，避免全部为0
                _totals[i] = _rd.Next(2_000, 8_000);

                var slotData = new PlayerSlot
                {
                    Name = $"{i + 1:00}.",   // 01. 02. ...
                    Nickname = nick,
                    Profession = prof,
                    Icon = icon,
                    ValueText = "24.81万(2456.0)"
                };
                var color = professionBrush.TryGetValue(prof, out var c) ? c : Colors.SteelBlue;

                _slots.Add(new ProgressBarData
                {
                    ID = i,
                    ProgressBarBrush = new SolidColorBrush(color), // 每个条目单独的 Brush
                    ProgressBarCornerRadius = 5,
                    ProgressBarValue = 0, // 先给0，等会定时器里刷新
                    Data = slotData
                });
            }

            // 先给一次
            ProgressBarList.Data = _slots;

            // 3) 定时器：实时更新
            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (_, __) => UpdateBars();
            _timer.Start();
        }

        private void UpdateBars()
        {
            // 随机增长各自总伤
            for (int i = 0; i < _totals.Length; i++)
                _totals[i] += _rd.Next(10, 20);

            var max = Math.Max(1, _totals.Max()); // 防止除0

            // 计算“每秒值”举例：取最近随机的一点点变化，示意 dps
            // 这里简化：用一个近似（0.4~0.8）* (当下相对值*1w)，只做展示
            for (int i = 0; i < _slots.Count; i++)
            {
                var bar = _slots[i];
                var total = _totals[i];
                var ratio = (double)total / max;       // 0~1
                bar.ProgressBarValue = ratio;          // 控制条的长度

                // 右侧文本：总伤(每秒)
                var approxPerSec = ratio * 10000 * (0.4 + 0.4 * _rd.NextDouble());
                var valueText = $"{FormatWan(total)}({approxPerSec:0.0})";

                // 更新 Data 里的文本（绑定会刷新）
                if (bar.Data is PlayerSlot p)
                {
                    p.ValueText = valueText;
                    // 也可以顺带更新 Name 为名次，但需要排序后再写（见可选排序）
                }
            }

            // 可选：按照总伤排序（若你的控件会按 Data 输入顺序渲染）
            // 如果 SortedProgressBarList 自己会排序，则可不要这段
            var ordered = _slots.Zip(_totals, (bar, total) => (bar, total))
                                .OrderByDescending(x => x.total)
                                .Select(x => x.bar)
                                .ToList();

            // 更新名次显示（01. 02. ...）
            for (int rank = 0; rank < ordered.Count; rank++)
            {
                if (ordered[rank].Data is PlayerSlot p)
                    p.Name = $"{rank + 1:00}.";
            }

            // 把排序后的列表重新赋值（若控件 Data 是 IEnumerable，并允许替换）
            // 如果你的控件支持就地更新而不需要替换，也可以直接 ProgressBarList.Data = _slots;
            ProgressBarList.Data = ordered;
        }

        // 小工具：格式化万（示例：22400 -> 2.24万）
        private static string FormatWan(long n)
        {
            if (n >= 10_000) return $"{(n / 10000.0):0.##}万";
            return n.ToString();
        }
    }
}
