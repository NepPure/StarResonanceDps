using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.WPF.Controls;
using StarResonanceDpsAnalysis.WPF.ViewModels;
using Color = System.Windows.Media.Color;

namespace StarResonanceDpsAnalysis.WPF.Views;

public partial class DpsStatisticsView
{
    private readonly Random _rd = new();
    private readonly List<ProgressBarData> _slots = [];
    private readonly long[] _totals = new long[6]; // 6位玩家示例
    private DispatcherTimer _timer = null!;

    private void InitDemoProgressBars()
    {
        // 1) 职业 → 颜色映射（你可以按项目规范调整）
        var professionBrush = new Dictionary<string, Color>
        {
            ["弓手"] = Color.FromRgb(66, 133, 244), // 蓝
            ["战士"] = Color.FromRgb(234, 67, 53), // 红
            ["法师"] = Color.FromRgb(155, 81, 224), // 紫
            ["牧师"] = Color.FromRgb(15, 157, 88), // 绿
            ["刺客"] = Color.FromRgb(244, 180, 0), // 金
            ["骑士"] = Color.FromRgb(95, 99, 104) // 灰
        };

        // 2) 造几位玩家（随便举例，图标请换成你项目里存在的）
        var players = new (string Nick, string Profession, string IconPath)[]
        {
            ("惊奇猫猫盒-狼弓(23207)", "弓手", "pack://application:,,,/Assets/Images/Profession_SSS.png"),
            ("无双重剑-测试(19876)", "战士", "pack://application:,,,/Assets/Images/Profession_LYJS.png"),
            ("奥术回响-测试(20111)", "法师", "pack://application:,,,/Assets/Images/Profession_BMDS.png"),
            ("圣光之约-测试(18770)", "牧师", "pack://application:,,,/Assets/Images/Profession_LHYS.png"),
            ("影袭-测试(20990)", "刺客", "pack://application:,,,/Assets/Images/Profession_QLQS.png"),
            ("圣盾壁垒-测试(17654)", "骑士", "pack://application:,,,/Assets/Images/Profession_SDQS.png")
        };

        _slots.Clear();
        for (var i = 0; i < players.Length; i++)
        {
            var (nick, prof, iconPath) = players[i];

            // 初始化一点基础值，避免全部为0
            _totals[i] = _rd.Next(2_000, 8_000);

            var slotData = new PlayerSlotViewModel
            {
                Name = $"{i + 1:00}.", // 01. 02. ...
                Nickname = nick,
                Profession = prof,
                Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute)),
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
        for (var i = 0; i < _totals.Length; i++)
            _totals[i] += _rd.Next(10, 20);

        var max = Math.Max(1, _totals.Max()); // 防止除0

        // 计算“每秒值”举例：取最近随机的一点点变化，示意 dps
        // 这里简化：用一个近似（0.4~0.8）* (当下相对值*1w)，只做展示
        for (var i = 0; i < _slots.Count; i++)
        {
            var bar = _slots[i];
            var total = _totals[i];
            var ratio = (double)total / max; // 0~1
            bar.ProgressBarValue = ratio; // 控制条的长度

            // 右侧文本：总伤(每秒)
            var approxPerSec = ratio * 10000 * (0.4 + 0.4 * _rd.NextDouble());
            var valueText = $"{total.ToChineseUnitString()}({approxPerSec:0.0})";

            // 更新 Data 里的文本（绑定会刷新）
            if (bar.Data is PlayerSlotViewModel p)
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
        for (var rank = 0; rank < ordered.Count; rank++)
        {
            if (ordered[rank].Data is PlayerSlotViewModel p)
                p.Name = $"{rank + 1:00}.";
        }

        // 把排序后的列表重新赋值（若控件 Data 是 IEnumerable，并允许替换）
        // 如果你的控件支持就地更新而不需要替换，也可以直接 ProgressBarList.Data = _slots;
        ProgressBarList.Data = ordered;
    }

    public class SkillItem
    {
        public string SkillName { get; set; } = string.Empty;
        public string TotalDamage { get; set; } = string.Empty;
        public int HitCount { get; set; }
        public int CritCount { get; set; }
        public int AvgDamage { get; set; }
    }
}