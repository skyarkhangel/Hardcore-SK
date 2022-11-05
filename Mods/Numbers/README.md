# Numbers!

Adds a customizable general overview tab, allowing you to see any stats on all your colonists or prisoners in a single window. Quickly compare colonists to see who your best doctor is, or to assign gear optimally.


![Main Image](./.github/assets/images/img1.png)

### Features

- Customizable overview tab, displays as many columns as you can fit on your screen.
- Displays Stats, Skills, Needs, Gear, Queued jobs, Pawn Records, etc etc.
- Prisoner and medical controls, equipment overview, current job.
- A Medical Tab that's better than Fluffy's Medical Tab.
- Works with colonists, prisoners, enemies, animals, wild animals, and even corpses.
- Presets! Save, load, move and share your custom layouts. Store them as default.

### Usability

- Slide and reorganise columns as you see fit. Reorderable headers!
- Click headers to sort by stat and compare.
- Click to jump to colonist.
- Right-click headers to close.
- Draggable options for things like: prisoner interaction, all checkmarks. Try it on outfits, medical care, hostility response mode and be pleasantly surprised.

It can be added to existing saves without problems.

### Links

- [Latest release](https://github.com/Mehni/kNumbers/releases/latest)
- [Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=1414302321)
- [Ludeon](https://ludeon.com/forums/index.php?topic=35832.0)
- [GitHub](https://github.com/Mehni/kNumbers)
- [All the releases](https://github.com/Mehni/kNumbers/releases)

## Credits

Much thanks to [Maarxx](https://github.com/maarxx) for singlehandedly adding support for Royalty. Check out their mods [here](https://ludeon.com/forums/index.php?topic=53539)!

### Languages

- English: Mehni
- Chinese (simplified): AlongWY
- German: Amalek
- Russian: JasKill
- Spanish: Crusader

## Adding a column (for contributors)

1. Add a new class in Numbers\PawnColumnWorkers. Use the `Numbers` namespace. Inherit from the correct PawnColumnWorker.
1. Create a new PawnColumnDef. Save it in `1.1\Defs\PawnColumnDef\PawnColumns_Numbers.xml`. Adhere to the naming scheme there.
1. To add your new column to the Misc button, add the defName of your PawnColumnDef to the appropriate Def in `1.1\Defs\PawnColumnOptionDef\Numbers_PawnColumnOptionDef.xml`. This step should not be skipped.
1. To add your new column to the default table, add the defName of your PawnColumnDef to the approppriate PawnTableDef in `1.1\Defs\PawnTableDef\Numbers_PawnTableDef.xml`.

## License

Original idea by koisama: https://github.com/koisama/kNumbers, whose original license I respect by the preceding link. For the license since 2018/11/21, see LICENSE.