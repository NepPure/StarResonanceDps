using AntdUI;
using SharpPcap;
using System.Windows.Forms;
using 星痕共鸣DPS统计.Plugin;

namespace 星痕共鸣DPS统计.Control
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
            if (config.network_card == -1)
            {
                SelectActiveNetworkCard();
                //InterfaceComboBox.SelectedIndex = config.network_card;
            }
            else
            {
                Console.WriteLine($"LoadDevices: 配置中指定的网卡索引: {config.network_card}");
                InterfaceComboBox.SelectedIndex = config.network_card;
                if (InterfaceComboBox.SelectedIndex >= 0 && InterfaceComboBox.Items.Count > InterfaceComboBox.SelectedIndex)
                {
                    string selectedCard = InterfaceComboBox.Items[InterfaceComboBox.SelectedIndex]?.ToString() ?? "未知网卡";
                    Console.WriteLine($"LoadDevices: 配置中指定的网卡: {selectedCard} (索引: {config.network_card})");
                }
                else
                {
                    Console.WriteLine($"LoadDevices: 配置中指定的网卡索引无效: {config.network_card}");
                }
            }
            combox_changed = true;

            input1.Text = config.mouseThroughKey.ToString();
            input2.Text = config.formTransparencyKey.ToString();
            input3.Text = config.windowToggleKey.ToString();
            input4.Text = config.clearDataKey.ToString();
            input5.Text = config.clearHistoryKey.ToString();
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
                            config.network_card = i;
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
                            config.network_card = i;
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
                    config.network_card = 0;
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
                config.network_card = InterfaceComboBox.SelectedIndex;
            }
        }

        private void colorPicker1_ValueChanged(object sender, ColorEventArgs e)
        {
            config.dpscolor = e.Value;
        }


        //鼠标穿透键位
        private void input1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input1.Text = string.Empty;
                return;
            }
            input1.Text = e.KeyCode.ToString();
            config.mouseThroughKey = e.KeyCode;
        }
        //窗体透明键位
        private void input2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input2.Text = string.Empty;
                return;
            }
            input2.Text = e.KeyCode.ToString();
            config.formTransparencyKey = e.KeyCode;
        }

        //开关键位
        private void input3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input3.Text = string.Empty;
                return;
            }
            input3.Text = e.KeyCode.ToString();
            config.windowToggleKey = e.KeyCode;
        }

        //清空数据键位
        private void input4_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input4.Text = string.Empty;
                return;
            }
            input4.Text = e.KeyCode.ToString();
            config.clearDataKey = e.KeyCode;
        }
        //清空历史键位
        private void input5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                input5.Text = string.Empty;
                return;
            }
            input5.Text = e.KeyCode.ToString();
            config.clearHistoryKey = e.KeyCode;
        }
    }
}
