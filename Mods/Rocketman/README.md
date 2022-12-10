# <a href="https://steamcommunity.com/sharedfiles/filedetails/?id=2479389928&searchtext=">RimWorld-RocketMan</a>

[![Build](https://github.com/kbatbouta/RimWorld-RocketMan/actions/workflows/main.yml/badge.svg)](https://github.com/kbatbouta/RimWorld-RocketMan/actions/workflows/main.yml) ![Version](https://img.shields.io/badge/Rimworld-1.2-brightgreen.svg)

![Alt text](https://github.com/kbatbouta/RimWorld-RocketMan/blob/master/Textures/RocketMan/UI/rocketman_main.png?raw=true)


## Description:

RocketMan is a RimWorld mod that is designed to improve RimWorld performance.

<b>Important note: RocketMan should be the last mod in your mod list.</b>

## Features:

Description comming soon...

## Notes: 

1. RocketMan doesn't support RimThreaded and never will.
2. Currently The Multiplayer mod is not supported.
3. Bug reports with no logs will not get a response.

Github Link: https://github.com/kbatbouta/RocketMan

RocketMan is OpenSource and for anyone to use, modify, update.

## Credits: 

* The main developer: Karim (aka Krkr)
* The Thumbnail: Trisscar.
* The original concept of Statistic caching: Notooshabby.

DISCLAIMER: I’m not responsible in any way for damage done by RocketMan to your saves. 

### RocketRules (Compatibility system)

RocketMan support a new rule system to avoid compatibility issues.
This works by placing `RocketRules.xml` files in `YourModFolder/Extras/RocketRules.xml`

### Request and notification system
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RocketRules>
     <Notify type="PawnDirty" packageId="krkr.RocketMan" method="ThingWithComps:Notify_Equipped"/>
</RocketRules>
```
Your mod can notify RocketMan to clear the statCache by calling a function in your code (preferably not empty due to patching limitations). You can follow this format
* `packageId` is your mod `packageId`. This is used only to keep track of the current rules.
* `method` (formated as `YourClass:Method`) is the method that you call to notify rocketman that your mod need the cache cleared.

**Note** This works by applying a `Prefix` patch on the destination/provided method (in this case `ThingWithComps:Notify_Equipped`) thus every time you call `ThingWithComps:Notify_Equipped` in this example the prefix is executed and the cache is cleared.
and that prefix notify rocketman to clear the cache

**Note** The destination/provided method should have something in it otherwise patching it may not be possible

**Notification types**
* `PawnDirty` The target/provided method for this need to have `Pawn pawn` as a parameter. 

**Note on notification types** For now there is only one which is `PawnDirty`. This system is the new way forword for your mod to call RocketMan regardless of the load order.

## Special Thanks goes to:

* Madman666 the entire Dubwise server for the help and testing they provided!
* Trisscar for their awesome thumbnail.
* Dubwise for hosting me on their discord server.
* Wiri for their awesome contribution.
* Bad Vlad (ModderK) for their awesome feedback.
* Taranchuk for their awesome help.
* Brrainz for Harmony and ZombieLand which inspired my time dilation solution.

You can always ask questions on the Dubwise discord server: https://discord.gg/mKVDMqq4

## You can always support me!
<a href='https://ko-fi.com/karimbat' target='_blank'><img height='35' style='border:0px;height:46px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' />
 
