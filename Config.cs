using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace MCC_Mod_Manager
{
    class Config
    {
        private readonly Form1 form1;
        private readonly string _cfgLocation = @".\MCC_Mod_Manager.cfg";
        private readonly string _bakcfgName = @"\backups.cfg";
        private Dictionary<string, string> _cfg = new Dictionary<string, string>();

        public Config(Form1 caller)
        {
            
            form1 = caller;
            loadCfg();
        }

        public string MCC_home
        {
            get { return _cfg["MCC_home"]; }
            set { _cfg["MCC_home"] = value; }
        }
        public string backup_dir
        {
            get { return _cfg["backup_dir"]; }
            set { _cfg["backup_dir"] = value; }
        }
        public string backupCfg
        {
            get { return _cfg["backup_dir"] + _bakcfgName; }
        }
        public string modpack_dir
        {
            get { return _cfg["modpack_dir"]; }
            set { _cfg["modpack_dir"] = value; }
        }
        public bool deleteOldBaks
        {
            get { return (_cfg["deleteOldBaks"] == "true"); }
            set { _cfg["deleteOldBaks"] = (value) ? "true" : "false"; }
        }

        public bool createDefaultCfg()
        {
            // TODO: Ask user if they want to use default config first
            MCC_home = @"C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection";
            backup_dir = @".\backups";
            modpack_dir = @".\modpacks";
            deleteOldBaks = false;
            saveCfg();

            return true;
        }

        public void saveCfg()
        {
            string json = JsonConvert.SerializeObject(_cfg, Formatting.Indented);
            using (FileStream fs = File.Create(_cfgLocation)) {
                byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        public bool loadCfg()
        {
            bool err = false;
            if (!File.Exists(_cfgLocation)) {
                createDefaultCfg();
            } else {
                string json = File.ReadAllText(_cfgLocation);
                try {
                    Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    MCC_home = values["MCC_home"];
                    backup_dir = values["backup_dir"];
                    modpack_dir = values["modpack_dir"];
                    deleteOldBaks = (values["deleteOldBaks"] == "true");
                } catch (JsonSerializationException) {
                    err = true;
                    createDefaultCfg();
                } catch (KeyNotFoundException) {
                    err = true;
                    createDefaultCfg();
                }
            }
            if (err) {
                MessageBox.Show("There was an error in your configuration file. A default config has been created. Please review and update it if needed.", "Error");
            }

            // Update config tab
            form1.cfgTextBox1Text = MCC_home;
            form1.cfgTextBox2Text = backup_dir;
            form1.cfgTextBox3Text = modpack_dir;
            form1.delOldBaks = deleteOldBaks;

            return true;
        }
    }
}
