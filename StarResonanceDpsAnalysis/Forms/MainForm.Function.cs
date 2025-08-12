using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Extends;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System.Runtime.InteropServices;

namespace StarResonanceDpsAnalysis
{
    public partial class MainForm
    {
        #region InitTableColumnsConfigAtFirstRun() 首次运行时初始化表头配置

        private void InitTableColumnsConfigAtFirstRun()
        {
            if (AppConfig.GetConfigExists())
            {
                AppConfig.NickName = AppConfig.GetValue("UserConfig", "NickName", "未知昵称");
                AppConfig.Uid = (ulong)AppConfig.GetValue("UserConfig", "Uid", "0").ToInt();
                AppConfig.Profession = AppConfig.GetValue("UserConfig", "Profession", "未知职业");
                AppConfig.CombatPower = AppConfig.GetValue("UserConfig", "CombatPower", "0").ToInt();
                StatisticData._manager.SetNickname(AppConfig.Uid, AppConfig.NickName);
                StatisticData._manager.SetProfession(AppConfig.Uid, AppConfig.Profession);
                StatisticData._manager.SetCombatPower(AppConfig.Uid, AppConfig.CombatPower);
               
                return;
            }



        }

        #endregion


        #region TableLoad() 初始化Table表头

        /// <summary>
        /// 初始化 或者刷新 表头 用于用户更改了显示列设置
        /// </summary>
        private void ToggleTableView()
        {
            table_DpsDataTable.Columns.Clear();


            table_DpsDataTable.Columns = ColumnSettingsManager.BuildColumns();

            table_DpsDataTable.StackedHeaderRows = ColumnSettingsManager.BuildStackedHeader();


            table_DpsDataTable.Binding(DpsTableDatas.DpsTable);

        }

        #endregion


        #region RefreshDpsTable() 刷新DPS信息

        public static void RefreshDpsTable()
        {
            // 1) 拷贝一份所有玩家数据，避免并发修改
            List<PlayerData> statsList = StatisticData._manager
                .GetPlayersWithCombatData()
                .ToList();

            if (statsList.Count <= 0) return;
            // 2) 计算最大总伤害，用于归一化进度条
            float totalDamageSum = statsList
            .Where(p => p?.DamageStats != null)
            .Sum(p => (float)p.DamageStats.Total);

            if (totalDamageSum <= 0f) totalDamageSum = 1f;

            // 3) 遍历，新增或更新行
            foreach (var stat in statsList)
            {
                if (stat == null) continue;
                // 3.1 计算进度条比例
                float percent = (float)stat.DamageStats.Total / totalDamageSum;

                // 3.2 按 UID 查找已有行
                var row = DpsTableDatas.DpsTable
                    .FirstOrDefault(x => x.Uid == stat.Uid);

                // 3.3 计算暴击率/幸运率（以 % 计）
                double critRate = stat.DamageStats.CountTotal > 0
                                 ? (double)stat.DamageStats.CountCritical / stat.DamageStats.CountTotal * 100
                                 : 0.0;
                double luckyRate = stat.DamageStats.CountTotal > 0
                                 ? (double)stat.DamageStats.CountLucky / stat.DamageStats.CountTotal * 100
                                 : 0.0;


                if (row == null)
                {
                    // 新增一行
                    DpsTableDatas.DpsTable.Add(new DpsTable(
                         stat.Uid,
                         stat.Nickname,
                         stat.TakenDamage,
                         stat.HealingStats.Total,
                         stat.HealingStats.Critical,
                        stat.HealingStats.Lucky,
                        stat.HealingStats.CritLucky,
                        stat.HealingStats.RealtimeValue,
                        stat.HealingStats.RealtimeMax,
                         stat.Profession,
                        stat.DamageStats.Total,
                        stat.DamageStats.Critical,
                         stat.DamageStats.Lucky,
                        stat.DamageStats.CritLucky,
                        Math.Round(critRate, 1),
                        Math.Round(luckyRate, 1),
                       stat.DamageStats.RealtimeValue,     // 即时 DPS
                        stat.DamageStats.RealtimeMax,       // 峰值 DPS
                        Math.Round(stat.DamageStats.GetTotalPerSecond(), 1), // 总平均 DPS
                        Math.Round(stat.HealingStats.GetTotalPerSecond(), 1), // 总平均 HPS
                    new CellProgress(percent)
                    {
                        Size = new Size(200, 10),
                        Fill = AppConfig.DpsColor
                    },
                    stat.CombatPower

                    ));
                }
                else
                {
                    // —— 更新 DPS 部分 —— 
                    row.Profession = stat.Profession;
                    row.NickName = stat.Nickname;
                    row.TotalDamage = stat.DamageStats.Total.ToString();
                    row.CriticalDamage = stat.DamageStats.Critical.ToString();
                    row.LuckyDamage = stat.DamageStats.Lucky.ToString();
                    row.CritLuckyDamage = stat.DamageStats.CritLucky.ToString();
                    row.CritRate = Math.Round(critRate, 1).ToString();
                    row.LuckyRate = Math.Round(luckyRate, 1).ToString();
                    row.InstantDps = stat.DamageStats.RealtimeValue.ToString();
                    row.MaxInstantDps = stat.DamageStats.RealtimeMax.ToString();
                    row.TotalDps = Math.Round(stat.DamageStats.GetTotalPerSecond(), 1).ToString();

                    // —— 更新 HPS（治疗）部分 —— 
                    row.DamageTaken = stat.TakenDamage.ToString();
                    row.TotalHealingDone = stat.HealingStats.Total.ToString();
                    row.CriticalHealingDone = stat.HealingStats.Critical.ToString();
                    row.LuckyHealingDone = stat.HealingStats.Lucky.ToString();
                    row.CritLuckyHealingDone = stat.HealingStats.CritLucky.ToString();

                    row.InstantHps = stat.HealingStats.RealtimeValue.ToString();
                    row.MaxInstantHps = stat.HealingStats.RealtimeMax.ToString();
                    row.TotalHps = Math.Round(stat.HealingStats.GetTotalPerSecond(), 1).ToString();

                    row.CombatPower = stat.CombatPower;
                    // —— 更新进度条 —— 
                    if (row.CellProgress is CellProgress cp)
                    {
                        cp.Value = percent;  // 仅更新进度值
                    }
                    else
                    {
                        // 兼容：如果之前没创建过，就新建一个
                        row.CellProgress = new CellProgress(percent)
                        {
                            Size = new Size(200, 10),
                            Fill = AppConfig.DpsColor
                        };
                    }
                }

            }
        }

        #endregion


        #region HandleMouseThrough() 响应鼠标穿透

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;

        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        private bool IsMousePenetrate = false;

        /// <summary>
        /// 保存进入穿透模式前的透明度值，用于退出时恢复
        /// </summary>
        private double? _savedOpacityBeforePenetrate = null;

        private void HandleMouseThrough()
        {
            try
            {
                var exStyle = GetWindowLong(Handle, GWL_EXSTYLE);

                // 修正逻辑：切换鼠标穿透状态
                int dwNewLong;
                if (IsMousePenetrate)
                {
                    // 当前是穿透状态，现在要禁用穿透，恢复正常点击
                    dwNewLong = exStyle & ~(WS_EX_TRANSPARENT | WS_EX_LAYERED);
                }
                else
                {
                    // 当前不是穿透状态，现在要启用穿透，让鼠标完全穿过窗体
                    dwNewLong = exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT;
                }

                var result = SetWindowLong(Handle, GWL_EXSTYLE, dwNewLong);

                // 切换状态标志
                IsMousePenetrate = !IsMousePenetrate;

                // 调试输出
                Console.WriteLine($"鼠标穿透状态切换: {(IsMousePenetrate ? "启用 - 窗体完全不可点击" : "禁用 - 窗体恢复正常点击")}");
                Console.WriteLine($"SetWindowLong 调用结果: {result}，当前ExStyle: 0x{exStyle:X8} -> 0x{dwNewLong:X8}");

                // 更新界面显示状态和透明度
                UpdateMouseThroughStatus();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"切换鼠标穿透状态时出错: {ex.Message}");
                // 发生错误时确保状态一致
                IsMousePenetrate = false;
                UpdateMouseThroughStatus();
            }
        }

        /// <summary>
        /// 更新鼠标穿透状态的界面显示
        /// </summary>
        private void UpdateMouseThroughStatus()
        {
            try
            {
                if (IsMousePenetrate)
                {
                    // 穿透状态：在标题中添加提示
                    if (!pageHeader_MainHeader.SubText.Contains("[鼠标穿透]"))
                    {
                        pageHeader_MainHeader.SubText += " [鼠标穿透]";
                    }

                    // 保存当前透明度设置，然后设置为穿透模式的透明度
                    _savedOpacityBeforePenetrate = Opacity;

                    // 设置鼠标穿透时的固定透明度（0.4，既透明又能看到界面）
                    Opacity = 0.4;

                    // 启动光标控制定时器，强制保持默认光标
                    StartCursorControlTimer();

                    Console.WriteLine($"鼠标穿透模式：透明度已设置为 {Opacity} (40%)");
                }
                else
                {
                    // 正常状态：移除穿透提示
                    pageHeader_MainHeader.SubText = pageHeader_MainHeader.SubText.Replace(" [鼠标穿透]", "");

                    // 停止光标控制定时器
                    StopCursorControlTimer();

                    // 恢复透明度的优先级：
                    // 1. 使用保存的穿透前透明度（优先）
                    // 2. 如果没有保存值，使用配置中的透明度设置
                    // 3. 考虑hyaline状态（窗体透明热键的状态）
                    if (_savedOpacityBeforePenetrate.HasValue)
                    {
                        Opacity = _savedOpacityBeforePenetrate.Value;
                        _savedOpacityBeforePenetrate = null;
                        Console.WriteLine($"退出穿透模式：透明度已恢复为保存值 {Opacity}");
                    }
                    else
                    {
                        // 根据hyaline状态决定透明度
                        if (hyaline)
                        {
                            // 如果hyaline为true，表示用户之前设置为完全不透明
                            Opacity = 1.0;
                            Console.WriteLine($"退出穿透模式：透明度已恢复为完全不透明 {Opacity}");
                        }
                        else
                        {
                            // 否则使用配置中的透明度
                            Opacity = AppConfig.Transparency / 100.0;
                            Console.WriteLine($"退出穿透模式：透明度已设置为配置值 {Opacity} ({AppConfig.Transparency}%)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新鼠标穿透状态界面显示时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 强制重置鼠标穿透状态为正常（可点击）状态
        /// 用于在程序启动时或发生错误时确保窗体可以正常操作
        /// </summary>
        public void ResetMouseThroughState()
        {
            try
            {
                var exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
                var dwNewLong = exStyle & ~(WS_EX_TRANSPARENT | WS_EX_LAYERED);
                SetWindowLong(Handle, GWL_EXSTYLE, dwNewLong);

                IsMousePenetrate = false;
                UpdateMouseThroughStatus();

                Console.WriteLine("鼠标穿透状态已重置为正常（可点击）状态");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置鼠标穿透状态时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动光标控制定时器，在鼠标穿透模式下强制保持默认光标
        /// </summary>
        private void StartCursorControlTimer()
        {
            try
            {
                // 先停止现有的定时器
                StopCursorControlTimer();

                // 创建新的定时器
                _cursorControlTimer = new System.Windows.Forms.Timer();
                _cursorControlTimer.Interval = 100; // 每100毫秒检查一次
                _cursorControlTimer.Tick += CursorControlTimer_Tick;
                _cursorControlTimer.Start();

                Console.WriteLine("光标控制定时器已启动");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动光标控制定时器时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止光标控制定时器
        /// </summary>
        private void StopCursorControlTimer()
        {
            try
            {
                if (_cursorControlTimer != null)
                {
                    _cursorControlTimer.Stop();
                    _cursorControlTimer.Dispose();
                    _cursorControlTimer = null;
                    Console.WriteLine("光标控制定时器已停止");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止光标控制定时器时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 光标控制定时器回调，强制设置光标为默认样式
        /// </summary>
        private void CursorControlTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (IsMousePenetrate && IsInMousePenetrateMode())
                {
                    // 在鼠标穿透模式下，强制设置光标为默认箭头
                    if (Cursor.Current != Cursors.Default)
                    {
                        Cursor.Current = Cursors.Default;
                        Console.WriteLine("强制重置光标为默认样式");
                    }
                }
                else
                {
                    // 如果不在穿透模式，停止定时器
                    StopCursorControlTimer();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"光标控制定时器回调出错: {ex.Message}");
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
            // 检查是否在鼠标穿透模式下
            if (IsMousePenetrate)
            {
                // 在鼠标穿透模式下，不允许切换透明度
                Console.WriteLine("鼠标穿透模式下，透明度由穿透功能控制");
                return;
            }

            if (hyaline)
            {
                // 当前是透明状态（1.0），要切换到配置透明度
                var opacity = AppConfig.Transparency / 100.0;
                Opacity = opacity;
                hyaline = false;
                Console.WriteLine($"切换到配置透明度: {AppConfig.Transparency}% (Opacity: {opacity})");
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


        #region HandleSwitchMonitoring() 响应监控切换

        private void HandleSwitchMonitoring()
        {
            switch_IsMonitoring.Checked = !switch_IsMonitoring.Checked;
        }

        #endregion


        #region HandleClearData() 响应清空数据

        private void HandleClearData()
        {

            // 先停止所有图表的自动刷新
            ChartVisualizationService.StopAllChartsAutoRefresh();

            // 在清空数据前，通知图表服务战斗结束
            ChartVisualizationService.OnCombatEnd();


            DpsTableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            SkillTableDatas.SkillTable.Clear();

            // 完全重置所有图表（包括清空历史数据和重置视图状态）
            ChartVisualizationService.FullResetAllCharts();

            // 如果当前正在抓包，重新启动图表自动刷新
            if (IsCaptureStarted)
            {
                ChartVisualizationService.StartAllChartsAutoRefresh(1000);
            }
        }

        #endregion


        #region HandleClearHistory() 响应清空历史



        #endregion


        #region RefreshHotKeyTips() 更新热键提示

        public void RefreshHotKeyTips()
        {
            label_HotKeyTips.Text = @$"{AppConfig.MouseThroughKey}：鼠标穿透 | {AppConfig.FormTransparencyKey}：窗体透明 | {AppConfig.WindowToggleKey}：开启/关闭 | {AppConfig.ClearDataKey}：清空数据 | {AppConfig.ClearHistoryKey}：清空历史";
        }

        #endregion




        #region StartCapture() 抓包：开始/停止/事件/统计

        /// <summary>
        /// 开始抓包
        /// </summary>
        private void StartCapture()
        {
            // 前置校验 ——
            if (AppConfig.NetworkCard < 0)
            {
                MessageBox.Show("请选择一个网卡设备");

                switch_IsMonitoring.Checked = false;
                return;
            }

            var devices = CaptureDeviceList.Instance;
            if (devices == null || devices.Count == 0)
                throw new InvalidOperationException("没有找到可用的网络抓包设备");

            if (AppConfig.NetworkCard < 0 || AppConfig.NetworkCard >= devices.Count)
                throw new InvalidOperationException($"无效的网络设备索引: {AppConfig.NetworkCard}");

            SelectedDevice = devices[AppConfig.NetworkCard];
            if (SelectedDevice == null)
                throw new InvalidOperationException($"无法获取网络设备，索引: {AppConfig.NetworkCard}");


            // 清空当前统计 ——
            DpsTableDatas.DpsTable.Clear();
            StatisticData._manager.ClearAll();
            SkillTableDatas.SkillTable.Clear();

            // 清空图表历史数据，开始新的战斗记录
            ChartVisualizationService.ClearAllHistory();

            // 启动所有图表的自动刷新
            ChartVisualizationService.StartAllChartsAutoRefresh(1000);

            // 事件注册与启动监听 --
            SelectedDevice.Open(new DeviceConfiguration
            {
                Mode = DeviceModes.Promiscuous,
                Immediate = true,
                ReadTimeout = 1000,
                BufferSize = 1024 * 1024 * 4
            });
            SelectedDevice.Filter = "ip and tcp";
            SelectedDevice.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            SelectedDevice.StartCapture();

            IsCaptureStarted = true;
            Console.WriteLine("开始抓包...");


            // 启动统计 --
            timer_RefreshDpsTable.Enabled = true;
            pageHeader_MainHeader.SubText = "监控已开启";
            label_SettingTip.Visible = true;
            label_SettingTip.Text = "00:00";

            timer_RefreshRunningTime.Start();
        }

        /// <summary>
        /// 停止抓包
        /// </summary>
        private void StopCapture()
        {
            // 先停止所有图表的自动刷新，防止在停止抓包后继续更新数据
            ChartVisualizationService.StopAllChartsAutoRefresh();


            // 在停止抓包时，通知图表服务战斗结束，确保显示最终的0值状态
            ChartVisualizationService.OnCombatEnd();

            if (SelectedDevice != null)
            {
                try
                {
                    // 1) 先解绑，避免回调里撞到已释放对象
                    SelectedDevice.OnPacketArrival -= Device_OnPacketArrival;

                    // 2) 停止抓包（异步）
                    SelectedDevice.StopCapture();

                    // 3) 等待后台线程真正退出（最多 ~1s）
                    for (int i = 0; i < 100; i++)
                    {
                        // 有的版本有 Started 属性；没有就直接 sleep 一下
                        if (!(SelectedDevice.Started)) break;
                        System.Threading.Thread.Sleep(10);
                    }

                    // 4) 关闭并彻底释放驱动句柄
                    SelectedDevice.Close();
                    SelectedDevice.Dispose();   // ← 关键：第二次不触发通常是没 Dispose
                    Console.WriteLine("停止抓包");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"停止抓包异常: {ex}");
                }
                finally
                {
                    SelectedDevice = null;
                }
            }

            IsCaptureStarted = false;

            // 状态复位/计时器复位
            timer_RefreshDpsTable.Enabled = false;
            pageHeader_MainHeader.SubText = string.Empty;

            timer_RefreshRunningTime.Stop();

            // 清空解析/重组状态 ——（按你的实际字段名来）
            PacketAnalyzer.ResetCaptureState();

            // 更新网卡设置提示状态
            UpdateNetworkCardSettingTip();
        }

        #endregion


        #region OpenSettingsDialog() 设置/对话框

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

        private void dataDisplay()
        {
            using (var form = new DataDisplaySettings(this))
            {
                string title = Localization.Get("DataDisplaySettings", "请勾选需要显示的统计");
                AntdUI.Modal.open(new Modal.Config(this, title, form, TType.Info)
                {
                    CloseIcon = true,
                    BtnHeight = 0,
                });

                table_DpsDataTable.Columns = ColumnSettingsManager.BuildColumns();

                table_DpsDataTable.StackedHeaderRows = ColumnSettingsManager.BuildStackedHeader();

            }
        }

        private void SetUserUid()
        {

        }
        #endregion
    }
}
