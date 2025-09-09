using System;
using System.Runtime.InteropServices;

using StarResonanceDpsAnalysis.WinForm.Control;
using StarResonanceDpsAnalysis.WinForm.Forms.AuxiliaryForms;
using StarResonanceDpsAnalysis.WinForm.Forms.ModuleForm;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public class FormManager
    {
        public const string APP_NAME = "别查我DPS";
        public static string AppVersion { get => $"v{Application.ProductVersion.Split('+')[0]}"; }

        private static SettingsForm? _settingsForm = null;
        private static DpsStatisticsForm? _dpsStatisticsForm = null;
        private static UserUidSetForm? _userUidSetForm = null;
        private static MainForm? _mainForm = null;

        public static SettingsForm SettingsForm
        {
            get
            {
                if (_settingsForm == null || _settingsForm.IsDisposed)
                {
                    _settingsForm = new();
                }

                return _settingsForm;
            }
        }

        public static DpsStatisticsForm DpsStatistics 
        {
            get 
            {
                if (_dpsStatisticsForm == null || _dpsStatisticsForm.IsDisposed)
                {
                    _dpsStatisticsForm = new();
                }

                return _dpsStatisticsForm;
            }   
        }

        public static UserUidSetForm UserUidSetForm
        {
            get 
            {
                if (_userUidSetForm == null || _userUidSetForm.IsDisposed)
                {
                    _userUidSetForm = new();
                }

                return _userUidSetForm;
            }
        }

        public static MainForm MainForm 
        {
            get 
            {
                if (_mainForm == null || _mainForm.IsDisposed)
                {
                    _mainForm = new();
                }

                return _mainForm;
            }
        }

        private static Form[] TransparencyForms => [SettingsForm, DpsStatistics, UserUidSetForm];
        /// <summary>
        /// 统一设置透明度
        /// </summary>
        /// <param name="opacity"></param>
        public static void FullFormTransparency(double opacity)
        {
            foreach (var form in TransparencyForms)
            {
                try
                {
                    form.Opacity = opacity;
                }
                catch (Exception) { }
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    }
}
