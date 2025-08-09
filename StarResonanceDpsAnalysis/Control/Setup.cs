using AntdUI;
using SharpPcap;
using System.Windows.Forms;
using StarResonanceDpsAnalysis.Plugin;
using System.Text;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class Setup : UserControl
    {
        public Setup(BorderlessForm borderlessForm)
        {
            InitializeComponent();
        }

        private void Setup_Load(object sender, EventArgs e)
        {
            LoadDevices();
        }

        private bool combox_changed = false;

        /// <summary>
        /// 加载本机所有网卡到下拉框
        /// </summary>
        private const bool DEBUG_LOG = true;
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");

        private void Log(string msg)
        {
            if (!DEBUG_LOG) return;
            string text = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            Console.WriteLine(text);
            try { File.AppendAllText(LogFile, text + Environment.NewLine, Encoding.UTF8); } catch { }
        }

        public void LoadDevices()
        {
            var devices = CaptureDeviceList.Instance;
            InterfaceComboBox.Items.Clear();
            foreach (var d in devices) InterfaceComboBox.Items.Add(d.Description);

            // 自动选择或按配置选择
            int targetIndex = (AppConfig.NetworkCard >= 0 && AppConfig.NetworkCard < devices.Count)
                ? AppConfig.NetworkCard
                : GetBestNetworkCardIndex(devices);

            if (targetIndex >= 0)
            {
                InterfaceComboBox.SelectedIndex = targetIndex;
                AppConfig.NetworkCard = targetIndex;
                SaveConfig("SetUp", "NetworkCard", targetIndex.ToString());
                Log($"选择网卡: {devices[targetIndex].Description} (索引: {targetIndex})");
            }
            else
            {
                Log("未找到可用网卡");
            }

            combox_changed = true;
            input1.Text = AppConfig.MouseThroughKey.ToString();
            input2.Text = AppConfig.FormTransparencyKey.ToString();
            input3.Text = AppConfig.WindowToggleKey.ToString();
            input4.Text = AppConfig.ClearDataKey.ToString();
            input5.Text = AppConfig.ClearHistoryKey.ToString();
        }

        private int GetBestNetworkCardIndex(CaptureDeviceList devices)
        {
            var active = System.Net.NetworkInformation.NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                             ni.GetIPProperties().UnicastAddresses.Any(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                .OrderByDescending(ni => ni.GetIPProperties().GatewayAddresses.Any()) // 网关优先
                .FirstOrDefault();

            if (active == null) return devices.Count > 0 ? 0 : -1;

            // 匹配分数最高的设备
            int bestIndex = -1, bestScore = -1;
            for (int i = 0; i < devices.Count; i++)
            {
                int score = 0;
                if (devices[i].Description.Contains(active.Name, StringComparison.OrdinalIgnoreCase)) score += 2;
                if (devices[i].Description.Contains(active.Description, StringComparison.OrdinalIgnoreCase)) score += 3;
                if (score > bestScore) { bestScore = score; bestIndex = i; }
            }
            return bestIndex;
        }

        private void SaveConfig(string section, string key, string value)
        {
            AppConfig.Reader.Load(AppConfig.ConfigIni);
            AppConfig.Reader.SaveValue(section, key, value);
            AppConfig.Reader.Save(AppConfig.ConfigIni);
        }
        private void InterfaceComboBox_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (combox_changed)
            {
                AppConfig.NetworkCard = InterfaceComboBox.SelectedIndex;
            }
        }

        private void colorPicker1_ValueChanged(object sender, ColorEventArgs e)
        {
            AppConfig.DpsColor = e.Value;
        }

        /// <summary>
        /// 鼠标穿透键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input1.Text = string.Empty;
                return;
            }
            input1.Text = e.KeyCode.ToString();
            AppConfig.MouseThroughKey = e.KeyCode;
        }

        /// <summary>
        /// 窗体透明键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input2.Text = string.Empty;
                return;
            }
            input2.Text = e.KeyCode.ToString();
            AppConfig.FormTransparencyKey = e.KeyCode;
        }

        /// <summary>
        /// 开关键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input3.Text = string.Empty;
                return;
            }
            input3.Text = e.KeyCode.ToString();
            AppConfig.WindowToggleKey = e.KeyCode;
        }

        /// <summary>
        /// 清空数据键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input4_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input4.Text = string.Empty;
                return;
            }
            input4.Text = e.KeyCode.ToString();
            AppConfig.ClearDataKey = e.KeyCode;
        }

        /// <summary>
        /// 清空历史键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input5.Text = string.Empty;
                return;
            }
            input5.Text = e.KeyCode.ToString();
            AppConfig.ClearHistoryKey = e.KeyCode;
        }
    }
}
