

using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[StaticConstructorOnStartup]
public class Textures
{
    public static readonly Texture2D PersonalLoadout = ContentFinder<Texture2D>.Get("UI/personalLoadout");
    public static readonly Texture2D OptimizeApparel = ContentFinder<Texture2D>.Get("UI/optimizeApparel");
    public static readonly Texture2D LoadoutLoad = ContentFinder<Texture2D>.Get("UI/loadout-load");
    public static readonly Texture2D LoadoutLoadAll = ContentFinder<Texture2D>.Get("UI/loadout-loadall");
    public static readonly Texture2D LoadoutSave = ContentFinder<Texture2D>.Get("UI/loadout-save");
    public static readonly Texture2D Error = ContentFinder<Texture2D>.Get("UI/Save/error");
    public static readonly Texture2D Warning = ContentFinder<Texture2D>.Get("UI/Save/warning");
    public static readonly Texture2D Fine = ContentFinder<Texture2D>.Get("UI/Save/fine");
    public static readonly Texture2D Load = ContentFinder<Texture2D>.Get("UI/Save/load");
    public static readonly Texture2D Save = ContentFinder<Texture2D>.Get("UI/Save/save");
    public static readonly Texture2D Delete = ContentFinder<Texture2D>.Get("UI/Save/delete");
    public static readonly Texture2D Uncheck = ContentFinder<Texture2D>.Get("UI/Save/unchek");
    public static readonly Texture2D Check = ContentFinder<Texture2D>.Get("UI/Save/check");
}

[StaticConstructorOnStartup]
public class Strings
{
    public static readonly string OptimizeApparelDesc = "Settings.OptimizeApparel.Desc".Translate();
}