using AntdUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StarResonanceDpsAnalysis.Plugin;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class UserUidSet : BorderlessForm
    {
        public UserUidSet()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
        }

        private void UserUidSet_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
            table1.Columns = new ColumnCollection()
            {
                new Column("uid","用户UID"),
                 new Column("name","用户昵称"),
            };
            table1.Binding(user_tabel);
        }

        private async void table1_CellEditComplete(object sender, ITableEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(user_tabel[e.ColumnIndex - 1].name))
                return;

            string url = "https://api.jx3rec.com/add_player_uid_map";
            var query = new
            {
                uid = user_tabel[e.ColumnIndex - 1].uid,
                name = user_tabel[e.ColumnIndex - 1].name,
            };
            var data = await Common.RequestGet(url, query);
        }

        public static AntdUI.AntList<UserTabel> user_tabel = new AntdUI.AntList<UserTabel>();


        public class UserTabel : AntdUI.NotifyProperty
        {
            public UserTabel(ulong uid, string name = null)
            {
                Uid = uid;
                Name = name;

            }

            public ulong Uid;
            public ulong uid
            {
                get => Uid;
                set
                {
                    if (Uid == value) return;
                    Uid = value;
                    OnPropertyChanged(nameof(uid));
                }
            }


            public string Name;
            public string name
            {
                get => Name;
                set
                {
                    if (Name == value) return;
                    Name = value;
                    OnPropertyChanged(nameof(name));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<PlayerData> statsList = StatisticData._manager
                .GetAllPlayers()
                .ToList();

            foreach (var item in statsList)
            {
                if (!user_tabel.Any(x => x.Uid == item.Uid))
                {
                    if (!string.IsNullOrWhiteSpace(item.Nickname) && item.Nickname != "未知")
                    {
                        user_tabel.Add(new UserTabel(item.Uid, item.Nickname));
                    }
                    else
                    {
                        user_tabel.Add(new UserTabel(item.Uid));
                    }
                }
            }

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (!user_tabel.Any(x => x.Uid == inputNumber1.Value))
            {

                if (inputNumber1.Value == null || input2.Text == null)
                {
                    FormGui.Modal(this, "输入不能为空", "请输入UID和昵称");
                    return;
                }
                string url = "https://api.jx3rec.com/add_player_uid_map";
                var query = new
                {
                    uid = Convert.ToUInt64(inputNumber1.Value),
                    name = input2.Text,
                };
                var data = await Common.RequestGet(url, query);
                if (data["code"].ToString() == "200")
                {
                    user_tabel.Add(new UserTabel(Convert.ToUInt64(inputNumber1.Value), input2.Text));
                }
                else
                {
                    user_tabel.Add(new UserTabel(Convert.ToUInt64(inputNumber1.Value), input2.Text));
                }
            }
            else
            {
                FormGui.Modal(this, "用户UID已存在", "请勿重复添加");
            }
        }

        private bool table1_CellEndEdit(object sender, TableEndEditEventArgs e)
        {

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            user_tabel.Clear();
        }

        private void UserUidSet_FormClosed(object sender, FormClosedEventArgs e)
        {
            user_tabel.Clear();
        }
    }
}
