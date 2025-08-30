using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Extends.Data
{
    public static class ProfessionExtends
    {
        /// <summary>
        /// 职业ID映射为职业名称
        /// </summary>
        public static string GetProfessionNameFromId(this int professionId) 
            => professionId switch
            {
                1 => "雷影剑士",
                2 => "冰魔导师",
                3 => "涤罪恶火_战斧",
                4 => "青岚骑士",
                5 => "森语者",
                9 => "巨刃守护者",
                11 => "神射手",
                12 => "神盾骑士",
                8 => "雷霆一闪_手炮",
                10 => "暗灵祈舞_仪刀_仪仗",
                13 => "灵魂乐手",
                _ => string.Empty,
            };
    }
}
