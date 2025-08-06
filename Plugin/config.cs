using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 星痕共鸣DPS统计.Plugin
{
    public class config
    {
        public static int network_card = -1;//网卡顺序
        /// <summary>
        /// ini读取保存
        /// </summary>
        public static IniFileReader reader = new IniFileReader();
        public static string config_ini = "config.ini";
        public static double transparency = 100;//透明度
        public static Color dpscolor = Color.FromArgb(252, 227, 138);//DPS占比条颜色

        public static bool isLight = true;//是否为浅色主题


        public static Keys mouseThroughKey = Keys.F6;         // 鼠标穿透键位  
        public static Keys formTransparencyKey = Keys.F7;     // 窗体透明键位
        public static Keys windowToggleKey = Keys.F8;         // 开关键位（比如 显示/隐藏 窗口）
        public static Keys clearDataKey = Keys.F9;            // 清空数据键位
        public static Keys clearHistoryKey = Keys.F10;         // 清空历史键位

    }
}
