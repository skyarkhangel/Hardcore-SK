# ExpandedRoofing [![RimWorld 1.1](https://img.shields.io/badge/RimWorld-1.1-green.svg?longCache=true&style=plastic)](http://rimworldgame.com/)

Adds customizable roofing, with transparent greenhouse roofing and photovoltaic solar roofing. Reworked from Vendan's original idea.

## Features
- Transparent roofs that let almost all light through
- Solar roofs and controllers that can be used to generate power
- Removal of thick roofs
- Thick roofs (which require maintenance as of Late A17/B18)
- 4 new techs to research for each of these features.
- Disables settings when `Don't Tempt Me!` is enabled (Vassteel Rydex's request)

## Solar Roofing and Controllers explained
- Each controller has a max output of 2500w
- Each solar tile produces 200w during full sun
- There is no limit to the number of controllers connected to a section of tiles

## Roof Maintenance
- Maintenance required for thick roofs (only)
- Maintenance is a long tick (once every 2000 ticks)
- Maintenance is required after 5000 ticks (~167 days)
- After 7500 ticks collapses can occur by MTB event. (~250 days)
- MTB event is set for 3.5 days.
- These numbers are subject to change.

## Notes
- New designators are in the `Zone` tab
- Due to how roofs are implement, it's easiest to rezone default roofing to new types

## Translations
- English
- Turkish (by Slevilex)
- Russian (by Garr Incorporated, updated by lex1975)
- French (by Jozay)
- Chinese (by Rosenmund and 不为人知的汉子)
- Japanese (by mundane_m)

## Acknowledgements

Thank to Erdelf for getting me back on track and Xen for designator artwork.

Special thanks to Pardeike's amazing non-destructive patching library, Harmony.

<p align="center">
  <a href="https://github.com/pardeike/Harmony">
    <img src="https://raw.githubusercontent.com/pardeike/Harmony/master/HarmonyLogo.png" alt="Harmony" width="128" />
  </a>
</p>

<hr>

<p align="center">
  <a href="./LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-lightgray.svg?style=flat" alt="MIT License" />
  </a>
</p>
