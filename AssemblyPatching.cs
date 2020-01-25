using Blamite.Blam;
using Blamite.IO;
using Blamite.Patching;
using Blamite.Serialization;
using Blamite.Serialization.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCC_Mod_Manager
{
    static class AssemblyPatching
    {
		// Majority of the code in this class was pulled from the Assembly project on GitHub and ported to work with my GUI
		// https://github.com/XboxChaos/Assembly

		public static Form1 form1;  // this is set on form load
		private static Patch currentPatch;

		private static void LoadPatch(string patchFilePath)
		{
			using (var reader = new EndianReader(File.OpenRead(patchFilePath), Endian.LittleEndian)) {
				string magic = reader.ReadAscii(4);
				reader.SeekTo(0);

				if (magic == "asmp") {
					reader.Endianness = Endian.BigEndian;
					currentPatch = AssemblyPatchLoader.LoadPatch(reader);
				}
			}
		}

		public static void applyPatch(string patchFilePath, string outputPath)
        {
			LoadPatch(patchFilePath);

			// Copy the original map to the destination path
			//File.Copy(unmoddedMapPath, outputPath, true);

			// Open the destination map
			using (var stream = new EndianStream(File.Open(outputPath, FileMode.Open, FileAccess.ReadWrite), Endian.BigEndian)) {
				EngineDatabase engineDb = XMLEngineDatabaseLoader.LoadDatabase("Formats/Engines.xml");
				ICacheFile cacheFile = CacheFileLoader.LoadCacheFile(stream, engineDb);
				//if (currentPatch.MapInternalName != null && cacheFile.InternalName != currentPatch.MapInternalName) {
				//	MetroMessageBox.Show("Unable to apply patch",
				//		"Hold on there! That patch is for " + currentPatch.MapInternalName +
				//		".map, and the unmodified map file you selected doesn't seem to match that. Find the correct file and try again.");
				//	return;
				//}
				//if (!string.IsNullOrEmpty(currentPatch.BuildString) && cacheFile.BuildString != currentPatch.BuildString) {
				//	MetroMessageBox.Show("Unable to apply patch",
				//		"Hold on there! That patch is for a map with a build version of" + currentPatch.BuildString +
				//		", and the unmodified map file you selected doesn't seem to match that. Find the correct file and try again.");
				//	return;
				//}

				// Apply the patch!
				if (currentPatch.MapInternalName == null)
					currentPatch.MapInternalName = cacheFile.InternalName;
				// Because Ascension doesn't include this, and ApplyPatch() will complain otherwise

				PatchApplier.ApplyPatch(currentPatch, cacheFile, stream);
			}

			form1.showMsg("Your patch has been applied successfully.", "Info");
		}

    }
}
