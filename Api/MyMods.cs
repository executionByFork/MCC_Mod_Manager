using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MCC_Mod_Manager.Api;
using Newtonsoft.Json;

namespace MCC_Mod_Manager
{
    public partial class MyMods
    {
        #region Event Handlers

        public static void ManualOverride_CheckedChanged(object sender, EventArgs e)
        {
            if (Program.MasterForm.manualOverride.Checked == false)
            {   // make warning only show if checkbox is getting enabled
                return;
            }

            DialogResult ans = Utility.ShowMsg("Please do not mess with this unless you know what you are doing or are trying to fix a syncing issue.\r\n\r\n" +
                "This option allows you to click the red/green icons beside modpack entries to force the mod manager to flag a modpack as enabled/disabled. " +
                "This does not make changes to files, but it does make the mod manager 'think' that modpacks are/aren't installed. If the game was just patched, " +
                "you should use the 'Reset App' button in the Config tab instead.\r\n\r\nEnable this feature?", "Question");
            if (ans == DialogResult.No)
            {
                Program.MasterForm.manualOverride.Checked = false;
                return;
            }

            Modpacks.LoadModpacks();
        }

        public static void SelectEnabled_chb_CheckedChanged(object sender, EventArgs e)
        {
            foreach (CheckBox chb in Program.MasterForm.modListPanel.Controls.OfType<CheckBox>())
            {
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (Config.IsPatched(modpackname))
                {
                    chb.Checked = ((CheckBox)sender).Checked;
                }
            }
        }


        public static void PatchUnpatch_Click(object sender, EventArgs e)
        {
            IEnumerable<CheckBox> modpacksList = Program.MasterForm.modListPanel.Controls.OfType<CheckBox>();
            List<CheckBox> toPatch = new List<CheckBox>();
            List<CheckBox> toUnpatch = new List<CheckBox>();
            foreach (CheckBox chb in modpacksList)
            {
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                if (chb.Checked && !Config.IsPatched(modpackname))
                {
                    toPatch.Add(chb);
                }
                else if (!chb.Checked && Config.IsPatched(modpackname))
                {
                    toUnpatch.Add(chb);
                }
            }

            if (toPatch.Count() == 0 && toUnpatch.Count() == 0)
            {
                Utility.ShowMsg("You did not select any changes. No modpacks were patched or unpatched.", "Info");
                return;
            }

            // Unpatch mods before trying to patch new ones
            int retU = UnpatchModpacks(toUnpatch);
            int retP = PatchModpacks(toPatch);

            Config.SaveCfg();

            if (retU == 2 || retP == 2)
            {   // fail / partial success - At least one modpack was not patched
                Utility.ShowMsg("Failed in patching/unpatching at least one modpack.", "Warning");
            }
            else if (retP == 1)
            {  // success and new backup(s) created
                Utility.ShowMsg("The game has been updated with your modpack selection.\r\nNew backups were created.", "Info");
            }
            else
            {    // success, no new backups (retU == 0 && retP == 0)
                Utility.ShowMsg("The game has been updated with your changes.", "Info");
            }
        }

        public static void DeleteSelected_Click(object sender, EventArgs e)
        {
            IEnumerable<CheckBox> modpacksList = Program.MasterForm.modListPanel.Controls.OfType<CheckBox>();
            bool chk = false;
            bool del = false;
            bool partial = false;
            Program.MasterForm.pBar_show(modpacksList.Count());
            foreach (CheckBox chb in modpacksList)
            {
                Program.MasterForm.pBar_update();
                if (chb.Checked)
                {
                    if (!chk)
                    { // only prompt user once
                        DialogResult ans = Utility.ShowMsg("Are you sure you want to delete the selected modpacks(s)?\r\nNo crying afterwards?", "Question");
                        if (ans == DialogResult.No)
                        {
                            Program.MasterForm.pBar_hide();
                            return;
                        }
                    }
                    chk = true;

                    string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                    if (Config.IsPatched(modpackname))
                    {    // deliberately prompt for each modpack that is enabled
                        DialogResult ans = Utility.ShowMsg("WARNING: The " + modpackname + " modpack is showing as currently installed. " +
                            "Deleting this modpack will also unpatch it from the game. Continue?", "Question");
                        if (ans == DialogResult.No)
                        {
                            partial = true;
                            continue;
                        }
                        else
                        {
                            if (UnpatchModpack(modpackname) == 2)
                            {
                                partial = true;
                                continue;
                            }
                            Config.RmPatched(modpackname);
                        }
                    }

                    if (!Utility.DeleteFile(Config.modpack_dir + @"\" + modpackname + ".zip"))
                    {
                        Utility.ShowMsg("Could not delete '" + modpackname + ".zip'. Is the zip file open somewhere?", "Error");
                    }
                    del = true;
                    chb.Checked = false;
                }
            }
            if (!chk)
            {
                Utility.ShowMsg("No items selected from the list.", "Error");
            }
            else if (!del)
            {
                Utility.ShowMsg("No modpacks were deleted.", "Warning");
            }
            else if (del && partial)
            {
                Utility.ShowMsg("Only some of the selected modpacks have been deleted.", "Warning");
                Modpacks.LoadModpacks();
            }
            else
            {
                Utility.ShowMsg("Selected modpacks have been deleted.", "Info");
                Modpacks.LoadModpacks();
            }
            Config.SaveCfg();
            Program.MasterForm.pBar_hide();
        }

        #endregion

        #region Api Functions

        #region Patch
        private static int PatchModpacks(List<CheckBox> toPatch)
        {
            bool baksMade = false;
            bool packErr = false;
            Program.MasterForm.pBar_show(toPatch.Count());
            foreach (CheckBox chb in toPatch)
            {
                Program.MasterForm.pBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = PatchModpack(modpackname);
                if (ret == 2)
                {
                    packErr = true;
                    chb.Checked = false;
                }
                else
                {    // modpack was patched
                    Config.AddPatched(modpackname);
                    ((PictureBox)chb.Parent.GetChildAtPoint(new Point(chb.Location.X - 45, chb.Location.Y))).Image = Properties.Resources.greenDot_15px;
                }
            }

            Program.MasterForm.pBar_hide();
            if (packErr)
            {   // fail / partial success - At least one modpack was not patched
                return 2;
            }
            else if (baksMade)
            {  // success and new backup(s) created
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static int PatchModpack(string modpackname)
        {
            string retStr = WillOverwriteOtherMod(modpackname);
            if (!String.IsNullOrEmpty(retStr))
            {
                Utility.ShowMsg("Installing '" + modpackname + "' would overwrite files for '" + retStr + "'. Modpack will be skipped.", "Error");
                return 2;
            }

            bool baksMade = false;
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip"))
                {
                    List<Dictionary<string, string>> modpackConfig = GetModpackConfig(archive);
                    if (modpackConfig == null)
                    {
                        Utility.ShowMsg("The file '" + modpackname + ".zip' is either not a compatible modpack or the config is corrupted." +
                            "\r\nTry using the 'Create Modpack' Tab to convert this mod into a compatible modpack.", "Error");
                        return 2;
                    }

                    List<string> patched = new List<string>();   // track patched files in case of failure mid patch
                    foreach (Dictionary<string, string> dict in modpackConfig)
                    {
                        int r = PatchFile(archive, dict["src"], dict["dest"]);
                        if (r == 2 || r == 3)
                        {
                            string errMsg;
                            if (r == 2)
                            {
                                errMsg = "File Access Exception. If the game is running, exit it and try again.";
                            }
                            else
                            {    // r == 3
                                errMsg = "This modpack appears to be missing files.";
                            }
                            Utility.ShowMsg(errMsg + "\r\nCould not install the '" + modpackname + "' modpack.", "Error");

                            if (Backups.restoreBaks(patched) != 0)
                            {
                                Utility.ShowMsg("At least one file restore failed. Your game is likely in an unstable state.", "Warning");
                            }
                            return 2;
                        }
                        else if (r == 1)
                        {
                            baksMade = true;
                        }

                        patched.Add(ExpandPath(dict["dest"]));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Utility.ShowMsg("Could not find the '" + modpackname + "' modpack.", "Error");
                return 2;
            }
            catch (InvalidDataException)
            {
                Utility.ShowMsg("The modpack '" + modpackname + ".zip' appears corrupted." +
                "\r\nThis modpack cannot be installed.", "Error");
                return 2;
            }

            if (baksMade)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static string WillOverwriteOtherMod(string modpack)
        {
            List<Dictionary<string, string>> primaryConfig;
            using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpack + ".zip"))
            {
                primaryConfig = GetModpackConfig(archive);
            }

            foreach (string enabledModpack in Config.patched)
            {
                List<Dictionary<string, string>> modpackConfig;
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + enabledModpack + ".zip"))
                {
                    modpackConfig = GetModpackConfig(archive);
                }
                // Deliberately not checking for null so program throws a stack trace
                // This should never happen, but if it does I want the user to let me know about it
                foreach (Dictionary<string, string> dict in modpackConfig)
                {
                    foreach (Dictionary<string, string> primary in primaryConfig)
                    {
                        if (dict["dest"] == primary["dest"])
                        {
                            return enabledModpack;
                        }
                    }
                }
            }

            return null;
        }
        #endregion

        #region Unpatch
        private static int UnpatchModpacks(List<CheckBox> toUnpatch)
        {
            bool packErr = false;
            Program.MasterForm.pBar_show(toUnpatch.Count());
            foreach (CheckBox chb in toUnpatch)
            {
                Program.MasterForm.pBar_update();
                string modpackname = chb.Text.Replace(Config.dirtyPadding, "");
                int ret = UnpatchModpack(modpackname);
                if (ret == 2)
                {
                    packErr = true;
                }
                else
                {    // modpack was patched
                    Config.RmPatched(modpackname); // modpack was unpatched
                    ((PictureBox)chb.Parent.GetChildAtPoint(new Point(chb.Location.X - 45, chb.Location.Y))).Image = Properties.Resources.redDot_15px;
                }
                chb.Checked = false;
            }

            Program.MasterForm.pBar_hide();
            if (packErr)
            { // fail / partial success - At least one modpack was not patched
                return 2;
            }
            else
            {    // success, no errors
                return 0;
            }
        }

        private static int UnpatchModpack(string modpackname)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(Config.modpack_dir + @"\" + modpackname + ".zip"))
                {
                    List<Dictionary<string, string>> modpackConfig = GetModpackConfig(archive);
                    if (modpackConfig == null)
                    {
                        Utility.ShowMsg("Could not unpatch '" + modpackname + "' because the modpack's configuration is corrupted or missing." +
                            "\r\nPlease manually restore from backups or verify integrity of game files on Steam.", "Error");
                        return 2;
                    }
                    List<Dictionary<string, string>> restored = new List<Dictionary<string, string>>(); // track restored files in case of failure mid unpatch
                    foreach (Dictionary<string, string> dict in modpackConfig)
                    {
                        if (!Backups.restoreBak(ExpandPath(dict["dest"])))
                        {
                            // repatch restored mod files
                            foreach (Dictionary<string, string> entry in restored)
                            {
                                int r = PatchFile(archive, entry["src"], entry["dest"]);
                                if (r == 2 || r == 3)
                                {
                                    Utility.ShowMsg("Critical error encountered while unpatching '" + modpackname + "'." +
                                        "\r\nYou may need to verify your game files on steam or reinstall.", "Error");
                                }
                            }
                            return 2;
                        }
                        restored.Add(dict);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Utility.ShowMsg("Could not unpatch '" + modpackname + "'. Could not find the modpack file to read the config from.", "Error");
                return 2;
            }
            catch (InvalidDataException)
            {
                Utility.ShowMsg("Could not unpatch '" + modpackname + "'. The modpack file appears corrupted and the config cannot be read.", "Error");
                return 2;
            }

            return 0;
        }
        #endregion

        #region Helper Functions
        private static List<Dictionary<string, string>> GetModpackConfig(ZipArchive archive)
        {
            ZipArchiveEntry modpackConfigEntry = archive.GetEntry("modpack_config.cfg");
            if (modpackConfigEntry == null)
            {
                return null;
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
                    return null;
                }
                catch (JsonReaderException)
                {
                    return null;
                }
            }

            return modpackConfig;
        }

        private static int PatchFile(ZipArchive archive, string src, string dest)
        {
            string destination = ExpandPath(dest);
            bool baksMade = false;
            ZipArchiveEntry modFile = archive.GetEntry(src);
            if (modFile == null)
            {
                return 3;
            }

            if (File.Exists(destination))
            {
                if (Backups.createBackup(destination, false) == 0)
                {
                    baksMade = true;
                }
                if (!Utility.DeleteFile(destination))
                {
                    return 2;
                }
            }

            try
            {
                modFile.ExtractToFile(destination);
            }
            catch (IOException)
            {
                return 2;
            }

            if (baksMade)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static string ExpandPath(string p)
        {
            return p.Replace("$MCC_home", Config.MCC_home);
        }
        #endregion

        #endregion
    }
}
