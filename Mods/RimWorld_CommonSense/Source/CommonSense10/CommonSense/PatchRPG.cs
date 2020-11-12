using System;
using Harmony;
using Verse;

namespace CommonSense
{
    [StaticConstructorOnStartup]
    public static class PatchRPG
    {
        // Token: 0x06000056 RID: 86 RVA: 0x000040C4 File Offset: 0x000022C4
        static PatchRPG()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("net.avilmask.rimworld.mod.CommonSense.RPGInventory");
            Type type;
            if ((type = AccessTools.TypeByName("Sandy_Detailed_RPG_GearTab")) != null)
            {
                var mi = AccessTools.Method(type, "DrawThingRow", null, null);
                //Log.Message($"mi = {mi}");
                HarmonyMethod hm = new HarmonyMethod(typeof(ITab_Pawn_Gear_DrawThingRow_CommonSensePatch), nameof(ITab_Pawn_Gear_DrawThingRow_CommonSensePatch.Prefix), null);
                harmonyInstance.Patch(mi, hm, null);
            }
        }
    }
}
