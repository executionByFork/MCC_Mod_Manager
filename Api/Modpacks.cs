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
        public static void AddRowButton_Click(object sender, EventArgs e)
        {
            PictureBox del = new PictureBox();
            del.Image = del.ErrorImage;    // bit of a hack to get the error image to appear
            del.Width = 14;
            del.Height = 16;
            del.MouseEnter += Program.MasterForm.BtnHoverOn;
            del.MouseLeave += Program.MasterForm.btnHoverOff;
            del.Click += Modpacks.DeleteRow;
            del.Location = Config.delBtnPoint;

            TextBox txt1 = new TextBox
            {
                Width = 180,
                Location = Config.sourceTextBoxPoint
            };

            Button btn1 = new Button
            {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "..."
            };
            btn1.Click += Modpacks.Create_fileBrowse1;
            btn1.Location = Config.sourceBtnPoint;

            Label lbl = new Label
            {
                Width = 33,
                Font = Config.arrowFont,
                Text = ">>",
                Location = Config.arrowPoint
            };

            TextBox txt2 = new TextBox
            {
                Width = 180,
                Location = Config.destTextBoxPoint
            };

            Button btn2 = new Button
            {
                BackColor = SystemColors.ButtonFace,
                Width = 39,
                Font = Config.btnFont,
                Text = "..."
            };
            btn2.Click += Modpacks.Create_fileBrowse2;
            btn2.Location = Config.destBtnPoint;

            Panel p = new Panel
            {
                Width = 500,
                Height = 25,
                Location = new Point(10, (Program.MasterForm.createFilesPanel.Controls.Count * 25) + 5)
            };
            p.Controls.Add(del);
            p.Controls.Add(txt1);
            p.Controls.Add(btn1);
            p.Controls.Add(lbl);
            p.Controls.Add(txt2);
            p.Controls.Add(btn2);

            Program.MasterForm.createPageList.Add(p);
            Program.MasterForm.createFilesPanel.Controls.Add(p);
        }

        public static void DeleteRow(object sender, EventArgs e)
        {
            Program.MasterForm.createPageList.Remove((Panel)((PictureBox)sender).Parent);
            Program.MasterForm.createFilesPanel.Controls.Clear();
            for (int i = 0; i < Program.MasterForm.createPageList.Count; i++)
            {
                Program.MasterForm.createPageList[i].Location = new Point(10, (Program.MasterForm.createFilesPanel.Controls.Count * 25) + 5);
                Program.MasterForm.createFilesPanel.Controls.Add(Program.MasterForm.createPageList[i]);
            }
        }

        public static void CreateModpackBtn_Click(object sender, EventArgs e)
        {
            string modpackName = Program.MasterForm.modpackName_txt.Text;
            IEnumerable<Panel> modFilesList = Program.MasterForm.createFilesPanel.Controls.OfType<Panel>();
            if (modFilesList.Count() == 0)
            {
                Utility.ShowMsg("Please add at least one modded file entry", "Error");
                return;
            }
            if (String.IsNullOrEmpty(modpackName))
            {
                Utility.ShowMsg("Please enter a modpack name", "Error");
                return;
            }

            List<String> chk = new List<string>();
            List<Dictionary<string, string>> fileMap = new List<Dictionary<string, string>>();
            foreach (Panel row in modFilesList)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>
                {
                    ["src"] = row.GetChildAtPoint(Config.sourceTextBoxPoint).Text,
                    ["dest"] = row.GetChildAtPoint(Config.destTextBoxPoint).Text
                };
                if (string.IsNullOrEmpty(dict["src"]) || string.IsNullOrEmpty(dict["dest"]))
                {
                    Utility.ShowMsg("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(dict["src"]))
                {
                    Utility.ShowMsg("The source file '" + dict["src"] + "' does not exist.", "Error");
                    return;
                }
                if (!dict["dest"].StartsWith(Config.MCC_home))
                {
                    Utility.ShowMsg("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }

                // make modpack compatable with any MCC_home directory
                dict["dest"] = CompressPath(dict["dest"]);

                fileMap.Add(dict);
                chk.Add(row.GetChildAtPoint(Config.destTextBoxPoint).Text);
            }

            if (chk.Distinct().Count() != chk.Count())
            {
                Utility.ShowMsg("You have multiple files trying to write to the same destination.", "Error");
                return;
            }

            EnsureModpackFolderExists();
            String modpackFilename = modpackName + ".zip";
            String zipPath = Config.modpack_dir + @"\" + modpackFilename;
            if (File.Exists(zipPath))
            {
                Utility.ShowMsg("A modpack with that name already exists.", "Error");
                return;
            }

            Program.MasterForm.PBar_show(fileMap.Count());
            try
            {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var entry in fileMap)
                    {
                        Program.MasterForm.PBar_update();
                        String fileName = Path.GetFileName(entry["src"]);
                        archive.CreateEntryFromFile(entry["src"], fileName);    // TODO: Fix issues when two source files have same name but diff path
                                                                                // change src path to just modpack after archive creation but before json serialization
                        entry["src"] = fileName;
                    }
                    ZipArchiveEntry configFile = archive.CreateEntry("modpack_config.cfg");
                    string json = JsonConvert.SerializeObject(fileMap, Formatting.Indented);
                    using (StreamWriter writer = new StreamWriter(configFile.Open()))
                    {
                        writer.WriteLine(json);
                    }
                    ZipArchiveEntry readmeFile = archive.CreateEntry("README.txt");
                    using (StreamWriter writer = new StreamWriter(readmeFile.Open()))
                    {
                        writer.WriteLine("Install using MCC Mod Manager: https://github.com/executionByFork/MCC_Mod_Manager/tree/master");
                    }
                }
            }
            catch (NotSupportedException)
            {
                Utility.ShowMsg("The modpack name you have provided is not a valid filename on Windows.", "Error");
                return;
            }

            Utility.ShowMsg("Modpack '" + modpackFilename + "' created.", "Info");
            Program.MasterForm.PBar_hide();
            ResetCreateModpacksTab();
            LoadModpacks();
            return;
        }

        public static void ClearBtn_Click(object sender, EventArgs e)
        {
            ResetCreateModpacksTab();
        }
        #endregion

        #region Api Functions

        public static bool LoadModpacks()
        {
            EnsureModpackFolderExists();
            Program.MasterForm.modListPanel.Controls.Clear();

            string[] fileEntries = Directory.GetFiles(Config.modpack_dir);
            foreach (string file in fileEntries)
            {
                int currentCbCount = Program.MasterForm.modListPanel.Controls.OfType<CheckBox>().Count();
                string modpackName = Path.GetFileName(file).Replace(".zip", "");
                CheckBox chb = new CheckBox
                {
                    AutoSize = true,
                    Text = Config.dirtyPadding + modpackName,
                    Location = new Point(60, (currentCbCount * 20) + 1),
                    Checked = Config.IsPatched(modpackName)
                };
                PictureBox p = new PictureBox
                {
                    Width = 15,
                    Height = 15,
                    Location = new Point(15, (currentCbCount * 20) + 1),
                    Image = Config.IsPatched(modpackName) ? Properties.Resources.greenDot_15px : Properties.Resources.redDot_15px
                };
                if (Program.MasterForm.manualOverride.Checked)
                {
                    p.Click += ForceModpackState;
                    p.MouseEnter += Program.MasterForm.BtnHoverOn;
                    p.MouseLeave += Program.MasterForm.btnHoverOff;
                }

                Program.MasterForm.modListPanel.Controls.Add(p);
                Program.MasterForm.modListPanel.Controls.Add(chb);
            }

            return true;
        }

        public static bool VerifyExists(string modpackname)
        {
            if (File.Exists(Config.modpack_dir + @"\" + modpackname + ".zip"))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Helper Functions
        public static void ResetCreateModpacksTab()
        {
            Program.MasterForm.createFilesPanel.Controls.Clear();
            Program.MasterForm.createPageList = new List<Panel>(); // garbage collector magic
            Program.MasterForm.modpackName_txt.Text = "";
        }

        private static bool EnsureModpackFolderExists()
        {
            if (!Directory.Exists(Config.modpack_dir))
            {
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
            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"  // using the GUID to access 'This PC' folder
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).Parent.GetChildAtPoint(Config.sourceTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static void Create_fileBrowse2(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = false,    // allow modpack creators to type in a filename for creating new files
                InitialDirectory = Config.MCC_home
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).Parent.GetChildAtPoint(Config.destTextBoxPoint).Text = ofd.FileName;
            }
        }

        private static void ForceModpackState(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            string modpackname = ((CheckBox)p.Parent.GetChildAtPoint(new Point(p.Location.X + 45, p.Location.Y))).Text.Replace(Config.dirtyPadding, "");

            if (Config.IsPatched(modpackname))
            {
                Config.RmPatched(modpackname);
                p.Image = Properties.Resources.redDot_15px;
            }
            else
            {
                Config.AddPatched(modpackname);
                p.Image = Properties.Resources.greenDot_15px;
            }

            Config.SaveCfg();
        }
        #endregion
    }
}
