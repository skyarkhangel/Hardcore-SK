
# Map Preview

A mod for the game Rimworld.

Adds a map preview to the world map.
It shows you the map that will generate if you settle on the currently selected world tile.

The size of the preview window can be changed in the mod settings. To easily turn the preview on/off, you can use the small toggle button in the bottom right of the world map.

This mod was developed as a QoL addition for Geological Landforms and works great alongside it, but can also be used without it.


# Additional Features

Reroll map seeds:
- You can reroll the seed for individual world tiles. This feature is disabled by default and can be enabled in the mod settings. The main difference to the Map Reroll mod is that the rerolling happens on the world screen, before the actual map is generated, which is better for mod compatibility.

True terrain colors:
- Instead of a fixed color palette that only supports vanilla terrain, this mod dynamically extracts the actual texture colors from all terrains, including those added by other mods. So basically you will see the terrain how it will actually look when you settle on that tile. If for some reason you prefer the original fixed color palette from Map Reroll, I added an option to disable this feature in the mod settings.

Exact preview generator:
- Since version 1.6 this mod now uses its own preview generator instead of the original one from the Map Reroll mod. It is more accurate and most importantly compatible with other mods that modify or add map features, for example Terra Project Core, CaveBiome and various other biome mods.

Faster river generation:
- Significantly speeds up generation of maps that have a river.


# FAQ

Can I safely add or remove this mod from existing saves?
- Yes.

Does this also work during an ongoing game, after selecting the starting location?
- Yes. For example you can use it when starting a caravan or creating a second settlement.

Does this slow down world map generation?
- No. Nothing additional is generated until you actually select a tile and trigger the preview.

How long does it take to render the preview when you click a tile?
- Usually less than a second, unless there is a huge cave or complex modded biome on the tile, then it can take a bit longer.

Will the preview/map be the same when I select the same world tile again later?
- Yes, RimWorld generates maps with a seeded RNG, so same world seed + same location = same map. Unless some other mod messes with that system.


# Compatibility

Terrain added by other mods is supported via the "True Terrain Colors" feature (make sure it is enabled in the mod settings).

If you find any mods that cause the previews to be inaccurate or incomplete, please let me know so I can add a compatibility patch.

When reporting bugs or mod conflicts, always upload your log file via HugsLib (press Ctrl + F12 directly after the problem happened) and include the link in your message. Without a log file there is no way for me to figure out what exactly is wrong or how to fix it.

Not compatible with the Multiplayer mod.


# Credit

This mod uses modified code and graphics from the mod Map Reroll created by UnlimitedHugs.

Idea for mod concept by orbittwz#2340

Included translations:
- Korean submitted by Seanggag
- Russian submitted by Dmitry6
- Chinese Traditional submitted by shiuanyue


# Links

Steam Workshop:
https://steamcommunity.com/sharedfiles/filedetails/?id=2800857642


# License

Copyright (c) 2022 m00nl1ght <<https://github.com/m00nl1ght-dev>>

<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Creative Commons Licence" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a><br />All contents of this work (including source code, assemblies and other files), except where a different license is specified within the file itself, are licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License</a>.

In addition to this license, content creators on YouTube and Twitch are expressly permitted to incorporate this work within their (monetized) videos and livestreams.
