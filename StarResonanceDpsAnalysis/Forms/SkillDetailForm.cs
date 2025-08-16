using AntdUI;
using StarResonanceDpsAnalysis.Effects;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.Charts;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Properties;
using System.Runtime.InteropServices;
using static StarResonanceDpsAnalysis.Forms.DpsStatisticsForm;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SkillDetailForm : BorderlessForm
    {
        // 添加折线图成员变量
        private FlatLineChart _dpsTrendChart;
        // 添加条形图和饼图成员变量
        private FlatBarChart _skillDistributionChart;
        private FlatPieChart _critLuckyChart;

        // 添加缺失的isSelect变量
        bool isSelect = false;

        // 添加分割器动态调整相关变量
        private int _lastSplitterPosition = 350; // 记录上次分割器位置，与Designer中的默认值保持一致
        private const int SPLITTER_STEP_PIXELS = 30; // 每30像素触发一次调整
        private const int PADDING_ADJUSTMENT = 15;   // 每次调整PaddingRight的数值

        private void SetDefaultFontFromResources()
        {

            TitleText.Font = AppConfig.TitleFont;
            label1.Font = AppConfig.HeaderText;
            label2.Font = label3.Font = label4.Font = AppConfig.ContentText;
            if (FontLoader.TryLoadFontFromBytes("HarmonyOS Sans11", Resources.HarmonyOS_Sans, 11, out var font))
            {
                label3.Font = label9.Font = font;
            }
            if (FontLoader.TryLoadFontFromBytes("HarmonyOS Sans12", Resources.HarmonyOS_Sans, 11, out var ont))
            {
                NickNameText.Font = font;
            }

            BeatenLabel.Font = AvgDamageText.Font = LuckyDamageText.Font = LuckyTimesLabel.Font =  CritDamageText.Font = NormalDamageText.Font = NumberCriticalHitsLabel.Font = LuckyRate.Font = CritRateText.Font = NumberHitsLabel.Font = TotalDpsText.Font =  TotalDamageText.Font = AppConfig.DigitalFonts;
            table_DpsDetailDataTable.Font = label13.Font = label14.Font = label1.Font = label2.Font = label4.Font = label5.Font = label6.Font = label7.Font = label8.Font= label9.Font =  label17.Font= NumberCriticalHitsText.Font= UidText.Font = PowerText.Font = segmented1.Font = collapse1.Font = AppConfig.ContentText;
        }

        public SkillDetailForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            SetDefaultFontFromResources();


            ToggleTableView();
        }

        private int fixedWidth = 1911;//窗体宽度
        private void SkillDetailForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色

            isSelect = true;
            select1.Items = new AntdUI.BaseCollection() { "按伤害排序", "按秒伤排序", "按命中次数排序", "按暴击率排序" };
            select1.SelectedIndex = 0;
            isSelect = false;

            // 初始化并添加折线图到panel7
            InitializeDpsTrendChart();

            // 初始化并添加条形图和饼图
            InitializeSkillDistributionChart();
            InitializeCritLuckyChart();

            // 订阅panel7的Resize事件以确保图表正确调整大小
            collapseItem1.Resize += Panel7_Resize;

            // 手动绑定splitter1事件，确保事件处理正确
            splitter1.SplitterMoving += splitter1_SplitterMoving;
            splitter1.SplitterMoved += splitter1_SplitterMoved;

            // 设置分割器的最小位置为350，防止向左拖动
            splitter1.Panel1MinSize = 350;

            // 初始化分割器位置跟踪，确保与实际位置同步
            _lastSplitterPosition = splitter1.SplitterDistance;

            // 确保图表初始状态正确 - 基准位置350时，PaddingRight=160，垂直线条=5
            if (_dpsTrendChart != null)
            {
                var offsetFrom350 = splitter1.SplitterDistance - 350;
                var steps = offsetFrom350 / SPLITTER_STEP_PIXELS;
                var initialPadding = Math.Max(10, Math.Min(300, 160 - steps * PADDING_ADJUSTMENT));
                var initialGridLines = Math.Max(3, Math.Min(10, 5 + steps)); // 修改：将最大值从20改为10

                _dpsTrendChart.SetPaddingRight(initialPadding);
                _dpsTrendChart.SetVerticalGridLines(initialGridLines);

                //Console.WriteLine($"初始化图表 - 分割器位置: {splitter1.SplitterDistance}, PaddingRight: {initialPadding}, 垂直线条: {initialGridLines}");
            }
        }

        /// <summary>
        /// panel7大小变化时的处理
        /// </summary>
        private void Panel7_Resize(object sender, EventArgs e)
        {
            if (_dpsTrendChart != null)
            {
                try
                {
                    // 由于使用了Dock.Fill，图表会自动调整大小
                    // 这里只需要延迟重绘以确保布局完成后再刷新
                    var resizeTimer = new System.Windows.Forms.Timer { Interval = 100 };
                    resizeTimer.Tick += (s, args) =>
                    {
                        resizeTimer.Stop();
                        resizeTimer.Dispose();

                        if (_dpsTrendChart != null && !_dpsTrendChart.IsDisposed)
                        {
                            _dpsTrendChart.Invalidate();
                        }
                    };
                    resizeTimer.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"调整图表大小时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 初始化DPS趋势图表
        /// </summary>
        private void InitializeDpsTrendChart()
        {
            try
            {
                // 清空panel7现有控件
                collapseItem1.Controls.Clear();

                // 确保panel7大小正确设置并支持自动调整
                collapseItem1.MinimumSize = new Size(ChartConfigManager.MIN_WIDTH, ChartConfigManager.MIN_HEIGHT);
                collapseItem1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                // 创建DPS趋势折线图，使用统一的配置管理
                _dpsTrendChart = ChartVisualizationService.CreateDpsTrendChart(specificPlayerId: Uid);

                // 设置图表为自适应大小，与其他两个图表保持一致
                _dpsTrendChart.Dock = DockStyle.Fill;

                // 设置实时刷新回调，传入当前玩家ID
                _dpsTrendChart.SetRefreshCallback(() =>
                {
                    try
                    {
                        // 只有在正在捕获数据时才更新数据点，避免停止抓包后继续显示虚假数据
                        if (ChartVisualizationService.IsCapturing)
                        {
                            ChartVisualizationService.UpdateAllDataPoints();
                        }

                        // 根据当前选择的模式决定显示的数据类型
                        var dataType = segmented1.SelectIndex switch
                        {
                            0 => ChartDataType.Damage,      // 伤害
                            1 => ChartDataType.Healing,     // 治疗
                            2 => ChartDataType.TakenDamage, // 承伤
                            _ => ChartDataType.Damage       // 默认伤害
                        };

                        ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, dataType);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"图表刷新回调出错: {ex.Message}");
                    }
                });

                // 添加到panel7
                collapseItem1.Controls.Add(_dpsTrendChart);

                // 确保图表被正确添加后再刷新数据
                Application.DoEvents(); // 让UI更新完成

                // 初始刷新图表数据
                RefreshDpsTrendChart();
            }
            catch (Exception ex)
            {
                // 如果图表初始化失败，显示错误信息
                var errorLabel = new AntdUI.Label
                {
                    Text = $"图表初始化失败: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red,
                    Font = new Font("Microsoft YaHei", 10, FontStyle.Regular)
                };
                collapseItem1.Controls.Add(errorLabel);

                Console.WriteLine($"图表初始化失败: {ex}");
            }
        }

        /// <summary>
        /// 刷新DPS趋势图表数据
        /// </summary>
        private void RefreshDpsTrendChart()
        {
            if (_dpsTrendChart != null)
            {
                try
                {
                    // 只有在正在捕获数据时才更新数据点，避免停止抓包后继续显示虚假数据
                    if (ChartVisualizationService.IsCapturing)
                    {
                        ChartVisualizationService.UpdateAllDataPoints();
                    }

                    // 根据当前选择的模式决定显示的数据类型
                    var dataType = segmented1.SelectIndex switch
                    {
                        0 => ChartDataType.Damage,      // 伤害
                        1 => ChartDataType.Healing,     // 治疗
                        2 => ChartDataType.TakenDamage, // 承伤
                        _ => ChartDataType.Damage       // 默认伤害
                    };

                    // 刷新图表，传入当前玩家ID和数据类型
                    ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, dataType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"刷新DPS趋势图表时出错: {ex.Message}");
                }
            }
        }

        private bool _suspendUiUpdate = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_suspendUiUpdate) return;

            SelectDataType();

            // 图表现在有自己的实时刷新机制，这里只做必要的数据更新检查
            // RefreshDpsTrendChart(); // 移除手动刷新，由图表内部处理
        }

        private void segmented1_SelectIndexChanged(object sender, IntEventArgs e)
        {
            select1.Items.Clear();
            isSelect = true;
            label3.Text = "伤害信息";
            label1.Text = "总伤害";
            label2.Text = "秒伤";
            label4.Text = "暴击率";
            label5.Text = "幸运率";
            switch (e.Value)
            {
                case 0:
                    select1.Items = new AntdUI.BaseCollection() { "按伤害排序", "按秒伤排序", "按命中次数排序", "按暴击率排序" };
                    break;
                case 1:
                    select1.Items = new AntdUI.BaseCollection() { "按治疗量排序", "按HPS排序", "按命中次数排序", "按暴击率排序" };
                    label3.Text = "治疗信息";
                    label1.Text = "总治疗";
                    label2.Text = "秒治疗";
                    label4.Text = "暴击率";
                    label5.Text = "幸运率";
                    break;
                case 2:
                    select1.Items = new AntdUI.BaseCollection() { "按承伤排序", "按秒承伤排序", "按受击次数排序", "按暴击率排序" };
                    label3.Text = "承伤信息";
                    label1.Text = "总承伤";
                    label2.Text = "秒承伤";
                    label4.Text = "最大承伤";
                    label5.Text = "最小承伤";
                    break;
            }

            select1.SelectedValue = select1.Items[0];
            // 手动刷新 UI


            isSelect = false;
            // 暂停一次刷新
            _suspendUiUpdate = true;

            // 切换时清一次技能表，避免残留
            SkillTableDatas.SkillTable.Clear();

            // 立刻按新模式刷新一次
            bool isHeal = segmented1.SelectIndex != 0;
            SelectDataType();

            // 更新图表数据
            UpdateSkillDistributionChart();
            UpdateCritLuckyChart();

            // 下一轮计时器再恢复
            _suspendUiUpdate = false;

        }




        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectDataType();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 停止图表自动刷新
            if (_dpsTrendChart != null)
            {
                _dpsTrendChart.StopAutoRefresh();
            }

            this.Close();
        }

        /// <summary>
        /// 检测窗体变动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillDetailForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                //浅色
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#AED4FB");
                panel1.Back = panel2.Back = ColorTranslator.FromHtml("#67AEF6");
            }
            else
            {
                //深色
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#10529a");
                panel1.Back = panel2.Back = ColorTranslator.FromHtml("#255AD0");

            }

            // 更新折线图主题
            if (_dpsTrendChart != null)
            {
                _dpsTrendChart.IsDarkTheme = !Config.IsLight;
                _dpsTrendChart.Invalidate(); // 强制重绘图表
            }

            // 更新条形图主题
            if (_skillDistributionChart != null)
            {
                _skillDistributionChart.IsDarkTheme = !Config.IsLight;
                _skillDistributionChart.Invalidate();
            }

            // 更新饼图主题
            if (_critLuckyChart != null)
            {
                _critLuckyChart.IsDarkTheme = !Config.IsLight;
                _critLuckyChart.Invalidate();
            }
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //fixedWidth = this.Width;

            // 延迟一点时间再调整图表，确保所有控件都已经完成布局
            if (_dpsTrendChart != null)
            {
                this.BeginInvoke(new Action(() =>
                {
                    _dpsTrendChart.Invalidate();
                    RefreshDpsTrendChart();
                }));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            //this.Width = fixedWidth;

            // 当窗体大小变化时，强制刷新折线图布局
            // 由于使用了Dock.Fill，图表会自动调整大小，只需要重绘即可
            if (_dpsTrendChart != null && collapseItem1 != null)
            {
                _dpsTrendChart.Invalidate();
            }
        }

        //protected override void WndProc(ref System.Windows.Forms.Message m)
        //{
        //    const int WM_NCHITTEST = 0x84;
        //    const int HTTOP = 12;
        //    const int HTBOTTOM = 15;
        //    const int HTLEFT = 10;
        //    const int HTRIGHT = 11;
        //    const int HTTOPLEFT = 13;
        //    const int HTTOPRIGHT = 14;
        //    const int HTBOTTOMLEFT = 16;
        //    const int HTBOTTOMRIGHT = 17;

        //    base.WndProc(ref m);

        //    if (m.Msg == WM_NCHITTEST)
        //    {
        //        int result = m.Result.ToInt32();

        //        // 禁止左右和四角的拖动，只允许上下拖动
        //        if (result == HTLEFT || result == HTRIGHT ||
        //            result == HTTOPLEFT || result == HTTOPRIGHT ||
        //            result == HTBOTTOMLEFT || result == HTBOTTOMRIGHT)
        //        {
        //            m.Result = IntPtr.Zero; // 禁用这些区域
        //        }
        //    }
        //}

        // 建议放在类里（如果还没有）
        // using StarResonanceDpsAnalysis.Plugin.DamageStatistics; // 记得加 using
 

        private void select1_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (isSelect) return;

            // 1) 确定指标（segmented1: 0=伤害 1=治疗 2=承伤）
            MetricType metric = segmented1.SelectIndex switch
            {
                1 => MetricType.Healing,
                2 => MetricType.Taken,
                _ => MetricType.Damage
            };

            // 2) 设置排序（统一返回 double，避免泛型不变性/类型不一致）
            SkillOrderBySelector = e.Value switch
            {
                0 => s => s.Total,       // 总量
                1 => s => s.TotalDps,    // 秒伤
                2 => s => s.HitCount,    // 次数
                3 => s => s.CritRate,    // 暴击率
                _ => s => s.Total
            };

            // 3) 确定数据源（单次/全程）
            SourceType source = FormManager.showTotal ? SourceType.FullRecord : SourceType.Current;

            // 4) 刷新技能表（内部会按 SkillOrderBySelector 排序）
            UpdateSkillTable(Uid, source, metric);

            // （可选）如果需要同时更新右侧图表：
            try { RefreshDpsTrendChart(); } catch { /* 忽略绘图异常 */ }
            UpdateSkillDistributionChart();
            UpdateCritLuckyChart();
        }


        private void panel4_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 更新玩家信息
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="power"></param>
        /// <param name="profession"></param>
        public void GetPlayerInfo(string nickname, int power, string profession)
        {
            // 注释掉玩家名称的显示
            NickNameText.Text = nickname;
            PowerText.Text = power.ToString();
            UidText.Text = Uid.ToString();



            object? resourceObj = Properties.Resources.ResourceManager.GetObject(profession+"10");

            if (resourceObj is byte[] bytes)
            {
                using var ms = new MemoryStream(bytes);
                table_DpsDetailDataTable.BackgroundImage = Image.FromStream(ms);
            }
            else if (resourceObj is Image img)
            {
                table_DpsDetailDataTable.BackgroundImage = img;
            }
            else
            {
                table_DpsDetailDataTable.BackgroundImage = null; // 默认空白
            }

            // 更新玩家信息后，重新初始化图表以显示新玩家的数据
            if (_dpsTrendChart != null)
            {
                // 重新设置刷新回调以使用新的玩家ID
                _dpsTrendChart.SetRefreshCallback(() =>
                {
                    try
                    {
                        // 只有在正在捕获数据时才更新数据点，避免停止抓包后继续显示虚假数据
                        if (ChartVisualizationService.IsCapturing)
                        {
                            ChartVisualizationService.UpdateAllDataPoints();
                        }

                        // 根据当前选择的模式决定显示的数据类型
                        var dataType = segmented1.SelectIndex switch
                        {
                            0 => ChartDataType.Damage,      // 伤害
                            1 => ChartDataType.Healing,     // 治疗
                            2 => ChartDataType.TakenDamage, // 承伤
                            _ => ChartDataType.Damage       // 默认伤害
                        };

                        ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, dataType);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"图表刷新回调出错: {ex.Message}");
                    }
                });

                // 立即刷新图表数据
                RefreshDpsTrendChart();
            }
        }

        /// <summary>
        /// 重置DPS趋势图表（用于数据清空）
        /// </summary>
        public void ResetDpsTrendChart()
        {
            if (_dpsTrendChart != null)
            {
                try
                {
                    // 完全重置图表状态
                    _dpsTrendChart.FullReset();

                    // 使用统一的配置管理器重新应用设置
                    ChartConfigManager.ApplySettings(_dpsTrendChart);

                    // 如果当前正在捕获数据，重新启动自动刷新
                    if (ChartVisualizationService.IsCapturing)
                    {
                        _dpsTrendChart.StartAutoRefresh(ChartConfigManager.REFRESH_INTERVAL);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重置DPS趋势图表时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 初始化暴击率与幸运率条形图（现在用条形图显示暴击率数据）
        /// </summary>
        private void InitializeSkillDistributionChart()
        {
            try
            {
                // 创建暴击率与幸运率条形图
                _skillDistributionChart = new FlatBarChart
                {
                    Dock = DockStyle.Fill,
                    TitleText = "", // 移除标题以增大图表占比
                    XAxisLabel = "",
                    YAxisLabel = "",
                    IsDarkTheme = !Config.IsLight
                };

                // 添加到collapseItem3
                collapseItem3.Controls.Add(_skillDistributionChart);

                // 初始化数据
                UpdateSkillDistributionChart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"暴击率与幸运率图表初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化技能占比饼图（现在用饼图显示技能分布数据）
        /// </summary>
        private void InitializeCritLuckyChart()
        {
            try
            {
                // 创建技能占比饼图
                _critLuckyChart = new FlatPieChart
                {
                    Dock = DockStyle.Fill,
                    TitleText = "", // 移除标题以增大图表占比
                    ShowLabels = true,
                    ShowPercentages = true,
                    IsDarkTheme = !Config.IsLight
                };

                // 添加到collapseItem2
                collapseItem2.Controls.Add(_critLuckyChart);

                // 初始化数据
                UpdateCritLuckyChart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"技能占比图表初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 限制分割器移动并动态调整图表参数
        /// </summary>
        private void splitter1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            // 如果尝试将分割器拖动到小于350的位置，则取消移动
            if (e.SplitX < 350)
            {
                e.Cancel = true;
                return;
            }

            // 计算相对于基准位置350的偏移
            var offsetFrom350 = e.SplitX - 350;
            var steps = offsetFrom350 / SPLITTER_STEP_PIXELS; // 计算移动了多少个30px步长

            // 根据用户需求调整_paddingRight：每向右移动30px，_paddingRight减15
            var newPadding = Math.Max(10, Math.Min(300, 160 - steps * PADDING_ADJUSTMENT));

            // 直接通过分割器位移来控制垂直线条数量
            // 基准：分割器350位置时，垂直线条=5
            // 向右移动增加线条，向左移动减少线条
            // ★ 修改：将最大垂直线条数从20改为10
            var newGridLines = Math.Max(3, Math.Min(10, 5 + steps));

            // 添加调试输出
            //Console.WriteLine($"[DEBUG] splitter1_SplitterMoving - 位置: {e.SplitX}, 偏移: {offsetFrom350}, 步数: {steps}, 新线条数: {newGridLines}");

            if (_dpsTrendChart != null)
            {
                var currentGridLines = _dpsTrendChart.GetVerticalGridLines();
                var currentPadding = _dpsTrendChart.GetPaddingRight();

                //Console.WriteLine($"[DEBUG] 当前参数 - 线条数: {currentGridLines}, PaddingRight: {currentPadding}");

                // 只有当计算出的值与当前值不同时才更新
                if (currentGridLines != newGridLines || currentPadding != newPadding)
                {
                    _dpsTrendChart.SetPaddingRight(newPadding);
                    _dpsTrendChart.SetVerticalGridLines(newGridLines);

                    //Console.WriteLine($"[SUCCESS] 更新成功 - 分割器位置: {e.SplitX}, 移动步数: {steps}, PaddingRight: {newPadding}, 垂直线条: {newGridLines}");
                }
                else
                {
                   // Console.WriteLine($"[SKIP] 参数未变化，跳过更新");
                }
            }
            else
            {
                Console.WriteLine($"[ERROR] _dpsTrendChart 为 null，无法更新参数");
            }
        }

        /// <summary>
        /// 分割器移动完成后的处理
        /// </summary>
        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // 最终确认分割器位置，确保图表参数正确应用
            if (_dpsTrendChart != null)
            {
                // 计算相对于基准位置350的偏移步数
                var offsetFrom350 = e.SplitX - 350;
                var steps = offsetFrom350 / SPLITTER_STEP_PIXELS;

                // 同步最终位置记录
                _lastSplitterPosition = 350 + steps * SPLITTER_STEP_PIXELS;

                // 最终计算参数
                var finalPadding = Math.Max(10, Math.Min(300, 160 - steps * PADDING_ADJUSTMENT));
                // ★ 修改：将最大垂直线条数从20改为10
                var finalGridLines = Math.Max(3, Math.Min(10, 5 + steps));

                // 确保最终设置正确
                _dpsTrendChart.SetPaddingRight(finalPadding);
                _dpsTrendChart.SetVerticalGridLines(finalGridLines);

                // 强制重绘图表以应用新的设置
                _dpsTrendChart.Invalidate();

                //Console.WriteLine($"分割器移动完成 - 实际位置: {e.SplitX}, 基准位置: {_lastSplitterPosition}, 最终PaddingRight: {finalPadding}, 最终垂直线条: {finalGridLines}");
            }
        }

        private void table_DpsDetailDataTable_CellClick(object sender, TableClickEventArgs e)
        {

        }
    }
}
