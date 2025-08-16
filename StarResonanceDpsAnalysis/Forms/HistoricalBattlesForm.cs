using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class HistoricalBattlesForm : BorderlessForm
    {
        public HistoricalBattlesForm()
        {
            InitializeComponent();
            Text = FormManager.APP_NAME;
            FormGui.SetDefaultGUI(this); // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）
        }

        private void HistoricalBattlesForm_Load(object sender, EventArgs e)
        {
            RefreshSnapshotCombo();
        }

        // ===== 1) 绑定到 ComboBox：把所有快照的时间放进去 =====
        private sealed class SnapshotComboItem
        {
            public int Index { get; init; }
            public DateTime StartedAt { get; init; }
            public DateTime EndedAt { get; init; }
            public TimeSpan Duration { get; init; }
            public BattleSnapshot Snapshot { get; init; }

            // 下拉显示的文字：08-16 17:20:05 ~ 17:33:41（00:13:36）
            public string Display => $"{StartedAt:MM-dd HH:mm:ss} ~ {EndedAt:HH:mm:ss}（{Duration:hh\\:mm\\:ss}）";
            public override string ToString() => Display; // 以防未设置 DisplayMember
        }

        // 放窗体里：给下拉项一个承载“参数”的类型
        private sealed class ComboItem
        {
            public BattleSnapshot Snapshot { get; init; }
            public override string ToString()
            {
                var s = Snapshot;
                return $"{s.StartedAt:MM-dd HH:mm:ss} ~ {s.EndedAt:HH:mm:ss}（{s.Duration:hh\\:mm\\:ss}）";
            }
        }

        /// <summary>
        /// 重新加载历史快照下拉框（可在窗体加载、拍完快照后调用）
        /// </summary>
        private void RefreshSnapshotCombo(bool selectLast = true)
        {
            var history = StatisticData._manager.History?.ToList() ?? new List<BattleSnapshot>();

         
            select1.Items.Clear();

            if (history.Count == 0)
            {
                select1.Items.Add("暂无记录");
                select1.Enabled = false;
                select1.SelectedIndex = 0;
             
                return;
            }

            select1.Enabled = true;
            foreach (var snap in history)
            {
                select1.Items.Add(new ComboItem { Snapshot = snap }); // 直接把快照塞到项里
            }

            // 选中最后一条（最新）
            select1.SelectedIndex = selectLast ? select1.Items.Count - 1 : 0;
            
        }
        private void HistoricalBattles()
        {

        }
    }
}
