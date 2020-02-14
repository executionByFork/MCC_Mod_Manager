using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCC_Mod_Manager {
    static class IO {
        public static Form1 form1;  // this is set on form load

        public static bool DeleteFile(string path) {
            try {
                File.Delete(path);
            } catch (IOException) {
                return false;
            } catch (UnauthorizedAccessException) {
                return false;
            }
            return true;
        }

        public static int CopyFile(string src, string dest, bool overwrite) {
            //TODO: check source file exists before deleting the destination file
            if (File.Exists(dest)) {
                if (overwrite) {
                    if (!DeleteFile(dest)) {
                        return 2;   // fail - file in use
                    }
                } else {
                    return 1;   // success - not overwriting the existing file
                }
            }
            try {
                File.Copy(src, dest);
            } catch (IOException) {
                return 3;   // fail - file access error
            }
            return 0;   // success
        }

        public static string ReadFirstLine(string filePath) {
            try {
                return File.ReadLines(filePath).First();
            } catch (IOException) {
                return null;
            } catch (UnauthorizedAccessException) {
                return null;
            }
        }

        private static string RetrieveHash(string[] dirArray, int i, Dictionary<string, object> fileTree) {
            if (fileTree.ContainsKey(dirArray[i])) {
                string hash = fileTree[dirArray[i]] as string;
                if (hash == null) { // If object is not a string
                    return RetrieveHash(dirArray, i + 1, JObject.FromObject(fileTree[dirArray[i]]).ToObject<Dictionary<string, object>>());
                }
                return hash;
            }

            return null;
        }

        public static string GetUnmodifiedHash(string filePath) {
            string[] dirArray = filePath.Split(Path.DirectorySeparatorChar);

            string json = File.ReadAllText("Formats/filetree.json");
            Dictionary<string, object> fileTree;
            try {
                fileTree = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            } catch (JsonSerializationException) {
                throw new JsonReaderException();
            } catch (JsonReaderException) {
                return null;
            }

            return RetrieveHash(dirArray, 1, fileTree);
        }

        public static bool IsHaloFile(string filePath) {
            return (GetUnmodifiedHash(filePath) != null);
        }
    }
}
