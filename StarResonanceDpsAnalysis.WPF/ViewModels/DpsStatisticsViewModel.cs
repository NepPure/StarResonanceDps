using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.Controls.Models;
using StarResonanceDpsAnalysis.WPF.Extensions;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.Core.Extends.Data;
using SharpPcap;
using StarResonanceDpsAnalysis.Core.Analyze.Exceptions;
using System.IO;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class DpsStatisticsViewModel : BaseViewModel
{

  

    private readonly Random _rd = new();
    private readonly long[] _totals = new long[6]; // 6位玩家示例

    [ObservableProperty] private DateTime _battleDuration;
    [ObservableProperty] private ScopeTime _scopeTime = ScopeTime.Current;
    [ObservableProperty] private bool _showSkillListPopup;
    [ObservableProperty] private ObservableCollection<ProgressBarData> _slots = [];
    [ObservableProperty] private StatisticType _statisticIndex;
    [ObservableProperty] private bool _showContextMenu;

    private DispatcherTimer _timer = null!;

    public DpsStatisticsViewModel()
    {
        Debug.WriteLine("VM Loaded");

        InitDemoProgressBars();
    }

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




    /// <summary>
    /// 读取用户缓存
    /// </summary>
    private void LoadPlayerCache()
    {
        try
        {
            DataStorage.LoadPlayerInfoFromFile();
        }
        catch (FileNotFoundException)
        {
            // 没有缓存
        }
        catch (DataTamperedException)
        {

            DataStorage.ClearAllPlayerInfos();
            DataStorage.SavePlayerInfoToFile();
        }
    }


    private readonly Stopwatch _fullBattleTimer = new();
    private bool _isShowFullData = false;
    private readonly Stopwatch _battleTimer = new();
    private int _stasticsType = 0;
    private Stopwatch InUsingTimer => _isShowFullData ? _fullBattleTimer : _battleTimer;

    [RelayCommand]
    private void OnLoaded()
    {

    

        // 开始监听DPS更新事件
        DataStorage.DpsDataUpdated += DataStorage_DpsDataUpdated;

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
        var dpsList = _isShowFullData
            ? DataStorage.ReadOnlyFullDpsDataList
            : DataStorage.ReadOnlySectionedDpsDataList;

        // 无数据 → 清空
        if (dpsList.Count == 0)
        {
            Slots.Clear();
            return;
        }

        // 2) 过滤（按类型去掉 0 值/无效项）
        var filtered = GetDefaultFilter(dpsList, _stasticsType).ToList();
        if (filtered.Count == 0)
        {
            Slots.Clear();
            return;
        }

        // 3) 取最大值、总和用于比例/占比
        var (maxValue, sumValue) = GetMaxSumValueByType(filtered, _stasticsType);
        if (maxValue <= 0) maxValue = 1; // 防止除 0

        // 4) 先按当前数值降序排，后面要写名次
        var ordered = filtered
            .Select(e => new
            {
                Data = e,
                Value = GetValueByType(e, _stasticsType)
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        // 把现有 Slots 建索引，便于“就地更新”
        var slotIndex = Slots.ToDictionary(s => s.ID, s => s);

        // 5) 逐个构建/更新进度条项
        var newList = new List<ProgressBarData>(ordered.Count);
        for (int i = 0; i < ordered.Count; i++)
        {
            var x = ordered[i];
            var e = x.Data;
            var value = x.Value;
            var ratio = (double)value / maxValue;

            // 额外信息（职业、昵称、战力）
            DataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(e.UID, out var playerInfo);
            var professionName = playerInfo?.SubProfessionName
                                 ?? playerInfo?.ProfessionID?.GetProfessionNameById()
                                 ?? string.Empty;

            // 每秒（DPS/HPS）= 总值 / 持续秒数
            var seconds = Math.Max(1,
                TimeSpan.FromTicks(e.LastLoggedTick - (e.StartLoggedTick ?? 0)).TotalSeconds);
            var perSec = value / seconds;

            // 右侧显示文本：总值(每秒)
            var valueText = $"{value.ToCompactString()} ({perSec.ToCompactString()})";

            // 这四个字段要和 XAML 里的绑定名字一致：
            // OrderText、Classes、Nickname、ValueText
            // 注意：XAML 里用的是 “Classes”（复数），和你的 Demo 里的“Class”不一样
            PlayerSlotViewModel slotVm;

            if (slotIndex.TryGetValue(e.UID, out var existed) && existed.Data is PlayerSlotViewModel existedVm)
            {
                // 就地更新，避免闪烁
                slotVm = existedVm;
                slotVm.Nickname = (playerInfo?.Name == null ? string.Empty : $"{playerInfo.Name}-")
                                   + (playerInfo?.SubProfessionName ?? professionName)
                                   + $"({playerInfo?.CombatPower?.ToString() ?? ($"UID: {e.UID}")})";
                //slotVm.Class = professionName.GetProfessionEnumForWpf(); // 见下面备注
                slotVm.ValueText = valueText;

                existed.ProgressBarValue = ratio; // 0~1
                newList.Add(existed);
            }
            else
            {
                // 新建
                slotVm = new PlayerSlotViewModel
                {
                    Nickname = (playerInfo?.Name == null ? string.Empty : $"{playerInfo.Name}-")
                                + (playerInfo?.SubProfessionName ?? professionName)
                                + $"({playerInfo?.CombatPower?.ToString() ?? ($"UID: {e.UID}")})",
                    //Class = professionName.GetProfessionEnumForWpf(), // 见下面备注
                    ValueText = valueText
                };

                var bar = new ProgressBarData
                {
                    ID = e.UID,
                    ProgressBarValue = ratio,
                    Data = slotVm
                    // 如你的控件还支持颜色、圆角等，可一并赋值：
                    // ProgressBarColor = professionName.GetProfessionThemeColor(Config.IsLight),
                    // ProgressBarCornerRadius = 3,
                };

                newList.Add(bar);
            }
        }

        // 6) 用排序好的新列表替换 Slots（需要重排时用这种）
        //    如果你的控件支持就地修改顺序，也可逐项搬移；最简单是整体替换。
        Slots = new ObservableCollection<ProgressBarData>(newList);
    }

    /// <summary>
    /// 获取每个统计类别的默认筛选器
    /// </summary>
    /// <param name="list"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private IEnumerable<DpsData> GetDefaultFilter(IEnumerable<DpsData> list, int type)
    {
        return type switch
        {
            0 => list.Where(e => !e.IsNpcData && e.TotalAttackDamage != 0),
            1 => list.Where(e => !e.IsNpcData && e.TotalHeal != 0),
            2 => list.Where(e => !e.IsNpcData && e.TotalTakenDamage != 0),
            3 => list.Where(e => e.IsNpcData && e.TotalTakenDamage != 0),

            _ => list
        };
    }

    private (long max, long sum) GetMaxSumValueByType(IEnumerable<DpsData> list, int type)
    {
        return type switch
        {
            0 => (list.Max(e => e.TotalAttackDamage), list.Sum(e => e.TotalAttackDamage)),
            1 => (list.Max(e => e.TotalHeal), list.Sum(e => e.TotalHeal)),
            2 or 3 => (list.Max(e => e.TotalTakenDamage), list.Sum(e => e.TotalTakenDamage)),

            _ => (long.MaxValue, long.MaxValue)
        };
    }

    private long GetValueByType(DpsData data, int type)
    {
        return type switch
        {
            0 => data.TotalAttackDamage,
            1 => data.TotalHeal,
            2 or 3 => data.TotalTakenDamage,

            _ => long.MaxValue
        };
    }


    private void StartRefreshTimer()
    {
        // 3) 定时器：实时更新
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += (_, _) => UpdateBars();
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

    public List<ProgressBarData> UpdateBars()
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
        return ordered;
    }

    [RelayCommand]
    private void Refresh()
    {
        // 手动触发一次更新（如果需要）
        throw new NotImplementedException();
    }

    [ObservableProperty] private List<SkillItem>? _skillList;

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

    public class SkillItem
    {
        public string SkillName { get; set; } = string.Empty;
        public string TotalDamage { get; set; } = string.Empty;
        public int HitCount { get; set; }
        public int CritCount { get; set; }
        public int AvgDamage { get; set; }
    }
}