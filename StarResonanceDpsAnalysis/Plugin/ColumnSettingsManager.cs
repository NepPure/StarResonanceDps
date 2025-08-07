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
        public static List<ColumnSetting> AllSettings = new List<ColumnSetting>
        {
          
            new ColumnSetting {
                Key = "damageTaken", Title = "承伤", IsVisible = true,
                Builder = () => new Column("damageTaken", "承伤", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "critRate", Title = "暴击率",IsVisible = true,
                Builder = () => new Column("critRate", "暴击率") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "luckyRate", Title = "幸运率",IsVisible = true,
                Builder = () => new Column("luckyRate", "幸运率") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "criticalDamage", Title = "纯暴击",IsVisible = true,
                Builder = () => new Column("criticalDamage", "纯暴击") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "luckyDamage", Title = "纯幸运",IsVisible = true,
                Builder = () => new Column("luckyDamage", "纯幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "critLuckyDamage", Title = "暴击幸运",IsVisible = true,
                Builder = () => new Column("critLuckyDamage", "暴击幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "instantDps", Title = "瞬时Dps",IsVisible = true,
                Builder = () => new Column("instantDps", "瞬时Dps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "maxInstantDps", Title = "最大瞬时Dps",IsVisible = true,
                Builder = () => new Column("maxInstantDps", "最大瞬时Dps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "totalDps", Title = "DPS/秒",IsVisible = true,
                Builder = () => new Column("totalDps", "DPS/秒", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "totalHealingDone", Title = "总治疗",IsVisible = true,
                Builder = () => new Column("totalHealingDone", "总治疗", ColumnAlign.Center) { SortOrder = true }
            },
            new ColumnSetting {
                Key = "criticalHealingDone", Title = "治疗暴击",IsVisible = true,
                Builder = () => new Column("criticalHealingDone", "治疗暴击") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "luckyHealingDone", Title = "治疗幸运",IsVisible = true,
                Builder = () => new Column("luckyHealingDone", "治疗幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "critLuckyHealingDone", Title = "治疗暴击幸运",IsVisible = true,
                Builder = () => new Column("critLuckyHealingDone", "治疗暴击幸运") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "instantHps", Title = "瞬时Hps",IsVisible = true,
                Builder = () => new Column("instantHps", "瞬时Hps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "maxInstantHps", Title = "最大瞬时Hps",IsVisible = true,
                Builder = () => new Column("maxInstantHps", "最大瞬时Hps") { SortOrder = true }
            },
            new ColumnSetting {
                Key = "totalHps", Title = "HPS/秒",IsVisible = true,
                Builder = () => new Column("totalHps", "HPS/秒", ColumnAlign.Center) { SortOrder = true }
            },
        };

        public static StackedHeaderRow[] BuildStackedHeader()
        {
            var list = new List<StackedColumn[]>();

            // 每个组：只包含当前显示的字段
            string[] group1 = { "totalDamage", "criticalDamage", "luckyDamage", "critLuckyDamage" };
            string[] group2 = { "instantDps", "maxInstantDps", "totalDps" };
            string[] group3 = { "totalHealingDone", "criticalHealingDone", "luckyHealingDone", "critLuckyHealingDone" };
            string[] group4 = { "instantHps", "maxInstantHps", "totalHps" };

            list.Add(BuildGroup(group1, "总伤害"));
            list.Add(BuildGroup(group2, "DPS"));
            list.Add(BuildGroup(group3, "总治疗"));
            list.Add(BuildGroup(group4, "HPS"));

            return new[] { new StackedHeaderRow(list.SelectMany(x => x).ToArray()) };
        }

        private static StackedColumn[] BuildGroup(string[] keys, string title)
        {
            var visible = keys.Where(k => ColumnSettingsManager.AllSettings.FirstOrDefault(x => x.Key == k)?.IsVisible == true).ToList();
            return visible.Count > 1
                ? new[] { new StackedColumn(string.Join(",", visible), title) }
                : Array.Empty<StackedColumn>();
        }


        public static ColumnCollection BuildColumns(bool includeExtraColumns)
        {
            var columns = new List<Column>
            {
                new AntdUI.Column("", "序号")
                {
                    Width = "50",
                    Render = (value, record, rowIndex) => rowIndex + 1,
                    Fixed = true
                },
                new AntdUI.Column("uid", "角色ID",ColumnAlign.Center){ SortOrder=true},
                new AntdUI.Column("nickname", "角色昵称",ColumnAlign.Center){ SortOrder=true},
                new AntdUI.Column("profession", "职业",ColumnAlign.Center),
                new AntdUI.Column("totalDamage", "总伤害",ColumnAlign.Center){ SortOrder=true},
            };
            if(!includeExtraColumns)
            {
                columns.AddRange(AllSettings.Where(s => s.IsVisible).Select(s => s.Builder()));
            }
            else
            {
                columns.Add(new Column("CellProgress", "团队总伤害占比", ColumnAlign.Center));
                columns.Add(new AntdUI.Column("totalDps", "Dps/秒", ColumnAlign.Center) { SortOrder = true });
                columns.Add(new AntdUI.Column("totalHps", "Hps/秒", ColumnAlign.Center) { SortOrder = true });
            }

            return new ColumnCollection(columns);
        }


       
    }
}
