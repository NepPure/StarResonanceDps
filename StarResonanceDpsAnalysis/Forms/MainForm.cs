using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Core;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Properties;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class MainForm : BorderlessForm
    {


        #region ========== 构造与启动加载 ==========

        public MainForm()
        {

            InitializeComponent(); // # WinForms 初始化
            FormGui.SetDefaultGUI(this); // # UI 样式：统一默认外观

            /* Application.ProductVersion 默认会被 MSBuild 附加 Git 哈希, 
             * 如: "1.0.0+123456789acbdef", 
             * 将 + 后面去掉就是项目属性的版本号,
             * 这样可以让生成文件的版本号与标题版本号一致
             * * * * * * * * * * * * * * * * * * * * * * * * * * */
            pageHeader_MainHeader.Text += $" v{Application.ProductVersion.Split('+')[0]}"; // # 版本号展示：去除+hash部分

            //InitTableColumnsConfigAtFirstRun(); // # 列显隐初始化：首次运行建立列配置
            //LoadTableColumnVisibilitySettings(); // # 读取列显隐配置
            //ToggleTableView(); // # 表格视图切换（依配置）
            //LoadFromEmbeddedSkillConfig(); // # 预装载技能元数据 → SkillBook

           
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
          

          

            FormGui.SetColorMode(this, AppConfig.IsLight); // # 主题：主窗体明暗模式

        }


        #endregion


        #region ========== 启动时设备/表格配置 ==========


     
        #endregion

        #region ========== 热键/交互事件 ==========

        #region —— 按钮/复选框/下拉事件 —— 
        private void button_ThemeSwitch_Click(object sender, EventArgs e)
        {
            AppConfig.IsLight = !AppConfig.IsLight; // # 状态翻转：明/暗

            button_ThemeSwitch.Toggle = !AppConfig.IsLight; // # UI同步：按钮切换状态

            FormGui.SetColorMode(this, AppConfig.IsLight);
            FormGui.SetColorMode(FormManager.skillDiary, AppConfig.IsLight);

            FormGui.SetColorMode(FormManager.skillDetailForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.settingsForm, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.dpsStatistics, AppConfig.IsLight);//设置窗体颜色
            FormGui.SetColorMode(FormManager.rankingsForm, AppConfig.IsLight);//设置窗体颜色
          
            // # 注意：部分窗体可能为 null 或已释放，SetColorMode 内部应做空值与IsDisposed判断方可安全调用
        }

        private void button_AlwaysOnTop_Click(object sender, EventArgs e)
        {

        }

        private void dropdown_History_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
          
        }

        private void button_SkillDiary_Click(object sender, EventArgs e)
        {
            if (FormManager.dpsStatistics == null || FormManager.dpsStatistics.IsDisposed)
            {
                FormManager.dpsStatistics = new DpsStatisticsForm(); // # 打开团队统计窗体
            }

            FormManager.dpsStatistics.Show();

        }

        private void button_Settings_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        #endregion
        #endregion


        #region ========== 计时器Tick事件 ==========

        private void timer_RefreshDpsTable_Tick(object sender, EventArgs e)
        {
            // Task.Run(() => RefreshDpsTable()); // # 性能提示：如需异步刷新表格，这里可放开；当前关闭避免并发更新
        }

        private void timer_RefreshRunningTime_Tick(object sender, EventArgs e)
        {


        }

        #endregion

        private void table_DpsDataTable_CellClick(object sender, TableClickEventArgs e)
        {
            if (e.RowIndex == 0) return; // # 表头点击忽略
            //ulong uid = 0;

            //if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed)
            //{
            //    FormManager.skillDetailForm = new SkillDetailForm(); // # 详情窗体：延迟创建
            //}
            //SkillTableDatas.SkillTable.Clear(); // # 清空旧详情数据

            //FormManager.skillDetailForm.Uid = uid;
            ////获取玩家信息
            //var info = StatisticData._manager.GetPlayerBasicInfo(uid); // # 查询玩家基础信息（昵称/战力/职业）
            //FormManager.skillDetailForm.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession);
            //FormManager.skillDetailForm.SelectDataType(); // # 按当前选择的“伤害/治疗/承伤”类型刷新
            //FormManager.skillDetailForm.Show(); // # 显示详情

        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (FormManager.rankingsForm == null || FormManager.rankingsForm.IsDisposed)
            {
                FormManager.rankingsForm = new RankingsForm(); // # 排行窗口：延迟创建
            }
            FormManager.rankingsForm.Show();
        }
    }
}
