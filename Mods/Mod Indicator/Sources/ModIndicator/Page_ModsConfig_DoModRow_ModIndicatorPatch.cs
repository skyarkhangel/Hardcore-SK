using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ModIndicator
{
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("DoModRow")]
    public class Page_ModsConfig_DoModRow_ModIndicatorPatch
    {
        private static readonly ModListerSettingsDef modListerSettingsDef = ModListerSettingsDefOfLocal.ModListerSettingsDef;

        private static float timer = 0f;
        //private static float originalWidth;

        private static Dictionary<string, ModIndicator> idByIndicator;
        private static void Postfix(ref Rect r, ModMetaData mod, List<ModMetaData> list, ref int index, ref bool isDragged)
        {
            if (idByIndicator == null)
            {
                idByIndicator = new Dictionary<string, ModIndicator>();
            }
            //originalWidth = r.width;
            Rect statusRect = new Rect(r.x + r.width - r.height, r.y, r.height, r.height);
            statusRect = statusRect.ScaledBy(0.5f);

            string tip = $"{ModTypeDefOfLocal.UnknownCompatibility.LabelCap}\n\n{ModTypeDefOfLocal.UnknownCompatibility.description}";
            Color color = ModTypeDefOfLocal.UnknownCompatibility.color;

            ModIndicator modIndicator;

            if (!idByIndicator.TryGetValue(mod.PackageId, out modIndicator))
            {
                //Log.Message("idByIndicator is true, Found: " +mod.PackageId + ". Is Workshop Mod? : " + mod.OnSteamWorkshop);
                if (!mod.OnSteamWorkshop)
                {
                    idByIndicator.Add(mod.PackageId, modListerSettingsDef.modIndicators.Find(x => x.mod.ToLower() == mod.PackageId.ToLower()));
                }
                else
                {
                    idByIndicator.Add(mod.PackageId, modListerSettingsDef.modIndicators.Find(x => x.mod.ToLower() == mod.PackageId.ToLower().Replace("_steam","") /*&& !x.modTypeDef.defName.ToLower().Contains("workshop")*/));
                }
            }

            if (timer > 0)
                timer--;

            if (modIndicator != null)
            {
                color = modIndicator.modTypeDef.color;
                tip = $"{modIndicator.modTypeDef.LabelCap}\n\n{modIndicator.modTypeDef.description}";

                if (!modIndicator.link.NullOrEmpty())
                {
                    //Log.Message("Link found, setting up...");
                    Rect iconRect = new Rect(statusRect.x - r.height, r.y, r.height, r.height);

                    if(timer <= 0 && Mouse.IsOver(iconRect) && Input.GetMouseButtonUp(0))
                    {
                        //SoundDefOf.Mouseover_Standard.PlayOneShotOnCamera();
                        timer = 500;
                        Application.OpenURL(modIndicator.link);
                    }
                    if (mod.OnSteamWorkshop && (mod.Active || mod.enabled) && !modIndicator.link.Contains("steamcommunity.com"))
                    {
                        //Log.Message("Workshop mod found...");
                        if (!modIndicator.requiredMods.NullOrEmpty() && modIndicator.modTypeDef.defName.ToLower().Contains("workshop") && !modIndicator.modTypeDef.defName.ToLower().Contains("incompatible"))
                        {
                            //Log.Message("Required Mods Needed:");
                            string required = "";
                            foreach (var id in modIndicator.requiredMods)
                            {
                                if (id == "None")
                                {
                                    required = "None";
                                    break;
                                }

                                Log.Message(id);
                                required += id;
                                if (modIndicator.requiredMods.Last() != id)
                                    required += ",";
                            }
                            if (required != "None" && !ModsConfig.AreAllActive(required))
                            {
                                //Log.Message("Required mods missing");
                                tip = tip.Insert(0, "HSKPatchAvailable".Translate() + "\n\n");
                                tip += "\n\nMods Required:\n\n";
                                foreach (var modID in modIndicator.requiredMods)
                                {
                                    ModMetaData meta = ModLister.GetModWithIdentifier(modID);
                                    if (meta != null && !ModsConfig.IsActive(meta))
                                        tip += meta.Name + "\n";
                                    else if (meta == null)
                                        tip += modID + "\n";
                                }
                                //Log.Message("Patch link added");
                                color = new Color(1f, 0.5f, 0f);
                                Widgets.ButtonImage(iconRect, TextureResources.ExclamationMarkIcon);
                            }
                            else if (required == "None")
                            {
                                tip += "\n\n" + "HSKInfoAvailable".Translate();
                                Widgets.ButtonImage(iconRect, TextureResources.TexButtonInfo);
                            }
                        }
                        else
                        {
                            //Log.Message("HSK Version link added");
                            tip = tip.Insert(0, "HSKVersionAvailable".Translate() + "\n\n");
                            color = Color.red;
                            Widgets.ButtonImage(iconRect, TextureResources.ExclamationMarkIcon);
                        }
                    }
                    else
                    {
                        //Log.Message("Standard link added");
                        Widgets.ButtonImage(iconRect, TextureResources.TexButtonInfo);
                    }

                }
            }

            Widgets.DrawBoxSolid(statusRect, color);
            if (Mouse.IsOver(r))
            {
                TooltipHandler.TipRegion(r, tip);
            }
        }
    }
}
