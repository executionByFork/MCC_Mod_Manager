using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;
using System.IO.Compression;

namespace MCC_Mod_Manager
{
    static class Backups
    {
        public static Form1 form1;  // this is set on form load
        public static Dictionary<string, string> _baks = new Dictionary<string, string>();

        private static bool EnsureBackupFolderExists()
        {
            if (!Directory.Exists(Config.Backup_dir))
            {
                Directory.CreateDirectory(Config.Backup_dir);
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
            using (FileStream fs = File.Create(Config.BackupCfg))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
            return true;
        }

        public static bool UpdateBackupList()
        {
            EnsureBackupFolderExists();

            form1.bakListPanel_clear();
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
                    Location = new Point(30, form1.bakListPanel_getCount() * 20)
                };

                form1.bakListPanel_add(chb);
            }
            return true;
        }

        public static bool LoadBackups()
        {
            EnsureBackupFolderExists();
            if (!File.Exists(Config.BackupCfg))
            {   //TODO: create blank config
                return false;
            }

            bool err = false;
            string json = File.ReadAllText(Config.BackupCfg);
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
                DialogResult ans = form1.showMsg(
                    "The backup configuration file is corrupted. You may need to verify your game files on steam or reinstall." +
                    "Would you like to delete the corrupted backup config file?",
                    "Question"
                );
                if (ans == DialogResult.Yes)
                {
                    if (!IO.DeleteFile(Config.BackupCfg))
                    {
                        form1.showMsg("The backup file could not be deleted. Is it open somewhere?", "Error");
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
            string bakPath = Config.Backup_dir + @"\" + fileName + fileExt;

            if (File.Exists(bakPath))
            {
                if (!_baks.ContainsKey(path))
                { // if file exists in backups folder but the original filepaths differ, rename
                    int num = 0;
                    do
                    {
                        num++;
                        bakPath = Config.Backup_dir + @"\" + fileName + "(" + num + ")" + fileExt;
                    } while (File.Exists(bakPath));
                }
            }

            int res = IO.CopyFile(path, bakPath, overwrite);
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

            int ret;
            try
            {
                ret = IO.CopyFile(Config.Backup_dir + @"\" + _baks[filePath], filePath, true);
            }
            catch (KeyNotFoundException)
            {
                ret = 99;
            }
            if (ret == 0)
            {
                if (Config.DeleteOldBaks)
                {
                    if (IO.DeleteFile(Config.Backup_dir + @"\" + _baks[filePath]))
                    {
                        _baks.Remove(filePath);
                    }
                    else
                    {
                        form1.showMsg("Could not remove old backup '" + _baks[filePath] + "'. Is the file open somewhere?", "Error");
                    }
                }
                return true;
            }
            else if (ret == 99)
            {
                form1.showMsg("Could not locate a backup for the file at '" + filePath + "'.", "Error");
                return false;
            }
            else
            {
                form1.showMsg("Could not restore '" + _baks[filePath] + "'. If the game is open, close it and try again.", "Error");
                return false;
            }
        }

        public static int RestoreBaks(List<string> backupPathList)
        {
            EnsureBackupFolderExists();

            if (backupPathList.Count == 0)
            {
                return 0;
            }

            form1.PBar_show(backupPathList.Count);
            bool chk = false;
            bool err = false;
            foreach (string path in backupPathList)
            {
                form1.PBar_update();
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
            form1.PBar_hide();
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

        public static void NewBackup()
        {
            EnsureBackupFolderExists();

            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Config.MCC_home,
                Multiselect = true
            };

            bool newbaks = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (_baks.ContainsKey(file))
                    {
                        DialogResult ans = form1.showMsg("A backup of ' " + file + "' already exists. Would you like to overwrite?", "Question");
                        if (ans == DialogResult.No)
                        {
                            continue;
                        }
                    }

                    if (CreateBackup(file, true) != 0)
                    {
                        form1.showMsg("Could not create a backup of '" + file + "'. Is the file open somewhere?", "Error");
                    }
                    else
                    {
                        newbaks = true;
                    }
                }

                if (newbaks)
                {
                    form1.showMsg("New Backup(s) Created", "Info");
                    SaveBackups();
                    LoadBackups();
                }
            }
        }

        public static void RestoreSelected(IEnumerable<CheckBox> bakList)
        {
            List<string> backupPaths = new List<string>();
            foreach (CheckBox chb in bakList)
            {
                if (chb.Checked)
                {
                    if (form1.fullBakPath_Checked())
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

            if (backupPaths.Count == 0)
            {
                form1.showMsg("No items selected from the list.", "Error");
                return;
            }
            int r = RestoreBaks(backupPaths);
            if (r == 0)
            {
                form1.showMsg("Selected files have been restored.", "Info");
            }
            else if (r == 1)
            {
                form1.showMsg("At least one file restore failed. Your game may be in an unstable state.", "Warning");
            }
        }

        public static void RestoreAll()
        {
            EnsureBackupFolderExists();

            form1.PBar_show(_baks.Count);
            List<string> remainingBaks = new List<string>();
            bool chk = false;
            foreach (KeyValuePair<string, string> entry in _baks)
            {
                form1.PBar_update();
                string bakPath = Config.Backup_dir + @"\" + entry.Value;
                if (IO.CopyFile(bakPath, entry.Key, true) == 0)
                {
                    if (Config.DeleteOldBaks)
                    {
                        if (!IO.DeleteFile(bakPath))
                        {
                            remainingBaks.Add(entry.Key);
                            form1.showMsg("Could not remove old backup '" + entry.Value + "'. Is the file open somewhere?", "Error");
                        }
                    }
                    chk = true;
                }
                else
                {
                    remainingBaks.Add(entry.Key);
                    form1.showMsg("Could not restore '" + Path.GetFileName(entry.Key) + "'. If the game is open, close it and try again.", "Error");
                }
            }

            if (Config.DeleteOldBaks)
            {
                if (remainingBaks.Count == 0)
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
                form1.showMsg("Files have been restored.", "Info");
            }
            SaveBackups();
            UpdateBackupList();
            form1.PBar_hide();
        }

        private static List<string> FilterNeededBackups(List<string> paths)
        {
            List<string> requiredBaks = new List<string>();
            foreach (string enabledModpack in Config.GetEnabledModpacks())
            {
                modpackCfg modpackConfig = Modpacks.GetModpackConfig(enabledModpack);

                foreach (modpackEntry entry in modpackConfig.entries)
                {
                    foreach (string path in paths)
                    {
                        if (path == Modpacks.ExpandPath(entry.dest))
                        {
                            requiredBaks.Add(path);
                        }
                    }
                }
            }

            return requiredBaks;
        }

        public static bool DeleteBak(string path)
        {
            if (IO.DeleteFile(Config.Backup_dir + @"\" + _baks[path]))
            {
                _baks.Remove(path);
                return true;
            }
            return false;
        }

        public static void DeleteSelected(List<CheckBox> bakList)
        {
            EnsureBackupFolderExists();

            DialogResult ans = form1.showMsg("Are you sure you want to delete the selected backup(s)?\r\nNo crying afterwards?", "Question");
            if (ans == DialogResult.No)
            {
                return;
            }

            bool chk = false;
            form1.PBar_show(bakList.Count);
            List<string> toDelete = new List<string>();
            foreach (CheckBox chb in bakList)
            {
                form1.PBar_update();
                if (chb.Checked)
                {
                    chk = true;
                    string path;
                    if (form1.fullBakPath_Checked())
                    {
                        path = chb.Text.Replace(Config.dirtyPadding, "");
                    }
                    else
                    {
                        path = GetBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                    }
                    toDelete.Add(path);
                    chb.Checked = false;
                }
            }
            if (chk)
            {
                List<string> requiredBaks = FilterNeededBackups(toDelete);
                foreach (string path in toDelete)
                {
                    if (requiredBaks.Contains(path))
                    {
                        continue;
                    }
                    if (IO.DeleteFile(Config.Backup_dir + @"\" + _baks[path]))
                    {
                        _baks.Remove(path);
                    }
                    else
                    {
                        form1.showMsg("Could not delete '" + _baks[path] + "'. Is the file open somewhere?", "Error");
                    }
                }

                if (requiredBaks.Count == 0)
                {
                    form1.showMsg("Selected files have been deleted.", "Info");
                }
                else
                {
                    form1.showMsg(requiredBaks.Count + " backups were not deleted because the original file(s) are currently patched with a mod. " +
                        "Deleting these backups would make it impossible to unpatch the mod(s).", "Info");
                }
                SaveBackups();
                UpdateBackupList();
                form1.PBar_hide();
            }
            else
            {
                form1.showMsg("No items selected from the list.", "Error");
                form1.PBar_hide();
                return;
            }
        }

        public static bool DeleteAll(bool y)
        {
            EnsureBackupFolderExists();

            if (!y)
            {
                DialogResult ans = form1.showMsg("Are you sure you want to delete ALL of your backup(s)?\r\nNo crying afterwards?", "Question");
                if (ans == DialogResult.No)
                {
                    return true;
                }
            }

            bool err = false;
            form1.PBar_show(_baks.Count);
            List<string> toDelete = new List<string>();
            foreach (KeyValuePair<string, string> entry in _baks)
            {
                toDelete.Add(entry.Key);
            }
            if (toDelete.Count == 0)
            {
                form1.showMsg("Nothing to delete.", "Info");
                return true;
            }

            List<string> remainingBaks = new List<string>();
            List<string> requiredBaks = FilterNeededBackups(toDelete);
            foreach (string path in toDelete)
            {
                if (requiredBaks.Contains(path))
                {
                    remainingBaks.Add(path);
                    continue;
                }
                if (!IO.DeleteFile(Config.Backup_dir + @"\" + _baks[path]))
                {
                    remainingBaks.Add(path);
                    form1.showMsg("Could not delete '" + path + "'. Is the file open somewhere?", "Error");
                    err = true;
                }
            }

            if (remainingBaks.Count == 0)
            {
                _baks = new Dictionary<string, string>();
                form1.showMsg("All backups deleted.", "Info");
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
            form1.PBar_hide();

            if (requiredBaks.Count != 0)
            {
                form1.showMsg(requiredBaks.Count + " backups were not deleted because the original file(s) are currently patched with a mod. " +
                        "Deleting these backups would make it impossible to unpatch the mod(s).", "Info");
            }
            return !err;
        }
    }
}
