using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.IO.Compression;

namespace MCC_Mod_Manager
{
    public class mainCfg
    {
        public string version = Config.version;
        public string MCC_version;
        public string MCC_home;
        public string backup_dir;
        public string modpack_dir;
        public bool deleteOldBaks;
        public Dictionary<string, Dictionary<string,string>> patched = new Dictionary<string, Dictionary<string, string>>();
    }

    static class Config
    {
        public static readonly string version = "v0.7";
        private static readonly string _cfgLocation = @".\MCC_Mod_Manager.cfg";
        private static readonly string _bakcfgName = @"\backups.cfg";

        // UI elements
        public static string dirtyPadding = "        ";
        public static readonly Point delBtnPoint = new Point(0, 15);
        public static readonly Point sourceTextBoxPoint = new Point(20, 1);
        public static readonly Point sourceBtnPoint = new Point(203, 0);
        public static readonly Point origTextBoxPoint = new Point(20, 26);
        public static readonly Point origBtnPoint = new Point(203, 25);
        public static readonly Point arrowPoint = new Point(245, 7);
        public static readonly Point destTextBoxPoint = new Point(278, 14);
        public static readonly Point destBtnPoint = new Point(461, 13);
        public static readonly Font btnFont = new Font("Lucida Console", 10, FontStyle.Regular);
        public static readonly Font arrowFont = new Font("Reem Kufi", 12, FontStyle.Bold);

        public static Form1 form1;  // this is set on form load
        private static mainCfg _cfg = new mainCfg(); // this is set on form load
        public static bool fullBakPath = false;

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
        public static string backup_dir {
            get {
                return _cfg.backup_dir;
            }
            set {
                _cfg.backup_dir = value;
            }
        }
        public static string backupCfg {
            get {
                return _cfg.backup_dir + _bakcfgName;
            }
        }
        public static string modpack_dir {
            get {
                return _cfg.modpack_dir;
            }
            set {
                _cfg.modpack_dir = value;
            }
        }
        public static bool deleteOldBaks {
            get {
                return _cfg.deleteOldBaks;
            }
            set {
                _cfg.deleteOldBaks = value;
            }
        }
        public static Dictionary<string, Dictionary<string, string>> patched {
            get {
                return _cfg.patched;
            }
            set {
                _cfg.patched = value;
            }
        }

        public static List<string> getEnabledModpacks()
        {
            List<string> list = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, string>> modpack in patched) {
                list.Add(modpack.Key);
            }
            return list;
        }

        public static bool isPatched(string modpackName)
        {
            try {
                return patched.ContainsKey(modpackName);
            } catch (NullReferenceException) {
                return false;
            }
        }
        public static bool addPatched(string modpackName)
        {
            Dictionary<string, string> modfiles = new Dictionary<string, string>();
            modpackCfg mCfg = Modpacks.getModpackConfig(modpackName);
            using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackName + ".zip")) {
                if (mCfg == null) {
                    form1.showMsg("Cannot set state to enabled. The file '" + modpackName + ".zip' is either not a compatible modpack or the config is corrupted.", "Error");
                    return false;
                }

                List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                foreach (modpackEntry entry in mCfg.entries) {
                    modfiles[entry.dest] = Modpacks.getMD5(Modpacks.expandPath(entry.dest));
                }
            }

            patched[modpackName] = modfiles;
            return true;
        }
        public static void rmPatched(string modpackName)
        {
            patched.Remove(modpackName);
        }

        public static string getCurrentBuild()
        {
            return IO.readFirstLine(MCC_home + @"\build_tag.txt");
        }

        public static void doResetApp()
        {
            patched = new Dictionary<string, Dictionary<string, string>>();
            MCC_version = getCurrentBuild();
            saveCfg();
            Modpacks.loadModpacks();
            if (!Backups.deleteAll(true)) {
                form1.showMsg("There was an issue deleting at least one backup. Please delete these in the Backups tab to avoid restoring an old " +
                    "version of the file in the future.", "Error");
            }
            Backups.loadBackups();
        }

        public static bool createDefaultCfg()
        {
            _cfg = new mainCfg();
            // default values declared here so that mainCfg class does not implicitly set defaults and bypass warning triggers
            MCC_home = @"C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";
            MCC_version = getCurrentBuild();   // sets MCC_version to null if not found
            backup_dir = @".\backups";
            modpack_dir = @".\modpacks";
            deleteOldBaks = false;
            patched = new Dictionary<string, Dictionary<string, string>>();

            saveCfg();
            form1.showMsg("A default configuration file has been created. Please review and update it as needed.", "Info");

            return true;
        }

        public static void saveCfg()
        {
            string json = JsonConvert.SerializeObject(_cfg, Formatting.Indented);
            using (FileStream fs = File.Create(_cfgLocation)) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        private static int readCfg()
        {
            string json = File.ReadAllText(_cfgLocation);
            try {
                mainCfg values = JsonConvert.DeserializeObject<mainCfg>(json);
                // TODO: implement old config version handling
                if (String.IsNullOrEmpty(values.version)) {
                    return 2;
                }

                MCC_home = values.MCC_home;
                MCC_version = String.IsNullOrEmpty(values.MCC_version) ? getCurrentBuild() : values.MCC_version;
                backup_dir = values.backup_dir;
                modpack_dir = values.modpack_dir;
                deleteOldBaks = values.deleteOldBaks;
                patched = values.patched;
            } catch (JsonSerializationException) {
                return 1;
            } catch (JsonReaderException) {
                return 1;
            } catch (KeyNotFoundException) {
                return 1;
            }
            if (patched == null) {
                return 2;
            }

            return 0;
        }

        public static int loadCfg()
        {
            bool needsStabilize = false;
            if (!File.Exists(_cfgLocation)) {
                createDefaultCfg();
            } else {
                int r = readCfg();
                if (r == 1) {
                    DialogResult ans = form1.showMsg("Your configuration has formatting errors, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No) {
                        return 2;
                    }
                    createDefaultCfg();
                } else if (r == 2) {
                    DialogResult ans = form1.showMsg("Your config file is using an old format, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No) {
                        return 2;
                    }
                    createDefaultCfg();
                } else {
                    // check if game was updated
                    if (MCC_version != getCurrentBuild()) {
                        DialogResult ans = form1.showMsg("It appears that MCC has been updated. MCC Mod Manager needs to stabilize the game by uninstalling certain modpacks." +
                            "\r\nWould you like to do this now? Selecting 'No' will disable features.", "Question");
                        if (ans == DialogResult.No) {
                            needsStabilize = true;
                        } else {
                            Modpacks.stabilizeGame();
                        }
                    }
                }
            }

            bool msg = false;
            List<string> tmp = new List<string>();
            foreach (KeyValuePair<string, Dictionary<string,string>> modpack in patched) {
                if (!Modpacks.verifyExists(modpack.Key)) {
                    if (!msg) {
                        msg = true;
                        form1.showMsg("The '" + modpack.Key + "' modpack is missing from the modpacks folder. If this modpack is actually installed, " +
                            "MCC Mod Manager won't be able to uninstall it. You should restore from backups or verify the game files through Steam." +
                            "\r\nThis warning will only show once.", "Warning");
                    }
                    tmp.Add(modpack.Key);
                }
            }
            foreach (string modpack in tmp) {
                rmPatched(modpack);
            }
            saveCfg();

            // Update config tab
            form1.cfgTextBox1Text = MCC_home;
            form1.cfgTextBox2Text = backup_dir;
            form1.cfgTextBox3Text = modpack_dir;
            form1.delOldBaks = deleteOldBaks;

            if (needsStabilize) {
                return 1;
            } else {
                return 0;
            }
        }

        public static bool chkHomeDir(String dir)
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
    }
}
