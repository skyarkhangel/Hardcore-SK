# Changelog

## 1.3.0
- Added quick selection of last loaded plans by right clicking 'load plan' designator
- Added selection of opacity, texture and visibility for each plan type individually
- Added custom keybinding for PlanDoors/Floors/Objects/Walls designators
- Removed top right icon from PlanDoors/Floors/Objects/Walls designators

## 1.2.0
- Added saving / loading / deleting of plans
- Refactored shape code
  - Added shape variants
  - Added potential shape options
- Reworked the interaction with the 'plan xxx' designator buttons
  - Left click opens color dialog (can force Ctrl+click in options)
  - Right click opens shape selection
- Added 'Remove (count) xxx plans' on right click to 'Remove Plan' designator (including Undo/Redo support)
- Last selected color is now saved and loaded for each plan type
- Improved Plan Visibility
  - Added 'Toggle Visibility' designator
  - Added global shortcut
  - Plans should now become visible on all plan interactions
- Added Plan Opacity
  - Controllable when left clicking the newly added 'Change Appearance' designator
  - Opacity is saved and loaded for each plan type separately
- Added Plan TextureSets
  - Controllable when right clicking the newly added 'Change Appearance' designator
  - Texture set is saved and loaded for each plan type separately
- Added sounds to undo / redo clicks

## 1.1.0
- Replaced various icons
- Fixed using Cancel tool removes plan designations
- Fixed using 'Remove plan designator' throws error message
- Added option in settings to display extra cut designator
- Added option in settings to enable/disable persistent plans (plans will / won't be removed after a finished task)
- Added toggle in the bottom right to switch plan visibility

## 1.0.0
- Initial Version
- Draw colored designations, such as walls, doors, objects and floors
- Optionally skip (don't overwrite) already placed plan designations
- Use different shapes, e.g. rectangle, area, cross
- Copy / Cut / Paste your plans
- Undo / Redo your planned actions
