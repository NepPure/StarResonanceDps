using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Module
{
    public class ModuleCardDisplay
    {
        public class ResultCardSimpleItem : AntdUI.VirtualShadowItem
        {
            // ====== 数据 ======
            public string RankText { get; set; } = "第一名搭配";
            public int HighestLevel { get; set; } = 5;
            public string Score { get; set; } = "";
            public int? ForceCardWidthPx { get; set; }  // 若有值，Size 用这个宽
            public int? ForceCardHeightPx { get; set; }  // 若有值，Size 用这个高

            // 每一行：左侧名称、右侧方括号属性文本
            public List<(string Left, string Right)> ModuleRows { get; set; } = new();

            // 属性分布：名称 -> 数值
            public List<(string Name, int Value)> AttrLines { get; set; } = new();

            // ====== 外观 ======
            public Padding ContentPadding { get; set; } = new Padding(16, 16, 16, 16);
            public int LineGap { get; set; } = 6;
            public int SectionGap { get; set; } = 10;
            public int MinHeightDp { get; set; } = 160;
            public int LeftRightGap { get; set; } = 8; // 左文本与右方括号之间的最小间距

            // 颜色（可替换为你的主题色）
            private static Color H(string hex) => ColorTranslator.FromHtml(hex);
            private static readonly Color Primary = H("#2563EB"); // 综合评分颜色
            private static readonly Color Muted = H("#64748B"); // 次级文字

            public override void Paint(AntdUI.Canvas g, AntdUI.VirtualPanelArgs e)
            {
                float dpi = AntdUI.Config.Dpi;
                int padL = (int)(ContentPadding.Left * dpi);
                int padT = (int)(ContentPadding.Top * dpi);
                int padR = (int)(ContentPadding.Right * dpi);
                int padB = (int)(ContentPadding.Bottom * dpi);

                // 卡片背景 + 边框
                using (var path = AntdUI.Helper.RoundPath(e.Rect, e.Radius))
                {
                    g.Fill(AntdUI.Style.Db.BgContainer, path);
                    var border = Hover ? AntdUI.Style.Db.BorderColorDisable : AntdUI.Style.Db.BorderColor;
                    g.Draw(border, 1 * dpi, path);
                }

                int x = e.Rect.X + padL;
                int y = e.Rect.Y + padT;
                int w = Math.Max(1, e.Rect.Width - padL - padR);

                var sfLT = AntdUI.Helper.SF(lr: StringAlignment.Near, tb: StringAlignment.Near);
                //var sfRT = AntdUI.Helper.SF(lr: StringAlignment.Far, tb: StringAlignment.Near);

                var fontTitle = new Font(e.Panel.Font, FontStyle.Bold);
                var fontBody = e.Panel.Font ?? SystemFonts.DefaultFont;   // ✅ 不要 Dispose

                // 标题（排名信息）
                var sz = AntdUI.Helper.Size(g.MeasureString(RankText ?? string.Empty, fontTitle, w));
                g.String(RankText ?? string.Empty, fontTitle, AntdUI.Style.Db.Text,
                         new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 最高属性等级
                string levelText = $"最高属性等级 {HighestLevel}";
                sz = AntdUI.Helper.Size(g.MeasureString(levelText, fontBody, w));
                g.String(levelText, fontBody, AntdUI.Style.Db.Text,
                         new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 综合评分（“综合评分：”为次级色，数值为主色）
                string scoreLabel = "综合评分：";
                int labelW = AntdUI.Helper.Size(g.MeasureString(scoreLabel, fontBody)).Width;
                sz = AntdUI.Helper.Size(g.MeasureString($"{scoreLabel}{Score:0.0}", fontBody, w));
                g.String(scoreLabel, fontBody, Muted, new Rectangle(x, y, w, sz.Height), sfLT);
                g.String($"{Score:0.0}", fontBody, Primary,
                         new Rectangle(x + labelW, y, Math.Max(1, w - labelW), sz.Height), sfLT);
                y += sz.Height + SectionGap;

                // 模组列表标题
                const string listTitle = "模组列表：";
                sz = AntdUI.Helper.Size(g.MeasureString(listTitle, fontBody, w));
                g.String(listTitle, fontBody, Muted, new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 模组列表：左侧名称；右侧方括号属性右对齐
                // 模组列表：左列 + 右列（都左对齐，用左列实际宽定位右列）
                foreach (var row in ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var leftText = row.Left ?? string.Empty;
                    var rightText = $"[{row.Right ?? string.Empty}]";

                    int gapPx = (int)MathF.Round(LeftRightGap * dpi);
                    int rightW = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Width;

                    // 先算“左列最大可用宽度”，保证两列同一行能放下
                    int leftMaxW = Math.Max(1, w - rightW - gapPx);

                    // 测左列在 leftMaxW 下的“实际绘制宽度”（可能小于 leftMaxW）
                    var leftSize = AntdUI.Helper.Size(g.MeasureString(leftText, fontBody, leftMaxW));
                    int leftActualW = Math.Min(leftSize.Width, leftMaxW);

                    // 行高取两列的最大高度
                    var rightSize = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody, rightW));
                    int rowH = Math.Max(leftSize.Height, rightSize.Height);

                    // 两个矩形：右列紧跟左列实际宽度之后开始
                    var leftRect = new Rectangle(x, y, leftMaxW, rowH);
                    var rightRect = new Rectangle(x + leftActualW + gapPx, y, rightW, rowH);

                    g.String(leftText, fontBody, AntdUI.Style.Db.Text, leftRect, sfLT);
                    g.String(rightText, fontBody, Muted, rightRect, sfLT);

                    y += rowH + 4;
                }



                y += SectionGap;

                // 属性分布标题
                const string distTitle = "属性分布：";
                sz = AntdUI.Helper.Size(g.MeasureString(distTitle, fontBody, w));
                g.String(distTitle, fontBody, Muted, new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 属性分布内容
                foreach (var kv in AttrLines ?? Enumerable.Empty<(string Name, int Value)>())
                {
                    string line = $"{kv.Name}：+{kv.Value}";
                    sz = AntdUI.Helper.Size(g.MeasureString(line, fontBody, w));
                    g.String(line, fontBody, AntdUI.Style.Db.Text, new Rectangle(x, y, w, sz.Height), sfLT);
                    y += sz.Height + 4;
                }
            }


            public override Size Size(AntdUI.Canvas g, AntdUI.VirtualPanelArgs e)
            {
                float dpi = AntdUI.Config.Dpi;
                int padL = (int)MathF.Round(ContentPadding.Left * dpi);
                int padT = (int)MathF.Round(ContentPadding.Top * dpi);
                int padR = (int)MathF.Round(ContentPadding.Right * dpi);
                int padB = (int)MathF.Round(ContentPadding.Bottom * dpi);
                int gap = (int)MathF.Round(LeftRightGap * dpi);

                // 用与 Paint 一致的字体测量（不要 Dispose e.Panel.Font）
                var baseFont = e.Panel?.Font ?? SystemFonts.DefaultFont;
                using var fontBody = new Font(baseFont, baseFont.Style);
                using var fontTitle = new Font(baseFont, FontStyle.Bold);

                // 1) 量出“模组列表”里每一行的总宽度（左 + gap + 右方括号）
                int maxRowContentW = 0;
                foreach (var row in ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var leftText = row.Left ?? string.Empty;
                    var rightText = $"[{row.Right ?? string.Empty}]";

                    int leftW = AntdUI.Helper.Size(g.MeasureString(leftText, fontBody)).Width;
                    int rightW = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Width;

                    maxRowContentW = Math.Max(maxRowContentW, leftW + gap + rightW);
                }

                // 2) 由“最长一行”推导卡片宽度（加上左右内边距），并做夹取
                // 计算出卡片“期望宽度”
                int minCardW = (int)(240 * dpi);
                int maxCardW = Math.Max(1, e.Rect.Width);
                int cardW = padL + maxRowContentW + padR;
                if (cardW < minCardW) cardW = minCardW;
                if (cardW > maxCardW) cardW = maxCardW;
                // ⬅️ 收集“期望宽度”与容器宽
                ModuleCardDisplay.UniformCardWidth.Collect(cardW, e.Rect.Width);

                // ⬅️ 若已冻结统一宽度，则改用统一宽度
                cardW = ModuleCardDisplay.UniformCardWidth.GetOr(cardW);

                // 用（统一后的）cardW 计算内容宽，再继续你的高度测量
                int contentW = Math.Max(1, cardW - padL - padR);
                int y = 0;

                // 标题
                y += AntdUI.Helper.Size(g.MeasureString(RankText ?? string.Empty, fontTitle, contentW)).Height + LineGap;
                // 最高属性等级
                y += AntdUI.Helper.Size(g.MeasureString($"最高属性等级 {HighestLevel}", fontBody, contentW)).Height + LineGap;
                // 综合评分
                y += AntdUI.Helper.Size(g.MeasureString($"综合评分：{Score:0.0}", fontBody, contentW)).Height + SectionGap;

                // 列表标题
                y += AntdUI.Helper.Size(g.MeasureString("模组列表：", fontBody, contentW)).Height + LineGap;

                // 列表每行高度（左列可能会被压缩到 contentW-rightW-gap 的宽度）
                // 列表每行高度（左列宽 = contentW - 右列自然宽 - gap）
                foreach (var row in ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var leftText = row.Left ?? string.Empty;
                    var rightText = $"[{row.Right ?? string.Empty}]";

                    // 右列自然宽度（不指定宽度＝不换行）
                    int rightW = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Width;
                    int leftMaxW = Math.Max(1, contentW - rightW - gap);

                    // 用各自的宽度测量高度
                    int hL = AntdUI.Helper.Size(g.MeasureString(leftText, fontBody, leftMaxW)).Height;
                    int hR = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Height;

                    y += Math.Max(hL, hR) + 4;
                }

                y += SectionGap;
                // 属性分布标题
                y += AntdUI.Helper.Size(g.MeasureString("属性分布：", fontBody, contentW)).Height + LineGap;
                // 属性分布每行
                foreach (var (name, val) in AttrLines ?? Enumerable.Empty<(string Name, int Value)>())
                    y += AntdUI.Helper.Size(g.MeasureString($"{name}：+{val}", fontBody, contentW)).Height + 4;

                int minH = (int)(MinHeightDp * dpi);
                int measuredH = Math.Max(minH, y + padT + padB);

                // ⚠️ 优先使用强制宽/高（直接一步到位）
                int finalW = ForceCardWidthPx ?? cardW;
                int finalH = ForceCardHeightPx ?? measuredH;

                return new Size(finalW, finalH);
            }

        }

        public class ResultCardData
        {
            public string RankText { get; set; } = "";
            public int HighestLevel { get; set; }
            public string Score { get; set; } = "";
            public List<(string Left, string Right)> ModuleRows { get; set; } = new();
            public List<(string Name, int Value)> AttrLines { get; set; } = new();

            public static ResultCardData FromCombination(ModuleCombination c, int rank)
            {
                var dto = new ResultCardData
                {
                    RankText = $"第{rank}名搭配",
                    HighestLevel = c.ThresholdLevel,
                    Score = c.Score.ToString(),
                    AttrLines = c.AttrBreakdown
                                   .OrderBy(k => k.Key, StringComparer.Ordinal)
                                   .Select(kv => (kv.Key, kv.Value)).ToList()
                };

                for (int i = 0; i < c.Modules.Count; i++)
                {
                    var m = c.Modules[i];
                    var right = string.Join(" ", (m.Parts ?? new List<ModulePart>())
                                              .Select(p => $"{p.Name}+{p.Value}"));
                    dto.ModuleRows.Add((m.Name ?? $"模组{i + 1}", right));
                }
                return dto;
            }
        }

        public static class ModuleResultMemory
        {
            private static readonly List<ResultCardData> _list = new();

            public static void Clear() => _list.Clear();

            public static void Set(IEnumerable<ResultCardData> items)
            {
                _list.Clear();
                if (items != null) _list.AddRange(items);
            }

            public static void Add(ResultCardData item) => _list.Add(item);

            public static List<ResultCardData> GetSnapshot() => new List<ResultCardData>(_list);

            // 方便：一次性从组合列表写入
            public static void FromCombinations(IEnumerable<ModuleCombination> combos)
                => Set(combos?.Select((c, i) => ResultCardData.FromCombination(c, i + 1)) ?? Enumerable.Empty<ResultCardData>());
        }

        public static class UniformCardHeight
        {
            private static readonly object _lock = new();
            private static int _maxH = 0;
            private static bool _frozen = false;

            public static void Reset()
            {
                lock (_lock)
                {
                    _maxH = 0;
                    _frozen = false;
                }
            }

            public static void Collect(int h)
            {
                lock (_lock)
                {
                    if (!_frozen && h > _maxH) _maxH = h;
                }
            }

            public static void Freeze()
            {
                lock (_lock) { _frozen = true; }
            }

            public static int GetOr(int measured)
            {
                lock (_lock) { return _frozen ? _maxH : measured; }
            }

            public static bool IsFrozen
            {
                get { lock (_lock) return _frozen; }
            }
        }
        public static class UniformCardWidth
        {
            private static readonly object _lock = new();
            private static int _maxMeasuredW = 0;   // 所有卡自测得到的“期望宽度”（像素）
            private static int _panelW = 0;         // 容器宽（像素）
            private static int _gutterPx = 12;      // 列间距（像素）
            private static int _minCardW = 240;     // 卡片最小宽（像素）
            private static int _maxCols = 3;       // 最多列数
            private static bool _frozen = false;
            private static int _frozenW = 0;

            public static void Reset(int gutterPx, int minCardW, int maxCols)
            {
                lock (_lock)
                {
                    _gutterPx = Math.Max(0, gutterPx);
                    _minCardW = Math.Max(1, minCardW);
                    _maxCols = Math.Max(1, maxCols);
                    _maxMeasuredW = 0;
                    _panelW = 0;
                    _frozen = false;
                    _frozenW = 0;
                }
            }

            public static void Collect(int preferredCardW, int panelW)
            {
                lock (_lock)
                {
                    if (preferredCardW > _maxMeasuredW) _maxMeasuredW = preferredCardW;
                    if (panelW > _panelW) _panelW = panelW;
                }
            }

            public static void Freeze()
            {
                lock (_lock)
                {
                    int W = Math.Max(1, _panelW);
                    int desired = Math.Max(_minCardW, _maxMeasuredW);

                    // 能排多少列（含间距）
                    int cols = (desired + _gutterPx) > 0
                        ? Math.Max(1, (W + _gutterPx) / (desired + _gutterPx))
                        : 1;

                    cols = Math.Min(cols, _maxCols);
                    cols = Math.Max(1, cols);

                    // 统一卡宽
                    int unified = (W - _gutterPx * (cols - 1)) / cols;
                    unified = Math.Min(unified, W);
                    unified = Math.Max(1, unified);

                    _frozenW = unified;
                    _frozen = true;
                }
            }

            public static int GetOr(int measured)
            {
                lock (_lock) return _frozen ? _frozenW : measured;
            }

            public static bool IsFrozen { get { lock (_lock) return _frozen; } }
        }

    }
}
