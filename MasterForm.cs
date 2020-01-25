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
        /////        HOME TAB        /////
        //////////////////////////////////

        public bool ManualOverrideEnabled()
        {
            return manualOverride.Checked;
        }

        public int ModListPanel_getCount()
        {
            return modListPanel.Controls.OfType<CheckBox>().Count();
        }

        public void ModListPanel_clear()
        {
            modListPanel.Controls.Clear();
        }

        public void ModListPanel_add(PictureBox p, CheckBox chb)
        {
            modListPanel.Controls.Add(p);
            modListPanel.Controls.Add(chb);
        }

        //////////////////////////////////
        /////       CREATE TAB       /////
        //////////////////////////////////

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
            btn1.Click += Modpacks.Create_fileBrowse1;
            btn1.Location = Config.sourceBtnPoint;

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
            btn2.Click += Modpacks.Create_fileBrowse2;
            btn2.Location = Config.destBtnPoint;

            Panel p = new Panel {
                Width = 500,
                Height = 25,
                Location = new Point(10, (createFilesPanel.Controls.Count * 25) + 5)
            };
            p.Controls.Add(del);
            p.Controls.Add(txt1);
            p.Controls.Add(btn1);
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
                createPageList[i].Location = new Point(10, (createFilesPanel.Controls.Count * 25) + 5);
                createFilesPanel.Controls.Add(createPageList[i]);
            }
        }

        private void createModpackBtn_Click(object sender, EventArgs e)
        {
            Modpacks.CreateModpack(modpackName_txt.Text, createFilesPanel.Controls.OfType<Panel>());
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

        //////////////////////////////////
        /////       BACKUP TAB       /////
        //////////////////////////////////


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
