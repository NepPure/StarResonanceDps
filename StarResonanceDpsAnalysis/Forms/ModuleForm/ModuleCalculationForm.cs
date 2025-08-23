using AntdUI;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Core.Module;
using StarResonanceDpsAnalysis.Forms.PopUp;
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
using static StarResonanceDpsAnalysis.Core.Module.ModuleCardDisplay;

namespace StarResonanceDpsAnalysis.Forms.ModuleForm
{
    public partial class ModuleCalculationForm : BorderlessForm
    {
        public ModuleCalculationForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
        }

        private void ModuleCalculationForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
            TitleText.Font = AppConfig.SaoFont;
            select1.Font = label1.Font = groupBox1.Font = groupBox2.Font = groupBox3.Font = groupBox4.Font = AppConfig.ContentFont;
            button1.Font = AppConfig.ContentFont;
            AntdUI.Checkbox[] checkboxes =
            {
                chkStrengthBoost,
                chkAgilityBoost,
                chkIntelligenceBoost,
                chkSpecialAttackDamage,
                chkSpecialHealingBoost,
                chkExpertHealingBoost,
                chkCastingFocus,
                chkAttackSpeedFocus,
                chkCriticalFocus,
                chkLuckFocus,
                chkMagicResistance,
                chkPhysicalResistance,
            };
            foreach (var item in checkboxes)
            {
                item.Font = AppConfig.ContentFont;
            }
        }

        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageAnalyzer.PayloadBuffer.Length == 0)
            {
                var result = AppMessageBox.ShowMessage("""
                    请先过图一次在点此按钮
                    """, this);
                return;
            }

            BuildEliteCandidatePool.ParseModuleInfo(MessageAnalyzer.PayloadBuffer);


            virtualPanel1.Waterfall = false;      // 行优先（有 Align 的话设 Start）
            virtualPanel1.Items.Clear();

            // 取数据并先实例化所有卡（还不加到 panel）
            var cards = new List<ModuleCardDisplay.ResultCardSimpleItem>();
            foreach (var dto in ModuleCardDisplay.ModuleResultMemory.GetSnapshot())
            {
                cards.Add(new ModuleCardDisplay.ResultCardSimpleItem
                {
                    RankText = dto.RankText,
                    HighestLevel = dto.HighestLevel,
                    Score = dto.Score,        // 你现在是 string
                    ModuleRows = dto.ModuleRows,
                    AttrLines = dto.AttrLines
                });
            }

            // ========== 第一步：统一列宽 ==========
            float dpi = AntdUI.Config.Dpi;
            int gutterPx = (int)MathF.Round(12 * dpi);     // 列间距
            int minCardW = (int)MathF.Round(240 * dpi);    // 最小卡宽
            int maxCols = 3;                              // 最多列
            int panelW = Math.Max(1, virtualPanel1.ClientSize.Width);

            // 用与卡片一致的字体
            var baseFont = virtualPanel1.Font ?? SystemFonts.DefaultFont;
            using var fontBody = new Font(baseFont, baseFont.Style);
            using var fontTitle = new Font(baseFont, FontStyle.Bold);

            // 计算每张卡“期望宽度”：左右 Padding + 模组列表里“最长一行（左+gap+右）”
            int ComputePreferredCardWidth(ModuleCardDisplay.ResultCardSimpleItem card)
            {
                int padL = (int)MathF.Round(card.ContentPadding.Left * dpi);
                int padR = (int)MathF.Round(card.ContentPadding.Right * dpi);
                int gap = (int)MathF.Round(card.LeftRightGap * dpi);

                int maxRowContentW = 0;
                foreach (var row in card.ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var left = row.Left ?? string.Empty;
                    var right = $"[{row.Right ?? string.Empty}]";
                    int leftW = TextRenderer.MeasureText(left, fontBody).Width;
                    int rightW = TextRenderer.MeasureText(right, fontBody).Width;
                    maxRowContentW = Math.Max(maxRowContentW, leftW + gap + rightW);
                }
                int preferred = padL + maxRowContentW + padR;
                preferred = Math.Max(preferred, minCardW);
                preferred = Math.Min(preferred, panelW);
                return preferred;
            }

            int preferredMaxW = cards.Count == 0 ? minCardW : cards.Max(ComputePreferredCardWidth);

            // 算能排几列，并反推统一卡宽
            int cols = Math.Max(1, Math.Min(maxCols, (panelW + gutterPx) / (preferredMaxW + gutterPx)));
            int unifiedCardW = (panelW - gutterPx * (cols - 1)) / cols;
            unifiedCardW = Math.Max(1, Math.Min(unifiedCardW, panelW));

            // 把统一宽写到每张卡
            foreach (var c in cards) c.ForceCardWidthPx = unifiedCardW;

            // ========== 第二步：统一高度 ==========
            // 用统一宽度测每张卡高度，取最大值
            int MeasureCardHeight(ModuleCardDisplay.ResultCardSimpleItem card)
            {
                int padL = (int)MathF.Round(card.ContentPadding.Left * dpi);
                int padT = (int)MathF.Round(card.ContentPadding.Top * dpi);
                int padR = (int)MathF.Round(card.ContentPadding.Right * dpi);
                int padB = (int)MathF.Round(card.ContentPadding.Bottom * dpi);
                int gap = (int)MathF.Round(card.LeftRightGap * dpi);

                int contentW = Math.Max(1, unifiedCardW - padL - padR);
                int y = 0;

                // 标题
                y += TextRenderer.MeasureText(card.RankText ?? "", fontTitle,
                      new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + card.LineGap;

                // 最高属性等级
                y += TextRenderer.MeasureText($"最高属性等级 {card.HighestLevel}", fontBody,
                      new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + card.LineGap;

                // 综合评分（两段同一行，测一整段足够）
                string scoreText = string.IsNullOrEmpty(card.Score) ? "—" : card.Score;
                y += TextRenderer.MeasureText($"综合评分：{scoreText}", fontBody,
                      new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + card.SectionGap;

                // 列表标题
                y += TextRenderer.MeasureText("模组列表：", fontBody,
                      new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + card.LineGap;

                // 列表每行（左列宽 = contentW - 右列自然宽 - gap；右列自然宽不换行）
                foreach (var row in card.ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var left = row.Left ?? string.Empty;
                    var right = $"[{row.Right ?? string.Empty}]";
                    int rightW = TextRenderer.MeasureText(right, fontBody).Width;
                    int leftMaxW = Math.Max(1, contentW - rightW - gap);

                    int hL = TextRenderer.MeasureText(left, fontBody,
                             new Size(leftMaxW, int.MaxValue), TextFormatFlags.WordBreak).Height;
                    int hR = TextRenderer.MeasureText(right, fontBody).Height;

                    y += Math.Max(hL, hR) + 4;
                }

                y += card.SectionGap;

                // 属性分布标题
                y += TextRenderer.MeasureText("属性分布：", fontBody,
                      new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + card.LineGap;

                // 属性分布每行
                foreach (var (name, val) in card.AttrLines ?? Enumerable.Empty<(string Name, int Value)>())
                {
                    string line = $"{name}：+{val}";
                    y += TextRenderer.MeasureText(line, fontBody,
                         new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + 4;
                }

                int minH = (int)(card.MinHeightDp * dpi);
                return Math.Max(minH, y + padT + padB);
            }

            int unifiedCardH = cards.Count == 0 ? (int)(160 * dpi) : cards.Max(MeasureCardHeight);
            foreach (var c in cards) c.ForceCardHeightPx = unifiedCardH;

            // ========== 第三步：一次性加入并刷新 ==========
            virtualPanel1.SuspendLayout();
            foreach (var c in cards) virtualPanel1.Items.Add(c);
            virtualPanel1.ResumeLayout();
            virtualPanel1.Refresh();              // 一次刷新即可看到统一宽高效果


        }

        private void chkAttackSpeedFocus_CheckedChanged(object sender, BoolEventArgs e)
        {
            Checkbox checkbox = (Checkbox)sender;
            if (e.Value)
            {

                BuildEliteCandidatePool.Attributes.Add(checkbox.Text);
            }
            else
            {

                BuildEliteCandidatePool.Attributes.Remove(checkbox.Text);
            }
        }

        private void select1_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            BuildEliteCandidatePool.type = select1.SelectedValue.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("装饰用");
        }

        private void ModuleCalculationForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                groupBox1.ForeColor = groupBox2.ForeColor = groupBox3.ForeColor = groupBox4.ForeColor = Color.Wheat;
            }
            else
            {
                groupBox1.ForeColor = groupBox2.ForeColor = groupBox3.ForeColor = groupBox4.ForeColor = Color.Black;
            }
        }
    }
}
