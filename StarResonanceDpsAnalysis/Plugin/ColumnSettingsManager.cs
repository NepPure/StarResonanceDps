using AntdUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Plugin
{
    public class ColumnSetting
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public bool IsVisible { get; set; } = false; // 默认全部隐藏
        public Func<Column> Builder { get; set; }
    }

    public static class ColumnSettingsManager
    {
        public static Action? RefreshTableAction { get; set; }

        // 这里的 Key 必须与 DpsTable 公有属性名完全一致（大小写也要一致）
        public static List<ColumnSetting> AllSettings = new List<ColumnSetting>
        {
            new ColumnSetting {
                Key = "CombatPower", Title = "战力", IsVisible = true,
                Builder = () => new Column("CombatPower", "战力", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "TotalDamage", Title = "总伤害", IsVisible = true,
                Builder = () => new Column("TotalDamage", "总伤害", ColumnAlign.Center){ SortOrder = true }
            },
            new ColumnSetting {
                Key = "DamageTaken", Title = "承伤", IsVisible = true,
                Builder = () => new Column("DamageTaken", "承伤", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "CritRate", Title = "暴击率", IsVisible = true,
                Builder = () => new Column("CritRate", "暴击率") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "LuckyRate", Title = "幸运率", IsVisible = true,
                Builder = () => new Column("LuckyRate", "幸运率") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "CriticalDamage", Title = "纯暴击", IsVisible = true,
                Builder = () => new Column("CriticalDamage", "纯暴击") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "LuckyDamage", Title = "纯幸运", IsVisible = true,
                Builder = () => new Column("LuckyDamage", "纯幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "CritLuckyDamage", Title = "暴击幸运", IsVisible = true,
                Builder = () => new Column("CritLuckyDamage", "暴击幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "InstantDps", Title = "瞬时Dps", IsVisible = true,
                Builder = () => new Column("InstantDps", "瞬时Dps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "MaxInstantDps", Title = "最大瞬时Dps", IsVisible = true,
                Builder = () => new Column("MaxInstantDps", "最大瞬时Dps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "TotalDps", Title = "DPS/秒", IsVisible = true,
                Builder = () => new Column("TotalDps", "DPS/秒", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "TotalHealingDone", Title = "总治疗", IsVisible = true,
                Builder = () => new Column("TotalHealingDone", "总治疗", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "CriticalHealingDone", Title = "治疗暴击", IsVisible = true,
                Builder = () => new Column("CriticalHealingDone", "治疗暴击") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "LuckyHealingDone", Title = "治疗幸运", IsVisible = true,
                Builder = () => new Column("LuckyHealingDone", "治疗幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "CritLuckyHealingDone", Title = "治疗暴击幸运", IsVisible = true,
                Builder = () => new Column("CritLuckyHealingDone", "治疗暴击幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "InstantHps", Title = "瞬时Hps", IsVisible = true,
                Builder = () => new Column("InstantHps", "瞬时Hps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "MaxInstantHps", Title = "最大瞬时Hps", IsVisible = true,
                Builder = () => new Column("MaxInstantHps", "最大瞬时Hps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "TotalHps", Title = "HPS/秒", IsVisible = true,
                Builder = () => new Column("TotalHps", "HPS/秒", ColumnAlign.Center) { SortOrder = true }
            },
        };

        public static StackedHeaderRow[] BuildStackedHeader()
        {
            var list = new List<StackedColumn[]>();

            // 与上方 Key 一致的分组
            string[] group1 = { "TotalDamage", "CriticalDamage", "LuckyDamage", "CritLuckyDamage" };
            string[] group2 = { "InstantDps", "MaxInstantDps", "TotalDps" };
            string[] group3 = { "TotalHealingDone", "CriticalHealingDone", "LuckyHealingDone", "CritLuckyHealingDone" };
            string[] group4 = { "InstantHps", "MaxInstantHps", "TotalHps" };

            list.Add(BuildGroup(group1, "总伤害"));
            list.Add(BuildGroup(group2, "DPS"));
            list.Add(BuildGroup(group3, "总治疗"));
            list.Add(BuildGroup(group4, "HPS"));

            return new[] { new StackedHeaderRow(list.SelectMany(x => x).ToArray()) };
        }

        private static StackedColumn[] BuildGroup(string[] keys, string title)
        {
            var visible = keys
                .Where(k => AllSettings.FirstOrDefault(x => x.Key == k)?.IsVisible == true)
                .ToList();

            return visible.Count > 1
                ? new[] { new StackedColumn(string.Join(",", visible), title) }
                : Array.Empty<StackedColumn>();
        }

        public static ColumnCollection BuildColumns(bool includeExtraColumns)
        {
            var columns = new List<Column>
            {
                new Column("", "序号")
                {
                    Width = "50",
                    Render = (value, record, rowIndex) => rowIndex + 1,
                    Fixed = true
                },
                // 这些字段名需要与 DpsTable 公有属性名一致
                new Column("Uid", "角色ID", ColumnAlign.Center){ SortOrder = true },
                new Column("NickName", "角色昵称", ColumnAlign.Center){ SortOrder = true },
                new Column("Profession", "职业", ColumnAlign.Center),
            };

            if (!includeExtraColumns)
            {
                columns.AddRange(AllSettings.Where(s => s.IsVisible).Select(s => s.Builder()));
            }
            else
            {
                columns.Add(new Column("CellProgress", "团队总伤害占比", ColumnAlign.Center));
                columns.Add(new Column("TotalDps", "Dps/秒", ColumnAlign.Center) { SortOrder = true });
                columns.Add(new Column("TotalHps", "Hps/秒", ColumnAlign.Center) { SortOrder = true });
            }

            return new ColumnCollection(columns);
        }
    }
}
