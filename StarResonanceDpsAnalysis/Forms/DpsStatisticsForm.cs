using AntdUI;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Forms.PopUp;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Plugin.LaunchFunction;
using StarResonanceDpsAnalysis.Properties;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatisticsForm : BorderlessForm
    {
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
        public DpsStatisticsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            //ApplyResolutionScale();
            //加载钩子
            RegisterKeyboardHook();
            //先加载基础配置
            InitTableColumnsConfigAtFirstRun();
            //加载网卡
            LoadNetworkDevices();
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
            //加载技能配置
            StartupInitializer.LoadFromEmbeddedSkillConfig();
            sortedProgressBarList1.SelectionChanged += (s, i, d) =>
            {
                // # UI 列表交互：当用户点击列表项时触发（i 为索引，d 为 ProgressBarData）
                if (i < 0 || d == null)
                {
                    // Console.WriteLine("Nothing Clicked.");
                    return;
                }
                // # 将选中项的 UID 传入详情窗口刷新
                sortedProgressBarList_SelectionChanged((ulong)d.ID);



            };
            SetStyle();
            new TestForm().Show(); // # 调试/测试窗体：启动即显示

        }

        // # 分辨率缩放（可选）
        private void ApplyResolutionScale()
        {
            // 仅针对 Designer 初始尺寸进行一次整体缩放，使在 2K/4K 上更合适
            float scale = GetPrimaryResolutionScale();
            if (Math.Abs(scale - 1.0f) < 0.01f) return;

            // 缩放窗体和控件
            this.Scale(new SizeF(scale, scale));

            // 调整一些固定高度/字体
            try
            {
                pageHeader1.Font = new Font(pageHeader1.Font.FontFamily, pageHeader1.Font.Size * scale, pageHeader1.Font.Style);
                pageHeader1.SubFont = new Font(pageHeader1.SubFont.FontFamily, pageHeader1.SubFont.Size * scale, pageHeader1.SubFont.Style);

                //textProgressBar1.Font = new Font(textProgressBar1.Font.FontFamily, textProgressBar1.Font.Size * scale, textProgressBar1.Font.Style);
                BattleTimeText.Font = new Font(BattleTimeText.Font.FontFamily, BattleTimeText.Font.Size * scale, BattleTimeText.Font.Style);

                // 调整自定义控件的高度等参数
                sortedProgressBarList1.ProgressBarHeight = (int)Math.Round(sortedProgressBarList1.ProgressBarHeight * scale);
                sortedProgressBarList1.ProgressBarPadding = new Padding(
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Left * scale),
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Top * scale),
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Right * scale),
                    (int)Math.Round(sortedProgressBarList1.ProgressBarPadding.Bottom * scale)
                );

                //textProgressBar1.ProgressBarCornerRadius = (int)Math.Round(textProgressBar1.ProgressBarCornerRadius * scale);
                //textProgressBar1.Padding = new Padding(
                // (int)Math.Round(textProgressBar1.Padding.Left * scale),
                //  (int)Math.Round(textProgressBar1.Padding.Top * scale),
                // (int)Math.Round(textProgressBar1.Padding.Right * scale),
                // (int)Math.Round(textProgressBar1.Padding.Bottom * scale)
                //);
            }
            catch { }
        }

        // # 屏幕分辨率缩放判定
        private static float GetPrimaryResolutionScale()
        {
            try
            {
                var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
                if (bounds.Height >= 2160) return 2.0f;       // 4K
                if (bounds.Height >= 1440) return 1.3333f;    // 2K
                return 1.0f;                                   // 1080p
            }
            catch
            {
                return 1.0f;
            }
        }

        // # 窗体加载事件：启动抓包
        private void DpsStatistics_Load(object sender, EventArgs e)
        {
            //开启DPS统计
            StartCapture();
        }

        // # 列表选择变更 → 打开技能详情
        private void sortedProgressBarList_SelectionChanged(ulong uid)
        {


            if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed)
            {
                FormManager.skillDetailForm = new SkillDetailForm(); // # 详情窗体：延迟创建
            }
            SkillTableDatas.SkillTable.Clear(); // # 清空旧详情数据

            FormManager.skillDetailForm.Uid = uid;
            //获取玩家信息
            var info = StatisticData._manager.GetPlayerBasicInfo(uid); // # 查询玩家基础信息（昵称/战力/职业）
            FormManager.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession);
            FormManager.skillDetailForm.SelectDataType(); // # 按当前选择的“伤害/治疗/承伤”类型刷新
            FormManager.skillDetailForm.Show(); // # 显示详情
        }

        // # 顶部：置顶窗口按钮
        private void button_AlwaysOnTop_Click(object sender, EventArgs e)
        {
            if (this.TopMost)
            {
                this.TopMost = false;
                button_AlwaysOnTop.Toggle = false;
            }
            else
            {
                this.TopMost = true;
                button_AlwaysOnTop.Toggle = true;

            }
        }

        #region 切换显示类型（支持单次/全程伤害）
        // # 统计视图标签：与 currentIndex 对应
        List<string> singleLabels = new() { "单次伤害", "单次治疗", "单次承伤" };
        List<string> totalLabels = new() { "全程伤害", "全程治疗", "全程承伤" };


        // # 头部标题文本刷新：依据 showTotal & currentIndex
        private void UpdateHeaderText()
        {
            DamageModeLabel.Text = FormManager.showTotal ? totalLabels[FormManager.currentIndex]
                                            : singleLabels[FormManager.currentIndex];
        }

        // 左切换
        private void LeftHandoffButton_Click(object sender, EventArgs e)
        {
            FormManager.currentIndex--;
            if (FormManager.currentIndex < 0) FormManager.currentIndex = singleLabels.Count - 1;
            UpdateHeaderText();
        }

        // 右切换
        private void RightHandoffButton_Click(object sender, EventArgs e)
        {
            FormManager.currentIndex++;
            if (FormManager.currentIndex >= singleLabels.Count) FormManager.currentIndex = 0;
            UpdateHeaderText();
        }

        // 单次/全程切换
        private void button3_Click(object sender, EventArgs e)
        {
            FormManager.showTotal = !FormManager.showTotal;
            UpdateHeaderText();
        }
        #endregion

        // # 定时刷新：战斗时长显示 + 榜单刷新
        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e)
        {
            //var snap = FullRecord.GetEffectiveDurationString();

            var duration = StatisticData._manager.GetFormattedCombatDuration();

            if (FormManager.showTotal)
            {
                // # 全程视图：
                duration = FullRecord.GetEffectiveDurationString();
                switch (FormManager.currentIndex)
                {
                    case 0:
                        // 当前-伤害
                        RefreshDpsTable(SourceType.FullRecord, MetricType.Damage);
                        break;
                    case 1:
                        // 当前-治疗
                        RefreshDpsTable(SourceType.FullRecord, MetricType.Healing);
                        break;
                    case 2:
                        // 当前-承伤
                        RefreshDpsTable(SourceType.FullRecord, MetricType.Taken);
                        break;

                }
            }
            else
            {
                // # 单次视图：
                switch (FormManager.currentIndex)
                {
                    case 0:
                        // 当前-伤害
                        RefreshDpsTable(SourceType.Current, MetricType.Damage);
                        break;
                    case 1:
                        // 当前-治疗
                        RefreshDpsTable(SourceType.Current, MetricType.Healing);
                        break;
                    case 2:
                        // 当前-承伤
                        RefreshDpsTable(SourceType.Current, MetricType.Taken);
                        break;

                }
            }
            BattleTimeText.Text = duration;
            //RefreshDpsTable(true);
        }

        /// <summary>
        /// 清空当前数据数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // # 清空：触发 HandleClearData（停止图表刷新→清空数据→重置图表）
            HandleClearData();
        }

        // # 设置按钮 → 右键菜单
        private void button_Settings_Click(object sender, EventArgs e)
        {
            var menulist = new IContextMenuStripItem[]
             {
                    new ContextMenuStripItem("历史战斗")
                    {
                        IconSvg = Resources.historicalRecords,
                        Sub = new IContextMenuStripItem[]
                        {
                            new ContextMenuStripItem("战斗记录")
                            {


                            },
                        }
                    },
                    new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up},
                    new ContextMenuStripItem("主窗体"){ IconSvg = Resources.HomeIcon, },
                    new ContextMenuStripItem("技能循环监测"),
                    //new ContextMenuStripItem(""){ IconSvg = Resources.userUid, },
                    new ContextMenuStripItem("统计排除"){ IconSvg = Resources.exclude, },
                    new ContextMenuStripItem("打桩模式"){ IconSvg = Resources.Stakes },
                    new ContextMenuStripItem("退出"){ IconSvg = Resources.quit, },
             }
            ;

            AntdUI.ContextMenuStrip.open(this, it =>
            {
                // # 菜单点击回调：根据 Text 执行对应动作
                switch (it.Text)
                {
                    case "基础设置":
                        OpenSettingsDialog();
                        break;
                    case "主窗体":
                        if (FormManager.mainForm == null || FormManager.mainForm.IsDisposed)
                        {
                            FormManager.mainForm = new MainForm();
                        }
                        FormManager.mainForm.Show();
                        break;
                    case "技能循环监测":
                        if (FormManager.skillRotationMonitorForm == null || FormManager.skillRotationMonitorForm.IsDisposed)
                        {
                            FormManager.skillRotationMonitorForm = new SkillRotationMonitorForm();
                        }
                        FormManager.skillRotationMonitorForm.Show();
                        FormGui.SetColorMode(FormManager.skillRotationMonitorForm, AppConfig.IsLight);
                        break;
                    case "数据显示设置":
                        //dataDisplay(); 
                        break;
                    case "统计排除":
                        break;
                    case "打桩模式":
                        if (PilingModeCheckbox.Visible)
                        {
                            PilingModeCheckbox.Visible = false;
                        }
                        else
                        {
                            PilingModeCheckbox.Visible = true;
                        }

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
            if (FormManager.settingsForm == null || FormManager.settingsForm.IsDisposed)
            {
                FormManager.settingsForm = new SettingsForm();
            }
            FormManager.settingsForm.Show();

        }

        // # 按钮提示气泡（置顶）
        private void button_AlwaysOnTop_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button_AlwaysOnTop, "置顶窗口");


        }

        // # 通用提示气泡工具
        private void ToolTip(System.Windows.Forms.Control control, string text)
        {

            AntdUI.TooltipComponent tooltip = new AntdUI.TooltipComponent()
            {
                Font = new Font("HarmonyOS Sans SC", 8, FontStyle.Regular),
            };
            tooltip.ArrowAlign = AntdUI.TAlign.TL;
            tooltip.SetTip(control, text);
        }

        // # 按钮提示气泡（清空）
        private void button1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button1, "清空当前数据");
        }

        // # 按钮提示气泡（单次/全程切换）
        private void button3_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button3, "点击切换：单次统计/全程统");
        }


        // # 打桩模式：周期检查（1s）
        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (PilingModeCheckbox.Checked)
            {
                // # 预校验：需要昵称与 UID 才能进行打桩
                if (string.IsNullOrWhiteSpace(AppConfig.NickName) || AppConfig.Uid == 0)
                {
                    PilingModeCheckbox.Checked = false;
                    timer1.Enabled = false;
                    var result = AppMessageBox.ShowMessage("未获取到昵称或者UID，请换个地图后再进协会", this);

                    return;
                }
                // # 达到 1 分钟：自动结束一次打桩（你在提示里说 3 分钟，这里逻辑为 >1 分钟，保持原样）
                TimeSpan duration = StatisticData._manager.GetCombatDuration();//获取时间
                if (duration > TimeSpan.FromMinutes(1))
                {
                    //暂停打桩模式
                    PilingModeCheckbox.Checked = false;
                    timer1.Enabled = false;
                    // 这里可以写你的其它逻辑

                    var snapshot = StatisticData._manager.TakeSnapshotAndGet();//获取快照
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
                    else
                    {


                    }



                }
            }
        }

        // # 打桩模式：启停控制（CheckBox）
        private void PilingModeCheckbox_CheckedChanged(object sender, BoolEventArgs e)
        {
            TimeSpan duration = StatisticData._manager.GetCombatDuration();//获取时间

            if (e.Value)
            {

                var result = AppMessageBox.ShowMessage("打桩时间为3分钟，需注意以下3点:\n0.:打桩模式开启后只会记录自己的数据\n1.开启后请找协会内最右侧木桩[靠窗的那根]\n2.确保战斗计时为0开启\n3.如果伤害不满意可关闭打桩模式重新勾选\n4.异常数据会被删除\n", this);
                if (result == DialogResult.OK)
                {
                    DpsTableDatas.DpsTable.Clear();
                    StatisticData._manager.ClearAll();
                    SkillTableDatas.SkillTable.Clear();
                    Task.Delay(200);
                    //打桩模式启动
                    AppConfig.PilingMode = true;
                    timer1.Enabled = true;
                }
                else
                {
                    // 用户关闭或取消

                    PilingModeCheckbox.Checked = false;
                }

            }
            else
            {
                AppConfig.PilingMode = false;
                //打桩模式关闭
                timer1.Enabled = false;
            }
        }

        // # 主题切换：前景色变化时适配控件背景
        private void DpsStatisticsForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                //浅色

                sortedProgressBarList1.BackColor = ColorTranslator.FromHtml("#E0E0E0");
                //textProgressBar1.BackColor = ColorTranslator.FromHtml("#FFFFFF");

            }
            else
            {
                //深色
                sortedProgressBarList1.BackColor = ColorTranslator.FromHtml("#999999");
                //textProgressBar1.BackColor = ColorTranslator.FromHtml("#000000");

            }
        }


        #region 钩子
        /// <summary>
        /// 键盘钩子
        /// </summary>
        private KeyboardHook KbHook { get; } = new(); // # 全局输入：全局热键钩子，用于响应窗口控制/穿透/清空等快捷键
        public void RegisterKeyboardHook()
        {
            // 键盘钩子初始化
            KbHook.SetHook(); // # 全局输入：安装键盘钩子
            KbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent; // # 热键绑定：统一在此监听
        }

        /// <summary>
        /// 窗体关闭时的清理工作
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // 释放键盘钩子
                KbHook?.UnHook(); // # 全局输入：卸载键盘钩子，避免句柄泄漏
            }
            catch (Exception ex)
            {
                Console.WriteLine($"窗体关闭清理时出错: {ex.Message}");
            }

            base.OnFormClosed(e);
        }

        #region —— 全局热键 —— 

        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e)
        {
            // # 将按键与配置的功能键匹配，解耦具体键位
            if (e.KeyData == AppConfig.MouseThroughKey) { HandleMouseThrough(); } // # 切换鼠标穿透
            else if (e.KeyData == AppConfig.FormTransparencyKey) { HandleFormTransparency(); } // # 切换窗体透明度
            else if (e.KeyData == AppConfig.WindowToggleKey) { } // # 开关监控/窗口
            else if (e.KeyData == AppConfig.ClearDataKey) { HandleClearData(); } // # 清空当前统计
            else if (e.KeyData == AppConfig.ClearHistoryKey) { }//等待重写实现 // # 预留：清空历史
        }

        #endregion

        #region HandleMouseThrough() 响应鼠标穿透
        private void HandleMouseThrough()
        {

            // 判断当前窗体是否在穿透模式
            if (!MousePenetrationHelper.IsPenetrating(this.Handle))
            {
                // 当前不是穿透模式 → 开启穿透
                MousePenetrationHelper.SetMousePenetrate(this, enable: true, alpha: 230);
                Opacity = AppConfig.Transparency;
            }
            else
            {
                // 当前是穿透模式 → 关闭穿透
                MousePenetrationHelper.SetMousePenetrate(this, enable: false);
                Opacity = 1.0;
            }

        }

        #endregion


        #region HandleFormTransparency() 响应窗体透明

        /// <summary>
        /// 是否开启透明
        /// </summary>
        bool hyaline = false;

        private void HandleFormTransparency()
        {


            if (hyaline)
            {
                // 当前是透明状态（1.0），要切换到配置透明度

                Opacity = AppConfig.Transparency / 100;
                hyaline = false;
                Console.WriteLine($"切换到配置透明度: {AppConfig.Transparency}%)");
            }
            else
            {
                // 当前是配置透明度，要切换到完全不透明（1.0）
                Opacity = 1.0;
                hyaline = true;
                Console.WriteLine($"切换到完全不透明: 100% (Opacity: 1.0)");
            }
        }

        #endregion
        #endregion
    }
}
