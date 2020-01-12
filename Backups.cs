using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;

namespace MCC_Mod_Manager
{
    static class Backups
    {
        public static Form1 form1;  // this is set on form load
        public static Dictionary<string, string> _baks = new Dictionary<string, string>();

        private static bool ensureBackupFolderExists()
        {
            if (!Directory.Exists(Config.backup_dir)) {
                Directory.CreateDirectory(Config.backup_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
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

            form1.bakListPanel_clear();
            foreach (KeyValuePair<string, string> entry in _baks) {
                CheckBox chb = new CheckBox {
                    AutoSize = true,
                    Text = Config.dirtyPadding + entry.Key,
                    Location = new Point(30, form1.bakListPanel_getCount() * 20)
                };

                form1.bakListPanel_add(chb);
            }
            return true;
        }

        public static bool loadBackups()
        {
            ensureBackupFolderExists();
            if (!File.Exists(Config.backupCfg)) {
                return false;
            }

            string json = File.ReadAllText(Config.backupCfg);
            try {
                _baks = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
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
                    if (!IO.DeleteFile(Config.backupCfg)) {
                        MessageBox.Show("The backup file could not be deleted. Is it open somewhere?", "Error");
                    }
                }
            }

            return true;
        }

        public static int createBackup(string path, bool overwrite)
        {
            ensureBackupFolderExists();

            String fileName = Path.GetFileName(path);
            int res = IO.CopyFile(path, Config.backup_dir + @"\" + fileName, overwrite);
            if (res == 0 || res == 1) {
                _baks[fileName] = path;
                saveBackups();
                updateBackupList();
            }
            return res;
        }

        public static int restoreBaks(List<string> backupNames)
        {
            ensureBackupFolderExists();

            if (backupNames.Count() == 0) {
                return 0;
            }

            form1.pBar_show(backupNames.Count());
            bool chk = false;
            bool err = false;
            foreach (string fileName in backupNames) {
                form1.pBar_update();
                if (IO.CopyFile(Config.backup_dir + @"\" + fileName, _baks[fileName], true) == 0) {
                    if (Config.deleteOldBaks) {
                        if (IO.DeleteFile(Config.backup_dir + @"\" + fileName)) {
                            _baks.Remove(fileName);
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
            form1.pBar_hide();
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
                if (File.Exists(Config.backup_dir + @"\" + Path.GetFileName(ofd.FileName))) {
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

        public static void restoreSelected(IEnumerable<CheckBox> bakList)
        {
            List<string> backupNames = new List<string>();
            foreach (CheckBox chb in bakList) {
                if (chb.Checked) {
                    backupNames.Add(chb.Text.Replace(Config.dirtyPadding, ""));
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

        public static void restoreAll()
        {
            ensureBackupFolderExists();

            form1.pBar_show(_baks.Count());
            List<string> remainingBaks = new List<string>();
            bool chk = false;
            foreach (KeyValuePair<string, string> entry in _baks) {
                form1.pBar_update();
                if (IO.CopyFile(Config.backup_dir + @"\" + entry.Key, entry.Value, true) == 0) {
                    if (Config.deleteOldBaks) {
                        if (!IO.DeleteFile(Config.backup_dir + @"\" + entry.Key)) {
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

            if (Config.deleteOldBaks) {
                if (remainingBaks.Count() == 0) {
                    _baks = new Dictionary<string, string>();
                } else {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    foreach (string file in remainingBaks) {    // create backup config of files which couldn't be restored and removed
                        tmp[file] = Backups._baks[file];
                    }
                    _baks = tmp;
                }
            }

            if (chk) {
                MessageBox.Show("Files have been restored.", "Info");
            }
            saveBackups();
            updateBackupList();
            form1.pBar_hide();
        }

        public static void deleteSelected(IEnumerable<CheckBox> bakList)
        {
            ensureBackupFolderExists();

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
            form1.pBar_show(bakList.Count());
            foreach (CheckBox chb in bakList) {
                form1.pBar_update();
                if (chb.Checked) {
                    chk = true;
                    string fileName = chb.Text.Replace(Config.dirtyPadding, "");
                    if (IO.DeleteFile(Config.backup_dir + @"\" + fileName)) {
                        _baks.Remove(fileName);
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
            form1.pBar_hide();
        }

        public static void deleteAll()
        {
            ensureBackupFolderExists();

            DialogResult ans = MessageBox.Show(
                "Are you sure you want to delete ALL of your backup(s)?\r\nNo crying afterwards?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (ans == DialogResult.No) {
                return;
            }

            form1.pBar_show(_baks.Count());
            List<string> remainingBaks = new List<string>();
            foreach (KeyValuePair<string, string> entry in _baks) {
                form1.pBar_update();
                if (!IO.DeleteFile(Config.backup_dir + @"\" + entry.Key)) {
                    remainingBaks.Add(entry.Key);
                    MessageBox.Show("Could not delete '" + entry.Key + "'. Is the file open somewhere?", "Error");
                }
            }
            if (remainingBaks.Count() == 0) {
                _baks = new Dictionary<string, string>();
                MessageBox.Show("All backups deleted.", "Info");
            } else {
                Dictionary<string, string> tmp = new Dictionary<string, string>();
                foreach (string file in remainingBaks) {    // create backup config of files which couldn't be deleted
                    tmp[file] = _baks[file];
                }
                _baks = tmp;
            }
            saveBackups();
            updateBackupList();
            form1.pBar_hide();
        }
    }
}
