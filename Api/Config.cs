using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using MCC_Mod_Manager.Api;

namespace MCC_Mod_Manager
{
    public class MainCfg
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
        #region Misc Config Fields
        public static readonly string version = "v0.6";
        private static readonly string _cfgLocation = @".\MCC_Mod_Manager.cfg";
        private static readonly string _bakcfgName = @"\backups.cfg";

        public static string dirtyPadding = "        ";
        public static readonly Point delBtnPoint = new Point(0, 3);
        public static readonly Point sourceTextBoxPoint = new Point(20, 1);
        public static readonly Point sourceBtnPoint = new Point(203, 0);
        public static readonly Point arrowPoint = new Point(245, -5);
        public static readonly Point destTextBoxPoint = new Point(278, 1);
        public static readonly Point destBtnPoint = new Point(461, 0);
        public static readonly Font btnFont = new Font("Lucida Console", 10, FontStyle.Regular);
        public static readonly Font arrowFont = new Font("Reem Kufi", 12, FontStyle.Bold);

        private static MainCfg _cfg = new MainCfg(); // this is set on form load
        public static bool fullBakPath = false;
        #endregion

        #region Primary Config Mutators
        public static string MCC_home
        {
            get
            {
                return _cfg.MCC_home;
            }
            set
            {
                _cfg.MCC_home = value;
            }
        }
        public static string backup_dir
        {
            get
            {
                return _cfg.backup_dir;
            }
            set
            {
                _cfg.backup_dir = value;
            }
        }
        public static string backupCfg
        {
            get
            {
                return _cfg.backup_dir + _bakcfgName;
            }
        }
        public static string modpack_dir
        {
            get
            {
                return _cfg.modpack_dir;
            }
            set
            {
                _cfg.modpack_dir = value;
            }
        }
        public static bool deleteOldBaks
        {
            get
            {
                return _cfg.deleteOldBaks;
            }
            set
            {
                _cfg.deleteOldBaks = value;
            }
        }
        public static List<string> patched
        {
            get
            {
                return _cfg.patched;
            }
            set
            {
                _cfg.patched = value;
            }
        }
        public static void AddPatched(string modpackName)
        {
            patched.Add(modpackName);
        }
        public static void RmPatched(string modpackName)
        {
            patched.Remove(modpackName);
        }
        public static bool IsPatched(string modpackName)
        {
            try
            {
                return patched.Contains(modpackName);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
        #endregion

        #region Event Handlers
        public static void BrowseFolderBtn_Click(object sender, EventArgs e)
        {
            var dialog = new FolderSelectDialog
            {
                InitialDirectory = Config.MCC_home,
                Title = "Select a folder"
            };
            if (dialog.Show(Program.MasterForm.Handle))
            {
                ((Button)sender).Parent.GetChildAtPoint(new Point(5, 3)).Text = dialog.FileName;
            }
        }

        public static void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Program.MasterForm.cfgTextBox1.Text) || string.IsNullOrEmpty(Program.MasterForm.cfgTextBox2.Text) || string.IsNullOrEmpty(Program.MasterForm.cfgTextBox3.Text))
            {
                Utility.ShowMsg("Config entries must not be empty.", "Error");
                return;
            }

            if (!Config.chkHomeDir(Program.MasterForm.cfgTextBox1.Text))
            {
                Utility.ShowMsg("It seems you have selected the wrong MCC install directory. " +
                    "Please make sure to select the folder named 'Halo The Master Chief Collection' in your Steam files.", "Error");
                Program.MasterForm.cfgTextBox1.Text = Config.MCC_home;
                return;
            }
            Config.MCC_home = Program.MasterForm.cfgTextBox1.Text;
            Config.backup_dir = Program.MasterForm.cfgTextBox2.Text;
            Config.modpack_dir = Program.MasterForm.cfgTextBox3.Text;
            Config.deleteOldBaks = Program.MasterForm.delOldBaks_chb.Checked;

            Config.SaveCfg();

            Utility.ShowMsg("Config Updated!", "Info");
        }

        public static void ResetApp_Click(object sender, EventArgs e)
        {
            DialogResult ans = Utility.ShowMsg("WARNING: This should only be used after an offical MCC update has been applied." +
                "\r\n\r\nThis button will reset the application state, so that the mod manager believes your Halo install is COMPLETELY unmodded. It will " +
                "delete ALL of your backups, and WILL NOT restore them beforehand. This is because after an offical update, the backup files will be old." +
                "\r\n\r\nAre you sure you want to continue?", "Question");
            if (ans == DialogResult.No)
            {
                return;
            }

            Config.DoResetApp();
        }
        #endregion

        #region Api Functions
        public static void SaveCfg()
        {
            string json = JsonConvert.SerializeObject(_cfg, Formatting.Indented);
            using (FileStream fs = File.Create(_cfgLocation))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        public static bool LoadCfg()
        {
            if (!File.Exists(_cfgLocation))
            {
                CreateDefaultCfg();
            }
            else
            {
                int r = CheckCfg();
                if (r == 1)
                {
                    DialogResult ans = Utility.ShowMsg("Your configuration has format errors, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No)
                    {
                        return false;
                    }
                    CreateDefaultCfg();
                }
                else if (r == 2)
                {
                    DialogResult ans = Utility.ShowMsg("Your config file is using an old format, would you like to overwrite it with a default config?", "Question");
                    if (ans == DialogResult.No)
                    {
                        return false;
                    }
                    CreateDefaultCfg();
                }
            }

            if (patched == null)
            {
                DialogResult ans = Utility.ShowMsg("Your config file is using an old format, would you like to overwrite it with a default config?", "Question");
                if (ans == DialogResult.No)
                {
                    return false;
                }
                CreateDefaultCfg();
            }
            bool msg = false;
            List<string> tmp = new List<string>();
            foreach (string modpack in patched)
            {
                if (!Modpacks.VerifyExists(modpack))
                {
                    if (!msg)
                    {
                        msg = true;
                        Utility.ShowMsg("One or more of your enabled modpacks is missing from the modpacks folder. This likely means your game " +
                            "is in an unstable state. You should restore from backups or verify the game files through Steam." +
                            "\r\nThis warning will only show once.", "Warning");
                    }
                    tmp.Add(modpack);
                }
            }
            foreach (string modpack in tmp)
            {
                RmPatched(modpack);
            }
            SaveCfg();

            // Update config tab
            Program.MasterForm.cfgTextBox1.Text = MCC_home;
            Program.MasterForm.cfgTextBox2.Text = backup_dir;
            Program.MasterForm.cfgTextBox3.Text = modpack_dir;
            Program.MasterForm.delOldBaks_chb.Checked = deleteOldBaks;

            return true;
        }
        #endregion

        #region Helper Functions
        public static void DoResetApp()
        {
            patched = new List<string>();
            SaveCfg();
            Modpacks.LoadModpacks();
            if (!Backups.deleteAll(true))
            {
                Utility.ShowMsg("There was an issue deleting at least one backup. Please delete this manually in the Backups tab to avoid restoring an old " +
                    "version of the file in the future.", "Error");
            }
            Backups.LoadBackups();
        }

        public static bool CreateDefaultCfg()
        {
            _cfg = new MainCfg();
            // default values declared here so that mainCfg class does not implicitly set defaults and bypass warning triggers
            MCC_home = @"C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";
            backup_dir = @".\backups";
            modpack_dir = @".\modpacks";
            deleteOldBaks = false;
            patched = new List<string>();

            SaveCfg();
            Utility.ShowMsg("A default configuration file has been created. Please review and update it as needed.", "Info");

            return true;
        }

        private static int CheckCfg()
        {
            string json = File.ReadAllText(_cfgLocation);
            try
            {
                MainCfg values = JsonConvert.DeserializeObject<MainCfg>(json);
                // Future config version handling: float.Parse(values.version.Substring(1)) < 0.5
                if (String.IsNullOrEmpty(values.version))
                {
                    return 2;
                }

                MCC_home = values.MCC_home;
                backup_dir = values.backup_dir;
                modpack_dir = values.modpack_dir;
                deleteOldBaks = values.deleteOldBaks;
                patched = values.patched;
            }
            catch (JsonSerializationException)
            {
                return 1;
            }
            catch (JsonReaderException)
            {
                return 1;
            }
            catch (KeyNotFoundException)
            {
                return 1;
            }

            return 0;
        }

        public static bool chkHomeDir(String dir)
        {
            if (!File.Exists(dir + @"\haloreach\haloreach.dll"))
            {
                return false;
            }
            if (!File.Exists(dir + @"\MCC\Content\Paks\MCC-WindowsNoEditor.pak"))
            {
                return false;
            }
            if (!File.Exists(dir + @"\mcclauncher.exe"))
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
