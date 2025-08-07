using AntdUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 星痕共鸣DPS统计.Plugin;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace 星痕共鸣DPS统计.Control
{
    public partial class DataDisplaySettings : UserControl
    {
        public DataDisplaySettings(BorderlessForm borderlessForm)
        {
            InitializeComponent();
        }

        private void DataDisplaySettings_Load(object sender, EventArgs e)
        {
            flowPanel1.Controls.Clear();

            foreach (var item in ColumnSettingsManager.AllSettings)
            {
                var cb = new AntdUI.Checkbox
                {
                    Text = item.Title,
                    Name = item.Key,
                    Checked = item.IsVisible,
                    AutoSize = true,
                    Tag = item.Key, // 标记 key 方便操作
                   
                };
                cb.CheckedChanged += checkbox_CheckedChanged;
                flowPanel1.Controls.Add(cb);
            }
        }

        private void checkbox_CheckedChanged(object sender, BoolEventArgs e)
        {
            if (sender is AntdUI.Checkbox cb && cb.Tag is string key)
            {
                var setting = ColumnSettingsManager.AllSettings.FirstOrDefault(x => x.Key == key);
                if (setting != null)
                {
                    setting.IsVisible = cb.Checked;

                }
                ColumnSettingsManager.RefreshTableAction?.Invoke();

                
                config.reader.SaveValue("TabelSet", cb.Name, cb.Checked.ToString());
                config.reader.Save(config.config_ini);
            }
        }
    }
}
