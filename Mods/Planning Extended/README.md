# Planning Extended

This mod adds a more advanced planning menu.

## Features

### Implemented

- Draw colored designations, such as walls, doors, objects and floors
- Optionally skip (don't overwrite) already placed plan designations
- Use different shapes, e.g. points, lines, rectangles
- Load / Save plan blueprints, quickly select last loaded plans
- Cut / Copy / Paste your plans
- Undo / Redo your planned actions
- Show / Hide your planning designations (incl. global shortcut)
- Change plan texture sets and opacity for each plan type individually
- Convert your plan from vanilla Rimworld or MorePlanning mod

### Planned

- Improve grabbing position while pasting
- Add more shapes, such as ellipse, fixed shapes (e.g. sun lamp, max room size)
- Add more shape options, e.g. width
- Add plan groups / layers
- Add ability to convert plan to real blueprints
- Add toolbox
- Add text overlay to describe areas
- Add mining, pipe, cable designations?
- Add door variants
- Display wall designations according to surrounding walls
- Allow blueprints to be stored within save game or settings (cloud sync)

## Default Shortcuts
- Q, E: Rotation, Number of Columns
- Z, X: Flip, Width, Number of Rows
- V: Change Shape-Variant
- Ctrl: Color-Picker
- Alt: Skip / Replace
- Shift: Shape-Modfier (e.g. square)
- Backslash ('\\'): Global Plan Visibility Toggle

## Shapes

### Variants

- Point (Number of Points)
- Line
  - Simple Line
  - Line Grid (Number of Cells)
- Rectangle
  - Filled
  - Grid (Number of Segments)
  - Outline
- *Ellipse
  - Filled
  - Outline
- *Fixed
  - Max Room-Size
  - Sun-Lamp
  - Orbital-Trade-Station

**To be implemented*

### Modifier

- Simple Line: draws a horizontal or vertical line
- other shapes: applies square modfier

## Supported Languages
- English
- Japanese translation via sub-mod by Proxyer
- Russian translation by mmavka

---

## FAQ

### Can this mod be safely added and removed at any time?
> This mod can be added at any time. By removing this mod you will get a one-time error message and lose all your planning designations. Other than that, you should be fine.

### Is there a way to automatically remove the plan once a building or mining task has finished?
> Yes, there is. Go to the 'mod settings' and turn 'persistent plans' off.

### My plans have disappeared!
> Be sure they're not just hidden. You can toggle the plan visibility either in the architect menu, by clicking on the small icon in the bottom right corner or by using the global hotkey (default: '\\').

### How do I convert my More Planning mod plans?
> Be sure both mods are enabled. If the MorePlanning mod is not enabled, its plan designations won't be loaded, therefore there is nothing for this mod to convert. Go to the mod settings and press the 'Convert' button. All MorePlanning designations will be converted to WallDesignations with a color that is close to the original MorePlanning colors. So if you have changed the original colors, then the new colors won't match.

### Can I use this mod together with the mod 'Designator Shapes'?
> Yes, you can. But for it to work, you have to change the shape of this mod to 'rectangle' (right click on 'Plan Wall' button) and its variant to 'filled rectangle' (default key to switch between variants is 'V'). Otherwise, both mods try to modify the shape, which will not result in what you would expect.

### I feel this mod is missing something or could be improved!
> Feel free to make any suggestions on [GitHub](https://github.com/Scherub/rw-planning-extended/). If you feel confident, you're more than welcome to branch the code and create a pull request for a newly implemented feature.

### I really want this mod available in my native language!
> Unfortunately, I'll have to rely on the community here. I'm only fluent in German and English, everything else I require the help from one of you. If you're willing to help, then [here](https://steamcommunity.com/linkfilter/?url=https://github.com/Scherub/rw-planning-extended/blob/develop/Common/Languages/English/Keyed/Translations.xml) is the file required to translate.

### Why does this mod work only with RimWorld v1.4?
> I've tried to compile it for RimWorld v1.3. Unfortunately, there is quite a bit that I would have to add or change, to make it work. So for now, I don't think it would be worth the effort.

### Why does my performance decrease when I paint lots of planning designations?
> The problem is, that the designations aren't rendered in a batched call. When I add about 20.000 designations and fully zoom out (using Camera+), my FPS takes a slight hit. You will encounter the same problem, when using the MorePlanning mod or vanilla paint-tool (for floors or buildings/structures) and add that many designations.

---

## Installation

* Go to the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2877392159) and subscribe to the mod.