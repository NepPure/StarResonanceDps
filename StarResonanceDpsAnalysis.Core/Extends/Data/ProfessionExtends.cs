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
        public static string GetProfessionNameById(this int professionId)
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

        public static string GetSubProfessionBySkillId(this long skillId)
            => skillId switch
            {
                // 神射手
                2292 or 1700820 or 1700825 or 1700827 => "狼弓",
                220112 or 2203622 or 220106 => "鹰弓",

                // 森语者
                1518 or 1541 or 21402 => "惩戒",
                20301 => "愈合",

                // 雷影剑士
                1714 or 1734 => "居合",
                44701 or 179906 => "月刃",

                // 冰魔导师
                120901 or 120902 => "冰矛",
                1241 => "射线",

                // 青岚骑士
                1405 or 1418 => "重装",
                1419 => "空枪",

                // 巨刃守护者
                199902 => "岩盾",
                1930 or 1931 or 1934 or 1935 => "格挡",

                // 神盾骑士
                2405 => "防盾",
                2406 => "光盾",

                // 灵魂乐手
                2306 => "狂音",
                2307 or 2361 or 55302 => "协奏",

                _ => string.Empty
            };
    }
}
