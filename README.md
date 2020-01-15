# A Mod Manager for Halo MCC on PC
**_**NOTE:** This application was developed for Windows PCs with Halo MCC installed via Steam. As such, operating systems other than Windows, and Halo MCC installations not utilizing Steam, are not officially supported. If you [dump the Windows Store version of MCC](https://www.reddit.com/r/halomods/comments/e5tsmu/dumping_the_ms_store_version_of_halo_mcc/) then this tool **should** work as intended, though I am not testing for this and so I can not guarantee it will work flawlessly._

## PURPOSE
This application is intended to both make mod installations easy, and to create a standard format for Halo MCC mods. The tool utilizes 'modpacks', or zip archives containing mod files and a config, to package and deploy Halo MCC mods in a consistent format. It automatically creates a backup of any files getting overwritten if one doesn't yet exist, and it allows painless creation of modpacks from already existing mods. Technically all this application does is copy files back and forth, but it removes the manual process of keeping track of where mod files need to be placed and it maintains a backup archive of Halo files should you need to restore the base game. The main benefit of using this tool is that it establishes a
**standardized format** for any and all Halo mods.

### The benefits of using this tool
- **Modpack creation is simple.**
  - Mod creators can easily bundle their mod(s) into a modpack. All that is required selecting each mod file and where it needs to be placed in order to function properly. Mod creators already add this in their Readme files.
  - All mods can be bundled into a modpack, making all mods compatable with this tool. End users can even bundle mods not using this format into a modpack for their own use, granted that they know where to put the mod files.
  - When a modpack is bundled and zipped, a `readme.txt` file is also created inside the zip folder. Mod makers who want to include manual instructions for installing their mods can add them to this file.
- **All mod files are contained in a `zip` archive. Not `rar`, and not `7zip`.**
  - This is because default Windows only support zip formats. This removes the overhead of users having to find and install 3rd party tools to open zipped archives
  - The mod files are compressed, saving hard drive space
- **No unzipping OR file manipulation is neccessary.**
  - This application extracts the zip file contents in memory. It does not write temporary files to the hard drive, avoiding clutter and saving space.
  - Users do not have to bother with extracting zip files and handling multiple mod files. Everything is contained in a single, easy to handle package.
- **This tool manages backups for you.**
  - You no longer have to worry about creating a backup schema and remembering where you put your backup files and where they need to go. This tool manages all of that for you, reducing the complexity and overhead of using mods safely.
  - Only the files that need to get backed up, are backed up. This tool doesn't simply copy the entire Halo install directory to a separate folder, doubling the install size. Instead, it only creates a backup of files when it needs to, saving space.
- **NO administrator privileges needed**
  - Applications which manage mods by using symbolic links need to run with Administrator privileges, and this poses a security threat to the user. This application does **NOT** need Administrator privileges to run, because it avoids using links.

## INSTALLATION
This application can be downloaded from the [Github Release](https://github.com/executionByFork/MCC_Mod_Manager/releases) page or from [Nexus Mods](https://www.nexusmods.com/halothemasterchiefcollection/mods/185).  
Make sure `MCC Mod Manager.exe` and `Newtonsoft.Json.dll` are in a folder together. The executable will create folders and config files in the same directory, so it's best if these two files are in a folder alone.

If you would like to install from source, you can pull down this repository and open the project in [Visual Studio 2019](https://visualstudio.microsoft.com/vs/). You can create an executable application from that IDE. [Read more here](https://docs.microsoft.com/en-us/visualstudio/ide/building-and-cleaning-projects-and-solutions-in-visual-studio?view=vs-2019).

## USAGE
I have recorded a video on how to use this tool in its entirety [here](https://www.youtube.com/watch?v=wvRcdXpgIos). However, if you prefer reading, details on the application can be found below.

### Top Bar and Buttons
You can drag the window by holding down the left mouse on the light gray bar at the top of the window.  
The red power icon quits the application.  
The black minimize icon minimizes the app.  
The green arrows refreshes the application and reloads the configuration from files. This should typically be done for you,
but it's there if you need it.

### My Mods Tab
This is the primary screen. The large box lists all the modpacks you currently have installed in the `modpacks/` directory.  
'Patch Game' will overwrite the neccesary game files from the ones in the selected modpacks. Be careful, because some modpacks will not be compatable with one another. I plan to add checks for this in a future version, but right now two modpacks that need to modify the same file will overwrite one another.  
The 'Delete Selected' button will delete the modpacks you have checked after confirming that you actuall want to delete them.

### Create Modpack Tab
This is primarily intended for mod makers, but can also be used by end users to create modpacks for mods which do not ship a modpack bundle. The green plus button adds an entry to the modpack. The left text field and browse button can be used to search for the file you would like to add to the modpack. The right text field and browse button is used to select the location the modded file should be placed when a user installs the mod. The far left 'X' button can be used to delete a single entry from the modpack. The name of the modpack can be supplied at the bottom.  
The 'Clear All' button simply clears the above list, avoiding users having to click the 'X' beside every entry.

### Configuration Tab
The app comes with a default configuration which may need to be changed. Different backup and modpack storage directories can be configured here. The MCC Install will need to be set to the root directory of the install. This is typically `C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection` on Steam installs, but the drive letter or path may vary. When updating the main config with the 'Update' button, the app will make a few checks to ensure that the MCC Install Directory was selected correctly. If it detects that the wrong folder was selected, the config won't be saved.

The MCC Install Directory here is very important. When this tool creates or tries to install a modpack, it substitutes the value of the configured folder with a variable (`$MCC_home`) when writing to the individual modpack configs. This allows the modpack to function between users with different install directories. For example, Alice has Halo installed on her `C:\` drive, and Bob has Halo installed on his `F:\` drive. Upon install of a mod, this tool will use their indivdually configured MCC Install Directory as a starting point before locating where the file needs to go. This allows the same modpack to be installed on both systems without any extra overhead.

### Backups Tab
This tool manages backups for you, however, there are times you may need to remove old backups or will want to create new ones. Additionally, if you want to restore your game's original functionality this is where you can do that. The large box displays all the file names which are currently backed up in the backup directory.  
'New Backup' can be used to create a new backup of a file. If you select a file that is already backed up, it will ask if you want to overwrite it.  
The 'Restore Selected' and 'Restore All Files' buttons restore from backups the selected files/all files, respectively.  
The 'Delete Selected' and 'Delete All Backups' buttons remove the selected/all backup files, respectively, after confirming that you actually want to delete them.  

## Known Issues
- Two modpacks which overwrite the same files will overwrite one another.
- The progress bar doesn't update properly when creating modpacks
