using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class SkillDetailForm : BorderlessForm
    {
        // 添加折线图成员变量
        private FlatLineChart _dpsTrendChart;
        
        // 添加缺失的isSelect变量
        bool isSelect = false;
        
        public SkillDetailForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
           
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
            
            // 订阅panel7的Resize事件以确保图表正确调整大小
            panel7.Resize += Panel7_Resize;
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
                    // 确保图表尺寸正确
                    var panel = sender as AntdUI.Panel;
                    if (panel != null && panel.Width > 50 && panel.Height > 50) // 增加最小尺寸检查
                    {
                        // 延迟重绘，避免频繁调整大小时的性能问题
                        var resizeTimer = new System.Windows.Forms.Timer { Interval = 150 }; // 稍微延长延迟时间
                        resizeTimer.Tick += (s, args) =>
                        {
                            resizeTimer.Stop();
                            resizeTimer.Dispose();
                            
                            if (_dpsTrendChart != null && !_dpsTrendChart.IsDisposed)
                            {
                                // 字体自适应在控件大小改变时会自动重新计算
                                _dpsTrendChart.Invalidate();
                            }
                        };
                        resizeTimer.Start();
                    }
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
                panel7.Controls.Clear();
                
                // 确保panel7大小正确设置并支持自动调整
                panel7.MinimumSize = new Size(ChartConfigManager.MIN_WIDTH, ChartConfigManager.MIN_HEIGHT);
                panel7.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                
                // 创建DPS趋势折线图，使用统一的配置管理
                _dpsTrendChart = ChartVisualizationService.CreateDpsTrendChart(specificPlayerId: Uid);
                
                // 设置实时刷新回调，传入当前玩家ID
                _dpsTrendChart.SetRefreshCallback(() => {
                    try
                    {
                        // 只有在正在捕获数据时才更新数据点，避免停止抓包后继续显示虚假数据
                        if (ChartVisualizationService.IsCapturing)
                        {
                            ChartVisualizationService.UpdateAllDataPoints();
                        }
                        
                        // 根据当前选择的模式决定显示DPS还是HPS
                        bool showHps = segmented1.SelectIndex != 0; // 0是伤害，1是治疗
                        ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, showHps);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"图表刷新回调出错: {ex.Message}");
                    }
                });
                
                // 添加到panel7
                panel7.Controls.Add(_dpsTrendChart);
                
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
                panel7.Controls.Add(errorLabel);
                
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
                    
                    // 根据当前选择的模式决定显示DPS还是HPS
                    bool showHps = segmented1.SelectIndex != 0; // 0是伤害，1是治疗
                    
                    // 刷新图表，传入当前玩家ID和数据类型
                    ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, showHps);
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
            if (e.Value == 0)
            {
                select1.Items = new AntdUI.BaseCollection() { "按伤害排序", "按秒伤排序", "按命中次数排序", "按暴击率排序" };
            }
            else
            {
                select1.Items = new AntdUI.BaseCollection() { "按治疗量排序", "按HPS排序", "按命中次数排序", "按暴击率排序" };
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

            // 下一轮计时器再恢复
            _suspendUiUpdate = false;

        }




        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
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
                // 更新panel7背景色
                if (panel7 != null)
                    panel7.BackColor = Color.White;
            }
            else
            {
                //深色
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#10529a");
                panel1.Back = panel2.Back = ColorTranslator.FromHtml("#255AD0");
                // 更新panel7背景色
                if (panel7 != null)
                    panel7.BackColor = Color.FromArgb(31, 31, 31);
            }
            
            // 更新折线图主题
            if (_dpsTrendChart != null)
            {
                _dpsTrendChart.IsDarkTheme = !Config.IsLight;
                _dpsTrendChart.Invalidate(); // 强制重绘图表
            }
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //fixedWidth = this.Width;
            
            // 延迟一点时间再调整图表，确保所有控件都已经完成布局
            if (_dpsTrendChart != null)
            {
                this.BeginInvoke(new Action(() => {
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
            if (_dpsTrendChart != null && panel7 != null)
            {
                // 由于使用了Dock.Fill，图表会自动调整大小
                // 这里只需要强制重绘即可
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

        private void select1_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (isSelect) return;
            //DPS排序
            // 判断是否为治疗（true）或伤害（false）
            bool isHeal = segmented1.SelectIndex != 0;

            // 根据当前排序方式，设置委托
            SkillOrderBySelector = e.Value switch
            {
                0 => s => s.Total,
                1 => s => s.TotalDps,
                2 => s => s.HitCount,
                3 => s => s.CritRate,
                _ => s => s.Total  // 默认排序（可选）
            };

            // 更新表格数据
            UpdateSkillTable(Uid, isHeal);
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


          
            object? resourceObj = Properties.Resources.ResourceManager.GetObject(profession);

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
                _dpsTrendChart.SetRefreshCallback(() => {
                    try
                    {
                        // 只有在正在捕获数据时才更新数据点，避免停止抓包后继续显示虚假数据
                        if (ChartVisualizationService.IsCapturing)
                        {
                            ChartVisualizationService.UpdateAllDataPoints();
                        }
                        
                        // 根据当前选择的模式决定显示DPS还是HPS
                        bool showHps = segmented1.SelectIndex != 0; // 0是伤害，1是治疗
                        ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, Uid, showHps);
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
    }
}
