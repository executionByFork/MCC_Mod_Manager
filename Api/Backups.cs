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

        private static bool ensureBackupFolderExists()
        {
            if (!Directory.Exists(Config.backup_dir)) {
                Directory.CreateDirectory(Config.backup_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static string getBakKey(string bakFileName)
        {
            foreach (KeyValuePair<string, string> entry in _baks) {
                if (entry.Value == bakFileName) {
                    return entry.Key;
                }
            }
            return null;
        }

        public static bool saveBackups()
        {
            ensureBackupFolderExists();

            string json = JsonConvert.SerializeObject(_baks, Formatting.Indented);
            using (FileStream fs = File.Create(Config.backupCfg)) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
            return true;
        }

        public static bool updateBackupList()
        {
            ensureBackupFolderExists();

            Program.MasterForm.bakListPanel_clear();
            foreach (KeyValuePair<string, string> entry in _baks) {
                string entryName;
                if (Config.fullBakPath) {
                    entryName = entry.Key;
                } else {
                    entryName = entry.Value;
                }
                CheckBox chb = new CheckBox {
                    AutoSize = true,
                    Text = Config.dirtyPadding + entryName,
                    Location = new Point(30, Program.MasterForm.bakListPanel_getCount() * 20)
                };

                Program.MasterForm.bakListPanel_add(chb);
            }
            return true;
        }

        public static bool LoadBackups()
        {
            ensureBackupFolderExists();
            if (!File.Exists(Config.backupCfg)) {   //TODO: create blank config
                return false;
            }

            bool err = false;
            string json = File.ReadAllText(Config.backupCfg);
            try {
                _baks = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                updateBackupList();
            } catch (JsonSerializationException) {
                err = true;
            } catch (JsonReaderException) {
                err = true;
            }
            if (err) {
                DialogResult ans = Utility.ShowMsg(
                    "The backup configuration file is corrupted. You may need to verify your game files on steam or reinstall." +
                    "Would you like to delete the corrupted backup config file?",
                    "Question"
                );
                if (ans == DialogResult.Yes) {
                    if (!Utility.DeleteFile(Config.backupCfg)) {
                        Utility.ShowMsg("The backup file could not be deleted. Is it open somewhere?", "Error");
                    }
                }
            }

            return true;
        }

        public static int createBackup(string path, bool overwrite)
        {
            ensureBackupFolderExists();
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);
            string bakPath = Config.backup_dir + @"\" + fileName + fileExt;
            
            if (File.Exists(bakPath)) {
                if (!_baks.ContainsKey(path)) { // if file exists in backups folder but the original filepaths differ, rename
                    int num = 0;
                    do {
                        num++;
                        bakPath = Config.backup_dir + @"\" + fileName + "(" + num + ")" + fileExt;
                    } while (File.Exists(bakPath));
                }
            }

            int res = Utility.CopyFile(path, bakPath, overwrite);
            if (res == 0 || res == 1) {
                _baks[path] = Path.GetFileName(bakPath);
                saveBackups();
                updateBackupList();
            }
            return res;
        }

        public static bool restoreBak(string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) {
                return false;
            }

            if (Utility.CopyFile(Config.backup_dir + @"\" + _baks[filePath], filePath, true) == 0) {
                if (Config.deleteOldBaks) {
                    if (Utility.DeleteFile(Config.backup_dir + @"\" + _baks[filePath])) {
                        _baks.Remove(filePath);
                    } else {
                        Utility.ShowMsg("Could not remove old backup '" + _baks[filePath] + "'. Is the file open somewhere?", "Error");
                    }
                }
                return true;
            } else {
                Utility.ShowMsg("Could not restore '" + _baks[filePath] + "'. If the game is open, close it and try again.", "Error");
                return false;
            }
        }

        public static int restoreBaks(List<string> backupPathList)
        {
            ensureBackupFolderExists();

            if (backupPathList.Count() == 0) {
                return 0;
            }

            Program.MasterForm.pBar_show(backupPathList.Count());
            bool chk = false;
            bool err = false;
            foreach (string path in backupPathList) {
                Program.MasterForm.pBar_update();
                if (restoreBak(path)) {
                    chk = true;
                } else {
                    err = true;
                }
            }
            saveBackups();
            updateBackupList();
            Program.MasterForm.pBar_hide();
            if (chk) {
                if (err) {
                    return 1;   // Partial success - Some files were restored
                }
                return 0;   // Success - All files were restored
            }
            return 2;   // Failure - No files were restored
        }

        public static void newBackup()
        {
            ensureBackupFolderExists();

            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = Config.MCC_home
            };
            if (ofd.ShowDialog() == DialogResult.OK) {
                if (_baks.ContainsKey(ofd.FileName)) {
                    DialogResult ans = Utility.ShowMsg("A backup of that file already exists. Would you like to overwrite?", "Question");
                    if (ans == DialogResult.No) {
                        return;
                    }
                }

                if (createBackup(ofd.FileName, true) != 0) {
                    Utility.ShowMsg("Could not create a backup of the chosen file. Is the file open somewhere?", "Error");
                } else {
                    Utility.ShowMsg("New Backup Created", "Info");
                    saveBackups();
                    LoadBackups();
                }
            }
        }

        public static void restoreSelected(IEnumerable<CheckBox> bakList)
        {
            List<string> backupPaths = new List<string>();
            foreach (CheckBox chb in bakList) {
                if (chb.Checked) {
                    if (Program.MasterForm.fullBakPath_Checked()) {
                        backupPaths.Add(chb.Text.Replace(Config.dirtyPadding, ""));
                    } else {
                        backupPaths.Add(getBakKey(chb.Text.Replace(Config.dirtyPadding, "")));
                    }
                    chb.Checked = false;
                }
            }

            if (backupPaths.Count() == 0) {
                Utility.ShowMsg("No items selected from the list.", "Error");
                return;
            }
            int r = restoreBaks(backupPaths);
            if (r == 0) {
                Utility.ShowMsg("Selected files have been restored.", "Info");
            } else if (r == 1) {
                Utility.ShowMsg("At least one file restore failed. Your game may be in an unstable state.", "Warning");
            }
        }

        public static void restoreAll()
        {
            ensureBackupFolderExists();

            Program.MasterForm.pBar_show(_baks.Count());
            List<string> remainingBaks = new List<string>();
            bool chk = false;
            foreach (KeyValuePair<string, string> entry in _baks) {
                Program.MasterForm.pBar_update();
                string bakPath = Config.backup_dir + @"\" + entry.Value;
                if (Utility.CopyFile(bakPath, entry.Key, true) == 0) {
                    if (Config.deleteOldBaks) {
                        if (!Utility.DeleteFile(bakPath)) {
                            remainingBaks.Add(entry.Key);
                            Utility.ShowMsg("Could not remove old backup '" + entry.Value + "'. Is the file open somewhere?", "Error");
                        }
                    }
                    chk = true;
                } else {
                    remainingBaks.Add(entry.Key);
                    Utility.ShowMsg("Could not restore '" + Path.GetFileName(entry.Key) + "'. If the game is open, close it and try again.", "Error");
                }
            }

            if (Config.deleteOldBaks) {
                if (remainingBaks.Count() == 0) {
                    _baks = new Dictionary<string, string>();
                } else {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    foreach (string path in remainingBaks) {    // create backup config of files which couldn't be restored/removed
                        tmp[path] = _baks[path];
                    }
                    _baks = tmp;
                }
            }

            if (chk) {
                Utility.ShowMsg("Files have been restored.", "Info");
            }
            saveBackups();
            updateBackupList();
            Program.MasterForm.pBar_hide();
        }

        public static void deleteSelected(IEnumerable<CheckBox> bakList)
        {
            ensureBackupFolderExists();

            DialogResult ans = Utility.ShowMsg("Are you sure you want to delete the selected backup(s)?\r\nNo crying afterwards?", "Question");
            if (ans == DialogResult.No) {
                return;
            }

            bool chk = false;
            Program.MasterForm.pBar_show(bakList.Count());
            foreach (CheckBox chb in bakList) {
                Program.MasterForm.pBar_update();
                if (chb.Checked) {
                    chk = true;
                    string path;
                    if (Program.MasterForm.fullBakPath_Checked()) {
                        path = chb.Text.Replace(Config.dirtyPadding, "");
                    } else {
                        path = getBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                    }

                    if (Utility.DeleteFile(Config.backup_dir + @"\" + _baks[path])) {
                        _baks.Remove(path);
                    } else {
                        Utility.ShowMsg("Could not delete '" + _baks[path] + "'. Is the file open somewhere?", "Error");
                    }
                    chb.Checked = false;
                }
            }
            if (!chk) {
                Utility.ShowMsg("No items selected from the list.", "Error");
            } else {
                saveBackups();
                Utility.ShowMsg("Selected files have been deleted.", "Info");
                updateBackupList();
            }
            Program.MasterForm.pBar_hide();
        }

        public static bool deleteAll(bool y)
        {
            ensureBackupFolderExists();

            if (!y) {
                DialogResult ans = Utility.ShowMsg("Are you sure you want to delete ALL of your backup(s)?\r\nNo crying afterwards?", "Question");
                if (ans == DialogResult.No) {
                    return true;
                }
            }

            bool err = false;
            Program.MasterForm.pBar_show(_baks.Count());
            List<string> remainingBaks = new List<string>();
            foreach (KeyValuePair<string, string> entry in _baks) {
                Program.MasterForm.pBar_update();
                if (!Utility.DeleteFile(Config.backup_dir + @"\" + entry.Value)) {
                    remainingBaks.Add(entry.Key);
                    Utility.ShowMsg("Could not delete '" + entry.Value + "'. Is the file open somewhere?", "Error");
                    err = true;
                }
            }
            if (remainingBaks.Count() == 0) {
                _baks = new Dictionary<string, string>();
                Utility.ShowMsg("All backups deleted.", "Info");
            } else {
                Dictionary<string, string> tmp = new Dictionary<string, string>();
                foreach (string path in remainingBaks) {    // create backup config of files which couldn't be deleted
                    tmp[path] = _baks[path];
                }
                _baks = tmp;
            }
            saveBackups();
            updateBackupList();
            Program.MasterForm.pBar_hide();

            return !err;
        }
    }
}
