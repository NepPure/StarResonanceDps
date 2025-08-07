using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Plugin
{
    public class AppConfig
    {
        public static string ConfigIni = "config.ini";

        /// <summary>
        /// 网卡顺序
        /// </summary>
        public static int NetworkCard = -1;

        /// <summary>
        /// ini读取保存
        /// </summary>
        public static IniFileReader Reader = new();

        /// <summary>
        /// 透明度
        /// </summary>
        public static double Transparency = 100;

        /// <summary>
        /// DPS占比条颜色
        /// </summary>
        public static Color DpsColor = Color.FromArgb(252, 227, 138);

        /// <summary>
        /// 是否为浅色主题
        /// </summary>
        public static bool IsLight = true;

        /// <summary>
        /// 鼠标穿透键位
        /// </summary>
        public static Keys MouseThroughKey = Keys.F6;

        /// <summary>
        /// 窗体透明键位
        /// </summary>
        public static Keys FormTransparencyKey = Keys.F7;

        /// <summary>
        /// 开关键位（比如 显示/隐藏 窗口）
        /// </summary>
        public static Keys WindowToggleKey = Keys.F8;

        /// <summary>
        /// 清空数据键位
        /// </summary>
        public static Keys ClearDataKey = Keys.F9;

        /// <summary>
        /// 清空历史键位
        /// </summary>
        public static Keys ClearHistoryKey = Keys.F10;

    }
}
