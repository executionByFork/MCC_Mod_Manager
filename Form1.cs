using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;


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
            Config.form1 = this;
            Config.loadCfg();
            Backups.form1 = this;
            Backups.loadBackups();
            Modpacks.form1 = this;
            Modpacks.loadModpacks();
        }

        ///////////////////////////////////
        /////    GENERAL FUNCTIONS    /////
        ///////////////////////////////////

        private void btnHoverOn(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.Refresh();
        }

        private void btnHoverOff(object sender, EventArgs e)
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

        private void patchButton_Click(object sender, EventArgs e) {
            Modpacks.patchModpack(modListPanel.Controls.OfType<CheckBox>());
        }

        private void delModpack_Click(object sender, EventArgs e)
        {
            Modpacks.delModpack(modListPanel.Controls.OfType<CheckBox>());
        }

        public int modListPanel_getCount()
        {
            return modListPanel.Controls.Count;
        }

        public void modListPanel_clear()
        {
            modListPanel.Controls.Clear();
        }

        public void modListPanel_add(CheckBox chb)
        {
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

            TextBox txt1 = new TextBox();
            txt1.Width = 180;
            txt1.Location = Config.sourceTextBoxPoint;

            Button btn1 = new Button();
            btn1.BackColor = SystemColors.ButtonFace;
            btn1.Width = 39;
            btn1.Font = Config.btnFont;
            btn1.Text = "...";
            btn1.Click += Modpacks.create_fileBrowse1;
            btn1.Location = Config.sourceBtnPoint;

            Label lbl = new Label();
            lbl.Width = 33;
            lbl.Font = Config.arrowFont;
            lbl.Text = ">>";
            lbl.Location = Config.arrowPoint;

            TextBox txt2 = new TextBox();
            txt2.Width = 180;
            txt2.Location = Config.destTextBoxPoint;

            Button btn2 = new Button();
            btn2.BackColor = SystemColors.ButtonFace;
            btn2.Width = 39;
            btn2.Font = Config.btnFont;
            btn2.Text = "...";
            btn2.Click += Modpacks.create_fileBrowse2;
            btn2.Location = Config.destBtnPoint;

            Panel p = new Panel();
            p.Width = 500;
            p.Height = 25;
            p.Location = new Point(10, (createFilesPanel.Controls.Count * 25) + 5);
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

        public string cfgTextBox1Text
        {
            set { cfgTextBox1.Text = value; }
        }
        public string cfgTextBox2Text
        {
            set { cfgTextBox2.Text = value; }
        }
        public string cfgTextBox3Text
        {
            set { cfgTextBox3.Text = value; }
        }
        public bool delOldBaks
        {
            set { delOldBaks_chb.Checked = value; }
        }

        private void cfgUpdateBtn_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cfgTextBox1.Text) || String.IsNullOrEmpty(cfgTextBox2.Text) || String.IsNullOrEmpty(cfgTextBox3.Text)) {
                MessageBox.Show("Config entries must not be empty.", "Error");
                return;
            }

            if (!Config.chkHomeDir(cfgTextBox1.Text)) {
                MessageBox.Show("It seems you have selected the wrong MCC install directory. " +
                    "Please make sure to select the folder named 'Halo The Master Chief Collection' in your Steam files.", "Error");
                cfgTextBox1.Text = Config.MCC_home;
                return;
            }
            Config.MCC_home = cfgTextBox1.Text;
            Config.backup_dir = cfgTextBox2.Text;
            Config.modpack_dir = cfgTextBox3.Text;
            Config.deleteOldBaks = delOldBaks_chb.Checked;

            Config.saveCfg();

            MessageBox.Show("Config Updated!", "Info");
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
            Backups.deleteAll();
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

        public bool pBar_show(int maxVal)
        {
            pBar.Value = 0;
            pBar.Maximum = maxVal;
            pBar.Visible = true;

            return true;
        }
        public bool pBar_update()
        {
            pBar.PerformStep();
            return true;
        }

        public bool pBar_hide()
        {
            pBar.Visible = false;
            return true;
        }
    }
}
