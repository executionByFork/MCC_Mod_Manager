using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;
using MCC_Mod_Manager.Api;

namespace MCC_Mod_Manager
{
    static class Backups
    {
        public static Dictionary<string, string> _baks = new Dictionary<string, string>();

        #region Event Handlers
        public static void ShowFullPathCheckbox_Click(object sender, EventArgs e)
        {
            Config.fullBakPath = Program.MasterForm.fullBakPath_chb.Checked;
            if (Config.fullBakPath)
            {   // swapping from filename to full path
                foreach (CheckBox chb in Program.MasterForm.bakListPanel.Controls.OfType<CheckBox>())
                {
                    chb.Text = Config.dirtyPadding + Backups.GetBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                }
            }
            else
            {    // swapping from full path to filename
                foreach (CheckBox chb in Program.MasterForm.bakListPanel.Controls.OfType<CheckBox>())
                {
                    chb.Text = Config.dirtyPadding + Backups._baks[chb.Text.Replace(Config.dirtyPadding, "")];
                }
            }
        }

        public static void MakeBakBtn_Click(object sender, EventArgs e)
        {
            EnsureBackupFolderExists();

            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Config.MCC_home
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (_baks.ContainsKey(ofd.FileName))
                {
                    DialogResult ans = Utility.ShowMsg("A backup of that file already exists. Would you like to overwrite?", "Question");
                    if (ans == DialogResult.No)
                    {
                        return;
                    }
                }

                if (CreateBackup(ofd.FileName, true) != 0)
                {
                    Utility.ShowMsg("Could not create a backup of the chosen file. Is the file open somewhere?", "Error");
                }
                else
                {
                    Utility.ShowMsg("New Backup Created", "Info");
                    SaveBackups();
                    LoadBackups();
                }
            }
        }

        public static void RestoreSelectedBtn_Click(object sender, EventArgs e)
        {
            IEnumerable<CheckBox> bakList = Program.MasterForm.bakListPanel.Controls.OfType<CheckBox>();
            List<string> backupPaths = new List<string>();
            foreach (CheckBox chb in bakList)
            {
                if (chb.Checked)
                {
                    if (Program.MasterForm.fullBakPath_chb.Checked)
                    {
                        backupPaths.Add(chb.Text.Replace(Config.dirtyPadding, ""));
                    }
                    else
                    {
                        backupPaths.Add(GetBakKey(chb.Text.Replace(Config.dirtyPadding, "")));
                    }
                    chb.Checked = false;
                }
            }

            if (backupPaths.Count() == 0)
            {
                Utility.ShowMsg("No items selected from the list.", "Error");
                return;
            }
            int r = RestoreBaks(backupPaths);
            if (r == 0)
            {
                Utility.ShowMsg("Selected files have been restored.", "Info");
            }
            else if (r == 1)
            {
                Utility.ShowMsg("At least one file restore failed. Your game may be in an unstable state.", "Warning");
            }
        }

        public static void RestoreAllBaksBtn_Click(object sender, EventArgs e)
        {
            EnsureBackupFolderExists();

            Program.MasterForm.pBar_show(_baks.Count());
            List<string> remainingBaks = new List<string>();
            bool chk = false;
            foreach (KeyValuePair<string, string> entry in _baks)
            {
                Program.MasterForm.pBar_update();
                string bakPath = Config.backup_dir + @"\" + entry.Value;
                if (Utility.CopyFile(bakPath, entry.Key, true) == 0)
                {
                    if (Config.deleteOldBaks)
                    {
                        if (!Utility.DeleteFile(bakPath))
                        {
                            remainingBaks.Add(entry.Key);
                            Utility.ShowMsg("Could not remove old backup '" + entry.Value + "'. Is the file open somewhere?", "Error");
                        }
                    }
                    chk = true;
                }
                else
                {
                    remainingBaks.Add(entry.Key);
                    Utility.ShowMsg("Could not restore '" + Path.GetFileName(entry.Key) + "'. If the game is open, close it and try again.", "Error");
                }
            }

            if (Config.deleteOldBaks)
            {
                if (remainingBaks.Count() == 0)
                {
                    _baks = new Dictionary<string, string>();
                }
                else
                {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    foreach (string path in remainingBaks)
                    {    // create backup config of files which couldn't be restored/removed
                        tmp[path] = _baks[path];
                    }
                    _baks = tmp;
                }
            }

            if (chk)
            {
                Utility.ShowMsg("Files have been restored.", "Info");
            }
            SaveBackups();
            UpdateBackupList();
            Program.MasterForm.pBar_hide();
        }

        public static void DelSelectedBak_Click(object sender, EventArgs e)
        {
            IEnumerable<CheckBox> bakList = Program.MasterForm.bakListPanel.Controls.OfType<CheckBox>();
            EnsureBackupFolderExists();

            DialogResult ans = Utility.ShowMsg("Are you sure you want to delete the selected backup(s)?\r\nNo crying afterwards?", "Question");
            if (ans == DialogResult.No)
            {
                return;
            }

            bool chk = false;
            Program.MasterForm.pBar_show(bakList.Count());
            foreach (CheckBox chb in bakList)
            {
                Program.MasterForm.pBar_update();
                if (chb.Checked)
                {
                    chk = true;
                    string path;
                    if (Program.MasterForm.fullBakPath_chb.Checked)
                    {
                        path = chb.Text.Replace(Config.dirtyPadding, "");
                    }
                    else
                    {
                        path = GetBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                    }

                    if (Utility.DeleteFile(Config.backup_dir + @"\" + _baks[path]))
                    {
                        _baks.Remove(path);
                    }
                    else
                    {
                        Utility.ShowMsg("Could not delete '" + _baks[path] + "'. Is the file open somewhere?", "Error");
                    }
                    chb.Checked = false;
                }
            }
            if (!chk)
            {
                Utility.ShowMsg("No items selected from the list.", "Error");
            }
            else
            {
                SaveBackups();
                Utility.ShowMsg("Selected files have been deleted.", "Info");
                UpdateBackupList();
            }
            Program.MasterForm.pBar_hide();
        }

        public static void DelAllBaksBtn_Click(object sender, EventArgs e)
        {
            Backups.DeleteAll(false);
        }
        #endregion

        #region Api Functions
        public static bool LoadBackups()
        {
            EnsureBackupFolderExists();
            if (!File.Exists(Config.backupCfg))
            {   //TODO: create blank config
                return false;
            }

            bool err = false;
            string json = File.ReadAllText(Config.backupCfg);
            try
            {
                _baks = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                UpdateBackupList();
            }
            catch (JsonSerializationException)
            {
                err = true;
            }
            catch (JsonReaderException)
            {
                err = true;
            }
            if (err)
            {
                DialogResult ans = Utility.ShowMsg(
                    "The backup configuration file is corrupted. You may need to verify your game files on steam or reinstall." +
                    "Would you like to delete the corrupted backup config file?",
                    "Question"
                );
                if (ans == DialogResult.Yes)
                {
                    if (!Utility.DeleteFile(Config.backupCfg))
                    {
                        Utility.ShowMsg("The backup file could not be deleted. Is it open somewhere?", "Error");
                    }
                }
            }

            return true;
        }

        public static int CreateBackup(string path, bool overwrite)
        {
            EnsureBackupFolderExists();
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);
            string bakPath = Config.backup_dir + @"\" + fileName + fileExt;

            if (File.Exists(bakPath))
            {
                if (!_baks.ContainsKey(path))
                { // if file exists in backups folder but the original filepaths differ, rename
                    int num = 0;
                    do
                    {
                        num++;
                        bakPath = Config.backup_dir + @"\" + fileName + "(" + num + ")" + fileExt;
                    } while (File.Exists(bakPath));
                }
            }

            int res = Utility.CopyFile(path, bakPath, overwrite);
            if (res == 0 || res == 1)
            {
                _baks[path] = Path.GetFileName(bakPath);
                SaveBackups();
                UpdateBackupList();
            }
            return res;
        }

        public static bool RestoreBak(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (Utility.CopyFile(Config.backup_dir + @"\" + _baks[filePath], filePath, true) == 0)
            {
                if (Config.deleteOldBaks)
                {
                    if (Utility.DeleteFile(Config.backup_dir + @"\" + _baks[filePath]))
                    {
                        _baks.Remove(filePath);
                    }
                    else
                    {
                        Utility.ShowMsg("Could not remove old backup '" + _baks[filePath] + "'. Is the file open somewhere?", "Error");
                    }
                }
                return true;
            }
            else
            {
                Utility.ShowMsg("Could not restore '" + _baks[filePath] + "'. If the game is open, close it and try again.", "Error");
                return false;
            }
        }

        public static int RestoreBaks(List<string> backupPathList)
        {
            EnsureBackupFolderExists();

            if (backupPathList.Count() == 0)
            {
                return 0;
            }

            Program.MasterForm.pBar_show(backupPathList.Count());
            bool chk = false;
            bool err = false;
            foreach (string path in backupPathList)
            {
                Program.MasterForm.pBar_update();
                if (RestoreBak(path))
                {
                    chk = true;
                }
                else
                {
                    err = true;
                }
            }
            SaveBackups();
            UpdateBackupList();
            Program.MasterForm.pBar_hide();
            if (chk)
            {
                if (err)
                {
                    return 1;   // Partial success - Some files were restored
                }
                return 0;   // Success - All files were restored
            }
            return 2;   // Failure - No files were restored
        }

        public static bool DeleteAll(bool y)
        {
            EnsureBackupFolderExists();

            if (!y)
            {
                DialogResult ans = Utility.ShowMsg("Are you sure you want to delete ALL of your backup(s)?\r\nNo crying afterwards?", "Question");
                if (ans == DialogResult.No)
                {
                    return true;
                }
            }

            bool err = false;
            Program.MasterForm.pBar_show(_baks.Count());
            List<string> remainingBaks = new List<string>();
            foreach (KeyValuePair<string, string> entry in _baks)
            {
                Program.MasterForm.pBar_update();
                if (!Utility.DeleteFile(Config.backup_dir + @"\" + entry.Value))
                {
                    remainingBaks.Add(entry.Key);
                    Utility.ShowMsg("Could not delete '" + entry.Value + "'. Is the file open somewhere?", "Error");
                    err = true;
                }
            }
            if (remainingBaks.Count() == 0)
            {
                _baks = new Dictionary<string, string>();
                Utility.ShowMsg("All backups deleted.", "Info");
            }
            else
            {
                Dictionary<string, string> tmp = new Dictionary<string, string>();
                foreach (string path in remainingBaks)
                {    // create backup config of files which couldn't be deleted
                    tmp[path] = _baks[path];
                }
                _baks = tmp;
            }
            SaveBackups();
            UpdateBackupList();
            Program.MasterForm.pBar_hide();

            return !err;
        }

        #endregion

        #region Helper Functions
        private static bool EnsureBackupFolderExists()
        {
            if (!Directory.Exists(Config.backup_dir))
            {
                Directory.CreateDirectory(Config.backup_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static string GetBakKey(string bakFileName)
        {
            foreach (KeyValuePair<string, string> entry in _baks)
            {
                if (entry.Value == bakFileName)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        public static bool SaveBackups()
        {
            EnsureBackupFolderExists();

            string json = JsonConvert.SerializeObject(_baks, Formatting.Indented);
            using (FileStream fs = File.Create(Config.backupCfg))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
            return true;
        }

        public static bool UpdateBackupList()
        {
            EnsureBackupFolderExists();

            Program.MasterForm.bakListPanel.Controls.Clear();
            foreach (KeyValuePair<string, string> entry in _baks)
            {
                string entryName;
                if (Config.fullBakPath)
                {
                    entryName = entry.Key;
                }
                else
                {
                    entryName = entry.Value;
                }
                CheckBox chb = new CheckBox
                {
                    AutoSize = true,
                    Text = Config.dirtyPadding + entryName,
                    Location = new Point(30, Program.MasterForm.bakListPanel.Controls.Count * 20)
                };

                Program.MasterForm.bakListPanel.Controls.Add(chb);
            }
            return true;
        }
        #endregion
    }
}
