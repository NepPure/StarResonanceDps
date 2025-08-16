using AntdUI; // 引用 AntdUI 组件库（第三方 UI 控件/样式）
using StarResonanceDpsAnalysis.Control; // 引用项目内的 UI 控制/辅助类命名空间
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Forms.PopUp; // 引用弹窗相关窗体/组件命名空间
using StarResonanceDpsAnalysis.Plugin; // 引用项目插件层通用命名空间
using StarResonanceDpsAnalysis.Plugin.DamageStatistics; // 引用伤害统计插件命名空间（含 FullRecord、StatisticData 等）
using StarResonanceDpsAnalysis.Plugin.LaunchFunction; // 引用启动相关功能（加载技能配置等）
using StarResonanceDpsAnalysis.Properties; // 引用资源（图标/本地化字符串等）
using System.Runtime.InteropServices; // 引用互操作（Win32 API 等）命名空间
using System.Threading.Tasks; // 引用异步任务支持（Task/async/await）
using static System.Windows.Forms.VisualStyles.VisualStyleElement; // 引用 VisualStyleElement 静态成员，便于直接使用

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

        // # 重要提醒
        // #   * 按你的要求，此版本仅添加注释与分类标识，不改动任何可执行代码。

        // # 构造与启动流程
        public DpsStatisticsForm() // 构造函数：创建窗体实例时执行一次
        { 
            // 构造函数开始
            InitializeComponent(); // 初始化设计器生成的控件与布局

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
                    // Console.WriteLine("Nothing Clicked."); // 调试输出（当前注释掉）
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
            // 仅针对 Designer 初始尺寸进行一次整体缩放，使在 2K/4K 上更合适
            float scale = GetPrimaryResolutionScale(); // 计算缩放比例（1.0/1.3333/2.0）
            if (Math.Abs(scale - 1.0f) < 0.01f) return; // 若接近 1.0（无需缩放）则直接返回

            // 缩放窗体和控件
            this.Scale(new SizeF(scale, scale)); // 对窗体整体进行缩放

            // 调整一些固定高度/字体
            try // 保护性尝试：某些控件属性在不同主题下可能抛异常
            { // try 开始
                pageHeader1.Font = new Font(pageHeader1.Font.FontFamily, pageHeader1.Font.Size * scale, pageHeader1.Font.Style); // 按比例放大标题字体
                pageHeader1.SubFont = new Font(pageHeader1.SubFont.FontFamily, pageHeader1.SubFont.Size * scale, pageHeader1.SubFont.Style); // 放大副标题字体

                //textProgressBar1.Font = new Font(textProgressBar1.Font.FontFamily, textProgressBar1.Font.Size * scale, textProgressBar1.Font.Style); // 示例：如需也缩放文字进度条
                BattleTimeText.Font = new Font(BattleTimeText.Font.FontFamily, BattleTimeText.Font.Size * scale, BattleTimeText.Font.Style); // 放大战斗时间字体

                // 调整自定义控件的高度等参数
                sortedProgressBarList1.ProgressBarHeight = (int)Math.Round(sortedProgressBarList1.ProgressBarHeight * scale); // 进度条高度随比例缩放
                sortedProgressBarList1.ProgressBarPadding = new Padding( // 按比例缩放进度条内边距
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Left * scale), // 左内边距
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Top * scale), // 上内边距
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Right * scale), // 右内边距
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Bottom * scale) // 下内边距
                ); // 内边距设置结束

                //textProgressBar1.ProgressBarCornerRadius = (int)Math.Round(textProgressBar1.ProgressBarCornerRadius * scale); // 如需：圆角半径缩放
                //textProgressBar1.Padding = new Padding( // 如需：进度条控件整体内边距缩放
                // (int)Math.Round(textProgressBar1.Padding.Left * scale),
                //  (int)Math.Round(textProgressBar1.Padding.Top * scale),
                // (int)Math.Round(textProgressBar1.Padding.Right * scale),
                // (int)Math.Round(textProgressBar1.Padding.Bottom * scale)
                //);
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
            //开启DPS统计
            StartCapture(); // 启动网络抓包/数据采集（核心运行入口之一）
        } // 方法结束

        // # 列表选择变更 → 打开技能详情
        private void sortedProgressBarList_SelectionChanged(ulong uid) // 列表项选择回调：传入选中玩家 UID
        { // 方法开始


            if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed) // 若详情窗体未创建或已被释放
            { // 条件开始
                FormManager.skillDetailForm = new SkillDetailForm(); // # 详情窗体：延迟创建
            } // 条件结束
            SkillTableDatas.SkillTable.Clear(); // # 清空旧详情数据（表格数据源重置）

            FormManager.skillDetailForm.Uid = uid; // 将当前选中 UID 传递给详情窗体
            //获取玩家信息
            var info = StatisticData._manager.GetPlayerBasicInfo(uid); // # 查询玩家基础信息（昵称/战力/职业）
            FormManager.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession); // 将基础信息写入详情窗体
            FormManager.skillDetailForm.SelectDataType(); // # 按当前选择的“伤害/治疗/承伤”类型刷新详情
            FormManager.skillDetailForm.Show(); // # 显示详情窗体（若已显示则置顶）
        } // 方法结束

        // # 顶部：置顶窗口按钮
        private void button_AlwaysOnTop_Click(object sender, EventArgs e) // 置顶按钮点击事件
        { // 方法开始
            if (this.TopMost) // 若当前已置顶
            { // 分支开始
                this.TopMost = false; // 取消置顶
                button_AlwaysOnTop.Toggle = false; // 同步按钮的视觉状态
            } // 分支结束
            else // 否则：未置顶
            { // 分支开始
                this.TopMost = true; // 开启置顶
                button_AlwaysOnTop.Toggle = true; // 同步按钮的视觉状态

            } // 分支结束
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
        { // 方法开始
            var menulist = new IContextMenuStripItem[] // 构建右键菜单项数组
             { // 数组开始
                    new ContextMenuStripItem("历史战斗") // 一级菜单：历史战斗
                    { // 配置开始
                        IconSvg = Resources.historicalRecords, // 设置图标（资源）
                        Sub = new IContextMenuStripItem[] // 子菜单集合
                        { // 子菜单数组开始
                            new ContextMenuStripItem("战斗记录") // 子项：战斗记录
                            {


                            }, // 子项配置结束（此处行为待实现）
                        } // 子菜单数组结束
                    }, // 一级菜单配置结束
                    new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up}, // 一级菜单：基础设置
                    new ContextMenuStripItem("主窗体"){ IconSvg = Resources.HomeIcon, }, // 一级菜单：主窗体
                    new ContextMenuStripItem("技能循环监测"), // 一级菜单：技能循环监测
                    //new ContextMenuStripItem(""){ IconSvg = Resources.userUid, }, // 示例：用户 UID（暂不用）
                    new ContextMenuStripItem("统计排除"){ IconSvg = Resources.exclude, }, // 一级菜单：统计排除
                    new ContextMenuStripItem("打桩模式"){ IconSvg = Resources.Stakes }, // 一级菜单：打桩模式
                    new ContextMenuStripItem("退出"){ IconSvg = Resources.quit, }, // 一级菜单：退出
             } // 数组结束
            ; // 语句结束（分号保持）

            AntdUI.ContextMenuStrip.open(this, it => // 打开右键菜单并处理点击回调（it 为被点击项）
            { // 回调开始
                // # 菜单点击回调：根据 Text 执行对应动作
                switch (it.Text) // 分支根据菜单文本
                { // switch 开始
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
                        FormGui.SetColorMode(FormManager.skillRotationMonitorForm, AppConfig.IsLight); // 同步主题（明/暗）
                        break; // 跳出 switch
                    case "数据显示设置": // 点击“数据显示设置”（当前仅保留占位）
                        //dataDisplay(); 
                        break; // 占位：后续实现
                    case "统计排除": // 点击“统计排除”
                        break; // 占位：后续实现
                    case "打桩模式": // 点击“打桩模式”
                        if (PilingModeCheckbox.Visible) // 若复选框已可见
                        { // 条件开始
                            PilingModeCheckbox.Visible = false; // 设为隐藏
                        } // 条件结束
                        else // 否则
                        { // 条件开始
                            PilingModeCheckbox.Visible = true; // 设为可见
                        } // 条件结束

                        break; // 跳出 switch
                    case "退出": // 点击“退出”
                        Application.Exit(); // 结束应用程序
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


        // # 打桩模式：周期检查（1s）
        private async void timer1_Tick(object sender, EventArgs e) // 定时器回调：打桩模式的周期性检查
        { // 方法开始
            if (PilingModeCheckbox.Checked) // 若勾选了打桩模式
            { // 条件开始
                // # 预校验：需要昵称与 UID 才能进行打桩
                if (string.IsNullOrWhiteSpace(AppConfig.NickName) || AppConfig.Uid == 0) // 若昵称或 UID 无效
                { // 条件开始
                    PilingModeCheckbox.Checked = false; // 自动取消勾选
                    timer1.Enabled = false; // 关闭定时器
                    var result = AppMessageBox.ShowMessage("未获取到昵称或者UID，请换个地图后再进协会", this); // 弹窗提示用户

                    return; // 终止本次回调
                } // 条件结束
                // # 达到 1 分钟：自动结束一次打桩（你在提示里说 3 分钟，这里逻辑为 >1 分钟，保持原样）
                TimeSpan duration = StatisticData._manager.GetCombatDuration();//获取时间 // 获取当前战斗持续时间
                if (duration >= TimeSpan.FromMinutes(3)) // 3分钟及以上为打桩 // 若达到/超过 3 分钟
                { // 条件开始
                    //暂停打桩模式
                    PilingModeCheckbox.Checked = false; // 取消勾选打桩
                    timer1.Enabled = false; // 停止定时器
                    // 这里可以写你的其它逻辑

                    var snapshot = StatisticData._manager.TakeSnapshotAndGet();//获取快照 // 生成/获取当前快照
                    var result = AppMessageBox.ShowMessage("打桩完成,是否上传(排行榜仅供娱乐，请勿恶意上传)\n1.如果对自己数据不满意可再次勾选打桩模式重新打桩", this); // 询问是否上传

                    if (result == DialogResult.OK) // 用户选择“确定”
                    { // 条件开始
                        bool data = await Common.AddUserDps(snapshot); // 调用上传接口（异步）
                        if (data) // 上传成功
                        { // 分支开始
                            AntdUI.Modal.open(new AntdUI.Modal.Config(this, "上传成功", "上传成功") // 打开成功提示框
                            {
                                CloseIcon = true, // 显示关闭图标
                                Keyboard = false, // 禁用键盘关闭
                                MaskClosable = false, // 禁用点击遮罩关闭
                            }); // 模态框配置结束
                        } // 分支结束
                        else // 上传失败
                        { // 分支开始
                            AntdUI.Modal.open(new AntdUI.Modal.Config(this, "上传失败", "请检查网络状况，服务器暂时不支持外网上传") // 打开失败提示框
                            {
                                CloseIcon = true, // 显示关闭图标
                                Keyboard = false, // 禁用键盘关闭
                                MaskClosable = false, // 禁用点击遮罩关闭
                            }); // 模态框配置结束
                        } // 分支结束
                    } // 条件结束
                    else // 用户取消上传
                    { // 分支开始


                    } // 分支结束



                } // 条件结束
            } // 条件结束
        } // 方法结束

        // # 打桩模式：启停控制（CheckBox）
        private void PilingModeCheckbox_CheckedChanged(object sender, BoolEventArgs e) // 勾选框状态改变事件
        { // 方法开始
            TimeSpan duration = StatisticData._manager.GetCombatDuration();//获取时间 // 获取当前战斗持续时间（此处仅示例，不参与判断）

            if (e.Value) // 若本次变更为“勾选”（开启打桩）
            { // 条件开始

                var result = AppMessageBox.ShowMessage("打桩时间为3分钟，需注意以下3点:\n0.:打桩模式开启后只会记录自己的数据\n1.开启后请找协会内最右侧木桩[靠窗的那根]\n2.确保战斗计时为0开启\n3.如果伤害不满意可关闭打桩模式重新勾选\n4.异常数据会被删除\n", this); // 显示打桩须知
                if (result == DialogResult.OK) // 用户确认开启
                { // 条件开始
                    DpsTableDatas.DpsTable.Clear(); // 清空 DPS 表数据
                    StatisticData._manager.ClearAll(); // 清空核心统计缓存
                    SkillTableDatas.SkillTable.Clear(); // 清空技能详情表
                    Task.Delay(200); // 延时 200ms（未 await，表示“放手给线程池”，原样保留）
                    //打桩模式启动
                    AppConfig.PilingMode = true; // 设置全局配置为“打桩模式”
                    timer1.Enabled = true; // 启用打桩定时器
                } // 条件结束
                else // 用户取消开启
                { // 分支开始
                    // 用户关闭或取消

                    PilingModeCheckbox.Checked = false; // 反选复选框，保持未开启
                } // 分支结束

            } // 条件结束
            else // 若本次变更为“取消勾选”（关闭打桩）
            { // 分支开始
                AppConfig.PilingMode = false; // 关闭全局打桩标志
                //打桩模式关闭
                timer1.Enabled = false; // 停止打桩定时器
            } // 分支结束
        } // 方法结束

        // # 主题切换：前景色变化时适配控件背景
        private void DpsStatisticsForm_ForeColorChanged(object sender, EventArgs e) // ForeColor 改变事件
        { // 方法开始
            if (Config.IsLight) // 若当前为浅色模式
            { // 分支开始
                //浅色

                sortedProgressBarList1.BackColor = ColorTranslator.FromHtml("#E0E0E0"); // 进度条列表背景设置为浅灰
                //textProgressBar1.BackColor = ColorTranslator.FromHtml("#FFFFFF"); // 示例：另一个控件背景（保留注释）

            } // 分支结束
            else // 深色模式
            { // 分支开始
                //深色
                sortedProgressBarList1.BackColor = ColorTranslator.FromHtml("#999999"); // 进度条列表背景设置为中灰
                //textProgressBar1.BackColor = ColorTranslator.FromHtml("#000000"); // 示例：另一个控件背景（保留注释）

            } // 分支结束
        } // 方法结束

        private void SetDefaultFontFromResources() 
        {
            if (FontLoader.TryLoadFontFromBytes("AlimamaShuHeiTi", Resources.AlimamaShuHeiTi, 9, out var font))
            {
                DamageModeLabel.Font = font;
            }
        }

        #region 钩子 // 折叠：全局键盘钩子安装/卸载与热键路由
        /// <summary>
        /// 键盘钩子
        /// </summary>
        private KeyboardHook KbHook { get; } = new(); // # 全局输入：全局热键钩子，用于响应窗口控制/穿透/清空等快捷键
        public void RegisterKeyboardHook() // 安装并注册键盘钩子
        { // 方法开始
            // 键盘钩子初始化
            KbHook.SetHook(); // # 全局输入：安装键盘钩子
            KbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent; // # 热键绑定：统一在此监听
        } // 方法结束

        /// <summary>
        /// 窗体关闭时的清理工作
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e) // 窗体关闭事件：资源清理
        { // 方法开始
            try // 尝试卸载钩子，避免异常导致句柄泄漏
            { // try 开始
                // 释放键盘钩子
                KbHook?.UnHook(); // # 全局输入：卸载键盘钩子，避免句柄泄漏
            } // try 结束
            catch (Exception ex) // 捕获卸载过程中的异常
            { // catch 开始
                Console.WriteLine($"窗体关闭清理时出错: {ex.Message}"); // 输出异常信息（调试用）
            } // catch 结束

            base.OnFormClosed(e); // 调用父类实现，完成标准关闭流程
        } // 方法结束

        #region —— 全局热键 ——  // 折叠：热键处理分发

        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e) // 键盘按下事件回调（全局）
        { // 方法开始
            // # 将按键与配置的功能键匹配，解耦具体键位
            if (e.KeyData == AppConfig.MouseThroughKey) { HandleMouseThrough(); } // # 切换鼠标穿透
            else if (e.KeyData == AppConfig.FormTransparencyKey) { HandleFormTransparency(); } // # 切换窗体透明度
            else if (e.KeyData == AppConfig.WindowToggleKey) { } // # 开关监控/窗口（占位）
            else if (e.KeyData == AppConfig.ClearDataKey) { HandleClearData(); } // # 清空当前统计
            else if (e.KeyData == AppConfig.ClearHistoryKey) { }//等待重写实现 // # 预留：清空历史
        } // 方法结束

        #endregion

        #region HandleMouseThrough() 响应鼠标穿透 // 折叠：鼠标穿透开关
        private void HandleMouseThrough() // 切换窗体是否“鼠标穿透”（不接收点击）
        { // 方法开始

            // 判断当前窗体是否在穿透模式
            if (!MousePenetrationHelper.IsPenetrating(this.Handle)) // 若当前不是穿透模式
            { // 分支开始
                // 当前不是穿透模式 → 开启穿透
                MousePenetrationHelper.SetMousePenetrate(this, enable: true, alpha: 230); // 设置为穿透，顺带调整透明度
                Opacity = AppConfig.Transparency; // 将窗体不透明度设为配置值（注意：此处取值区间依实现）
            } // 分支结束
            else // 当前是穿透模式
            { // 分支开始
                // 当前是穿透模式 → 关闭穿透
                MousePenetrationHelper.SetMousePenetrate(this, enable: false); // 关闭穿透
                Opacity = 1.0; // 复原完全不透明
            } // 分支结束

        } // 方法结束

        #endregion


        #region HandleFormTransparency() 响应窗体透明 // 折叠：窗体透明度切换

        /// <summary>
        /// 是否开启透明
        /// </summary>
        bool hyaline = false; // 标记当前是否处于“完全不透明”状态的反相：true 表示刚切到 1.0，不用配置透明

        private void HandleFormTransparency() // 在“完全不透明(1.0)”与“配置透明度”之间切换
        { // 方法开始


            if (hyaline) // 若当前标记为 true（处于完全不透明）
            { // 分支开始
                // 当前是透明状态（1.0），要切换到配置透明度

                Opacity = AppConfig.Transparency / 100; // 将窗体透明度设置为配置值（百分比→0-1）
                hyaline = false; // 更新标志：进入“配置透明度”状态
                Console.WriteLine($"切换到配置透明度: {AppConfig.Transparency}%)"); // 控制台提示（多了一个括号，保留原文）
            } // 分支结束
            else // 当前不是完全不透明（处于配置透明）
            { // 分支开始
                // 当前是配置透明度，要切换到完全不透明（1.0）
                Opacity = 1.0; // 设为完全不透明
                hyaline = true; // 更新标志：进入“完全不透明”状态
                Console.WriteLine($"切换到完全不透明: 100% (Opacity: 1.0)"); // 控制台提示
            } // 分支结束
        } // 方法结束

        #endregion
        #endregion
    } // 类结束
} // 命名空间结束
