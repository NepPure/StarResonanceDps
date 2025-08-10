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
        public static List<ColumnSetting> AllSettings =
        [
            new() {
                Key = "combatPower", Title = "战力", IsVisible = true,
                Builder = () => new Column("combatPower", "战力", ColumnAlign.Center)
            },
            new() {
                Key = "totalDamage", Title = "总伤害", IsVisible = true,
                Builder = () =>  new AntdUI.Column("totalDamage", "总伤害",ColumnAlign.Center)
            },
            new() {
                Key = "damageTaken", Title = "承伤", IsVisible = true,
                Builder = () => new Column("damageTaken", "承伤", ColumnAlign.Center)
            },
            new() {
                Key = "critRate", Title = "暴击率", IsVisible = true,
                Builder = () => new Column("critRate", "暴击率")
            },
            new() {
                Key = "luckyRate", Title = "幸运率", IsVisible = true,
                Builder = () => new Column("luckyRate", "幸运率")
            },
            new() {
                Key = "criticalDamage", Title = "纯暴击", IsVisible = true,
                Builder = () => new Column("criticalDamage", "纯暴击")
            },
            new() {
                Key = "luckyDamage", Title = "纯幸运", IsVisible = true,
                Builder = () => new Column("luckyDamage", "纯幸运")
            },
            new() {
                Key = "critLuckyDamage", Title = "暴击幸运", IsVisible = true,
                Builder = () => new Column("critLuckyDamage", "暴击幸运")
            },
            new() {
                Key = "instantDps", Title = "瞬时Dps", IsVisible = true,
                Builder = () => new Column("instantDps", "瞬时Dps")
            },
            new() {
                Key = "maxInstantDps", Title = "最大瞬时Dps", IsVisible = true,
                Builder = () => new Column("maxInstantDps", "最大瞬时Dps")
            },
            new() {
                Key = "totalDps", Title = "DPS", IsVisible = true,
                Builder = () => new Column("totalDps", "DPS", ColumnAlign.Center)
            },
            new() {
                Key = "totalHealingDone", Title = "总治疗", IsVisible = true,
                Builder = () => new Column("totalHealingDone", "总治疗", ColumnAlign.Center)
            },
            new() {
                Key = "criticalHealingDone", Title = "治疗暴击", IsVisible = true,
                Builder = () => new Column("criticalHealingDone", "治疗暴击")
            },
            new() {
                Key = "luckyHealingDone", Title = "治疗幸运", IsVisible = true,
                Builder = () => new Column("luckyHealingDone", "治疗幸运")
            },
            new() {
                Key = "critLuckyHealingDone", Title = "治疗暴击幸运", IsVisible = true,
                Builder = () => new Column("critLuckyHealingDone", "治疗暴击幸运")
            },
            new() {
                Key = "instantHps", Title = "瞬时Hps", IsVisible = true,
                Builder = () => new Column("instantHps", "瞬时Hps")
            },
            new() {
                Key = "maxInstantHps", Title = "最大瞬时Hps", IsVisible = true,
                Builder = () => new Column("maxInstantHps", "最大瞬时Hps")
            },
            new() {
                Key = "totalHps", Title = "HPS", IsVisible = true,
                Builder = () => new Column("totalHps", "HPS", ColumnAlign.Center)
            },
        ];

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

            return [new StackedHeaderRow([.. list.SelectMany(x => x)])];
        }

        private static StackedColumn[] BuildGroup(string[] keys, string title)
        {
            var visible = keys.Where(k => AllSettings.FirstOrDefault(x => x.Key == k)?.IsVisible ?? false).ToList();
            return visible.Count > 1
                ? [new StackedColumn(string.Join(',', visible), title)]
                : [];
        }


        public static ColumnCollection BuildColumns(bool includeExtraColumns)
        {
            var columns = new List<Column>
            {
                new("", "序号")
                {
                    Width = "50",
                    Render = (value, record, rowIndex) => rowIndex + 1,
                    Fixed = true
                },
                new("uid", "角色ID",ColumnAlign.Center){ SortOrder = true },
                new("nickname", "角色昵称",ColumnAlign.Center){ SortOrder = true },
                new("profession", "职业",ColumnAlign.Center),
            };
            if (!includeExtraColumns)
            {
                columns.AddRange(AllSettings.Where(s => s.IsVisible).Select(s => s.Builder()));
            }
            else
            {
                columns.Add(new Column("CellProgress", "团队总伤害占比", ColumnAlign.Center));
                columns.Add(new Column("totalDps", "DPS", ColumnAlign.Center) { SortOrder = true });
                columns.Add(new Column("totalHps", "HPS", ColumnAlign.Center) { SortOrder = true });
            }

            return [.. columns];
        }



    }
}
