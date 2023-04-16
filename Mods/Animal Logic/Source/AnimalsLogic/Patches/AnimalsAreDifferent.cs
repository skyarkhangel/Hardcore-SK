using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace AnimalsLogic
{
    /**
     *  Changes animals in caravan froming screen and trade screen to contain more important info: bond, pregnancy, training.
     */

    [HarmonyPatch(typeof(Tradeable_Pawn), "get_Label", new Type[0])]
    static class Patch_Tradeable_Pawn_Label
    {
        static void Postfix(ref String __result, Tradeable_Pawn __instance)
        {
            if (!Settings.trade_tags)
                return;

            Pawn p = (Pawn)__instance.AnyThing;
            if (!p.RaceProps.Animal) return;


            String e = AnimalImportantInfoUtil.AnimalImportantInfo(p);
            if (e.Length > 0)
                __result = "[" + e + "] " + __result;
        }
    }

    [HarmonyPatch(typeof(TransferableOneWay), "get_Label", new Type[0])]
    static class Patch_TransferableOneWay_Label
    {
        static void Postfix(ref String __result, TransferableOneWay __instance)
        {
            if (!Settings.trade_tags)
                return;

            if (__instance.AnyThing == null || !(__instance.AnyThing is Pawn)) return;

            Pawn p = (Pawn)__instance.AnyThing;
            if (!p.RaceProps.Animal) return;

            String e = AnimalImportantInfoUtil.AnimalImportantInfo(p, true);
            if (e.Length > 0)
                __result = "[" + e + "] " + __result;
        }
    }

    static class AnimalImportantInfoUtil
    {
        public static string AnimalImportantInfo(Pawn p, bool gender = false)
        {
            String e = "";

            if (!Settings.trade_tags)
                return e;

            // M/F
            if (gender && p.RaceProps.hasGenders)
            {
                if (e.Length > 0)
                    e += ";";
                e += p.gender.ToString().Substring(0, 1);
            }

            // [T]rained
            if (p.training != null)
            {
                int trained = 0;
                foreach (TrainableDef current2 in DefDatabase<TrainableDef>.AllDefs)
                {
                    if (p.training.HasLearned(current2))
                    {
                        trained++;
                    }
                }
                if (trained > 0)
                {
                    if (e.Length > 0)
                        e += ";";
                    e += "T" + trained;
                }
            }

            // [W]ool
            CompShearable wool = p.TryGetComp<CompShearable>();
            if (wool != null && wool.Fullness > 0.05)
            {
                if (e.Length > 0)
                    e += ";";
                e += "W" + wool.Fullness.ToStringPercent();
            }

            return e;
        }
    }

    /*
    public class AnimalProductionInfo : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!Settings.extra_display_stats)
                return false;

            ThingDef thingDef = GetThingDef(req);
            if (thingDef == null)
            {
                return false;
            }
            return thingDef.race?.Animal == true;
        }

        public override bool IsDisabledFor(Thing thing)
        {
            if (thing == null || !Settings.extra_display_stats)
            {
                return true;
            }
            return thing.def?.race?.Animal != true;
        }

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            return "AL_PRD_LABEL".Translate();
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            return "";
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return "";
        }

        private ThingDef GetThingDef(StatRequest req)
        {
            object obj = req.Def as ThingDef;
            if (obj == null)
            {
                Pawn pawn = req.Pawn;
                if (pawn == null)
                {
                    return null;
                }
                obj = pawn.def;
            }
            return (ThingDef)obj;
        }
    }
    */

    public static class AnimalProductionInfo
    {
        public static void Patch()
        {
            AnimalsLogic.harmony.Patch(
                typeof(RaceProperties).GetMethod("SpecialDisplayStats"),
                postfix: new HarmonyMethod(typeof(AnimalProductionInfo).GetMethod(nameof(ProductionInfo)))
                );
        }

        [HarmonyPostfix]
        public static void ProductionInfo(ref IEnumerable<StatDrawEntry> __result, RaceProperties __instance, ThingDef parentDef, StatRequest req)
        {
            if (!Settings.extra_display_stats)
                return;

            // Create a modifyable list
            List<StatDrawEntry> NewList = new List<StatDrawEntry>();

            // copy vanilla entries into the new list
            // since original method returns enumerator instead of collection, to actually get values iterator should be iterated; also this method should not be used for enumerators that change condition when called - it may be bad a idea for work givers / drivers
            foreach (StatDrawEntry entry in __result)
                NewList.Add(entry);

            int startOrder = 2150;
            StatCategoryDef StatCategory = StatCategoryDefOf.PawnMisc;

            Pawn pawn = null;
            if (req.HasThing && req.Thing is Pawn)
                pawn = (req.Thing as Pawn);

            if (pawn == null)
                return;

            foreach (ThingComp comp in pawn.AllComps)
            {
                CompProperties c = comp.props;
                if (c is CompProperties_Shearable shearable)
                {
                    NewList.Add(new StatDrawEntry(StatCategory, "WoolType".Translate(), shearable.woolDef.LabelCap, "Stat_Race_WoolType_Desc".Translate(), startOrder--, null, new Dialog_InfoCard.Hyperlink[1]
                    {
                        new Dialog_InfoCard.Hyperlink(shearable.woolDef)
                    }));
                    TaggedString woolStat = shearable.woolAmount + " / " + string.Format("{0:0.#}", shearable.shearIntervalDays) + "LetterDay".Translate();
                    if (shearable.shearIntervalDays != 1)
                        woolStat += " (" + string.Format("{0:0.#}", shearable.woolAmount / shearable.shearIntervalDays) + "/" + "LetterDay".Translate() + ")";
                    NewList.Add(new StatDrawEntry(StatCategory, "WoolAmount".Translate(), woolStat, "Stat_Race_WoolAmount_Desc".Translate(), startOrder--));
                }
                else if (c is CompProperties_Milkable milkable)
                {
                    NewList.Add(new StatDrawEntry(StatCategory, "MilkType".Translate(), milkable.milkDef.LabelCap, "Stat_Race_MilkType_Desc".Translate(), startOrder--, null, new Dialog_InfoCard.Hyperlink[1]
                    {
                        new Dialog_InfoCard.Hyperlink(milkable.milkDef)
                    }));
                    String milkStat = milkable.milkAmount + " / " + string.Format("{0:0.#}", milkable.milkIntervalDays) + "LetterDay".Translate();
                    if (milkable.milkIntervalDays != 1)
                        milkStat += " (" + string.Format("{0:0.#}", milkable.milkAmount / milkable.milkIntervalDays) + "/" + "LetterDay".Translate() + ")";
                    NewList.Add(new StatDrawEntry(StatCategory, "MilkAmount".Translate(), milkStat, "Stat_Race_MilkAmount_Desc".Translate(), startOrder--));
                    NewList.Add(new StatDrawEntry(StatCategory, "MilkFemale".Translate(), milkable.milkFemaleOnly ? "Yes".Translate() : "No".Translate(), "MilkFemaleExplanation".Translate(), startOrder--));
                }
                // catch all for custom comps based on CompHasGatherableBodyResource abstract
                else if (comp is CompHasGatherableBodyResource resource)
                {
                    ThingDef ResourceDef = (ThingDef)AccessTools.Method(comp.GetType(), "get_ResourceDef").Invoke(resource, null);
                    Log.Warning("AL: resource " + ResourceDef);
                    if (ResourceDef == null)
                    {
                        Log.Warning("Animals Logic: " + parentDef.defName + " has a custom comp for body resources but no resource def.");
                        continue;
                    }
                    NewList.Add(new StatDrawEntry(StatCategory, "AL_ResourceType".Translate(), ResourceDef.LabelCap, "AL_ResourceType_Desc".Translate(), startOrder--, null, new Dialog_InfoCard.Hyperlink[1]
                    {
                        new Dialog_InfoCard.Hyperlink(ResourceDef)
                    }));

                    int ResourceAmount = (int)AccessTools.Method(comp.GetType(), "get_ResourceAmount").Invoke(resource, null);
                    int GatherResourcesIntervalDays = (int)AccessTools.Method(comp.GetType(), "get_GatherResourcesIntervalDays").Invoke(resource, null);

                    String resourceStat = ResourceAmount + " / " + string.Format("{0:0.#}", GatherResourcesIntervalDays) + "LetterDay".Translate();
                    if (GatherResourcesIntervalDays != 1)
                        resourceStat += " (" + string.Format("{0:0.#}", ResourceAmount / GatherResourcesIntervalDays) + "/" + "LetterDay".Translate() + ")";
                    NewList.Add(new StatDrawEntry(StatCategory, "AL_ResourceAmount".Translate(), resourceStat, "AL_ResourceAmount_Desc".Translate(), startOrder--));
                }

                if (c is CompProperties_EggLayer layer)
                {
                    List<Dialog_InfoCard.Hyperlink> hyperlinks = new List<Dialog_InfoCard.Hyperlink>
                    {
                        new Dialog_InfoCard.Hyperlink(layer.eggFertilizedDef)
                    };
                    if (layer.eggUnfertilizedDef != null && layer.eggUnfertilizedDef != layer.eggFertilizedDef)
                        hyperlinks.Add(new Dialog_InfoCard.Hyperlink(layer.eggUnfertilizedDef));

                    NewList.Add(new StatDrawEntry(StatCategory, "EggType".Translate(), layer.eggFertilizedDef.LabelCap, "Stat_Race_EggType_Desc".Translate(), startOrder--, null, hyperlinks));
                    TaggedString eggsStat = (layer.eggCountRange.min == layer.eggCountRange.max ? layer.eggCountRange.max.ToString() : layer.eggCountRange.ToString())
                        + " / " + string.Format("{0:0.#}", layer.eggLayIntervalDays) + "LetterDay".Translate();
                    if (layer.eggLayIntervalDays != 1 || layer.eggCountRange.min != layer.eggCountRange.max)
                        eggsStat += " (" + string.Format("{0:0.#}", layer.eggCountRange.Average / layer.eggLayIntervalDays) + "/" + "LetterDay".Translate() + ")";
                    NewList.Add(new StatDrawEntry(StatCategory, "EggAmount".Translate(), eggsStat, "Stat_Race_EggAmount_Desc".Translate(), startOrder--));
                    NewList.Add(new StatDrawEntry(StatCategory, "EggFemale".Translate(), layer.eggLayFemaleOnly ? "Yes".Translate() : "No".Translate(), "EggFemaleExplanation".Translate(), startOrder--));
                    NewList.Add(new StatDrawEntry(StatCategory, "EggNeedFertilization".Translate(), layer.eggProgressUnfertilizedMax < 1 ? "Yes".Translate() : "No".Translate(), "EggNeedFertilizationExplanation".Translate(), startOrder--));

                    CompProperties_Hatcher hatcher = layer.eggFertilizedDef.GetCompProperties<CompProperties_Hatcher>();
                    if (hatcher != null)
                    {
                        NewList.Add(new StatDrawEntry(StatCategory, "EggIncubation".Translate(), hatcher.hatcherDaystoHatch + "LetterDay".Translate(), "EggIncubationExplanation".Translate(), startOrder--));
                    }
                }
            }

            // TODO: break it down into explanation of each stage
            if (!__instance.lifeStageAges.NullOrEmpty())
            {
                LifeStageAge matureAge = __instance.lifeStageAges.FirstOrFallback(
                    p => p.def.reproductive || p.def.milkable || p.def.shearable,
                    __instance.lifeStageAges.Last()
                    );

                if (matureAge != null)
                {
                    float age = matureAge.minAge; // contains number of years as float
                    int years = (int)age;
                    age %= 1.0f; // now contains number of years that is less than 1
                    int days = (int)(age * 60);
                    age = (age * 60) % 1.0f; // now contains number of days that is less than 1
                    int hours = (int)age * 24;

                    String matureAgeString = ""; // unused now
                    String matureAgeStringShort = "";

                    if (years > 1)
                    {
                        matureAgeString += "PeriodYears".Translate(years);
                        matureAgeStringShort += years + "LetterYear".Translate();
                    }
                    if (years == 1)
                    {
                        matureAgeString += "Period1Year".Translate();
                        matureAgeStringShort += 1 + "LetterYear".Translate();
                    }

                    if (days > 1)
                    {
                        if (years > 0)
                        {
                            matureAgeString += " ";
                            matureAgeStringShort += " ";
                        }
                        matureAgeString += "PeriodDays".Translate(days);
                        matureAgeStringShort += days + "LetterDay".Translate();
                    }
                    if (days == 1)
                    {
                        if (years > 0)
                        {
                            matureAgeString += " ";
                            matureAgeStringShort += " ";
                        }
                        matureAgeString += "Period1Day".Translate();
                        matureAgeStringShort += 1 + "LetterDay".Translate();
                    }

                    if (hours > 1)
                    {
                        if (years + hours > 0)
                        {
                            matureAgeString += " ";
                            matureAgeStringShort += " ";
                        }
                        matureAgeString += "PeriodHours".Translate(hours);
                        matureAgeStringShort += hours + "LetterHour".Translate();
                    }
                    if (hours == 1)
                    {
                        if (years + hours > 0)
                        {
                            matureAgeString += " ";
                            matureAgeStringShort += " ";
                        }
                        matureAgeString += "Period1Hour".Translate();
                        matureAgeStringShort += 1 + "LetterHour".Translate();
                    }

                    NewList.Add(new StatDrawEntry(StatCategory, "TimeToMature".Translate(), matureAgeStringShort, "TimeToMatureExplanation".Translate(), startOrder--));
                }

                LifeStageAge reproductiveAge = __instance.lifeStageAges.Find(
                    p => p.def.reproductive
                    );

                if (__instance.hasGenders && reproductiveAge != null && !parentDef.HasComp(typeof(CompEggLayer)))
                {
                    NewList.Add(new StatDrawEntry(StatCategory, "GestationPeriod".Translate(), String.Format("{0:0.#}", __instance.gestationPeriodDays) + "LetterDay".Translate(), "GestationPeriodExplanation".Translate(), startOrder--));

                    float litterAvg = 1f;
                    String litterSize = "1";

                    if (__instance.litterSizeCurve != null && Math.Max(1, __instance.litterSizeCurve.First().x) != Math.Max(1, __instance.litterSizeCurve.Last().x))
                    {
                        litterAvg = Rand.ByCurveAverage(__instance.litterSizeCurve);
                        litterAvg = Math.Max(1, litterAvg);
                        litterSize = Math.Max(1, __instance.litterSizeCurve.First().x) + "~" + Math.Max(1, __instance.litterSizeCurve.Last().x) + " (" + String.Format("{0:0.#}", litterAvg) + ")";
                    }
                    NewList.Add(new StatDrawEntry(StatCategory, "LitterSize".Translate(), litterSize, "LitterSizeExplanation".Translate(), startOrder--));
                }
            }

            // convert list to IEnumerable to match the caller's expectations
            IEnumerable<StatDrawEntry> output = NewList;

            // make caller use the list
            __result = output;
        }
    }

    public static class ShowAnimalRelations
    {
        public static void Patch()
        {
            AnimalsLogic.harmony.Patch(
                AccessTools.Method(typeof(SocialCardUtility), "ShouldShowPawnRelations"),
                postfix: new HarmonyMethod(typeof(ShowAnimalRelations).GetMethod(nameof(ShouldShowPawnRelations_Postfix)))
                );
        }

        [HarmonyPostfix]
        public static void ShouldShowPawnRelations_Postfix(ref bool __result, Pawn pawn, Pawn selPawnForSocialInfo)
        {

            if (__result || !Settings.always_show_relations)
                return;

            if (pawn.relations.everSeenByPlayer && pawn.RaceProps.Animal && (pawn.Name == null || pawn.Name.Numerical))
                __result = true;

            return;
        }
    }
}