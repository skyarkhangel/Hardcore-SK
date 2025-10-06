# Manual of using the whole repo in game root folder

Current .gitignore has already included all files, folders and everything else that's in vanilla root foler:
```sh
Data/**
MonoBleedingEdge/
RimWorldWin64_Data/
Source/
EULA.txt
Licenses.txt
ModUpdating.txt
mono-2.0-bdwgc.dll
PhinixCredentials.bin
Readme.txt
RimWorldWin64.exe
ScenarioPreview.jpg
steam_appid.txt
SteamInput*
Unity*
```

But what if you have some custom local mod? Git will still track everything that's contained in Mods/, right?

That's where .git/info/exclude works. It works just like a local, per-user .gitignore file, so you don't have to worry about messing with current .gitignore file.

My current .git/info/exclude file looks like this:
```sh
# git ls-files --others --exclude-from=.git/info/exclude
# Lines that start with '#' are comments.
# For a project mostly in C, the following would be a good set of
# exclude patterns (uncomment them if you want to use them):
# *.[oa]
# *~

# VS code workspace file
*.code-workspace

# Start Up script
Start_Win.bat
# SaveData (including configs, etc.) folder
SaveData

# True ModsConfig.xml used by game or RimPy
Config/ModsConfig.xml

# Local Mods
Mods/Vile*
Mods/Core_SK_Patch
Mods/AgeReversalAgeRebalanced
Mods/AndroidsIdeologyPatch
Mods/ChooseYourMedicine-zh
Mods/Eaves, Rafters and Mad Walls
Mods/HSK_CN
Mods/Parabellum
Mods/Simple-Storage-HSK
Mods/Tecs-Assorted-Patches
Mods/VisibleRaidPoints-SK
```
