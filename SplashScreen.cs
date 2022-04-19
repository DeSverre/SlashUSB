using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlashUSB
{
    public partial class SplashScreen : Form
    {
        readonly System.Windows.Forms.Timer tmr = new();

        public SplashScreen()
        {
            InitializeComponent();
        }

        private void SplashScreen_Shown(object sender, EventArgs e)
        {
            tmr.Interval = 2500;
            tmr.Start();
            tmr.Tick += Tmr_Tick;
        }

        private void Tmr_Tick(object? sender, EventArgs e)
        {
            tmr.Stop();
            MainForm mf = new();
            mf.Show();
            this.Hide();
        }
    }
}
