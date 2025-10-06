
v1.6.10

- Fixed an error that occured when clicking "Any" or "Any POI" in the landform search
- Adjusted Surface Cave landform to ensure there is enough unroofed area for quest spawns
- Fixed landform filter in "Prepare Landing" not being saved in presets
- Fixed compatibility with "BiomesKit" which has been annexed into "Regrowth Core"
- Fixed false positive in patch conflict detection for "Biomes! Caverns"
- Adjusted logging for compatibility with the new Harmony enhanced stacktraces
- Added debug action that regenerates the BiomeGrid of the current map

v1.6.9

- Fix compatibility issues with pocket maps (Anomaly content)
- Fix an issue with BetterLoading that occured when other mods throw startup errors

v1.6.8

- Fix animal and plant spawning on multi-biome maps
- Fix compatibility with the recently updated BetterLoading mod
- Fix compatibility with the Map Reroll mod

v1.6.7

- Support for RimWorld 1.5 (unstable)
- Fixed mech cluster quests appearing in cave landforms
- Fixed excessive lightning strike amount in cave landforms
- Fixed terrain from biome variants not applying in caves
- Fixed some exceptions in the landform editor

v1.6.6

- Fixed overlapping slider from "Realistic Planets" on the world settings screen
- Fixed a warning that was logged when the "Regrowth Wastelands" mod is present
- Fixed wind turbine auto-cut filter not showing plants from biome transitions
- Fixed manually placed cave landforms not visually connecting with existing ones
- Fixed Debug tab not appearing in the mod settings when dev mode was disabled on startup
- Added a more clear message when the game version is too outdated for the mod to work
- Added load order rule for "Multiplayer" mod to prevent it from breaking some GL features

v1.6.5

- Fixed an issue that sometimes caused cave systems to vanish from the world map
- Added extra validation for cave generation to ensure maps can be entered and exited correctly
- Added debug options to set the landform, biome and map size for the "Dev Quick Test" button
- Fixed a compatibility issue with RimWar that prevented caravans on impassable tiles
- The mountains and cave system density sliders are now compatible with Realistic Planets Continued
- Prepare Landing will now be able to find impassable tiles with cave systems by default

v1.6.4

- Added sliders for cave system density and overall mountain density to the world settings page
- Reduced caravan movement difficulty on tiles with cave systems to 10
- Fixed a compatibility issue with RocketMan that prevented caravans from entering and leaving cave systems
- Fixed a compatibility issue that caused an error when trying to form a caravan starting from an impassable tile
- Fixed a compatibility issue that caused a lagspike once per in-game day
- Fixed a compatibility issue with the FasterGameLoading mod
- Adjusted cave tile graphics to make them work when World Map Beautification Project is installed

v1.6.3

- Full release of all the new content added in the v1.6.x pre-release versions
- Bugfixes and compatibility improvements

v1.6.2 (pre-release)

- Bugfixes and improvements for the new content added in the previous pre-release

v1.6.1 (pre-release)

- Added new landforms: Secluded Valley and Sinkhole
- Improved existing landforms: Surface Caves and Cave Entrance
- Fixed cave landforms sometimes generating with no entrance to the map
- Fixed river angle and offset on coasts to be more consistent with the world map and landform angles
- Various performance improvements, such as much faster cave generation
- Improved landform editor workflow for custom landforms stored in local mod folders
- Added the option to override world tile values such as temperature and rainfall on a per-biome basis
- Added landform node that allows restricting incidents and raid strategies
- Added landform node that runs any GenStep by Type with custom parameters
- Added landform node that allows displacing and scaling grids
- Compatibility improvements for Map Designer

v1.6.0 (pre-release)

- Added new landforms: Surface Cave and Cave Entrance
- Cave systems appear on impassable mountain tiles on the world map and can be entered by caravan
- Completely reworked mod settings with a new tab-based UI
- Added toggles for each landform, it's now easy to choose which landforms can appear in the world
- Added per-biome settings with toggles for landforms and biome transitions
- Reworked topology calculations, landforms should now better match the land shapes on the world map
- Improved landform search feature UX and performance
- Improved landform editor and implemented asyncronous previews (no more UI lag)
- Fixed an error caused by a broken biome def from "TerraProject Core"
- Improved compatibility with "Map Designer" mod's beach settings
- Fixed UI sliders not working correctly after they were changed in a recent RimWorld update
- Fixed a patch on an obsolete method from a recent RimWorld update
- Reworked many Harmony patches into transpilers for better mod compatibility
- Added compatibility for the BetterLoading 1.4 fork (experimental, may be unstable)

v1.5.2

- Added a XML DefModExtension that authors of biome mods can use to control the following properties:
  - Whether landforms are allowed to appear in the biome (it is also possible to disallow only specific landforms)
  - Whether the biome can be part of biome transitions (it is also possible to disallow only specific transition biomes)
  - Whether the biome should be treated as water-covered (biomes that spawn in/on the ocean, like sea ice, islands, etc.)
  - Which terrain should be placed at the edges of mountains (vanilla default is Gravel)
  - Which terrain should be placed on beaches (vanilla default is Sand)
- More details and an example are available on the [Wiki page](https://github.com/m00nl1ght-dev/GeologicalLandforms/wiki)

v1.5.1

- Fix for RimWorld version 1.4.3514 and later

v1.5.0

- Support for RimWorld 1.4
- Fixed some small bugs in topology calculations
- Fixed plant density in Desert areas on maps with multiple biomes
- Fixed animal density in biomes added by other mods on maps with very little open area
- Various other bug fixes and improvements
