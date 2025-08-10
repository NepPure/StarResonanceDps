using Microsoft.VisualBasic.Devices;
using StarResonanceDpsAnalysis.Extends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Plugin
{
    public class AppConfig
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);


        private static string FilePath { get; } = $"{Environment.CurrentDirectory}\\config.ini";

        private static int? _networkCard = null;
        private static double? _transparency = null;
        private static int[] _defDpsColor = [252, 227, 138];
        private static Color? _dpsColor = null;
        private static bool? _isLight = null;
        private static Keys? _mouseThroughKey = null;
        private static Keys? _formTransparencyKey = null;
        private static Keys? _windowToggleKey = null;
        private static Keys? _clearDataKey = null;
        private static Keys? _clearHistoryKey = null;

        public static string NickName= "未设置昵称";
        public static ulong Uid=0;

        /// <summary>
        /// 网卡顺序
        /// </summary>
        public static int NetworkCard
        {
            get
            {
                if (_networkCard == null)
                {
                    var value = GetValue("SetUp", "NetworkCard", "-1").ToInt();
                    _networkCard = value;
                }
                return _networkCard!.Value;
            }
            set
            {
                SetValue("SetUp", "NetworkCard", value.ToString());
                _networkCard = value;
            }
        }

        /// <summary>
        /// 透明度
        /// </summary>
        public static double Transparency
        {
            get
            {
                if (_transparency == null)
                {
                    var value = GetValue("SetUp", "Transparency", "100").ToDouble();
                    _transparency = value;
                }
                return _transparency.Value;
            }
            set
            {
                SetValue("SetUp", "Transparency", value.ToString());
                _transparency = value;
            }
        }

  

        /// <summary>
        /// DPS占比条颜色
        /// </summary>
        public static Color DpsColor
        {
            get
            {
                if (_dpsColor == null)
                {
                    var value = GetValue("SetUp", "DpsColor", "252,227,138");
                    var rgb = value.Split(',').Select(e => e.ToInt()).ToArray();
                    foreach (var _byte in rgb)
                    {
                        if (_byte < 0 || _byte > 255)
                        {
                            rgb = _defDpsColor;
                            break;
                        }
                    }
                    _dpsColor = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                }
                return _dpsColor.Value;
            }
            set
            {
                SetValue("SetUp", "DpsColor", $"{value.R},{value.G},{value.B}");
                _dpsColor = value;
            }
        }

        /// <summary>
        /// 是否为浅色主题
        /// </summary>
        public static bool IsLight
        {
            get
            {
                if (_isLight == null)
                {
                    var value = GetValue("SetUp", "IsLight", "1");
                    _isLight = value == "1";
                }
                return _isLight.Value;
            }
            set
            {
                SetValue("SetUp", "IsLight", value ? "1" : "0");
                _isLight = value;
            }
        }

        /// <summary>
        /// 鼠标穿透键位
        /// </summary>
        public static Keys MouseThroughKey
        {
            get
            {
                if (_mouseThroughKey == null)
                {
                    var value = GetValue("SetKey", "MouseThroughKey", ((int)Keys.F6).ToString()).ToInt();
                    if (!Enum.IsDefined(typeof(Keys), value))
                    {
                        value = (int)Keys.F6;
                    }
                    _mouseThroughKey = (Keys)value;
                }
                return _mouseThroughKey.Value;
            }
            set
            {
                SetValue("SetKey", "MouseThroughKey", ((int)value).ToString());
                _mouseThroughKey = value;
            }
        }

        /// <summary>
        /// 窗体透明键位
        /// </summary>
        public static Keys FormTransparencyKey
        {
            get
            {
                if (_formTransparencyKey == null)
                {
                    var value = GetValue("SetKey", "FormTransparencyKey", ((int)Keys.F7).ToString()).ToInt();
                    if (!Enum.IsDefined(typeof(Keys), value))
                    {
                        value = (int)Keys.F7;
                    }
                    _formTransparencyKey = (Keys)value;
                }
                return _formTransparencyKey.Value;
            }
            set
            {
                SetValue("SetKey", "FormTransparencyKey", ((int)value).ToString());
                _formTransparencyKey = value;
            }
        }

        /// <summary>
        /// 开关键位（比如 显示/隐藏 窗口）
        /// </summary>
        public static Keys WindowToggleKey
        {
            get
            {
                if (_windowToggleKey == null)
                {
                    var value = GetValue("SetKey", "WindowToggleKey", ((int)Keys.F8).ToString()).ToInt();
                    if (!Enum.IsDefined(typeof(Keys), value))
                    {
                        value = (int)Keys.F8;
                    }
                    _windowToggleKey = (Keys)value;
                }
                return _windowToggleKey.Value;
            }
            set
            {
                SetValue("SetKey", "WindowToggleKey", ((int)value).ToString());
                _windowToggleKey = value;
            }
        }

        /// <summary>
        /// 清空数据键位
        /// </summary>
        public static Keys ClearDataKey
        {
            get
            {
                if (_clearDataKey == null)
                {
                    var value = GetValue("SetKey", "ClearDataKey", ((int)Keys.F9).ToString()).ToInt();
                    if (!Enum.IsDefined(typeof(Keys), value))
                    {
                        value = (int)Keys.F9;
                    }
                    _clearDataKey = (Keys)value;
                }
                return _clearDataKey.Value;
            }
            set
            {
                SetValue("SetKey", "ClearDataKey", ((int)value).ToString());
                _clearDataKey = value;
            }
        }

        /// <summary>
        /// 清空历史键位
        /// </summary>
        public static Keys ClearHistoryKey
        {
            get
            {
                if (_clearHistoryKey == null)
                {
                    var value = GetValue("SetKey", "ClearHistoryKey", ((int)Keys.F10).ToString()).ToInt();
                    if (!Enum.IsDefined(typeof(Keys), value))
                    {
                        value = (int)Keys.F10;
                    }
                    _clearHistoryKey = (Keys)value;
                }
                return _clearHistoryKey.Value;
            }
            set
            {
                SetValue("SetKey", "ClearHistoryKey", ((int)value).ToString());
                _clearHistoryKey = value;
            }
        }

        public static bool GetConfigExists()
        {
            return File.Exists(FilePath);
        }

        /// <summary>
        /// 从 INI 配置文件中读取指定 Section 和 Key 的值。
        /// </summary>
        /// <param name="section">配置节名称（Section）</param>
        /// <param name="key">键名称（Key）</param>
        /// <param name="def">如果找不到该键时返回的默认值</param>
        /// <returns>读取到的值（字符串）</returns>
        public static string GetValue(string section, string key, string def)
        {
            // 用于存储读取结果的缓冲区
            var buffer = new StringBuilder(64);
            // 调用 WinAPI GetPrivateProfileString 读取 INI 文件内容
            _ = GetPrivateProfileString(section, key, def, buffer, buffer.Capacity, FilePath);

            return buffer.ToString();
        }

        /// <summary>
        /// 将指定的值写入到 INI 配置文件的指定 Section 和 Key 中。
        /// </summary>
        /// <param name="section">配置节名称（Section）</param>
        /// <param name="key">键名称（Key）</param>
        /// <param name="value">要写入的值</param>
        public static void SetValue(string section, string key, string value)
        {
            // 调用 WinAPI WritePrivateProfileString 写入 INI 文件
            WritePrivateProfileString(section, key, value, FilePath);
        }

    }
}
