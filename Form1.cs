using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace MCC_Mod_Manager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            version_lbl.Text = Config.version;
            Config.form1 = this;
            if (!Config.loadCfg()) {
                showMsg("MCC Mod Manager cannot load because there are problems with the configuration file.", "Error");
                Environment.Exit(1);
            }
            Backups.form1 = this;
            Backups.loadBackups();
            Modpacks.form1 = this;
            Modpacks.loadModpacks();
            AssemblyPatching.form1 = this;
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

        public DialogResult showMsg(string msg, string type)
        {
            if (type == "Info") {
                return MessageBox.Show(
                    msg, "Info", MessageBoxButtons.OK,
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1
                );
            } else if (type == "Question") {
                return MessageBox.Show(
                    msg, "Question", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1
                );
            } else if (type == "Warning") {
                return MessageBox.Show(
                    msg, "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1
                );
            } else if (type == "Error") {
                return MessageBox.Show(
                    msg, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1
                );
            }
            throw new FormatException("Please notify the developer: " + type + " is not a valid type for showMsg.");
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

        private void refreshButton_Click(object sender, EventArgs e)
        {
            Config.loadCfg();
            Backups.loadBackups();
            Modpacks.loadModpacks();
        }

        //////////////////////////////////
        /////        HOME TAB        /////
        //////////////////////////////////

        private void homeTab_Click(object sender, EventArgs e)
        {
            homeTab.BackColor = Color.WhiteSmoke;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = true;
            createPanel.Visible = false;
            configPanel.Visible = false;
            backupPanel.Visible = false;
        }

        private void selectEnabled_chb_CheckedChanged(object sender, EventArgs e)
        {
            foreach (CheckBox chb in modListPanel.Controls.OfType<CheckBox>()) {
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (Config.isPatched(modpackname)) {
                    chb.Checked = ((CheckBox)sender).Checked;
                }
            }
        }

        public bool selectEnabled_checked()
        {
            return selectEnabled_chb.Checked;
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            Modpacks.runPatchUnpatch(modListPanel.Controls.OfType<CheckBox>());
        }

        private void manualOverride_CheckedChanged(object sender, EventArgs e)
        {
            if (manualOverride.Checked == false) {   // make warning only show if checkbox is getting enabled
                return;
            }

            DialogResult ans = showMsg("Please do not mess with this unless you know what you are doing or are trying to fix a syncing issue.\r\n\r\n" +
                "This option allows you to click the red/green icons beside modpack entries to force the mod manager to flag a modpack as enabled/disabled. " +
                "This does not make changes to files, but it does make the mod manager 'think' that modpacks are/aren't installed. If the game was just patched, " +
                "you should use the 'Reset App' button in the Config tab instead.\r\n\r\nEnable this feature?", "Question");
            if (ans == DialogResult.No) {
                manualOverride.Checked = false;
                return;
            }

            Modpacks.loadModpacks();
        }

        public bool manualOverrideEnabled()
        {
            return manualOverride.Checked;
        }

        private void delModpack_Click(object sender, EventArgs e)
        {
            Modpacks.delModpack(modListPanel.Controls.OfType<CheckBox>());
        }

        public int modListPanel_getCount()
        {
            return modListPanel.Controls.OfType<CheckBox>().Count();
        }

        public void modListPanel_clear()
        {
            modListPanel.Controls.Clear();
        }

        public void modListPanel_add(string modpackName, bool versionMatches)
        {   //"This modpack was made for a different version of MCC and may cause issues if installed."
            CheckBox chb = new CheckBox {
                AutoSize = true,
                Text = Config.dirtyPadding + modpackName,
                Location = new Point(60, (modListPanel_getCount() * 20) + 1),
                Checked = Config.isPatched(modpackName) && selectEnabled_checked()
            };
            PictureBox p = new PictureBox {
                Width = 15,
                Height = 15,
                Location = new Point(15, (modListPanel_getCount() * 20) + 1),
                Image = Config.isPatched(modpackName) ? Properties.Resources.greenDot_15px : Properties.Resources.redDot_15px
            };
            PictureBox c = new PictureBox {
                Width = 15,
                Height = 15,
                Location = new Point(37, (modListPanel_getCount() * 20) + 1),
                Image = Properties.Resources.caution_15px,
                Visible = !versionMatches
            };
            if (manualOverrideEnabled()) {
                p.Click += Modpacks.forceModpackState;
                p.MouseEnter += btnHoverOn;
                p.MouseLeave += btnHoverOff;
            }

            modListPanel.Controls.Add(p);
            modListPanel.Controls.Add(c);
            modListPanel.Controls.Add(chb);
        }

        //////////////////////////////////
        /////       CREATE TAB       /////
        //////////////////////////////////

        private void CreateTab_Click(object sender, EventArgs e)
        {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.WhiteSmoke;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = false;
            createPanel.Visible = true;
            configPanel.Visible = false;
            backupPanel.Visible = false;
        }

        private List<Panel> createPageList = new List<Panel>(); // used to redraw UI list when deleting one row at a time
        private void addRowButton_Click(object sender, EventArgs e)
        {
            PictureBox del = new PictureBox();
            del.Image = del.ErrorImage;    // bit of a hack to get the error image to appear
            del.Width = 14;
            del.Height = 16;
            del.MouseEnter += btnHoverOn;
            del.MouseLeave += btnHoverOff;
            del.Click += deleteRow;
            del.Location = Config.delBtnPoint;

            TextBox txt1 = new TextBox {
                Width = 180,
                Location = Config.sourceTextBoxPoint
            };

            Button btn1 = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "..."
            };
            btn1.Click += Modpacks.create_fileBrowse1;
            btn1.Location = Config.sourceBtnPoint;

            TextBox txt_orig = new TextBox {
                Width = 180,
                Location = Config.origTextBoxPoint,
                Text = "Not necessary",
                Enabled = false
            };

            Button btn_orig = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "...",
                Enabled = false
            };
            btn_orig.Click += Modpacks.create_fileBrowse_orig;
            btn_orig.Location = Config.origBtnPoint;

            Label lbl = new Label {
                Width = 33,
                Font = Config.arrowFont,
                Text = ">>",
                Location = Config.arrowPoint
            };

            TextBox txt2 = new TextBox {
                Width = 180,
                Location = Config.destTextBoxPoint
            };

            Button btn2 = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "..."
            };
            btn2.Click += Modpacks.create_fileBrowse2;
            btn2.Location = Config.destBtnPoint;

            Panel p = new Panel {
                Width = 500,
                Height = 50,
                Location = new Point(10, (createFilesPanel.Controls.Count * 55) + 5)
            };
            p.Controls.Add(del);
            p.Controls.Add(txt1);
            p.Controls.Add(btn1);
            p.Controls.Add(txt_orig);
            p.Controls.Add(btn_orig);
            p.Controls.Add(lbl);
            p.Controls.Add(txt2);
            p.Controls.Add(btn2);

            createPageList.Add(p);
            createFilesPanel.Controls.Add(p);
        }

        private void deleteRow(object sender, EventArgs e)
        {
            createPageList.Remove((Panel)((PictureBox)sender).Parent);
            createFilesPanel.Controls.Clear();
            for (int i = 0; i < createPageList.Count; i++) {
                createPageList[i].Location = new Point(10, (createFilesPanel.Controls.Count * 55) + 5);
                createFilesPanel.Controls.Add(createPageList[i]);
            }
        }

        private void createModpackBtn_Click(object sender, EventArgs e)
        {
            Modpacks.createModpack(modpackName_txt.Text, createFilesPanel.Controls.OfType<Panel>());
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            resetCreateModpacksTab();
        }

        public void resetCreateModpacksTab()
        {
            createFilesPanel.Controls.Clear();
            createPageList = new List<Panel>(); // garbage collector magic
            modpackName_txt.Text = "";
        }

        //////////////////////////////////
        /////       CONFIG TAB       /////
        //////////////////////////////////

        private void configTab_Click(object sender, EventArgs e)
        {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.WhiteSmoke;
            backupTab.BackColor = Color.DarkGray;

            homePanel.Visible = false;
            createPanel.Visible = false;
            configPanel.Visible = true;
            backupPanel.Visible = false;
        }

        private void cfgFolderBrowseBtn_Click(object sender, EventArgs e)
        {
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

        private void cfgUpdateBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cfgTextBox1.Text) || string.IsNullOrEmpty(cfgTextBox2.Text) || string.IsNullOrEmpty(cfgTextBox3.Text)) {
                showMsg("Config entries must not be empty.", "Error");
                return;
            }

            if (!Config.chkHomeDir(cfgTextBox1.Text)) {
                showMsg("It seems you have selected the wrong MCC install directory. " +
                    "Please make sure to select the folder named 'Halo The Master Chief Collection' in your Steam files.", "Error");
                cfgTextBox1.Text = Config.MCC_home;
                return;
            }
            Config.MCC_home = cfgTextBox1.Text;
            Config.backup_dir = cfgTextBox2.Text;
            Config.modpack_dir = cfgTextBox3.Text;
            Config.deleteOldBaks = delOldBaks_chb.Checked;

            Config.saveCfg();

            showMsg("Config Updated!", "Info");
        }

        private void resetApp_Click(object sender, EventArgs e)
        {
            DialogResult ans = showMsg("WARNING: This should only be used after an offical MCC update has been applied." +
                "\r\n\r\nThis button will reset the application state, so that the mod manager believes your Halo install is COMPLETELY unmodded. It will " +
                "delete ALL of your backups, and WILL NOT restore them beforehand. This is because after an offical update, the backup files will be old." +
                "\r\n\r\nAre you sure you want to continue?", "Question");
            if (ans == DialogResult.No) {
                return;
            }

            Config.doResetApp();
        }

        //////////////////////////////////
        /////       BACKUP TAB       /////
        //////////////////////////////////

        private void backupTab_Click(object sender, EventArgs e)
        {
            homeTab.BackColor = Color.DarkGray;
            createTab.BackColor = Color.DarkGray;
            configTab.BackColor = Color.DarkGray;
            backupTab.BackColor = Color.WhiteSmoke;

            homePanel.Visible = false;
            createPanel.Visible = false;
            configPanel.Visible = false;
            backupPanel.Visible = true;
        }

        private void makeBakBtn_Click(object sender, EventArgs e)
        {
            Backups.newBackup();
        }

        private void restoreSelectedBtn_Click(object sender, EventArgs e)
        {
            Backups.restoreSelected(bakListPanel.Controls.OfType<CheckBox>());
        }

        private void restoreAllBaksBtn_Click(object sender, EventArgs e)
        {
            Backups.restoreAll();
        }

        private void delSelectedBak_Click(object sender, EventArgs e)
        {
            Backups.deleteSelected(bakListPanel.Controls.OfType<CheckBox>());
        }

        private void delAllBaksBtn_Click(object sender, EventArgs e)
        {
            Backups.deleteAll(false);
        }

        private void fullBakPath_chb_CheckedChanged(object sender, EventArgs e)
        {
            Config.fullBakPath = fullBakPath_chb.Checked;
            if (Config.fullBakPath) {   // swapping from filename to full path
                foreach (CheckBox chb in bakListPanel.Controls.OfType<CheckBox>()) {
                    chb.Text = Config.dirtyPadding + Backups.getBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                }
            } else {    // swapping from full path to filename
                foreach (CheckBox chb in bakListPanel.Controls.OfType<CheckBox>()) {
                    chb.Text = Config.dirtyPadding + Backups._baks[chb.Text.Replace(Config.dirtyPadding, "")];
                }
            }
        }

        public bool fullBakPath_Checked()
        {
            return fullBakPath_chb.Checked;
        }

        public int bakListPanel_getCount()
        {
            return bakListPanel.Controls.Count;
        }

        public void bakListPanel_clear()
        {
            bakListPanel.Controls.Clear();
        }

        public void bakListPanel_add(CheckBox chb)
        {
            bakListPanel.Controls.Add(chb);
        }

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
