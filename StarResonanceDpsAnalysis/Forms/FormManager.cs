using StarResonanceDpsAnalysis.Control;
using System.Runtime.InteropServices;

namespace StarResonanceDpsAnalysis.Forms
{
    public class FormManager
    {
        public static SkillDiary skillDiary;//



        public static SkillDetailForm skillDetailForm;//技能详情窗体

        public static SettingsForm settingsForm;//设置窗体

        public static DpsStatistics dpsStatistics;//DPS统计窗体

        public static UserUidSetForm userUidSetForm;//用户Uid设置窗体

        public static RankingsForm rankingsForm;//排行榜窗体


        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
