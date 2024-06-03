# Interaction Bubbles

![Mod Version](https://img.shields.io/badge/Mod_Version-3.1-blue)
![RimWorld Version](https://img.shields.io/badge/Built_for_RimWorld-1.5-blue)
![Harmony Version](https://img.shields.io/badge/Powered_by_Harmony-2.3.3-blue)\
[![Steam Subscribers](https://img.shields.io/steam/downloads/1516158345?color=blue&label=Steam%20Downloads&logo=Steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=1516158345)
[![GitHub Downloads](https://img.shields.io/github/downloads/Jaxe-Dev/Bubbles/total?color=blue&label=GitHub%20Downloads&logo=GitHub)](https://github.com/Jaxe-Dev/Bubbles)

[Link to Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1516158345)\
[Link to Ludeon Forum thread](https://ludeon.com/forums/index.php?topic=45958.0)

[![Chat on Discord](https://img.shields.io/badge/Chat_on_Discord-_-_?style=social&logo=Discord)](https://discord.gg/VvSnYQ8)\
[![Donate via PayPal](https://img.shields.io/badge/Donate_via_PayPal-_-_?style=social&logo=PayPal)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6RS7DLLGCJT8L)\
[![Support on Patreon](https://img.shields.io/badge/Support_via_Patreon-_-_?style=social&logo=Patreon)](https://www.patreon.com/jaxe)

---

Shows bubbles when pawns perform a social interaction with the text that would normally only be found in the log.

Bubbles will fade away after a short time but they are linked to the game time so pausing the game will halt the bubble from fading. Hovering over a bubble will make it nearly transparent and they can be clicked through to objects underneath. Use the toggle button in the play settings area to disable bubbles from being shown.

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

