using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using MCC_Mod_Manager.Api.Utilities;

namespace MCC_Mod_Manager.Api {
    public class MainCfg {
        public string version = Config.version;
        public string MCC_version;
        public string MCC_home;
        public string backup_dir;
        public string modpack_dir;
        public bool deleteOldBaks;
        public Dictionary<string, patchedEntry> patched = new Dictionary<string, patchedEntry>();
    }

    public class patchedEntry {
        public bool error;
        public Dictionary<string, string> files = new Dictionary<string, string>();
    }

    static class Config {
        #region Config Fields
        public const string version = "v0.8";
        private const string _cfgLocation = @".\MCC_Mod_Manager.cfg";
        private const string _bakcfgName = @"\backups.cfg";
        public const string _defaultReadmeText = @"Install using MCC Mod Manager: https://github.com/executionByFork/MCC_Mod_Manager/blob/master/README.md";

        // UI elements
        public static string dirtyPadding = "        ";
        public static readonly Point delBtnPoint = new Point(0, 3);
        public static readonly Point delBtnPointAlt = new Point(0, 15);
        public static readonly Point sourceTextBoxPoint = new Point(20, 1);
        public static readonly Point sourceBtnPoint = new Point(203, 0);
        public static readonly Point origTextBoxPoint = new Point(20, 26);
        public static readonly Point origBtnPoint = new Point(203, 25);
        public static readonly Point arrowPoint = new Point(245, -5);
        public static readonly Point arrowPointAlt = new Point(245, 7);
        public static readonly Point destTextBoxPoint = new Point(278, 1);
        public static readonly Point destTextBoxPointAlt = new Point(278, 14);
        public static readonly Point destBtnPoint = new Point(461, 0);
        public static readonly Point destBtnPointAlt = new Point(461, 13);
        public static readonly Font btnFont = new Font("Lucida Console", 10, FontStyle.Regular);
        public static readonly Font arrowFont = new Font("Reem Kufi", 12, FontStyle.Bold);

        public static readonly Point MyModsEnabledPoint = new Point(15, 1);
        public static readonly Point MyModsCautionPoint = new Point(37, 1);
        public static readonly Point MyModsChbPoint = new Point(60, 1);

        private static MainCfg _cfg = new MainCfg(); // this is set on form load
        public static bool fullBakPath = false;

        #endregion

        #region Primary Config Mutators
        public static string MCC_version {
            get {
                return _cfg.MCC_version;
            }
            set {
                _cfg.MCC_version = value;
            }
        }
        public static string MCC_home {
            get {
                return _cfg.MCC_home;
            }
            set {
                _cfg.MCC_home = value;
            }
        }
        public static string Backup_dir {
            get {
                return _cfg.backup_dir;
            }
            set {
                _cfg.backup_dir = value;
            }
        }
        public static string BackupCfg {
            get {
                return _cfg.backup_dir + _bakcfgName;
            }
        }
        public static string Modpack_dir {
            get {
                return _cfg.modpack_dir;
            }
            set {
                _cfg.modpack_dir = value;
            }
        }
        public static bool DeleteOldBaks {
            get {
                return _cfg.deleteOldBaks;
            }
            set {
                _cfg.deleteOldBaks = value;
            }
        }

        public static Dictionary<string, patchedEntry> Patched {
            get {
                return _cfg.patched;
            }
            set {
                _cfg.patched = value;
            }
        }

        #endregion

        #region Event Handlers
        public static void BrowseFolderBtn_Click(object sender, EventArgs e) {
            var dialog = new FolderSelectDialog {
                InitialDirectory = Config.MCC_home,
                Title = "Select a folder"
            };
            if (dialog.Show(Program.MasterForm.Handle)) {
                ((Button)sender).Parent.GetChildAtPoint(new Point(5, 3)).Text = dialog.FileName;
            }
        }

        public static void UpdateBtn_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(Program.MasterForm.cfgTextBox1.Text) || string.IsNullOrEmpty(Program.MasterForm.cfgTextBox2.Text) || string.IsNullOrEmpty(Program.MasterForm.cfgTextBox3.Text)) {
                Utility.ShowMsg("Config entries must not be empty.", "Error");
                return;
            }

            if (!Config.ChkHomeDir(Program.MasterForm.cfgTextBox1.Text)) {
                Utility.ShowMsg("It seems you have selected the wrong MCC install directory. " +
                    "Please make sure to select the folder named 'Halo The Master Chief Collection' in your Steam files.", "Error");
                Program.MasterForm.cfgTextBox1.Text = Config.MCC_home;
                return;
            }
            Config.MCC_home = Program.MasterForm.cfgTextBox1.Text;
            Config.Backup_dir = Program.MasterForm.cfgTextBox2.Text;
            Config.Modpack_dir = Program.MasterForm.cfgTextBox3.Text;
            Config.DeleteOldBaks = Program.MasterForm.delOldBaks_chb.Checked;

            Config.SaveCfg();

            Utility.ShowMsg("Config Updated!", "Info");
        }

        public static void ResetApp_Click(object sender, EventArgs e) {
            DialogResult ans = Utility.ShowMsg("WARNING: This dangerous, and odds are you don't need to do it." +
                "\r\n\r\nThis button will reset the application state, so that the mod manager believes your Halo install is COMPLETELY unmodded. It will " +
                "delete ALL of your backups, and WILL NOT restore them beforehand. This is to reset the app to a default state and flush out any broken files." +
                "\r\n\r\nAre you sure you want to continue?", "Question");
            if (ans == DialogResult.No) {
                return;
            }

            Config.DoResetApp();
        }
        #endregion

        #region Api Functions

        public static void SaveCfg() {
            string json = JsonConvert.SerializeObject(_cfg, Formatting.Indented);
            using (FileStream fs = File.Create(_cfgLocation)) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        public static int LoadCfg() {
            bool stabilize = false;
            bool needsStabilize = false;
            if (!File.Exists(_cfgLocation)) {
                CreateDefaultCfg();
            } else {
                int r = ReadCfg();
                if (r == 1) {
                    DialogResult ans = Utility.ShowMsg("Your configuration has formatting errors, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No) {
                        return 3;
                    }
                    CreateDefaultCfg();
                } else if (r == 2) {
                    DialogResult ans = Utility.ShowMsg("Your config file is using an old format, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No) {
                        return 3;
                    }
                    CreateDefaultCfg();
                } else {
                    // check if game was updated
                    if (MCC_version != GetCurrentBuild()) {
                        DialogResult ans = Utility.ShowMsg("It appears that MCC has been updated. MCC Mod Manager needs to stabilize the game by uninstalling certain modpacks." +
                            "\r\nWould you like to do this now? Selecting 'No' will disable features.", "Question");
                        if (ans == DialogResult.Yes) {
                            stabilize = true;
                        } else {
                            needsStabilize = true;
                        }
                    }
                }
            }

            bool msg = false;
            List<string> tmp = new List<string>();
            foreach (KeyValuePair<string, patchedEntry> modpack in Patched) {
                if (!Modpacks.VerifyExists(modpack.Key)) {
                    if (!msg) {
                        msg = true;
                        Utility.ShowMsg("The '" + modpack.Key + "' modpack is missing from the modpacks folder. If this modpack is actually installed, " +
                            "MCC Mod Manager won't be able to uninstall it. You should restore from backups or verify the game files through Steam." +
                            "\r\nThis warning will only show once.", "Warning");
                    }
                    tmp.Add(modpack.Key);
                }
            }
            foreach (string modpack in tmp) {
                RmPatched(modpack);
            }
            SaveCfg();

            // Update config tab
            Program.MasterForm.cfgTextBox1.Text = MCC_home;
            Program.MasterForm.cfgTextBox2.Text = Backup_dir;
            Program.MasterForm.cfgTextBox3.Text = Modpack_dir;
            Program.MasterForm.delOldBaks_chb.Checked = DeleteOldBaks;

            if (stabilize) {
                return 1;
            } else if (needsStabilize) {
                return 2;
            } else {
                return 0;
            }
        }

        public static bool IsPatched(string modpackName) {
            try {
                return Patched.ContainsKey(modpackName);
            } catch (NullReferenceException) {
                return false;
            }
        }

        public static bool AddPatched(string modpackName) {
            Dictionary<string, string> modfiles = new Dictionary<string, string>();
            ModpackCfg mCfg = Modpacks.GetModpackConfig(modpackName);
            using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackName + ".zip")) {
                if (mCfg == null) {
                    Utility.ShowMsg("Cannot set state to enabled. The file '" + modpackName + ".zip' is either not a compatible modpack or the config is corrupted.", "Error");
                    return false;
                }

                List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                foreach (ModpackEntry entry in mCfg.entries) {
                    modfiles[entry.dest] = Modpacks.GetMD5(Utility.ExpandPath(entry.dest));
                }
            }

            Patched[modpackName] = new patchedEntry();
            Patched[modpackName].files = modfiles;
            return true;
        }

        public static void RmPatched(string modpackName) {
            Patched.Remove(modpackName);
        }

        public static void DoResetApp() {
            Patched = new Dictionary<string, patchedEntry>();
            MCC_version = GetCurrentBuild();
            SaveCfg();
            MyMods.LoadModpacks();
            if (!Backups.DeleteAll(true)) {
                Utility.ShowMsg("There was an issue deleting at least one backup. Please delete these in the Backups tab to avoid restoring an old " +
                    "version of the file in the future.", "Error");
            }
            Backups.LoadBackups();
        }

        #endregion

        #region Helper Functions

        public static List<string> GetEnabledModpacks() {
            List<string> list = new List<string>();

            foreach (KeyValuePair<string, patchedEntry> modpack in Patched) {
                list.Add(modpack.Key);
            }
            return list;
        }

        public static string GetCurrentBuild() {
            return Utility.ReadFirstLine(MCC_home + @"\build_tag.txt");
        }

        private static int ReadCfg() {
            string json = File.ReadAllText(_cfgLocation);
            JObject jsonObject = null;
            try {
                jsonObject = JObject.Parse(json);
            } catch (JsonSerializationException) {
                return 1;
            } catch (JsonReaderException) {
                return 1;
            }
            //MainCfg values = JsonConvert.DeserializeObject<MainCfg>(json);
            if (jsonObject.SelectToken("version") == null) { // error on pre v0.5 config
                return 2;
            }

            MCC_home = jsonObject.SelectToken("MCC_home").ToString();
            if (jsonObject.SelectToken("MCC_version") == null) {    // check for pre v0.7 config
                MCC_version = GetCurrentBuild();
            } else {
                MCC_version = jsonObject.SelectToken("MCC_version").ToString();
            }

            Backup_dir = jsonObject.SelectToken("backup_dir").ToString();
            Modpack_dir = jsonObject.SelectToken("modpack_dir").ToString();
            DeleteOldBaks = (bool)jsonObject.SelectToken("deleteOldBaks");

            JObject tmp = (JObject)jsonObject.SelectToken("patched");
            if (tmp == null || !tmp.HasValues) {  // convert config patch object from v0.7 to v0.8
                Patched = new Dictionary<string, patchedEntry>();
            } else {
                try {
                    if (tmp.Properties().ElementAt(0).Value.SelectToken("error") == null) {
                        Patched = new Dictionary<string, patchedEntry>();
                        var entries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,string>>>(jsonObject.SelectToken("patched").ToString());
                        foreach (var entry in entries) {
                            Patched[entry.Key] = new patchedEntry();
                            Patched[entry.Key].error = false;   // assume no partial install modpack errors
                            Patched[entry.Key].files = entry.Value;
                        }
                    } else {
                        Patched = JsonConvert.DeserializeObject<Dictionary<string, patchedEntry>>(jsonObject.SelectToken("patched").ToString());
                    }
                } catch (JsonSerializationException) {
                    return 1;
                } catch (JsonReaderException) {
                    return 1;
                }
            }

            return 0;
        }

        public static bool CreateDefaultCfg() {
            _cfg = new MainCfg();
            // default values declared here so that mainCfg class does not implicitly set defaults and bypass warning triggers
            MCC_home = @"C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";
            MCC_version = GetCurrentBuild();   // sets MCC_version to null if not found
            Backup_dir = @".\backups";
            Modpack_dir = @".\modpacks";
            DeleteOldBaks = false;
            Patched = new Dictionary<string, patchedEntry>();

            SaveCfg();
            Utility.ShowMsg("A default configuration file has been created. Please review and update it as needed.", "Info");

            return true;
        }

        public static bool ChkHomeDir(String dir) {
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

        #endregion
    }
}
