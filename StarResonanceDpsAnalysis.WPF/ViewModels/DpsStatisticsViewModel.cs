using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.WPF.Controls;
using StarResonanceDpsAnalysis.WPF.Data;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.Controls.Models;
using StarResonanceDpsAnalysis.WPF.Extensions;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class DpsStatisticsOptions : BaseViewModel
{
    [ObservableProperty] private int _minimalDurationInSeconds;

    [RelayCommand]
    private void SetMinimalDuration(int duration)
    {
        MinimalDurationInSeconds = duration;
    }
}

public partial class DpsStatisticsViewModel : BaseViewModel
{
    private readonly IApplicationController _appController;
    private readonly IDataSource _dataSource;
    private readonly Random _rd = new();
    private readonly long[] _totals = new long[6]; // 6位玩家示例

    [ObservableProperty] private DateTime _battleDuration;
    [ObservableProperty] private ScopeTime _scopeTime = ScopeTime.Current;
    [ObservableProperty] private bool _showContextMenu;
    [ObservableProperty] private bool _showSkillListPopup;

    [ObservableProperty] private List<SkillItem>? _skillList;
    [ObservableProperty] private ObservableCollection<ProgressBarData> _slots = [];
    [ObservableProperty] private StatisticType _statisticIndex;

    private DispatcherTimer _timer = null!;

    public DpsStatisticsViewModel(IApplicationController appController, IDataSource dataSource)
    {
        _appController = appController;
        _dataSource = dataSource;
        Debug.WriteLine("VM Loaded");

        InitDemoProgressBars();
    }

    public DpsStatisticsOptions Options { get; } = new();

    [Conditional("DEBUG")]
    private void InitDemoProgressBars()
    {
        // 2) 造几位玩家（随便举例，图标请换成你项目里存在的）
        var players = new[]
        {
            ("惊奇猫猫盒-狼弓(23207)", Classes.Marksman),
            ("无双重剑-测试(19876)", Classes.ShieldKnight),
            ("奥术回响-测试(20111)", Classes.FrostMage),
            ("圣光之约-测试(18770)", Classes.VerdantOracle),
            ("影袭-测试(20990)", Classes.Stormblade)
        };

        var list = new List<ProgressBarData>();
        for (var i = 0; i < players.Length; i++)
        {
            var (nick, @class) = players[i];

            // 初始化一点基础值，避免全部为0
            _totals[i] = _rd.Next(2_000, 8_000);

            var slotData = new PlayerSlotViewModel
            {
                Name = $"{i + 1:00}.", // 01. 02. ...
                Nickname = nick,
                Class = @class,
                ValueText = "24.81万(2456.0)"
            };
            list.Add(new ProgressBarData
            {
                ID = i,
                ProgressBarValue = 0, // 先给0，等会定时器里刷新
                Data = slotData
            });
        }

        Slots = [.. list];
    }

    [RelayCommand]
    private void OnLoaded()
    {
        StartRefreshTimer();
    }

    private void StartRefreshTimer()
    {
        // 3) 定时器：实时更新
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(2000)
        };
        _timer.Tick += (_, __) => UpdateData();
        _timer.Start();
    }


    [RelayCommand]
    private void NextMetricType()
    {
        StatisticIndex = StatisticIndex.Next();
    }

    [RelayCommand]
    private void PreviousMetricType()
    {
        StatisticIndex = StatisticIndex.Previous();
    }

    [RelayCommand]
    private void ToggleScopeTime()
    {
        ScopeTime = ScopeTime.Next();
    }

    private void UpdateData()
    {
        Debug.WriteLine("Enter updatedata");

        // 随机增长各自总伤
        for (var i = 0; i < _totals.Length; i++)
            _totals[i] += _rd.Next(10, 20);

        var max = Math.Max(1, _totals.Max()); // 防止除0

        // 计算“每秒值”举例：取最近随机的一点点变化，示意 dps
        // 这里简化：用一个近似（0.4~0.8）* (当下相对值*1w)，只做展示
        for (var i = 0; i < Slots.Count; i++)
        {
            var bar = Slots[i];
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
        var ordered = Slots.Zip(_totals, (bar, total) => (bar, total))
            .OrderByDescending(x => x.total)
            .Select(x => x.bar)
            .ToList();

        // 更新名次显示（01. 02. ...）
        for (var rank = 0; rank < ordered.Count; rank++)
        {
            if (ordered[rank].Data is PlayerSlotViewModel p)
                p.Name = $"{rank + 1:00}.";
        }

        // Reorder the observable collection to match the desired order with minimal notifications.
        SortData(ordered);
        Debug.WriteLine("Exit updatedata");
    }

    [RelayCommand]
    private void Refresh()
    {
        // 手动触发一次更新（如果需要）
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void RefreshButtonMouseEntered()
    {
        var skills = new List<SkillItem>
        {
            new() { SkillName = "技能A", TotalDamage = "939.1万", HitCount = 4, CritCount = 121, AvgDamage = 121 },
            new() { SkillName = "技能B", TotalDamage = "88.6万", HitCount = 8, CritCount = 23, AvgDamage = 11 },
            new() { SkillName = "技能C", TotalDamage = "123.4万", HitCount = 3, CritCount = 45, AvgDamage = 233 }
        };

        SkillList = skills;
        ShowSkillListPopup = true;
    }

    [RelayCommand]
    private void RefreshButtonMouseLeaved()
    {
        SkillList = null;
        ShowSkillListPopup = false;
    }

    [RelayCommand]
    private void OpenContextMenu()
    {
        ShowContextMenu = true;
    }

    [RelayCommand]
    private void Shutdown()
    {
        _appController.Shutdown();
    }

    private void SortData(IEnumerable<ProgressBarData> orderedEnumerable)
    {
        if (orderedEnumerable is null)
            return;

        var desired = orderedEnumerable as IList<ProgressBarData> ?? orderedEnumerable.ToList();

        // Defensive: if collection sizes differ or some items are missing, replace the entire collection.
        if (Slots.Count != desired.Count || desired.Any(d => !Slots.Contains(d)))
        {
            // Replace the whole collection (property setter will raise change notification).
            Slots = new System.Collections.ObjectModel.ObservableCollection<ProgressBarData>(desired);
            return;
        }

        // In-place reordering using ObservableCollection.Move to minimize change notifications.
        // For each position i, find the desired item and move it into position i if necessary.
        for (var i = 0; i < desired.Count; i++)
        {
            var target = desired[i];
            var currentIndex = Slots.IndexOf(target);
            if (currentIndex < 0)
                continue; // shouldn't happen due to previous check

            if (currentIndex == i)
                continue;

            // Move element from currentIndex to i.
            Slots.Move(currentIndex, i);
        }
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

public sealed class DpsStatisticsDesignTimeViewModel() : DpsStatisticsViewModel(null!, null!);