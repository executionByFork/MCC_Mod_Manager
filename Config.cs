using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace MCC_Mod_Manager
{
    public class mainCfg
    {
        public string version = Config.version;
        public string MCC_home;
        public string backup_dir;
        public string modpack_dir;
        public bool deleteOldBaks;
        public List<string> patched;
    }

    static class Config
    {
        public static readonly string version = "v0.5";
        private static readonly string _cfgLocation = @".\MCC_Mod_Manager.cfg";
        private static readonly string _bakcfgName = @"\backups.cfg";

        // UI elements
        public static string dirtyPadding = "        ";
        public static readonly Point delBtnPoint = new Point(0, 3);
        public static readonly Point sourceTextBoxPoint = new Point(20, 1);
        public static readonly Point sourceBtnPoint = new Point(203, 0);
        public static readonly Point arrowPoint = new Point(245, -5);
        public static readonly Point destTextBoxPoint = new Point(278, 1);
        public static readonly Point destBtnPoint = new Point(461, 0);
        public static readonly Font btnFont = new Font("Lucida Console", 10, FontStyle.Regular);
        public static readonly Font arrowFont = new Font("Reem Kufi", 12, FontStyle.Bold);

        public static Form1 form1;  // this is set on form load
        private static mainCfg _cfg = new mainCfg(); // this is set on form load
        public static bool fullBakPath = false;
        
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
        public static List<string> patched {
            get {
                return _cfg.patched;
            }
            set {
                _cfg.patched = value;
            }
        }

        public static bool isPatched(string modpackName)
        {
            try {
                return patched.Contains(modpackName);
            } catch (NullReferenceException) {
                return false;
            }
        }
        public static void addPatched(string modpackName)
        {
            patched.Add(modpackName);
        }
        public static void rmPatched(string modpackName)
        {
            patched.Remove(modpackName);
        }

        public static void doResetApp()
        {
            patched = new List<string>();
            saveCfg();
            Modpacks.loadModpacks();
            if (!Backups.deleteAll(true)) {
                form1.showMsg("There was an issue deleting at least one backup. Please delete this manually in the Backups tab to avoid restoring an old " +
                    "version of the file in the future.", "Error");
            }
            Backups.loadBackups();
        }

        public static bool createDefaultCfg()
        {
            _cfg = new mainCfg();
            // default values declared here so that mainCfg class does not implicitly set defaults and bypass warning triggers
            MCC_home = @"C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";
            backup_dir = @".\backups";
            modpack_dir = @".\modpacks";
            deleteOldBaks = false;
            patched = new List<string>();

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

        private static int checkCfg()
        {
            string json = File.ReadAllText(_cfgLocation);
            try {
                mainCfg values = JsonConvert.DeserializeObject<mainCfg>(json);
                // Future config version handling: float.Parse(values.version.Substring(1)) < 0.5
                if (String.IsNullOrEmpty(values.version)) {
                    return 2;
                }

                MCC_home = values.MCC_home;
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

            return 0;
        }

        public static bool loadCfg()
        {
            if (!File.Exists(_cfgLocation)) {
                createDefaultCfg();
            } else {
                int r = checkCfg();
                if (r == 1) {
                    DialogResult ans = form1.showMsg("Your configuration has format errors, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No) {
                        return false;
                    }
                    createDefaultCfg();
                } else if (r == 2) {
                    DialogResult ans = form1.showMsg("Your config file is using an old format, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No) {
                        return false;
                    }
                    createDefaultCfg();
                }
            }

            if (patched == null) {
                DialogResult ans = form1.showMsg("Your config file is using an old format, would you like to overwrite it with a default config?", "Question");
                if (ans == DialogResult.No) {
                    return false;
                }
                createDefaultCfg();
            }
            bool msg = false;
            List<string> tmp = new List<string>();
            foreach (string modpack in patched) {
                if (!Modpacks.verifyExists(modpack)) {
                    if (!msg) {
                        msg = true;
                        form1.showMsg("One or more of your enabled modpacks is missing from the modpacks folder. This likely means your game " +
                            "is in an unstable state. You should restore from backups or verify the game files through Steam." +
                            "\r\nThis warning will only show once.", "Warning");
                    }
                    tmp.Add(modpack);
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

            return true;
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
