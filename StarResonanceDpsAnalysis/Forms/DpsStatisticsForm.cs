using AntdUI; // 引用 AntdUI 组件库（第三方 UI 控件/样式）
using StarResonanceDpsAnalysis.Control; // 引用项目内的 UI 控制/辅助类命名空间
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Forms.PopUp; // 引用弹窗相关窗体/组件命名空间
using StarResonanceDpsAnalysis.Plugin; // 引用项目插件层通用命名空间
using StarResonanceDpsAnalysis.Plugin.DamageStatistics; // 引用伤害统计插件命名空间（含 FullRecord、StatisticData 等）
using StarResonanceDpsAnalysis.Plugin.LaunchFunction; // 引用启动相关功能（加载技能配置等）
using StarResonanceDpsAnalysis.Properties; // 引用资源（图标/本地化字符串等）
using System.Threading.Tasks; // 引用异步任务支持（Task/async/await）
using System;
using System.Drawing;
using System.Windows.Forms;
using static StarResonanceDpsAnalysis.Control.SkillDetailForm;
using System.Security.Cryptography.Xml;

namespace StarResonanceDpsAnalysis.Forms // 定义命名空间：窗体相关代码所在位置
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
            // 构造函数开始
            InitializeComponent(); // 初始化设计器生成的控件与布局
            //开启默认置顶
            TopMost = !TopMost; // 简化切换
            button_AlwaysOnTop.Toggle = TopMost; // 同步按钮的视觉状态

            Text = FormManager.APP_NAME;

            FormGui.SetDefaultGUI(this); // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）

            //ApplyResolutionScale(); // 可选：根据屏幕分辨率对整体界面进行缩放（当前禁用，仅保留调用）

            SetDefaultFontFromResources();

            //加载钩子
            RegisterKeyboardHook(); // 安装键盘钩子，用于全局热键监听与处理

            //先加载基础配置
            InitTableColumnsConfigAtFirstRun(); // 首次运行初始化表格列配置（列宽/显示项等）

            //加载网卡
            LoadNetworkDevices(); // 加载/枚举网络设备（抓包设备列表）

            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色 // 根据配置设置窗体的颜色主题（明亮/深色）

            //加载技能配置
            StartupInitializer.LoadFromEmbeddedSkillConfig(); // 从内置资源读取并加载技能数据（元数据/图标/映射）
            sortedProgressBarList1.SelectionChanged += (s, i, d) => // 订阅进度条列表的选择变化事件（点击条目）
            { // 事件处理开始
                // # UI 列表交互：当用户点击列表项时触发（i 为索引，d 为 ProgressBarData）
                if (i < 0 || d == null) // 若未选中有效项或数据为空
                { // 条件分支开始
                    return; // 直接返回，不做任何处理
                } // 条件分支结束
                // # 将选中项的 UID 传入详情窗口刷新
                sortedProgressBarList_SelectionChanged((ulong)d.ID); // 将条目 ID 转为 UID 并调用详情刷新逻辑
            }; // 事件处理结束并解除与下一语句的关联

            SetStyle(); // 设置/应用本窗体的个性化样式（定义在同类/局部类的其他部分）

        } // 构造函数结束

        // # 分辨率缩放（可选）
        private void ApplyResolutionScale() // 将界面按主显示器分辨率进行缩放
        { // 方法开始
            float scale = GetPrimaryResolutionScale(); // 计算缩放比例（1.0/1.3333/2.0）
            if (Math.Abs(scale - 1.0f) < 0.01f) return; // 若接近 1.0（无需缩放）则直接返回

            this.Scale(new SizeF(scale, scale)); // 对窗体整体进行缩放

            try // 保护性尝试：某些控件属性在不同主题下可能抛异常
            { // try 开始
                pageHeader1.Font = new Font(pageHeader1.Font.FontFamily, pageHeader1.Font.Size * scale, pageHeader1.Font.Style);
                pageHeader1.SubFont = new Font(pageHeader1.SubFont.FontFamily, pageHeader1.SubFont.Size * scale, pageHeader1.SubFont.Style);

                BattleTimeText.Font = new Font(BattleTimeText.Font.FontFamily, BattleTimeText.Font.Size * scale, BattleTimeText.Font.Style);

                sortedProgressBarList1.ProgressBarHeight = (int)Math.Round(sortedProgressBarList1.ProgressBarHeight * scale);
                sortedProgressBarList1.ProgressBarPadding = new Padding(
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Left * scale),
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Top * scale),
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Right * scale),
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Bottom * scale)
                );
            } // try 结束
            catch { } // 忽略缩放过程中的非关键异常，保证 UI 不崩溃
        } // 方法结束

        // # 屏幕分辨率缩放判定
        private static float GetPrimaryResolutionScale() // 依据主屏高度返回推荐缩放比例
        { // 方法开始
            try // 防御：获取屏幕信息可能在某些环境异常
            { // try 开始
                var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080); // 获取主屏尺寸，失败则默认 1080p
                if (bounds.Height >= 2160) return 2.0f;       // 4K 屏：建议缩放 2.0
                if (bounds.Height >= 1440) return 1.3333f;    // 2K 屏：建议缩放 1.3333
                return 1.0f;                                   // 1080p：不缩放
            } // try 结束
            catch // 捕获任何异常
            { // catch 开始
                return 1.0f; // 异常时安全返回 1.0（不缩放）
            } // catch 结束
        } // 方法结束

        // # 窗体加载事件：启动抓包
        private void DpsStatistics_Load(object sender, EventArgs e) // 窗体 Load 事件处理
        { // 方法开始
            StartCapture(); // 启动网络抓包/数据采集（核心运行入口之一）
        } // 方法结束

        // # 列表选择变更 → 打开技能详情
        private void sortedProgressBarList_SelectionChanged(ulong uid) // 列表项选择回调：传入选中玩家 UID
        {  // 方法开始
            if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed)
            {
                FormManager.skillDetailForm = new SkillDetailForm(); // # 详情窗体：延迟创建
            }
            SkillTableDatas.SkillTable.Clear(); // # 清空旧详情数据（表格数据源重置）

            // 基础信息
            FormManager.skillDetailForm.Uid = uid; // 将当前选中 UID 传递给详情窗体
            var info = StatisticData._manager.GetPlayerBasicInfo(uid); // # 查询玩家基础信息（昵称/战力/职业）
            FormManager.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession); // 将基础信息写入详情窗体

            // ★ 关键：根据当前视图显式设定数据上下文，清掉快照时间，避免残留
            if (FormManager.showTotal) // 你全程视图的全局开关；如果有自己的判定就换成你的
            {
                FormManager.skillDetailForm.ContextType = DetailContextType.FullRecord; // 全程
                FormManager.skillDetailForm.SnapshotStartTime = null;
            }
            else
            {
                FormManager.skillDetailForm.ContextType = DetailContextType.Current;    // 单程（当前战斗）
                FormManager.skillDetailForm.SnapshotStartTime = null;
            }

            // 刷新 & 显示
            FormManager.skillDetailForm.SelectDataType(); // # 按当前选择的“伤害/治疗/承伤”类型刷新详情
            if (!FormManager.skillDetailForm.Visible) FormManager.skillDetailForm.Show(); else FormManager.skillDetailForm.Activate(); // # 显示/置顶
        } // 方法结束

        // # 顶部：置顶窗口按钮
        private void button_AlwaysOnTop_Click(object sender, EventArgs e) // 置顶按钮点击事件
        { // 方法开始
            TopMost = !TopMost; // 简化切换
            button_AlwaysOnTop.Toggle = TopMost; // 同步按钮的视觉状态
        } // 方法结束

        #region 切换显示类型（支持单次/全程伤害） // 折叠：视图标签与切换逻辑
        // # 统计视图标签：与 currentIndex 对应
        List<string> singleLabels = new() { "单次伤害", "单次治疗", "单次承伤" }; // 单次模式下三种标签
        List<string> totalLabels = new() { "全程伤害", "全程治疗", "全程承伤" }; // 全程模式下三种标签


        // # 头部标题文本刷新：依据 showTotal & currentIndex
        private void UpdateHeaderText() // 根据当前模式与索引更新顶部标签文本
        { // 方法开始
            DamageModeLabel.Text = FormManager.showTotal ? totalLabels[FormManager.currentIndex]
                                            : singleLabels[FormManager.currentIndex]; // 三元：选用全程/单次对应标题
        } // 方法结束

        // 左切换
        private void LeftHandoffButton_Click(object sender, EventArgs e) // 左切换按钮事件：currentIndex--
        { // 方法开始
            FormManager.currentIndex--; // 索引左移
            if (FormManager.currentIndex < 0) FormManager.currentIndex = singleLabels.Count - 1; // 下溢回绕至末尾
            UpdateHeaderText(); // 刷新顶部文本
        } // 方法结束

        // 右切换
        private void RightHandoffButton_Click(object sender, EventArgs e) // 右切换按钮事件：currentIndex++
        { // 方法开始
            FormManager.currentIndex++; // 索引右移
            if (FormManager.currentIndex >= singleLabels.Count) FormManager.currentIndex = 0; // 越界回绕至开头
            UpdateHeaderText(); // 刷新顶部文本
        } // 方法结束

        // 单次/全程切换
        private void button3_Click(object sender, EventArgs e) // 单次/全程切换按钮事件
        { // 方法开始
            FormManager.showTotal = !FormManager.showTotal; // 取反：在单次与全程之间切换
            UpdateHeaderText(); // 切换后刷新顶部文本
        } // 方法结束
        #endregion

        // # 定时刷新：战斗时长显示 + 榜单刷新
        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e) // 定时器：周期刷新（UI 绑定）
        { // 方法开始
            //var snap = FullRecord.GetEffectiveDurationString(); // 之前的调用示例（未使用）
            // ✅ 只调一次，按当前视图来
            var source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current; // 根据 showTotal 选择数据源
            var metric = FormManager.currentIndex switch // 根据 currentIndex 选择指标（伤害/治疗/承伤）
            { // switch 表达式开始
                1 => MetricType.Healing, // 索引 1 → 治疗
                2 => MetricType.Taken, // 索引 2 → 承伤
                _ => MetricType.Damage // 其他（默认 0）→ 伤害
            }; // switch 结束
            RefreshDpsTable(source, metric); // 刷新榜单数据（注意：内部有“视图闸门”校验）

            var duration = StatisticData._manager.GetFormattedCombatDuration(); // 获取当前战斗计时（格式化字符串）

            if (FormManager.showTotal) // 若当前是全程视图
            { // 分支开始
                // # 全程视图：
                duration = FullRecord.GetEffectiveDurationString(); // 使用全程计时字符串（去掉无效等待等）

            } // 分支结束

            BattleTimeText.Text = duration; // 将时长显示到 UI 文本
            //RefreshDpsTable(true); // 旧实现保留注释
        } // 方法结束

        /// <summary>
        /// 清空当前数据数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) // 清空按钮点击：触发清空逻辑
        { // 方法开始
            // # 清空：触发 HandleClearData（停止图表刷新→清空数据→重置图表）
            HandleClearData(); // 调用清空处理
        } // 方法结束


        // # 设置按钮 → 右键菜单
        private void button_Settings_Click(object sender, EventArgs e) // 设置按钮点击：弹出右键菜单
        {

            // 方法开始
            var menulist = new IContextMenuStripItem[] // 构建右键菜单项数组
             { // 数组开始
                    new ContextMenuStripItem("历史战斗") // 一级菜单：历史战斗
                    { // 配置开始
                        IconSvg = Resources.historicalRecords, // 图标
                    }, // 一级菜单配置结束
                    new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up}, // 一级菜单：基础设置
                    new ContextMenuStripItem("主窗体"){ IconSvg = Resources.HomeIcon, }, // 一级菜单：主窗体
                    //new ContextMenuStripItem("技能循环监测"), // 一级菜单：技能循环监测
                    //new ContextMenuStripItem(""){ IconSvg = Resources.userUid, }, // 示例：用户 UID（暂不用）
                    //new ContextMenuStripItem("统计排除"){ IconSvg = Resources.exclude, }, // 一级菜单：统计排除
                    new ContextMenuStripItem("伤害参考"){ IconSvg = Resources.reference, },
                    new ContextMenuStripItem("打桩模式"){ IconSvg = Resources.Stakes }, // 一级菜单：打桩模式
                    new ContextMenuStripItem("退出"){ IconSvg = Resources.quit, }, // 一级菜单：退出
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
                        { // 条件开始
                            FormManager.mainForm = new MainForm(); // 创建主窗体
                        } // 条件结束
                        FormManager.mainForm.Show(); // 显示主窗体
                        break; // 跳出 switch
                    case "技能循环监测": // 点击“技能循环监测”
                        if (FormManager.skillRotationMonitorForm == null || FormManager.skillRotationMonitorForm.IsDisposed) // 若监测窗体不存在或已释放
                        { // 条件开始
                            FormManager.skillRotationMonitorForm = new SkillRotationMonitorForm(); // 创建窗口
                        } // 条件结束
                        FormManager.skillRotationMonitorForm.Show(); // 显示窗口
                        //FormGui.SetColorMode(FormManager.skillRotationMonitorForm, AppConfig.IsLight); // 同步主题（明/暗）
                        break; // 跳出 switch
                    case "数据显示设置": // 点击“数据显示设置”（当前仅保留占位）
                        //dataDisplay(); 
                        break; // 占位：后续实现
                    case "伤害参考":
                        if (FormManager.rankingsForm == null || FormManager.rankingsForm.IsDisposed) // 若监测窗体不存在或已释放
                        { // 条件开始
                            FormManager.rankingsForm = new RankingsForm(); // 创建窗口
                        } // 条件结束
                        FormManager.rankingsForm.Show(); // 显示窗口
                        break;
                    case "统计排除": // 点击“统计排除”
                        break; // 占位：后续实现
                    case "打桩模式": // 点击“打桩模式”
                        PilingModeCheckbox.Visible = !PilingModeCheckbox.Visible;
                        break; // 跳出 switch
                    case "退出": // 点击“退出”
                        System.Windows.Forms.Application.Exit(); // 结束应用程序
                        break; // 跳出 switch
                } // switch 结束
            }, menulist); // 打开菜单并传入菜单项
        } // 方法结束

        /// <summary>
        /// 打开基础设置面板
        /// </summary>
        private void OpenSettingsDialog() // 打开基础设置窗体
        { // 方法开始
            if (FormManager.settingsForm == null || FormManager.settingsForm.IsDisposed) // 若设置窗体不存在或已释放
            { // 条件开始
                FormManager.settingsForm = new SettingsForm(); // 创建设置窗体
            } // 条件结束
            FormManager.settingsForm.Show(); // 显示设置窗体（或置顶）

        } // 方法结束

        // # 按钮提示气泡（置顶）
        private void button_AlwaysOnTop_MouseEnter(object sender, EventArgs e) // 鼠标进入置顶按钮时显示提示
        { // 方法开始
            ToolTip(button_AlwaysOnTop, "置顶窗口"); // 显示“置顶窗口”的气泡提示


        } // 方法结束

        // # 通用提示气泡工具
        private void ToolTip(System.Windows.Forms.Control control, string text) // 通用封装：在指定控件上显示提示文本
        { // 方法开始

            AntdUI.TooltipComponent tooltip = new AntdUI.TooltipComponent() // 创建 Tooltip 组件实例
            { // 对象初始化器开始
                Font = new Font("HarmonyOS Sans SC", 8, FontStyle.Regular), // 设置提示文字字体
            }; // 对象初始化器结束
            tooltip.ArrowAlign = AntdUI.TAlign.TL; // 设置箭头朝向/对齐方式
            tooltip.SetTip(control, text); // 在目标控件上显示指定文本提示
        } // 方法结束

        // # 按钮提示气泡（清空）
        private void button1_MouseEnter(object sender, EventArgs e) // 鼠标进入“清空”按钮时显示提示
        { // 方法开始
            ToolTip(button1, "清空当前数据"); // 显示“清空当前数据”的气泡提示
        } // 方法结束

        // # 按钮提示气泡（单次/全程切换）
        private void button3_MouseEnter(object sender, EventArgs e) // 鼠标进入“单次/全程切换”按钮时显示提示
        { // 方法开始
            ToolTip(button3, "点击切换：单次统计/全程统"); // 显示切换提示（原文如此，保留）
        } // 方法结束

        // 打桩模式定时逻辑
        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (PilingModeCheckbox.Checked)
            {
                if (string.IsNullOrWhiteSpace(AppConfig.NickName) || AppConfig.Uid == 0)
                {
                    PilingModeCheckbox.Checked = false;
                    timer1.Enabled = false;
                    var _ = AppMessageBox.ShowMessage("未获取到昵称或者UID，请换个地图后再进协会", this);
                    return;
                }
                TimeSpan duration = StatisticData._manager.GetCombatDuration();
                if (duration >= TimeSpan.FromMinutes(3))
                {
                    PilingModeCheckbox.Checked = false;
                    timer1.Enabled = false;

                    var snapshot = StatisticData._manager.TakeSnapshotAndGet();
                    var result = AppMessageBox.ShowMessage("打桩完成,是否上传(排行榜仅供娱乐，请勿恶意上传)\n1.如果对自己数据不满意可再次勾选打桩模式重新打桩", this);

                    if (result == DialogResult.OK)
                    {
                        bool data = await Common.AddUserDps(snapshot);
                        if (data)
                        {
                            AntdUI.Modal.open(new AntdUI.Modal.Config(this, "上传成功", "上传成功")
                            {
                                CloseIcon = true,
                                Keyboard = false,
                                MaskClosable = false,
                            });
                        }
                        else
                        {
                            AntdUI.Modal.open(new AntdUI.Modal.Config(this, "上传失败", "请检查网络状况，服务器暂时不支持外网上传")
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
        private void PilingModeCheckbox_CheckedChanged(object sender, BoolEventArgs e)
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
                    timer1.Enabled = true;
                }
                else
                {
                    PilingModeCheckbox.Checked = false;
                }
            }
            else
            {
                AppConfig.PilingMode = false;
                timer1.Enabled = false;
            }
        }

        // 主题切换
        private void DpsStatisticsForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                sortedProgressBarList1.BackColor = ColorTranslator.FromHtml("#E0E0E0");
            }
            else
            {
                sortedProgressBarList1.BackColor = ColorTranslator.FromHtml("#999999");
            }
        }

        private void SetDefaultFontFromResources()
        {
            DamageModeLabel.Font = AppConfig.HeaderFont;
            PilingModeCheckbox.Font = AppConfig.HeaderFont;
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
                StarResonanceDpsAnalysis.Plugin.DamageStatistics.FullRecord.ClearSessionHistory();//清空全程快照

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
        private void HandleFormTransparency()
        {
            if (hyaline)
            {
                Opacity = AppConfig.Transparency / 100;
                hyaline = false;
                Console.WriteLine($"切换到配置透明度: {AppConfig.Transparency}%)");
            }
            else
            {
                Opacity = 1.0;
                hyaline = true;
                Console.WriteLine($"切换到完全不透明: 100% (Opacity: 1.0)");
            }
        }
        #endregion



        private void button2_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
