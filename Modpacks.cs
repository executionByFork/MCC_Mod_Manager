using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace MCC_Mod_Manager
{
    public class modpackEntry
    {
        public string src;
        public string orig;
        public string dest;
        public string type;
    }
    public class modpackCfg
    {
        public string MCC_version;
        public List<modpackEntry> entries = new List<modpackEntry>();
    }

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

        public static string expandPath(string p)
        {
            return p.Replace("$MCC_home", Config.MCC_home);
        }
        private static string compressPath(string p)
        {
            return p.Replace(Config.MCC_home, "$MCC_home");
        }

        public static string getMD5(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath)) {
                using (MD5 md5 = MD5.Create()) {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void create_fileBrowse(object sender, EventArgs e)
        {
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
                            form1.swapRowType(panel);
                        } else {
                            TextBox orig_txt = (TextBox)panel.GetChildAtPoint(Config.origTextBoxPoint);
                            Button orig_btn = (Button)panel.GetChildAtPoint(Config.origBtnPoint);
                            orig_txt.Enabled = true;
                            orig_txt.Text = "";
                            orig_btn.Enabled = true;
                        }
                    } else {    // if not an .asmp file
                        if ((string)panel.Tag != "normal") {
                            form1.swapRowType(panel);
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
                        form1.showMsg("The file must have a .map extension", "Error");
                    }
                }
            }
        }

        public static Dictionary<string, List<string>> getFilesToRestore() // used after update is detected to find file changes
        {
            Dictionary<string, List<string>> restoreMapping = new Dictionary<string, List<string>>();
            bool packClobbered = false;
            foreach (KeyValuePair<string, Dictionary<string, string>> modpack in Config.patched) {
                List<string> potentialRestores = new List<string>();
                foreach (KeyValuePair<string, string> fileEntry in modpack.Value) {
                    if (getMD5(expandPath(fileEntry.Key)) == fileEntry.Value) { // if file is still modded after the update
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

        public static bool stabilizeGame()  // used after update is detected to uninstall half clobbered mods
        {
            Dictionary<string, List<string>> restoreMap = getFilesToRestore();

            foreach (KeyValuePair<string, List<string>> modpack in restoreMap) {
                foreach (KeyValuePair<string,string> entry in Config.patched[modpack.Key]) {
                    if (modpack.Value.Contains(entry.Key)) {
                        Backups.restoreBak(expandPath(entry.Key));
                    } else {
                        Backups.deleteBak(expandPath(entry.Key));
                    }
                }
                
                Config.rmPatched(modpack.Key);
            }
            Config.MCC_version = Config.getCurrentBuild();
            Config.saveCfg();
            Backups.saveBackups();

            return true;
        }

        public static bool loadModpacks()
        {
            ensureModpackFolderExists();
            form1.modListPanel_clear();

            string[] fileEntries = Directory.GetFiles(Config.modpack_dir);
            foreach (string file in fileEntries) {
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                modpackCfg modpackConfig = getModpackConfig(modpackName);
                if (modpackConfig != null) {
                    form1.modListPanel_add(modpackName, modpackConfig.MCC_version == Config.getCurrentBuild());
                }
            }

            return true;
        }

        public static void forceModpackState(object sender, EventArgs e)
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
            modpackCfg mCfg = new modpackCfg();
            mCfg.MCC_version = IO.readFirstLine(Config.MCC_home + @"\build_tag.txt");
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
                    form1.showMsg("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(srcText)) {
                    form1.showMsg("The source file '" + srcText + "' does not exist.", "Error");
                    return;
                }
                if (!destText.StartsWith(Config.MCC_home)) {
                    form1.showMsg("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }
                if ((string)row.Tag == "alt" && origTextbox.Enabled && !origTextbox.Text.StartsWith(Config.MCC_home)) {
                    form1.showMsg("Unmodified map files must be selected at their default install location within the MCC install directory to allow the patch " +
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
                        isOriginalFile = IO.isHaloFile(compressPath(destText));
                    } catch (JsonReaderException) {
                        form1.showMsg(@"MCC Mod Manager could not parse Formats\filetree.json", "Error");
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
                    orig = (patchType == "patch") ? compressPath(origTextbox.Text) : null,
                    dest = compressPath(destText),  // make modpack compatable with any MCC_home directory
                    type = patchType
                });
                chk.Add(destText);
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

            form1.pBar_show(mCfg.entries.Count());
            try {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    foreach (var entry in mCfg.entries) {
                        form1.pBar_update();
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
                form1.pBar_update();    //TODO: This updates on EVERY modpack which isn't quite accurate
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

        public static modpackCfg getModpackConfig(string modpackName)
        {
            try {
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackName + ".zip")) {
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

        private static string willOverwriteOtherMod(string modpack)
        {
            modpackCfg primaryConfig = getModpackConfig(modpack);

            foreach (string enabledModpack in Config.getEnabledModpacks()) {
                modpackCfg modpackConfig = getModpackConfig(enabledModpack);

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

        private static int patchFile(ZipArchive archive, modpackEntry entry)
        {
            string destination = expandPath(entry.dest);
            bool baksMade = false;
            ZipArchiveEntry modFile = archive.GetEntry(entry.src);
            if (modFile == null) {
                return 3;
            }
            if (String.IsNullOrEmpty(entry.type) || entry.type == "replace") {  // assume replace type entry
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
            } else if (entry.type == "patch") {
                if (File.Exists(destination)) {
                    if (Backups.createBackup(destination, false) == 0) {
                        baksMade = true;
                    }
                    if (!IO.DeleteFile(destination)) {
                        return 2;
                    }
                }

                string unmoddedPath = expandPath(entry.orig);
                if (!IO.getUnmodifiedHash(entry.orig).Equals(getMD5(unmoddedPath), StringComparison.OrdinalIgnoreCase)) {
                    unmoddedPath = Config.backup_dir + @"\" + Backups._baks[unmoddedPath];  // use backup version
                }

                if (!AssemblyPatching.applyPatch(modFile, Path.GetFileName(entry.src), unmoddedPath, destination)) {
                    return 5;   // no extra error message
                }

                if (baksMade) {
                    return 1;
                } else {
                    return 0;
                }
            } else if (entry.type == "create") {
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
                modpackCfg modpackConfig = getModpackConfig(modpackname);
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                    if (modpackConfig == null) {
                        form1.showMsg("The file '" + modpackname + ".zip' is either not a compatible modpack or the config is corrupted." +
                            "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                        return 2;
                    }

                    List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                    foreach (modpackEntry entry in modpackConfig.entries) {
                        int r = patchFile(archive, entry);
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

                        patched.Add(expandPath(entry.dest));
                    }
                }
            } catch (FileNotFoundException) {
                form1.showMsg("Could not find the '" + modpackname + "' modpack.", "Error");
                return 2;
            } catch (InvalidDataException) {
                form1.showMsg("The modpack '" + modpackname + "' appears corrupted." +
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
                    if (ret == 1) {
                        baksMade = true;
                    }
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
                modpackCfg modpackConfig = getModpackConfig(modpackname);
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                    if (modpackConfig == null) {
                        form1.showMsg("Could not unpatch '" + modpackname + "' because the modpack's configuration is corrupted or missing." +
                            "\r\nPlease restore from backups using the Backups tab or verify integrity of game files on Steam.", "Error");
                        return 2;
                    }
                    List<modpackEntry> restored = new List<modpackEntry>(); // track restored files in case of failure mid unpatch
                    foreach (modpackEntry entry in modpackConfig.entries) {
                        if (String.IsNullOrEmpty(entry.type) || entry.type == "replace" || entry.type == "patch") { // assume replace type entry if null
                            if (!Backups.restoreBak(expandPath(entry.dest))) {
                                // repatch restored mod files
                                foreach (modpackEntry e in restored) {
                                    int r = patchFile(archive, e);
                                    if (r == 2 || r == 3) {
                                        form1.showMsg("Critical error encountered while unpatching '" + modpackname + "'." +
                                            "\r\nYou may need to verify your game files on steam or reinstall.", "Error");
                                    }
                                }
                                return 2;
                            }
                        } else if (entry.type == "create") {
                            if (!IO.DeleteFile(expandPath(entry.dest))) {
                                form1.showMsg("Could not delete the file '" + expandPath(entry.dest) + "'. This may affect your game. " +
                                    "if you encounter issue please delete this file manually.", "Warning");
                            }
                        } else {
                            form1.showMsg("Unknown modfile type in modpack config.\r\nCould not install the '" + modpackname + "' modpack.", "Error");
                        }
                        restored.Add(entry);
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
