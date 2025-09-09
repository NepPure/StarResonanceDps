using System;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;

using AntdUI;
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.Core.Extends.Data;
using StarResonanceDpsAnalysis.WinForm.Control;
using StarResonanceDpsAnalysis.WinForm.Control.GDI;
using StarResonanceDpsAnalysis.WinForm.Plugin;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.WinForm.Plugin.LaunchFunction;

using Button = AntdUI.Button;
using StarResonanceDpsAnalysis.Core.Analyze.Exceptions;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class DpsStatisticsForm : BorderlessForm
    {
        public DpsStatisticsForm()
        {
            InitializeComponent();

            // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）
            FormGui.SetDefaultGUI(this);
            //设置窗体颜色, 根据配置设置窗体的颜色主题（明亮/深色）
            FormGui.SetColorMode(this, AppConfig.IsLight);

            Text = FormManager.APP_NAME;

            // 从资源文件设置字体
            SetDefaultFontFromResources();

            // 安装键盘钩子，用于全局热键监听与处理
            RegisterKeyboardHook();

            // 首次运行初始化表格列配置（列宽/显示项等）
            InitTableColumnsConfigAtFirstRun();

            // 加载/枚举网络设备（抓包设备列表）
            LoadNetworkDevices();

            // 读取玩家信息缓存
            LoadPlayerCache();

            // 加载技能配置
            LoadFromEmbeddedSkillConfig(); // 从内置资源读取并加载技能数据（元数据/图标/映射）

            SetStyle(); // 设置/应用本窗体的个性化样式（定义在同类/局部类的其他部分）

            // 开始监听服务器变更事件
            DataStorage.ServerChanged += DataStorage_ServerChanged;

            // 开始监听DPS更新事件
            DataStorage.DpsDataUpdated += DataStorage_DpsDataUpdated;
        }

        #region 钩子
        private KeyboardHook KbHook { get; } = new();
        public void RegisterKeyboardHook()
        {
            KbHook.SetHook();
            KbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent;
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { KbHook?.UnHook(); }
            catch (Exception ex) { Console.WriteLine($"窗体关闭清理时出错: {ex.Message}"); }
            base.OnFormClosed(e);
        }

        #endregion

        private void DataStorage_ServerChanged(string currentServer, string prevServer)
        {
            DataStorage.ClearDpsData();
        }

        private readonly Dictionary<long, List<RenderContent>> _renderListDict = [];
        private void DataStorage_DpsDataUpdated()
        {
            var dpsList = DataStorage.ReadOnlySectionedDpsDataList;

            if (dpsList.Count == 0)
            {
                sortedProgressBarList_MainList.Data = [];
                return;
            }

            var dpsIEnum = dpsList.Where(e => !e.IsNpcData && e.TotalAttackDamage != 0);
            if (!dpsIEnum.Any())
            {
                sortedProgressBarList_MainList.Data = [];
                return;
            }

            // 正式使用时, 需要在此处判断当前类型(DPS/HPS/承伤)(全程/阶段)
            var maxValue = dpsIEnum.Max(e => e.TotalAttackDamage);
            var sumValue = dpsIEnum.Sum(e => e.TotalAttackDamage);

            var progressBarDataList = dpsIEnum
                .Select(e =>
                {
                    DataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(e.UID, out var playerInfo);
                    var professionName = playerInfo?.ProfessionID?.GetProfessionNameById() ?? string.Empty;

                    if (!_renderListDict.TryGetValue(e.UID, out var renderContent))
                    {
                        var profBmp = imgDict.TryGetValue(professionName, out var bmp) ? bmp : imgDict["未知"];
                        renderContent = BuildNewRenderContent(profBmp);
                        _renderListDict[e.UID] = renderContent;
                    }

                    renderContent[1].Text = $"{playerInfo?.Name}-{professionName}({e.UID})";
                    renderContent[2].Text = $"{e.TotalAttackDamage.ToCompactString()} ({(e.TotalAttackDamage / Math.Max(1, new TimeSpan(e.LastLoggedTick - (e.StartLoggedTick ?? 0)).TotalSeconds)).ToCompactString()})";
                    renderContent[3].Text = $"{Math.Round(100d * e.TotalAttackDamage / sumValue, 0, MidpointRounding.AwayFromZero)}%";

                    return new ProgressBarData()
                    {
                        ID = e.UID,
                        ProgressBarColor = GetProfessionColor(playerInfo?.ProfessionID ?? 0),
                        ProgressBarCornerRadius = 3,
                        ProgressBarValue = 1f * e.TotalAttackDamage / maxValue,
                        ContentList = renderContent
                    };
                }).ToList();

            sortedProgressBarList_MainList.Data = progressBarDataList;
        }

        private List<RenderContent> BuildNewRenderContent(Bitmap professionBmp)
        {
            return [
                new() { Type = RenderContent.ContentType.Image, Align = RenderContent.ContentAlign.MiddleLeft, Offset = AppConfig.ProgressBarImage, Image = professionBmp, ImageRenderSize = AppConfig.ProgressBarImageSize },
                new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleLeft, Offset = AppConfig.ProgressBarNmae, ForeColor = AppConfig.colorText, Font = AppConfig.ProgressBarFont },
                new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleRight, Offset = AppConfig.ProgressBarHarm, ForeColor = AppConfig.colorText, Font = AppConfig.ProgressBarFont },
                new() { Type = RenderContent.ContentType.Text, Align = RenderContent.ContentAlign.MiddleRight, Offset = AppConfig.ProgressBarProportion, ForeColor = AppConfig.colorText, Font = AppConfig.ProgressBarFont },
            ];
        }

        private Color GetProfessionColor(int professionID)
        {
            var map = Config.IsLight ? colorDict : blackColorDict;
            var professionName = professionID.GetProfessionNameById();
            var flag = map.TryGetValue(professionName, out var color);
            if (flag) return color;
            return map["未知"];
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
            {
                new ContextMenuStripItem("基础设置"){ IconSvg = HandledAssets.Set_Up }, // 一级菜单：基础设置
                new ContextMenuStripItem("主窗体"){ IconSvg = HandledAssets.HomeIcon }, // 一级菜单：主窗体
                new ContextMenuStripItem("退出"){ IconSvg = HandledAssets.Quit, }, // 一级菜单：退出
            };

            AntdUI.ContextMenuStrip.open(this, it => // 打开右键菜单并处理点击回调（it 为被点击项）
            {
                // 回调开始
                // # 菜单点击回调：根据 Text 执行对应动作
                switch (it.Text) // 分支根据菜单文本
                {
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

            var tooltip = new TooltipComponent() // 创建 Tooltip 组件实例
            {
                Font = new Font("HarmonyOS Sans SC", 8, FontStyle.Regular), // 设置提示文字字体
                ArrowAlign = AntdUI.TAlign.TL // 设置箭头朝向/对齐方式
            }; // 对象初始化器结束
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

        // 主题切换
        private void DpsStatisticsForm_ForeColorChanged(object sender, EventArgs e)
        {
            List<Button> buttonList = [button_TotalDamage, button_TotalTreatment, button_AlwaysInjured, button_NpcTakeDamage];

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
            Button button = (Button)sender;
            List<Button> buttonList = [button_TotalDamage, button_TotalTreatment, button_AlwaysInjured, button_NpcTakeDamage];
            Color colorBack = Color.FromArgb(60, 60, 60);
            Color colorWhite = Color.FromArgb(223, 223, 223);
            foreach (Button btn in buttonList)
            {
                btn.DefaultBack = btn.Name == button.Name
                    ? Config.IsLight ? colorWhite : colorBack
                    : Config.IsLight ? Color.FromArgb(247, 247, 247) : Color.FromArgb(27, 27, 27);
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

            DataStorage.DpsDataUpdated -= DataStorage_DpsDataUpdated;
            DataStorage.SavePlayerInfoToFile();
        }

        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight; // # 状态翻转：明/暗

            button_ThemeSwitch.Toggle = !AppConfig.IsLight; // # UI同步：按钮切换状态

            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.mainForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.settingsForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.dpsStatistics, AppConfig.IsLight);//设置窗体颜色
        }
    }
}
