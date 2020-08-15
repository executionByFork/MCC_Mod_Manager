using MCC_Mod_Manager.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace MCC_Mod_Manager.Api {
    class MyMods {

        #region Event Handlers

        public static void SelectEnabled_chb_CheckedChanged(object sender, EventArgs e) {
            ApplyBulkChecks(((CheckBox)sender).Checked, false, true);
        }

        public static void SelectDisabled_chb_CheckedChanged(object sender, EventArgs e) {
            ApplyBulkChecks(((CheckBox)sender).Checked, false, false);
        }

        public static void SelectAll_chb_CheckedChanged(object sender, EventArgs e) {
            Program.MasterForm.selectEnabled_chb.Checked = ((CheckBox)sender).Checked;
            Program.MasterForm.selectDisabled_chb.Checked = ((CheckBox)sender).Checked;

            ApplyBulkChecks(((CheckBox)sender).Checked, true);
        }

        public static void ManualOverride_CheckedChanged(object sender, EventArgs e) {
            if (Program.MasterForm.manualOverride.Checked == false) {   // make warning only show if checkbox is getting enabled
                LoadModpacks();
                return;
            } else {
                DialogResult ans = Utility.ShowMsg("Please do not mess with this unless you know what you are doing or are trying to fix a syncing issue.\r\n\r\n" +
                    "This option allows you to click the red/green icons beside modpack entries to force the mod manager to flag a modpack as enabled/disabled. " +
                    "This does not make changes to files, but it does make the mod manager 'think' that modpacks are/aren't installed." +
                    "\r\n\r\nEnable this feature?", "Question");
                if (ans == DialogResult.No) {
                    Program.MasterForm.manualOverride.Checked = false;
                    return;
                }

                LoadModpacks();
            }
        }

        public static void PatchUnpatch_Click(object sender, EventArgs e) {
            IEnumerable<Panel> modpacksList = Program.MasterForm.modListPanel.Controls.OfType<Panel>();
            List<CheckBox> toPatch = new List<CheckBox>();
            List<CheckBox> toUnpatch = new List<CheckBox>();
            List<string> oldModpacks = new List<string>();
            foreach (Panel p in modpacksList) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.MyModsChbPoint);
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (chb.Checked && !Config.IsPatched(modpackname)) {
                    toPatch.Add(chb);
                    if ((string)p.Tag != Config.GetCurrentBuild()) {
                        oldModpacks.Add(modpackname);
                    }
                }
                if (!chb.Checked && Config.IsPatched(modpackname)) {
                    toUnpatch.Add(chb);
                }
            }

            if (!toPatch.Any() && !toUnpatch.Any()) {
                Utility.ShowMsg("You did not select any changes. No modpacks were patched or unpatched.", "Info");
                return;
            }
            if (oldModpacks.Any()) {
                string list = "";
                foreach (string name in oldModpacks) {
                    if (!String.IsNullOrEmpty(list)) {
                        list += ", ";
                    }
                    list += name;
                }

                DialogResult ans = Utility.ShowMsg("The following modpacks were made for an older version of the game and may cause issues:\r\n" + list +
                    "\r\nContinue with patch?", "Question");
                if (ans == DialogResult.No) {
                    Program.MasterForm.PBar_hide();
                    return;
                }
            }

            // Unpatch mods before trying to patch new ones
            int retU = UnpatchModpacks(toUnpatch);
            int retP = PatchModpacks(toPatch);

            Config.SaveCfg();

            if (retU == 2 || retP == 2) {   // fail / partial success - At least one modpack was not patched
                Utility.ShowMsg("Failed in patching/unpatching at least one modpack.", "Warning");
            } else if (retP == 1) {  // success and new backup(s) created
                Utility.ShowMsg("The game has been updated with your modpack selection.\r\nNew backups were created.", "Info");
            } else {    // success, no new backups (retU == 0 && retP == 0)
                Utility.ShowMsg("The game has been updated with your changes.", "Info");
            }
        }

        public static void DeleteSelected_Click(object sender, EventArgs e) {
            IEnumerable<Panel> modpacksList = Program.MasterForm.modListPanel.Controls.OfType<Panel>();
            bool chk = false;
            bool del = false;
            bool partial = false;
            Program.MasterForm.PBar_show(modpacksList.Count());
            foreach (Panel p in modpacksList) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.MyModsChbPoint);
                Program.MasterForm.PBar_update();    //TODO: This updates on EVERY modpack which isn't quite accurate
                if (chb.Checked) {
                    if (!chk) { // only prompt user once
                        DialogResult ans = Utility.ShowMsg("Are you sure you want to delete the selected modpacks(s)?\r\nNo crying afterwards?", "Question");
                        if (ans == DialogResult.No) {
                            Program.MasterForm.PBar_hide();
                            return;
                        }
                    }
                    chk = true;

                    string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                    if (Config.IsPatched(modpackname)) {    // deliberately prompt for each modpack that is enabled
                        DialogResult ans = Utility.ShowMsg("WARNING: The " + modpackname + " modpack is showing as currently installed. " +
                            "Deleting this modpack will also unpatch it from the game. Continue?", "Question");
                        if (ans == DialogResult.No) {
                            partial = true;
                            continue;
                        } else {
                            if (MyMods.UnpatchModpack(modpackname) == 2) {
                                partial = true;
                                continue;
                            }
                            Config.RmPatched(modpackname);
                        }
                    }

                    if (!Utility.DeleteFile(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                        Utility.ShowMsg("Could not delete '" + modpackname + ".zip'. Is the zip file open somewhere?", "Error");
                    }
                    del = true;
                    chb.Checked = false;
                }
            }
            if (!chk) {
                Utility.ShowMsg("No items selected from the list.", "Error");
            } else if (!del) {
                Utility.ShowMsg("No modpacks were deleted.", "Warning");
            } else if (del && partial) {
                Utility.ShowMsg("Only some of the selected modpacks have been deleted.", "Warning");
                LoadModpacks();
            } else {
                Utility.ShowMsg("Selected modpacks have been deleted.", "Info");
                LoadModpacks();
            }
            Config.SaveCfg();
            Program.MasterForm.PBar_hide();
        }
        #endregion

        #region UI Functions

        public static bool LoadModpacks() {
            Modpacks.EnsureModpackFolderExists();
            Program.MasterForm.modListPanel.Controls.Clear();

            string[] fileEntries = Directory.GetFiles(Config.Modpack_dir);
            foreach (string file in fileEntries) {
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                ModpackCfg modpackConfig = Modpacks.GetModpackConfig(modpackName);
                if (modpackConfig != null) {
                    ModListPanel_add(modpackName, modpackConfig.MCC_version);
                }
            }

            return true;
        }

        public static void ModListPanel_add(string modpackName, string modpackGameVersion) {
            var modListCount = Program.MasterForm.modListPanel.Controls.OfType<Panel>().Count();
            Panel container = new Panel {
                Width = 400,
                Height = 17,
                Location = new Point(0, (modListCount * 20) + 1),
                Tag = modpackGameVersion    //tag the game version this modpack was made for, to make it easier to do prelim patch checks
            };
            
            CheckBox chb = new CheckBox {
                AutoSize = true,
                Text = Config.dirtyPadding + modpackName,
                Location = Config.MyModsChbPoint,
                Checked = Config.IsPatched(modpackName)
            };
            PictureBox p = new PictureBox {
                Width = 15,
                Height = 15,
                Location = Config.MyModsEnabledPoint,
                Image = Config.IsPatched(modpackName) ? Properties.Resources.greenDot_15px : Properties.Resources.redDot_15px
            };
            PictureBox c = new PictureBox {
                Width = 15,
                Height = 15,
                Location = Config.MyModsCautionPoint,
                Image = Properties.Resources.caution_15px,
                Visible = false
            };
            if (Program.MasterForm.manualOverride.Checked) {
                p.Click += MyMods.ForceModpackState;
                p.MouseEnter += Program.MasterForm.BtnHoverOn;
                p.MouseLeave += Program.MasterForm.BtnHoverOff;
            }

            Program.MasterForm.modListPanel.Controls.Add(container);
            container.Controls.Add(p);
            container.Controls.Add(c);
            container.Controls.Add(chb);
        }

        public static void ForceModpackState(object sender, EventArgs e) {
            Program.MasterForm.Cursor = Cursors.WaitCursor;
            PictureBox p = (PictureBox)sender;
            string modpackname = ((CheckBox)p.Parent.GetChildAtPoint(Config.MyModsChbPoint)).Text.Replace(Config.dirtyPadding, "");

            if (Config.IsPatched(modpackname)) {
                Config.RmPatched(modpackname);
                p.Image = Properties.Resources.redDot_15px;
            } else {
                Config.AddPatched(modpackname);
                p.Image = Properties.Resources.greenDot_15px;
            }

            Config.SaveCfg();

            Program.MasterForm.Cursor = Cursors.Default;
        }

        #endregion

        #region Api Functions

        #region Patch
        private static int PatchModpacks(List<CheckBox> toPatch) {
            bool baksMade = false;
            bool packErr = false;
            Program.MasterForm.PBar_show(toPatch.Count);
            foreach (CheckBox chb in toPatch) {
                Program.MasterForm.PBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = PatchModpack(modpackname);
                if (ret == 2) {
                    packErr = true;
                    chb.Checked = false;
                } else if (ret == 3) {
                    setModpackStatePartial((Panel)chb.Parent);
                } else {    // modpack was patched
                    if (ret == 1) {
                        baksMade = true;
                    }
                    Config.AddPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(Config.MyModsEnabledPoint)).Image = Properties.Resources.greenDot_15px;
                }
            }

            Program.MasterForm.PBar_hide();
            if (packErr) {   // fail / partial success - At least one modpack was not patched
                return 2;
            } else if (baksMade) {  // success and new backup(s) created
                return 1;
            } else {
                return 0;
            }
        }

        private static int PatchModpack(string modpackname) {
            string retStr = WillOverwriteOtherMod(modpackname);
            if (!String.IsNullOrEmpty(retStr)) {
                Utility.ShowMsg("Installing '" + modpackname + "' would overwrite files for '" + retStr + "'. Modpack will be skipped.", "Error");
                return 2;
            }

            bool baksMade = false;
            try {
                ModpackCfg modpackConfig = Modpacks.GetModpackConfig(modpackname);
                using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                    if (modpackConfig == null) {
                        Utility.ShowMsg("The file '" + modpackname + ".zip' is either not a compatible modpack or the config is corrupted." +
                            "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                        return 2;
                    }

                    List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                    foreach (ModpackEntry entry in modpackConfig.entries) {
                        int r = PatchFile(archive, entry);
                        if (r != 0 && r != 1) {
                            string errMsg;
                            if (r == 2) {
                                errMsg = "File Access Exception.\n" +
                                    "If you're using the MCStore version of the game, please run this tool as an administrator.\n" +
                                    "If the game is running, exit it and try again.";
                            } else if (r == 3) {
                                errMsg = "This modpack appears to be missing files.";
                            } else {    // r == 4
                                errMsg = "Unknown modfile type in modpack config.";
                            }
                            Utility.ShowMsg(errMsg + "\r\nCould not install the '" + modpackname + "' modpack.", "Error");

                            if (Backups.RestoreBaks(patched) != 0) {
                                Utility.ShowMsg("At least one file restore failed. Your game is likely in an unstable state.", "Warning");
                                return 3;
                            }
                            return 2;
                        } else if (r == 1) {
                            baksMade = true;
                        }

                        patched.Add(Utility.ExpandPath(entry.dest));
                    }
                }
            } catch (FileNotFoundException) {
                Utility.ShowMsg("Could not find the '" + modpackname + "' modpack.", "Error");
                return 2;
            } catch (InvalidDataException) {
                Utility.ShowMsg("The modpack '" + modpackname + "' appears corrupted." +
                "\r\nThis modpack cannot be installed.", "Error");
                return 2;
            }

            if (baksMade) {
                return 1;
            } else {
                return 0;
            }
        }

        private static int PatchFile(ZipArchive archive, ModpackEntry entry) {
            string destination = Utility.ExpandPath(entry.dest);
            bool baksMade = false;
            ZipArchiveEntry modFile = archive.GetEntry(entry.src);
            if (modFile == null) {
                return 3;
            }
            if (String.IsNullOrEmpty(entry.type) || entry.type == "replace") {  // assume replace type entry
                if (File.Exists(destination)) {
                    if (Backups.CreateBackup(destination, false) == 0) {
                        baksMade = true;
                    }
                    if (!Utility.DeleteFile(destination)) {
                        return 2;
                    }
                }
                try {
                    modFile.ExtractToFile(destination);
                } catch (IOException) {
                    return 2;
                }

                if (baksMade) {
                    return 1;
                } else {
                    return 0;
                }
            } else if (entry.type == "patch") {
                if (File.Exists(destination)) {
                    if (Backups.CreateBackup(destination, false) == 0) {
                        baksMade = true;
                    }
                    if (!Utility.DeleteFile(destination)) {
                        return 2;
                    }
                }

                string unmoddedPath = Utility.ExpandPath(entry.orig);
                if (!Utility.GetUnmodifiedHash(entry.orig).Equals(Modpacks.GetMD5(unmoddedPath), StringComparison.OrdinalIgnoreCase)) {
                    unmoddedPath = Config.Backup_dir + @"\" + Backups._baks[unmoddedPath];  // use backup version
                }

                if (!AssemblyPatching.ApplyPatch(modFile, Path.GetFileName(entry.src), unmoddedPath, destination)) {
                    return 5;   // no extra error message
                }

                if (baksMade) {
                    return 1;
                } else {
                    return 0;
                }
            } else if (entry.type == "create") {
                if (File.Exists(destination)) {
                    if (!Utility.DeleteFile(destination)) {
                        return 2;
                    }
                }
                try {
                    modFile.ExtractToFile(destination);
                } catch (IOException) {
                    return 2;
                }

                return 0;
            } else {
                return 4;
            }
        }

        #endregion

        #region Unpatch

        public static int UnpatchModpacks(List<CheckBox> toUnpatch) {
            bool packErr = false;
            Program.MasterForm.PBar_show(toUnpatch.Count);
            foreach (CheckBox chb in toUnpatch) {
                Program.MasterForm.PBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = UnpatchModpack(modpackname);
                if (ret == 2) {
                    packErr = true;
                    chb.Checked = true;
                } else if (ret == 3) {
                    setModpackStatePartial((Panel)chb.Parent);
                } else {    // modpack was unpatched
                    Config.RmPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(Config.MyModsEnabledPoint)).Image = Properties.Resources.redDot_15px;
                }
            }

            if (Config.DeleteOldBaks) { // update backup pane because backups will have been deleted
                Backups.SaveBackups();
                Backups.UpdateBackupList();
            }

            Program.MasterForm.PBar_hide();
            if (packErr) { // fail / partial success - At least one modpack was not patched
                return 2;
            } else {    // success, no errors
                return 0;
            }
        }

        public static int UnpatchModpack(string modpackname) {
            try {
                ModpackCfg modpackConfig = Modpacks.GetModpackConfig(modpackname);
                using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                    if (modpackConfig == null) {
                        Utility.ShowMsg("Could not unpatch '" + modpackname + "' because the modpack's configuration is corrupted or missing." +
                            "\r\nPlease restore from backups using the Backups tab or verify integrity of game files on Steam.", "Error");
                        return 2;
                    }
                    List<ModpackEntry> restored = new List<ModpackEntry>(); // track restored files in case of failure mid unpatch
                    foreach (ModpackEntry entry in modpackConfig.entries) {
                        if (String.IsNullOrEmpty(entry.type) || entry.type == "replace" || entry.type == "patch") { // assume replace type entry if null
                            if (!Backups.RestoreBak(Utility.ExpandPath(entry.dest))) {
                                // repatch restored mod files
                                bool err = false;
                                foreach (ModpackEntry e in restored) {
                                    int r = PatchFile(archive, e);
                                    if (r == 2 || r == 3) {
                                        err = true;
                                    }
                                }
                                if (err) {
                                    Utility.ShowMsg("Critical error encountered while unpatching '" + modpackname + "'." +
                                        "\r\nYou may need to verify your game files on steam or reinstall.", "Error");
                                }
                                return 3;
                            }
                        } else if (entry.type == "create") {
                            if (!Utility.DeleteFile(Utility.ExpandPath(entry.dest))) {
                                Utility.ShowMsg("Could not delete the file '" + Utility.ExpandPath(entry.dest) + "'. This may affect your game. " +
                                    "if you encounter issues please delete this file manually.", "Warning");
                            }
                        } else {
                            Utility.ShowMsg("Unknown modfile type in modpack config.\r\nCould not install the '" + modpackname + "' modpack.", "Error");
                        }
                        restored.Add(entry);
                    }
                }
            } catch (FileNotFoundException) {
                Utility.ShowMsg("Could not unpatch '" + modpackname + "'. Could not find the modpack file to read the config from.", "Error");
                return 2;
            } catch (InvalidDataException) {
                Utility.ShowMsg("Could not unpatch '" + modpackname + "'. The modpack file appears corrupted and the config cannot be read.", "Error");
                return 2;
            }

            return 0;
        }

        #endregion

        public static void setModpacksDisabled(List<string> modpackList) {
            foreach (Panel p in Program.MasterForm.modListPanel.Controls.OfType<Panel>()) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.MyModsChbPoint);
                string modpackName = chb.Text.Replace(Config.dirtyPadding, "");
                if (modpackList.Contains(modpackName)) {
                    ((PictureBox)chb.Parent.GetChildAtPoint(Config.MyModsEnabledPoint)).Image = Properties.Resources.redDot_15px;
                    modpackList.Remove(modpackName);
                    Config.RmPatched(modpackName);
                    if (!modpackList.Any()) {
                        break;
                    }
                }
            }
            Config.SaveCfg();
        }

        public static void setModpacksPartial(List<string> modpackList) {
            foreach (Panel p in Program.MasterForm.modListPanel.Controls.OfType<Panel>()) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.MyModsChbPoint);
                string modpackName = chb.Text.Replace(Config.dirtyPadding, "");
                if (modpackList.Contains(modpackName)) {
                    setModpackStatePartial(p);
                    modpackList.Remove(modpackName);
                    if (!modpackList.Any()) {
                        break;
                    }
                }
            }
            Config.SaveCfg();
        }

        public static void setModpackStatePartial(Panel p) {
            CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.MyModsChbPoint);
            Config.Patched[chb.Text.Replace(Config.dirtyPadding, "")].error = true;

            foreach (PictureBox cautionSign in p.Controls.OfType<PictureBox>()) {
                if (cautionSign.Bounds.Contains(new Point(chb.Location.X - 23, chb.Location.Y))) {
                    cautionSign.Visible = true;
                    Program.MasterForm.tt.SetToolTip(cautionSign, "This modpack is only partially installed and will " +
                        "likely cause issues. Please try unpatching this modpack.");
                    break;
                }
            }
        }
        #endregion

        #region Helper Functions

        private static string WillOverwriteOtherMod(string modpack) {
            ModpackCfg primaryConfig = Modpacks.GetModpackConfig(modpack);

            foreach (string enabledModpack in Config.GetEnabledModpacks()) {
                ModpackCfg modpackConfig = Modpacks.GetModpackConfig(enabledModpack);

                // Deliberately not checking for null so program throws a stack trace
                // This should never happen, but if it does I want the user to let me know about it
                foreach (ModpackEntry entry in modpackConfig.entries) {
                    foreach (ModpackEntry primary in primaryConfig.entries) {
                        if (entry.dest == primary.dest) {
                            return enabledModpack;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Generic function that applies checks to rows.
        /// If patched, masterBoxvalue = value of Select Enabled chb. If not patched, masterBoxValue = Select Disabled chb.
        /// </summary>
        /// <param name="masterBoxValue"></param>
        /// <param name="isSelectPatched"></param>
        public static void ApplyBulkChecks(bool parentChbValue, bool isSelectAll, bool isPatchedChb = false) {
            foreach (Panel p in Program.MasterForm.modListPanel.Controls.OfType<Panel>()) {
                CheckBox chb = (CheckBox)p.GetChildAtPoint(Config.MyModsChbPoint);
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (isSelectAll || (isPatchedChb && Config.IsPatched(modpackname))) {
                    chb.Checked = parentChbValue;
                } else if (isSelectAll || (!isPatchedChb && !Config.IsPatched(modpackname))) {
                    chb.Checked = parentChbValue;
                }
            }
        }

        #endregion
    }
}
