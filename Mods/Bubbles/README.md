# Interaction Bubbles
![Mod Version](https://img.shields.io/badge/Mod_Version-1.6.1-blue.svg)
![RimWorld Version](https://img.shields.io/badge/Built_for_RimWorld-1.1-blue.svg)
![Harmony Version](https://img.shields.io/badge/Powered_by_Harmony-2.0-blue.svg)\
![Steam Subscribers](https://img.shields.io/badge/dynamic/xml.svg?label=Steam+Subscribers&query=//table/tr[2]/td[1]&colorB=blue&url=https://steamcommunity.com/sharedfiles/filedetails/%3Fid=1516158345&suffix=+total)
![GitHub Downloads](https://img.shields.io/github/downloads/Jaxe-Dev/Bubbles/total.svg?colorB=blue&label=GitHub+Downloads)

[Link to Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1516158345)\
[Link to Ludeon Forum thread](https://ludeon.com/forums/index.php?topic=45958.0)


---

Shows bubbles when pawns perform a social interaction with the text that would normally only be found in the log.

Bubbles will fade away after a short time but they are linked to the game time so pausing the game will halt the bubble from fading. Hovering over a bubble will make it nearly transparent and they can be clicked through to objects underneath. There is a toggle button in the play settings area to disable bubbles from being shown.

---

##### STEAM INSTALLATION
- **[Go to the Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1516158345]) and subscribe to the mod.**

##### NON-STEAM INSTALLATION
- **[Download the latest release](https://github.com/Jaxe-Dev/Bubbles/releases/latest) and unzip it into your *RimWorld/Mods* folder.**

---

The following base methods are patched with Harmony:
```
Postfix : RimWorld.PlaySettings.DoPlaySettingsGlobalControls
Postfix : RimWorld.MapInterface.MapInterfaceOnGUI_BeforeMainTabs
Postfix : Verse.PlayLog.Add
Prefix  : Verse.Profile.MemoryUtility.ClearAllMapsAndWorld
```

