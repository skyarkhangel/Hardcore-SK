
v1.12.15

- Fixed compatibility patch for recently updated mod Vehicle Framework
- Fixed preview not auto-refreshing when settings are changed

v1.12.14

- Adjust logging for compatibility with the new Harmony enhanced stacktraces
- Refactor the optimization patch for the cave generation code

v1.12.13

- Fix compatibility issues with pocket maps (Anomaly content)
- Fix an issue with BetterLoading that occured when other mods throw startup errors

v1.12.12

- Fix compatibility with the recently updated BetterLoading mod

v1.12.11

- Support for RimWorld 1.5 (unstable)

v1.12.10

- Added Turkish translation (thanks to @penu1881 for submitting)

v1.12.9

- The performance optimizations added in v1.12.8 are now enabled by default
- Added 'Compatibility mode' option that can help with potential issues
- Adjusted visibility of the world map toolbar to be more consistent
- Fixed an error that could occur when closing reroll window while it is generating

v1.12.8

- Added new experimental performance optimizations (disabled by default)
- Added labels to the reroll helper window (the seeds are now numbered)
- Added option to jump to any seed by number (hold shift when clicking refresh)
- The original map seed will now be included and automatically pinned
- Fixed an uncommon compatibility issue with the "Configurable Maps" mod
- Fixed the learning helper obscuring part of the reroll window

v1.12.7

- Added option to enable map preview only during new game setup (starting tile selection)
- Added option to not show previews when a settlement or quest site is selected
- Fixed an issue that caused the Undo Rerolls button to appear unexpectedly
- Fixed impassable tiles adjacent to settlements not being previewable
- Fixed an incompatibility with the mod "Clouds"

v1.12.6

- Fixed incorrect preview map size in games with zero active maps
- Removed a compatibility patch for "Biomes! Core" which is no longer needed

v1.12.5

- Added performance patches to make previews with caves generate around 3 times as fast
- Removed the need to wait for preview completion during new game setup
- Fixed a compatibility patch error that only happened with specific mod load order
- Fixed an error that could occur when caravans trigger a preview

v1.12.4

- Improved support for Terra Project Core (previews for cave maps will now generate faster)
- Some more compatibility improvements and bugfixes

v1.12.3

- Fixed compatibility with "Vanilla Base Generation Expanded"

v1.12.2

- Added support for RimNauts 2, you can now preview moons and asteroids!
- When selecting a tile to preview, it's now possible to directly select world objects (such as quest sites and settlements)
- Added toolbar button for quick access to Geological Landforms settings (disabled by default to avoid clutter)
- Added Chinese Traditional translation (thanks to @shiuanyue for submitting)
- Some small bug fixes and performance improvements

v1.12.1

- Added a reroll helper window that shows previews for multiple seeds at once and allows pinning favorites
- Fixed preview window not opening when a tile is clicked in some circumstances
- Fixed compatibility with RimThreaded

v1.12.0

- Added option to exclude caves from previews (makes preview generation faster)
- Added option to hide individual toolbar buttons (in case there is some you never use)
- Added option that locks preview and toolbar position to prevent accidental dragging
- When "WorldEdit" is installed, a new toolbar button will be displayed that opens the WorldEdit window
- When "Prepare Landing" is installed, a new toolbar button will be displayed that opens the Prepare Landing window
- The preview window will no longer auto-open immediately after switching to world map (no more lag spikes)
- Fixed UI sliders not working correctly after they were changed in a recent RimWorld update
- Reworked many Harmony patches into transpilers for better mod compatibility
- Added compatibility for the BetterLoading 1.4 fork (experimental, may be unstable)

v1.11.0

- Integration with Map Designer: Open it on the world map and Map Preview will act as a live preview for your changes
- Thanks to Zylleon for the code changes on Map Designer's side that make this possible!
- Moved the buttons from the preview window to a separate toolbar, so they no longer cover parts of the preview
- Added a button to quickly reroll the world seed (during starting site selection)
- Preview will now automatically refresh when you change the map size (also works with Better Map Sizes mod)
- Reworked how seeds for rerolls are chosen. They are still deterministic, but now tied to world seed and tile
- Fixed reroll button being unavailable for tiles that have quest sites on them

v1.10.3

- Added tooltip that shows terrain at hovered position in preview
- Fixed compatibility with "Performance Optimizer" mod in 1.4
- Some minor bugfixes

v1.10.2

- Added basic map seed reroll feature (disabled by default and can be enabled in the mod settings)

v1.10.1

- Fix for RimWorld version 1.4.3514 and later

v1.10.0

- Support for RimWorld 1.4
- Fixed a bug that caused terrain to generate slightly differently than in vanilla
- Added support for non-standard map sizes, and added integration with the "Better Map Sizes" mod
- Reworked how Harmony patches are applied to improve performance
- Added compatibility patches for Save Our Ship 2, Startup Impact, Fish Traps and Animal Traps
