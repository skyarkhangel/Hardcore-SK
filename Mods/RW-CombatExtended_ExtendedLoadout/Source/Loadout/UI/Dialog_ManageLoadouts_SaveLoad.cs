using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Serialization;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

#region CE Serialize
[Serializable]
public class LoadoutConfig
{
    public string label;
    public LoadoutSlotConfig[] slots;
}

[Serializable]
public class LoadoutSlotConfig
{
    public bool isGenericDef;
    public string defName;
    public int count;
}

[Serializable]
public class LoadoutConfigs
{
    public LoadoutConfig[] configs;
}

public static class LoadUtil
{
    public static LoadoutSlotConfig ToConfig(this LoadoutSlot loadoutSlot)
    {
        return new LoadoutSlotConfig
        {
            isGenericDef = loadoutSlot._type == typeof(LoadoutGenericDef),
            defName = loadoutSlot._def.defName,
            count = loadoutSlot._count
        };
    }

    public static LoadoutConfigs ToConfig(this IEnumerable<Loadout> loadouts)
    {
        return new LoadoutConfigs { configs = loadouts.Select(x => ToConfig(x)).ToArray() };
        /*LoadoutConfigs result= new LoadoutConfigs();
        foreach(var loadout in loadouts)
        {
            result.configs.AddItem(ToConfig(loadout));
        }
        return result;*/
    }

    public static Loadout[] FromConfig(LoadoutConfigs loadoutConfig, out List<string> unloadableDefNames)
    {
        var result = new List<Loadout>();
        unloadableDefNames = new List<string>();
        foreach (var cfg in loadoutConfig.configs)
        {
            result.Add(FromConfig(cfg, out List<string> defs));
            unloadableDefNames.AddRange(defs);
        }
        return result.ToArray();
    }

    public static LoadoutConfig ToConfig(this Loadout loadout)
    {
        /*int i = 0;
        LoadoutSlotConfig[] temp = new LoadoutSlotConfig[loadout.ToConfig().slots.Length];
        Log.Message("temp created");
        foreach (var loadoutSlot in loadout.ToConfig().slots)
        {
            Traverse traverse = new Traverse(loadoutSlot);
            Log.Message($"isGeneric {traverse.Field("isGenericDef").GetValue()}\ndefName {traverse.Field("defName").GetValue()}\ncount {traverse.Field("count").GetValue()}");
            temp.AddItem(new LoadoutSlotConfig { 
                isGenericDef = loadoutSlot.isGenericDef,
                defName = loadoutSlot.defName,
                count= loadoutSlot.count
            });
            *//*Log.Message("isGeneric");
            temp[i].isGenericDef = loadoutSlot.isGenericDef;
            Log.Message("defName");
            temp[i].defName = loadoutSlot.defName;
            Log.Message("Count");
            temp[i].count = loadoutSlot.count;
            i++;*//*
        }
        Log.Message("temp added");*/
        return new LoadoutConfig
        {
            label = loadout.label,
            slots = loadout.ToConfig().slots.Select(x => new LoadoutSlotConfig { isGenericDef = x.isGenericDef,defName = x.defName,count= x.count}).ToArray()
        };
    }

    static bool IsUniqueLoadoutLabel(this string label)
    {
        LoadoutManager manager = Current.Game.GetComponent<LoadoutManager>();
        // For consistency with the 'GetUniqueLabel' behavior
        if (manager == null)
        {
            return false;
        }
        return !manager._loadouts.Any(l => l.label == label);
    }

    public static LoadoutSlot FromConfig(LoadoutSlotConfig loadoutSlotConfig)
    {
        if (loadoutSlotConfig.isGenericDef)
        {
            LoadoutGenericDef genericThingDef = DefDatabase<LoadoutGenericDef>.GetNamed(loadoutSlotConfig.defName, false);
            return genericThingDef == null ? null : new LoadoutSlot(genericThingDef, loadoutSlotConfig.count);
        }

        var thingDef = DefDatabase<ThingDef>.GetNamed(loadoutSlotConfig.defName, false);
        return thingDef == null ? null : new LoadoutSlot(thingDef, loadoutSlotConfig.count);
    }

    public static Loadout FromConfig(LoadoutConfig loadoutConfig, out List<string> unloadableDefNames)
    {
        // Create the new loadout, preventing name clashes if the loadout already exists
        string uniqueLabel = loadoutConfig.label.IsUniqueLoadoutLabel()
            ? loadoutConfig.label
            : LoadoutManager.GetUniqueLabel(loadoutConfig.label);

        Loadout loadout = new Loadout(uniqueLabel);

        unloadableDefNames = new List<string>();

        // Now create each of the slots
        foreach (LoadoutSlotConfig loadoutSlotConfig in loadoutConfig.slots)
        {
            LoadoutSlot loadoutSlot = FromConfig(loadoutSlotConfig);
            // If the LoadoutSlot could not be loaded then continue loading the others as this most likely means
            // that the current game does not have the mod loaded that was used to create the initial loadout.
            if (loadoutSlot == null)
            {
                unloadableDefNames.Add(loadoutSlotConfig.defName);
                continue;
            }
            loadout.AddSlot(FromConfig(loadoutSlotConfig));
        }

        return loadout;
    }
}

#endregion CE Serialize


[HarmonyPatch(typeof(Dialog_ManageLoadouts))]
[HotSwappable]
public static class Dialog_ManageLoadouts_SaveLoad
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    [HarmonyPatch(nameof(Dialog_ManageLoadouts.DoWindowContents))]
    [HarmonyPostfix]
    public static void Postfix(Dialog_ManageLoadouts __instance, Rect canvas)
    {
        var saveRect = canvas.RightPartPixels(Dialog_ManageLoadouts._topAreaHeight + Dialog_ManageLoadouts._margin);
        saveRect.height = saveRect.width = Dialog_ManageLoadouts._topAreaHeight;
        TooltipHandler.TipRegion(saveRect, new TipSignal("CE_Extended.SaveLoadTip".Translate()));
        if (Widgets.ButtonImage(saveRect, Textures.Save))
        {
            Find.WindowStack.Add(new Dialog_SaveLoad());
        }
    }
}

public class Dialog_SaveLoad : Window
{
    private const bool closeOnSave = false;
    private const int elementHeight = 25;
    private const int margin = 5;
    private string _saveFileName;
    private List<(string name, Loadout[] loadouts, LoadStatus status, string loadStatusMessage)> _files;
    private int _selectedFile = -1, _previousSelectedFile = -1;
    private string _savePath = GenFilePaths.SaveDataFolderPath + "/CE.ExtendedLoadouts";
    private Dictionary<Loadout, bool> _checkState = new();
    private Vector2 _loaudoutsScroll, _filesScroll;
    private Dictionary<string, ThingDef> _firstGenericCache = new();

    public override Vector2 InitialSize => new(900, 600);

    public enum LoadStatus
    {
        Fine,
        Warning,
        Error
    }

    public Dialog_SaveLoad()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(_savePath);
        if (!directoryInfo.Exists)
            directoryInfo.Create();
        doCloseButton = false;
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        draggable = true;
        ReloadFiles();
    }

    private void ReloadFiles()
    {
        _files = new();
        foreach (var file in Directory.GetFiles(_savePath, "*.xml"))
        {
            try
            {
                var mySerializer = new XmlSerializer(typeof(LoadoutConfigs));
                using var myFileStream = new FileStream(new FileInfo(file).FullName, FileMode.Open);
                var loadoutConfigs = (LoadoutConfigs)mySerializer.Deserialize(myFileStream);
                var loadouts = LoadUtil.FromConfig(loadoutConfigs, out List<string> unloadableDefNames);
                _files.Add(new(
                    Path.GetFileNameWithoutExtension(file),
                    loadouts,
                    unloadableDefNames.Any() ? LoadStatus.Warning : LoadStatus.Fine,
                    unloadableDefNames.Any() ? "unloadableDefNames:\n" + string.Join("\n", unloadableDefNames) : string.Empty));
            }
            catch (Exception ex)
            {
                _files.Add(new(Path.GetFileNameWithoutExtension(file), null, LoadStatus.Error, ex.ToString()));
            }
        }
    }

    private Loadout[] GetLoadouts()
    {
        return _selectedFile == -1 ? LoadoutManager.Loadouts.Where(x => !x.defaultLoadout).ToArray() : _files[_selectedFile].loadouts;
    }

    private static Texture2D StatusToTexture(LoadStatus status) => status switch
    {
        LoadStatus.Fine => Textures.Fine,
        LoadStatus.Warning => Textures.Warning,
        LoadStatus.Error => Textures.Error,
        _ => throw new ArgumentException("Unknown status")
    };

    private Loadout[] GetCheckedLoadouts() => _checkState.Where(x => x.Value).Select(x => x.Key).ToArray();

    private void DrawSaveFile(Rect rect, int fileNum)
    {
        float yPos = (fileNum + 1) * (elementHeight + margin * 2/* top and bottom*/);
        Rect lineRect = new(rect.x, rect.y + yPos, rect.width, elementHeight + margin * 2);
        Rect inlineRect = lineRect.ContractedBy(margin);

        float fileNameWidth = inlineRect.width - 3 * (elementHeight + margin);
        Rect loadStatusRect = new(inlineRect.x, inlineRect.y, elementHeight, elementHeight);
        Rect fileNameRect = new(loadStatusRect.xMax + margin, inlineRect.y, fileNameWidth, elementHeight);
        Rect saveOrLoadIconRect = new(fileNameRect.xMax + margin, inlineRect.y, elementHeight, elementHeight);
        Rect deleteIconRect = new(saveOrLoadIconRect.xMax + margin, inlineRect.y, elementHeight, elementHeight);
        if (fileNum == -1)
        {
            // new file text entry
            _saveFileName = Widgets.TextField(fileNameRect, _saveFileName);

            if (Mouse.IsOver(saveOrLoadIconRect))
                TooltipHandler.TipRegion(saveOrLoadIconRect, "CE_SL.Save".Translate());
            if (Widgets.ButtonImage(saveOrLoadIconRect, Textures.Save))
            {
                if (!string.IsNullOrWhiteSpace(_saveFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoadoutConfigs));
                    /*foreach(var loadout in GetCheckedLoadouts())
                        Log.Message($"ID:{loadout.uniqueID}\nLabel:{loadout.label}");*/
                    using (TextWriter writer = new StreamWriter(new FileInfo(Path.Combine(_savePath, _saveFileName + ".xml")).FullName))
                    {
                        xmlSerializer.Serialize(writer, GetCheckedLoadouts().ToConfig());
                    }
                    ReloadFiles();
                    if (closeOnSave)
                        Close();
                }
            }
        }
        else
        {
            // existing file
            var file = _files[fileNum];
            Widgets.Label(fileNameRect, file.name);

            if (file.status != LoadStatus.Error)
            {
                if (Mouse.IsOver(saveOrLoadIconRect))
                    TooltipHandler.TipRegion(saveOrLoadIconRect, "CE_SL.Load".Translate());
                if (Widgets.ButtonImage(saveOrLoadIconRect, Textures.Load))
                {
                    foreach (var loudout in GetCheckedLoadouts())
                    {
                        LoadoutManager.AddLoadout(loudout);
                    }
                    Close();
                }
            }
            if (Mouse.IsOver(deleteIconRect))
                TooltipHandler.TipRegion(deleteIconRect, "CE_SL.Delete".Translate());
            if (Widgets.ButtonImage(deleteIconRect, Textures.Delete))
            {
                File.Delete(Path.Combine(_savePath, file.name + ".xml"));
                _files.Remove(file);
                _selectedFile = -1;
            }

            GUI.DrawTexture(loadStatusRect, StatusToTexture(file.status));
            if (Mouse.IsOver(loadStatusRect))
            {
                TooltipHandler.TipRegion(loadStatusRect, file.loadStatusMessage);
                Widgets.DrawHighlight(loadStatusRect);
            }
        }

        if (Input.GetMouseButtonDown(0) && Mouse.IsOver(lineRect))
        {
            _selectedFile = fileNum;
        }
        if (_selectedFile == fileNum)
        {
            Widgets.DrawHighlight(lineRect);
        }
    }

    private void DrawLoadoutsButtons(Rect rect)
    {
        Rect checkRect = new(rect.x, rect.y, elementHeight, elementHeight);
        Rect uncheckRect = new(checkRect.xMax + margin, rect.y, elementHeight, elementHeight);

        if (Mouse.IsOver(checkRect))
            TooltipHandler.TipRegion(checkRect, "CE_SL.CheckAll".Translate());
        if (Widgets.ButtonImage(checkRect, Textures.Check))
        {
            _checkState = _checkState.ToDictionary(x => x.Key, y => true);
        }
        if (Mouse.IsOver(uncheckRect))
            TooltipHandler.TipRegion(uncheckRect, "CE_SL.UncheckAll".Translate());
        if (Widgets.ButtonImage(uncheckRect, Textures.Uncheck))
        {
            _checkState = _checkState.ToDictionary(x => x.Key, y => false);
        }
    }

    private void DrawLoadouts(Rect rect, int fileNum, Loadout loadout)
    {
        float yPos = fileNum * (elementHeight + margin);
        Rect lineRect = new(rect.x, rect.y + yPos, rect.width, elementHeight + margin);

        if (!_checkState.TryGetValue(loadout, out bool state))
        {
            _checkState.Add(loadout, state = true);
        }

        Widgets.CheckboxLabeled(lineRect, loadout.LabelCap, ref state);
        _checkState[loadout] = state;
        if (Mouse.IsOver(lineRect))
        {
            TooltipHandler.TipRegion(lineRect, string.Join("\n", loadout.Slots.Select(x => $"{x.LabelCap} x{x.count}")));
            Widgets.DrawHighlight(lineRect);
        }

        int slotCount = (loadout.Slots.Count < 0) ? 0 : (loadout.Slots.Count > 5) ? 5 : loadout.Slots.Count; // clamp 0:5
        var iconsRect = lineRect.RightPartPixels((elementHeight) * (slotCount + 1));
        for (int i = 0; i < slotCount; i++)
        {
            Rect iconRect = new(iconsRect) { width = elementHeight, height = elementHeight };
            iconRect.x += i * (elementHeight);
            var slot = loadout.Slots[i];
            ThingDef def;
            if (slot.genericDef != null)
            {
                if (!_firstGenericCache.TryGetValue(slot.genericDef.defName, out def))
                {
                    def = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => slot.genericDef.lambda(x));
                    _firstGenericCache.Add(slot.genericDef.defName, def);
                }
            }
            else def = slot.thingDef;
            Widgets.DefIcon(iconRect, def);

            if (def != null && Mouse.IsOver(iconRect))
            {
                TooltipHandler.TipRegion(iconRect, def.DescriptionDetailed);
                Widgets.DrawHighlight(iconRect);
            }
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        // split window two parts
        var leftRect = inRect.LeftPartPixels(inRect.width / 2f);
        leftRect.yMin = leftRect.xMin = margin;
        var rightRect = inRect.RightPartPixels(inRect.width / 2f);
        rightRect.yMin = margin;
        rightRect.xMin += margin;

        // draw saves border
        Widgets.DrawMenuSection(leftRect);
        Widgets.DrawMenuSection(rightRect);
        leftRect = leftRect.ContractedBy(1);
        rightRect = rightRect.ContractedBy(1);
        rightRect.xMin += margin;

        Rect viewRect = new(leftRect.x, leftRect.y, leftRect.width - 25f, (_files.Count + 1) * (elementHeight + margin * 2/* top and bottom*/));
        Widgets.BeginScrollView(leftRect, ref _filesScroll, viewRect);
        leftRect.width -= 25f;
        DrawSaveFile(leftRect, -1);
        for (int i = 0; i < _files.Count; i++)
            DrawSaveFile(leftRect, i);
        Widgets.EndScrollView();
        //


        var rightButtonsRect = rightRect.TopPartPixels(elementHeight);
        var rightLoadoutsRect = rightRect.BottomPartPixels(rightRect.height - elementHeight);

        DrawLoadoutsButtons(rightButtonsRect);


        var loadouts = GetLoadouts();
        if (loadouts != null)
        {
            viewRect = new(rightLoadoutsRect.x, rightLoadoutsRect.y, rightLoadoutsRect.width - 25f, loadouts.Length * (elementHeight + margin));
            Widgets.BeginScrollView(rightLoadoutsRect, ref _loaudoutsScroll, viewRect);
            rightLoadoutsRect.width -= 25f;
            for (int i = 0; i < loadouts.Length; i++)
            {
                DrawLoadouts(rightLoadoutsRect, i, loadouts[i]);
            }
            Widgets.EndScrollView();
        }

        // Widgets.DrawHighlight(rightRect);

        // selected loadouts
        if (_previousSelectedFile != _selectedFile)
        {
            _previousSelectedFile = _selectedFile;
            _checkState = new();
        }
    }
}


