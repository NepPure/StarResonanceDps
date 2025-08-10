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
    public partial class UserUidSet : UserControl
    {
        public UserUidSet(BorderlessForm borderlessForm)
        {
            InitializeComponent();
          
        }

        private void UserUidSet_Load(object sender, EventArgs e)
        {
        }



        private async void button2_Click(object sender, EventArgs e)
        {


            AppConfig.SetValue("UserConfig", "NickName", input2.Text);
            AppConfig.SetValue("UserConfig", "Uid", inputNumber1.Value.ToString());

            this.Dispose();

        }



        private void UserUidSet_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    }
}
