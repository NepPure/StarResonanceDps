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
    }
}
