using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;

namespace MCC_Mod_Manager
{
    static class Backups
    {
        public static Form1 form1;  // this is set on form load
        public static Dictionary<string, string> _baks = new Dictionary<string, string>();

        public static bool ensureBackupFolderExists()
        {
            if (!Directory.Exists(Config.backup_dir))
            {
                Directory.CreateDirectory(Config.backup_dir);
            }

            return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
        }

        public static bool saveBackups()
        {
            string json = JsonConvert.SerializeObject(Backups._baks, Formatting.Indented);
            using (FileStream fs = File.Create(Config.backupCfg))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
            return true;
        }

        public static bool updateBackupList()
        {
            form1.bakListPanel_clear();
            foreach (KeyValuePair<string, string> entry in Backups._baks)
            {
                CheckBox chb = new CheckBox();
                chb.AutoSize = true;
                chb.Text = Config.dirtyPadding + entry.Key;
                chb.Location = new Point(30, form1.bakListPanel_getCount() * 20);

                form1.bakListPanel_add(chb);
            }
            return true;
        }

        public static bool loadBackups()
        {
            if (!File.Exists(Config.backupCfg))
            {
                return false;
            }

            string json = File.ReadAllText(Config.backupCfg);
            try
            {
                Backups._baks = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                updateBackupList();
            }
            catch (JsonSerializationException)
            {
                DialogResult ans = MessageBox.Show(
                    "The backup configuration file is corrupted. You may need to verify your game files on steam or reinstall." +
                    "Would you like to delete the corrupted backup config file?",
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (ans == DialogResult.Yes)
                {
                    if (!IO.DeleteFile(Config.backupCfg))
                    {
                        MessageBox.Show("The backup file could not be deleted. Is it open somewhere?", "Error");
                    }
                }
            }

            return true;
        }

        public static int createBackup(string path, bool overwrite)
        {
            String fileName = Path.GetFileName(path);
            int res = IO.CopyFile(path, Config.backup_dir + @"\" + fileName, overwrite);
            if (res == 0 || res == 1)
            {
                Backups._baks[fileName] = path;
                saveBackups();
                updateBackupList();
            }
            return res;
        }

        public static int restoreBaks(List<string> backupNames)
        {
            if (backupNames.Count() == 0)
            {
                return 0;
            }

            form1.pBar_show(backupNames.Count());
            bool chk = false;
            bool err = false;
            foreach (string fileName in backupNames)
            {
                form1.pBar_update();
                if (IO.CopyFile(Config.backup_dir + @"\" + fileName, Backups._baks[fileName], true) == 0)
                {
                    if (Config.deleteOldBaks)
                    {
                        if (IO.DeleteFile(Config.backup_dir + @"\" + fileName))
                        {
                            Backups._baks.Remove(fileName);
                        }
                        else
                        {
                            MessageBox.Show("Could not remove old backup '" + fileName + "'. Is the file open somewhere?", "Error");
                        }
                    }
                    chk = true;
                }
                else
                {
                    MessageBox.Show("Could not restore '" + fileName + "'. If the game is open, close it and try again.", "Error");
                    err = true;
                }
            }
            Backups.saveBackups();
            Backups.updateBackupList();
            form1.pBar_hide();
            if (chk)
            {
                if (err)
                {
                    return 1;   // Partial success - Some files were restored
                }
                return 0;   // Success - All files were restored
            }
            return 2;   // Failure - No files were restored
        }
    }
}
