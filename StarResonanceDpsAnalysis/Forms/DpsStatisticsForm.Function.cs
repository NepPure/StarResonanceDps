using DocumentFormat.OpenXml.Office2010.ExcelAc;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Effects.Enum;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatisticsForm
    {
        public void SetStyle()
        {
            // ======= 单个进度条（textProgressBar1）的外观设置 =======
            textProgressBar1.Padding = new Padding(3, 3, 3, 3);
            textProgressBar1.TextPadding = new Padding(3, 3, 3, 3);
            textProgressBar1.ProgressBarCornerRadius = 3; // 超大圆角

            // ======= 进度条列表（sortedProgressBarList1）的初始化与外观 =======
            sortedProgressBarList1.Data = list;             // 绑定数据源（引用类型，后续更新 list[i] 会反映到控件）
            sortedProgressBarList1.ProgressBarHeight = 30;  // 每行高度
            sortedProgressBarList1.AnimationDuration = 1000; // 动画时长（毫秒）
            sortedProgressBarList1.AnimationQuality = Quality.High; // 动画品质（你项目里的枚举）


        }
        readonly static List<ProgressBarData> list = new List<ProgressBarData>();

        public static void RefreshDpsTable()
        {
            var statsList = StatisticData._manager
                .GetPlayersWithCombatData();
            if (statsList.Count() <= 0) return;
           
            //foreach (var stats in statsList)
            //{
            //    var index = list.FindIndex(x => x.ID == stats.Uid);
            //    list[index]

            //}
        }
    }
}
