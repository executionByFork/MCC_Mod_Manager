using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCC_Mod_Manager.Api
{
    static class Utility
    {
        public static DialogResult ShowMsg(string msg, string type)
        {
            if (type == "Info")
            {
                return MessageBox.Show(
                    msg, "Info", MessageBoxButtons.OK,
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1
                );
            }
            else if (type == "Question")
            {
                return MessageBox.Show(
                    msg, "Question", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1
                );
            }
            else if (type == "Warning")
            {
                return MessageBox.Show(
                    msg, "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1
                );
            }
            else if (type == "Error")
            {
                return MessageBox.Show(
                    msg, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1
                );
            }
            throw new FormatException("Please notify the developer: " + type + " is not a valid type for Utility.ShowMsg.");
        }

        #region IO
        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        public static int CopyFile(string src, string dest, bool overwrite)
        {
            //TODO: check source file exists before deleting the destination file
            if (File.Exists(dest))
            {
                if (overwrite)
                {
                    if (!DeleteFile(dest))
                    {
                        return 2;   // fail - file in use
                    }
                }
                else
                {
                    return 1;   // success - not overwriting the existing file
                }
            }
            try
            {
                File.Copy(src, dest);
            }
            catch (IOException)
            {
                return 3;   // fail - file access error
            }
            return 0;   // success
        }
        #endregion
    }
}
