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
        private void LoadDevices()
        {

            var devices = CaptureDeviceList.Instance;
            foreach (var dev in devices)
            {

                InterfaceComboBox.Items.Add(dev.Description);
            }
            if (config.network_card != -1)
            {
                InterfaceComboBox.SelectedIndex = config.network_card;
            }
            combox_changed = true;

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
    }
}
