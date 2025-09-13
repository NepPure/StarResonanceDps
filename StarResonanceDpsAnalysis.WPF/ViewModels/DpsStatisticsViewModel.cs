using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using StarResonanceDpsAnalysis.Core.Analyze.Exceptions;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.Controls.Models;
using StarResonanceDpsAnalysis.WPF.Converters;
using StarResonanceDpsAnalysis.WPF.Data;
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
    private readonly Stopwatch _battleTimer = new();
    private readonly IDataSource _dataSource;

    private readonly Stopwatch _fullBattleTimer = new();
    private readonly Random _rd = new();
    private readonly IDataStorage _storage;
    private readonly ILogger<DpsStatisticsViewModel> _logger;
    private readonly long[] _totals = new long[6]; // 6位玩家示例

    [ObservableProperty] private DateTime _battleDuration;
    [ObservableProperty] private NumberDisplayMode _numberDisplayMode = NumberDisplayMode.Wan;
    [ObservableProperty] private ScopeTime _scopeTime = ScopeTime.Current;
    [ObservableProperty] private bool _showContextMenu;
    [ObservableProperty] private bool _showSkillListPopup;
    [ObservableProperty] private List<SkillItem>? _skillList;
    [ObservableProperty] private BulkObservableCollection<StatisticDataViewModel> _slots = new();
    [ObservableProperty] private SortDirectionEnum _sortDirection = SortDirectionEnum.Descending;
    [ObservableProperty] private string _sortMemberPath = "Value";
    [ObservableProperty] private StatisticType _statisticIndex;

    private DispatcherTimer _timer = null!;

    public DpsStatisticsViewModel(IApplicationController appController, IDataSource dataSource, IDataStorage storage, ILogger<DpsStatisticsViewModel> logger)
    {
        _appController = appController;
        _dataSource = dataSource;
        _storage = storage;
        _logger = logger;
        _logger.LogDebug("VM Loaded");

        InitDemoProgressBars();
    }

    public DpsStatisticsOptions Options { get; } = new();
    private Stopwatch InUsingTimer => ScopeTime == ScopeTime.Total ? _fullBattleTimer : _battleTimer;

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
            ("影袭-测试(20990)", Classes.Stormblade),
            ("Jojo-未知(20990)", Classes.Unknown)
        };

        Slots.BeginUpdate();
        for (var i = 0; i < players.Length; i++)
        {
            var (nick, @class) = players[i];
            var barData = new StatisticDataViewModel
            {
                Id = i + 1, // 1-based index
                Name = nick,
                Classes = @class
            };
            Slots.Add(barData);
        }

        UpdateData();
        Slots.EndUpdate();
    }


    /// <summary>
    /// 读取用户缓存
    /// </summary>
    private void LoadPlayerCache()
    {
        try
        {
            _storage.LoadPlayerInfoFromFile();
        }
        catch (FileNotFoundException)
        {
            // 没有缓存
        }
        catch (DataTamperedException)
        {
            _storage.ClearAllPlayerInfos();
            _storage.SavePlayerInfoToFile();
        }
    }

    [RelayCommand]
    private void OnLoaded()
    {
        // 开始监听DPS更新事件
        _storage.DpsDataUpdated += DataStorage_DpsDataUpdated;

        StartRefreshTimer();
    }


    private void DataStorage_DpsDataUpdated()
    {
        if (!_fullBattleTimer.IsRunning)
        {
            _fullBattleTimer.Restart();
        }

        if (!_battleTimer.IsRunning)
        {
            _battleTimer.Restart();
        }

        UpdateSortProgressBarListData();
    }


    // 核心：根据最新 dps 数据，填充 / 更新 Slots（供 XAML 进度条显示）
    private void UpdateSortProgressBarListData()
    {

        // 1) 选择全程 or 分段
        var dpsList = ScopeTime == ScopeTime.Total
            ? DataStorage.ReadOnlyFullDpsDataList
            : DataStorage.ReadOnlySectionedDpsDataList;

        // 无数据 → 清空
        if (dpsList.Count == 0)
        {
            Slots.Clear();
            return;
        }

        // 2) 过滤（按类型去掉 0 值/无效项）
        var filtered = GetDefaultFilter(dpsList, StatisticIndex, ScopeTime).ToList();
        if (filtered.Count == 0)
        {
            Slots.Clear();
            return;
        }

        // 3) 取最大值、总和用于比例/占比
        var (maxValue, sumValue) = GetMaxSumValueByType(filtered, StatisticIndex, ScopeTime);
        if (maxValue <= 0) maxValue = 1; // 防止除 0

        // 4) 先按当前数值降序排，后面要写名次
        var ordered = filtered
            .Select(e => new
            {
                Data = e,
                Value = GetValueByType(e, StatisticIndex)
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        // 把现有 Slots 建索引，便于“就地更新”
        var slotIndex = Slots.ToDictionary(s => s.Id, s => s);

        // // 5) 逐个构建/更新进度条项
        // var newList = new List<ProgressBarData>(ordered.Count);
        // for (int i = 0; i < ordered.Count; i++)
        // {
        //     var x = ordered[i];
        //     var e = x.Data;
        //     var value = x.Value;
        //     var ratio = (double)value / maxValue;
        //
        //     // 额外信息（职业、昵称、战力）
        //     DataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(e.UID, out var playerInfo);
        //     var professionName = playerInfo?.SubProfessionName
        //                          ?? playerInfo?.ProfessionID?.GetProfessionNameById()
        //                          ?? string.Empty;
        //
        //     // 每秒（DPS/HPS）= 总值 / 持续秒数
        //     // var seconds = Math.Max(1,
        //     //     TimeSpan.FromTicks(e.LastLoggedTick - (e.StartLoggedTick ?? 0)).TotalSeconds);
        //     // var perSec = value / seconds;
        //
        //     // 右侧显示文本：总值(每秒)
        //     // var valueText = $"{value.ToCompactString()} ({perSec.ToCompactString()})";
        //
        //     // 这四个字段要和 XAML 里的绑定名字一致：
        //     // OrderText、Classes、Nickname、ValueText
        //     // 注意：XAML 里用的是 “Classes”（复数），和你的 Demo 里的“Class”不一样
        //     PlayerSlotViewModel slotVm;
        //
        //     if (slotIndex.TryGetValue(e.UID, out var existed) && existed.Data is PlayerSlotViewModel existedVm)
        //     {
        //         // 就地更新，避免闪烁
        //         slotVm = existedVm;
        //         slotVm.Nickname = (playerInfo?.Name == null ? string.Empty : $"{playerInfo.Name}-")
        //                            + (playerInfo?.SubProfessionName ?? professionName)
        //                            + $"({playerInfo?.CombatPower?.ToString() ?? ($"UID: {e.UID}")})";
        //         //slotVm.Class = professionName.GetProfessionEnumForWpf(); // 见下面备注
        //         // slotVm.Value = valueText;
        //
        //         // existed.ProgressBarValue = ratio; // 0~1
        //         newList.Add(existed);
        //     }
        //     else
        //     {
        //         // 新建
        //         slotVm = new PlayerSlotViewModel
        //         {
        //             Nickname = (playerInfo?.Name == null ? string.Empty : $"{playerInfo.Name}-")
        //                         + (playerInfo?.SubProfessionName ?? professionName)
        //                         + $"({playerInfo?.CombatPower?.ToString() ?? ($"UID: {e.UID}")})",
        //             //Class = professionName.GetProfessionEnumForWpf(), // 见下面备注
        //             // ValueText = valueText
        //         };
        //
        //         var bar = new ProgressBarData
        //         {
        //             ID = e.UID,
        //             ProgressBarValue = ratio,
        //             Data = slotVm
        //             // 如你的控件还支持颜色、圆角等，可一并赋值：
        //             // ProgressBarColor = professionName.GetProfessionThemeColor(Config.IsLight),
        //             // ProgressBarCornerRadius = 3,
        //         };
        //
        //         newList.Add(bar);
        //     }
        // }

        // 6) 用排序好的新列表替换 Slots（需要重排时用这种）
        //    如果你的控件支持就地修改顺序，也可逐项搬移；最简单是整体替换。
        // Slots = new ObservableCollection<ProgressBarData>(newList);
    }

    /// <summary>
    /// 获取每个统计类别的默认筛选器
    /// </summary>
    /// <param name="list"></param>
    /// <param name="type"></param>
    /// <param name="scopeTime"></param>
    /// <returns></returns>
    private static IEnumerable<DpsData> GetDefaultFilter(IEnumerable<DpsData> list, StatisticType type, ScopeTime scopeTime)
    {
        return (scopeTime, type) switch
        {
            (ScopeTime.Total, StatisticType.Damage) => list.Where(e => !e.IsNpcData && e.TotalAttackDamage != 0),
            (ScopeTime.Total, StatisticType.Healing) => list.Where(e => !e.IsNpcData && e.TotalHeal != 0),
            (ScopeTime.Total, StatisticType.TakenDamage) => list.Where(e => !e.IsNpcData && e.TotalTakenDamage != 0),
            (ScopeTime.Total, StatisticType.NpcTakenDamage) => list.Where(e => e.IsNpcData && e.TotalTakenDamage != 0),
            _ => list
        };
    }

    private static (long max, long sum) GetMaxSumValueByType(IEnumerable<DpsData> list, StatisticType type, ScopeTime scopeTime)
    {
        return type switch
        {
            StatisticType.Damage => (list.Max(e => e.TotalAttackDamage), list.Sum(e => e.TotalAttackDamage)),
            StatisticType.Healing => (list.Max(e => e.TotalHeal), list.Sum(e => e.TotalHeal)),
            StatisticType.TakenDamage or StatisticType.NpcTakenDamage => (list.Max(e => e.TotalTakenDamage), list.Sum(e => e.TotalTakenDamage)),

            _ => (long.MaxValue, long.MaxValue)
        };
    }

    private static long GetValueByType(DpsData data, StatisticType type)
    {
        return type switch
        {
            StatisticType.Damage => data.TotalAttackDamage,
            StatisticType.Healing => data.TotalHeal,
            StatisticType.TakenDamage or StatisticType.NpcTakenDamage => data.TotalTakenDamage,

            _ => long.MaxValue
        };
    }


    [RelayCommand]
    private void AddRandomData()
    {
        UpdateData();
    }

    private void StartRefreshTimer()
    {
        // 3) 定时器：实时更新
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _timer.Tick += (_, __) => UpdateData();
        // _timer.Start();
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
        _logger.LogDebug("Enter updatedata");

        // Update values for each slot
        foreach (var data in Slots)
        {
            data.Value += (ulong)_rd.Next(1000, 80000);
            _logger.LogDebug($"Updated {data.Name}'s value to {data.Value}");
        }

        // Calculate percentage of max
        if (Slots.Count > 0)
        {
            var maxValue = Slots.Max(d => d.Value);
            foreach (var data in Slots)
            {
                data.PercentOfMax = maxValue > 0 ? data.Value / (double)maxValue * 100 : 0;
            }

            // Calculate percentage of total
            var totalValue = Slots.Sum(d => Convert.ToDouble(d.Value));
            foreach (var data in Slots)
            {
                data.Percent = totalValue > 0 ? data.Value / totalValue : 0;
            }
        }

        // Sort data in place 
        SortSlotsInPlace();

        _logger.LogDebug("Exit updatedata");
    }

    /// <summary>
    /// Updates the Id property of items to reflect their current position in the collection
    /// </summary>
    private void UpdateItemIndices()
    {
        for (var i = 0; i < Slots.Count; i++)
        {
            Slots[i].Id = i + 1; // 1-based index
        }
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

    public class SkillItem
    {
        public string SkillName { get; set; } = string.Empty;
        public string TotalDamage { get; set; } = string.Empty;
        public int HitCount { get; set; }
        public int CritCount { get; set; }
        public int AvgDamage { get; set; }
    }

    #region  Sort

    /// <summary>
    /// Changes the sort member path and re-sorts the data
    /// </summary>
    [RelayCommand]
    private void SetSortMemberPath(string memberPath)
    {
        if (SortMemberPath == memberPath)
        {
            // Toggle sort direction if the same property is clicked
            SortDirection = SortDirection == SortDirectionEnum.Ascending
                ? SortDirectionEnum.Descending
                : SortDirectionEnum.Ascending;
        }
        else
        {
            SortMemberPath = memberPath;
            SortDirection = SortDirectionEnum.Descending; // Default to descending for new properties
        }

        // Trigger immediate re-sort
        SortSlotsInPlace();
    }

    /// <summary>
    /// Manually triggers a sort operation
    /// </summary>
    [RelayCommand]
    private void ManualSort()
    {
        SortSlotsInPlace();
    }

    /// <summary>
    /// Sorts by Value in descending order (highest DPS first)
    /// </summary>
    [RelayCommand]
    private void SortByValue()
    {
        SetSortMemberPath("Value");
    }

    /// <summary>
    /// Sorts by Name in ascending order
    /// </summary>
    [RelayCommand]
    private void SortByName()
    {
        SortMemberPath = "Name";
        SortDirection = SortDirectionEnum.Ascending;
        SortSlotsInPlace();
    }

    /// <summary>
    /// Sorts by Classes
    /// </summary>
    [RelayCommand]
    private void SortByClass()
    {
        SetSortMemberPath("Classes");
    }

    /// <summary>
    /// Sorts the slots collection in-place based on the current sort criteria
    /// </summary>
    private void SortSlotsInPlace()
    {
        if (Slots.Count == 0 || string.IsNullOrWhiteSpace(SortMemberPath))
            return;

        try
        {
            // Sort the collection based on the current criteria
            switch (SortMemberPath)
            {
                case "Value":
                    Slots.SortBy(x => x.Value, SortDirection == SortDirectionEnum.Descending);
                    break;
                case "Name":
                    Slots.SortBy(x => x.Name, SortDirection == SortDirectionEnum.Descending);
                    break;
                case "Classes":
                    Slots.SortBy(x => (int)x.Classes, SortDirection == SortDirectionEnum.Descending);
                    break;
                case "PercentOfMax":
                    Slots.SortBy(x => x.PercentOfMax, SortDirection == SortDirectionEnum.Descending);
                    break;
                case "Percent":
                    Slots.SortBy(x => x.Percent, SortDirection == SortDirectionEnum.Descending);
                    break;
            }

            // Update the Id property to reflect the new order (1-based index)
            UpdateItemIndices();
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error during sorting: {ex.Message}");
        }
    }

    #endregion
}

public sealed class DpsStatisticsDesignTimeViewModel() : DpsStatisticsViewModel(null!, null!, new InstantizedDataStorage(), null!);