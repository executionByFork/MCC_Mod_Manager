using MCC_Mod_Manager.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace MCC_Mod_Manager
{
    public partial class MasterForm : Form
    {
        public MasterForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            version_lbl.Text = Config.version;
            if (!Config.LoadCfg()) {
                Utility.ShowMsg("MCC Mod Manager cannot load because there are problems with the configuration file.", "Error");
                Environment.Exit(1);
            }
            Backups.LoadBackups();
            Modpacks.LoadModpacks();
            pBar_init();
        }

        ///////////////////////////////////
        /////    GENERAL FUNCTIONS    /////
        ///////////////////////////////////

        public void btnHoverOn(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.Refresh();
        }

        public void btnHoverOff(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            this.Refresh();
        }

        ///////////////////////////////////
        /////         TOP BAR         /////
        ///////////////////////////////////

        Point lastPoint;
        private void topBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void topBar_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void minButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            Config.LoadCfg();
            Backups.LoadBackups();
            Modpacks.LoadModpacks();
        }

        //////////////////////////////////
        /////       CREATE TAB       /////
        //////////////////////////////////

        public List<Panel> createPageList = new List<Panel>(); // used to redraw UI list when deleting one row at a time

        //////////////////////////////////
        /////      PROGRESS BAR      /////
        //////////////////////////////////

        private bool pBar_init()
        {
            for (int i = 0; i < 16; i++) {
                PictureBox x = new PictureBox {
                    Image = Properties.Resources.HaloHelmetIcon_small,
                    Width = 35,
                    Height = 40,
                    Location = new Point((i * 35) + 5, 0),
                    Visible = false
                };
                betterPBar.Controls.Add(x);
            }

            return true;
        }

        private int pBarSections;
        private int pBarCounter = 0;
        public bool pBar_show(int sections)
        {
            pBarSections = sections;
            pBarCounter = 0;

            return true;
        }
        public bool pBar_update()
        {
            // casting to int drops decimal values, flooring the divide
            for (int i = 0; i < (int)(16 / pBarSections); i++) {
                betterPBar.Controls[pBarCounter].Visible = true;
                pBarCounter++;
                Thread.Sleep(10);
                Application.DoEvents();     // I know this isn't "correct" but...
            }
            
            return true;
        }

        public bool pBar_hide()
        {
            foreach (PictureBox helm in betterPBar.Controls.OfType<PictureBox>()) {
                helm.Visible = false;
            }

            return true;
        }
    }
}
