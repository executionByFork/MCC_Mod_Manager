using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using Newtonsoft.Json;
using MCC_Mod_Manager.Api.Utilities;

namespace MCC_Mod_Manager.Api {
    public class ModpackEntry {
        public string src;
        public string orig;
        public string dest;
        public string type;
    }
    public class ModpackCfg {
        public string MCC_version;
        public List<ModpackEntry> entries = new List<ModpackEntry>();
    }

    static class Modpacks {

        #region Event Handlers

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
            ModpackCfg mCfg = new ModpackCfg {
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

                mCfg.entries.Add(new ModpackEntry {
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

        #endregion

        #region Api Functions

        public static ModpackCfg GetModpackConfig(string modpackName) {
            try {
                using (ZipArchive archive = ZipFile.OpenRead(Config.Modpack_dir + @"\" + modpackName + ".zip")) {
                    ZipArchiveEntry modpackConfigEntry = archive.GetEntry("modpack_config.cfg");
                    if (modpackConfigEntry == null) {
                        return null;
                    }
                    ModpackCfg modpackConfig;
                    using (Stream jsonStream = modpackConfigEntry.Open()) {
                        StreamReader reader = new StreamReader(jsonStream);
                        try {
                            modpackConfig = JsonConvert.DeserializeObject<ModpackCfg>(reader.ReadToEnd());
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

        public static bool LoadModpacks() {
            EnsureModpackFolderExists();
            Program.MasterForm.ModListPanel_clear();

            string[] fileEntries = Directory.GetFiles(Config.Modpack_dir);
            foreach (string file in fileEntries) {
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                ModpackCfg modpackConfig = GetModpackConfig(modpackName);
                if (modpackConfig != null) {
                    Program.MasterForm.ModListPanel_add(modpackName, modpackConfig.MCC_version == Config.GetCurrentBuild());
                }
            }

            return true;
        }

        public static string ExpandPath(string p) {
            return p.Replace("$MCC_home", Config.MCC_home);
        }

        public static bool StabilizeGame() { // used after update is detected to uninstall half clobbered mods
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

        public static string GetMD5(string filePath) {
            using (FileStream stream = File.OpenRead(filePath)) {
                using (MD5 md5 = MD5.Create()) {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static bool VerifyExists(string modpackname) {
            if (File.Exists(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                return true;
            }
            return false;
        }

        #endregion

        #region Helper Functions

        private static bool EnsureModpackFolderExists() {
            if (!Directory.Exists(Config.Modpack_dir)) {
                _ = Directory.CreateDirectory(Config.Modpack_dir);
            }
            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        private static string CompressPath(string p) {
            return p.Replace(Config.MCC_home, "$MCC_home");
        }

        public static Dictionary<string, List<string>> GetFilesToRestore() // used after update is detected to find file changes
{
            Dictionary<string, List<string>> restoreMapping = new Dictionary<string, List<string>>();
            bool packClobbered = false;
            foreach (KeyValuePair<string, Dictionary<string, string>> modpack in Config.Patched) {
                List<string> potentialRestores = new List<string>();
                foreach (KeyValuePair<string, string> fileEntry in modpack.Value) {
                    if (GetMD5(ExpandPath(fileEntry.Key)) == fileEntry.Value) { // if file is still modded after the update
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

        #endregion
    }
}
