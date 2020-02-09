using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace JMDLoader
{
    public partial class AboutMe : Form
    {
        public AboutMe()
        {
            InitializeComponent();
            Assembly assem = Assembly.GetExecutingAssembly();
            Version v = assem.GetName().Version;
            label2.Text = String.Format($"{label2.Text}", v.Major, v.Minor, v.Build, v.Revision == 1 ? " dev" : "");
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If this program is running incorrectly,\r\nYou can comment in the video which was used to post this.\r\nThank you for reporting!");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
