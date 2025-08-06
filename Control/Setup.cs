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

            input1.Text = config.mouseThroughKey.ToString();
            input2.Text = config.formTransparencyKey.ToString();
            input3.Text = config.windowToggleKey.ToString();
            input4.Text = config.clearDataKey.ToString();
            input5.Text = config.clearHistoryKey.ToString();
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
