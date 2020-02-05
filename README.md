# A Mod Manager for Halo MCC on PC
\*\*_**NOTE:** This application was developed for Windows PCs with Halo MCC installed via Steam. As such, operating systems other than Windows, and Halo MCC installations not utilizing Steam, are not officially supported. If you [dump the Windows Store version of MCC](https://www.reddit.com/r/halomods/comments/e5tsmu/dumping_the_ms_store_version_of_halo_mcc/) then this tool **should** work as intended, though I am not testing for this and so I can not guarantee it will work flawlessly._

\*\**Credit is due to the makers of Assembly and specifically the Blamite library, which this tool uses to support Assembly patch files. Thanks to [Zeddikins](https://github.com/Lord-Zedd), [adierking](https://github.com/adierking), and [0xdeafcafe](https://github.com/0xdeafcafe) for allowing me to use their library to add support.* 

## Table of Contents
- [Purpose of the application](https://github.com/executionByFork/MCC_Mod_Manager#purpose)
- [Usage guide](https://github.com/executionByFork/MCC_Mod_Manager#usage)
- [Installation instructions](https://github.com/executionByFork/MCC_Mod_Manager#installation)
- [Known Issues](https://github.com/executionByFork/MCC_Mod_Manager#known-issues)

## PURPOSE
This application is intended to both make mod installations easy, and to create a standard format for Halo MCC mods. The tool utilizes 'modpacks', or zip archives containing mod files and a config, to package and deploy Halo MCC mods in a consistent format. It automatically creates a backup of any files getting overwritten if one doesn't yet exist, and it allows painless creation of modpacks from already existing mods. Technically all this application does is copy files back and forth, but it removes the manual process of keeping track of where mod files need to be placed and it maintains a backup archive of Halo files should you need to restore the base game. The main benefit of using this tool is that it establishes a
**standardized format** for any and all Halo mods.

### The benefits of using this tool
- **Modpack creation is simple.**
  - Mod creators can easily bundle their mod(s) into a modpack. All that is required is selecting each mod file and where it needs to be placed in order to function properly. Mod creators already add this in their readme files.
  - All mods can be bundled into a modpack, making all mods compatable with this tool. End users can even bundle mods not using this format into a modpack for their own use, granted that they know where to put the mod files.
  - Assembly `.asmp` files are supported in modpacks, meaning that modpacks can be small and easy to download without the hassle of teaching users how to patch a file correctly with Assembly.
  - When a modpack is bundled and zipped, a `readme.txt` file is also created inside of the zip folder. Mod makers who want to include manual instructions for installing their mods can add them to this file.
- **All mod files are contained in a `zip` archive. Not `rar`, and not `7zip`.**
  - This is because default Windows only support zip formats. This removes the overhead of users having to find and install 3rd party tools to open zipped archives
  - The mod files are compressed, saving hard drive space
- **No unzipping OR file manipulation is neccessary.**
  - This application extracts the zip file contents in memory. It avoids writing files to the hard drive, avoiding clutter and saving space.
  - Users do not have to bother with extracting zip files and handling multiple mod files. Everything is contained in a single, easy to handle package.
- **This tool manages backups for you.**
  - You no longer have to worry about creating a backup schema and remembering where you put your backup files and where they need to go. This tool manages all of that for you, reducing the complexity and overhead of using mods safely.
  - Only the files that *need* to get backed up are backed up. This tool doesn't simply copy the entire Halo install directory to a separate folder, doubling the install size. Instead, it only creates a backup of files when it needs to, and it keeps track of where they need to go for you.
- **NO administrator privileges needed**
  - Applications which manage mods by using symbolic links need to run with Administrator privileges, and this poses a security threat to the user. This application does **NOT** need Administrator privileges to run, because it avoids using symlinks.

## USAGE
I have recorded a video on how to use this tool in its entirety [here](https://www.youtube.com/watch?v=wvRcdXpgIos). However, although most of it still applies, it is somewhat out of date now. I will be making a new video soon. The details listed below are a fully up to date.

### Top Bar and Buttons
- You can drag the window by holding down the left mouse on the light gray bar at the top of the window.  
- The red power icon quits the application.  
- The black minimize icon minimizes the app.  
- The green arrows refreshes the application and reloads the configuration from files. This should typically be done for you, but it's there if you need it.  
- If you see a yellow caution sign at the top bar, this indicates an issue with the mod manager or game state that needs to be resolved. Hover over the icon for more details.

### My Mods Tab
- This is the primary screen. The large box lists all the modpacks you currently have installed in the `modpacks/` directory.  
- A red dot indicates that the modpack is not currently installed, while a green dot indicates that it is installed.  
- If you see a yellow caution sign beside a modpack it is a warning. Hover over it for more details.  
- The checkbox beside a modpack can be used to select it.  
- The 'Select Enabled' checkbox will select or deselect all modpacks which are listed as enabled (green dot).  
- 'Patch/Unpatch' will patch the selected modpacks to the game if they are not yet patched, and unpatch the currently enabled modpacks which are not checked. The app will make sure you are not overwriting a mod before taking action, so you don't have to worry about clobbering anything.  
- The 'Delete Selected' button will ask for comfirmation, then uninstall the modpack(s) if necessary before deleting them.  
- The 'Allow Manual Override' option should be used with care. This is meant to fix syncing issues, and can be used to set the mod manager to think that a mod is or isn't currently installed. It does not modify game files.

### Create Modpack Tab
\*\**This is primarily intended for mod makers, but can also be used by end users to create modpacks for mods which do not ship a modpack bundle.*  
- The green plus button allows you to select modded files to add to the modpack.  
- The left text fields contain the modded files and the browse button beside them can be used to change the file.  
- The right text field and browse button is used to select the location the modded file should be placed when a user installs the mod.  
- If the modded file is an `.asmp` file, a third text box and button will show below the first on the left. This defines the path to the original, unmodified map file that the patch will be appiled to when installed by the user.  
- The far left 'X' button can be used to delete a single entry from the modpack. The name of the modpack can be supplied at the bottom.  
- The 'Clear All' button simply clears the above list and modpack name, avoiding users having to click the 'X' beside every entry.

### Configuration Tab
\*\**Remember to hit update to save the config changes*  
- The app comes with a default configuration which may need to be changed. Different backup and modpack storage directories can be configured here. The MCC Install will need to be set to the root directory of the install. This is typically `C:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection` on Steam installs, but the drive letter or path may vary. When updating the main config with the 'Update' button, the app will make a few checks to ensure that the MCC Install Directory was selected correctly. If it detects that the wrong folder was selected, the config won't be saved.
- There is an option in the config to delete backups after restoring them. This is enabled by default, and is meant to save space on the computer. If you would like to retain the backups even after they are restored, you may uncheck this box.  
- The 'Reset App' button should be used with caution. Its purpose is to reset the application state to default if the state gets out of sync. This typically should not happen with normal use, however if you are manually modifying files alongside using the mod manager, issues may arise. 'Reset App' assumes that all modpacks are uninstalled and sets them as such, then deletes all backups.

- The MCC Install Directory here is very important. When this tool creates or tries to install a modpack, it substitutes the value of the configured folder with a variable (`$MCC_home`) when writing to the individual modpack configs. This allows the modpack to function between users with different install directories. For example, Alice has Halo installed on her `C:\` drive, and Bob has Halo installed on his `F:\` drive. Upon install of a mod, this tool will use their indivdually configured MCC Install Directory as a starting point before locating where the file needs to go. This allows the same modpack to be installed on both systems without any extra overhead.

### Backups Tab
- This tool manages backups for you, however, there are times you may need to remove old backups or will want to create new ones. Additionally, if you want to restore your game's original functionality this is where you can do that. The large box displays all the file names which are currently backed up in the backup directory.  
- The 'Show full path' option allows you to toggle the backup window between displaying the entire backup path or only the filename.  
- 'New Backup' can be used to create a new backup of a file. If you select a file that is already backed up, it will ask if you want to overwrite it.  
- The 'Restore Selected' and 'Restore All Files' buttons restore from backups the selected files/all files, respectively.  
- The 'Delete Selected' and 'Delete All Backups' buttons remove the selected/all backup files, respectively, after confirming that you actually want to delete them.  

## INSTALLATION
This application can be downloaded from the [Github Release](https://github.com/executionByFork/MCC_Mod_Manager/releases) page or from [Nexus Mods](https://www.nexusmods.com/halothemasterchiefcollection/mods/185).  
Make sure `MCC Mod Manager.exe`, `Blamite.dll`, `Newtonsoft.Json.dll`, and the `Formats/` folder are in the same folder together. The executable will create folders and config files in the same directory, so it's best if these files are in a folder alone.

### Installation From Source
Clone this repository.  
`git clone https://github.com/executionByFork/MCC_Mod_Manager.git`  
Clone [Assembly](https://github.com/XboxChaos/Assembly) source.  
`git clone https://github.com/XboxChaos/Assembly.git`  
Switch to the latest public release (As of 2/4/2020, this commit: [249d4ac3](https://github.com/XboxChaos/Assembly/tree/249d4ac3b4a4e85ee2cd934bdb7122a590007d30)) using a temporary branch to avoid a detached HEAD state.    
`git checkout -b tmp 249d4ac3b4a4e85ee2cd934bdb7122a590007d30`  
Open the Assembly project in [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) (If it asks if you want to retarget the project, you may select yes). Then right click the "Blamite" project in Solution Explorer and select "Build".  
Once built, go to `Assembly\src\Blamite\bin\x64\Debug` and copy the `Blamite.dll` and `Formats/` folder into `MCC_Mod_Manager\bin\Debug`.  
In the `Formats/` folder in the new location, you may optionally delete every folder except for `ReachMCC/`. If you do this, you must also edit `Engines.xml`. Open `Engines.xml` and delete every `<engine/>` tag (plus their contents) except for the engine tag labeled "Halo: Reach MCC". Save the file.  
Clone or download my [jsonifyFileTree](https://github.com/executionByFork/jsonifyFileTree) project.  
`git clone https://github.com/executionByFork/jsonifyFileTree.git`  
`cd` into the folder and run the perl script as follows:  
`./jsonifyFileTree.pl --hash [MCC_INSTALL_DIRECTORY] > filetree.json`, replacing `[MCC_INSTALL_DIRECTORY]` with the path to the root of your Halo MCC install directory (`Halo The Master Chief Collection/`).  
Once `filetree.json` is created, move it to `MCC_Mod_Manager\bin\Debug\Formats`.  
Open the MCC Mod Manager project in [Visual Studio 2019](https://visualstudio.microsoft.com/vs/). Go to `Project` > `Manage NuGet Packages` > `Browse`, then search for and install the following dependencies:
- Newtonsoft.Json (v12.0.3+)
- System.IO.Compression (v4.3.0+)  
You may now click the "Start" button at the top to build `MCC Mod Manager.exe`. Copy `MCC Mod Manager.exe`, `Blamite.dll`, `Newtonsoft.Json.dll`, and the `Formats/` folder into a new folder of your choosing. All other files will be generated by the app and can be ignored.

## Known Issues
- Patching a modpack which includes at least one `.asmp` file will cause a memory leak. This is an issue in the underlying Blamite library that MCC Mod Manager uses ([more info here](https://github.com/XboxChaos/Assembly/issues/268)). As a workaround, close and reopen the mod manager to free up the memory.
- Restoring from backups using the backup tab will not change the status of whether or not a mod is installed
- Creating a modpack where two source files have the same name will cause problems on install
