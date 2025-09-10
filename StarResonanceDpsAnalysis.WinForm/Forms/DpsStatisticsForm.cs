using System;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;

using AntdUI;
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.Core.Extends.Data;
using StarResonanceDpsAnalysis.WinForm.Control;
using StarResonanceDpsAnalysis.WinForm.Control.GDI;
using StarResonanceDpsAnalysis.WinForm.Extends;
using StarResonanceDpsAnalysis.WinForm.Plugin;

using Button = AntdUI.Button;

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
            if (e.KeyData == AppConfig.MouseThroughKey)
            {
                HandleMouseThrough();
            }
            else if (e.KeyData == AppConfig.ClearDataKey)
            {
                HandleClearData();
            }
        }

        #endregion

        private void DataStorage_ServerChanged(string currentServer, string prevServer)
        {
            HandleClearData();
        }

        private readonly Dictionary<long, List<RenderContent>> _renderListDict = [];
        private void DataStorage_DpsDataUpdated()
        {
            var dpsList = _isShowFullData
                ? DataStorage.ReadOnlyFullDpsDataList
                : DataStorage.ReadOnlySectionedDpsDataList;

            if (dpsList.Count == 0)
            {
                sortedProgressBarList_MainList.Data = [];
                return;
            }

            var dpsIEnum = GetDefaultFilter(dpsList, _stasticsType);
            if (!dpsIEnum.Any())
            {
                sortedProgressBarList_MainList.Data = [];
                return;
            }

            (var maxValue, var sumValue) = GetMaxSumValueByType(dpsIEnum, _stasticsType);

            var progressBarDataList = dpsIEnum
                .Select(e =>
                {
                    DataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(e.UID, out var playerInfo);
                    var professionName = playerInfo?.ProfessionID?.GetProfessionNameById() ?? string.Empty;

                    if (!_renderListDict.TryGetValue(e.UID, out var renderContent))
                    {
                        var profBmp = professionName.GetProfessionBitmap();
                        renderContent = BuildNewRenderContent(profBmp);
                        _renderListDict[e.UID] = renderContent;
                    }

                    var value = GetValueByType(e, _stasticsType);

                    renderContent[1].Text = $"{playerInfo?.Name}-{professionName}({e.UID})";
                    renderContent[2].Text = $"{value.ToCompactString()} ({(value / Math.Max(1, new TimeSpan(e.LastLoggedTick - (e.StartLoggedTick ?? 0)).TotalSeconds)).ToCompactString()})";
                    renderContent[3].Text = $"{Math.Round(100d * value / sumValue, 0, MidpointRounding.AwayFromZero)}%";

                    return new ProgressBarData()
                    {
                        ID = e.UID,
                        ProgressBarColor = GetProfessionColor(playerInfo?.ProfessionID ?? 0),
                        ProgressBarCornerRadius = 3,
                        ProgressBarValue = (float)value / maxValue,
                        ContentList = renderContent
                    };
                }).ToList();

            sortedProgressBarList_MainList.Data = progressBarDataList;
        }

        /// <summary>
        /// 获取每个统计类别的默认筛选器
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<DpsData> GetDefaultFilter(IEnumerable<DpsData> list, int type)
        {
            return type switch
            {
                0 => list.Where(e => !e.IsNpcData && e.TotalAttackDamage != 0),
                1 => list.Where(e => !e.IsNpcData && e.TotalHeal != 0),
                2 => list.Where(e => !e.IsNpcData && e.TotalTakenDamage != 0),
                3 => list.Where(e => e.IsNpcData && e.TotalTakenDamage != 0),

                _ => list
            };
        }

        private (long max, long sum) GetMaxSumValueByType(IEnumerable<DpsData> list, int type)
        {
            return type switch
            {
                0 => (list.Max(e => e.TotalAttackDamage), list.Sum(e => e.TotalAttackDamage)),
                1 => (list.Max(e => e.TotalHeal), list.Sum(e => e.TotalHeal)),
                2 or 3 => (list.Max(e => e.TotalTakenDamage), list.Sum(e => e.TotalTakenDamage)),
                
                _ => (long.MaxValue, long.MaxValue)
            };
        }

        private long GetValueByType(DpsData data, int type) 
        {
            return type switch
            {
                0 => data.TotalAttackDamage,
                1 => data.TotalHeal,
                2 or 3 => data.TotalTakenDamage,

                _ => long.MaxValue
            };
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
            return professionID
                .GetProfessionNameById()
                .GetProfessionThemeColor(Config.IsLight);
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

        /// <summary>
        /// 获取当前统计类型 (伤害 / 治疗 / 承伤 / NPC承伤)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string StasticsTypeToName(int type)
        {
            return type switch
            {
                0 => "伤害",
                1 => "治疗",
                2 => "承伤",
                3 => "NPC承伤",

                _ => string.Empty
            };
        }

        /// <summary>
        /// 根据当前模式与索引更新顶部标签文本
        /// </summary>
        private void UpdateHeaderText()
        {
            pageHeader_MainHeader.SubText = $"{(_isShowFullData ? "全程" : "当前")} · {StasticsTypeToName(_stasticsType)}";
        }



        /// <summary>
        /// 单次 / 全程切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_SwitchStatisticsMode_Click(object sender, EventArgs e) // 单次/全程切换按钮事件
        {
            _isShowFullData = !_isShowFullData;

            // 更新标题状态副文本
            UpdateHeaderText();
        }
        #endregion

        /// <summary>
        /// 清空当前数据数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_RefreshDps_Click(object sender, EventArgs e) // 清空按钮点击：触发清空逻辑
        {
            HandleClearData();
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

            AntdUI.ContextMenuStrip.open(this, it =>
            {
                // 根据 Text 执行对应动作
                switch (it.Text)
                {
                    case "基础设置":
                        // 打开设置面板
                        OpenSettingsDialog();
                        break;

                    case "主窗体":
                        FormManager.MainForm.Show();
                        break;

                    case "退出":
                        Application.Exit();
                        break;
                }
            }, menulist);
        }

        /// <summary>
        /// 打开基础设置面板
        /// </summary>
        private void OpenSettingsDialog()
        {
            FormManager.SettingsForm.Show();
        }

        /// <summary>
        /// 按钮提示气泡 (置顶)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_AlwaysOnTop_MouseEnter(object sender, EventArgs e)
        {
            // 显示 "置顶窗口" 的气泡提示
            ToolTip(button_AlwaysOnTop, "置顶窗口");
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

        /// <summary>
        /// 主题切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DpsStatisticsForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                ChangeToLightTheme();
            }
            else
            {
                ChangeToDarkTheme();
            }

            SetSortedProgressBarListForeColor();
        }

        private List<Button> _stasticsTypeButtons => [button_TotalDamage, button_TotalTreatment, button_AlwaysInjured, button_NpcTakeDamage];
        private void ChangeToLightTheme()
        {
            AppConfig.colorText = Color.Black;

            sortedProgressBarList_MainList.BackColor = ColorTranslator.FromHtml("#F5F5F5");
            sortedProgressBarList_MainList.OrderColor = Color.Black;

            panel_Footer.Back = ColorTranslator.FromHtml("#F5F5F5");
            panel_ModeBox.Back = ColorTranslator.FromHtml("#F5F5F5");

            button_TotalDamage.Icon = HandledAssets.伤害;
            button_TotalTreatment.Icon = HandledAssets.治疗;
            button_AlwaysInjured.Icon = HandledAssets.承伤;
            button_NpcTakeDamage.Icon = HandledAssets.Npc;

            foreach (var item in _stasticsTypeButtons)
            {
                item.DefaultBack = Color.FromArgb(247, 247, 247);
            }
            _stasticsTypeButtons[_stasticsType].DefaultBack = Color.FromArgb(223, 223, 223);
        }

        private void ChangeToDarkTheme()
        {
            AppConfig.colorText = Color.White;
            sortedProgressBarList_MainList.BackColor = ColorTranslator.FromHtml("#252527");
            sortedProgressBarList_MainList.OrderColor = Color.White;

            panel_Footer.Back = ColorTranslator.FromHtml("#252527");
            panel_ModeBox.Back = ColorTranslator.FromHtml("#252527");

            button_TotalDamage.Icon = HandledAssets.伤害白色;
            button_TotalTreatment.Icon = HandledAssets.治疗白色;
            button_AlwaysInjured.Icon = HandledAssets.承伤白色;
            button_NpcTakeDamage.Icon = HandledAssets.NpcWhite;

            foreach (var item in _stasticsTypeButtons)
            {
                item.DefaultBack = Color.FromArgb(27, 27, 27);
            }
            _stasticsTypeButtons[_stasticsType].DefaultBack = Color.FromArgb(60, 60, 60);
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
                        if (content.Type != RenderContent.ContentType.Text) continue;

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
            // 先关再开, 强制触发样式刷新
            TopMost = false;
            TopMost = true;

            Activate();
            BringToFront();

            // 同步按钮状态
            button_AlwaysOnTop.Toggle = TopMost;
        }

        private void TypeButtons_Click(object sender, EventArgs e)
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

            _stasticsType = button.Tag.ToInt();

            // 刷新顶部文本
            UpdateHeaderText();
            // 刷新表单数据
            DataStorage_DpsDataUpdated();
        }

        private void DpsStatisticsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppConfig.StartUpState = new Rectangle(Left, Top, Width, Height);

            DataStorage.DpsDataUpdated -= DataStorage_DpsDataUpdated;
            DataStorage.SavePlayerInfoToFile();

            try { KbHook?.UnHook(); }
            catch (Exception ex) { Console.WriteLine($"窗体关闭清理时出错: {ex.Message}"); }
        }

        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            // # 状态翻转：明/暗
            AppConfig.IsLight = !AppConfig.IsLight;

            button_ThemeSwitch.Toggle = !AppConfig.IsLight; // # UI同步：按钮切换状态

            // 通知其他窗口更新主题
            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.MainForm, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.SettingsForm, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.DpsStatistics, AppConfig.IsLight);
        }
    }
}
