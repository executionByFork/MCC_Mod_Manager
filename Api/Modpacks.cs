using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using MCC_Mod_Manager.Api;

namespace MCC_Mod_Manager
{
    static class Modpacks
    {
        #region Event Handlers

        #endregion

        private static bool EnsureModpackFolderExists()
        {
            if (!Directory.Exists(Config.modpack_dir)) {
                Directory.CreateDirectory(Config.modpack_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        private static string CompressPath(string p)
        {
            return p.Replace(Config.MCC_home, "$MCC_home");
        }

        public static void Create_fileBrowse1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"  // using the GUID to access 'This PC' folder
            };
            if (ofd.ShowDialog() == DialogResult.OK) {
                ((Button)sender).Parent.GetChildAtPoint(Config.sourceTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static void Create_fileBrowse2(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {
                CheckFileExists = false,    // allow modpack creators to type in a filename for creating new files
                InitialDirectory = Config.MCC_home
            };
            if (ofd.ShowDialog() == DialogResult.OK) {
                ((Button)sender).Parent.GetChildAtPoint(Config.destTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static bool LoadModpacks()
        {
            EnsureModpackFolderExists();
            Program.MasterForm.ModListPanel_clear();

            string[] fileEntries = Directory.GetFiles(Config.modpack_dir);
            foreach (string file in fileEntries) {
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                CheckBox chb = new CheckBox {
                    AutoSize = true,
                    Text = Config.dirtyPadding + modpackName,
                    Location = new Point(60, (Program.MasterForm.ModListPanel_getCount() * 20) + 1),
                    Checked = Config.IsPatched(modpackName)
                };
                PictureBox p = new PictureBox {
                    Width = 15,
                    Height = 15,
                    Location = new Point(15, (Program.MasterForm.ModListPanel_getCount() * 20) + 1),
                    Image = Config.IsPatched(modpackName) ? Properties.Resources.greenDot_15px : Properties.Resources.redDot_15px
                };
                if (Program.MasterForm.ManualOverrideEnabled()) {
                    p.Click += ForceModpackState;
                    p.MouseEnter += Program.MasterForm.btnHoverOn;
                    p.MouseLeave += Program.MasterForm.btnHoverOff;
                }

                Program.MasterForm.ModListPanel_add(p, chb);
            }

            return true;
        }

        private static void ForceModpackState(object sender, EventArgs e)
        {
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

        public static bool VerifyExists(string modpackname)
        {
            if (File.Exists(Config.modpack_dir + @"\" + modpackname + ".zip")) {
                return true;
            }
            return false;
        }

        public static void CreateModpack(string modpackName, IEnumerable<Panel> modFilesList)
        {
            if (modFilesList.Count() == 0) {
                Utility.ShowMsg("Please add at least one modded file entry", "Error");
                return;
            }
            if (String.IsNullOrEmpty(modpackName)) {
                Utility.ShowMsg("Please enter a modpack name", "Error");
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
                    Utility.ShowMsg("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(dict["src"])) {
                    Utility.ShowMsg("The source file '" + dict["src"] + "' does not exist.", "Error");
                    return;
                }
                if (!dict["dest"].StartsWith(Config.MCC_home)) {
                    Utility.ShowMsg("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }

                // make modpack compatable with any MCC_home directory
                dict["dest"] = CompressPath(dict["dest"]);

                fileMap.Add(dict);
                chk.Add(row.GetChildAtPoint(Config.destTextBoxPoint).Text);
            }

            if (chk.Distinct().Count() != chk.Count()) {
                Utility.ShowMsg("You have multiple files trying to write to the same destination.", "Error");
                return;
            }

            EnsureModpackFolderExists();
            String modpackFilename = modpackName + ".zip";
            String zipPath = Config.modpack_dir + @"\" + modpackFilename;
            if (File.Exists(zipPath)) {
                Utility.ShowMsg("A modpack with that name already exists.", "Error");
                return;
            }

            Program.MasterForm.pBar_show(fileMap.Count());
            try {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    foreach (var entry in fileMap) {
                        Program.MasterForm.pBar_update();
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
                Utility.ShowMsg("The modpack name you have provided is not a valid filename on Windows.", "Error");
                return;
            }

            Utility.ShowMsg("Modpack '" + modpackFilename + "' created.", "Info");
            Program.MasterForm.pBar_hide();
            Program.MasterForm.resetCreateModpacksTab();
            LoadModpacks();
            return;
        }
    }
}
