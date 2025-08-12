using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Plugin;

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
                AppConfig.Uid = Convert.ToUInt64(AppConfig.GetValue("UserConfig", "Uid", "0"));
                StatisticData._manager.SetNickname(AppConfig.Uid, AppConfig.NickName);
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

        private bool IsMousePenetrate = false;

        private const int GWL_EXSTYLE = -20;

        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        private void HandleMouseThrough()
        {
            var exStyle = GetWindowLong(Handle, GWL_EXSTYLE);

            // 根据是否穿透组织传递给 SetWindowLong 的3参
            var dwNewLong = IsMousePenetrate
                ? exStyle & ~WS_EX_TRANSPARENT
                : exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT;

            _ = SetWindowLong(Handle, GWL_EXSTYLE, dwNewLong);

            IsMousePenetrate = !IsMousePenetrate;
        }

        #endregion


        #region HandleFormTransparency() 响应窗体透明

        /// <summary>
        /// 是否开启透明
        /// </summary>
        bool hyaline = false;

        private void HandleFormTransparency()
        {
            var opacity = hyaline ? 1 : AppConfig.Transparency / 100;
            Opacity = opacity;

            hyaline = !hyaline;
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
            if (DpsTableDatas.DpsTable.Count >= 0)
            {
                SaveCurrentDpsSnapshot();
            }
            
            // 先停止所有图表的自动刷新
            ChartVisualizationService.StopAllChartsAutoRefresh();
            
            // 在清空数据前，通知图表服务战斗结束
            ChartVisualizationService.OnCombatEnd();
           
            CombatWatch.Restart();
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

        private void HandleClearHistory()
        {
            dropdown_History.Items.Clear();
            HistoricalRecords.Clear();
        }

        #endregion


        #region RefreshHotKeyTips() 更新热键提示

        public void RefreshHotKeyTips()
        {
            label_HotKeyTips.Text = @$"{AppConfig.MouseThroughKey}：鼠标穿透 | {AppConfig.FormTransparencyKey}：窗体透明 | {AppConfig.WindowToggleKey}：开启/关闭 | {AppConfig.ClearDataKey}：清空数据 | {AppConfig.ClearHistoryKey}：清空历史";
        }

        #endregion


        #region SaveCurrentDpsSnapshot() 历史记录：保存/显示

        private void SaveCurrentDpsSnapshot()
        {
            var statsList = StatisticData._manager
                .GetAllPlayers()
                .ToList();
            if (statsList.Count == 0) return;

            string timeOnly = @$"结束时间：{DateTime.Now:HH:mm:ss}";

            HistoricalRecords[timeOnly] = statsList;
            dropdown_History.Items.Add(timeOnly);
            dropdown_History.SelectedValue = -1;
        }

        private void ShowHistoricalDps(string timeKey)
        {
            if (!HistoricalRecords.TryGetValue(timeKey, out var recordList))
            {
                MessageBox.Show($"未找到时间 {timeKey} 的历史记录");
                return;
            }
            if (recordList.Count <= 0) return;
            // 2) 计算最大总伤害，用于归一化进度条
            var totalDamageSum = recordList
                .Where(p => p?.DamageStats != null)
                .Sum(p => (float)p.DamageStats.Total);

            if (totalDamageSum <= 0f) totalDamageSum = 1f;

            // 3) 遍历，新增或更新行
            foreach (var stat in recordList)
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
            }
        }
        #endregion


        #region StartCapture() 抓包：开始/停止/事件/统计

        /// <summary>
        /// 是否开始抓包
        /// </summary>
        private bool IsCaptureStarted { get; set; } = false;

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
            CombatWatch.Restart();
            timer_RefreshRunningTime.Start();
        }

        /// <summary>
        /// 停止抓包
        /// </summary>
        private void StopCapture()
        {
            // 先停止所有图表的自动刷新，防止在停止抓包后继续更新数据
            ChartVisualizationService.StopAllChartsAutoRefresh();
            
            // 保存快照
            if (DpsTableDatas.DpsTable.Count > 0) SaveCurrentDpsSnapshot();
            
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
            label_SettingTip.Text = "00:00";
            CombatWatch.Stop();
            timer_RefreshRunningTime.Stop();

            // 清空解析/重组状态 ——（按你的实际字段名来）
            PacketAnalyzer.ResetCaptureState();
        }

        #endregion


        #region OpenSettingsDialog() 设置/对话框

        /// <summary>
        /// 打开基础设置面板
        /// </summary>
        private void OpenSettingsDialog()
        {
            using var form = new Setup(this);
            form.inputNumber1.Value = (decimal)AppConfig.Transparency;
            form.colorPicker1.Value = AppConfig.DpsColor;
           
            var title = Localization.Get("systemset", "请选择网卡");
            AntdUI.Modal.open(new Modal.Config(this, title, form, TType.Info)
            {
                CloseIcon = true,
                BtnHeight = 0,
                
            });

            AppConfig.Transparency = (double)form.inputNumber1.Value;
            if (AppConfig.Transparency < 10)
            {
                AppConfig.Transparency = 100;
                MessageBox.Show("透明度不能低于10%，已自动设置为100%");
            }

            RefreshHotKeyTips();

            label_SettingTip.Visible = false;
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
            using (var form = new UserUidSet(this))
            {
                string title = Localization.Get("UserUidSet", "");
                AntdUI.Modal.open(new Modal.Config(this, title, form, TType.Info)
                {
                    CloseIcon = false,
                    BtnHeight = 0,
                });

            }
        }
        #endregion
    }
}
