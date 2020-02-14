using Blamite.Blam;
using Blamite.IO;
using Blamite.Patching;
using Blamite.Serialization;
using Blamite.Serialization.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCC_Mod_Manager {
	static class AssemblyPatching {
		// Majority of the code in this class was pulled from the Assembly project on GitHub and ported to work with my mod manager
		// All of the code in Blamite comes from the Assembly project as well
		// https://github.com/XboxChaos/Assembly

		public static Form1 form1;  // this is set on form load

		private static Patch LoadPatch(string patchFilePath) {
			using (var reader = new EndianReader(File.OpenRead(patchFilePath), Endian.LittleEndian)) {
				string magic = reader.ReadAscii(4);
				reader.SeekTo(0);

				if (magic != "asmp") {
					return null;
				}
				reader.Endianness = Endian.BigEndian;
				return AssemblyPatchLoader.LoadPatch(reader);
			}
		}

		public static bool ApplyPatch(ZipArchiveEntry zippedPatchFile, string patchFileName, string unmoddedMapPath, string outputPath) {
			CreateTmpDir();
			try {
				zippedPatchFile.ExtractToFile(Config.Modpack_dir + @"\tmp\" + patchFileName);
			} catch (IOException) {
				RmTmpDir();
				CreateTmpDir();
				zippedPatchFile.ExtractToFile(Config.Modpack_dir + @"\tmp\" + patchFileName);
			}
			Patch currentPatch = LoadPatch(Config.Modpack_dir + @"\tmp\" + patchFileName);

			// Copy the original map to the destination path
			IO.CopyFile(unmoddedMapPath, outputPath, true); //if modpack has written to unmoddedmap, take from backups

			// Open the destination map
			using (var stream = new EndianStream(File.Open(outputPath, FileMode.Open, FileAccess.ReadWrite), Endian.BigEndian)) {
				EngineDatabase engineDb = XMLEngineDatabaseLoader.LoadDatabase("Formats/Engines.xml");
				ICacheFile cacheFile;
				try {
					cacheFile = CacheFileLoader.LoadCacheFile(stream, engineDb);
				} catch (NotSupportedException nse) {
					form1.showMsg("Error patching '" + patchFileName + "':" + nse.Message, "Error");
					return false;
				}
				if (!string.IsNullOrEmpty(currentPatch.BuildString) && cacheFile.BuildString != currentPatch.BuildString) {
					form1.showMsg("Unable to patch. That patch is for a map with a build version of " + currentPatch.BuildString +
						", and the unmodified map file doesn't match that.", "Error");
					return false;
				}

				if (currentPatch.MapInternalName == null) {
					// Because Ascension doesn't include this, and ApplyPatch() will complain otherwise
					currentPatch.MapInternalName = cacheFile.InternalName;
				}

				// Apply the patch!
				try {
					PatchApplier.ApplyPatch(currentPatch, cacheFile, stream);
				} catch (ArgumentException ae) {
					form1.showMsg("There was an issue applying the patch file '" + patchFileName + "': " + ae.Message, "Info");
					return false;
				}
			}

			RmTmpDir();
			return true;
		}

		private static bool CreateTmpDir() {
			if (!Directory.Exists(Config.Modpack_dir + @"\tmp")) {
				Directory.CreateDirectory(Config.Modpack_dir + @"\tmp");
			}

			return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
		}
		private static bool RmTmpDir() {
			Directory.Delete(Config.Modpack_dir + @"\tmp", true);

			return true;    // C# is dumb. If we dont return something here it 'optimizes' and runs this asynchronously
		}
	}
}
