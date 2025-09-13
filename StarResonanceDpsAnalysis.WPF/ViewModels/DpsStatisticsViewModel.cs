using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.WPF.Controls;
using StarResonanceDpsAnalysis.WPF.Data;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.Controls.Models;
using StarResonanceDpsAnalysis.WPF.Converters;
using StarResonanceDpsAnalysis.WPF.Extensions;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public enum SortDirectionEnum
{
    Ascending,
    Descending
}

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
    [ObservableProperty] private StatisticType _statisticIndex;
    [ObservableProperty] private NumberDisplayMode _numberDisplayMode = NumberDisplayMode.Wan;

    [ObservableProperty] private BulkObservableCollection<StatisticDataViewModel> _slots = new();
    [ObservableProperty] private string _sortMemberPath = "Value";
    [ObservableProperty] private SortDirectionEnum _sortDirection = SortDirectionEnum.Descending;

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
            ("影袭-测试(20990)", Classes.Stormblade),
            ("Jojo-未知(20990)", Classes.Unknown)
        };

        Slots.BeginUpdate();
        for (int i = 0; i < players.Length; i++)
        {
            var (nick, @class) = players[i];
            var barData = new StatisticDataViewModel()
            {
                Id = i + 1, // 1-based index
                Name = nick,
                Classes = @class,
            };
            Slots.Add(barData);
        }
        UpdateData();
        Slots.EndUpdate();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        StartRefreshTimer();
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
            Interval = TimeSpan.FromMilliseconds(100)
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
        
        // Update values for each slot
        foreach (var data in Slots)
        {
            data.Value += (ulong)_rd.Next(1000, 80000);
            Debug.WriteLine($"Updated {data.Name}'s value to {data.Value}");
        }
        
        // Calculate percentage of max
        if (Slots.Count > 0)
        {
            var maxValue = Slots.Max(d => d.Value);
            foreach (var data in Slots)
            {
                data.PercentOfMax = maxValue > 0 ? (data.Value / (double)maxValue * 100) : 0;
            }

            // Calculate percentage of total
            var totalValue = Slots.Sum(d => Convert.ToDouble(d.Value));
            foreach (var data in Slots)
            {
                data.Percent = totalValue > 0 ? (data.Value / totalValue) : 0;
            }
        }
        
        // Sort data in place 
        SortSlotsInPlace();

        Debug.WriteLine("Exit updatedata");
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
            Debug.WriteLine($"Error during sorting: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the Id property of items to reflect their current position in the collection
    /// </summary>
    private void UpdateItemIndices()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].Id = i + 1; // 1-based index
        }
    }

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
}

public sealed class DpsStatisticsDesignTimeViewModel() : DpsStatisticsViewModel(null!, null!);