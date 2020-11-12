using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using RimWorld;

namespace CommonSense
{
    [StaticConstructorOnStartup]
    public class CommonSense : Mod
    {
        public static Settings Settings;
        public CommonSense(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("net.avilmask.rimworld.mod.CommonSense");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //HarmonyMethod hm = new HarmonyMethod(typeof(IngredientPriority.WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_CommonSensePatch), nameof(IngredientPriority.WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_CommonSensePatch.Prefix), null);
            //var mi = AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet_NoMix", null, null);
            //harmony.Patch(mi, hm);
            base.GetSettings<Settings>();
        }
        
        public void Save()
        {
            LoadedModManager.GetMod<CommonSense>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "CommonSense";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}
