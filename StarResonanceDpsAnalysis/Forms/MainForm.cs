using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Properties;

namespace StarResonanceDpsAnalysis
{
    public partial class MainForm : BorderlessForm
    {

        public static void LoadFromEmbeddedSkillConfig()
        {
            // 1) 先用 int 键的表（已经解析过字符串）
            foreach (var kv in EmbeddedSkillConfig.AllByInt)
            {
                var id = (ulong)kv.Key;
                var def = kv.Value;

                // 将一条技能元数据（SkillMeta）写入 SkillBook 的全局字典中
                // 这里用的是整条更新（SetOrUpdate），如果该技能 ID 已存在则覆盖，不存在则添加
                SkillBook.SetOrUpdate(new SkillMeta
                {
                    Id = id,                         // 技能 ID（唯一标识一个技能）
                    Name = def.Name,                 // 技能名称（字符串，例如 "火球术"）
                    School = def.Element.ToString(), // 技能所属元素或流派（枚举转字符串）
                    Type = def.Type,                 // 技能类型（Damage/Heal/其他）——用于区分伤害技能和治疗技能
                    Element = def.Element            // 技能元素类型（枚举，例如 火/冰/雷）
                });


            }

            // 2) 有些 ID 可能超出 int 或不在 AllByInt，可以再兜底遍历字符串键
            foreach (var kv in EmbeddedSkillConfig.AllByString)
            {
                if (ulong.TryParse(kv.Key, out var id))
                {
                    // 如果 int 表已覆盖，这里会覆盖同名；没关系，等价
                    var def = kv.Value;
                    // 将一条技能元数据（SkillMeta）写入 SkillBook 的全局字典中
                    // 这里用的是整条更新（SetOrUpdate），如果该技能 ID 已存在则覆盖，不存在则添加
                    SkillBook.SetOrUpdate(new SkillMeta
                    {
                        Id = id,                         // 技能 ID（唯一标识一个技能）
                        Name = def.Name,                 // 技能名称（字符串，例如 "火球术"）
                        School = def.Element.ToString(), // 技能所属元素或流派（枚举转字符串）
                        Type = def.Type,                 // 技能类型（Damage/Heal/其他）——用于区分伤害技能和治疗技能
                        Element = def.Element            // 技能元素类型（枚举，例如 火/冰/雷）
                    });

                }
            }

            // 你也可以在这里写日志：加载了多少条技能
            // Console.WriteLine($"SkillBook loaded {EmbeddedSkillConfig.AllByInt.Count} + {EmbeddedSkillConfig.AllByString.Count} entries.");
        }



        #region ========== 字段与常量 ==========

        /// <summary>
        /// 分析器
        /// </summary>
        private PacketAnalyzer PacketAnalyzer { get; } = new();

        #region —— 抓包设备/统计 —— 

        private ICaptureDevice? SelectedDevice { get; set; } = null;

        #endregion

        /// <summary>
        /// 键盘钩子
        /// </summary>
        private KeyboardHook KbHook { get; } = new();

        /// <summary>
        /// 是否开始抓包
        /// </summary>
        private bool IsCaptureStarted { get; set; } = false;

        /// <summary>
        /// 光标强制控制定时器（用于鼠标穿透模式）
        /// </summary>
        private System.Windows.Forms.Timer? _cursorControlTimer = null;

        #region —— 内部类型 —— 

        private class PacketData(object packet)
        {
            public object Packet { get; set; } = packet;
        }

        #endregion

        #endregion

        #region ========== 构造与启动加载 ==========

        public MainForm()
        {

            InitializeComponent();
            FormGui.SetDefaultGUI(this);

            /* Application.ProductVersion 默认会被 MSBuild 附加 Git 哈希, 
             * 如: "1.0.0+123456789acbdef", 
             * 将 + 后面去掉就是项目属性的版本号,
             * 这样可以让生成文件的版本号与标题版本号一致
             * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            pageHeader_MainHeader.Text += $" v{Application.ProductVersion.Split('+')[0]}";

            InitTableColumnsConfigAtFirstRun();
            LoadTableColumnVisibilitySettings();
            ToggleTableView();
            LoadFromEmbeddedSkillConfig();


        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 键盘钩子初始化
            KbHook.SetHook();
            KbHook.OnKeyDownEvent += kbHook_OnKeyDownEvent;

            LoadNetworkDevices();

            FormGui.SetColorMode(this, AppConfig.IsLight);

            RefreshHotKeyTips();

            // 检查网卡设置状态，决定是否显示设置提示
            UpdateNetworkCardSettingTip();

            // 确保程序启动时鼠标穿透状态为正常（可点击）
            ResetMouseThroughState();
        }

        /// <summary>
        /// 窗体关闭时的清理工作
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // 停止光标控制定时器
                if (_cursorControlTimer != null)
                {
                    _cursorControlTimer.Stop();
                    _cursorControlTimer.Dispose();
                    _cursorControlTimer = null;
                }

                // 释放键盘钩子
                KbHook?.UnHook();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"窗体关闭清理时出错: {ex.Message}");
            }

            base.OnFormClosed(e);
        }

        #endregion

        #region ========== 窗体消息处理 ==========

        /// <summary>
        /// 覆盖窗体消息处理，实现完全的鼠标穿透
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // 如果鼠标穿透模式开启，拦截所有可能导致窗体交互的消息
            bool isCurrentlyPenetrating = IsInMousePenetrateMode();

            if (isCurrentlyPenetrating)
            {
                const int WM_NCHITTEST = 0x84;
                const int WM_SETCURSOR = 0x20;
                const int WM_NCLBUTTONDOWN = 0xA1;
                const int WM_NCLBUTTONUP = 0xA2;
                const int WM_NCRBUTTONDOWN = 0xA4;
                const int WM_NCRBUTTONUP = 0xA5;
                const int WM_NCMBUTTONDOWN = 0xA7;
                const int WM_NCMBUTTONUP = 0xA8;
                const int WM_NCLBUTTONDBLCLK = 0xA3;
                const int WM_NCRBUTTONDBLCLK = 0xA6;
                const int WM_NCMBUTTONDBLCLK = 0xA9;
                const int WM_LBUTTONDOWN = 0x201;
                const int WM_LBUTTONUP = 0x202;
                const int WM_RBUTTONDOWN = 0x204;
                const int WM_RBUTTONUP = 0x205;
                const int WM_MBUTTONDOWN = 0x207;
                const int WM_MBUTTONUP = 0x208;
                const int WM_LBUTTONDBLCLK = 0x203;
                const int WM_RBUTTONDBLCLK = 0x206;
                const int WM_MBUTTONDBLCLK = 0x209;
                const int WM_MOUSEWHEEL = 0x20A;
                const int WM_MOUSEHWHEEL = 0x20E;
                const int WM_MOUSEMOVE = 0x200;

                // 扩展的窗体命中测试常量
                const int HTTRANSPARENT = -1;

                switch (m.Msg)
                {
                    case WM_NCHITTEST:
                        // 对于命中测试消息，始终返回HTTRANSPARENT
                        // 这是最关键的消息，必须在任何其他处理之前拦截
                        m.Result = (IntPtr)HTTRANSPARENT;
                        Console.WriteLine($"WM_NCHITTEST intercepted - returning HTTRANSPARENT");
                        return;

                    case WM_SETCURSOR:
                        // 对于设置光标消息，返回TRUE表示我们已经处理了
                        m.Result = (IntPtr)1; // TRUE
                        Console.WriteLine($"WM_SETCURSOR intercepted - preventing cursor change");
                        return;

                    case WM_NCLBUTTONDOWN:
                    case WM_NCLBUTTONUP:
                    case WM_NCRBUTTONDOWN:
                    case WM_NCRBUTTONUP:
                    case WM_NCMBUTTONDOWN:
                    case WM_NCMBUTTONUP:
                    case WM_NCLBUTTONDBLCLK:
                    case WM_NCRBUTTONDBLCLK:
                    case WM_NCMBUTTONDBLCLK:
                    case WM_LBUTTONDOWN:
                    case WM_LBUTTONUP:
                    case WM_RBUTTONDOWN:
                    case WM_RBUTTONUP:
                    case WM_MBUTTONDOWN:
                    case WM_MBUTTONUP:
                    case WM_LBUTTONDBLCLK:
                    case WM_RBUTTONDBLCLK:
                    case WM_MBUTTONDBLCLK:
                    case WM_MOUSEWHEEL:
                    case WM_MOUSEHWHEEL:
                    case WM_MOUSEMOVE:
                        // 完全忽略所有鼠标消息，让它们穿透到下层窗口
                        Console.WriteLine($"Mouse message 0x{m.Msg:X} intercepted and ignored");
                        return;
                }
            }

            // 正常模式下，或者非鼠标消息，调用基类处理
            try
            {
                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WndProc base call failed: {ex.Message}");
                // 如果基类处理失败，我们仍然可以继续运行
            }
        }

        /// <summary>
        /// 检查是否处于鼠标穿透模式
        /// </summary>
        /// <returns></returns>
        private bool IsInMousePenetrateMode()
        {
            // 通过检查标题是否包含穿透标记来判断状态
            return pageHeader_MainHeader.SubText.Contains("[鼠标穿透]");
        }

        #endregion

        #region ========== 启动时设备/表格配置 ==========

        /// <summary>
        /// 启动时加载网卡设备
        /// </summary>
        private void LoadNetworkDevices()
        {
            Console.WriteLine("应用程序启动时加载网卡...");

            if (AppConfig.NetworkCard >= 0)
            {
                var devices = CaptureDeviceList.Instance;
                if (AppConfig.NetworkCard < devices.Count)
                {
                    SelectedDevice = devices[AppConfig.NetworkCard];
                    Console.WriteLine($"启动时已选择网卡: {SelectedDevice.Description} (索引: {AppConfig.NetworkCard})");
                }
            }
            else
            {
                if (FormManager.settingsForm == null || FormManager.settingsForm.IsDisposed)
                {
                    FormManager.settingsForm = new SettingsForm();
                }
                FormManager.settingsForm.LoadDevices();
            }
        }

        /// <summary>
        /// 更新网卡设置提示的显示状态
        /// </summary>
        private void UpdateNetworkCardSettingTip()
        {
            try
            {
                // 检查网卡是否已经正确设置
                bool isNetworkCardSet = false;

                if (AppConfig.NetworkCard >= 0)
                {
                    var devices = CaptureDeviceList.Instance;
                    if (devices != null && devices.Count > 0 && AppConfig.NetworkCard < devices.Count)
                    {
                        // 网卡索引有效，认为已经设置
                        isNetworkCardSet = true;
                    }
                }

                // 根据网卡设置状态决定是否显示提示
                if (isNetworkCardSet)
                {
                    // 网卡已设置，隐藏提示或显示其他内容
                    if (!IsCaptureStarted)
                    {
                        // 如果没有开始监控，显示00:00
                        label_SettingTip.Text = "00:00";
                    }
                    // 如果正在监控中，label_SettingTip会在StartCapture方法中被设置为计时器显示
                }
                else
                {
                    // 网卡未设置，显示设置提示
                    label_SettingTip.Text = "请先右上角设置网卡在启动哟！";
                }

                // label_SettingTip始终可见
                label_SettingTip.Visible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新网卡设置提示时出错: {ex.Message}");
                // 出错时保持默认提示
                label_SettingTip.Text = "请先右上角设置网卡在启动哟！";
                label_SettingTip.Visible = true;
            }
        }

        /// <summary>
        /// 公共方法：供外部调用来更新网卡设置提示
        /// </summary>
        public void RefreshNetworkCardSettingTip()
        {
            UpdateNetworkCardSettingTip();
        }

        /// <summary>
        /// 用于加载数据记录表格列名
        /// </summary>
        private void LoadTableColumnVisibilitySettings()
        {
            foreach (var item in ColumnSettingsManager.AllSettings)
            {
                string strValue = AppConfig.GetValue("TableSet", item.Key, string.Empty);

                // 如果没有保存记录（为空），默认显示（true）
                if (string.IsNullOrEmpty(strValue))
                {
                    item.IsVisible = true;
                }
                else
                {
                    item.IsVisible = string.Equals(strValue, "true", StringComparison.OrdinalIgnoreCase);
                }
            }

        }

        #endregion

        #region ========== 热键/交互事件 ==========
        #region —— 全局热键 —— 

        public void kbHook_OnKeyDownEvent(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == AppConfig.MouseThroughKey) { HandleMouseThrough(); }
            else if (e.KeyData == AppConfig.FormTransparencyKey) { HandleFormTransparency(); }
            else if (e.KeyData == AppConfig.WindowToggleKey) { HandleSwitchMonitoring(); }
            else if (e.KeyData == AppConfig.ClearDataKey) { HandleClearData(); }
            else if (e.KeyData == AppConfig.ClearHistoryKey) { }//等待重写实现
        }
        #endregion

        #region —— 按钮/复选框/下拉事件 —— 
        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight;

            button_ThemeSwitch.Toggle = !AppConfig.IsLight;

            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.skillDiary, AppConfig.IsLight);

            FormGui.SetColorMode(FormManager.skillDetailForm, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.settingsForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.dpsStatistics, AppConfig.IsLight);//设置窗体颜色


        }

        private void button_AlwaysOnTop_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;

            button_AlwaysOnTop.Toggle = TopMost;
        }

        private void dropdown_History_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            if (IsCaptureStarted)
            {
                MessageBox.Show("请先停止监控后再查看历史数据");
                return;
            }

            DpsTableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();

            dropdown_History.SelectedValue = -1;
        }

        private void button_SkillDiary_Click(object sender, EventArgs e)
        {
            if (FormManager.dpsStatistics == null || FormManager.dpsStatistics.IsDisposed)
            {
                FormManager.dpsStatistics = new DpsStatistics();
            }

            FormManager.dpsStatistics.Show();

            //var teamShare = StatisticData._manager.GetTeamSkillDamageShareTotal(topN: 15, includeOthers: true);
            //// 绑定表格或打印
            //foreach (var s in teamShare)
            //    Console.WriteLine($"{s.SkillName} 总伤害={s.Total} 占比={s.Percent}%");




            //FormGui.Modal(this, "正在开发", "正在开发");
            return;
            if (FormManager.skillDiary == null || FormManager.skillDiary.IsDisposed)
            {
                FormManager.skillDiary = new SkillDiary();
            }
            FormManager.skillDiary.Show();
        }

        private void switch_IsMonitoring_CheckedChanged(object sender, BoolEventArgs e)
        {
            if (IsCaptureStarted)
            {
                StopCapture();
            }
            else
            {
                StartCapture();
            }
        }

        private void button_Settings_MouseClick(object sender, MouseEventArgs e)
        {
            var menulist = new IContextMenuStripItem[]
            {
                new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up, },
                new ContextMenuStripItem("数据显示设置"){ IconSvg = Resources.data_display, },
                new ContextMenuStripItem("用户UID设置"){ IconSvg = Resources.userUid, },
            };

            AntdUI.ContextMenuStrip.open(this, it =>
            {
                switch (it.Text)
                {
                    case "基础设置":
                        OpenSettingsDialog(); break;
                    case "数据显示设置":
                        dataDisplay(); break;
                    case "用户UID设置":
                        SetUserUid();

                        break;
                }
            }, menulist);
        }

        #endregion
        #endregion


        /// <summary>
        /// 数据包到达事件
        /// </summary>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var dev = (ICaptureDevice)sender;
                PacketAnalyzer.StartNewAnalyzer(dev, e.GetPacket());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据包到达后进行处理时发生异常 {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        #region ========== 计时器Tick事件 ==========

        private void timer_RefreshDpsTable_Tick(object sender, EventArgs e)
        {
            // Task.Run(() => RefreshDpsTable());
        }

        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e)
        {

            var duration = StatisticData._manager.GetFormattedCombatDuration();
            label_SettingTip.Text = duration;

        }

        #endregion

        private void table_DpsDataTable_CellClick(object sender, TableClickEventArgs e)
        {
            if (e.RowIndex == 0) return;
            ulong uid = 0;

            if (sort != null)
            {
                uid = DpsTableDatas.DpsTable[sort[e.RowIndex - 1]].Uid;

            }
            else
            {
                uid = DpsTableDatas.DpsTable[e.RowIndex - 1].Uid;
            }

            if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed)
            {
                FormManager.skillDetailForm = new SkillDetailForm();
            }
            SkillTableDatas.SkillTable.Clear();

            FormManager.skillDetailForm.Uid = uid;
            //获取玩家信息
            var info = StatisticData._manager.GetPlayerBasicInfo(uid);
            FormManager.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession);
            FormManager.skillDetailForm.SelectDataType();
            FormManager.skillDetailForm.Show();

        }

        private int[] sort;//存储排列后的顺序
        private void table_DpsDataTable_SortRows(object sender, IntEventArgs e)
        {
            sort = table_DpsDataTable.SortIndex();
        }

        private void label_SettingTip_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(FormManager.rankingsForm==null||FormManager.rankingsForm.IsDisposed)
            {
                FormManager.rankingsForm = new RankingsForm();
            }
            FormManager.rankingsForm.Show();
        }
    }
}
