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

        string cfg_location = @".\MCC_Mod_Manager.cfg";
        Dictionary<string, string> cfg = new Dictionary<string, string>();
        Dictionary<string, string> baks = new Dictionary<string, string>();
        private void Form1_Load(object sender, EventArgs e)
        {
            loadCfg();
            ensureBackupFolderExists();
            loadBackups();
            ensureModpackFolderExists();
            loadModpacks();
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

        private bool ensureModpackFolderExists()
        {
            if (!Directory.Exists(cfg["modpack_dir"])) {
                Directory.CreateDirectory(cfg["modpack_dir"]);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }
        private bool ensureBackupFolderExists()
        {
            if (!Directory.Exists(cfg["backup_dir"])) {
                Directory.CreateDirectory(cfg["backup_dir"]);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        private bool DeleteFile(string path)
        {
            try {
                File.Delete(path);
            } catch (IOException) {
                return false;
            }
            return true;
        }

        private int CopyFile(string src, string dest, bool overwrite)
        {
            if (File.Exists(dest)) {
                if (overwrite) {
                    if (!DeleteFile(dest)) {
                        return 2;   // fail - file in use
                    }
                } else {
                    return 1;   // success - not overwriting the existing file
                }
            }
            try {
                File.Copy(src, dest);
            } catch (IOException) {
                return 3;   // fail - file access error
            }
            return 0;   // success
        }

        private void saveCfg()
        {
            string json = JsonConvert.SerializeObject(cfg, Formatting.Indented);
            using (FileStream fs = File.Create(cfg_location)) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        private bool createDefaultCfg() {
            // default config values
            // TODO: Ask user if they want to use default config first
            cfg["MCC_home"] = @"C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";
            cfg["backup_dir"] = @".\backups";
            cfg["modpack_dir"] = @".\modpacks";
            cfg["deleteOldBaks"] = "false";
            saveCfg();

            return true;
        }

        private bool loadCfg()
        {
            bool err = false;
            if (!File.Exists(cfg_location)) {
                createDefaultCfg();
            } else {
                string json = File.ReadAllText(cfg_location);
                try {
                    Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    cfg["MCC_home"] = values["MCC_home"];
                    cfg["backup_dir"] = values["backup_dir"];
                    cfg["modpack_dir"] = values["modpack_dir"];
                    cfg["deleteOldBaks"] = values["deleteOldBaks"];
                } catch (JsonSerializationException) {
                    err = true;
                    createDefaultCfg();
                } catch (KeyNotFoundException) {
                    err = true;
                    createDefaultCfg();
                }
            }
            if (err) {
                MessageBox.Show("There was an error in your configuration file. A default config has been created. Please review and update it if needed.", "Error");
            }


            // Update config tab
            cfgTextBox1.Text = cfg["MCC_home"];
            cfgTextBox2.Text = cfg["backup_dir"];
            cfgTextBox3.Text = cfg["modpack_dir"];
            if (cfg["deleteOldBaks"] == "true") {
                delOldBaks_chb.Checked = true;
            }

            return true;
        }

        String dirtyPadding = "              ";
        private bool loadModpacks()
        {
            modListPanel.Controls.Clear();

            string[] fileEntries = Directory.GetFiles(cfg["modpack_dir"]);
            foreach (string file in fileEntries) {
                CheckBox chb = new CheckBox();
                chb.AutoSize = true;
                chb.Text = dirtyPadding + Path.GetFileName(file).Replace(".zip", "");
                chb.Location = new Point(30, modListPanel.Controls.Count * 20);

                modListPanel.Controls.Add(chb);
            }

            return true;
        }

        private bool saveBackups()
        {
            string json = JsonConvert.SerializeObject(baks, Formatting.Indented);
            using (FileStream fs = File.Create(cfg["backup_dir"] + @"\backups.cfg")) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
            return true;
        }

        private bool updateBackupList()
        {
            bakListPanel.Controls.Clear();
            foreach (KeyValuePair<string, string> entry in baks) {
                CheckBox chb = new CheckBox();
                chb.AutoSize = true;
                chb.Text = dirtyPadding + entry.Key;
                chb.Location = new Point(30, bakListPanel.Controls.Count * 20);

                bakListPanel.Controls.Add(chb);
            }
            return true;
        }

        private bool loadBackups()
        {
            if (!File.Exists(cfg["backup_dir"] + @"\backups.cfg")) {
                return false;
            }

            string json = File.ReadAllText(cfg["backup_dir"] + @"\backups.cfg");
            try {
                baks = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                updateBackupList();
            } catch (JsonSerializationException) {
                DialogResult ans = MessageBox.Show(
                    "The backup configuration file is corrupted. You may need to verify your game files on steam or reinstall." +
                    "Would you like to delete the corrupted backup config file?",
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (ans == DialogResult.Yes) {
                    if (!DeleteFile(cfg["backup_dir"] + @"\backups.cfg")) {
                        MessageBox.Show("The backup file could not be deleted. Is it open somewhere?", "Error");
                    }
                }
            }

            return true;
        }

        private int createBackup(string path, bool overwrite)
        {
            String fileName = Path.GetFileName(path);
            int res = CopyFile(path, cfg["backup_dir"] + @"\" + fileName, overwrite);
            if (res == 0 || res == 1) {
                baks[fileName] = path;
                saveBackups();
                updateBackupList();
            }
            return res;
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
            loadCfg();
            loadBackups();
            loadModpacks();
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
            bool baksMade = false;
            bool chk = false;
            bool packErr = false;
            pBar.Visible = true;
            pBar.Maximum = modListPanel.Controls.OfType<CheckBox>().Count();
            foreach (CheckBox chb in modListPanel.Controls.OfType<CheckBox>()) {
                pBar.PerformStep();
                if (chb.Checked) {
                    chk = true;
                    string modpackname = chb.Text.Replace(dirtyPadding, "");
                    try {
                        using (ZipArchive archive = ZipFile.OpenRead(cfg["modpack_dir"] + @"\" + modpackname + ".zip")) {
                            ZipArchiveEntry modpackConfigEntry = archive.GetEntry("modpack_config.cfg");
                            if (modpackConfigEntry == null) {
                                MessageBox.Show("Could not open modpack config file. The file '" + modpackname + ".zip' is not a compatible modpack." +
                                    "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                                packErr = true;
                                continue;
                            }
                            List<Dictionary<string, string>> modpackConfig;
                            using (Stream jsonStream = modpackConfigEntry.Open()) {
                                StreamReader reader = new StreamReader(jsonStream);
                                try {
                                    modpackConfig = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(reader.ReadToEnd());
                                } catch (JsonSerializationException) {
                                    MessageBox.Show("The configuration file in '" + modpackname + ".zip' is corrupted." +
                                    "\r\nThis modpack cannot be installed.", "Error");
                                    continue;
                                }
                            }
                            List<string> modpackBakList = new List<string>();   // track patched files in case of failure mid patch
                            foreach (Dictionary<string, string> dict in modpackConfig) {
                                ZipArchiveEntry modFile = archive.GetEntry(dict["src"]);
                                string destination = dict["dest"].Replace("$MCC_home", cfg["MCC_home"]);
                                bool err = false;
                                if (File.Exists(destination)) {
                                    if (createBackup(destination, false) == 0) {
                                        baksMade = true;
                                    }
                                    if (!DeleteFile(destination)) {
                                        err = true;
                                    }
                                }
                                if (!err) {
                                    try {
                                        modFile.ExtractToFile(destination);
                                    } catch (IOException) {
                                        err = true;     // strange edge case which will *probably* never happen
                                    }
                                }
                                if (err) {
                                    MessageBox.Show("File Access Exception. If the game is running, exit it and try again." +
                                            "\r\nCould not install the '" + modpackname + "' modpack.", "Error");
                                    if (restoreBaks(modpackBakList) != 0) {
                                        MessageBox.Show("At least one file restore failed. Your game may be in an unstable state.", "Warning");
                                    }
                                    packErr = true;
                                    break;
                                }
                                modpackBakList.Add(Path.GetFileName(dict["dest"]));
                            }
                        }
                    } catch (FileNotFoundException) {
                        MessageBox.Show("Could not find the '" + modpackname + "' modpack.", "Error");
                        packErr = true;
                    }
                    chb.Checked = false;
                }
            }

            if (!chk) { // fail - no boxes checked
                MessageBox.Show("No modpacks selected.", "Error");
            } else if (packErr) {   // fail / partial success - At least one modpack was not patched
                MessageBox.Show("One or more of the selected modpacks were not patched to the game.", "Warning");
            } else if (baksMade) {  // success and new backup(s) created
                MessageBox.Show("The selected mods have been patched to the game.\r\nNew backups were created.", "Info");
            } else {
                MessageBox.Show("The selected mods have been patched to the game.", "Info");
            }
            pBar.Value = 0;
            pBar.Visible = false;
        }

        private void delModpack_Click(object sender, EventArgs e)
        {
            DialogResult ans = MessageBox.Show(
                "Are you sure you want to delete the selected modpacks(s)?\r\nNo crying afterwards?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (ans == DialogResult.No) {
                return;
            }

            bool chk = false;
            pBar.Visible = true;
            pBar.Maximum = modListPanel.Controls.OfType<CheckBox>().Count();
            foreach (CheckBox chb in modListPanel.Controls.OfType<CheckBox>()) {
                pBar.PerformStep();
                if (chb.Checked) {
                    chk = true;
                    string modpackname = chb.Text.Replace(dirtyPadding, "");
                    if (!DeleteFile(cfg["modpack_dir"] + @"\" + modpackname + ".zip")) {
                        MessageBox.Show("Could not delete '" + modpackname + ".zip'. Is the zip file open somewhere?", "Error");
                    }
                    chb.Checked = false;
                }
            }
            if (!chk) {
                MessageBox.Show("No items selected from the list.", "Error");
            } else {
                MessageBox.Show("Selected modpacks have been deleted.", "Info");
                loadModpacks();
            }
            pBar.Value = 0;
            pBar.Visible = false;
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

        List<Panel> createPageList = new List<Panel>();
        Point delBtnPoint = new Point(0, 3);
        Point sourceTextBoxPoint = new Point(20, 1);
        Point sourceBtnPoint = new Point(203, 0);
        Point arrowPoint = new Point(245, -5);
        Point destTextBoxPoint = new Point(278, 1);
        Point destBtnPoint = new Point(461, 0);
        Font btnFont = new Font("Lucida Console", 10, FontStyle.Regular);
        Font arrowFont = new Font("Reem Kufi", 12, FontStyle.Bold);
        private void addRowButton_Click(object sender, EventArgs e)
        {
            PictureBox del = new PictureBox();
            del.Image = del.ErrorImage;    // bit of a hack to get the error image to appear
            del.Width = 14;
            del.Height = 16;
            del.MouseEnter += btnHoverOn;
            del.MouseLeave += btnHoverOff;
            del.Click += deleteRow;
            del.Location = delBtnPoint;

            TextBox txt1 = new TextBox();
            txt1.Width = 180;
            //txt1.Enabled = false;
            txt1.Location = sourceTextBoxPoint;

            Button btn1 = new Button();
            btn1.BackColor = SystemColors.ButtonFace;
            btn1.Width = 39;
            btn1.Font = btnFont;
            btn1.Text = "...";
            btn1.Click += create_fileBrowse1;
            btn1.Location = sourceBtnPoint;

            Label lbl = new Label();
            lbl.Width = 33;
            lbl.Font = arrowFont;
            lbl.Text = ">>";
            lbl.Location = arrowPoint;

            TextBox txt2 = new TextBox();
            txt2.Width = 180;
            //txt2.Enabled = false;
            txt2.Location = destTextBoxPoint;

            Button btn2 = new Button();
            btn2.BackColor = SystemColors.ButtonFace;
            btn2.Width = 39;
            btn2.Font = btnFont;
            btn2.Text = "...";
            btn2.Click += create_fileBrowse2;
            btn2.Location = destBtnPoint;

            Panel p = new Panel();
            //p.BackColor = Color.Aqua;
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

        private void create_fileBrowse1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";  // using the GUID for 'This PC' folder
            if (ofd.ShowDialog() == DialogResult.OK) {
                ((Button)sender).Parent.GetChildAtPoint(sourceTextBoxPoint).Text = ofd.FileName;
            }
        }

        private void create_fileBrowse2(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = false;    // allow modpack creators to type in a filename for creating new files
            ofd.InitialDirectory = cfg["MCC_home"];
            if (ofd.ShowDialog() == DialogResult.OK) {
                ((Button)sender).Parent.GetChildAtPoint(destTextBoxPoint).Text = ofd.FileName;
            }
        }
        private void createModpackBtn_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(modpackName_txt.Text)) {
                MessageBox.Show("Please enter a modpack name", "Error");
                return;
            }

            List<String> chk = new List<String>();
            List<Dictionary<string, string>> fileMap = new List<Dictionary<string, string>>();
            foreach (Panel row in createFilesPanel.Controls.OfType<Panel>()) {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["src"] = row.GetChildAtPoint(sourceTextBoxPoint).Text;
                dict["dest"] = row.GetChildAtPoint(destTextBoxPoint).Text;
                if (String.IsNullOrEmpty(dict["src"]) || String.IsNullOrEmpty(dict["dest"])) {
                    MessageBox.Show("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(dict["src"])) {
                    MessageBox.Show("The source file '" + dict["src"] + "' does not exist.", "Error");
                    return;
                }
                if (!dict["dest"].StartsWith(cfg["MCC_home"])) {
                    MessageBox.Show("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }

                // make modpack compatable with any MCC_home directory
                dict["dest"] = dict["dest"].Replace(cfg["MCC_home"], "$MCC_home");

                fileMap.Add(dict);
                chk.Add(row.GetChildAtPoint(destTextBoxPoint).Text);
            }

            if (chk.Distinct().Count() != chk.Count()) {
                MessageBox.Show("You have multiple files trying to write to the same destination.", "Error");
                return;
            }

            ensureModpackFolderExists();
            String modpackName = modpackName_txt.Text + ".zip";
            String zipPath = cfg["modpack_dir"] + @"\" + modpackName;
            if (File.Exists(zipPath)) {
                MessageBox.Show("A modpack with that name already exists.", "Error");
                return;
            }

            pBar.Visible = true;
            pBar.Maximum = fileMap.Count();
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                foreach (var entry in fileMap) {
                    pBar.PerformStep();
                    String fileName = Path.GetFileName(entry["src"]);
                    archive.CreateEntryFromFile(entry["src"], fileName);    // TODO: Fix issues when two source files have same name but diff path
                    // change src path to just modpack after archive creation but before json serialization
                    entry["src"] = fileName;
                }
                ZipArchiveEntry configFile = archive.CreateEntry("modpack_config.cfg");
                string json = JsonConvert.SerializeObject(fileMap, Formatting.Indented);
                using (StreamWriter writer = new StreamWriter(configFile.Open())) {
                    writer.WriteLine(json);
                }
                ZipArchiveEntry readmeFile = archive.CreateEntry("README.txt");
                using (StreamWriter writer = new StreamWriter(readmeFile.Open())) {
                    writer.WriteLine("Install using MCC Mod Manager: https://github.com/executionByFork/MCC_Mod_Manager/tree/master");
                }
            }

            MessageBox.Show("Modpack '" + modpackName + "' created.", "Info");
            pBar.Value = 0;
            pBar.Visible = false;
            createFilesPanel.Controls.Clear();
            createPageList = new List<Panel>(); // garbage collector magic
            modpackName_txt.Text = "";
            loadModpacks();
            return;
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            createFilesPanel.Controls.Clear();
            createPageList = new List<Panel>(); // garbage collector magic
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
                InitialDirectory = cfg["MCC_home"],
                Title = "Select a folder"
            };
            if (dialog.Show(Handle)) {
                ((Button)sender).Parent.GetChildAtPoint(new Point(5, 3)).Text = dialog.FileName;
            }
        }

        private bool correctHomeDir(String dir)
        {
            if (!File.Exists(dir + @"\haloreach\haloreach.dll")) {
                return false;
            }
            if (!File.Exists(dir + @"\MCC\Content\Paks\MCC-WindowsNoEditor.pak")) {
                return false;
            }
            if (!File.Exists(dir + @"\mcclauncher.exe")) {
                return false;
            }

            return true;
        }

        private void cfgUpdateBtn_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(cfgTextBox1.Text)) {
                if (!correctHomeDir(cfgTextBox1.Text)) {
                    MessageBox.Show("It seems you have selected the wrong MCC install directory. " +
                        "Please make sure to select the folder named 'Halo The Master Chief Collection' in your Steam files.", "Error");
                    cfgTextBox1.Text = cfg["MCC_home"];
                    return;
                }
                cfg["MCC_home"] = cfgTextBox1.Text;
            }
            if (!String.IsNullOrEmpty(cfgTextBox2.Text)) {
                cfg["backup_dir"] = cfgTextBox2.Text;
            }
            if (!String.IsNullOrEmpty(cfgTextBox3.Text)) {
                cfg["modpack_dir"] = cfgTextBox3.Text;
            }
            if (delOldBaks_chb.Checked) {
                cfg["deleteOldBaks"] = "true";
            } else {
                cfg["deleteOldBaks"] = "false";
            }

            saveCfg();

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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = cfg["MCC_home"];
            if (ofd.ShowDialog() == DialogResult.OK) {
                if (File.Exists(cfg["backup_dir"] + @"\" + Path.GetFileName(ofd.FileName))) {
                    DialogResult ans = MessageBox.Show(
                        "A backup of that file already exists. Would you like to overwrite?",
                        "Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (ans == DialogResult.No) {
                        return;
                    }
                }

                if (createBackup(ofd.FileName, true) != 0) {
                    MessageBox.Show("Could not create a backup of the chosen file. Is the file open somewhere?", "Error");
                } else {
                    MessageBox.Show("New Backup Created");
                    saveBackups();
                    loadBackups();
                }
            }
        }

        private int restoreBaks(List<string> backupNames)
        {
            if (backupNames.Count() == 0) {
                return 0;
            }

            pBar.Visible = true;
            pBar.Maximum = backupNames.Count();
            bool chk = false;
            bool err = false;
            foreach (string fileName in backupNames) {
                pBar.PerformStep();
                if (CopyFile(cfg["backup_dir"] + @"\" + fileName, baks[fileName], true) == 0) {
                    if (cfg["deleteOldBaks"] == "true") {
                        if (DeleteFile(cfg["backup_dir"] + @"\" + fileName)) {
                            baks.Remove(fileName);
                        } else {
                            MessageBox.Show("Could not remove old backup '" + fileName + "'. Is the file open somewhere?", "Error");
                        }
                    }
                    chk = true;
                } else {
                    MessageBox.Show("Could not restore '" + fileName + "'. If the game is open, close it and try again.", "Error");
                    err = true;
                }
            }
            saveBackups();
            updateBackupList();
            pBar.Value = 0;
            pBar.Visible = false;
            if (chk) {
                if (err) {
                    return 1;   // Partial success - Some files were restored
                }
                return 0;   // Success - All files were restored
            }
            return 2;   // Failure - No files were restored
        }

        private void restoreSelectedBtn_Click(object sender, EventArgs e)
        {
            
            List<string> backupNames = new List<string>();
            foreach (CheckBox chb in bakListPanel.Controls.OfType<CheckBox>()) {
                if (chb.Checked) {
                    backupNames.Add(chb.Text.Replace(dirtyPadding, ""));
                    chb.Checked = false;
                }
            }

            if (backupNames.Count() == 0) {
                MessageBox.Show("No items selected from the list.", "Error");
                return;
            }
            int r = restoreBaks(backupNames);
            if (r == 0) {
                MessageBox.Show("Selected files have been restored.", "Info");
            } else if (r == 1) {
                MessageBox.Show("At least one file restore failed. Your game may be in an unstable state.", "Warning");
            }
        }

        private void restoreAllBaksBtn_Click(object sender, EventArgs e)
        {
            pBar.Visible = true;
            pBar.Maximum = baks.Count();
            List<string> remainingBaks = new List<string>();
            List<string> toRemove = new List<string>();
            bool chk = false;
            foreach (KeyValuePair<string, string> entry in baks) {
                pBar.PerformStep();
                if (CopyFile(cfg["backup_dir"] + @"\" + entry.Key, entry.Value, true) == 0) {
                    if (cfg["deleteOldBaks"] == "true") {
                        if (!DeleteFile(cfg["backup_dir"] + @"\" + entry.Key)) {
                            remainingBaks.Add(entry.Key);
                            MessageBox.Show("Could not remove old backup '" + entry.Key + "'. Is the file open somewhere?", "Error");
                        }
                    }
                    chk = true;
                } else {
                    remainingBaks.Add(entry.Key);
                    MessageBox.Show("Could not restore '" + entry.Key + "'. If the game is open, close it and try again.", "Error");
                }
            }

            if (cfg["delOldBaks"] == "true") {
                if (remainingBaks.Count() == 0) {
                    baks = new Dictionary<string, string>();
                } else {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    foreach (string file in remainingBaks) {    // create backup config of files which couldn't be restored and removed
                        tmp[file] = baks[file];
                    }
                    baks = tmp;
                }
            }

            if (chk) {
                MessageBox.Show("Files have been restored.", "Info");
            }
            saveBackups();
            updateBackupList();
            pBar.Value = 0;
            pBar.Visible = false;
        }

        private void delSelectedBak_Click(object sender, EventArgs e)
        {
            DialogResult ans = MessageBox.Show(
                "Are you sure you want to delete the selected backup(s)?\r\nNo crying afterwards?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (ans == DialogResult.No) {
                return;
            }

            bool chk = false;
            pBar.Visible = true;
            pBar.Maximum = bakListPanel.Controls.OfType<CheckBox>().Count();
            foreach (CheckBox chb in bakListPanel.Controls.OfType<CheckBox>()) {
                pBar.PerformStep();
                if (chb.Checked) {
                    chk = true;
                    string fileName = chb.Text.Replace(dirtyPadding, "");
                    if (DeleteFile(cfg["backup_dir"] + @"\" + fileName)) {
                        baks.Remove(fileName);
                    } else {
                        MessageBox.Show("Could not delete '" + fileName + "'. Is the file open somewhere?", "Error");
                    }
                    chb.Checked = false;
                }
            }
            if (!chk) {
                MessageBox.Show("No items selected from the list.", "Error");
            } else {
                saveBackups();
                MessageBox.Show("Selected files have been deleted.", "Info");
                updateBackupList();
            }
            pBar.Value = 0;
            pBar.Visible = false;
        }

        private void delAllBaksBtn_Click(object sender, EventArgs e)
        {
            DialogResult ans = MessageBox.Show(
                "Are you sure you want to delete ALL of your backup(s)?\r\nNo crying afterwards?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (ans == DialogResult.No) {
                return;
            }

            pBar.Visible = true;
            pBar.Maximum = baks.Count();
            List<string> remainingBaks = new List<string>();
            foreach (KeyValuePair<string, string> entry in baks) {
                pBar.PerformStep();
                if (!DeleteFile(cfg["backup_dir"] + @"\" + entry.Key)) {
                    remainingBaks.Add(entry.Key);
                    MessageBox.Show("Could not delete '" + entry.Key + "'. Is the file open somewhere?", "Error");
                }
            }
            if (remainingBaks.Count() == 0) {
                baks = new Dictionary<string, string>();
                MessageBox.Show("All backups deleted.", "Info");
            } else {
                Dictionary<string, string> tmp = new Dictionary<string, string>();
                foreach (string file in remainingBaks) {    // create backup config of files which couldn't be deleted
                    tmp[file] = baks[file];
                }
                baks = tmp;
            }
            saveBackups();
            updateBackupList();
            pBar.Value = 0;
            pBar.Visible = false;
        }
    }
}
