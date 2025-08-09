using AntdUI;
using SharpPcap;
using System.Windows.Forms;
using StarResonanceDpsAnalysis.Plugin;

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
        public void LoadDevices()
        {

            var devices = CaptureDeviceList.Instance;
            foreach (var dev in devices)
            {
                InterfaceComboBox.Items.Add(dev.Description);
            }
            if (AppConfig.NetworkCard == -1)
            {
                SelectActiveNetworkCard();
                //InterfaceComboBox.SelectedIndex = AppConfig.NetworkCard;
            }
            else
            {
                Console.WriteLine($"LoadDevices: 配置中指定的网卡索引: {AppConfig.NetworkCard}");
                InterfaceComboBox.SelectedIndex = AppConfig.NetworkCard;
                if (InterfaceComboBox.SelectedIndex >= 0 && InterfaceComboBox.Items.Count > InterfaceComboBox.SelectedIndex)
                {
                    string selectedCard = InterfaceComboBox.Items[InterfaceComboBox.SelectedIndex]?.ToString() ?? "未知网卡";
                    Console.WriteLine($"LoadDevices: 配置中指定的网卡: {selectedCard} (索引: {AppConfig.NetworkCard})");
                }
                else
                {
                    Console.WriteLine($"LoadDevices: 配置中指定的网卡索引无效: {AppConfig.NetworkCard}");
                }
            }
            combox_changed = true;

            input1.Text = AppConfig.MouseThroughKey.ToString();
            input2.Text = AppConfig.FormTransparencyKey.ToString();
            input3.Text = AppConfig.WindowToggleKey.ToString();
            input4.Text = AppConfig.ClearDataKey.ToString();
            input5.Text = AppConfig.ClearHistoryKey.ToString();
        }

        /// <summary>
        /// 自动选择当前活动的网卡（优先选择有网络连接的网卡）
        /// </summary>
        private void SelectActiveNetworkCard()
        {
            try
            {
                // 获取所有网络接口和SharpPcap设备
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                var sharpPcapDevices = CaptureDeviceList.Instance;

                Console.WriteLine("开始自动选择活动网卡...");
                Console.WriteLine($"找到 {networkInterfaces.Length} 个网络接口");
                Console.WriteLine($"SharpPcap设备数量: {sharpPcapDevices.Count}");

                // 筛选运行中且有IPv4地址的活动网卡
                var activeInterfaces = networkInterfaces
                    .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                                ni.GetIPProperties().UnicastAddresses.Any(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                    .ToList();

                Console.WriteLine($"活动网卡数量: {activeInterfaces.Count}");

                // 优先选择有默认网关的网卡（通常表示有Internet连接）
                var preferredInterface = activeInterfaces
                    .FirstOrDefault(ni => ni.GetIPProperties().GatewayAddresses.Any());

                if (preferredInterface != null)
                {
                    Console.WriteLine($"找到有默认网关的网卡: {preferredInterface.Name} ({preferredInterface.Description})");

                    // 尝试匹配SharpPcap设备（不区分大小写）
                    for (int i = 0; i < sharpPcapDevices.Count; i++)
                    {
                        if (sharpPcapDevices[i].Description.IndexOf(preferredInterface.Name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            sharpPcapDevices[i].Description.IndexOf(preferredInterface.Description, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine($"匹配到SharpPcap设备: {sharpPcapDevices[i].Description}");
                            InterfaceComboBox.SelectedIndex = i;
                            AppConfig.NetworkCard = i;
                            string selectedCard = sharpPcapDevices[i].Description;
                            Console.WriteLine($"已自动选择有网络连接的网卡: {selectedCard} (索引: {i})");
                            return;
                        }
                    }
                }

                // 如果没有找到有默认网关的网卡，检查所有活动网卡
                Console.WriteLine("未找到有默认网关的网卡，检查所有活动网卡");
                foreach (var ni in activeInterfaces)
                {
                    Console.WriteLine($"检查活动网卡: {ni.Name} ({ni.Description})");

                    // 尝试匹配SharpPcap设备（不区分大小写）
                    for (int i = 0; i < sharpPcapDevices.Count; i++)
                    {
                        if (sharpPcapDevices[i].Description.IndexOf(ni.Name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            sharpPcapDevices[i].Description.IndexOf(ni.Description, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine($"匹配到SharpPcap设备: {sharpPcapDevices[i].Description}");
                            InterfaceComboBox.SelectedIndex = i;
                            AppConfig.NetworkCard = i;
                            string selectedCard = sharpPcapDevices[i].Description;
                            Console.WriteLine($"已自动选择活动网卡: {selectedCard} (索引: {i})");
                            return;
                        }
                    }
                }

                // 如果没有找到匹配的活动网卡，选择第一个可用网卡
                if (InterfaceComboBox.Items.Count > 0)
                {
                    Console.WriteLine("未找到活动网卡，选择第一个可用网卡");
                    InterfaceComboBox.SelectedIndex = 0;
                    AppConfig.NetworkCard = 0;
                }
                else
                {
                    Console.WriteLine("没有可用的网卡");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动选择网卡失败: {ex.Message}");
            }
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
