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
        /// <summary>阈值化后的总值（用于展示）</summary>
        public int TotalAttrValue { get; init; }
        /// <summary>原始属性分布：属性名 -> 累计值</summary>
        public Dictionary<string, int> AttrBreakdown { get; init; } = new();
        /// <summary>达到的最高阈值等级（对应 ATTR_THRESHOLDS 的索引；-1 表示无）</summary>
        public int ThresholdLevel { get; init; }
        /// <summary>综合评分（越高越好）</summary>
        public double Score { get; init; }
    }

    /// <summary>
    /// 模组搭配优化器（Console 版本）
    /// 对齐 ModuleType.cs 的定义：全部类型来自 StarResonanceDpsAnalysis.Core.TabelJson
    /// 使用 ModuleMaps.MODULE_CATEGORY_MAP / ATTR_THRESHOLDS / MODULE_CATEGORY_NAMES
    /// </summary>
    public class ModuleOptimizer
    {
        // 阶梯基础分配置（包含 1 级，配合 ModuleMaps.ATTR_THRESHOLDS = {1,4,8,12,16,20}）
        private static readonly Dictionary<int, int> TierScores = new()
        {
            { 20, 100000 }, { 16, 50000 }, { 12, 15000 }, {  8,  5000 },
            {  4,   1000 }, {  1,   100  },
        };

        /// <summary>按 config_id 获取分类，未命中默认 ATTACK</summary>
        public ModuleCategory GetModuleCategory(ModuleInfo module)
        {
            return ModuleMaps.MODULE_CATEGORY_MAP.TryGetValue(module.ConfigId, out var cat)
                ? cat
                : ModuleCategory.ATTACK;
        }

        /// <summary>
        /// 计算“阈值化总值”与“原始属性分布”
        /// 阈值化总值：每个属性按阈值就近向下取整累加（如 17→16）
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
                // ATTR_THRESHOLDS 在 ModuleType.cs 中定义为升序：{1,4,8,12,16,20}
                foreach (var t in ModuleMaps.ATTR_THRESHOLDS)
                {
                    if (val >= t) chosen = t;
                    else break;
                }
                totalThresholdValue += chosen;
            }

            return (totalThresholdValue, attrBreakdown);
        }

        /// <summary>
        /// 组合评分（V2）：
        /// 1) 强奖励高阈值(16/20)；2) 超过20的浪费强惩罚；3) 多个高等级词条额外加分。
        /// </summary>
        public double CalculateCombinationScoreV2(Dictionary<string, int> attrBreakdown)
        {
            double total = 0.0;
            int highTierCount = 0;

            var thresholdsDesc = ModuleMaps.ATTR_THRESHOLDS.OrderByDescending(x => x).ToArray();

            foreach (var (_, value) in attrBreakdown)
            {
                int waste = Math.Max(0, value - 20);

                int achieved = 0;
                foreach (var t in thresholdsDesc)
                {
                    if (value >= t) { achieved = t; break; }
                }

                double score = 0.0;
                if (achieved > 0)
                {
                    if (TierScores.TryGetValue(achieved, out var baseScore))
                        score += baseScore;

                    // 同阈值下细分：更高原始值稍优
                    score += value * 10;

                    // 浪费强惩罚
                    score -= waste * 2500;
                }

                total += score;
                if (achieved >= 16) highTierCount++;
            }

            // 多高等级词条奖励
            if (highTierCount > 1)
                total += (highTierCount - 1) * 75000;

            return total;
        }

        /// <summary>
        /// 寻找最优 4 件组合（启发式精英池）
        /// </summary>
        public List<ModuleCombination> FindOptimalCombinations(
            List<ModuleInfo> modules,
            ModuleCategory category,
            int topN = 20)
        {
            //Console.WriteLine($"[INFO] 开始计算 {ToChinese(category)} 类型模组的最优搭配 (启发式精英池算法)");

            // 1) 过滤类别
            var filtered = modules.Where(m => GetModuleCategory(m) == category).ToList();
            //Console.WriteLine($"[INFO] 找到 {filtered.Count} 个 {ToChinese(category)} 类型模组");

            if (filtered.Count < 4)
            {
                //Console.WriteLine($"[WARN] {ToChinese(category)} 类型模组数量不足4个, 无法形成搭配");
                return new();
            }

            // 2) 每个属性的“单科状元”榜
            var allAttrs = new HashSet<string>(
                filtered.SelectMany(m => m.Parts ?? new List<ModulePart>())
                        .Select(p => p.Name)
                        .Where(n => !string.IsNullOrEmpty(n)),
                StringComparer.Ordinal);

            var attrSortedModules = new Dictionary<string, List<ModuleInfo>>(StringComparer.Ordinal);
            foreach (var attrName in allAttrs)
            {
                var list = new List<(ModuleInfo m, int val)>();
                foreach (var m in filtered)
                {
                    int v = m.Parts?.FirstOrDefault(p => p.Name == attrName)?.Value ?? 0;
                    if (v > 0) list.Add((m, v));
                }
                list.Sort((a, b) => b.val.CompareTo(a.val));
                attrSortedModules[attrName] = list.Select(x => x.m).ToList();
            }

            // 3) 精英池（去重）
            var elite = new Dictionary<string, ModuleInfo>(StringComparer.Ordinal);
            const int pickPerAttr = 10;

            foreach (var list in attrSortedModules.Values)
            {
                foreach (var m in list.Take(pickPerAttr))
                {
                    var key = m.Uuid.ToString(); // ModuleType.cs: Uuid 是 long
                    elite[key] = m;
                }
            }

            // 不足则补“总属性值”高者
            if (elite.Count < 20 && filtered.Count > elite.Count)
            {
                foreach (var m in filtered.OrderByDescending(SumParts))
                {
                    var key = m.Uuid.ToString();
                    if (!elite.ContainsKey(key))
                        elite[key] = m;
                    if (elite.Count >= 20) break;
                }
            }

            var candidates = elite.Values.ToList();
           // Console.WriteLine($"[INFO] 创建了 {candidates.Count} 个模组的精英候选池进行组合计算");

            // 4) 控规模
            if (candidates.Count > 40)
            {
                //Console.WriteLine($"[WARN] 精英池过大({candidates.Count})，将截取总属性值最高的前40个");
                candidates = candidates.OrderByDescending(SumParts).Take(40).ToList();
            }

            // 5) 生成 4 组合
            var combos = GenerateCombosOf4(candidates);
            //Console.WriteLine($"[INFO] 从精英池生成了 {combos.Count} 个4模组组合");

            // 6) 评分与汇总
            var results = new List<ModuleCombination>(combos.Count);
            foreach (var combo in combos)
            {
                var (thresholdTotal, breakdown) = CalculateTotalAttrValue(combo);
                double score = CalculateCombinationScoreV2(breakdown);

                int highestLevel = -1;
                foreach (var value in breakdown.Values)
                {
                    int level = -1;
                    for (int i = 0; i < ModuleMaps.ATTR_THRESHOLDS.Length; i++)
                    {
                        if (value >= ModuleMaps.ATTR_THRESHOLDS[i]) level = i;
                    }
                    if (level > highestLevel) highestLevel = level;
                }

                results.Add(new ModuleCombination
                {
                    Modules = combo.ToList(),
                    TotalAttrValue = thresholdTotal,
                    AttrBreakdown = breakdown,
                    ThresholdLevel = highestLevel,
                    Score = score
                });
            }

            // 7) 排序取前 N
            results.Sort((a, b) => b.Score.CompareTo(a.Score));
            return results.Take(Math.Max(0, topN)).ToList();
        }

        /// <summary>打印组合详情</summary>
        public void PrintCombinationDetails(ModuleCombination combination, int rank)
        {
            
           // Console.WriteLine($"\n=== 第{rank}名搭配 ===");

            string levelDesc = combination.ThresholdLevel >= 0 &&
                               combination.ThresholdLevel < ModuleMaps.ATTR_THRESHOLDS.Length
                ? $"{ModuleMaps.ATTR_THRESHOLDS[combination.ThresholdLevel]}点"
                : "无";

           // Console.WriteLine($"最高属性等级: {combination.ThresholdLevel} ({levelDesc})");
           // Console.WriteLine($"综合评分: {combination.Score:F1}");

            //Console.WriteLine("\n模组列表:");
            for (int i = 0; i < combination.Modules.Count; i++)
            {
                var m = combination.Modules[i];
                var partsStr = string.Join(", ",
                    (m.Parts ?? new List<ModulePart>()).Select(p => $"{p.Name}+{p.Value}"));

                var shortUuid = m.Uuid.ToString();
                if (shortUuid.Length > 6) shortUuid = shortUuid[..6];

              //  Console.WriteLine($"  {i + 1}. {m.Name} (品质{m.Quality}, UUID:{shortUuid}) - {partsStr}");
            }

           // Console.WriteLine("\n属性分布 (原始总值):");
            foreach (var kv in combination.AttrBreakdown.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                //Console.WriteLine($"  {kv.Key}: +{kv.Value}");
            }
            var dto = ResultCardData.FromCombination(combination, rank);
            ModuleResultMemory.Add(dto);

        }

        /// <summary>主流程：优化并打印</summary>
        public void OptimizeAndDisplay(List<ModuleInfo> modules, ModuleCategory category = ModuleCategory.ATTACK, int topN = 20)
        {
            //Console.WriteLine(new string('=', 50));
            //Console.WriteLine($"模组搭配优化 - {ToChinese(category)}类型");
            //Console.WriteLine(new string('=', 50));

            var optimal = FindOptimalCombinations(modules, category, topN);

            if (optimal.Count == 0)
            {
                Console.WriteLine($"未找到{ToChinese(category)}类型的有效搭配");
                return;
            }

            //Console.WriteLine($"\n找到{optimal.Count}个最优搭配:");
            for (int i = 0; i < optimal.Count; i++)
                PrintCombinationDetails(optimal[i], i + 1);

            //Console.WriteLine($"\n{new string('=', 50)}");
            //Console.WriteLine("统计信息:");
            //Console.WriteLine($"总模组数量: {modules.Count}");
            //Console.WriteLine($"{ToChinese(category)}类型模组: {modules.Count(m => GetModuleCategory(m) == category)}");
            //Console.WriteLine($"最高分搭配评分: {optimal[0].Score:F1}");
            //Console.WriteLine($"最高分搭配的最高属性等级: {optimal[0].ThresholdLevel}");
            //Console.WriteLine(new string('=', 50));
        }

        #region 私有辅助
        private static int SumParts(ModuleInfo m) =>
            m?.Parts?.Sum(p => p?.Value ?? 0) ?? 0;

        private static List<List<ModuleInfo>> GenerateCombosOf4(List<ModuleInfo> items)
        {
            var res = new List<List<ModuleInfo>>();
            int n = items.Count;
            for (int i = 0; i < n - 3; i++)
                for (int j = i + 1; j < n - 2; j++)
                    for (int k = j + 1; k < n - 1; k++)
                        for (int l = k + 1; l < n; l++)
                            res.Add(new List<ModuleInfo> { items[i], items[j], items[k], items[l] });
            return res;
        }

        private static string ToChinese(ModuleCategory c)
            => ModuleMaps.MODULE_CATEGORY_NAMES.TryGetValue(c, out var s) ? s : c.ToString();
        #endregion
    }
   
    public class BuildEliteCandidatePool
    {
        public static string type = "攻击";
        public static List<string> Attributes = new List<string>();
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
                int packageType = kv.Key;                   // key 类型通常是 uint / uint32
                var package = kv.Value;    // value 是生成的 Package message 类型
                foreach (var item in package.Items)
                {
                    var key = item.Key;
                    var value = item.Value;
                    if (value != null && value.ModNewAttr.ModParts.Count > 0)
                    {
                        int config_id = value.ConfigId;
                        string module_name = ModuleMaps.MODULE_NAMES[config_id];
                        var modParts = item.Value.ModNewAttr.ModParts;
                        // 参考类型值（比如 ModInfo 是 class）
                        var modInfo = mod_infos?.GetValueOrDefault(key);

                        // 若 TValue 是值类型并且你希望 default 值（例如 0/false/结构体默认）
                        var modInfoVal = mod_infos?.GetValueOrDefault(key) ?? default;

                        var module_info = new ModuleInfo
                        {
                            Name = module_name,
                            ConfigId = config_id,
                            Uuid = value.Uuid,
                            Quality = value.Quality,
                            Parts = new List<ModulePart>()  // 等价于 Python 的 parts=[]
                        };
                        var init_link_nums = modInfoVal.InitLinkNums;
                        int n = Math.Min(modParts.Count, init_link_nums.Count);   // 对齐两个列表的最小长度
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
                //属性筛选
                if (Attributes.Count > 0)
                {
                    var filtered_modules = new List<ModuleInfo>();
                    foreach (var module in modules)
                    {
                        var moduleAttrs = module.Parts.Select(part => part.Name).ToList();
                        bool allModuleAttrsInList = moduleAttrs.All(attr => Attributes.Contains(attr));
                        if (allModuleAttrsInList)
                        {
                            filtered_modules.Add(module);
                        }
                        else
                        {
                            var invalidAttrs = moduleAttrs
                            .Where(attr => !Attributes.Contains(attr)).ToList();
                        }

                    }
                    //挑选优质模组
                    FilterModulesByAttributes(filtered_modules, type);
                }
                else
                {
                    //挑选优质模组
                    FilterModulesByAttributes(modules, type);
                }
               
            }

        }

        public static void FilterModulesByAttributes(
    List<ModuleInfo> modules, string category)
        {
            ModuleCategory targetCategory = category switch
            {
                "攻击" => ModuleCategory.ATTACK,
                "守护" => ModuleCategory.GUARDIAN,
                "辅助" => ModuleCategory.SUPPORT,
                _ => ModuleCategory.ATTACK
            };
            var optimizer = new ModuleOptimizer();
            optimizer.OptimizeAndDisplay(modules, targetCategory);


        }
    }
}
