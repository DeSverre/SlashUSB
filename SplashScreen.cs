using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USkummelB
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        System.Windows.Forms.Timer tmr;

        private void SplashScreen_Shown(object sender, EventArgs e)
        {
            tmr = new();
            tmr.Interval = 3000;
            tmr.Start();
            tmr.Tick += Tmr_Tick;
        }

        private void Tmr_Tick(object? sender, EventArgs e)
        {
            tmr.Stop();
            MainForm mf = new MainForm();
            mf.Show();
            this.Hide();
        }
    }
}
