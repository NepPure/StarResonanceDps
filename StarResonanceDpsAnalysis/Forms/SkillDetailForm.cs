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
        public SkillDetailForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
          
            ToggleTableView();
        }

        private int fixedWidth = 1225;//窗体宽度
        private void SkillDetailForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        
            isSelect = true;
            select1.Items = new AntdUI.BaseCollection() { "按伤害排序", "按秒伤排序", "按命中次数排序", "按暴击率排序" };
            select1.SelectedIndex = 0;
            isSelect = false;

        
      
         

        }

        private bool _suspendUiUpdate = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_suspendUiUpdate) return;

            SelectDataType();
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
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            fixedWidth = this.Width;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Width = fixedWidth;
        }
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTTOP = 12;
            const int HTBOTTOM = 15;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST)
            {
                int result = m.Result.ToInt32();

                // 禁止左右和四角的拖动，只允许上下拖动
                if (result == HTLEFT || result == HTRIGHT ||
                    result == HTTOPLEFT || result == HTTOPRIGHT ||
                    result == HTBOTTOMLEFT || result == HTBOTTOMRIGHT)
                {
                    m.Result = IntPtr.Zero; // 禁用这些区域
                }
            }
        }
        bool isSelect = false;
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
    }
}
