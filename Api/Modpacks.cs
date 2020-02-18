using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace MCC_Mod_Manager {
    public class modpackEntry {
        public string src;
        public string orig;
        public string dest;
        public string type;
    }
    public class modpackCfg {
        public string MCC_version;
        public List<modpackEntry> entries = new List<modpackEntry>();
    }

    static class Modpacks {

        private static bool EnsureModpackFolderExists() {
            if (!Directory.Exists(Config.Modpack_dir)) {
                Directory.CreateDirectory(Config.Modpack_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static string ExpandPath(string p) {
            return p.Replace("$MCC_home", Config.MCC_home);
        }
        private static string CompressPath(string p) {
            return p.Replace(Config.MCC_home, "$MCC_home");
        }

        public static string getMD5(string filePath) {
            using (FileStream stream = File.OpenRead(filePath)) {
                using (MD5 md5 = MD5.Create()) {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void Create_fileBrowse(object sender, EventArgs e) {
            Button btn = (Button)sender;
            Panel panel = (Panel)((Button)sender).Parent;

            if ((string)btn.Tag == "btn1") {
                OpenFileDialog ofd = new OpenFileDialog {
                    InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"  // using the GUID to access 'This PC' folder
                };
                if (ofd.ShowDialog() == DialogResult.OK) {
                    panel.GetChildAtPoint(Config.sourceTextBoxPoint).Text = ofd.FileName;

                    if (Path.GetExtension(ofd.FileName) == ".asmp") {
                        if ((string)panel.Tag != "alt") {
                            Program.MasterForm.SwapRowType(panel);
                        } else {
                            TextBox orig_txt = (TextBox)panel.GetChildAtPoint(Config.origTextBoxPoint);
                            Button orig_btn = (Button)panel.GetChildAtPoint(Config.origBtnPoint);
                            orig_txt.Enabled = true;
                            orig_txt.Text = "";
                            orig_btn.Enabled = true;
                        }
                    } else {    // if not an .asmp file
                        if ((string)panel.Tag != "normal") {
                            Program.MasterForm.SwapRowType(panel);
                        }
                    }
                }
            } else if ((string)btn.Tag == "btn2") {
                OpenFileDialog ofd = new OpenFileDialog {
                    CheckFileExists = false,    // allow modpack creators to type in a filename for creating new files
                    InitialDirectory = Config.MCC_home
                };
                if (ofd.ShowDialog() == DialogResult.OK) {
                    if ((string)panel.Tag == "normal") {
                        panel.GetChildAtPoint(Config.destTextBoxPoint).Text = ofd.FileName;
                    } else {
                        panel.GetChildAtPoint(Config.destTextBoxPointAlt).Text = ofd.FileName;
                    }
                }
            } else {    // btn3
                OpenFileDialog ofd = new OpenFileDialog {
                    Filter = "Map files (*.map)|*.map",
                    InitialDirectory = Config.MCC_home
                };
                if (ofd.ShowDialog() == DialogResult.OK) {
                    if (Path.GetExtension(ofd.FileName) == ".map") {
                        panel.GetChildAtPoint(Config.origTextBoxPoint).Text = ofd.FileName;
                    } else {
                        Utility.ShowMsg("The file must have a .map extension", "Error");
                    }
                }
            }
        }

        public static Dictionary<string, List<string>> GetFilesToRestore() // used after update is detected to find file changes
        {
            Dictionary<string, List<string>> restoreMapping = new Dictionary<string, List<string>>();
            bool packClobbered = false;
            foreach (KeyValuePair<string, Dictionary<string, string>> modpack in Config.Patched) {
                List<string> potentialRestores = new List<string>();
                foreach (KeyValuePair<string, string> fileEntry in modpack.Value) {
                    if (getMD5(ExpandPath(fileEntry.Key)) == fileEntry.Value) { // if file is still modded after the update
                        potentialRestores.Add(fileEntry.Key);
                    } else {    // if file was changed by the update
                        if (!packClobbered) {   // if part of this pack has not yet been clobbered
                            packClobbered = true;
                        }
                    }
                }
                if (packClobbered) {
                    restoreMapping[modpack.Key] = potentialRestores;
                }
                packClobbered = false;
            }

            return restoreMapping;
        }

        public static bool StabilizeGame()  // used after update is detected to uninstall half clobbered mods
        {
            Dictionary<string, List<string>> restoreMap = GetFilesToRestore();

            foreach (KeyValuePair<string, List<string>> modpack in restoreMap) {
                foreach (KeyValuePair<string, string> entry in Config.Patched[modpack.Key]) {
                    if (modpack.Value.Contains(entry.Key)) {
                        Backups.RestoreBak(ExpandPath(entry.Key));
                    } else {
                        Backups.DeleteBak(ExpandPath(entry.Key));
                    }
                }

                Config.RmPatched(modpack.Key);
            }
            Config.MCC_version = Config.GetCurrentBuild();
            Config.SaveCfg();
            Backups.SaveBackups();

            return true;
        }

        public static bool LoadModpacks() {
            EnsureModpackFolderExists();
            Program.MasterForm.ModListPanel_clear();

            string[] fileEntries = Directory.GetFiles(Config.Modpack_dir);
            foreach (string file in fileEntries) {
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                modpackCfg modpackConfig = GetModpackConfig(modpackName);
                if (modpackConfig != null) {
                    Program.MasterForm.ModListPanel_add(modpackName, modpackConfig.MCC_version == Config.GetCurrentBuild());
                }
            }

            return true;
        }

        public static void ForceModpackState(object sender, EventArgs e) {
            PictureBox p = (PictureBox)sender;
            string modpackname = ((CheckBox)p.Parent.GetChildAtPoint(new Point(p.Location.X + 45, p.Location.Y))).Text.Replace(Config.dirtyPadding, "");

            if (Config.IsPatched(modpackname)) {
                Config.RmPatched(modpackname);
                p.Image = Properties.Resources.redDot_15px;
            } else {
                Config.AddPatched(modpackname);
                p.Image = Properties.Resources.greenDot_15px;
            }

            Config.SaveCfg();
        }

        public static bool VerifyExists(string modpackname) {
            if (File.Exists(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                return true;
            }
            return false;
        }

        public static void CreateModpack(string modpackName, List<Panel> modFilesList) {
            if (modFilesList.Count == 0) {
                Utility.ShowMsg("Please add at least one modded file entry", "Error");
                return;
            }
            if (String.IsNullOrEmpty(modpackName)) {
                Utility.ShowMsg("Please enter a modpack name", "Error");
                return;
            }

            List<String> chk = new List<string>();
            modpackCfg mCfg = new modpackCfg {
                MCC_version = Utility.ReadFirstLine(Config.MCC_home + @"\build_tag.txt")
            };
            foreach (Panel row in modFilesList) {
                string srcText = row.GetChildAtPoint(Config.sourceTextBoxPoint).Text;
                TextBox origTextbox = new TextBox();    // setting this to an empty value to avoid compile error on an impossible CS0165
                string destText;
                if ((string)row.Tag == "normal") {
                    destText = row.GetChildAtPoint(Config.destTextBoxPoint).Text;
                } else {
                    origTextbox = (TextBox)row.GetChildAtPoint(Config.origTextBoxPoint);
                    destText = row.GetChildAtPoint(Config.destTextBoxPointAlt).Text;
                }

                if (string.IsNullOrEmpty(srcText) || string.IsNullOrEmpty(destText) || ((string)row.Tag == "alt" && string.IsNullOrEmpty(origTextbox.Text))) {
                    Utility.ShowMsg("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(srcText)) {
                    Utility.ShowMsg("The source file '" + srcText + "' does not exist.", "Error");
                    return;
                }
                if (!destText.StartsWith(Config.MCC_home)) {
                    Utility.ShowMsg("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }
                if ((string)row.Tag == "alt" && origTextbox.Enabled && !origTextbox.Text.StartsWith(Config.MCC_home)) {
                    Utility.ShowMsg("Unmodified map files must be selected at their default install location within the MCC install directory to allow the patch " +
                        "to be correctly applied when this modpack is installed. The file you selected does not appear to lie inside the MCC install directory." +
                        "\r\nYou may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }
                string patchType;
                if (Path.GetExtension(srcText) == ".asmp") {
                    patchType = "patch";
                } else {
                    bool isOriginalFile;
                    try {
                        isOriginalFile = Utility.IsHaloFile(CompressPath(destText));
                    } catch (JsonReaderException) {
                        Utility.ShowMsg(@"MCC Mod Manager could not parse Formats\filetree.json", "Error");
                        return;
                    }

                    if (isOriginalFile) {
                        patchType = "replace";
                    } else {
                        patchType = "create";
                    }
                }

                mCfg.entries.Add(new modpackEntry {
                    src = srcText,
                    orig = (patchType == "patch") ? CompressPath(origTextbox.Text) : null,
                    dest = CompressPath(destText),  // make modpack compatable with any MCC_home directory
                    type = patchType
                });
                chk.Add(destText);
            }

            if (chk.Distinct().Count() != chk.Count) {
                Utility.ShowMsg("You have multiple files trying to write to the same destination.", "Error");
                return;
            }

            EnsureModpackFolderExists();
            String modpackFilename = modpackName + ".zip";
            String zipPath = Config.Modpack_dir + @"\" + modpackFilename;
            if (File.Exists(zipPath)) {
                Utility.ShowMsg("A modpack with that name already exists.", "Error");
                return;
            }

            Program.MasterForm.PBar_show(mCfg.entries.Count);
            try {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    foreach (var entry in mCfg.entries) {
                        Program.MasterForm.PBar_update();
                        String fileName = Path.GetFileName(entry.src);
                        archive.CreateEntryFromFile(entry.src, fileName);    // TODO: Fix issues when two source files have same name but diff path
                        entry.src = fileName;   // change src path to just modpack after archive creation but before json serialization
                    }
                    ZipArchiveEntry configFile = archive.CreateEntry("modpack_config.cfg");
                    string json = JsonConvert.SerializeObject(mCfg, Formatting.Indented);
                    using (StreamWriter writer = new StreamWriter(configFile.Open())) {
                        writer.WriteLine(json);
                    }
                    ZipArchiveEntry readmeFile = archive.CreateEntry("README.txt");
                    using (StreamWriter writer = new StreamWriter(readmeFile.Open())) {
                        writer.WriteLine("Install using MCC Mod Manager: https://github.com/executionByFork/MCC_Mod_Manager/tree/master");
                    }
                }
            } catch (NotSupportedException) {
                Utility.ShowMsg("The modpack name you have provided is not a valid filename on Windows.", "Error");
                return;
            }

            Utility.ShowMsg("Modpack '" + modpackFilename + "' created.", "Info");
            Program.MasterForm.PBar_hide();
            Program.MasterForm.resetCreateModpacksTab();
            LoadModpacks();
            return;
        }

        public static void DelModpack(IEnumerable<CheckBox> modpacksList) {
            bool chk = false;
            bool del = false;
            bool partial = false;
            Program.MasterForm.PBar_show(modpacksList.Count());
            foreach (CheckBox chb in modpacksList) {
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
                            if (UnpatchModpack(modpackname) == 2) {
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

        public static modpackCfg GetModpackConfig(string modpackName) {
            try {
                using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackName + ".zip")) {
                    ZipArchiveEntry modpackConfigEntry = archive.GetEntry("modpack_config.cfg");
                    if (modpackConfigEntry == null) {
                        return null;
                    }
                    modpackCfg modpackConfig;
                    using (Stream jsonStream = modpackConfigEntry.Open()) {
                        StreamReader reader = new StreamReader(jsonStream);
                        try {
                            modpackConfig = JsonConvert.DeserializeObject<modpackCfg>(reader.ReadToEnd());
                        } catch (JsonSerializationException) {
                            return null;
                        } catch (JsonReaderException) {
                            return null;
                        }
                    }

                    return modpackConfig;
                }
            } catch (InvalidDataException) {
                return null;
            }
        }

        private static string WillOverwriteOtherMod(string modpack) {
            modpackCfg primaryConfig = GetModpackConfig(modpack);

            foreach (string enabledModpack in Config.GetEnabledModpacks()) {
                modpackCfg modpackConfig = GetModpackConfig(enabledModpack);

                // Deliberately not checking for null so program throws a stack trace
                // This should never happen, but if it does I want the user to let me know about it
                foreach (modpackEntry entry in modpackConfig.entries) {
                    foreach (modpackEntry primary in primaryConfig.entries) {
                        if (entry.dest == primary.dest) {
                            return enabledModpack;
                        }
                    }
                }
            }

            return null;
        }

        private static int PatchFile(ZipArchive archive, modpackEntry entry) {
            string destination = ExpandPath(entry.dest);
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

                string unmoddedPath = ExpandPath(entry.orig);
                if (!Utility.GetUnmodifiedHash(entry.orig).Equals(getMD5(unmoddedPath), StringComparison.OrdinalIgnoreCase)) {
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

        private static int PatchModpack(string modpackname) {
            string retStr = WillOverwriteOtherMod(modpackname);
            if (!String.IsNullOrEmpty(retStr)) {
                Utility.ShowMsg("Installing '" + modpackname + "' would overwrite files for '" + retStr + "'. Modpack will be skipped.", "Error");
                return 2;
            }

            bool baksMade = false;
            try {
                modpackCfg modpackConfig = GetModpackConfig(modpackname);
                using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                    if (modpackConfig == null) {
                        Utility.ShowMsg("The file '" + modpackname + ".zip' is either not a compatible modpack or the config is corrupted." +
                            "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                        return 2;
                    }

                    List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                    foreach (modpackEntry entry in modpackConfig.entries) {
                        int r = PatchFile(archive, entry);
                        if (r != 0 && r != 1) {
                            string errMsg;
                            if (r == 2) {
                                errMsg = "File Access Exception. If the game is running, exit it and try again.";
                            } else if (r == 3) {
                                errMsg = "This modpack appears to be missing files.";
                            } else {    // r == 4
                                errMsg = "Unknown modfile type in modpack config.";
                            }
                            Utility.ShowMsg(errMsg + "\r\nCould not install the '" + modpackname + "' modpack.", "Error");

                            if (Backups.RestoreBaks(patched) != 0) {
                                Utility.ShowMsg("At least one file restore failed. Your game is likely in an unstable state.", "Warning");
                            }
                            return 2;
                        } else if (r == 1) {
                            baksMade = true;
                        }

                        patched.Add(ExpandPath(entry.dest));
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
                } else {    // modpack was patched
                    if (ret == 1) {
                        baksMade = true;
                    }
                    Config.AddPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(new Point(chb.Location.X - 45, chb.Location.Y))).Image = Properties.Resources.greenDot_15px;
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

        private static int UnpatchModpack(string modpackname) {
            try {
                modpackCfg modpackConfig = GetModpackConfig(modpackname);
                using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                    if (modpackConfig == null) {
                        Utility.ShowMsg("Could not unpatch '" + modpackname + "' because the modpack's configuration is corrupted or missing." +
                            "\r\nPlease restore from backups using the Backups tab or verify integrity of game files on Steam.", "Error");
                        return 2;
                    }
                    List<modpackEntry> restored = new List<modpackEntry>(); // track restored files in case of failure mid unpatch
                    foreach (modpackEntry entry in modpackConfig.entries) {
                        if (String.IsNullOrEmpty(entry.type) || entry.type == "replace" || entry.type == "patch") { // assume replace type entry if null
                            if (!Backups.RestoreBak(ExpandPath(entry.dest))) {
                                // repatch restored mod files
                                foreach (modpackEntry e in restored) {
                                    int r = PatchFile(archive, e);
                                    if (r == 2 || r == 3) {
                                        Utility.ShowMsg("Critical error encountered while unpatching '" + modpackname + "'." +
                                            "\r\nYou may need to verify your game files on steam or reinstall.", "Error");
                                    }
                                }
                                return 2;
                            }
                        } else if (entry.type == "create") {
                            if (!Utility.DeleteFile(ExpandPath(entry.dest))) {
                                Utility.ShowMsg("Could not delete the file '" + ExpandPath(entry.dest) + "'. This may affect your game. " +
                                    "if you encounter issue please delete this file manually.", "Warning");
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

        private static int UnpatchModpacks(List<CheckBox> toUnpatch) {
            bool packErr = false;
            Program.MasterForm.PBar_show(toUnpatch.Count);
            foreach (CheckBox chb in toUnpatch) {
                Program.MasterForm.PBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = UnpatchModpack(modpackname);
                if (ret == 2) {
                    packErr = true;
                    chb.Checked = true;
                } else {    // modpack was unpatched
                    Config.RmPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(new Point(chb.Location.X - 45, chb.Location.Y))).Image = Properties.Resources.redDot_15px;
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

        public static void RunPatchUnpatch(IEnumerable<CheckBox> modpacksList) {
            List<CheckBox> toPatch = new List<CheckBox>();
            List<CheckBox> toUnpatch = new List<CheckBox>();
            foreach (CheckBox chb in modpacksList) {
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (chb.Checked && !Config.IsPatched(modpackname)) {
                    toPatch.Add(chb);
                } else if (!chb.Checked && Config.IsPatched(modpackname)) {
                    toUnpatch.Add(chb);
                }
            }

            if (toPatch.Count == 0 && toUnpatch.Count == 0) {
                Utility.ShowMsg("You did not select any changes. No modpacks were patched or unpatched.", "Info");
                return;
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
    }
}
