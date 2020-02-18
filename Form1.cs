﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace MCC_Mod_Manager {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            version_lbl.Text = Config.version;
            int r = Config.LoadCfg();
            if (r == 3) {
                IO.ShowMsg("MCC Mod Manager cannot load because there are problems with the configuration file.", "Error");
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
            PBar_init();
            tt.SetToolTip(addRowButton, "Select mod file(s) to add");
        }

        ///////////////////////////////////
        /////    GENERAL FUNCTIONS    /////
        ///////////////////////////////////

        public void btnHoverOn(object sender, EventArgs e) {
            this.Cursor = Cursors.Hand;
            this.Refresh();
        }

        public void btnHoverOff(object sender, EventArgs e) {
            this.Cursor = Cursors.Default;
            this.Refresh();
        }

        ToolTip tt = new ToolTip {
            AutoPopDelay = 999999999,
            InitialDelay = 100,
            ReshowDelay = 300,
            // Force the ToolTip text to be displayed whether or not the form is active.
            ShowAlways = true
        };

        ///////////////////////////////////
        /////         TOP BAR         /////
        ///////////////////////////////////

        Point lastPoint;
        private void topBar_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void topBar_MouseDown(object sender, MouseEventArgs e) {
            lastPoint = new Point(e.X, e.Y);
        }

        private void exitButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void minButton_Click(object sender, EventArgs e) {
            WindowState = FormWindowState.Minimized;
        }

        private void refreshButton_Click(object sender, EventArgs e) {
            Config.LoadCfg();
            Backups.LoadBackups();
            Modpacks.LoadModpacks();
        }

        //////////////////////////////////
        /////        HOME TAB        /////
        //////////////////////////////////

        private void homeTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.WhiteSmoke;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = true;
            createPanel.Visible = false;
            configPanel.Visible = false;
            backupPanel.Visible = false;
        }

        private void selectEnabled_chb_CheckedChanged(object sender, EventArgs e) {
            foreach (CheckBox chb in modListPanel.Controls.OfType<CheckBox>()) {
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (Config.IsPatched(modpackname)) {
                    chb.Checked = ((CheckBox)sender).Checked;
                }
            }
        }

        public bool selectEnabled_checked() {
            return selectEnabled_chb.Checked;
        }

        private void patchButton_Click(object sender, EventArgs e) {
            Modpacks.RunPatchUnpatch(modListPanel.Controls.OfType<CheckBox>());
        }

        private void manualOverride_CheckedChanged(object sender, EventArgs e) {
            if (manualOverride.Checked == false) {   // make warning only show if checkbox is getting enabled
                Modpacks.LoadModpacks();
                return;
            } else {
                DialogResult ans = IO.ShowMsg("Please do not mess with this unless you know what you are doing or are trying to fix a syncing issue.\r\n\r\n" +
                    "This option allows you to click the red/green icons beside modpack entries to force the mod manager to flag a modpack as enabled/disabled. " +
                    "This does not make changes to files, but it does make the mod manager 'think' that modpacks are/aren't installed." +
                    "\r\n\r\nEnable this feature?", "Question");
                if (ans == DialogResult.No) {
                    manualOverride.Checked = false;
                    return;
                }

                Modpacks.LoadModpacks();
            }
        }

        public bool ManualOverrideEnabled() {
            return manualOverride.Checked;
        }

        private void DelModpack_Click(object sender, EventArgs e) {
            Modpacks.DelModpack(modListPanel.Controls.OfType<CheckBox>());
        }

        public int ModListPanel_getCount() {
            return modListPanel.Controls.OfType<CheckBox>().Count();
        }

        public void ModListPanel_clear() {
            modListPanel.Controls.Clear();
        }

        public void ModListPanel_add(string modpackName, bool versionMatches) {
            CheckBox chb = new CheckBox {
                AutoSize = true,
                Text = Config.dirtyPadding + modpackName,
                Location = new Point(60, (ModListPanel_getCount() * 20) + 1),
                Checked = Config.IsPatched(modpackName) && selectEnabled_checked()
            };
            PictureBox p = new PictureBox {
                Width = 15,
                Height = 15,
                Location = new Point(15, (ModListPanel_getCount() * 20) + 1),
                Image = Config.IsPatched(modpackName) ? Properties.Resources.greenDot_15px : Properties.Resources.redDot_15px
            };
            PictureBox c = new PictureBox {
                Width = 15,
                Height = 15,
                Location = new Point(37, (ModListPanel_getCount() * 20) + 1),
                Image = Properties.Resources.caution_15px,
                Visible = !versionMatches
            };
            if (ManualOverrideEnabled()) {
                p.Click += Modpacks.ForceModpackState;
                p.MouseEnter += btnHoverOn;
                p.MouseLeave += btnHoverOff;
            }

            modListPanel.Controls.Add(p);
            modListPanel.Controls.Add(c);
            tt.SetToolTip(c, "This modpack was made for a different version of MCC and may cause issues if installed.");
            modListPanel.Controls.Add(chb);
        }

        //////////////////////////////////
        /////       CREATE TAB       /////
        //////////////////////////////////

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

        private Panel MakeRow(string filepath, string type) {
            PictureBox del = new PictureBox();
            del.Image = del.ErrorImage;    // bit of a hack to get the error image to appear
            del.Width = 14;
            del.Height = 16;
            del.MouseEnter += btnHoverOn;
            del.MouseLeave += btnHoverOff;
            del.Click += deleteRow;
            if (type == "normal") {
                del.Location = Config.delBtnPoint;
            } else {
                del.Location = Config.delBtnPointAlt;
            }

            TextBox txt1 = new TextBox {
                Width = 180,
                Text = filepath,
                Location = Config.sourceTextBoxPoint
            };

            Button btn1 = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "...",
                Location = Config.sourceBtnPoint,
                Tag = "btn1"
            };
            btn1.Click += Modpacks.Create_fileBrowse;
            tt.SetToolTip(btn1, "Select modded file or .asmp file");

            Label lbl = new Label {
                Width = 33,
                Font = Config.arrowFont,
                Text = ">>",
                Location = (type == "normal") ? Config.arrowPoint : Config.arrowPointAlt
            };

            TextBox txt2 = new TextBox {
                Width = 180,
                Location = (type == "normal") ? Config.destTextBoxPoint : Config.destTextBoxPointAlt
            };

            Button btn2 = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "...",
                Location = (type == "normal") ? Config.destBtnPoint : Config.destBtnPointAlt,
                Tag = "btn2"
            };
            btn2.Click += Modpacks.Create_fileBrowse;
            tt.SetToolTip(btn2, "Select output location");

            int offset = 5;
            foreach (Panel row in createFilesPanel.Controls.OfType<Panel>()) {
                if ((string)row.Tag == "normal") {
                    offset += 27;
                } else {    // Tagged as alt
                    offset += 54;
                }
            }

            Panel p = new Panel {
                Width = 500,
                Height = (type == "normal") ? 25 : 50,
                Location = new Point(10, offset),
                Tag = type
            };

            if (type == "alt") {
                TextBox txt3 = new TextBox {
                    Width = 180,
                    Location = Config.origTextBoxPoint
                };

                Button btn3 = new Button {
                    BackColor = SystemColors.ButtonFace,
                    Width = 39,
                    Font = Config.btnFont,
                    Text = "...",
                    Location = Config.origBtnPoint,
                    Tag = "btn3"
                };
                btn3.Click += Modpacks.Create_fileBrowse;
                tt.SetToolTip(btn3, "Select unmodified map file");

                p.Controls.Add(txt3);
                p.Controls.Add(btn3);
            }

            p.Controls.Add(del);
            p.Controls.Add(txt1);
            p.Controls.Add(btn1);
            p.Controls.Add(lbl);
            p.Controls.Add(txt2);
            p.Controls.Add(btn2);

            return p;
        }

        public void SwapRowType(Panel p) {
            if ((string)p.Tag == "normal") {
                p.Height = 50;
                p.GetChildAtPoint(Config.delBtnPoint).Location = Config.delBtnPointAlt;
                // Retrieving label by coords doesn't work for some reason
                p.Controls.OfType<Label>().First().Location = Config.arrowPointAlt;
                p.GetChildAtPoint(Config.destTextBoxPoint).Location = Config.destTextBoxPointAlt;
                p.GetChildAtPoint(Config.destBtnPoint).Location = Config.destBtnPointAlt;
                p.Tag = "alt";

                TextBox txt3 = new TextBox {
                    Width = 180,
                    Location = Config.origTextBoxPoint
                };

                Button btn3 = new Button {
                    BackColor = SystemColors.ButtonFace,
                    Width = 39,
                    Font = Config.btnFont,
                    Text = "...",
                    Location = Config.origBtnPoint,
                    Tag = "btn3"
                };
                btn3.Click += Modpacks.Create_fileBrowse;

                p.Controls.Add(txt3);
                p.Controls.Add(btn3);
            } else {    // Tagged as alt
                p.Height = 25;
                p.GetChildAtPoint(Config.delBtnPointAlt).Location = Config.delBtnPoint;
                // Retrieving label by coords doesn't work for some reason
                p.Controls.OfType<Label>().First().Location = Config.arrowPoint;
                p.GetChildAtPoint(Config.destTextBoxPointAlt).Location = Config.destTextBoxPoint;
                p.GetChildAtPoint(Config.destBtnPointAlt).Location = Config.destBtnPoint;
                p.Tag = "normal";
            }
            redrawCreatePanel();
        }

        public void redrawCreatePanel() {
            int offset = 5;
            foreach (Panel panel in createFilesPanel.Controls.OfType<Panel>()) {
                panel.Location = new Point(10, offset);
                if ((string)panel.Tag == "normal") {
                    offset += 27;
                } else {    // Tagged as alt
                    offset += 54;
                }
            }
        }

        private void addRowButton_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}",  // using the GUID to access 'This PC' folder
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK) {
                foreach (string file in ofd.FileNames) {
                    string type;
                    if (Path.GetExtension(file) == ".asmp") {
                        type = "alt";
                    } else {
                        type = "normal";
                    }
                    createFilesPanel.Controls.Add(MakeRow(file, type));
                }
            }
        }

        private void deleteRow(object sender, EventArgs e) {
            createFilesPanel.Controls.Remove((Panel)((PictureBox)sender).Parent);
            redrawCreatePanel();
        }

        private void createModpackBtn_Click(object sender, EventArgs e) {
            Modpacks.CreateModpack(modpackName_txt.Text, createFilesPanel.Controls.OfType<Panel>().ToList());
        }

        private void clearBtn_Click(object sender, EventArgs e) {
            resetCreateModpacksTab();
        }

        public void resetCreateModpacksTab() {
            createFilesPanel.Controls.Clear();
            modpackName_txt.Text = "";
        }

        //////////////////////////////////
        /////       CONFIG TAB       /////
        //////////////////////////////////

        private void configTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.WhiteSmoke;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = false;
            createPanel.Visible = false;
            configPanel.Visible = true;
            backupPanel.Visible = false;
        }

        private void cfgFolderBrowseBtn_Click(object sender, EventArgs e) {
            var dialog = new FolderSelectDialog {
                InitialDirectory = Config.MCC_home,
                Title = "Select a folder"
            };
            if (dialog.Show(Handle)) {
                ((Button)sender).Parent.GetChildAtPoint(new Point(5, 3)).Text = dialog.FileName;
            }
        }

        public string cfgTextBox1Text {
            set {
                cfgTextBox1.Text = value;
            }
        }
        public string cfgTextBox2Text {
            set {
                cfgTextBox2.Text = value;
            }
        }
        public string cfgTextBox3Text {
            set {
                cfgTextBox3.Text = value;
            }
        }
        public bool delOldBaks {
            set {
                delOldBaks_chb.Checked = value;
            }
        }

        private void CfgUpdateBtn_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(cfgTextBox1.Text) || string.IsNullOrEmpty(cfgTextBox2.Text) || string.IsNullOrEmpty(cfgTextBox3.Text)) {
                IO.ShowMsg("Config entries must not be empty.", "Error");
                return;
            }

            if (!Config.ChkHomeDir(cfgTextBox1.Text)) {
                IO.ShowMsg("It seems you have selected the wrong MCC install directory. " +
                    "Please make sure to select the folder named 'Halo The Master Chief Collection' in your Steam files.", "Error");
                cfgTextBox1.Text = Config.MCC_home;
                return;
            }
            Config.MCC_home = cfgTextBox1.Text;
            Config.Backup_dir = cfgTextBox2.Text;
            Config.Modpack_dir = cfgTextBox3.Text;
            Config.DeleteOldBaks = delOldBaks_chb.Checked;

            Config.SaveCfg();

            IO.ShowMsg("Config Updated!", "Info");
        }

        private void resetApp_Click(object sender, EventArgs e) {
            DialogResult ans = IO.ShowMsg("WARNING: This dangerous, and odds are you don't need to do it." +
                "\r\n\r\nThis button will reset the application state, so that the mod manager believes your Halo install is COMPLETELY unmodded. It will " +
                "delete ALL of your backups, and WILL NOT restore them beforehand. This is to reset the app to a default state and flush out any broken files." +
                "\r\n\r\nAre you sure you want to continue?", "Question");
            if (ans == DialogResult.No) {
                return;
            }

            Config.DoResetApp();
        }

        //////////////////////////////////
        /////       BACKUP TAB       /////
        //////////////////////////////////

        private void backupTab_Click(object sender, EventArgs e) {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.WhiteSmoke;

            homePanel.Visible = false;
            createPanel.Visible = false;
            configPanel.Visible = false;
            backupPanel.Visible = true;
        }

        private void makeBakBtn_Click(object sender, EventArgs e) {
            Backups.NewBackup();
        }

        private void restoreSelectedBtn_Click(object sender, EventArgs e) {
            Backups.RestoreSelected(bakListPanel.Controls.OfType<CheckBox>());
        }

        private void restoreAllBaksBtn_Click(object sender, EventArgs e) {
            Backups.RestoreAll();
        }

        private void delSelectedBak_Click(object sender, EventArgs e) {
            Backups.DeleteSelected(bakListPanel.Controls.OfType<CheckBox>().ToList());
        }

        private void delAllBaksBtn_Click(object sender, EventArgs e) {
            Backups.DeleteAll(false);
        }

        private void fullBakPath_chb_CheckedChanged(object sender, EventArgs e) {
            Config.fullBakPath = fullBakPath_chb.Checked;
            if (Config.fullBakPath) {   // swapping from filename to full path
                foreach (CheckBox chb in bakListPanel.Controls.OfType<CheckBox>()) {
                    chb.Text = Config.dirtyPadding + Backups.GetBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                }
            } else {    // swapping from full path to filename
                foreach (CheckBox chb in bakListPanel.Controls.OfType<CheckBox>()) {
                    chb.Text = Config.dirtyPadding + Backups._baks[chb.Text.Replace(Config.dirtyPadding, "")];
                }
            }
        }

        public bool fullBakPath_Checked() {
            return fullBakPath_chb.Checked;
        }

        public int bakListPanel_getCount() {
            return bakListPanel.Controls.Count;
        }

        public void bakListPanel_clear() {
            bakListPanel.Controls.Clear();
        }

        public void bakListPanel_add(CheckBox chb) {
            bakListPanel.Controls.Add(chb);
        }

        //////////////////////////////////
        /////      PROGRESS BAR      /////
        //////////////////////////////////

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

            return true;
        }
    }
}
