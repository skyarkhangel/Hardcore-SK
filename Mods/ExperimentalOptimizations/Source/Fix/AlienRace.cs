using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExperimentalOptimizations.Fix
{
    [FixOn(InitStage.StaticConstructorOnStartup)]
    public class AlienRace
    {
        private static MethodInfo _canEatMethod;
        private static readonly Dictionary<RaceProperties, ThingDef> RaceDb = new Dictionary<RaceProperties, ThingDef>();

        public static void Patch()
        {
            _canEatMethod = "AlienRace.RaceRestrictionSettings:CanEat".Method(warn: false);
            
            if (_canEatMethod != null)
            {
                var canEverEat = typeof(RaceProperties).Method(nameof(RaceProperties.CanEverEat), new[] {typeof(ThingDef)});
                if (canEverEat != null)
                {
                    canEverEat.Unpatch(HarmonyPatchType.Postfix, "rimworld.erdelf.alien_race.main");
                    canEverEat.Patch(postfix: typeof(AlienRace).Method(nameof(CanEverEat)).ToHarmonyMethod());
                    Log.Message($"[ExperimentalOptimizations] AlienRace:CanEverEat patch fixed");
                }
            }
        }

        private static void CanEverEat(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (!__instance.Humanlike) return;

            ThingDef race;
            if (!RaceDb.TryGetValue(__instance, out race))
            {
                race = DefDatabase<ThingDef>.AllDefsListForReading.ToList()
                    .Concat(U.DefDatabaseAllDefs("AlienRace.ThingDef_AlienRace").Cast<ThingDef>().ToList())
                    .First(td => td.race == __instance);

                RaceDb[__instance] = race;
            }
            
            __result = __result && (bool)_canEatMethod.Invoke(null, new object[] {t, race});
        }
    }
}