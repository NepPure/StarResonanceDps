using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Properties;
using ZstdNet;
using static System.Windows.Forms.AxHost;

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
        /// 计时器&超时控制
        /// </summary>
        private Stopwatch CombatWatch { get; } = new();

        /// <summary>
        /// 键盘钩子
        /// </summary>
        private KeyboardHook KbHook { get; } = new();

        #region —— 历史记录/数据结构 —— 
        Dictionary<string, List<PlayerData>> HistoricalRecords = [];

        #endregion

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

    
      

            // 自动测试

            //// 鼠标步进测试
            //table_DpsDataTable.Click += (s, e) =>
            //{
            //    value += 0.01d;
            //    value = value % 0.1d;
            //    textProgressBar1.ProgressBarValue = value;
            //    textProgressBar1.Text = value.ToString();
            //};

       
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
                using var setup = new Setup(this);
                setup.LoadDevices();
            }
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
            else if (e.KeyData == AppConfig.ClearHistoryKey) { HandleClearHistory(); }
        }
        #endregion

        #region —— 按钮/复选框/下拉事件 —— 
        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight;

            button_ThemeSwitch.Toggle = !AppConfig.IsLight;

            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(Common.skillDiary, AppConfig.IsLight);

            FormGui.SetColorMode(Common.skillDetailForm, AppConfig.IsLight);
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
            ShowHistoricalDps(e.Value.ToString());
            dropdown_History.SelectedValue = -1;
        }

        private void button_SkillDiary_Click(object sender, EventArgs e)
        {

            DpsStatistics DpsStatistics = new DpsStatistics();
            DpsStatistics.Show();

            //var teamShare = StatisticData._manager.GetTeamSkillDamageShareTotal(topN: 15, includeOthers: true);
            //// 绑定表格或打印
            //foreach (var s in teamShare)
            //    Console.WriteLine($"{s.SkillName} 总伤害={s.Total} 占比={s.Percent}%");




            //FormGui.Modal(this, "正在开发", "正在开发");
            return;
            if (Common.skillDiary == null || Common.skillDiary.IsDisposed)
            {
                Common.skillDiary = new SkillDiary();
            }
            Common.skillDiary.Show();
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
            label_SettingTip.Text = CombatWatch.Elapsed.ToString(@"mm\:ss");
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

            if (Common.skillDetailForm == null || Common.skillDetailForm.IsDisposed)
            {
                Common.skillDetailForm = new SkillDetailForm();
            }
            SkillTableDatas.SkillTable.Clear();

            Common.skillDetailForm.Uid = uid;
            //获取玩家信息
            var info = StatisticData._manager.GetPlayerBasicInfo(uid);
            Common.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession);
            Common.skillDetailForm.SelectDataType();
            Common.skillDetailForm.Show();

        }

        private int[] sort;//存储排列后的顺序
        private void table_DpsDataTable_SortRows(object sender, IntEventArgs e)
        {
            sort = table_DpsDataTable.SortIndex();
        }
    }
}
