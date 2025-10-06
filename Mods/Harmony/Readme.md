# RimWorld Harmony Library Mod

This mod brings Harmony into RimWorld modding.

![Harmony](https://raw.githubusercontent.com/pardeike/Harmony/master/HarmonyLogo.png)  
GitHub Repository: [Harmony](https://github.com/pardeike/Harmony)

Instead of including `0Harmony.dll` in your `Assemblies` folder, you use the Harmony reference just for compiling and exclude it from being copied to the folder on build. Then, you add the following to your `About.xml` file:

```
<modDependencies>
    <li>
        <packageId>brrainz.harmony</packageId>
        <displayName>Harmony</displayName>
        <steamWorkshopUrl>steam://url/CommunityFilePage/2009463077</steamWorkshopUrl>
        <downloadUrl>https://github.com/pardeike/HarmonyRimWorld/releases/latest</downloadUrl>
    </li>
</modDependencies>
```

which will make RimWorld 1.1 force the user to install this mod. It will automatically want to be installed high up in the list which makes it supply `Harmony` to all mods below. This means that all mods will use **the same Harmony version**.

Whenever Harmony needs updating, this mod will update too. Unless it is a breaking change (we will avoid making those). In which case, a second Harmony mod will be created so users can use either this one or the new one.

/Andreas "Brrainz" Pardeike
