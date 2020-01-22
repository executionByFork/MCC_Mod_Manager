using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;

namespace MCC_Mod_Manager
{
    static class Modpacks
    {
        public static Form1 form1;  // this is set on form load

        private static bool ensureModpackFolderExists()
        {
            if (!Directory.Exists(Config.modpack_dir)) {
                Directory.CreateDirectory(Config.modpack_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        private static string expandPath(string p)
        {
            return p.Replace("$MCC_home", Config.MCC_home);
        }
        private static string compressPath(string p)
        {
            return p.Replace(Config.MCC_home, "$MCC_home");
        }

        public static void create_fileBrowse1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"  // using the GUID to access 'This PC' folder
            };
            if (ofd.ShowDialog() == DialogResult.OK) {
                ((Button)sender).Parent.GetChildAtPoint(Config.sourceTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static void create_fileBrowse2(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {
                CheckFileExists = false,    // allow modpack creators to type in a filename for creating new files
                InitialDirectory = Config.MCC_home
            };
            if (ofd.ShowDialog() == DialogResult.OK) {
                ((Button)sender).Parent.GetChildAtPoint(Config.destTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static bool loadModpacks()
        {
            ensureModpackFolderExists();
            form1.modListPanel_clear();

            string[] fileEntries = Directory.GetFiles(Config.modpack_dir);
            foreach (string file in fileEntries) {
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                CheckBox chb = new CheckBox {
                    AutoSize = true,
                    Text = Config.dirtyPadding + modpackName,
                    Location = new Point(60, (form1.modListPanel_getCount() * 20) + 1),
                    Checked = Config.isPatched(modpackName)
                };
                PictureBox p = new PictureBox {
                    Width = 15,
                    Height = 15,
                    Location = new Point(15, (form1.modListPanel_getCount() * 20) + 1),
                    Image = Config.isPatched(modpackName) ? Properties.Resources.greenDot_15px : Properties.Resources.redDot_15px
                };
                if (form1.manualOverrideEnabled()) {
                    p.Click += forceModpackState;
                    p.MouseEnter += form1.btnHoverOn;
                    p.MouseLeave += form1.btnHoverOff;
                }

                form1.modListPanel_add(p, chb);
            }

            return true;
        }

        private static void forceModpackState(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            string modpackname = ((CheckBox)p.Parent.GetChildAtPoint(new Point(p.Location.X + 45, p.Location.Y))).Text.Replace(Config.dirtyPadding, "");

            if (Config.isPatched(modpackname)) {
                Config.rmPatched(modpackname);
                p.Image = Properties.Resources.redDot_15px;
            } else {
                Config.addPatched(modpackname);
                p.Image = Properties.Resources.greenDot_15px;
            }

            Config.saveCfg();
        }

        public static bool verifyExists(string modpackname)
        {
            if (File.Exists(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                return true;
            }
            return false;
        }

        public static void createModpack(string modpackName, IEnumerable<Panel> modFilesList)
        {
            if (modFilesList.Count() == 0) {
                form1.showMsg("Please add at least one modded file entry", "Error");
                return;
            }
            if (String.IsNullOrEmpty(modpackName)) {
                form1.showMsg("Please enter a modpack name", "Error");
                return;
            }

            List<String> chk = new List<string>();
            List<Dictionary<string, string>> fileMap = new List<Dictionary<string, string>>();
            foreach (Panel row in modFilesList) {
                Dictionary<string, string> dict = new Dictionary<string, string> {
                    ["src"] = row.GetChildAtPoint(Config.sourceTextBoxPoint).Text,
                    ["dest"] = row.GetChildAtPoint(Config.destTextBoxPoint).Text
                };
                if (string.IsNullOrEmpty(dict["src"]) || string.IsNullOrEmpty(dict["dest"])) {
                    form1.showMsg("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(dict["src"])) {
                    form1.showMsg("The source file '" + dict["src"] + "' does not exist.", "Error");
                    return;
                }
                if (!dict["dest"].StartsWith(Config.MCC_home)) {
                    form1.showMsg("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }
                if (Path.GetExtension(dict["src"]) == ".asmp") {
                    dict["type"] = "patch";
                } else if (File.Exists(dict["dest"])) {
                    dict["type"] = "replace";
                } else {
                    dict["type"] = "create";
                }

                // make modpack compatable with any MCC_home directory
                dict["dest"] = compressPath(dict["dest"]);

                fileMap.Add(dict);
                chk.Add(row.GetChildAtPoint(Config.destTextBoxPoint).Text);
            }

            if (chk.Distinct().Count() != chk.Count()) {
                form1.showMsg("You have multiple files trying to write to the same destination.", "Error");
                return;
            }

            ensureModpackFolderExists();
            String modpackFilename = modpackName + ".zip";
            String zipPath = Config.modpack_dir + @"\" + modpackFilename;
            if (File.Exists(zipPath)) {
                form1.showMsg("A modpack with that name already exists.", "Error");
                return;
            }

            form1.pBar_show(fileMap.Count());
            try {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    foreach (var entry in fileMap) {
                        form1.pBar_update();
                        String fileName = Path.GetFileName(entry["src"]);
                        archive.CreateEntryFromFile(entry["src"], fileName);    // TODO: Fix issues when two source files have same name but diff path
                                                                                // change src path to just modpack after archive creation but before json serialization
                        entry["src"] = fileName;
                    }
                    ZipArchiveEntry configFile = archive.CreateEntry("modpack_config.cfg");
                    string json = JsonConvert.SerializeObject(fileMap, Formatting.Indented);
                    using (StreamWriter writer = new StreamWriter(configFile.Open())) {
                        writer.WriteLine(json);
                    }
                    ZipArchiveEntry readmeFile = archive.CreateEntry("README.txt");
                    using (StreamWriter writer = new StreamWriter(readmeFile.Open())) {
                        writer.WriteLine("Install using MCC Mod Manager: https://github.com/executionByFork/MCC_Mod_Manager/tree/master");
                    }
                }
            } catch (NotSupportedException) {
                form1.showMsg("The modpack name you have provided is not a valid filename on Windows.", "Error");
                return;
            }

            form1.showMsg("Modpack '" + modpackFilename + "' created.", "Info");
            form1.pBar_hide();
            form1.resetCreateModpacksTab();
            loadModpacks();
            return;
        }

        public static void delModpack(IEnumerable<CheckBox> modpacksList)
        {
            bool chk = false;
            bool del = false;
            bool partial = false;
            form1.pBar_show(modpacksList.Count());
            foreach (CheckBox chb in modpacksList) {
                form1.pBar_update();
                if (chb.Checked) {
                    if (!chk) { // only prompt user once
                        DialogResult ans = form1.showMsg("Are you sure you want to delete the selected modpacks(s)?\r\nNo crying afterwards?", "Question");
                        if (ans == DialogResult.No) {
                            form1.pBar_hide();
                            return;
                        }
                    }
                    chk = true;

                    string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                    if (Config.isPatched(modpackname)) {    // deliberately prompt for each modpack that is enabled
                        DialogResult ans = form1.showMsg("WARNING: The " + modpackname + " modpack is showing as currently installed. " +
                            "Deleting this modpack will also unpatch it from the game. Continue?", "Question");
                        if (ans == DialogResult.No) {
                            partial = true;
                            continue;
                        } else {
                            if (unpatchModpack(modpackname) == 2) {
                                partial = true;
                                continue;
                            }
                            Config.rmPatched(modpackname);
                        }
                    }

                    if (!IO.DeleteFile(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                        form1.showMsg("Could not delete '" + modpackname + ".zip'. Is the zip file open somewhere?", "Error");
                    }
                    del = true;
                    chb.Checked = false;
                }
            }
            if (!chk) {
                form1.showMsg("No items selected from the list.", "Error");
            } else if (!del) {
                form1.showMsg("No modpacks were deleted.", "Warning");
            } else if (del && partial) {
                form1.showMsg("Only some of the selected modpacks have been deleted.", "Warning");
                loadModpacks();
            } else {
                form1.showMsg("Selected modpacks have been deleted.", "Info");
                loadModpacks();
            }
            Config.saveCfg();
            form1.pBar_hide();
        }

        private static List<Dictionary<string, string>> getModpackConfig(ZipArchive archive)
        {
            ZipArchiveEntry modpackConfigEntry = archive.GetEntry("modpack_config.cfg");
            if (modpackConfigEntry == null) {
                return null;
            }
            List<Dictionary<string, string>> modpackConfig;
            using (Stream jsonStream = modpackConfigEntry.Open()) {
                StreamReader reader = new StreamReader(jsonStream);
                try {
                    modpackConfig = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(reader.ReadToEnd());
                } catch (JsonSerializationException) {
                    return null;
                } catch (JsonReaderException) {
                    return null;
                }
            }

            return modpackConfig;
        }

        private static string willOverwriteOtherMod(string modpack)
        {
            List<Dictionary<string, string>> primaryConfig;
            using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpack + ".zip")) {
                primaryConfig = getModpackConfig(archive);
            }

            foreach (string enabledModpack in Config.patched) {
                List<Dictionary<string, string>> modpackConfig;
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + enabledModpack + ".zip")) {
                    modpackConfig = getModpackConfig(archive);
                }
                // Deliberately not checking for null so program throws a stack trace
                // This should never happen, but if it does I want the user to let me know about it
                foreach (Dictionary<string, string> dict in modpackConfig) {
                    foreach (Dictionary<string, string> primary in primaryConfig) {
                        if (dict["dest"] == primary["dest"]) {
                            return enabledModpack;
                        }
                    }
                }
            }

            return null;
        }

        private static int patchFile(ZipArchive archive, Dictionary<string,string> entry)
        {
            string destination = expandPath(entry["dest"]);
            bool baksMade = false;
            ZipArchiveEntry modFile = archive.GetEntry(entry["src"]);
            if (modFile == null) {
                return 3;
            }
            if (!entry.ContainsKey("type") || String.IsNullOrEmpty(entry["type"]) || entry["type"] == "replace") {  // assume replace type entry
                if (File.Exists(destination)) {
                    if (Backups.createBackup(destination, false) == 0) {
                        baksMade = true;
                    }
                    if (!IO.DeleteFile(destination)) {
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
            } else if (entry["type"] == "patch") {
                return 0;   // TODO: Add patching functionality
            } else if (entry["type"] == "create") {
                if (File.Exists(destination)) {
                    if (!IO.DeleteFile(destination)) {
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

        private static int patchModpack(string modpackname)
        {
            string retStr = willOverwriteOtherMod(modpackname);
            if (!String.IsNullOrEmpty(retStr)) {
                form1.showMsg("Installing '" + modpackname + "' would overwrite files for '" + retStr + "'. Modpack will be skipped.", "Error");
                return 2;
            }

            bool baksMade = false;
            try {
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                    List<Dictionary<string, string>> modpackConfig = getModpackConfig(archive);
                    if (modpackConfig == null) {
                        form1.showMsg("The file '" + modpackname + ".zip' is either not a compatible modpack or the config is corrupted." +
                            "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                        return 2;
                    }

                    List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                    foreach (Dictionary<string, string> dict in modpackConfig) {
                        int r = patchFile(archive, dict);
                        if (r != 0 && r != 1) {
                            string errMsg;
                            if (r == 2) {
                                errMsg = "File Access Exception. If the game is running, exit it and try again.";
                            } else if (r == 3) {
                                errMsg = "This modpack appears to be missing files.";
                            } else {    // r == 4
                                errMsg = "Unknown modfile type in modpack config.";
                            }
                            form1.showMsg(errMsg + "\r\nCould not install the '" + modpackname + "' modpack.", "Error");

                            if (Backups.restoreBaks(patched) != 0) {
                                form1.showMsg("At least one file restore failed. Your game is likely in an unstable state.", "Warning");
                            }
                            return 2;
                        } else if (r == 1) {
                            baksMade = true;
                        }

                        patched.Add(expandPath(dict["dest"]));
                    }
                }
            } catch (FileNotFoundException) {
                form1.showMsg("Could not find the '" + modpackname + "' modpack.", "Error");
                return 2;
            } catch (InvalidDataException) {
                form1.showMsg("The modpack '" + modpackname + ".zip' appears corrupted." +
                "\r\nThis modpack cannot be installed.", "Error");
                return 2;
            }

            if (baksMade) {
                return 1;
            } else {
                return 0;
            }
        }

        private static int patchModpacks(List<CheckBox> toPatch)
        {
            bool baksMade = false;
            bool packErr = false;
            form1.pBar_show(toPatch.Count());
            foreach (CheckBox chb in toPatch) {
                form1.pBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = patchModpack(modpackname);
                if (ret == 2) {
                    packErr = true;
                    chb.Checked = false;
                } else {    // modpack was patched
                    Config.addPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(new Point(chb.Location.X - 45, chb.Location.Y))).Image = Properties.Resources.greenDot_15px;
                }
            }

            form1.pBar_hide();
            if (packErr) {   // fail / partial success - At least one modpack was not patched
                return 2;
            } else if (baksMade) {  // success and new backup(s) created
                return 1;
            } else {
                return 0;
            }
        }

        private static int unpatchModpack(string modpackname)
        {
            try {
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                    List<Dictionary<string, string>> modpackConfig = getModpackConfig(archive);
                    if (modpackConfig == null) {
                        form1.showMsg("Could not unpatch '" + modpackname + "' because the modpack's configuration is corrupted or missing." +
                            "\r\nPlease restore from backups using the Backups tab or verify integrity of game files on Steam.", "Error");
                        return 2;
                    }
                    List<Dictionary<string, string>> restored = new List<Dictionary<string, string>>(); // track restored files in case of failure mid unpatch
                    foreach (Dictionary<string, string> dict in modpackConfig) {
                        if (!dict.ContainsKey("type") || String.IsNullOrEmpty(dict["type"]) || dict["type"] == "replace") { // assume replace type entry
                            if (!Backups.restoreBak(expandPath(dict["dest"]))) {
                                // repatch restored mod files
                                foreach (Dictionary<string, string> entry in restored) {
                                    int r = patchFile(archive, entry);
                                    if (r == 2 || r == 3) {
                                        form1.showMsg("Critical error encountered while unpatching '" + modpackname + "'." +
                                            "\r\nYou may need to verify your game files on steam or reinstall.", "Error");
                                    }
                                }
                                return 2;
                            }
                        } else if (dict["type"] == "patch") {
                            //TODO: Add patch funtionality
                        } else if (dict["type"] == "create") {
                            if (!IO.DeleteFile(expandPath(dict["dest"]))) {
                                form1.showMsg("Could not delete the file '" + expandPath(dict["dest"]) + "'. This may affect your game. " +
                                    "if you encounter issue please delete this file manually.", "Warning");
                            }
                        } else {
                            form1.showMsg("Unknown modfile type in modpack config.\r\nCould not install the '" + modpackname + "' modpack.", "Error");
                        }
                        restored.Add(dict);
                    }
                }
            } catch (FileNotFoundException) {
                form1.showMsg("Could not unpatch '" + modpackname + "'. Could not find the modpack file to read the config from.", "Error");
                return 2;
            } catch (InvalidDataException) {
                form1.showMsg("Could not unpatch '" + modpackname + "'. The modpack file appears corrupted and the config cannot be read.", "Error");
                return 2;
            }

            return 0;
        }

        private static int unpatchModpacks(List<CheckBox> toUnpatch)
        {
            bool packErr = false;
            form1.pBar_show(toUnpatch.Count());
            foreach (CheckBox chb in toUnpatch) {
                form1.pBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = unpatchModpack(modpackname);
                if (ret == 2) {
                    packErr = true;
                    chb.Checked = true;
                } else {    // modpack was unpatched
                    Config.rmPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(new Point(chb.Location.X - 45, chb.Location.Y))).Image = Properties.Resources.redDot_15px;
                }
            }

            form1.pBar_hide();
            if (packErr) { // fail / partial success - At least one modpack was not patched
                return 2;
            } else {    // success, no errors
                return 0;
            }
        }

        public static void runPatchUnpatch(IEnumerable<CheckBox> modpacksList)
        {
            List<CheckBox> toPatch = new List<CheckBox>();
            List<CheckBox> toUnpatch = new List<CheckBox>();
            foreach (CheckBox chb in modpacksList) {
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (chb.Checked && !Config.isPatched(modpackname)) {
                    toPatch.Add(chb);
                } else if (!chb.Checked && Config.isPatched(modpackname)) {
                    toUnpatch.Add(chb);
                }
            }

            if (toPatch.Count() == 0 && toUnpatch.Count() == 0) {
                form1.showMsg("You did not select any changes. No modpacks were patched or unpatched.", "Info");
                return;
            }

            // Unpatch mods before trying to patch new ones
            int retU = unpatchModpacks(toUnpatch);
            int retP = patchModpacks(toPatch);

            Config.saveCfg();

            if (retU == 2 || retP == 2) {   // fail / partial success - At least one modpack was not patched
                form1.showMsg("Failed in patching/unpatching at least one modpack.", "Warning");
            } else if (retP == 1) {  // success and new backup(s) created
                form1.showMsg("The game has been updated with your modpack selection.\r\nNew backups were created.", "Info");
            } else {    // success, no new backups (retU == 0 && retP == 0)
                form1.showMsg("The game has been updated with your changes.", "Info");
            }
        }
    }
}
