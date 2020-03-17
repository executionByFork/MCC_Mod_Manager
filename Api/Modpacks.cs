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

        public static void AddRowButton_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}",  // using the GUID to access 'This PC' folder
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK) {
                foreach (string file in ofd.FileNames) {
                    string type;
                    if (Path.GetExtension(file) == ".asmp") {
                        type = "alt";
                    } else {
                        type = "normal";
                    }
                    Program.MasterForm.createFilesPanel.Controls.Add(MakeRow(file, type));
                }
            }
        }

        public static void readmeToggleButton_Click(object sender, EventArgs e) {
            if ((bool)Program.MasterForm.readmeToggleButton.Tag == false) {   // Hide readme editing
                Program.MasterForm.readmeTxt.Enabled = false;
                Program.MasterForm.readmeTxt.Visible = false;
                Program.MasterForm.createFilesPanel.Visible = true;
                Program.MasterForm.addRowButton.Visible = true;
                Program.MasterForm.addRowButton.Enabled = true;
                Program.MasterForm.createLabel1.Text = "Modded File";
                Program.MasterForm.createLabel2.Visible = true;

                Program.MasterForm.tt.SetToolTip(Program.MasterForm.readmeToggleButton, "Edit Readme");
                Program.MasterForm.readmeToggleButton.Tag = true;   // use tag to track UI state
            } else {    // Reveal readme editing
                Program.MasterForm.readmeTxt.Enabled = true;
                Program.MasterForm.readmeTxt.Visible = true;
                Program.MasterForm.createFilesPanel.Visible = false;
                Program.MasterForm.addRowButton.Visible = false;
                Program.MasterForm.addRowButton.Enabled = false;
                Program.MasterForm.createLabel1.Text = "Editing README.txt";
                Program.MasterForm.createLabel2.Visible = false;

                Program.MasterForm.tt.SetToolTip(Program.MasterForm.readmeToggleButton, "Manage files");
                Program.MasterForm.readmeToggleButton.Tag = false;  // use tag to track UI state
            }
        }

        public static void CreateModpackBtn_Click(object sender, EventArgs e) {
            string modpackName = Program.MasterForm.modpackName_txt.Text;
            IEnumerable<Panel> modFilesList = Program.MasterForm.createFilesPanel.Controls.OfType<Panel>();
            if (modFilesList.Count() == 0) {
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
                        isOriginalFile = Utility.IsHaloFile(Utility.CompressPath(destText));
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
                    orig = (patchType == "patch") ? Utility.CompressPath(origTextbox.Text) : null,
                    dest = Utility.CompressPath(destText),  // make modpack compatable with any MCC_home directory
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
                        if (String.IsNullOrEmpty(Program.MasterForm.readmeTxt.Text)) {
                            writer.WriteLine(Config._defaultReadmeText);
                        } else {
                            writer.WriteLine(Program.MasterForm.readmeTxt.Text);
                        }
                    }
                }
            } catch (NotSupportedException) {
                Utility.ShowMsg("The modpack name you have provided is not a valid filename on Windows.", "Error");
                return;
            }

            Utility.ShowMsg("Modpack '" + modpackFilename + "' created.", "Info");
            Program.MasterForm.PBar_hide();
            ResetCreateModpacksTab();
            MyMods.LoadModpacks();
            return;
        }

        public static void DeleteRow(object sender, EventArgs e) {
            Program.MasterForm.createFilesPanel.Controls.Remove((Panel)((PictureBox)sender).Parent);
            RedrawCreatePanel();
        }

        public static void ClearBtn_Click(object sender, EventArgs e) {
            ResetCreateModpacksTab();
        }

        #endregion

        #region UI Functions

        private static void ResetCreateModpacksTab() {
            Program.MasterForm.createFilesPanel.Controls.Clear();
            Program.MasterForm.modpackName_txt.Text = "";
            Program.MasterForm.readmeTxt.Text = Config._defaultReadmeText;
        }

        public static Panel MakeRow(string filepath, string type) {
            PictureBox del = new PictureBox();
            del.Image = del.ErrorImage;    // bit of a hack to get the error image to appear
            del.Width = 14;
            del.Height = 16;
            del.MouseEnter += Program.MasterForm.BtnHoverOn;
            del.MouseLeave += Program.MasterForm.BtnHoverOff;
            del.Click += DeleteRow;
            if (type == "normal") {
                del.Location = Config.delBtnPoint;
            } else {
                del.Location = Config.delBtnPointAlt;
            }

            TextBox txt1 = new TextBox {
                Width = 180,
                Text = filepath,
                Location = Config.sourceTextBoxPoint
            };

            Button btn1 = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "...",
                Location = Config.sourceBtnPoint,
                Tag = "btn1"
            };
            btn1.Click += Modpacks.Create_fileBrowse;
            Program.MasterForm.tt.SetToolTip(btn1, "Select modded file or .asmp file");

            Label lbl = new Label {
                Width = 33,
                Font = Config.arrowFont,
                Text = ">>",
                Location = (type == "normal") ? Config.arrowPoint : Config.arrowPointAlt
            };

            TextBox txt2 = new TextBox {
                Width = 180,
                Location = (type == "normal") ? Config.destTextBoxPoint : Config.destTextBoxPointAlt
            };

            Button btn2 = new Button {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "...",
                Location = (type == "normal") ? Config.destBtnPoint : Config.destBtnPointAlt,
                Tag = "btn2"
            };
            btn2.Click += Modpacks.Create_fileBrowse;
            Program.MasterForm.tt.SetToolTip(btn2, "Select output location");

            int offset = 5;
            foreach (Panel row in Program.MasterForm.createFilesPanel.Controls.OfType<Panel>()) {
                if ((string)row.Tag == "normal") {
                    offset += 27;
                } else {    // Tagged as alt
                    offset += 54;
                }
            }

            Panel newPanel = new Panel {
                Width = 500,
                Height = (type == "normal") ? 25 : 50,
                Location = new Point(10, offset),
                Tag = type
            };

            if (type == "alt") {
                TextBox txt3 = new TextBox {
                    Width = 180,
                    Location = Config.origTextBoxPoint
                };

                Button btn3 = new Button {
                    BackColor = SystemColors.ButtonFace,
                    Width = 39,
                    Font = Config.btnFont,
                    Text = "...",
                    Location = Config.origBtnPoint,
                    Tag = "btn3"
                };
                btn3.Click += Modpacks.Create_fileBrowse;
                Program.MasterForm.tt.SetToolTip(btn3, "Select unmodified map file");

                newPanel.Controls.Add(txt3);
                newPanel.Controls.Add(btn3);
            }

            newPanel.Controls.Add(del);
            newPanel.Controls.Add(txt1);
            newPanel.Controls.Add(btn1);
            newPanel.Controls.Add(lbl);
            newPanel.Controls.Add(txt2);
            newPanel.Controls.Add(btn2);

            return newPanel;
        }

        public static void SwapRowType(Panel p) {
            if ((string)p.Tag == "normal") {
                p.Height = 50;
                p.GetChildAtPoint(Config.delBtnPoint).Location = Config.delBtnPointAlt;
                // Retrieving label by coords doesn't work for some reason
                p.Controls.OfType<Label>().First().Location = Config.arrowPointAlt;
                p.GetChildAtPoint(Config.destTextBoxPoint).Location = Config.destTextBoxPointAlt;
                p.GetChildAtPoint(Config.destBtnPoint).Location = Config.destBtnPointAlt;
                p.Tag = "alt";

                TextBox txt3 = new TextBox {
                    Width = 180,
                    Location = Config.origTextBoxPoint
                };

                Button btn3 = new Button {
                    BackColor = SystemColors.ButtonFace,
                    Width = 39,
                    Font = Config.btnFont,
                    Text = "...",
                    Location = Config.origBtnPoint,
                    Tag = "btn3"
                };
                btn3.Click += Modpacks.Create_fileBrowse;

                p.Controls.Add(txt3);
                p.Controls.Add(btn3);
            } else {    // Tagged as alt
                p.Height = 25;
                p.GetChildAtPoint(Config.delBtnPointAlt).Location = Config.delBtnPoint;
                // Retrieving label by coords doesn't work for some reason
                p.Controls.OfType<Label>().First().Location = Config.arrowPoint;
                p.GetChildAtPoint(Config.destTextBoxPointAlt).Location = Config.destTextBoxPoint;
                p.GetChildAtPoint(Config.destBtnPointAlt).Location = Config.destBtnPoint;
                p.Tag = "normal";
            }
            RedrawCreatePanel();
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
                            SwapRowType(panel);
                        } else {
                            TextBox orig_txt = (TextBox)panel.GetChildAtPoint(Config.origTextBoxPoint);
                            Button orig_btn = (Button)panel.GetChildAtPoint(Config.origBtnPoint);
                            orig_txt.Enabled = true;
                            orig_txt.Text = "";
                            orig_btn.Enabled = true;
                        }
                    } else {    // if not an .asmp file
                        if ((string)panel.Tag != "normal") {
                            SwapRowType(panel);
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

        public static void RedrawCreatePanel() {
            int offset = 5;
            foreach (Panel panel in Program.MasterForm.createFilesPanel.Controls.OfType<Panel>()) {
                panel.Location = new Point(10, offset);
                if ((string)panel.Tag == "normal") {
                    offset += 27;
                } else {    // Tagged as alt
                    offset += 54;
                }
            }
        }
        #endregion

        #region Api Functions

        public static bool StabilizeGame() { // used after update is detected to uninstall half clobbered mods
            Dictionary<string, List<string>> restoreMap = GetFilesToRestore();

            foreach (KeyValuePair<string, List<string>> modpack in restoreMap) {
                foreach (KeyValuePair<string, string> entry in Config.Patched[modpack.Key].files) {
                    if (modpack.Value.Contains(entry.Key)) {
                        Backups.RestoreBak(Utility.ExpandPath(entry.Key));
                    } else {
                        Backups.DeleteBak(Utility.ExpandPath(entry.Key));
                    }
                }

                Config.RmPatched(modpack.Key);
            }
            Config.MCC_version = Config.GetCurrentBuild();
            Config.SaveCfg();
            Backups.SaveBackups();

            return true;
        }

        #endregion

        #region Helper Functions

        public static bool EnsureModpackFolderExists() {
            if (!Directory.Exists(Config.Modpack_dir)) {
                Directory.CreateDirectory(Config.Modpack_dir);
            }
            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static bool VerifyExists(string modpackname) {
            if (File.Exists(Config.Modpack_dir + @"\" + modpackname + ".zip")) {
                return true;
            }
            return false;
        }

        public static string GetMD5(string filePath) {
            try {
                using (FileStream stream = File.OpenRead(filePath)) {
                    using (MD5 md5 = MD5.Create()) {
                        byte[] hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            } catch (FileNotFoundException) {
                return null;
            }
        }

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

        private static Dictionary<string, List<string>> GetFilesToRestore() // used after update is detected to find file changes
{
            Dictionary<string, List<string>> restoreMapping = new Dictionary<string, List<string>>();
            bool packClobbered = false;
            foreach (KeyValuePair<string, patchedEntry> modpack in Config.Patched) {
                List<string> potentialRestores = new List<string>();
                foreach (KeyValuePair<string, string> fileEntry in modpack.Value.files) {
                    if (GetMD5(Utility.ExpandPath(fileEntry.Key)) == fileEntry.Value) { // if file is still modded after the update
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

        #endregion
    }
}
