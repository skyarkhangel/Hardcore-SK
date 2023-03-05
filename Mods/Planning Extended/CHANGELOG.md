# Changelog

## 1.7.3
- Fixed plans not being drawn when outside visible area
- Fixed disappearing vanilla plans while loading a map

## 1.7.2
- Added missing Russian translations

## 1.7.1
- Fixed missing shape translations

## 1.7.0
- Added point shape (useful helper to measure distances or add doors)
- Line shape now supports two types
  - Simple Line: draw a line from A to B
  - Line Grid
- Added grid support for rectangles
- Added shape modifier key
  - Simple Line: draws a horizontal or vertical line
  - other shapes: applies square modfier
- Fixed missing map boundaries while pasting a plan
- Fixed shape variants not remembering options
- Fixed missing translations

## 1.6.1
- Added Russian language support

## 1.6.0
- Fixed sound not playing on 'Plan xxx' button click
- Added custom keybinding for color picker
- Added option to skip instead of replace as default
- Added 'Paint Plan' designator
- Changing texture set also changes 'Plan xxx' icon accordingly

## 1.5.1
- Fixed vertical doors not always working when using undo-redo-system
- Added vanilla RimWorld plan converter

## 1.5.0
- Support of vertical doors
- Maps are now automatically updated (old plans, vertical doors)
- Added button to convert plans from MorePlanning mod

## 1.4.1
- Moved designators to its own category to avoid conflict with MorePlanning mod

## 1.4.0
- Fixed NoOverwriteMode not reacting to correct button
- Added support of NoOverwriteMode when pasting a plan
- Updated dashed and round designations

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
