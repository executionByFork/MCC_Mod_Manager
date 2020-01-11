using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (!Directory.Exists(Config.modpack_dir))
            {
                Directory.CreateDirectory(Config.modpack_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static void create_fileBrowse1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";  // using the GUID for 'This PC' folder
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).Parent.GetChildAtPoint(Config.sourceTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static void create_fileBrowse2(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = false;    // allow modpack creators to type in a filename for creating new files
            ofd.InitialDirectory = Config.MCC_home;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).Parent.GetChildAtPoint(Config.destTextBoxPoint).Text = ofd.FileName;
            }
        }

        public static bool loadModpacks()
        {
            form1.modListPanel_clear();

            string[] fileEntries = Directory.GetFiles(Config.modpack_dir);
            foreach (string file in fileEntries)
            {
                CheckBox chb = new CheckBox();
                chb.AutoSize = true;
                chb.Text = Config.dirtyPadding + Path.GetFileName(file).Replace(".zip", "");
                chb.Location = new Point(30, form1.modListPanel_getCount() * 20);

                form1.modListPanel_add(chb);
            }

            return true;
        }

        public static void createModpack(string modpackName, IEnumerable<Panel> modFilesList)
        {
            if (String.IsNullOrEmpty(modpackName))
            {
                MessageBox.Show("Please enter a modpack name", "Error");
                return;
            }

            List<String> chk = new List<String>();
            List<Dictionary<string, string>> fileMap = new List<Dictionary<string, string>>();
            foreach (Panel row in modFilesList)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["src"] = row.GetChildAtPoint(Config.sourceTextBoxPoint).Text;
                dict["dest"] = row.GetChildAtPoint(Config.destTextBoxPoint).Text;
                if (String.IsNullOrEmpty(dict["src"]) || String.IsNullOrEmpty(dict["dest"]))
                {
                    MessageBox.Show("Filepaths cannot be empty.", "Error");
                    return;
                }
                if (!File.Exists(dict["src"]))
                {
                    MessageBox.Show("The source file '" + dict["src"] + "' does not exist.", "Error");
                    return;
                }
                if (!dict["dest"].StartsWith(Config.MCC_home))
                {
                    MessageBox.Show("Destination files must be located within the MCC install directory. " +
                        "You may need to configure this directory if you haven't done so already.", "Error");
                    return;
                }

                // make modpack compatable with any MCC_home directory
                dict["dest"] = dict["dest"].Replace(Config.MCC_home, "$MCC_home");

                fileMap.Add(dict);
                chk.Add(row.GetChildAtPoint(Config.destTextBoxPoint).Text);
            }

            if (chk.Distinct().Count() != chk.Count())
            {
                MessageBox.Show("You have multiple files trying to write to the same destination.", "Error");
                return;
            }

            ensureModpackFolderExists();
            String modpackFilename = modpackName + ".zip";
            String zipPath = Config.modpack_dir + @"\" + modpackFilename;
            if (File.Exists(zipPath))
            {
                MessageBox.Show("A modpack with that name already exists.", "Error");
                return;
            }


            form1.pBar_show(fileMap.Count());
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var entry in fileMap)
                {
                    form1.pBar_update();
                    String fileName = Path.GetFileName(entry["src"]);
                    archive.CreateEntryFromFile(entry["src"], fileName);    // TODO: Fix issues when two source files have same name but diff path
                    // change src path to just modpack after archive creation but before json serialization
                    entry["src"] = fileName;
                }
                ZipArchiveEntry configFile = archive.CreateEntry("modpackConfig.cfg");
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

            MessageBox.Show("Modpack '" + modpackFilename + "' created.", "Info");
            form1.pBar_hide();
            form1.resetCreateModpacksTab();
            loadModpacks();
            return;
        }

        public static void delModpack(IEnumerable<CheckBox> modpacksList)
        {
            DialogResult ans = MessageBox.Show(
                "Are you sure you want to delete the selected modpacks(s)?\r\nNo crying afterwards?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (ans == DialogResult.No)
            {
                return;
            }

            bool chk = false;
            form1.pBar_show(modpacksList.Count());
            foreach (CheckBox chb in modpacksList)
            {
                form1.pBar_update();
                if (chb.Checked)
                {
                    chk = true;
                    string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                    if (!IO.DeleteFile(Config.modpack_dir + @"\" + modpackname + ".zip"))
                    {
                        MessageBox.Show("Could not delete '" + modpackname + ".zip'. Is the zip file open somewhere?", "Error");
                    }
                    chb.Checked = false;
                }
            }
            if (!chk)
            {
                MessageBox.Show("No items selected from the list.", "Error");
            }
            else
            {
                MessageBox.Show("Selected modpacks have been deleted.", "Info");
                loadModpacks();
            }
            form1.pBar_hide();
        }

        public static void patchModpack(IEnumerable<CheckBox> modpacksList)
        {
            bool baksMade = false;
            bool chk = false;
            bool packErr = false;
            form1.pBar_show(modpacksList.Count());
            foreach (CheckBox chb in modpacksList)
            {
                form1.pBar_update();
                if (chb.Checked)
                {
                    chk = true;
                    string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip"))
                        {
                            ZipArchiveEntry modpackConfigEntry = archive.GetEntry("modpackConfig.cfg");
                            if (modpackConfigEntry == null)
                            {
                                MessageBox.Show("Could not open modpack config file. The file '" + modpackname + ".zip' is not a compatible modpack." +
                                    "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                                packErr = true;
                                continue;
                            }
                            List<Dictionary<string, string>> modpackConfig;
                            using (Stream jsonStream = modpackConfigEntry.Open())
                            {
                                StreamReader reader = new StreamReader(jsonStream);
                                try
                                {
                                    modpackConfig = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(reader.ReadToEnd());
                                }
                                catch (JsonSerializationException)
                                {
                                    MessageBox.Show("The configuration file in '" + modpackname + ".zip' is corrupted." +
                                    "\r\nThis modpack cannot be installed.", "Error");
                                    continue;
                                }
                            }
                            List<string> modpackBakList = new List<string>();   // track patched files in case of failure mid patch
                            foreach (Dictionary<string, string> dict in modpackConfig)
                            {
                                ZipArchiveEntry modFile = archive.GetEntry(dict["src"]);
                                string destination = dict["dest"].Replace("$MCC_home", Config.MCC_home);
                                bool err = false;
                                if (File.Exists(destination))
                                {
                                    if (Backups.createBackup(destination, false) == 0)
                                    {
                                        baksMade = true;
                                    }
                                    if (!IO.DeleteFile(destination))
                                    {
                                        err = true;
                                    }
                                }
                                if (!err)
                                {
                                    try
                                    {
                                        modFile.ExtractToFile(destination);
                                    }
                                    catch (IOException)
                                    {
                                        err = true;     // strange edge case which will *probably* never happen
                                    }
                                }
                                if (err)
                                {
                                    MessageBox.Show("File Access Exception. If the game is running, exit it and try again." +
                                            "\r\nCould not install the '" + modpackname + "' modpack.", "Error");
                                    if (Backups.restoreBaks(modpackBakList) != 0)
                                    {
                                        MessageBox.Show("At least one file restore failed. Your game may be in an unstable state.", "Warning");
                                    }
                                    packErr = true;
                                    break;
                                }
                                modpackBakList.Add(Path.GetFileName(dict["dest"]));
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Could not find the '" + modpackname + "' modpack.", "Error");
                        packErr = true;
                    }
                    chb.Checked = false;
                }
            }

            if (!chk)
            { // fail - no boxes checked
                MessageBox.Show("No modpacks selected.", "Error");
            }
            else if (packErr)
            {   // fail / partial success - At least one modpack was not patched
                MessageBox.Show("One or more of the selected modpacks were not patched to the game.", "Warning");
            }
            else if (baksMade)
            {  // success and new backup(s) created
                MessageBox.Show("The selected mods have been patched to the game.\r\nNew backups were created.", "Info");
            }
            else
            {
                MessageBox.Show("The selected mods have been patched to the game.", "Info");
            }
            form1.pBar_hide();
        }
    }
}
