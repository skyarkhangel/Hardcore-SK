[![RimWorld Alpha undefined](https://img.shields.io/badge/RimWorld-Alpha%20undefined-brightgreen.svg)](http://rimworldgame.com/)

Provides a vastly more customizable work tab. 

# Important
Work Tab completely takes over job priorities from the vanilla game. In order to support core functionalities and other mods, it intercepts calls to get/set priorities. However, when it is told to set priorities by other modded code that is not aware of the time schedule or detailed priorities, the priority will be set for the whole day, and/or for all workgivers in a worktype.

# Features
Various usability extentions to the 'vanilla' work tab; 
 - Work types can be expanded (by Ctrl+clicking the column header) to allow you to set priorities for the individual tasks within each work type. 
 - Time scheduler to set priorities for a given time slot only - allowing you to designate a cleaning hour, or have your cook prepare meals right before dinner, etc. etc. 
 - Up to 9 priority levels (configurable in mod options)
 - Various small UX tweaks; scrolling to increase/decrease/toggle priorities, increase/decrease priorities for whole columns/rows (by holding shift and clicking/scrolling while hovering over the column header/pawn name respectively).
 - **All functions are detailed in the tooltips, take a moment to hover over and read them!**

# Known Issues
None.

# Notes
With great power comes great responsibility. The default priorities of tasks within a job is set for a good reason; it's (usually) a sensible default. Changing these can lead to deadlock situations, so change the priorities of individual jobs at your own risk! 

Finally, there will never be an 'autolabour' mode where a mod sets priorities for you. Due to the way the AI is handled (e.g. pawns actively look for work, instead of there being a 'bulletin board' of jobs that need doing), it's not feasible to get the complete list of work that needs doing that would be needed to make this a reality, without extreme overhead and loads of special exception coding.

# Powered by Harmony
![Powered by Harmony](https://camo.githubusercontent.com/074bf079275fa90809f51b74e9dd0deccc70328f/68747470733a2f2f7332342e706f7374696d672e6f72672f3538626c31727a33392f6c6f676f2e706e67)

# Contributors
 - Bronytamin:	Russian translation
 - Duduluu:	Chinese translation
 - Eric Swanson:	Help with time-dependent tooltips
 - DoctorVanGogh:	Help with typos in build script
 - MrClon:	Russian translation
 - mercutiodesign:	Optional scrollwheel usage setting
 - Bugo:	Russian translation (update)
 - Arex-rus:	Russian translation (fixes)
 - mora145:	Spanish translation
 - Alex TD:	Various tweaks and suggestions
 - Suh. Junmin:	Korean translation
 - rw-chaos:	German translation
 - Mehni:	Compatibility with Numbers mod
 - Silverside:	Fix UI scaling bug

# Think you found a bug? 
Please read [this guide](http://steamcommunity.com/sharedfiles/filedetails/?id=725234314) before creating a bug report,
 and then create a bug report [here](https://github.com/fluffy-mods/WorkTab/issues)

# Older versions
All current and past versions of this mod can be downloaded from [GitHub](https://github.com/fluffy-mods/WorkTab/releases).

# License
All original code in this mod is licensed under the [MIT license](https://opensource.org/licenses/MIT). Do what you want, but give me credit. 
All original content (e.g. text, imagery, sounds) in this mod is licensed under the [CC-BY-SA 4.0 license](http://creativecommons.org/licenses/by-sa/4.0/).

Parts of the code in this mod, and some content may be licensed by their original authors. If this is the case, the original author & license will either be given in the source code, or be in a LICENSE file next to the content. Please do not decompile my mods, but use the original source code available on [GitHub](https://github.com/fluffy-mods/WorkTab/), so license information in the source code is preserved.

# Are you enjoying my mods?
Show your appreciation by buying me a coffee (or contribute towards a nice single malt).

[![Buy Me a Coffee](http://i.imgur.com/EjWiUwx.gif)](https://ko-fi.com/fluffymods)

[![I Have a Black Dog](https://i.ibb.co/ss59Rwy/New-Project-2.png)](https://www.youtube.com/watch?v=XiCrniLQGYc)

# Version
This is version 3.13.289, for RimWorld 1.0.2408.