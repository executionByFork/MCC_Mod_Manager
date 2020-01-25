using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCC_Mod_Manager
{
    public static class Program
    {
        public static MasterForm MasterForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MasterForm = new MasterForm();
            Application.Run(MasterForm);
        }
    }
}
