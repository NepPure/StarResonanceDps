using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.WPF.Views
{
    public partial class DpsStatisticsView
    {
        public class SkillItem
        {
            public string SkillName { get; set; }
            public string TotalDamage { get; set; }
            public int HitCount { get; set; }
            public int CritCount { get; set; }
            public int AvgDamage { get; set; }
        }
    }
}
