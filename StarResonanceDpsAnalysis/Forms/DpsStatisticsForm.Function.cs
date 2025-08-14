using DocumentFormat.OpenXml.Office2010.ExcelAc;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Control.GDI;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatisticsForm
    {
        public void SetStyle()
        {
            // ======= 单个进度条（textProgressBar1）的外观设置 =======
            textProgressBar1.Padding = new Padding(3, 3, 3, 3);
            textProgressBar1.ProgressBarCornerRadius = 3; // 超大圆角

            // ======= 进度条列表（sortedProgressBarList1）的初始化与外观 =======
            sortedProgressBarList1.ProgressBarHeight = 50;  // 每行高度
            sortedProgressBarList1.AnimationDuration = 1000; // 动画时长（毫秒）
            sortedProgressBarList1.AnimationQuality = Quality.High; // 动画品质（你项目里的枚举）




        }
        readonly static List<ProgressBarData> list = new List<ProgressBarData>();
        Dictionary<string, Color> colorDict = new Dictionary<string, Color>()
        {
            { "神射手", ColorTranslator.FromHtml("#fffca3") }, //
            { "冰魔导师", ColorTranslator.FromHtml("#aaa6ff") }, // 
            { "巨刃守护者", ColorTranslator.FromHtml("#51a55d") }, // 
            { "雷影剑士", ColorTranslator.FromHtml("#9676ff") }, // 
            { "灵魂乐手", ColorTranslator.FromHtml("#ff5353") }, // 
            { "青岚骑士", ColorTranslator.FromHtml("#abfaff") }, // 
            { "森语者", ColorTranslator.FromHtml("#78ff95") }, // 
            { "神盾骑士", ColorTranslator.FromHtml("#2E86AB") }, // 
            {"未知",  ColorTranslator.FromHtml("#2E86AB")}
        };
        public void RefreshDpsTable()
        {
            var statsList = StatisticData._manager.GetPlayersWithCombatData().ToList();
            if (statsList.Count == 0) return;

            float totalDamageSum = statsList
                .Where(p => p?.DamageStats != null)
                .Sum(p => (float)p.DamageStats.Total);
            if (totalDamageSum <= 0f) totalDamageSum = 1f;

            var maxDamage = statsList.Max(p => (float)(p?.DamageStats?.Total ?? 0));

            var ordered = statsList
                .OrderByDescending(p => p?.DamageStats?.Total ?? 0)
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var p = ordered[i];
                var uid = (long)p.Uid;
                int ranking = i + 1;

                var realtime = Common.FormatWithEnglishUnits(Math.Round(p.DamageStats.GetTotalPerSecond(), 1));
                string totalFmt = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                string share = (p.DamageStats.Total / totalDamageSum * 100).ToString("0") + "%";

                float progress = maxDamage > 0 ? (float)(p.DamageStats.Total / maxDamage) : 0f;

                var existing = list.FirstOrDefault(x => x.ID == uid);
                if (existing != null)
                {
                    // 更新
                    existing.ContentList =
                    [
                        new RenderContent
                        {
                            Type = RenderContent.ContentType.Text,
                            Align = RenderContent.ContentAlign.MiddleLeft,
                            Offset = new RenderContent.ContentOffset { X = 10, Y = 0 },
                            Text =  $"  {ranking} [图标] {p.Nickname} ({p.CombatPower})      {totalFmt} ({realtime}) {share}",
                            ForeColor = Color.Black,
                            Font = SystemFonts.DefaultFont,
                        }
                    ];
                    existing.ProgressBarValue = progress;

                }
                else
                {
                    // 新增
                    list.Add(new ProgressBarData
                    {
                        ID = uid,
                        ContentList = 
                        [
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 10, Y = 0 },
                                Text = $"   {ranking} [图标] {p.Nickname} ({p.CombatPower})      {totalFmt} ({realtime}) {share}",
                                ForeColor = Color.Black,
                                Font = SystemFonts.DefaultFont
                            }    
                        ],
                        ProgressBarCornerRadius = 3,
                        ProgressBarValue = progress,
                        ProgressBarColor = colorDict[p.Profession],
                    });
                }
            }
            // 如果有控件需要刷新，可以在这里重新绑定一次数据
            sortedProgressBarList1.Data = list;


        }

    }
}
