﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;
using System.IO.Compression;
using MCC_Mod_Manager.Api.Utilities;

namespace MCC_Mod_Manager.Api {
    static class Backups {
        public static Dictionary<string, string> _baks = new Dictionary<string, string>();

        #region Event Handlers

        public static void ShowFullPathCheckbox_Click(object sender, EventArgs e) {
            Config.fullBakPath = Program.MasterForm.fullBakPath_chb.Checked;
            foreach (Panel p in Program.MasterForm.bakListPanel.Controls.OfType<Panel>()) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.BackupsChbPoint);
                if (Config.fullBakPath) {   // swapping from filename to full path
                    chb.Text = Config.dirtyPadding + Backups.GetBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                    p.Width = chb.Width + 40;
                } else {    // swapping from full path to filename
                    chb.Text = Config.dirtyPadding + Backups._baks[chb.Text.Replace(Config.dirtyPadding, "")];
                    p.Width = Config.backupPanelWidth;
                }
            }
        }

        public static void MakeBakBtn_Click(object sender, EventArgs e) {
            EnsureBackupFolderExists();

            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = Config.MCC_home,
                Multiselect = true
            };

            bool newbaks = false;
            if (ofd.ShowDialog() == DialogResult.OK) {
                foreach (string file in ofd.FileNames) {
                    if (_baks.ContainsKey(file)) {
                        DialogResult ans = Utility.ShowMsg("A backup of ' " + file + "' already exists. Would you like to overwrite?", "Question");
                        if (ans == DialogResult.No) {
                            continue;
                        }
                    }

                    if (CreateBackup(file, true) != 0) {
                        Utility.ShowMsg("Could not create a backup of '" + file + "'. Is the file open somewhere?", "Error");
                    } else {
                        newbaks = true;
                    }
                }

                if (newbaks) {
                    Utility.ShowMsg("New Backup(s) Created", "Info");
                    SaveBackups();
                    LoadBackups();
                }
            }
        }

        private static Dictionary<string, List<string>> checkForPartialRestores(List<string> paths) {
            // paths is list of full, original file paths of backed up files
            Dictionary<string, List<string>> enabledModpacks = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, patchedEntry> item in Config.Patched) {
                enabledModpacks[item.Key] = new List<string>(item.Value.files.Keys);
            }

            foreach (string path in paths) {
                foreach (KeyValuePair<string, List<string>> modpack in enabledModpacks) {
                    if (modpack.Value.Contains(Utility.CompressPath(path))) {
                        modpack.Value.Remove(Utility.CompressPath(path));
                        break;
                    }
                }
            }

            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();
            results["disable"] = new List<string>();
            results["partials"] = new List<string>();

            Dictionary<string, patchedEntry> enabledModpacksCheck = Config.Patched;
            foreach (KeyValuePair<string, List<string>> modpack in enabledModpacks) {
                if (!modpack.Value.Any()) {
                    results["disable"].Add(modpack.Key);
                } else if (modpack.Value.Count != enabledModpacksCheck[modpack.Key].files.Count) {
                    results["partials"].Add(modpack.Key);
                }
            }

            return results;
        }

        public static void RestoreSelectedBtn_Click(object sender, EventArgs e) {
            IEnumerable<Panel> bakList = Program.MasterForm.bakListPanel.Controls.OfType<Panel>();
            List<string> backupPaths = new List<string>();
            foreach (Panel p in bakList) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.BackupsChbPoint);
                if (chb.Checked) {
                    if (Program.MasterForm.fullBakPath_chb.Checked) {
                        backupPaths.Add(chb.Text.Replace(Config.dirtyPadding, ""));
                    } else {
                        backupPaths.Add(GetBakKey(chb.Text.Replace(Config.dirtyPadding, "")));
                    }
                    chb.Checked = false;
                }
            }

            if (!backupPaths.Any()) {
                Utility.ShowMsg("No items selected from the list.", "Error");
                return;
            }
            Dictionary<string, List<string>> x = checkForPartialRestores(backupPaths);
            if (x["partials"].Any()) {
                DialogResult ans = Utility.ShowMsg("Restoring the selected backups will partially unpatch at least one modpack. All files within a modpack " +
                    "are intended to be patched and unpatched together. This may cause issues with your game.\r\nContinue?", "Question");
                if (ans == DialogResult.No) {
                    return;
                }
            }
            int r = RestoreBaks(backupPaths);
            MyMods.setModpacksDisabled(x["disable"]);
            MyMods.setModpacksPartial(x["partials"]);

            if (r == 0) {
                Utility.ShowMsg("Selected files have been restored.", "Info");
            } else if (r == 1 || r == 2) {
                Utility.ShowMsg("At least one file restore failed. Your game may be in an unstable state.", "Warning");
            }
        }

        public static void RestoreAllBaksBtn_Click(object sender, EventArgs e) {
            EnsureBackupFolderExists();
            List<string> bakList = new List<string>();
            foreach (KeyValuePair<string, string> b in _baks) {
                bakList.Add(b.Key);
            }

            Dictionary<string, List<string>> x = checkForPartialRestores(bakList);
            if (x["partials"].Any()) {
                DialogResult ans = Utility.ShowMsg("Restoring the selected backups will partially unpatch at least one modpack. All files within a modpack " +
                    "are intended to be patched and unpatched together. This may cause issues with your game.\r\nContinue?", "Question");
                if (ans == DialogResult.No) {
                    return;
                }
            }
            int r = RestoreBaks(bakList);
            MyMods.setModpacksDisabled(x["disable"]);
            MyMods.setModpacksPartial(x["partials"]);

            if (r == 0) {
                Utility.ShowMsg("Files have been restored.", "Info");
            } else if (r == 1 || r == 2) {
                Utility.ShowMsg("At least one file restore failed. Your game may be in an unstable state.", "Warning");
            }
        }

        public static void DelSelectedBak_Click(object sender, EventArgs e) {
            IEnumerable<Panel> bakList = Program.MasterForm.bakListPanel.Controls.OfType<Panel>();
            EnsureBackupFolderExists();

            DialogResult ans = Utility.ShowMsg("Are you sure you want to delete the selected backup(s)?\r\nNo crying afterwards?", "Question");
            if (ans == DialogResult.No) {
                return;
            }

            bool chk = false;
            Program.MasterForm.PBar_show(bakList.Count());
            List<string> toDelete = new List<string>();
            foreach (Panel p in bakList) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.BackupsChbPoint);
                Program.MasterForm.PBar_update();
                if (chb.Checked) {
                    chk = true;
                    string path;
                    if (Program.MasterForm.fullBakPath_chb.Checked) {
                        path = chb.Text.Replace(Config.dirtyPadding, "");
                    } else {
                        path = GetBakKey(chb.Text.Replace(Config.dirtyPadding, ""));
                    }
                    toDelete.Add(path);
                    chb.Checked = false;
                }
            }
            if (chk) {
                List<string> requiredBaks = FilterNeededBackups(toDelete);
                foreach (string path in toDelete) {
                    if (requiredBaks.Contains(path)) {
                        continue;
                    }
                    if (Utility.DeleteFile(Config.Backup_dir + @"\" + _baks[path])) {
                        _baks.Remove(path);
                    } else {
                        Utility.ShowMsg("Could not delete '" + _baks[path] + "'. Is the file open somewhere?", "Error");
                    }
                }

                if (requiredBaks.Count == 0) {
                    Utility.ShowMsg("Selected files have been deleted.", "Info");
                } else {
                    Utility.ShowMsg(requiredBaks.Count + " backup(s) were not deleted because the original file(s) are currently patched with a mod. " +
                        "Deleting these backups would make it impossible to unpatch the mod(s).", "Info");
                }
                SaveBackups();
                UpdateBackupList();
                Program.MasterForm.PBar_hide();
            } else {
                Utility.ShowMsg("No items selected from the list.", "Error");
                Program.MasterForm.PBar_hide();
                return;
            }
        }

        public static void DelAllBaksBtn_Click(object sender, EventArgs e) {
            Backups.DeleteAll(false);
        }

        #endregion

        #region UI Functions

        public static bool LoadBackups() {
            EnsureBackupFolderExists();
            if (!File.Exists(Config.BackupCfg)) {   //TODO: create blank config
                return false;
            }

            bool err = false;
            string json = File.ReadAllText(Config.BackupCfg);
            try {
                _baks = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                UpdateBackupList();
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
                    if (!Utility.DeleteFile(Config.BackupCfg)) {
                        Utility.ShowMsg("The backup file could not be deleted. Is it open somewhere?", "Error");
                    }
                }
            }

            return true;
        }

        public static bool UpdateBackupList() {
            EnsureBackupFolderExists();

            Program.MasterForm.bakListPanel.Controls.Clear();
            foreach (KeyValuePair<string, string> entry in _baks) {
                string entryName;
                if (Config.fullBakPath) {
                    entryName = entry.Key;
                } else {
                    entryName = entry.Value;
                }

                Panel container = new Panel {
                    Width = Config.backupPanelWidth,
                    Height = 17,
                    Location = new Point(0, (Program.MasterForm.bakListPanel.Controls.Count * 20) + 1),
                };
                container.MouseEnter += Program.MasterForm.ListPanel_rowHoverOn;
                container.MouseLeave += Program.MasterForm.ListPanel_rowHoverOff;

                CheckBox chb = new CheckBox {
                    AutoSize = true,
                    Text = Config.dirtyPadding + entryName,
                    Location = Config.BackupsChbPoint
                };
                chb.MouseEnter += Program.MasterForm.ListPanel_rowChildHoverOn;
                chb.MouseLeave += Program.MasterForm.ListPanel_rowChildHoverOff;

                container.Controls.Add(chb);
                Program.MasterForm.bakListPanel.Controls.Add(container);
            }
            return true;
        }

        #endregion

        #region Api Functions

        public static int CreateBackup(string path, bool overwrite) {
            EnsureBackupFolderExists();
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);
            string bakPath = Config.Backup_dir + @"\" + fileName + fileExt;

            if (File.Exists(bakPath)) {
                if (!_baks.ContainsKey(path)) { // if file exists in backups folder but the original filepaths differ, rename
                    int num = 0;
                    do {
                        num++;
                        bakPath = Config.Backup_dir + @"\" + fileName + "(" + num + ")" + fileExt;
                    } while (File.Exists(bakPath));
                }
            }

            int res = Utility.CopyFile(path, bakPath, overwrite);
            if (res == 0 || res == 1) {
                _baks[path] = Path.GetFileName(bakPath);
                SaveBackups();
                UpdateBackupList();
            }
            return res;
        }

        public static bool RestoreBak(string filePath) {
            if (String.IsNullOrEmpty(filePath)) {
                return false;
            }

            int ret;
            try {
                ret = Utility.CopyFile(Config.Backup_dir + @"\" + _baks[filePath], filePath, true);
            } catch (KeyNotFoundException) {
                ret = 99;
            }
            if (ret == 0) {
                if (Config.DeleteOldBaks) {
                    if (Utility.DeleteFile(Config.Backup_dir + @"\" + _baks[filePath])) {
                        _baks.Remove(filePath);
                    } else {
                        Utility.ShowMsg("Could not remove old backup '" + _baks[filePath] + "'. Is the file open somewhere?", "Error");
                    }
                }
                return true;
            } else if (ret == 99) {
                Utility.ShowMsg("Could not locate a backup for the file at '" + filePath + "'.", "Error");
                return false;
            } else {
                Utility.ShowMsg("Could not restore '" + _baks[filePath] + "'. If the game is open, close it and try again.", "Error");
                return false;
            }
        }

        public static int RestoreBaks(List<string> backupPathList) {
            EnsureBackupFolderExists();

            if (backupPathList.Count == 0) {
                return 0;
            }

            Program.MasterForm.PBar_show(backupPathList.Count);
            bool chk = false;
            bool err = false;
            foreach (string path in backupPathList) {
                Program.MasterForm.PBar_update();
                if (RestoreBak(path)) {
                    chk = true;
                } else {
                    err = true;
                }
            }
            SaveBackups();
            UpdateBackupList();
            Program.MasterForm.PBar_hide();
            if (chk) {
                if (err) {
                    return 1;   // Partial success - Some files were restored
                }
                return 0;   // Success - All files were restored
            }
            return 2;   // Failure - No files were restored
        }

        public static bool DeleteAll(bool y) {
            EnsureBackupFolderExists();

            if (!y) {
                DialogResult ans = Utility.ShowMsg("Are you sure you want to delete ALL of your backup(s)?\r\nNo crying afterwards?", "Question");
                if (ans == DialogResult.No) {
                    return true;
                }
            }

            bool err = false;
            Program.MasterForm.PBar_show(_baks.Count);
            List<string> toDelete = new List<string>();
            foreach (KeyValuePair<string, string> entry in _baks) {
                toDelete.Add(entry.Key);
            }
            if (toDelete.Count == 0) {
                Utility.ShowMsg("Nothing to delete.", "Info");
                return true;
            }

            List<string> remainingBaks = new List<string>();
            List<string> requiredBaks = FilterNeededBackups(toDelete);
            foreach (string path in toDelete) {
                if (requiredBaks.Contains(path)) {
                    remainingBaks.Add(path);
                    continue;
                }
                if (!Utility.DeleteFile(Config.Backup_dir + @"\" + _baks[path])) {
                    remainingBaks.Add(path);
                    Utility.ShowMsg("Could not delete '" + path + "'. Is the file open somewhere?", "Error");
                    err = true;
                }
            }

            if (remainingBaks.Count == 0) {
                _baks = new Dictionary<string, string>();
                Utility.ShowMsg("All backups deleted.", "Info");
            } else {
                Dictionary<string, string> tmp = new Dictionary<string, string>();
                foreach (string path in remainingBaks) {    // create backup config of files which couldn't be deleted
                    tmp[path] = _baks[path];
                }
                _baks = tmp;
            }
            SaveBackups();
            UpdateBackupList();
            Program.MasterForm.PBar_hide();

            if (requiredBaks.Count != 0) {
                Utility.ShowMsg(requiredBaks.Count + " backups were not deleted because the original file(s) are currently patched with a mod. " +
                        "Deleting these backups would make it impossible to unpatch the mod(s).", "Info");
            }
            return !err;
        }
        public static bool DeleteBak(string path) {
            if (!_baks.ContainsKey(path)) {
                return false;
            }
            if (Utility.DeleteFile(Config.Backup_dir + @"\" + _baks[path])) {
                _baks.Remove(path);
                return true;
            }
            return false;
        }

        public static bool SaveBackups() {
            EnsureBackupFolderExists();

            string json = JsonConvert.SerializeObject(_baks, Formatting.Indented);
            using (FileStream fs = File.Create(Config.BackupCfg)) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
            return true;
        }

        #endregion

        #region Helper Functions

        private static bool EnsureBackupFolderExists() {
            if (!Directory.Exists(Config.Backup_dir)) {
                Directory.CreateDirectory(Config.Backup_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static string GetBakKey(string bakFileName) {
            foreach (KeyValuePair<string, string> entry in _baks) {
                if (entry.Value == bakFileName) {
                    return entry.Key;
                }
            }
            return null;
        }

        private static List<string> FilterNeededBackups(List<string> paths) {
            List<string> requiredBaks = new List<string>();
            foreach (string enabledModpack in Config.GetEnabledModpacks()) {
                ModpackCfg modpackConfig = Modpacks.GetModpackConfig(enabledModpack);

                foreach (ModpackEntry entry in modpackConfig.entries) {
                    foreach (string path in paths) {
                        if (path == Utility.ExpandPath(entry.dest)) {
                            requiredBaks.Add(path);
                        }
                    }
                }
            }

            return requiredBaks;
        }
        #endregion
    }
}
