
# Geological Landforms

A mod for the game Rimworld.

Adds 32 landforms to the game, which generate biome-independently.

The goal of this mod is to make map generation more interesting and varied, compared to the vanilla experience of simply potentially having either a cliff or a coast on one edge of your map.

Landforms of the selected world tile are listed at the bottom of the world map "Terrain" tab. Each landform only generates on tiles within its temperature range, rainfall amount and other conditions.

Some of the landforms significantly affect gameplay and can make the game both easier or much more challenging in various ways. For example, Islands and Archipelagos are very remote but provide very little stone and ores, and Canyons and Rifts are very easy to defend and have plenty of ores, but provide very little natural resources like food, light, wood, and space to grow plants or keep animals.

The landforms are optimized for the standard map size (250x250). Using a larger map size should also be fine, but some of the landforms might not generate correctly if the map is too small.


# List of landforms

- Coast, Cove, Cove with Island, Secluded Cove and Fjord
- Peninsula, Isthmus, Tombolo and Coastal Island
- Island, Atoll, Archipelago and Skerry
- Lake, Lake with Island and Dry Lake
- Cliff, Coast with Cliff and Cirque
- Valley, Canyon, Rift and Crater
- Lone Mountain and Caldera
- Oasis, Ice Oasis and Swamp Hill
- Badlands and Desert Plateau


# Add-ons

Biome Transitions
- Generates maps with multiple biomes on them when you settle at the border between two or more biomes on the world map.


# Creating your own landforms

Pretty much everything about the landforms provided by this mod is fully configurable, including where and how often they appear on the world map, and how exactly they affect map generation. This mod includes a node-based landform editor that can be accessed from the mod settings.

If you would like to create new landforms, this is also possible. If you don't know how the internals of RimWorld's map generation and the math behind it work yet, I recommend using one of the existing landforms as a template, so you can play around with the values and see what they do. This works best if you have Map Reroll installed, this way you can quickly check how your changes affect the result. The new editor also provides limited live previews now.

Landforms are stored as xml files, so if you want to share a landform you created, simply upload its file to a filesharing service and post the link. You can find the landform files by clicking the "Open custom landform data directory" button in the mod settings.


# FAQ

How is this mod different from other map generation mods?
- Geological Landforms does not add or modify any biomes, and instead adds a separate layer to map generation that can then independently be applied to any biome, including ones added by other mods.

How do events and raids work on islands?
- All landforms (should) always leave at least one part of the map edge walkable, which allows events and raids to happen normally. For islands specifically, at least one side will always have shallow water to allow entering and leaving the map.

Can I safely add or remove this mod from existing saves?
- Yes, it can safely be added or removed at any time. After removing it from an ongoing game there will an error appearing in the log once, which is harmless and can be ignored.

How can I easily find a specific landform on the world map?
- Select any tile, open the "Terrain" tab and click the "Search for specific landform nearby" button. Alternatively, you can install Prepare Landing which allows filtering all world tiles based on selected criteria. Geological Landforms contains an integration patch that adds a filter for landforms, you can find it in the "Terrain II & Temp" tab.


# Recommended Mods

Map Preview
- Adds a map preview to the world map. It shows you the map that will generate if you settle on the currently selected world tile.

Prepare Landing
- Allows filtering all world tiles based on selected criteria, including specific landforms, so you can find your perfect starting spot.


# Compatibility

This mod should be compatible with pretty much everything, including mods that add custom biomes. If you encounter any problematic interactions between the landforms and content from other mods, please let me know.

Biomes! Islands
- Compatible, however that mod makes deep water passable, which also significantly affects gameplay on many landforms. There is also an integration feature that allows the plants and animals from the Islands biomes to spawn on any coastal landform. This is disabled by default, you can enable it in the mod settings.

ReGrowth Collection, Alpha Biomes and other biome mods
- Compatible, landforms will generate in any biome as long as its requirements (temperature, rainfall, etc) are fulfilled.

[KV] Configurable Maps or Map Designer
- Compatible, but not all settings from those mods will still apply if there is a landform present, and some will override landforms.

Geological Landforms provides a XML DefModExtension that authors of biome mods can use to control which landforms are allowed to spawn in their biomes.
More details and an example are available on the [Wiki page](https://github.com/m00nl1ght-dev/GeologicalLandforms/wiki).


# Credit

The landform editor uses the C# library "Node Editor Framework" by Seneral, a flexible and modular framework for creating node based displays and editors in Unity.

Landform ideas:
- Desert Plateau suggested by Aranador
- Fjord suggested by Vhalknor

Included translations:
- Korean submitted by Seanggag

This mod is inspired by multiple great mods from earlier versions which unfortunately seem to be abandoned. Geological Landforms does not use any content or code from these mods though.

- Coastlines by Jaxe
- Terra Project by Lanilor
- Archipelagos by Rainbeau Flambe

# Links

Steam Workshop:
https://steamcommunity.com/sharedfiles/filedetails/?id=2773943594


# License

Copyright (c) 2022 m00nl1ght <<https://github.com/m00nl1ght-dev>>

<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Creative Commons Licence" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a><br />All contents of this work (including source code, assemblies and other files), except where a different license is specified within the file itself, are licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License</a>.

In addition to this license, content creators on YouTube and Twitch are expressly permitted to incorporate this work within their (monetized) videos and livestreams.
