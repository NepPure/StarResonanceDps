using System;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks; // 引用异步任务支持（Task/async/await）
using System.Windows.Forms;

using AntdUI; // 引用 AntdUI 组件库（第三方 UI 控件/样式）
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.WinForm.Control; // 引用项目内的 UI 控制/辅助类命名空间
using StarResonanceDpsAnalysis.WinForm.Forms.PopUp; // 引用弹窗相关窗体/组件命名空间
using StarResonanceDpsAnalysis.WinForm.Plugin; // 引用项目插件层通用命名空间
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics; // 引用伤害统计插件命名空间（含 FullRecord、StatisticData 等）
using StarResonanceDpsAnalysis.WinForm.Plugin.LaunchFunction; // 引用启动相关功能（加载技能配置等）
using StarResonanceDpsAnalysis.WinForm.Forms.ModuleForm;

using static StarResonanceDpsAnalysis.WinForm.Control.SkillDetailForm;
using Button = AntdUI.Button;
using Color = System.Drawing.Color;
using StarResonanceDpsAnalysis.WinForm.Control.GDI;
using StarResonanceDpsAnalysis.Core.Extends.System;

namespace StarResonanceDpsAnalysis.WinForm.Forms // 定义命名空间：窗体相关代码所在位置
{ // 命名空间开始
    public partial class DpsStatisticsForm : BorderlessForm // 定义无边框窗体的局部类（与 Designer 生成的部分合并）
    { // 类开始
        // # 导航
        // # 本文件职责：
        // #   1) 窗体构造与启动流程（初始化 UI/钩子/配置/设备/技能配置）。
        // #   2) 列表交互（选择条目 → 打开技能详情窗口）。
        // #   3) 顶部操作（置顶、设置菜单、提示气泡）。
        // #   4) 统计视图切换（单次/全程 + 左右切换指标）。
        // #   5) 定时刷新（战斗时长、榜单数据刷新）。
        // #   6) 清空/打桩模式（定时器与上传流程）。
        // #   7) 主题切换（前景色变化时的控件背景适配）。
        // #   8) 全局热键钩子（安装/卸载/按键路由）。
        // #   9) 窗口控制（鼠标穿透、透明度切换）。

        // # 构造与启动流程
        public DpsStatisticsForm() // 构造函数：创建窗体实例时执行一次
        {

            InitializeComponent(); // 初始化设计器生成的控件与布局


            Text = FormManager.APP_NAME;

            FormGui.SetDefaultGUI(this); // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）

            //ApplyResolutionScale(); // 可选：根据屏幕分辨率对整体界面进行缩放（当前禁用，仅保留调用）

            // 从资源文件设置字体
            SetDefaultFontFromResources();

            // 加载钩子
            RegisterKeyboardHook(); // 安装键盘钩子，用于全局热键监听与处理

            // 首次启动时初始化基础配置
            InitTableColumnsConfigAtFirstRun(); // 首次运行初始化表格列配置（列宽/显示项等）

            // 加载网卡
            LoadNetworkDevices(); // 加载/枚举网络设备（抓包设备列表）

            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色 // 根据配置设置窗体的颜色主题（明亮/深色）

            // 加载技能配置
            StartupInitializer.LoadFromEmbeddedSkillConfig(); // 从内置资源读取并加载技能数据（元数据/图标/映射）


            sortedProgressBarList_MainList.SelectionChanged += (s, i, d) => // 订阅进度条列表的选择变化事件（点击条目）
            {
                // # UI 列表交互：当用户点击列表项时触发（i 为索引，d 为 ProgressBarData）

                // 若未选中有效项或数据为空则直接返回
                if (i < 0 || d == null)
                {
                    return;
                }

                // # 将选中项的 UID 传入详情窗口刷新
                sortedProgressBarList_SelectionChanged(d.ID); // 将条目 ID 转为 UID 并调用详情刷新逻辑
            };

            SetStyle(); // 设置/应用本窗体的个性化样式（定义在同类/局部类的其他部分）

            // TODO: 此处的 4 个事件是临时测试用, 后续需要规范注册事件并实现功能
            DataStorage.PlayerInfoUpdated += playerInfo =>
            {
                //Console.WriteLine($"PlayerInfo Updated: {playerInfo.Name}({playerInfo.UID})");
            };

            DataStorage.BattleLogNewSectionCreated += () =>
            {
                Console.WriteLine($"New Battle Section Created.");
            };

            var renderListDict = new Dictionary<long, List<RenderContent>>();
            DataStorage.BattleLogUpdated += battleLog =>
            {
                // 此函数仅做测试用, 正式使用时需标准化 => 封装函数并在事件中调用

                var dpsList = DataStorage.ReadOnlySectionedDpsDataList;

                // 正式使用时, 需要在此处判断当前类型(DPS/HPS/承伤)(全程/阶段)
                var maxValue = dpsList.Max(e => e.TotalAttackDamage);
                var sumValue = dpsList.Sum(e => e.TotalAttackDamage);

                var progressBarDataList = dpsList
                    .Where(e => !e.IsNpcData && e.TotalAttackDamage != 0)
                    .Select(e =>
                    {
                        DataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(e.UID, out var playerInfo);
                        var professionName = Test_GetProfessionName(playerInfo?.ProfessionID ?? 0);

                        var flag = renderListDict.TryGetValue(e.UID, out var renderContent);
                        if (!flag)
                        {
                            var profBmp = imgDict.TryGetValue(professionName, out var bmp) ? bmp : imgDict["未知"];
                            renderContent =
                            [
                                new() { Type = RenderContent.ContentType.Image, Align = RenderContent.ContentAlign.MiddleLeft, Offset = AppConfig.ProgressBarImage, Image = profBmp, ImageRenderSize = AppConfig.ProgressBarImageSize },
                                new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleLeft, Offset = AppConfig.ProgressBarNmae, ForeColor = AppConfig.colorText, Font = AppConfig.ProgressBarFont },
                                new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleRight, Offset = AppConfig.ProgressBarHarm, ForeColor = AppConfig.colorText, Font = AppConfig.ProgressBarFont },
                                new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleRight, Offset = AppConfig.ProgressBarProportion,  ForeColor = AppConfig.colorText, Font = AppConfig.ProgressBarFont },
                            ];
                            renderListDict[e.UID] = renderContent;
                        }

                        renderContent![1].Text = $"{playerInfo?.Name}-{professionName}({e.UID})";

                        renderContent[2].Text = $"{e.TotalAttackDamage.ToCompactString()} ({(e.TotalAttackDamage / new TimeSpan(e.LastLoggedTick - (e.StartLoggedTick ?? 0)).TotalSeconds).ToCompactString()})";
                        renderContent[3].Text = $"{Math.Round(100d * e.TotalAttackDamage / sumValue, 0, MidpointRounding.AwayFromZero)}%";

                        return new ProgressBarData()
                        {
                            ID = e.UID,
                            ProgressBarColor = Test_GetProfessionColor(playerInfo?.ProfessionID ?? 0),
                            ProgressBarCornerRadius = 3,
                            ProgressBarValue = 1f * e.TotalAttackDamage / maxValue,
                            ContentList = renderContent
                        };
                    }).ToList();

                sortedProgressBarList_MainList.Data = progressBarDataList;

                //Console.WriteLine($"BattleLog Updated({DataStorage.ReadOnlyBattleLogs.Count}): {battleLog.AttackerUuid}→{battleLog.TargetUuid}: {battleLog.SkillID}({battleLog.Value})");
            };

            DataStorage.DataUpdated += () =>
            {
                //Console.WriteLine($"Data Updated.");
            };

        }

        private string Test_GetProfessionName(int professionID)
        {
            // 此函数仅做测试用, 正式使用时需标准化

            return StarResonanceDpsAnalysis.Core.Extends.Data.ProfessionExtends.GetProfessionNameById(professionID);
        }

        private Color Test_GetProfessionColor(int professionID)
        {
            // 此函数仅做测试用, 正式使用时需标准化

            var map = Config.IsLight ? colorDict : blackColorDict;
            var professionName = StarResonanceDpsAnalysis.Core.Extends.Data.ProfessionExtends.GetProfessionNameById(professionID);
            var flag = map.TryGetValue(professionName, out var color);
            if (flag) return color;
            return map["未知"];
        }

        // # 屏幕分辨率缩放判定
        private static float GetPrimaryResolutionScale() // 依据主屏高度返回推荐缩放比例
        {
            try
            {
                var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080); // 获取主屏尺寸，失败则默认 1080p
                if (bounds.Height >= 2160) return 2.0f;       // 4K 屏：建议缩放 2.0
                if (bounds.Height >= 1440) return 1.3333f;    // 2K 屏：建议缩放 1.3333
                return 1.0f;                                   // 1080p：不缩放
            }
            catch
            {
                return 1.0f; // 异常时安全返回 1.0（不缩放）
            }
        }

        // # 窗体加载事件：启动抓包
        private void DpsStatistics_Load(object sender, EventArgs e) // 窗体 Load 事件处理
        {
            //开启默认置顶

            StartCapture(); // 启动网络抓包/数据采集（核心运行入口之一）

            // 重置为上次关闭前的位置与大小
            SetStartupPositionAndSize();

            EnsureTopMost();
        }

        // # 列表选择变更 → 打开技能详情
        private void sortedProgressBarList_SelectionChanged(long uid) // 列表项选择回调：传入选中玩家 UID
        {
            // 如果当前是“NPC承伤”视图：点击 NPC 行切换到“打这个NPC的玩家排名”
            if (FormManager.currentIndex == 3)
            {
                // 全程显示：直接刷新为该NPC的攻击者榜
                _npcDetailMode = true;
                _npcFocusId = uid;

                // 立刻刷新该 NPC 的攻击者榜（当前/全程均已在方法内部自动分流）
                RefreshNpcAttackers(_npcFocusId);
                // 可选：更新标题
                pageHeader_MainHeader.SubText = FormManager.showTotal ? $"全程 · NPC攻击者榜 (NPC:{uid})" : $"当前 · NPC攻击者榜 (NPC:{uid})";
                return;
            }

            // ……下面是你原来的玩家技能详情逻辑……
            if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed)
                FormManager.skillDetailForm = new SkillDetailForm();

            SkillTableDatas.SkillTable.Clear();

            FormManager.skillDetailForm.Uid = uid;
            var info = StatisticData._manager.GetPlayerBasicInfo(uid);
            FormManager.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession);

            if (FormManager.showTotal) { FormManager.skillDetailForm.ContextType = DetailContextType.FullRecord; FormManager.skillDetailForm.SnapshotStartTime = null; }
            else { FormManager.skillDetailForm.ContextType = DetailContextType.Current; FormManager.skillDetailForm.SnapshotStartTime = null; }

            FormManager.skillDetailForm.SelectDataType();
            if (!FormManager.skillDetailForm.Visible) FormManager.skillDetailForm.Show(); else FormManager.skillDetailForm.Activate();
        }

        // # 顶部：置顶窗口按钮
        private void button_AlwaysOnTop_Click(object sender, EventArgs e) // 置顶按钮点击事件
        {
            TopMost = !TopMost; // 简化切换
            button_AlwaysOnTop.Toggle = TopMost; // 同步按钮的视觉状态
        }

        #region 切换显示类型（支持单次/全程伤害） // 折叠：视图标签与切换逻辑


        // # 头部标题文本刷新：依据 showTotal & currentIndex
        private void UpdateHeaderText() // 根据当前模式与索引更新顶部标签文本
        {

            if (FormManager.showTotal)
            {
                pageHeader_MainHeader.SubText = FormManager.currentIndex switch
                {
                    1 => "全程治疗",
                    2 => "全程承伤",
                    3 => "全程 · NPC承伤",
                    _ => "全程伤害"
                };
            }
            else
            {
                pageHeader_MainHeader.SubText = FormManager.currentIndex switch
                {
                    1 => "当前治疗",
                    2 => "当前承伤",
                    3 => "当前 · NPC承伤",
                    _ => "当前伤害"
                };
            }
        }



        // 单次/全程切换
        private void button_SwitchStatisticsMode_Click(object sender, EventArgs e) // 单次/全程切换按钮事件
        {
            FormManager.showTotal = !FormManager.showTotal; // 取反：在单次与全程之间切换
            UpdateHeaderText(); // 切换后刷新顶部文本
        }
        #endregion

        // # 定时刷新：战斗时长显示 + 榜单刷新
        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e) // 定时器：周期刷新（UI 绑定）
        {
            return;

            if (FormManager.currentIndex == 3)
            {
                // NPC 承伤页
                if (_npcDetailMode && _npcFocusId != 0)
                {
                    // 正在查看某个 NPC 的攻击者榜 —— 保持停留在详情页并刷新该榜单
                    RefreshNpcAttackers(_npcFocusId);

                    // （可选健壮性）该 NPC 若已消失/无数据，可自动退出详情回到总览
                    // 你可以在 RefreshNpcAttackers 内部判空时自动调用 ExitNpcDetailMode() + RefreshNpcOverview()
                }
                else
                {
                    // 非详情模式：刷新 NPC 承伤总览（当前/全程在方法内部已自行处理）
                    RefreshNpcOverview();
                }
            }
            else
            {
                var source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;
                var metric = FormManager.currentIndex switch
                {
                    1 => MetricType.Healing,
                    2 => MetricType.Taken,
                    3 => MetricType.NpcTaken,   // ★ 保留：其他地方如果有用到
                    _ => MetricType.Damage
                };
                RefreshDpsTable(source, metric);
            }

            var duration = StatisticData._manager.GetFormattedCombatDuration();
            if (FormManager.showTotal) duration = FullRecord.GetEffectiveDurationString();
            label_BattleTimeText.Text = duration;
        }


        /// <summary>
        /// 清空当前数据数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_RefreshDps_Click(object sender, EventArgs e) // 清空按钮点击：触发清空逻辑
        {
            // # 清空：触发 HandleClearData（停止图表刷新→清空数据→重置图表）
            HandleClearData(); // 调用清空处理
        }


        // # 设置按钮 → 右键菜单
        private void button_Settings_Click(object sender, EventArgs e) // 设置按钮点击：弹出右键菜单
        {


            var menulist = new IContextMenuStripItem[] // 构建右键菜单项数组
             { // 数组开始
                    new ContextMenuStripItem("历史战斗") // 一级菜单：历史战斗
                    { // 配置开始
                        IconSvg = HandledAssets.HistoricalRecords, // 图标
                    }, // 一级菜单配置结束
                    new ContextMenuStripItem("基础设置"){ IconSvg = HandledAssets.Set_Up }, // 一级菜单：基础设置
                    new ContextMenuStripItem("主窗体"){ IconSvg = HandledAssets.HomeIcon }, // 一级菜单：主窗体
                    new ContextMenuStripItem("模组配置"){ IconSvg= HandledAssets.ModuleIcon }, // 一级菜单：数据显示设置
                    //new ContextMenuStripItem("技能循环监测"), // 一级菜单：技能循环监测
                    //new ContextMenuStripItem(""){ IconSvg = Resources.userUid, }, // 示例：用户 UID（暂不用）
                    new ContextMenuStripItem("死亡统计"){ IconSvg = HandledAssets.Exclude, }, // 一级菜单：统计排除
                    new ContextMenuStripItem("技能日记"){ IconSvg = HandledAssets.DiaryIcon, },
                    new ContextMenuStripItem("伤害参考"){ IconSvg = HandledAssets.Reference, },
                    new ContextMenuStripItem("打桩模式"){ IconSvg = HandledAssets.Stakes }, // 一级菜单：打桩模式
                    new ContextMenuStripItem("退出"){ IconSvg = HandledAssets.Quit, }, // 一级菜单：退出
             } // 数组结束
            ; // 语句结束（分号保持）

            AntdUI.ContextMenuStrip.open(this, it => // 打开右键菜单并处理点击回调（it 为被点击项）
            {



                // 回调开始
                // # 菜单点击回调：根据 Text 执行对应动作
                switch (it.Text) // 分支根据菜单文本
                {
                    case "历史战斗":
                        if (FormManager.historicalBattlesForm == null || FormManager.historicalBattlesForm.IsDisposed)
                        {
                            FormManager.historicalBattlesForm = new HistoricalBattlesForm();
                        }
                        FormManager.historicalBattlesForm.Show();
                        break;
                    // switch 开始
                    case "基础设置": // 点击“基础设置”
                        OpenSettingsDialog(); // 打开设置面板
                        break; // 跳出 switch
                    case "主窗体": // 点击“主窗体”
                        if (FormManager.mainForm == null || FormManager.mainForm.IsDisposed) // 若主窗体不存在或已释放
                        {
                            FormManager.mainForm = new MainForm(); // 创建主窗体
                        }
                        FormManager.mainForm.Show(); // 显示主窗体
                        break; // 跳出 switch
                    case "模组配置":
                        if (FormManager.moduleCalculationForm == null || FormManager.moduleCalculationForm.IsDisposed) // 若主窗体不存在或已释放
                        {
                            FormManager.moduleCalculationForm = new ModuleCalculationForm(); // 创建主窗体
                        }
                        FormManager.moduleCalculationForm.Show(); // 显示主窗体
                        break;

                    case "死亡统计":
                        if (FormManager.deathStatisticsForm == null || FormManager.deathStatisticsForm.IsDisposed)
                        {
                            FormManager.deathStatisticsForm = new DeathStatisticsForm();
                        }
                        FormManager.deathStatisticsForm.Show();
                        break;
                    case "技能日记":
                        if (FormManager.skillDiary == null || FormManager.skillDiary.IsDisposed)
                        {
                            FormManager.skillDiary = new SkillDiary();
                        }
                        FormManager.skillDiary.Show();
                        break;
                    case "技能循环监测": // 点击“技能循环监测”
                        if (FormManager.skillRotationMonitorForm == null || FormManager.skillRotationMonitorForm.IsDisposed) // 若监测窗体不存在或已释放
                        {
                            FormManager.skillRotationMonitorForm = new SkillRotationMonitorForm(); // 创建窗口
                        }
                        FormManager.skillRotationMonitorForm.Show(); // 显示窗口
                        //FormGui.SetColorMode(FormManager.skillRotationMonitorForm, AppConfig.IsLight); // 同步主题（明/暗）
                        break; // 跳出 switch
                    case "数据显示设置": // 点击“数据显示设置”（当前仅保留占位）
                        //dataDisplay(); 
                        break; // 占位：后续实现
                    case "伤害参考":
                        if (FormManager.rankingsForm == null || FormManager.rankingsForm.IsDisposed) // 若监测窗体不存在或已释放
                        {
                            FormManager.rankingsForm = new RankingsForm(); // 创建窗口
                        }
                        FormManager.rankingsForm.Show(); // 显示窗口
                        break;
                    case "统计排除": // 点击“统计排除”
                        break; // 占位：后续实现
                    case "打桩模式": // 点击“打桩模式”
                        checkbox_PilingMode.Visible = !checkbox_PilingMode.Visible;
                        break; // 跳出 switch
                    case "退出": // 点击“退出”
                        System.Windows.Forms.Application.Exit(); // 结束应用程序
                        break; // 跳出 switch
                } // switch 结束
            }, menulist); // 打开菜单并传入菜单项
        }

        /// <summary>
        /// 打开基础设置面板
        /// </summary>
        private void OpenSettingsDialog() // 打开基础设置窗体
        {
            if (FormManager.settingsForm == null || FormManager.settingsForm.IsDisposed) // 若设置窗体不存在或已释放
            {
                FormManager.settingsForm = new SettingsForm(); // 创建设置窗体
            }
            FormManager.settingsForm.Show(); // 显示设置窗体（或置顶）

        }

        // # 按钮提示气泡（置顶）
        private void button_AlwaysOnTop_MouseEnter(object sender, EventArgs e) // 鼠标进入置顶按钮时显示提示
        {
            ToolTip(button_AlwaysOnTop, "置顶窗口"); // 显示“置顶窗口”的气泡提示


        }

        // # 通用提示气泡工具
        private void ToolTip(System.Windows.Forms.Control control, string text) // 通用封装：在指定控件上显示提示文本
        {

            AntdUI.TooltipComponent tooltip = new AntdUI.TooltipComponent() // 创建 Tooltip 组件实例
            { // 对象初始化器开始
                Font = new Font("HarmonyOS Sans SC", 8, FontStyle.Regular), // 设置提示文字字体
            }; // 对象初始化器结束
            tooltip.ArrowAlign = AntdUI.TAlign.TL; // 设置箭头朝向/对齐方式
            tooltip.SetTip(control, text); // 在目标控件上显示指定文本提示
        }

        // # 按钮提示气泡（清空）
        private void button_RefreshDps_MouseEnter(object sender, EventArgs e) // 鼠标进入“清空”按钮时显示提示
        {
            ToolTip(button_RefreshDps, "清空当前数据"); // 显示“清空当前数据”的气泡提示
        }

        // # 按钮提示气泡（单次/全程切换）
        private void button_SwitchStatisticsMode_MouseEnter(object sender, EventArgs e) // 鼠标进入“单次/全程切换”按钮时显示提示
        {
            ToolTip(button_SwitchStatisticsMode, "点击切换：单次统计/全程统"); // 显示切换提示（原文如此，保留）
        }

        // 打桩模式定时逻辑
        private async void timer_Piling_Tick(object sender, EventArgs e)
        {
            return;

            if (checkbox_PilingMode.Checked)
            {
                if (AppConfig.Uid == 0)
                {

                    checkbox_PilingMode.Checked = false;
                    timer_Piling.Enabled = false;
                    _ = AppMessageBox.ShowMessage("未获取到UID，请换个地图后再进协会", this);
                    return;
                }
                TimeSpan duration = StatisticData._manager.GetCombatDuration();
                if (duration >= TimeSpan.FromMinutes(3))
                {
                    checkbox_PilingMode.Checked = false;
                    timer_Piling.Enabled = false;

                    var snapshot = StatisticData._manager.TakeSnapshotAndGet();
                    var result = AppMessageBox.ShowMessage("打桩完成,是否上传(排行榜仅供娱乐，请勿恶意上传)\n1.如果对自己数据不满意可再次勾选打桩模式重新打桩", this);

                    if (result == DialogResult.OK)
                    {
                        bool data = await Common.AddUserDps(snapshot);
                        if (data)
                        {
                            AntdUI.Modal.open(new Modal.Config(this, "上传成功", "上传成功")
                            {
                                CloseIcon = true,
                                Keyboard = false,
                                MaskClosable = false,
                            });
                        }
                        else
                        {
                            AntdUI.Modal.open(new Modal.Config(this, "上传失败", "请检查网络状况，服务器暂时不支持外网上传")
                            {
                                CloseIcon = true,
                                Keyboard = false,
                                MaskClosable = false,
                            });
                        }
                    }
                }
            }
        }

        // 打桩模式勾选变化
        private void checkbox_PilingMode_CheckedChanged(object sender, BoolEventArgs e)
        {
            TimeSpan duration = StatisticData._manager.GetCombatDuration(); // 保留获取以与原逻辑一致

            if (e.Value)
            {
                var result = AppMessageBox.ShowMessage("打桩时间为3分钟，需注意以下3点:\n0.:打桩模式开启后只会记录自己的数据\n1.开启后请找协会内最右侧木桩[靠窗的那根]\n2.确保战斗计时为0开启\n3.如果伤害不满意可关闭打桩模式重新勾选\n4.异常数据会被删除\n", this);
                if (result == DialogResult.OK)
                {
                    DpsTableDatas.DpsTable.Clear();
                    StatisticData._manager.ClearAll();
                    SkillTableDatas.SkillTable.Clear();
                    Task.Delay(200);
                    AppConfig.PilingMode = true;
                    timer_Piling.Enabled = true;
                }
                else
                {
                    checkbox_PilingMode.Checked = false;
                }
            }
            else
            {
                AppConfig.PilingMode = false;
                timer_Piling.Enabled = false;
            }
        }

        // 主题切换
        private void DpsStatisticsForm_ForeColorChanged(object sender, EventArgs e)
        {
            List<Button> buttonList = new List<Button>() { button_TotalDamage, button_TotalTreatment, button_AlwaysInjured, button_NpcTakeDamage };

            if (Config.IsLight)
            {
                sortedProgressBarList_MainList.BackColor = ColorTranslator.FromHtml("#F5F5F5");
                AppConfig.colorText = Color.Black;
                sortedProgressBarList_MainList.OrderColor = Color.Black;
                panel_Footer.Back = ColorTranslator.FromHtml("#F5F5F5");
                panel_ModeBox.Back = ColorTranslator.FromHtml("#F5F5F5");

                button_TotalDamage.Icon = HandledAssets.伤害;
                button_TotalTreatment.Icon = HandledAssets.治疗;
                button_AlwaysInjured.Icon = HandledAssets.承伤;
                button_NpcTakeDamage.Icon = HandledAssets.Npc;
                Color colorWhite = Color.FromArgb(223, 223, 223);
                foreach (var item in buttonList)
                {
                    item.DefaultBack = Color.FromArgb(247, 247, 247);
                    if (item.Name == "TotalDamageButton" && FormManager.currentIndex == 0)
                    {
                        item.DefaultBack = colorWhite;
                    }
                    if (item.Name == "TotalTreatmentButton" && FormManager.currentIndex == 1)
                    {
                        item.DefaultBack = colorWhite;
                    }
                    if (item.Name == "AlwaysInjuredButton" && FormManager.currentIndex == 2)
                    {
                        item.DefaultBack = colorWhite;
                    }
                    if (item.Name == "NpcTakeDamageButton" && FormManager.currentIndex == 3)
                    {
                        item.DefaultBack = colorWhite;
                    }

                }

            }
            else
            {
                sortedProgressBarList_MainList.BackColor = ColorTranslator.FromHtml("#252527");
                panel_Footer.Back = ColorTranslator.FromHtml("#252527");
                panel_ModeBox.Back = ColorTranslator.FromHtml("#252527");

                AppConfig.colorText = Color.White;
                sortedProgressBarList_MainList.OrderColor = Color.White;
                button_TotalDamage.Icon = HandledAssets.伤害白色;
                button_TotalTreatment.Icon = HandledAssets.治疗白色;
                button_AlwaysInjured.Icon = HandledAssets.承伤白色;
                button_NpcTakeDamage.Icon = HandledAssets.NpcWhite;
                Color colorBack = Color.FromArgb(60, 60, 60);
                foreach (var item in buttonList)
                {
                    item.DefaultBack = Color.FromArgb(27, 27, 27);
                    if (item.Name == "TotalDamageButton" && FormManager.currentIndex == 0)
                    {
                        item.DefaultBack = colorBack;
                    }
                    if (item.Name == "TotalTreatmentButton" && FormManager.currentIndex == 1)
                    {
                        item.DefaultBack = colorBack;
                    }
                    if (item.Name == "AlwaysInjuredButton" && FormManager.currentIndex == 2)
                    {
                        item.DefaultBack = colorBack;
                    }
                    if (item.Name == "NpcTakeDamageButton" && FormManager.currentIndex == 3)
                    {
                        item.DefaultBack = colorBack;
                    }

                }
            }

            SetSortedProgressBarListForeColor();
        }

        private void SetSortedProgressBarListForeColor()
        {
            if (sortedProgressBarList_MainList.Data == null) return;

            lock (sortedProgressBarList_MainList.Data)
            {
                foreach (var data in sortedProgressBarList_MainList.Data)
                {
                    if (data.ContentList == null) continue;

                    foreach (var content in data.ContentList)
                    {
                        if (content.Type != Control.GDI.RenderContent.ContentType.Text) continue;

                        content.ForeColor = AppConfig.colorText;
                    }
                }
            }
        }

        private void SetDefaultFontFromResources()
        {
            pageHeader_MainHeader.Font = AppConfig.SaoFont;
            pageHeader_MainHeader.SubFont = AppConfig.ContentFont;
            checkbox_PilingMode.Font = AppConfig.ContentFont;
            label_CurrentDps.Font = label_CurrentOrder.Font = AppConfig.ContentFont;

            button_TotalDamage.Font = AppConfig.BoldHarmonyFont;
            button_TotalTreatment.Font = AppConfig.BoldHarmonyFont;
            button_AlwaysInjured.Font = AppConfig.BoldHarmonyFont;
            button_NpcTakeDamage.Font = AppConfig.BoldHarmonyFont;
        }

        private void SetStartupPositionAndSize()
        {
            var startupRect = AppConfig.StartUpState;
            if (startupRect != null && startupRect != Rectangle.Empty)
            {
                Left = startupRect.Value.Left;
                Top = startupRect.Value.Top;
                Width = startupRect.Value.Width;
                Height = startupRect.Value.Height;
            }
        }

        #region 钩子
        private KeyboardHook KbHook { get; } = new();
        public void RegisterKeyboardHook()
        {
            KbHook.SetHook();
            KbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { KbHook?.UnHook(); }
            catch (Exception ex) { Console.WriteLine($"窗体关闭清理时出错: {ex.Message}"); }
            base.OnFormClosed(e);
        }

        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == AppConfig.MouseThroughKey) { HandleMouseThrough(); }
            //else if (e.KeyData == AppConfig.FormTransparencyKey) { HandleFormTransparency(); }
            //else if (e.KeyData == AppConfig.WindowToggleKey) { }
            else if (e.KeyData == AppConfig.ClearDataKey) { HandleClearData(); }
            else if (e.KeyData == AppConfig.ClearHistoryKey)
            {
                StatisticData._manager.ClearSnapshots();//清空快照
                FullRecord.ClearSessionHistory();//清空全程快照

            }
        }

        private void HandleMouseThrough()
        {
            if (!MousePenetrationHelper.IsPenetrating(this.Handle))
            {
                // 方案 O：AppConfig.Transparency 现在表示“不透明度百分比”
                MousePenetrationHelper.SetMousePenetrate(this, enable: true, opacityPercent: AppConfig.Transparency);
            }
            else
            {
                MousePenetrationHelper.SetMousePenetrate(this, enable: false);
            }
        }

        // 配置变化时实时刷新不透明度（不改变穿透开关）
        private void ApplyOpacityFromConfig()
        {
            MousePenetrationHelper.UpdateOpacityPercent(this.Handle, AppConfig.Transparency);
        }


        bool hyaline = false;

        #endregion



        private void button_Minimum_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void DpsStatisticsForm_Shown(object sender, EventArgs e)
        {

        }

        private void EnsureTopMost()
        {
            TopMost = false;   // 先关再开，强制触发样式刷新
            TopMost = true;
            Activate();
            BringToFront();
            button_AlwaysOnTop.Toggle = TopMost; // 同步你的按钮状态
        }

        private void button_NpcTakeDamage_Click(object sender, EventArgs e)
        {
            ExitNpcDetailMode(); // 退出详情模式
            Button button = (Button)sender;
            List<Button> buttonList = new List<Button>() { button_TotalDamage, button_TotalTreatment, button_AlwaysInjured, button_NpcTakeDamage };
            Color colorBack = Color.FromArgb(60, 60, 60);
            Color colorWhite = Color.FromArgb(223, 223, 223);
            foreach (Button btn in buttonList)
            {
                if (btn.Name == button.Name)
                {
                    if (Config.IsLight)
                    {
                        btn.DefaultBack = colorWhite;
                    }
                    else
                    {
                        btn.DefaultBack = colorBack;
                    }

                }
                else
                {
                    if (Config.IsLight)
                    {
                        btn.DefaultBack = Color.FromArgb(247, 247, 247);
                    }
                    else
                    {
                        btn.DefaultBack = Color.FromArgb(27, 27, 27);
                    }

                }

            }

            switch (button.Name)
            {
                //总伤害
                case "TotalDamageButton":
                    FormManager.currentIndex = 0;
                    break;
                //总治疗
                case "TotalTreatmentButton":
                    FormManager.currentIndex = 1;
                    break;
                //总承伤
                case "AlwaysInjuredButton":
                    FormManager.currentIndex = 2;
                    break;
                //NPC承伤
                case "NpcTakeDamageButton":
                    FormManager.currentIndex = 3;
                    break;
            }

            UpdateHeaderText(); // 刷新顶部文本

        }

        private void DpsStatisticsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppConfig.StartUpState = new Rectangle(Left, Top, Width, Height);
        }

        /// <summary>
        /// 退出详情模式
        /// </summary>
        private void ExitNpcDetailMode()
        {
            _npcDetailMode = false;
            _npcFocusId = 0;
        }

        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight; // # 状态翻转：明/暗

            button_ThemeSwitch.Toggle = !AppConfig.IsLight; // # UI同步：按钮切换状态

            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.skillDiary, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.mainForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.skillDetailForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.settingsForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.dpsStatistics, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.rankingsForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.historicalBattlesForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.moduleCalculationForm, AppConfig.IsLight);//设置窗体颜色
        }
    }
}
