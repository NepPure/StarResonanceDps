using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SkillDetailForm : BorderlessForm
    {
        public SkillDetailForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            ToggleTableView();
        }

        private void SkillDetailForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色

        }

        private bool _suspendUiUpdate = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_suspendUiUpdate) return;

            var p = StatisticData._manager.GetOrCreate(Uid);

            if (segmented1.SelectIndex == 0)
            {
                // ===== 伤害总览 =====
                TotalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Total);
                TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalDps());
                CritRateText.Text = $"{(p.DamageStats.GetCritRate() * 100):0.#}%";
                LuckyRate.Text = $"{(p.DamageStats.GetLuckyRate() * 100):0.#}%";

                NormalDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Normal);
                CritDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Critical);
                LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.Lucky);
                AvgDamageText.Text = Common.FormatWithEnglishUnits(p.DamageStats.GetAveragePerHit());

                // ===== 技能表（伤害）=====
                UpdateSkillTable(Uid, false);
            }
            else
            {
                // ===== 治疗总览 =====
                TotalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Total);
                TotalDpsText.Text = Common.FormatWithEnglishUnits(p.GetTotalHps());
                CritRateText.Text = $"{(p.HealingStats.GetCritRate() * 100):0.#}%";
                LuckyRate.Text = $"{(p.HealingStats.GetLuckyRate() * 100):0.#}%";

                NormalDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Normal);
                CritDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Critical);
                LuckyDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.Lucky);
                AvgDamageText.Text = Common.FormatWithEnglishUnits(p.HealingStats.GetAveragePerHit());

                // ===== 技能表（治疗）=====
                UpdateSkillTable(Uid, true);
            }
        }

        private void segmented1_SelectIndexChanged(object sender, IntEventArgs e)
        {
            // 暂停一次刷新
            _suspendUiUpdate = true;

            // 切换时清一次技能表，避免残留
            SkillTableDatas.SkillTable.Clear();

            // 立刻按新模式刷新一次
            bool isHeal = segmented1.SelectIndex != 0;
            UpdateSkillTable(Uid, isHeal);

            // 下一轮计时器再恢复
            _suspendUiUpdate = false;
        }

    }
}
