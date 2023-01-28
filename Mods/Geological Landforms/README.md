
# Geological Landforms

A mod for the game Rimworld.

Adds 27 landforms to the game, which generate biome-independently.

The goal of this mod is to make map generation more interesting and varied, compared to the vanilla experience of  simply potentially having either a cliff or a coast on one edge of your map.

Landforms of the selected world tile appear in the "special features" section of the world map terrain tab. Some landforms only generate on tiles within the right temperature range, rainfall amount and other conditions.

Some of the landforms significantly affect gameplay and can make the game both easier or much more challenging in various ways. For example, Islands and Archipelagos are very remote but provide very little stone and ores, and Canyons and Rifts are very easy to defend and have plenty of ores, but provide very little natural resources like food, light, wood, and space to grow plants or keep animals.

The landforms are optimized for the standard map size (250x250). Using a larger map size should also be fine, but don't use smaller ones, because some of the landforms will not generate correctly if the map is too small.

This mod is currently in a beta state because the long-term gameplay effects of the landforms have not been tested and some things might need to be adjusted. If you encounter any issues or have suggestions, please leave a comment.

Landforms list:
- Coast (has a variant that stretches along tow edges of the map)
- Cove (has a variant with an island in the middle)
- Peninsula
- Landbridge
- Tombolo
- Coastal Island
- Island
- Atoll
- Archipelago
- Dry Lake
- Lake (has a variant with a small island)
- Cliff (has a variant that stretches along tow edges of the map)
- Coast with Cliff
- Cirque
- Valley
- Canyon
- Lone Mountain
- Caldera
- Rift
- Crater
- Oasis
- Ice Oasis
- Swamp Hill


# Creating your own landforms

Pretty much everything about the landforms provided by this mod is fully configurable via the mod settings, including where and how often they appear on the world map, and how exactly they affect map generation.

If you would like to create your own landforms, this is also possible. If you don't know how the internals of RimWorld's map generation and the math behind it work yet, I recommend using one of the existing landforms as a template, so you can play around with the values and see what they do. This works best if you have Map Reroll installed, this way you can quickly check how your changes affect the result.

Landforms are stored as xml files, so if you want to share a landform you created, simply upload its file to a filesharing service and post the link. You can find the landform files by clicking the "Open custom landform data directory" button in the mod settings.


# FAQ

How is this mod different from other map generation mods?
- Geological Landforms does not add or modify any biomes, and instead adds a separate layer to map generation that can then independently be applied to any biome, including ones added by other mods.


How do events and raids work on islands?
- All landforms (should) always leave at least one part of the map edge walkable, which allows events and raids to happen normally. For islands specifically, at least one side will always have shallow water to allow entering and leaving the map.


Can I safely add or remove this mod from existing saves?
- Yes. This mod only affects map generation and does not add any defs or saved data.


# Compatibility

This mod should be compatible with pretty much everything, including mods that add custom biomes. If you encounter any problematic interactions between the landforms and content from other mods, please let me know.

- Map Reroll: Fully compatible, previews include landforms.


# Credit

This mod is inspired by multiple great mods from earlier versions which unfortunately seem to be abondoned. Geological Coastlines does not use any content or code from these mods though.

- Coastlines by Jaxe
- Terra Project by Lanilor
- Archipelagos by Rainbeau Flambe

# Links

Steam Workshop:
https://steamcommunity.com/sharedfiles/filedetails/?id=2773943594

