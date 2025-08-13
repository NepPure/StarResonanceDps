using AntdUI;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Forms.PopUp;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Properties;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class DpsStatistics : BorderlessForm
    {
        public DpsStatistics()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }

        private void DpsStatistics_Load(object sender, EventArgs e)
        {

        }


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

        #region 切换显示类型（支持单次/全程联动）
        List<string> singleLabels = new() { "单次伤害", "单次治疗", "单次承伤" };
        List<string> totalLabels = new() { "全程伤害", "全程治疗", "全程承伤" };

        int currentIndex = 0;        // 当前类别：0伤害/1治疗/2承伤
        bool showTotal = false;      // false=单次；true=全程

        private void UpdateHeaderText()
        {
            DamageModeLabel.Text = showTotal ? totalLabels[currentIndex]
                                            : singleLabels[currentIndex];
        }

        // 左切换
        private void LeftHandoffButton_Click(object sender, EventArgs e)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = singleLabels.Count - 1;
            UpdateHeaderText();
        }

        // 右切换
        private void RightHandoffButton_Click(object sender, EventArgs e)
        {
            currentIndex++;
            if (currentIndex >= singleLabels.Count) currentIndex = 0;
            UpdateHeaderText();
        }

        // 单次/全程切换
        private void button3_Click(object sender, EventArgs e)
        {
            showTotal = !showTotal;
            UpdateHeaderText();
        }
        #endregion


        /// <summary>
        /// 清空当前数据数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button_Settings_Click(object sender, EventArgs e)
        {
            var menulist = new IContextMenuStripItem[]
             {
                    new ContextMenuStripItem("基础设置"){ IconSvg = Resources.set_up},
                    new ContextMenuStripItem("主窗体"){ IconSvg = Resources.data_display, },
                    //new ContextMenuStripItem("用户设置"){ IconSvg = Resources.userUid, },
                    new ContextMenuStripItem("统计排除"){ IconSvg = Resources.exclude, },
                    new ContextMenuStripItem("打桩模式"){ IconSvg = Resources.Stakes, },
                     new ContextMenuStripItem("退出"){ IconSvg = Resources.quit, },
             }
            ;

            AntdUI.ContextMenuStrip.open(this, it =>
            {
                switch (it.Text)
                {
                    case "基础设置":
                        OpenSettingsDialog();
                        break;
                    case "数据显示设置":
                        //dataDisplay(); 
                        break;
                    case "数据记录类型":
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
        /// <summary>
        /// 用户设置，但是用不到了
        /// </summary>
        private void SetUserUid()
        {
            if (FormManager.userUidSetForm == null || FormManager.userUidSetForm.IsDisposed)
            {
                FormManager.userUidSetForm = new UserUidSetForm();
            }
            FormManager.userUidSetForm.Show();
        }

        private void button_AlwaysOnTop_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button_AlwaysOnTop, "置顶窗口");


        }

        private void ToolTip(System.Windows.Forms.Control control, string text)
        {

            AntdUI.TooltipComponent tooltip = new AntdUI.TooltipComponent()
            {
                Font = new Font("HarmonyOS Sans SC", 8, FontStyle.Regular),
            };
            tooltip.ArrowAlign = AntdUI.TAlign.TL;
            tooltip.SetTip(control, text);
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button1, "清空当前数据");
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            ToolTip(button3, "点击切换：单次统计/全程统");
        }

        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e)
        {
            var duration = StatisticData._manager.GetFormattedCombatDuration();
            BattleTimeText.Text = duration;

        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (PilingModeCheckbox.Checked)
            {
                if(AppConfig.NickName==null&& AppConfig.Uid==null)
                {
                    PilingModeCheckbox.Checked = false;
                    timer1.Enabled = false;
                    var result = AppMessageBox.ShowMessage("未获取到昵称或者UID，请换个地图后再进协会", this);

                    return;
                }
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
                        if(data)
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

        private void PilingModeCheckbox_CheckedChanged(object sender, BoolEventArgs e)
        {
            TimeSpan duration = StatisticData._manager.GetCombatDuration();//获取时间

            if (e.Value)
            {

                var result = AppMessageBox.ShowMessage("打桩时间为3分钟，需注意以下3点:\n0.:打桩模式开启后只会记录自己的数据\n1.开启后请找协会内最右侧木桩[靠窗的那根]\n2.确保战斗计时为0开启\n3.如果伤害不满意可关闭打桩模式重新勾选\n4.异常数据会被删除\n",this);
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
    }
}
