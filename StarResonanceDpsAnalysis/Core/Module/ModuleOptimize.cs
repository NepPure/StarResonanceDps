using AntdUI;
using StarResonanceDpsAnalysis.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using static StarResonanceDpsAnalysis.Core.Module.ModuleCardDisplay;
using static StarResonanceDpsAnalysis.Forms.ModuleForm.ModuleCalculationForm;

namespace StarResonanceDpsAnalysis.Core.Module
{
    /// <summary>
    /// 模组搭配组合
    /// </summary>
    public class ModuleCombination
    {
        public List<ModuleInfo> Modules { get; init; } = new();
        public int TotalAttrValue { get; init; }
        public Dictionary<string, int> AttrBreakdown { get; init; } = new();
        public int ThresholdLevel { get; init; }
        public double Score { get; init; }

        // === 新增：链接效果评分总分 + 逐词条贡献（便于“准确展示链接效果评分”）
        public double LinkEffectScore { get; init; } = 0.0;
        public Dictionary<string, double> LinkEffectContrib { get; init; } = new(StringComparer.Ordinal);
    }

    public class ModuleSolution
    {
        public List<ModuleInfo> Modules { get; set; } = new();
        public double Score { get; set; }
        public Dictionary<string, int> AttrBreakdown { get; set; } = new();
    }

    /// <summary>
    /// 模组搭配优化器
    /// 使用 ModuleMaps.MODULE_CATEGORY_MAP / ATTR_THRESHOLDS / MODULE_CATEGORY_NAMES
    /// </summary>
    public class ModuleOptimizer
    {
        // ===== 可调参数 =====
        private readonly int LocalSearchIterations = 30;   // 局部搜索迭代次数
        private readonly int MaxSolutions = 40;            // 希望收集的候选解上限

        // 链接效果评分需要的等级权重（1..6级）
        private readonly Dictionary<int, double> LevelWeights = new()
        {
            { 1, 1.0 }, { 2, 4.0 }, { 3, 8.0 }, { 4,12.0 }, { 5, 16.0 }, { 6, 20.0 }
        };

        // 计算 breakdown 的“等级序列（强→弱）”
        private static List<int> GetLevelsDescFromBreakdown(Dictionary<string, int> breakdown)
        {
            return GetLevelsDesc(breakdown, ModuleMaps.ATTR_THRESHOLDS);
        }

        // 新增：聚焦属性分数（只看用户选中的 Attributes；没选则返回 -1 表示无效）
        private static long ComputeFocusScore(Dictionary<string, int> breakdown)
        {
            if (BuildEliteCandidatePool.Attributes == null || BuildEliteCandidatePool.Attributes.Count == 0)
                return -1;

            long s = 0;
            foreach (var name in BuildEliteCandidatePool.Attributes)
                if (!string.IsNullOrEmpty(name) && breakdown.TryGetValue(name, out var v))
                    s += Math.Min(v, 20); // cap 20，和你的 Score 口径一致
            return s;
        }


        // 计算形态桶（四位优先：6666/6665/6655/6555；三位兜底：666/665/655/654/644）
        private static int ComputePatternBucket(Dictionary<string, int> breakdown)
        {
            var levelsDesc = GetLevelsDescFromBreakdown(breakdown);
            var (bucket, _) = GetPatternBucket(levelsDesc);
            return bucket; // 越大越好；0 = 不命中任何形态
        }

        // 阈值化总和（用于最后再细分的兜底）
        private static long ComputeThresholdSum(Dictionary<string, int> breakdown)
        {
            long thresholdSum = 0;
            foreach (var v in breakdown.Values)
            {
                var (_, snapped) = GetLevelAndSnapped(v, ModuleMaps.ATTR_THRESHOLDS);
                thresholdSum += snapped;
            }
            return thresholdSum;
        }

        // 形态优先的比较：返回 true 表示 a 比 b 更好
        // 计算等级（0..6）
        private static int ToLevel(int value)
        {
            var t = ModuleMaps.ATTR_THRESHOLDS; // {1,4,8,12,16,20}
            int lv = 0;
            for (int i = 0; i < t.Length; i++) if (value >= t[i]) lv = i + 1; else break;
            return lv;
        }

        // 聚焦属性统计：minLevel(最短板)、sumLevels、sumCapped(Σmin(v,20))
        private static (bool hasFocus, int minLevel, int sumLevels, long sumCapped) ComputeFocusStats(Dictionary<string, int> breakdown)
        {
            var attrs = BuildEliteCandidatePool.Attributes;
            if (attrs == null || attrs.Count == 0) return (false, -1, -1, -1);

            int minLv = int.MaxValue, sumLv = 0, cnt = 0;
            long sumCap = 0;
            foreach (var name in attrs)
            {
                if (string.IsNullOrEmpty(name)) continue;
                breakdown.TryGetValue(name, out var v);
                int lv = ToLevel(v);
                minLv = Math.Min(minLv, lv);
                sumLv += lv;
                sumCap += Math.Min(v, 20);
                cnt++;
            }
            if (cnt == 0) return (false, -1, -1, -1);
            if (minLv == int.MaxValue) minLv = 0;
            return (true, minLv, sumLv, sumCap);
        }

        private static bool IsBetterPatternFirst(ModuleSolution a, ModuleSolution b)
        {
            // === 0) 若用户选了聚焦属性：自适应比较 ===
            var (hasFocusA, minA, sumLvA, sumCapA) = ComputeFocusStats(a.AttrBreakdown);
            var (hasFocusB, minB, sumLvB, sumCapB) = ComputeFocusStats(b.AttrBreakdown);
            bool hasFocus = hasFocusA && hasFocusB;

            if (hasFocus)
            {
                int k = BuildEliteCandidatePool.Attributes?.Count ?? 0;

                if (k >= 2)
                {
                    // 多选：先比“最短板等级”→再比“等级总和”
                    if (minA != minB) return minA > minB;
                    if (sumLvA != sumLvB) return sumLvA > sumLvB;
                    // 再进入形态/评分等
                }
                else // k == 1
                {
                    // 单选：先比该属性的 Σmin(v,20)
                    if (sumCapA != sumCapB) return sumCapA > sumCapB;
                    // 再进入形态/评分等
                }
            }

            // === 1) 形态桶
            int pa = ComputePatternBucket(a.AttrBreakdown);
            int pb = ComputePatternBucket(b.AttrBreakdown);
            if (pa != pb) return pa > pb;

            // === 2) 总评分（Σmin(value,20)）
            if (Math.Abs(a.Score - b.Score) > 1e-9) return a.Score > b.Score;

            // === 3) 件数
            if (a.Modules.Count != b.Modules.Count) return a.Modules.Count > b.Modules.Count;

            // === 4) 阈值化总和
            long ta = ComputeThresholdSum(a.AttrBreakdown);
            long tb = ComputeThresholdSum(b.AttrBreakdown);
            if (ta != tb) return ta > tb;

            // === 5) 原始总和
            long ra = a.AttrBreakdown.Values.Sum();
            long rb = b.AttrBreakdown.Values.Sum();
            return ra > rb;
        }



        // 等级/就近阈值（level: 0..6；snapped: 就近阈值 0/1/4/8/12/16/20）
        private static (int level, int snapped) GetLevelAndSnapped(int value, int[] thresholds)
        {
            int snapped = 0, level = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (value >= thresholds[i]) { snapped = thresholds[i]; level = i + 1; }
                else break;
            }
            return (level, snapped);
        }

        // 评分=属性分布原始值总和（不看阈值、不看形态）
        // 评分 = Σ min(value, 20) —— 超过 20 的部分不计分（视为浪费）
        public double CalculateCombinationScoreRawSum(Dictionary<string, int> attrBreakdown)
        {
            long sum = 0;
            foreach (var v in attrBreakdown.Values)
            {
                sum += Math.Min(v, 20);
            }
            return sum; // 用 double 承接
        }



        // 把各词条等级取出并按“降序”（强→弱）排序（用于挑 Top4）
        private static List<int> GetLevelsDesc(Dictionary<string, int> breakdown, int[] thresholds)
        {
            var list = new List<int>();
            foreach (var v in breakdown.Values)
            {
                var (lv, _) = GetLevelAndSnapped(v, thresholds);
                if (lv > 0) list.Add(lv);
            }
            list.Sort((a, b) => b.CompareTo(a)); // 降序
            return list;
        }

        // 计算：在 currentBreakdown 基础上加入 candidate 后的贪心评价：
        // 返回 composite（用于排序越大越好），以及三个组成分量（仅调试用）
        // 在 ModuleOptimizer 内，替换原来的 EvalGreedyCandidate 为下面这一版
        private static (long composite, long deltaExisting, long deltaNew, long totalAfter)
        EvalGreedyCandidate(Dictionary<string, int> currentBreakdown, ModuleInfo candidate)
        {
            // ---- 1) 试算加入 candidate 后的属性分布 ----
            var after = new Dictionary<string, int>(currentBreakdown, StringComparer.Ordinal);
            if (candidate?.Parts != null)
            {
                foreach (var p in candidate.Parts)
                {
                    if (string.IsNullOrEmpty(p?.Name)) continue;
                    if (!after.TryGetValue(p.Name, out var cur)) cur = 0;
                    after[p.Name] = cur + p.Value;
                }
            }

            // ---- 2) 原有分层分数（保持你的语义与数量级关系）----
            long totalBefore = 0, totalAfter = 0;
            foreach (var kv in currentBreakdown)
                totalBefore += Math.Min(kv.Value, 20);
            foreach (var kv in after)
                totalAfter += Math.Min(kv.Value, 20);

            long deltaTotal = totalAfter - totalBefore;

            long deltaExisting = 0;
            foreach (var kv in currentBreakdown)
            {
                int before = Math.Min(kv.Value, 20);
                int aft = Math.Min(after[kv.Key], 20);
                if (aft > before) deltaExisting += (aft - before);
            }

            long deltaNew = deltaTotal - deltaExisting;

            // ---- 3) 新增：模式桶 + “第三属性提升” 强引导 ----
            var thresholds = ModuleMaps.ATTR_THRESHOLDS; // {1,4,8,12,16,20}

            // 等级序列（强->弱）
            var beforeLevels = GetLevelsDesc(currentBreakdown, thresholds);
            var afterLevels = GetLevelsDesc(after, thresholds);

            // 模式桶（四位优先，三位兜底）
            var (patternBucketAfter, _) = GetPatternBucket(afterLevels);

            // “第三属性提升量”：比较加入前后的第3高等级（0..6）
            int beforeThird = beforeLevels.Count >= 3 ? beforeLevels[2] : 0;
            int afterThird = afterLevels.Count >= 3 ? afterLevels[2] : 0;
            long thirdGain = Math.Max(0, afterThird - beforeThird);  // 只奖励正提升

            // 可选：轻微惩罚浪费（>20 的部分），避免无意义堆叠
            long waste = 0;
            foreach (var kv in after)
                if (kv.Value > 20) waste += (kv.Value - 20);

            // 聚焦属性增量（只在有 Attributes 时生效）
            // === 聚焦属性增量（自适应）：多选→最短板等级提升；单选→该条 Σmin(v,20) 提升 ===
            long focusGain = 0;
            if (BuildEliteCandidatePool.Attributes != null && BuildEliteCandidatePool.Attributes.Count > 0)
            {
                int k = BuildEliteCandidatePool.Attributes.Count;

                if (k >= 2)
                {
                    // 计算 before/after 的“最短板等级”
                    int minBefore = int.MaxValue, minAfter = int.MaxValue;
                    foreach (var name in BuildEliteCandidatePool.Attributes)
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        currentBreakdown.TryGetValue(name, out var vb);
                        after.TryGetValue(name, out var va);
                        minBefore = Math.Min(minBefore, ToLevel(vb));
                        minAfter = Math.Min(minAfter, ToLevel(va));
                    }
                    if (minBefore == int.MaxValue) minBefore = 0;
                    if (minAfter == int.MaxValue) minAfter = 0;

                    // 最短板等级的提升优先
                    focusGain = Math.Max(0, minAfter - minBefore);

                    // 也可以微量奖励“聚焦等级总和”的提升（避免平分极端）
                    int sumLvBefore = 0, sumLvAfter = 0;
                    foreach (var name in BuildEliteCandidatePool.Attributes)
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        currentBreakdown.TryGetValue(name, out var vb);
                        after.TryGetValue(name, out var va);
                        sumLvBefore += ToLevel(vb);
                        sumLvAfter += ToLevel(va);
                    }
                    int sumLvGain = Math.Max(0, sumLvAfter - sumLvBefore);
                    focusGain = focusGain * 10L + sumLvGain; // 最短板提升权重大
                }
                else // k == 1
                {
                    long capBefore = 0, capAfter = 0;
                    foreach (var name in BuildEliteCandidatePool.Attributes)
                    {
                        currentBreakdown.TryGetValue(name, out var vb);
                        after.TryGetValue(name, out var va);
                        capBefore += Math.Min(vb, 20);
                        capAfter += Math.Min(va, 20);
                    }
                    focusGain = Math.Max(0, capAfter - capBefore);
                }
            }

            // ——权重：低于 WPATTERN/WTHIRD，避免压过形态与第三属性驱动
            const long WFOCUS = 20_000_000_000L; // 比 WTHIRD(1000亿)小很多
            // ---- 4) 组合分数：把“模式桶/第三属性提升”提到更高权级 ----
            const long WPATTERN = 1_000_000_000_000L; // 最高：强引导形态
            const long WTHIRD = 100_000_000_000L; // 次高：强引导第三属性抬升
            const long WEXISTING = 1_000_000_000L; // 之后：你的原有优先项
            const long WNEW = 1_000_000L;
            const long WTOTAL = 1_000L;
            const long WASTE_PEN = 1_000L; // 轻罚浪费

            long composite =
               patternBucketAfter * WPATTERN
             + thirdGain * WTHIRD
             + focusGain * WFOCUS      // ← 自适应聚焦奖励（降权）
             + deltaExisting * WEXISTING
             + deltaNew * WNEW
             + totalAfter * WTOTAL
             - waste * WASTE_PEN;

            return (composite, deltaExisting, deltaNew, totalAfter);
        }



        // 同时支持“四位模式优先”和“三位模式兜底”的模式桶：返回 (patternBucket, arity)
        // bucket 越大越好；四位命中：4/3/2/1；三位命中：5/4/3/2/1；都不命中：0
        // 同时支持“四位模式优先”和“三位模式兜底”的模式桶：返回 (bucket, arity)
        // bucket 越大越好；四位命中：4/3/2/1；三位命中：5/4/3/2/1；都不命中：0
        private static (int bucket, int arity) GetPatternBucket(List<int> levelsDesc)
        {
            // ---- 先尝试四位模式：6666, 6665, 6655, 6555 ----
            var top4 = levelsDesc.Take(4).ToList();
            while (top4.Count < 4) top4.Add(0);

            int[][] targets4 = new[]
            {
        new[]{6,6,6,6},
        new[]{6,6,6,5},
        new[]{6,6,5,5},
        new[]{6,5,5,5}
    };

            for (int i = 0; i < targets4.Length; i++)
            {
                bool same = true;
                for (int k = 0; k < 4; k++)
                {
                    if (top4[k] != targets4[i][k]) { same = false; break; }
                }
                if (same)
                {
                    // 四位命中：bucket = 4 - i（4,3,2,1）
                    return (targets4.Length - i, 4);
                }
            }

            // ---- 再尝试三位模式：666, 665, 655, 654, 644 ----
            var top3 = levelsDesc.Take(3).ToList();
            while (top3.Count < 3) top3.Add(0);

            int[][] targets3 = new[]
            {
        new[]{6,6,6},
        new[]{6,6,5},
        new[]{6,5,5},
        new[]{6,5,4},
        new[]{6,4,4}
    };

            for (int i = 0; i < targets3.Length; i++)
            {
                bool same = true;
                for (int k = 0; k < 3; k++)
                {
                    if (top3[k] != targets3[i][k]) { same = false; break; }
                }
                if (same)
                {
                    // 三位命中：bucket = 5 - i（5,4,3,2,1）
                    return (targets3.Length - i, 3);
                }
            }

            // 都不命中
            return (0, 0);
        }



        // V4b：主序=目标模式；次序=模组个数；三序=阈值总和；四序=等级字典序；五序=原始总值（全部为正）
        // V4b：主序=目标模式（四位优先，三位兜底）；次序=模组个数；三序=阈值总和；四序=等级字典序；五序=原始总值（全为正）
        // V4b：主序=目标模式（四位优先，三位兜底）；次序=模组个数；三序=阈值总和；四序=等级字典序；五序=原始总值（全为正）
        public double CalculateCombinationScoreV4(Dictionary<string, int> attrBreakdown, int modulesCount)
        {
            var thresholds = ModuleMaps.ATTR_THRESHOLDS; // {1,4,8,12,16,20}

            long thresholdSum = 0, rawSum = 0;
            foreach (var v in attrBreakdown.Values)
            {
                rawSum += v;
                var (_, snapped) = GetLevelAndSnapped(v, thresholds);
                thresholdSum += snapped;
            }

            // 等级序列（强→弱），再生成模式桶（四位优先，三位兜底）
            var levelsDesc = GetLevelsDesc(attrBreakdown, thresholds);
            var (patternBucket, patternArity) = GetPatternBucket(levelsDesc);
            // patternBucket：四位命中=4/3/2/1；三位命中=5..1；未命中=0（越大越好）

            // 升序等级向量（弱→强）字典序编码，用于细分
            var levelsAsc = levelsDesc.ToList();
            levelsAsc.Reverse();
            const long BASE = 10;
            long levelLexAsc = 0, mul = 1;
            foreach (var lv in levelsAsc)
            {
                levelLexAsc += lv * mul;
                mul *= BASE;
            }

            // 分层权重（数量级隔离，全部为正）
            const long W1 = 1_000_000_000_000L; // patternBucket（决定性）
            const long W2 = 1_000_000_000L;     // modulesCount（以后放开 3~4 件时才会拉开差距）
            const long W3 = 1_000_000L;         // thresholdSum
            const long W4 = 1_000L;             // levelLexAsc
            const long W5 = 1L;                 // rawSum

            long composed =
                  (long)patternBucket * W1
                + (long)Math.Max(0, modulesCount) * W2
                + (long)Math.Max(0, thresholdSum) * W3
                + (long)Math.Max(0, levelLexAsc) * W4
                + (long)Math.Max(0, rawSum) * W5;

            return (double)composed; // 非负、越好越大
        }


        /// <summary>按 config_id 获取分类，未命中默认 ATTACK</summary>
        public ModuleCategory GetModuleCategory(ModuleInfo module)
        {
            return ModuleMaps.MODULE_CATEGORY_MAP.TryGetValue(module.ConfigId, out var cat)
                ? cat
                : ModuleCategory.ATTACK;
        }

        /// <summary>
        /// 计算解的详细信息
        /// </summary>
        public ModuleSolution CalculateSolutionScore(List<ModuleInfo> modules)
        {
            var breakdown = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var m in modules)
            {
                foreach (var p in m.Parts)
                {
                    if (breakdown.ContainsKey(p.Name))
                        breakdown[p.Name] += p.Value;
                    else
                        breakdown[p.Name] = p.Value;
                }
            }

            double score = CalculateCombinationScoreRawSum(breakdown);

            return new ModuleSolution
            {
                Modules = modules.ToList(),
                Score = score,
                AttrBreakdown = breakdown
            };
        }

        // === 3.1 约束：单个模组是否允许 ===
        private static bool CheckModuleAllowed(ModuleInfo m)
        {
            if (m?.Parts == null) return false;

            // 词条数量限制
            int partCount = m.Parts.Count;
            if (BuildEliteCandidatePool.ExactPartCount.HasValue &&
                partCount != BuildEliteCandidatePool.ExactPartCount.Value)
                return false;
            if (BuildEliteCandidatePool.MinPartCount.HasValue &&
                partCount < BuildEliteCandidatePool.MinPartCount.Value)
                return false;
            if (BuildEliteCandidatePool.MaxPartCount.HasValue &&
                partCount > BuildEliteCandidatePool.MaxPartCount.Value)
                return false;

            // 排除词条
            if (BuildEliteCandidatePool.ExcludedAttributes.Count > 0)
            {
                foreach (var p in m.Parts)
                {
                    if (!string.IsNullOrEmpty(p?.Name) &&
                        BuildEliteCandidatePool.ExcludedAttributes.Contains(p.Name))
                        return false;
                }
            }

            // 白名单逻辑保留到 ParseModuleInfo 里做
            return true;
        }

        // 业务参数（可挪到 BuildEliteCandidatePool 做可配置）
        private const int MinHighAttrs = 3;
        private const int MinHighLevelThreshold = 4; // 对应 {1,4,8,12,16,20} 里的 4

        private static bool MeetsGlobalRequirements(List<ModuleInfo> modules)
        {
            // 先保留你已有的 RequiredAttributes 逻辑
            if (BuildEliteCandidatePool.RequiredAttributeCount > 0 &&
                BuildEliteCandidatePool.RequiredAttributes.Count > 0)
            {
                var appear = new HashSet<string>(StringComparer.Ordinal);
                foreach (var m in modules)
                    foreach (var p in m.Parts)
                        if (!string.IsNullOrEmpty(p?.Name)) appear.Add(p.Name);

                int hit = 0;
                foreach (var req in BuildEliteCandidatePool.RequiredAttributes)
                    if (appear.Contains(req)) hit++;

                if (hit < BuildEliteCandidatePool.RequiredAttributeCount) return false;
            }

            // === 新增硬约束：至少 3 个属性达到阈值 4（可调） ===
            var breakdown = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var m in modules)
                foreach (var p in m.Parts)
                {
                    if (string.IsNullOrEmpty(p?.Name)) continue;
                    breakdown[p.Name] = (breakdown.TryGetValue(p.Name, out var cur) ? cur : 0) + p.Value;
                }

            int cnt = 0;
            foreach (var v in breakdown.Values)
                if (v >= MinHighLevelThreshold) cnt++;

            return cnt >= MinHighAttrs;
        }


        // === 3.5 链接效果评分拆解：返回(总分, 每个词条的贡献)
        public (double total, Dictionary<string, double> perAttr) CalculateLinkEffect(Dictionary<string, int> attrBreakdown)
        {
            double total = 0.0;
            var per = new Dictionary<string, double>(StringComparer.Ordinal);
            var thresholds = ModuleMaps.ATTR_THRESHOLDS;

            foreach (var kv in attrBreakdown)
            {
                string name = kv.Key;
                int value = kv.Value;

                int maxThreshold = 0;
                int level = 0;
                for (int i = 0; i < thresholds.Length; i++)
                {
                    if (value >= thresholds[i]) { maxThreshold = thresholds[i]; level = i + 1; }
                    else break;
                }

                double w = 0.0;
                if (level > 0 && LevelWeights.TryGetValue(level, out var ww)) w = ww;

                int waste = value - maxThreshold;
                double contrib = 0.9 * w + 0.05 * value - 0.05 * waste;

                per[name] = contrib;
                total += contrib;
            }

            return (total, per);
        }

        // === 3.3 预筛：过滤掉不符合单模组约束的模组，并做每属性Top30候选池
        private const int MAX_POOL = 80;

        private List<ModuleInfo> PrefilterModules_Strict(List<ModuleInfo> modules)
        {
            var filteredBySingle = modules.Where(CheckModuleAllowed).ToList();
            var byAttr = new Dictionary<string, List<(ModuleInfo m, int v)>>(StringComparer.Ordinal);

            foreach (var m in filteredBySingle)
            {
                foreach (var p in m.Parts)
                {
                    if (string.IsNullOrEmpty(p?.Name)) continue;
                    if (!byAttr.TryGetValue(p.Name, out var list)) byAttr[p.Name] = list = new();
                    list.Add((m, p.Value));
                }
            }

            // 每属性 Top12
            var set = new Dictionary<string, ModuleInfo>(StringComparer.Ordinal);
            foreach (var kv in byAttr)
                foreach (var (m, _) in kv.Value.OrderByDescending(x => x.v).Take(12))
                    set[m.Uuid.ToString()] = m;

            // 聚焦属性注入 Top10
            if (BuildEliteCandidatePool.Attributes is { Count: > 0 })
            {
                const int INJECT_TOP_PER_ATTR = 10;
                foreach (var a in BuildEliteCandidatePool.Attributes)
                    if (byAttr.TryGetValue(a, out var list))
                        foreach (var (m, _) in list.OrderByDescending(x => x.v).Take(INJECT_TOP_PER_ATTR))
                            set[m.Uuid.ToString()] = m;
            }

            // 若仍然过少，用总值补齐到 30
            while (set.Count < 30)
            {
                foreach (var m in filteredBySingle.OrderByDescending(SumParts))
                {
                    var k = m.Uuid.ToString();
                    if (!set.ContainsKey(k)) set[k] = m;
                    if (set.Count >= 30) break;
                }
                break;
            }

            var pool = set.Values.ToList();

            // 过多则裁剪到 MAX_POOL：按“单件潜力”降序
            if (pool.Count > MAX_POOL)
            {
                // 基线空分布
                var baseline = new Dictionary<string, int>(StringComparer.Ordinal);
                var scored = pool
                    .Select(m => (m, score: EvalGreedyCandidate(baseline, m).composite))
                    .OrderByDescending(t => t.score)
                    .Take(MAX_POOL)
                    .Select(t => t.m)
                    .ToList();
                pool = scored;
            }

            return pool;
        }


        /// <summary>
        /// 计算“阈值化总值”与“原始属性分布”
        /// 阈值化总值：每个属性按阈值就近向下取整累加（如 17→16）
        /// 返回：(阈值化总值, 属性分布)
        /// </summary>
        public (int totalThresholdValue, Dictionary<string, int> attrBreakdown)
            CalculateTotalAttrValue(IReadOnlyList<ModuleInfo> modules)
        {
            var attrBreakdown = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var module in modules)
            {
                if (module?.Parts == null) continue;
                foreach (var part in module.Parts)
                {
                    if (string.IsNullOrEmpty(part?.Name)) continue;
                    if (!attrBreakdown.TryGetValue(part.Name, out var cur))
                        cur = 0;
                    attrBreakdown[part.Name] = cur + part.Value;
                }
            }

            int totalThresholdValue = 0;
            foreach (var kv in attrBreakdown)
            {
                int val = kv.Value;
                int chosen = 0;
                foreach (var t in ModuleMaps.ATTR_THRESHOLDS)
                {
                    if (val >= t) chosen = t;
                    else break;
                }
                totalThresholdValue += chosen;
            }

            return (totalThresholdValue, attrBreakdown);
        }

        public static int GetHighestThresholdLevel(Dictionary<string, int> attrBreakdown)
        {
            if (attrBreakdown == null || attrBreakdown.Count == 0) return 0;

            int highestLevel = 0; // 0 表示未达任何阈值；1..6 对应 {1,4,8,12,16,20}
            var thresholds = ModuleMaps.ATTR_THRESHOLDS;

            foreach (var value in attrBreakdown.Values)
            {
                int level = 0;
                for (int t = 0; t < thresholds.Length; t++)
                {
                    if (value >= thresholds[t]) level = t + 1; // 等级 = 索引+1
                    else break;
                }
                if (level > highestLevel) highestLevel = level;
            }

            return highestLevel; // “>=20”返回 6
        }

        // === 3.4 在“构造 + 局部搜索 + 入库去重”阶段，确保组合满足全局约束 ===
        public ModuleSolution GreedyConstruct(List<ModuleInfo> pool)
        {
            // 尝试 4 件，否则退到 3 件
            for (int targetSize = 4; targetSize >= 3; targetSize--)
            {
                if (pool.Count < targetSize) continue;

                var allowedPool = pool.Where(CheckModuleAllowed).ToList();
                if (allowedPool.Count < targetSize) continue;

                var rnd = new Random();
                var current = new List<ModuleInfo> { allowedPool[rnd.Next(allowedPool.Count)] };

                // 贪心补齐到 targetSize
                // 贪心补齐到 targetSize
                while (current.Count < targetSize)
                {
                    var candidates = new List<ModuleInfo>();
                    var scores = new List<long>();
                    var currentBreakdown = new Dictionary<string, int>(StringComparer.Ordinal);

                    // 先把 current 的分布算出来
                    foreach (var m in current)
                    {
                        foreach (var p in m.Parts)
                        {
                            if (string.IsNullOrEmpty(p?.Name)) continue;
                            if (!currentBreakdown.TryGetValue(p.Name, out var cur)) cur = 0;
                            currentBreakdown[p.Name] = cur + p.Value;
                        }
                    }

                    foreach (var m in allowedPool)
                    {
                        if (current.Contains(m)) continue;

                        // ★ 使用“优先补强已有属性”的评价函数
                        var (comp, _, _, _) = EvalGreedyCandidate(currentBreakdown, m);
                        candidates.Add(m);
                        scores.Add(comp);
                    }

                    if (candidates.Count == 0) break;

                    ModuleInfo choice;
                    if (rnd.NextDouble() < 0.8)
                    {
                        var maxIdx = scores.IndexOf(scores.Max());
                        choice = candidates[maxIdx];
                    }
                    else
                    {
                        var topTriples = scores
                            .Select((s, i) => (s, i))
                            .OrderByDescending(x => x.s)
                            .Take(3)
                            .Select(x => candidates[x.i])
                            .ToList();
                        choice = topTriples[rnd.Next(topTriples.Count)];
                    }

                    current.Add(choice);
                }


                if (current.Count == targetSize && MeetsGlobalRequirements(current))
                {
                    return CalculateSolutionScore(current);
                }
            }

            return null; // 3 也不行
        }


        /// <summary>
        /// 局部搜索：每轮随机抽 20 个候选尝试替换，若一轮后半段仍无改进则提前结束
        /// </summary>
        public ModuleSolution LocalSearch(ModuleSolution solution, List<ModuleInfo> pool, int iterations = 30)
        {
            var rnd = new Random();
            var allowedPool = pool.Where(CheckModuleAllowed).ToList();
            var best = solution;

            int iter = Math.Max(1, iterations);
            for (int it = 0; it < iter; it++)
            {
                bool improved = false;

                for (int i = 0; i < best.Modules.Count; i++)
                {
                    var candidates = RandomSample(allowedPool, Math.Min(20, allowedPool.Count), rnd);
                    foreach (var newMod in candidates)
                    {
                        if (best.Modules.Contains(newMod)) continue;

                        var newList = new List<ModuleInfo>(best.Modules);
                        newList[i] = newMod;

                        if (!MeetsGlobalRequirements(newList)) continue;

                        var newSol = CalculateSolutionScore(newList);
                        if (IsBetterPatternFirst(newSol, best))
                        {
                            best = newSol;
                            improved = true;
                            break;
                        }

                    }
                    if (improved) break;
                }

                if (!improved && it > iter / 2) break;
            }

            return best;
        }

        // === 穷举版：在预筛候选池上对 4件 & 3件 组合全枚举，保留TopN ===
        public List<ModuleSolution> FindOptimalCombinations_Exhaustive(
           List<ModuleInfo> modules, ModuleCategory category, int topN = 40)
        {
            // 1) 分类过滤
            List<ModuleInfo> filtered = category switch
            {
                ModuleCategory.ALL => modules.ToList(),
                _ => modules.Where(m => GetModuleCategory(m) == category).ToList()
            };
            if (filtered.Count < 3) return new();

            // 2) 强预筛
            var pool = PrefilterModules_Strict(filtered);
            int n = pool.Count;
            if (n < 3) return new();

            // 3) 预计算：每个模组的 Parts 聚合到字典拷贝成本太高，压成 List<(name, value)>
            var attrNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var m in pool) foreach (var p in m.Parts) if (!string.IsNullOrEmpty(p?.Name)) attrNames.Add(p.Name);
            var names = attrNames.ToList();
            var idxByName = names.Select((s, i) => (s, i)).ToDictionary(t => t.s, t => t.i, StringComparer.Ordinal);

            int A = names.Count;
            var modAttr = new int[n, A];
            for (int i = 0; i < n; i++)
                foreach (var p in pool[i].Parts)
                    if (!string.IsNullOrEmpty(p?.Name))
                        modAttr[i, idxByName[p.Name]] += p.Value;

            // 4) 为上界估计准备：每个属性对所有模组的贡献降序
            var perAttrSorted = new List<(int idx, int val)>[A];
            for (int a = 0; a < A; a++)
            {
                var list = new List<(int idx, int val)>();   // ← 这里改成命名元组
                for (int i = 0; i < n; i++)
                    if (modAttr[i, a] > 0)
                        list.Add((idx: i, val: modAttr[i, a])); // ← 带名字更清晰

                perAttrSorted[a] = list
                    .OrderByDescending(x => x.val) // ok
                    .ToList();
            }


            // 5) 维护 TopK（K=topN）的小根堆
            var best = new List<ModuleSolution>();
            double kthScore = double.NegativeInfinity;

            void TryPush(List<int> pickIdx)
            {
                // 计算 solution
                var modulesPicked = pickIdx.Select(i => pool[i]).ToList();
                if (!MeetsGlobalRequirements(modulesPicked)) return;

                var sol = CalculateSolutionScore(modulesPicked);

                if (best.Count < topN)
                {
                    best.Add(sol);
                    best.Sort((x, y) => IsBetterPatternFirst(x, y) ? -1 : (IsBetterPatternFirst(y, x) ? 1 : 0));
                    kthScore = best.Count == topN ? best[^1].Score : best[^1].Score;
                }
                else
                {
                    var worst = best[^1];
                    if (IsBetterPatternFirst(sol, worst))
                    {
                        best[^1] = sol;
                        best.Sort((x, y) => IsBetterPatternFirst(x, y) ? -1 : (IsBetterPatternFirst(y, x) ? 1 : 0));
                        kthScore = best[^1].Score;
                    }
                }
            }

            // 估算 Σmin(v,20) 的乐观上界（只用作剪枝，不改变排序语义）
            double UpperBoundScore(int[] cur, int start, int remaining)
            {
                // 对每个属性，尝试用 perAttrSorted 的前若干条（跳过已选索引）把 cur[a] 往上推
                // 简化：不排除已选索引带来的微小误差，上界只要“>=”即可——因此我们保守地取前 remaining 件的总和
                long s = 0;
                for (int a = 0; a < A; a++)
                {
                    int v = cur[a];
                    int add = 0; int took = 0;
                    var list = perAttrSorted[a];
                    for (int t = 0; t < list.Count && took < remaining; t++)
                    {
                        // 粗上界：不排除已选，可能略高；用于剪枝OK
                        add += list[t].val;
                        took++;
                    }
                    s += Math.Min(v + add, 20);
                }
                return s;
            }

            // 可达性检查：是否还能凑够 “≥阈值4 的属性数 >= MinHighAttrs”
            bool ReachableMinHigh4(int[] cur, int remaining)
            {
                int countNow = 0;
                for (int a = 0; a < A; a++) if (cur[a] >= MinHighLevelThreshold) countNow++;
                if (countNow >= MinHighAttrs) return true;

                // 估算还能把多少属性推到 >=4
                int need = MinHighAttrs - countNow;
                int promotable = 0;
                for (int a = 0; a < A; a++)
                {
                    if (cur[a] >= MinHighLevelThreshold) continue;

                    // 拿该属性的前 remaining 个潜在增量
                    int add = 0; int took = 0;
                    foreach (var (idx, val) in perAttrSorted[a])
                    {
                        add += val; took++;
                        if (took >= remaining) break;
                    }
                    if (cur[a] + add >= MinHighLevelThreshold) promotable++;
                    if (promotable >= need) return true;
                }
                return false;
            }

            // DFS：先尝试 4 件，若不足再 3 件
            void DFS(int need, int start, List<int> pick, int[] cur)
            {
                int remaining = need - pick.Count;
                if (remaining == 0)
                {
                    TryPush(pick);
                    return;
                }

                // 剪枝 1：剩余可选数不够
                if (n - start < remaining) return;

                // 剪枝 2：MinHighAttrs 可达性
                if (!ReachableMinHigh4(cur, remaining)) return;

                // 剪枝 3：评分上界（按 Σmin(v,20) 的粗上界）
                double ub = UpperBoundScore(cur, start, remaining);
                // 这里我们用原始 Score（Σmin）作剪枝对比；排序仍用 IsBetterPatternFirst
                if (best.Count == topN && ub < best[^1].Score - 1e-9) return;

                for (int i = start; i < n; i++)
                {
                    // 增量写 cur
                    var added = new List<(int a, int v)>();
                    for (int a = 0; a < A; a++)
                    {
                        int v = modAttr[i, a];
                        if (v != 0) { cur[a] += v; added.Add((a, v)); }
                    }
                    pick.Add(i);

                    DFS(need, i + 1, pick, cur);

                    // 回溯
                    pick.RemoveAt(pick.Count - 1);
                    foreach (var (a, v) in added) cur[a] -= v;
                }
            }

            var cur = new int[A];

            if (n >= 4) DFS(4, 0, new(), cur);
            // 若 4 件的结果不足，再补 3 件
            if (best.Count < topN) DFS(3, 0, new(), cur);

            // 排序：继续用你的“形态优先/聚焦优先”的比较器
            best.Sort((x, y) => IsBetterPatternFirst(x, y) ? -1 : (IsBetterPatternFirst(y, x) ? 1 : 0));
            return best.Take(Math.Min(topN, best.Count)).ToList();
        }


        /// <summary>
        /// 启发式搜索总流程：过滤→预筛→多次“贪心+局部搜索”→去重→取TopN
        /// </summary>
        public List<ModuleSolution> FindOptimalCombinations_GreedySearch(List<ModuleInfo> modules, ModuleCategory category, int topN = 20)
        {
            List<ModuleInfo> filtered = category switch
            {
                ModuleCategory.ALL => modules.ToList(),
                _ => modules.Where(m => GetModuleCategory(m) == category).ToList()
            };
            if (filtered.Count < 4) return new();

            var candidatePool = PrefilterModules_Strict(filtered);
            var results = new List<ModuleSolution>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int attempts = 0, maxAttempts = MaxSolutions * 20;

            while (results.Count < MaxSolutions && attempts < maxAttempts)
            {
                attempts++;
                var initial = GreedyConstruct(candidatePool);
                if (initial == null) continue;

                var improved = LocalSearch(initial, candidatePool, LocalSearchIterations);
                if (improved == null) continue;

                if (!MeetsGlobalRequirements(improved.Modules)) continue;

                var key = string.Join(",", improved.Modules.Select(m => m.Uuid).OrderBy(x => x));
                if (seen.Add(key))
                    results.Add(improved);
            }

            results.Sort((a, b) =>
            {
                // 用上面的比较器做降序排序
                if (IsBetterPatternFirst(a, b)) return -1;
                if (IsBetterPatternFirst(b, a)) return 1;
                return 0;
            });

            return results.Take(Math.Max(0, topN)).ToList();
        }

        /// <summary>
        /// 旧的“精英池枚举组合”版本（保留签名，便于回退）
        /// </summary>
        public List<ModuleCombination> FindOptimalCombinations(
            List<ModuleInfo> modules,
            ModuleCategory category,
            int topN = 20)
        {
            return new List<ModuleCombination>(); // 保留回退占位
        }

        /// <summary>打印组合详情（保持你的实现，不变）</summary>
        public void PrintCombinationDetails(ModuleCombination combination, int rank)
        {
            var dto = ResultCardData.FromCombination(combination, rank);
            ModuleResultMemory.Add(dto);
        }

        /// <summary>主流程：优化并打印（保持你的实现，不变）</summary>
        public void OptimizeAndDisplay(List<ModuleInfo> modules, ModuleCategory category = ModuleCategory.ATTACK, int topN = 20)
        {
            var optimal = FindOptimalCombinations(modules, category, topN);
            if (optimal.Count == 0) return;
            for (int i = 0; i < optimal.Count; i++)
                PrintCombinationDetails(optimal[i], i + 1);
        }

        // ===== 私有工具 =====
        private static int SumParts(ModuleInfo m) =>
            m?.Parts?.Sum(p => p?.Value ?? 0) ?? 0;

        private static List<ModuleInfo> RandomSample(List<ModuleInfo> src, int k, Random rnd)
        {
            if (k >= src.Count) return src.ToList();
            var indices = Enumerable.Range(0, src.Count).OrderBy(_ => rnd.Next()).Take(k);
            var res = new List<ModuleInfo>();
            foreach (var i in indices) res.Add(src[i]);
            return res;
        }

    }

    public class BuildEliteCandidatePool
    {
        public static string type = "攻击";
        public static List<string> Attributes = new List<string>();

        // === 筛选配置 ===
        public static HashSet<string> ExcludedAttributes { get; set; } = new(StringComparer.Ordinal);
        public static List<string> RequiredAttributes { get; set; } = new(); // 需要包含的词条集合
        public static int RequiredAttributeCount { get; set; } = 0;          // 需要命中的词条数量（全局组合维度）
        public static int? ExactPartCount { get; set; } = null;              // 精确词条数（每个模组）
        public static int? MinPartCount { get; set; } = null;                // 或者下限
        public static int? MaxPartCount { get; set; } = null;                // 或者上限
        public static bool MixCategories { get; set; } = false;              // 允许攻击/守护/辅助混搭

        public static void ParseModuleInfo(byte[] payloadBuffer)
        {
            ModuleResultMemory.Clear();
            var syncContainerData = BlueprotobufPb2.SyncContainerData.Parser.ParseFrom(payloadBuffer);
            BlueprotobufPb2.CharSerialize Serialize = syncContainerData.VData;

            if (Serialize?.Mod?.ModInfos == null) return;
            var mod_infos = Serialize?.Mod?.ModInfos;
            if (Serialize.ItemPackage.Packages == null) return;

            var modules = new List<ModuleInfo>();
            foreach (var kv in Serialize.ItemPackage.Packages)
            {
                int packageType = kv.Key;
                var package = kv.Value;
                foreach (var item in package.Items)
                {
                    var key = item.Key;
                    var value = item.Value;
                    if (value != null && value.ModNewAttr.ModParts.Count > 0)
                    {
                        int config_id = value.ConfigId;
                        string module_name = ModuleMaps.MODULE_NAMES[config_id];
                        var modParts = item.Value.ModNewAttr.ModParts;

                        var modInfoVal = mod_infos?.GetValueOrDefault(key) ?? default;

                        var module_info = new ModuleInfo
                        {
                            Name = module_name,
                            ConfigId = config_id,
                            Uuid = value.Uuid,
                            Quality = value.Quality,
                            Parts = new List<ModulePart>()
                        };
                        var init_link_nums = modInfoVal.InitLinkNums;
                        int n = Math.Min(modParts.Count, init_link_nums.Count);
                        for (int i = 0; i < n; i++)
                        {
                            int partId = modParts[i];

                            string attrName = ModuleMaps.MODULE_ATTR_NAMES.TryGetValue(partId, out var name)
                                ? name
                                : $"未知属性({partId})";

                            int attrValue = init_link_nums[i];

                            var modulePart = new ModulePart
                            {
                                Id = partId,
                                Name = attrName,
                                Value = attrValue,
                            };

                            module_info.Parts.Add(modulePart);
                        }
                        modules.Add(module_info);
                    }
                    else
                    {
                        //不是模组包
                        break;
                    }
                }
            }

            if (modules.Count > 0)
            {
                var filtered_modules = new List<ModuleInfo>();

                foreach (var module in modules)
                {
                    var moduleAttrs = module.Parts.Select(part => part.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();

                    // 1) 词条数量限制
                    int partCount = module.Parts?.Count ?? 0;
                    if (BuildEliteCandidatePool.ExactPartCount.HasValue &&
                        partCount != BuildEliteCandidatePool.ExactPartCount.Value) continue;
                    if (BuildEliteCandidatePool.MinPartCount.HasValue &&
                        partCount < BuildEliteCandidatePool.MinPartCount.Value) continue;
                    if (BuildEliteCandidatePool.MaxPartCount.HasValue &&
                        partCount > BuildEliteCandidatePool.MaxPartCount.Value) continue;

                    // 2) 排除词条
                    if (BuildEliteCandidatePool.ExcludedAttributes.Count > 0 &&
                        moduleAttrs.Any(a => BuildEliteCandidatePool.ExcludedAttributes.Contains(a)))
                    {
                        continue;
                    }

                    // 3) 白名单：若给了 Attributes，则要求“该模组的全部词条都在白名单中”
                    // 3) 白名单（宽松版）：命中数 >= k 即通过，允许出现额外的非白名单属性
                    if (Attributes.Count > 0)
                    {
                        // k 可做配置，常用 1 或 RequiredAttributeCount
                        int k = Math.Max(1, BuildEliteCandidatePool.RequiredAttributeCount);
                        int hit = moduleAttrs.Count(a => Attributes.Contains(a));
                        if (hit < k) continue; // 未达到最低命中数才筛掉
                    }


                    filtered_modules.Add(module);
                }

                // 挑选优质模组（走新策略）
                FilterModulesByAttributes(filtered_modules, type);
            }
        }


        public static void FilterModulesByAttributes(List<ModuleInfo> modules, string category)
        {
            ModuleCategory targetCategory = BuildEliteCandidatePool.MixCategories ? ModuleCategory.ALL
                : category switch
                {
                    "攻击" => ModuleCategory.ATTACK,
                    "守护" => ModuleCategory.GUARDIAN,
                    "辅助" => ModuleCategory.SUPPORT,
                    "全部" => ModuleCategory.ALL,
                    _ => ModuleCategory.ATTACK
                };

            var optimizer = new ModuleOptimizer();

            // 先强预筛，决定路线
            var tmpPool = optimizer
                .GetType()
                .GetMethod("PrefilterModules_Strict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(optimizer, new object[] { modules }) as List<ModuleInfo>;

            List<ModuleSolution> solutions;

            if ((tmpPool?.Count ?? 0) <= 50)
                solutions = optimizer.FindOptimalCombinations_Exhaustive(modules, targetCategory, topN: 40);
            else
                solutions = optimizer.FindOptimalCombinations_GreedySearch(modules, targetCategory, topN: 40);

            // …后续与你原逻辑一致（阈值化、最高等级、LinkEffect 等）…
            if (solutions != null && solutions.Count > 0)
            {
                for (int i = 0; i < solutions.Count; i++)
                {
                    var sol = solutions[i];
                    var (thresholdTotal, _) = optimizer.CalculateTotalAttrValue(sol.Modules);
                    int highestLevel = ModuleOptimizer.GetHighestThresholdLevel(sol.AttrBreakdown);
                    var (linkEffectScore, perAttrContrib) = optimizer.CalculateLinkEffect(sol.AttrBreakdown);

                    var combination = new ModuleCombination
                    {
                        Modules = sol.Modules.ToList(),
                        TotalAttrValue = thresholdTotal,
                        AttrBreakdown = new Dictionary<string, int>(sol.AttrBreakdown, StringComparer.Ordinal),
                        ThresholdLevel = highestLevel,
                        Score = sol.Score,
                        LinkEffectScore = linkEffectScore,
                        LinkEffectContrib = perAttrContrib
                    };
                    optimizer.PrintCombinationDetails(combination, i + 1);
                }
            }
            else
            {
                optimizer.OptimizeAndDisplay(modules, targetCategory);
            }
        }

    }
}
