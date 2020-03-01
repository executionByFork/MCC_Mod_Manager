using MCC_Mod_Manager.Api;
using MCC_Mod_Manager.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace MCC_Mod_Manager {
    public partial class MasterForm : Form {
        public MasterForm() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Program.MasterForm.Size = new System.Drawing.Size(577, 455);
            version_lbl.Text = Config.version;
            int r = Config.LoadCfg();
            if (r == 3) {
                Utility.ShowMsg("MCC Mod Manager cannot load because there are problems with the configuration file.", "Error");
                Environment.Exit(1);
            }
            Backups.LoadBackups();

            if (r == 2) {
                patchButton.Enabled = false;
                delModpack.Enabled = false;
                restoreSelectedBtn.Enabled = false;
                restoreAllBaksBtn.Enabled = false;
                delSelectedBak.Enabled = false;
                delAllBaksBtn.Enabled = false;
                manualOverride.Enabled = false;

                megaCaution.Visible = true;
                tt.SetToolTip(megaCaution, "MCC Mod Manager has detected an update and needs to stabilize the game. Please restart the app.");
            } else if (r == 1) {
                Modpacks.StabilizeGame();
                Backups.LoadBackups();
            }
            Modpacks.LoadModpacks();
            LoadEventHandlers();
            PBar_init();
            tt.SetToolTip(addRowButton, "Select mod file(s) to add");
        }

        #region GENERAL FUNCTIONS

        public void LoadEventHandlers() {
            #region Top Bar

            this.topBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseDown);
            this.topBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseMove);

            this.titleLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseDown);
            this.titleLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseMove);

            this.version_lbl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseDown);
            this.version_lbl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseMove);

            //this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseDown);
            //this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopBar_MouseMove);

            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            this.refreshButton.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.refreshButton.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.minButton.Click += new System.EventHandler(this.MinButton_Click);
            this.minButton.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.minButton.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
            this.exitButton.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.exitButton.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            #endregion

            #region MyMods

            this.patchButton.Click += new System.EventHandler(MyMods.PatchUnpatch_Click);
            this.patchButton.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.patchButton.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.delModpack.Click += new System.EventHandler(MyMods.DeleteSelected_Click);
            this.delModpack.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.delModpack.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.manualOverride.CheckedChanged += new System.EventHandler(MyMods.ManualOverride_CheckedChanged);

            this.selectEnabled_chb.CheckedChanged += new System.EventHandler(MyMods.SelectEnabled_chb_CheckedChanged);

            #endregion

            #region Modpacks

            this.addRowButton.Click += new System.EventHandler(Modpacks.AddRowButton_Click);
            this.addRowButton.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.addRowButton.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.createModpackBtn.Click += new System.EventHandler(Modpacks.CreateModpackBtn_Click);
            this.createModpackBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.createModpackBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.clearBtn.Click += new System.EventHandler(Modpacks.ClearBtn_Click);
            this.clearBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.clearBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            #endregion

            #region Configuration

            this.cfgBrowseBtn1.Click += new System.EventHandler(Config.BrowseFolderBtn_Click);
            this.cfgBrowseBtn2.Click += new System.EventHandler(Config.BrowseFolderBtn_Click);
            this.cfgBrowseBtn3.Click += new System.EventHandler(Config.BrowseFolderBtn_Click);

            this.resetApp.Click += new System.EventHandler(Config.ResetApp_Click);
            this.resetApp.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.resetApp.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.cfgUpdateBtn.Click += new System.EventHandler(Config.UpdateBtn_Click);
            this.cfgUpdateBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.cfgUpdateBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            #endregion

            #region Backups

            this.fullBakPath_chb.CheckedChanged += new System.EventHandler(Backups.ShowFullPathCheckbox_Click);

            this.makeBakBtn.Click += new System.EventHandler(Backups.MakeBakBtn_Click);
            this.makeBakBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.makeBakBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.restoreSelectedBtn.Click += new System.EventHandler(Backups.RestoreSelectedBtn_Click);
            this.restoreSelectedBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.restoreSelectedBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.restoreAllBaksBtn.Click += new System.EventHandler(Backups.RestoreAllBaksBtn_Click);
            this.restoreAllBaksBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.restoreAllBaksBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.delSelectedBak.Click += new System.EventHandler(Backups.DelSelectedBak_Click);
            this.delSelectedBak.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.delSelectedBak.MouseLeave += new System.EventHandler(this.BtnHoverOff);

            this.delAllBaksBtn.Click += new System.EventHandler(Backups.DelAllBaksBtn_Click);
            this.delAllBaksBtn.MouseEnter += new System.EventHandler(this.BtnHoverOn);
            this.delAllBaksBtn.MouseLeave += new System.EventHandler(this.BtnHoverOff);
            #endregion
        }


        public void BtnHoverOn(object sender, EventArgs e) {
            this.Cursor = Cursors.Hand;
            this.Refresh();
        }

        public void BtnHoverOff(object sender, EventArgs e) {
            this.Cursor = Cursors.Default;
            this.Refresh();
        }

        public readonly ToolTip tt = new ToolTip {
            AutoPopDelay = 999999999,
            InitialDelay = 100,
            ReshowDelay = 300,
            // Force the ToolTip text to be displayed whether or not the form is active.
            ShowAlways = true
        };

        #endregion

        #region TOP BAR

        Point lastPoint;
        private void TopBar_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void TopBar_MouseDown(object sender, MouseEventArgs e) {
            lastPoint = new Point(e.X, e.Y);
        }

        private void ExitButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void MinButton_Click(object sender, EventArgs e) {
            WindowState = FormWindowState.Minimized;
        }

        private void RefreshButton_Click(object sender, EventArgs e) {
            Config.LoadCfg();
            Backups.LoadBackups();
            Modpacks.LoadModpacks();
        }

        #endregion

        //////////////////////////////////
        /////        HOME TAB        ///// //MyMods
        //////////////////////////////////

        //Disappearing next PR
        private void HomeTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.WhiteSmoke;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = true;
            createPanel.Visible = false;
            configPanel.Visible = false;
            backupPanel.Visible = false;
        }

        //////////////////////////////////
        /////       CREATE TAB       ///// //Modpacks
        //////////////////////////////////

        //Disappearing next PR
        private void CreateTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.WhiteSmoke;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = false;
            createPanel.Visible = true;
            configPanel.Visible = false;
            backupPanel.Visible = false;
        }

        //////////////////////////////////
        /////       CONFIG TAB       /////
        //////////////////////////////////

        //Disappearing next PR
        private void ConfigTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.WhiteSmoke;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = false;
            createPanel.Visible = false;
            configPanel.Visible = true;
            backupPanel.Visible = false;
        }

        //////////////////////////////////
        /////       BACKUP TAB       /////
        //////////////////////////////////

        //Disappearing next PR
        private void BackupTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.WhiteSmoke;

            homePanel.Visible = false;
            createPanel.Visible = false;
            configPanel.Visible = false;
            backupPanel.Visible = true;
        }

        #region PROGRESS BAR

        private bool PBar_init() {
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
        public bool PBar_show(int sections) {
            pBarSections = sections;
            pBarCounter = 0;
            Program.MasterForm.Size = new System.Drawing.Size(577, 497);
            return true;
        }
        public bool PBar_update() {
            // casting to int drops decimal values, flooring the divide
            for (int i = 0; i < (int)(16 / pBarSections); i++) {
                betterPBar.Controls[pBarCounter].Visible = true;
                pBarCounter++;
                Thread.Sleep(10);
                Application.DoEvents();     // I know this isn't "correct" but...
            }

            return true;
        }

        public bool PBar_hide() {
            foreach (PictureBox helm in betterPBar.Controls.OfType<PictureBox>()) {
                helm.Visible = false;
            }
            Program.MasterForm.Size = new System.Drawing.Size(577, 455);
            return true;
        }

        #endregion
    }
}
