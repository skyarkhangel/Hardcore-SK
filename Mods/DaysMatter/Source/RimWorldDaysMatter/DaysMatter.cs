using System.Collections.Generic;
using System.Reflection;
//using HugsLib;
//using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;
//using HarmonyLib;
//using HugsLib.Core;
using Verse.AI.Group;
using RimWorld.Planet;
//using HugsLib.Settings;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace RimWorldDaysMatter
{
    public class RWDMModSettings : ModSettings
    {
        public bool PrivateAnniversaries = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref PrivateAnniversaries, "PrivateAnniversaries", true);
            base.ExposeData();
        }

        /*public void ResetPawn()
        {
            Log.Message("ResetPawnJob");
            _reset = false;
            foreach (object item in Find.Selector.SelectedObjects)
            {
                Log.Message("item");
                Pawn pawn = item as Pawn;
                if (pawn != null)
                {
                    Log.Message("Pawn " + pawn.ToString());
                    pawn.jobs.ClearQueuedJobs();
                }
            }
            foreach (Lord item in Find.CurrentMap.lordManager.lords)
            {
                Log.Message("Lord " + item.ToString());
                Log.Message("Job " + item.LordJob.ToString());
                Log.Message("owned " + item.ownedPawns.ToString());
                item.LordJob.
            }
        }*/
    }

    public class RWDMMod : Mod
    {
        public RWDMModSettings settings;

        public RWDMMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RWDMModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            list.Begin(inRect);
            list.CheckboxLabeled("PrivateAnniversaries".Translate(), ref settings.PrivateAnniversaries, "PrivateAnniversaries_Desc".Translate());
            //settings._reset = list.ButtonText("Job reset for selected pawn", "Select pawn, then press this button to reset pawn partying");
            list.GapLine();
            if (list.ButtonText("Stop ongoing party created by DaysMatter in current map"))
            {
                try
                {
                    LordManager lm = Find.CurrentMap.lordManager;
                    for (int i = lm.lords.Count - 1; i >= 0; i--)
                    {
                        if (lm.lords[i].LordJob != null && lm.lords[i].LordJob is JoinableParty)
                        {
                            //Log.Message("Removing");
                            lm.RemoveLord(lm.lords[i]);
                        }
                    }
                }
                catch
                {
                    Log.Message("Error! Tried to remove lord created by DaysMatter");
                }
                
            }
            list.GapLine();
            if (list.ButtonText("Stop ongoing systems (Lord) in current map", "It may help with never ending party bug"))
            {
                try
                {
                    LordManager lm = Find.CurrentMap.lordManager;
                    for (int i = lm.lords.Count - 1; i >= 0; i--)
                    {
                        lm.RemoveLord(lm.lords[i]);
                    }
                }
                catch
                {
                    Log.Message("Error! Tried to remove lord");
                }

            }
            list.GapLine();
            if (list.ButtonText("Remove job (LordJob) from one selected pawn", "It may help with never ending party bug"))
            {
                try
                {
                    Thing temp = Find.Selector.SingleSelectedThing;
                    if (temp != null && temp is Pawn)
                    {
                        Pawn p = temp as Pawn;
                        LordManager lm = Find.CurrentMap.lordManager;

                        for (int i = lm.lords.Count - 1; i >= 0; i--)
                        {
                            if (lm.lords[i].ownedPawns != null)
                            {
                                for (int j = lm.lords[i].ownedPawns.Count - 1; j >= 0; j--)
                                {
                                    if (lm.lords[i].ownedPawns[j] == p)
                                    {
                                        lm.lords[i].ownedPawns.RemoveAt(j);
                                    }
                                }

                                if (lm.lords[i].ownedPawns.Count == 0)
                                    lm.RemoveLord(lm.lords[i]);
                            }
                        }
                    }
                }
                catch
                {
                    Log.Message("Error! Tried to pawn from LordJob");
                }

            }
            list.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Days Matter";
        }
    }


    // MatteredDayStore included
    public class DaysMatterWorldComponent : WorldComponent 
    {
        private List<MatteredDay> _matteredDays = new List<MatteredDay>();
        private int _settlement;
        private int _birthdays;
        private int _loversAnniversaries;
        private int _marriageAnniversaries;
        //private int _deathDays;
        private int _lastHour;

        public List<MatteredDay> DMMatteredDays { get { return _matteredDays; } set { _matteredDays = value; } }
        public Duration DMSettlement { get { return (Duration)_settlement; } set { _settlement = (int)value; } }
        public Duration DMBirthdays { get { return (Duration)_birthdays; } set { _birthdays = (int)value; } }
        public Duration DMLoversAnniversaries { get { return (Duration)_loversAnniversaries; } set { _loversAnniversaries = (int)value; } }
        public Duration DMMarriageAnniversaries { get { return (Duration)_marriageAnniversaries; } set { _marriageAnniversaries = (int)value; } }
        //public Duration DeathDays { get { return (Duration)_deathDays; } set { _deathDays = (int)value; } }

        public List<Pawn> disallow; // no birthday for pawns


        public DaysMatterWorldComponent(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            //base.ExposeData();
            Scribe_Collections.Look(ref _matteredDays, false, "RWDM_customDays", LookMode.Deep, new object[0]);
            Scribe_Values.Look(ref _settlement, "RWDM_settlement", 0, true);
            Scribe_Values.Look(ref _birthdays, "RWDM_birthdays", 0, true);
            Scribe_Values.Look(ref _loversAnniversaries, "RWDM_loversAnniversaries", 0, true);
            Scribe_Values.Look(ref _marriageAnniversaries, "RWDM_marriageAnniversaries", 0, true);
            //Scribe_Values.Look(ref _deathDays, "deathDays", 0, true);
            Scribe_Values.Look(ref _lastHour, "RWDM_lastHour", -2, true); // -1 is duration.none // lastHour must != duration.none when initialized
            Scribe_Collections.Look(ref disallow, "RWDM_disallow", LookMode.Reference);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            int ticks = Find.TickManager.TicksAbs;

            ////////////// edit //// move to harmony patch of pawn death
            try
            {
                if (ticks % GenDate.TicksPerDay == 0) // clear not needed data
                {
                    if (disallow == null)
                        disallow = new List<Pawn>();

                    for (int i = disallow.Count - 1; i >= 0; i--)
                    {
                        if (disallow[i] == null || disallow[i].Dead || disallow[i].Faction != Faction.OfPlayer)
                        {
                            disallow.RemoveAt(i);
                        }
                    }
                }
            }
            catch
            {
                Log.Message("DaysMatter. Error! Something went wrong with Pawns ignore list!");
            }
            

            if (ticks % GenDate.TicksPerHour == 0)
            {
                int hour = GenDate.HourOfDay(ticks, 0);
                if (hour != _lastHour)
                {
                    _lastHour = hour;

                    List<Settlement> settlementsList = Find.WorldObjects.Settlements;
                    foreach (Settlement item in settlementsList)
                    {
                        if (item != null && item.Faction != null && item.Faction.IsPlayer && item.Map != null)//.HasMap)
                        {
                            int localHour = GenLocalDate.HourOfDay(item.Map);
                            int localDay = GenLocalDate.DayOfQuadrum(item.Map);
                            int month = GenLocalDate.DayOfYear(item.Map) / GenDate.DaysPerQuadrum;
                            if (month < 0)
                                month = 0;
                            if (month > 3)
                                month = 3;
                            Quadrum localMonth = (Quadrum)month;
                            int gameStart = Find.TickManager.gameStartAbsTick;

                            if (_settlement != 0)
                            {
                                int sTick = item.creationGameTicks + gameStart;
                                int sDay = GenDate.DayOfQuadrum(sTick, 0);
                                Quadrum sMonth = GenDate.Quadrum(sTick, 0);

                                if (sDay == localDay && sMonth == localMonth)
                                {
                                    if (localHour == 0)
                                        Messages.Message("DM.Message.TodaySettlement".Translate(item.Name), MessageTypeDefOf.PositiveEvent);
                                    else if (localHour == DMSettlement.Start())
                                        StartParty(item.Map, "DM.Letter.SettlementParty".Translate(item.Name), null, DMSettlement == Duration.AllDay);
                                }
                            }

                            if (item.Map.mapPawns != null && (_birthdays != 0 || _loversAnniversaries != 0 || _marriageAnniversaries != 0))
                            {
                                List<Pawn> pList = item.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
                                List<Pawn> ignoreList = new List<Pawn>();

                                //List<string> ignoreLoverList = new List<string>();
                                //List<string> ignoreMarriageList = new List<string>();

                                bool onlyTwo = LoadedModManager.GetMod<RWDMMod>().GetSettings<RWDMModSettings>().PrivateAnniversaries;

                                if (pList != null)
                                {
                                    if (disallow == null)
                                    {
                                        disallow = new List<Pawn>();
                                    }

                                    foreach (Pawn pawn in pList)
                                    {
                                        if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.Dead)
                                            continue;

                                        if (_birthdays != 0)
                                        {
                                            if (!disallow.Contains(pawn) && pawn.ageTracker != null)
                                            {
                                                long bTick = pawn.ageTracker.BirthAbsTicks;
                                                int bDay = GenDate.DayOfQuadrum(bTick, 0);
                                                Quadrum bMonth = GenDate.Quadrum(bTick, 0);
                                                int age = Mathf.RoundToInt(pawn.ageTracker.AgeChronologicalYearsFloat);

                                                if (bDay == localDay && bMonth == localMonth)
                                                {
                                                    if (localHour == 0)
                                                    {
                                                        Messages.Message("DM.Message.TodayBirthday".Translate(pawn.Name.ToStringShort, age), MessageTypeDefOf.PositiveEvent);
                                                    }
                                                    else if (localHour == DMBirthdays.Start())
                                                    {
                                                        ignoreList.Add(pawn);
                                                        StartParty(item.Map, "DM.Letter.BirthdayParty".Translate(pawn.Name.ToStringShort), null, DMBirthdays == Duration.AllDay, pawn);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    foreach (Pawn pawn in pList)
                                    {
                                        if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.Dead)
                                            continue;

                                        if (_loversAnniversaries != 0 && (localHour == 0 || localHour == DMLoversAnniversaries.Start()) && !ignoreList.Contains(pawn))// !ignoreLoverList.Contains(pawn.ThingID))
                                        {
                                            if (pawn.relations != null && pawn.relations.DirectRelations != null)
                                            {
                                                List<DirectPawnRelation> lovers = pawn.relations.DirectRelations.FindAll(x => x.def == PawnRelationDefOf.Lover);
                                                foreach (DirectPawnRelation relation in lovers)
                                                {
                                                    if (relation != null)
                                                    {
                                                        if (relation.otherPawn == null)
                                                            continue;
                                                        //ignoreLoverList.Add(relation.otherPawn.ThingID);
                                                        int lTick = relation.startTicks + gameStart;
                                                        int lDay = GenDate.DayOfQuadrum(lTick, 0);
                                                        Quadrum lMonth = GenDate.Quadrum(lTick, 0);

                                                        if (lDay == localDay && lMonth == localMonth)
                                                        {
                                                            if (localHour == 0)
                                                            {
                                                                Messages.Message("DM.Message.TodayRelationshipAnniversary".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), MessageTypeDefOf.PositiveEvent);
                                                            }
                                                            else if (localHour == DMLoversAnniversaries.Start())
                                                            {
                                                                //if (relation.otherPawn == null)
                                                                //{
                                                                //    Messages.Message("DM.Message.LoversCant".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
                                                                //}
                                                                if (relation.otherPawn.Map == null || relation.otherPawn.Map != pawn.Map)// !pList.Contains(relation.otherPawn))
                                                                {
                                                                    Messages.Message("DM.Message.LoversAway".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
                                                                    ignoreList.Add(relation.otherPawn);
                                                                }
                                                                else if (ignoreList.Contains(relation.otherPawn))
                                                                {
                                                                    Messages.Message("DM.Message.LoversBusy".Translate(relation.otherPawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
                                                                }
                                                                else
                                                                {
                                                                    ignoreList.Add(relation.otherPawn);
                                                                    if (onlyTwo)
                                                                    {
                                                                        StartParty(item.Map, "DM.Letter.RelationshipAnniversaryParty".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), new List<Pawn> { pawn, relation.otherPawn }, DMLoversAnniversaries == Duration.AllDay, pawn);
                                                                    }
                                                                    else
                                                                    {
                                                                        StartParty(item.Map, "DM.Letter.RelationshipAnniversaryParty".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), null, DMLoversAnniversaries == Duration.AllDay, pawn);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (_marriageAnniversaries != 0 && (localHour == 0 || localHour == DMLoversAnniversaries.Start()) && !ignoreList.Contains(pawn)) //!ignoreMarriageList.Contains(pawn.ThingID))
                                        {
                                            if (pawn.relations != null && pawn.relations.DirectRelations != null)
                                            {
                                                List<DirectPawnRelation> marriage = pawn.relations.DirectRelations.FindAll(x => x.def == PawnRelationDefOf.Spouse);
                                                foreach (DirectPawnRelation relation in marriage)
                                                {
                                                    if (relation != null)
                                                    {
                                                        if (relation.otherPawn == null)
                                                            continue;
                                                        //if (!pList.Contains(relation.otherPawn))
                                                        //    continue;
                                                        //ignoreMarriageList.Add(relation.otherPawn.ThingID);
                                                        int mTick = relation.startTicks + gameStart;
                                                        int mDay = GenDate.DayOfQuadrum(mTick, 0);
                                                        Quadrum mMonth = GenDate.Quadrum(mTick, 0);

                                                        if (mDay == localDay && mMonth == localMonth)
                                                        {
                                                            if (localHour == 0)
                                                            {
                                                                Messages.Message("DM.Message.TodayMarriageAnniversary".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), MessageTypeDefOf.PositiveEvent);
                                                            }
                                                            else if (localHour == DMMarriageAnniversaries.Start())
                                                            {
                                                                if (relation.otherPawn.Map == null || relation.otherPawn.Map != pawn.Map)
                                                                {
                                                                    Messages.Message("DM.Message.SpousesAway".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
                                                                    ignoreList.Add(relation.otherPawn);
                                                                }
                                                                else if (ignoreList.Contains(relation.otherPawn))
                                                                {
                                                                    Messages.Message("DM.Message.SpousesBusy".Translate(relation.otherPawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
                                                                }
                                                                else
                                                                {
                                                                    ignoreList.Add(relation.otherPawn);
                                                                    if (onlyTwo)
                                                                    {
                                                                        StartParty(item.Map, "DM.Letter.MarriageAnniversaryParty".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), new List<Pawn> { pawn, relation.otherPawn }, DMMarriageAnniversaries == Duration.AllDay, pawn);
                                                                    }
                                                                    else
                                                                    {
                                                                        StartParty(item.Map, "DM.Letter.MarriageAnniversaryParty".Translate(pawn.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), null, DMMarriageAnniversaries == Duration.AllDay, pawn);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (_matteredDays != null)
                            {
                                foreach (MatteredDay md in _matteredDays)
                                {
                                    if (md != null && md.DayOfQuadrum == (localDay + 1) && md.Quadrum == localMonth)
                                    {
                                        if (localHour == 0)
                                        {
                                            Messages.Message("DM.Message.TodayCustomDay".Translate(md.Name), MessageTypeDefOf.PositiveEvent);
                                        }
                                        else if (localHour == md.Duration.Start())
                                        {
                                            StartParty(item.Map, "DM.Letter.CustomDayParty".Translate(md.Name), null, md.Duration == Duration.AllDay);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void StartParty(Map map, string reason, List<Pawn> invited = null, bool wholeDay = false, Pawn starter = null)
        {
            TryStartParty(map, reason, wholeDay, starter, invited);
        }

        private bool TryStartParty(Map map, string reason, bool wholeDay, Pawn starter, List<Pawn> invitedPawns = null)
        {
            if (map == null)
                return false;
            
            if (starter == null)
            {
                //starter = GatheringsUtility.FindRandomGatheringOrganizer(Faction.OfPlayer, map, GatheringDefOf.Party);
                starter = PartyUtil.FindRandomPartyOrganizer(Faction.OfPlayer, map);
                if (starter == null)
                {
                    Messages.Message("DM.Error.NoStarter".Translate(), MessageTypeDefOf.NegativeEvent);
                    return false;
                }
            }

            IntVec3 spot;
            //if (!PartyUtil.TryFindPartySpot(starter, out spot))
            //if (!RCellFinder.TryFindGatheringSpot(starter, GatheringDefOf.Party, out spot))
            if (!RCellFinder.TryFindGatheringSpot(starter, GatheringDefOf.Party, true, out spot))
            {
                List<IntVec3> spots = new List<IntVec3>();
                foreach (Building item in starter.Map.listerBuildings.allBuildingsColonist)
                {
                    if (item.def == ThingDefOf.PartySpot)
                    {
                        spots.Add(item.Position);
                    }
                }
                if (spots.Count > 0)
                {
                    spot = spots.RandomElement();
                }
                else
                {
                    spot = starter.Position;
                }

                if (!spot.IsValid)
                {
                    Messages.Message("DM.Error.NoSpot".Translate(), MessageTypeDefOf.NegativeEvent);
                    return false;
                }
            }
            /*if (!GatheringsUtility.PawnCanStartOrContinueGathering(starter))
            {
                Messages.Message("DM.Error.NoStarter".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }*/

            LordJob partyJob = wholeDay ? new LongJoinableParty(spot, starter, GatheringDefOf.Party, invitedPawns) : new JoinableParty(spot, starter, GatheringDefOf.Party, invitedPawns);
            LordMaker.MakeNewLord(starter.Faction, partyJob, map);
            Find.LetterStack.ReceiveLetter("DM.Letter.PartyTitle".Translate(), "DM.Letter.Party".Translate(reason), LetterDefOf.PositiveEvent, new TargetInfo(spot, map));
            return true;
        }
    }

    /*public class DaysMatter
    {
        [HarmonyPatch(typeof(GenDate), "Quadrum")]
        public static class GenDateQuadrumPatch
        {
            [HarmonyPostfix]
            public static void FixQuadrum(ref Quadrum __result, long absTicks, float longitude)
            {
                var offset = (absTicks / 2500f / 24f / 15f) % 4;
                if (offset < 0)
                    offset = (offset + 4) % 4;
                __result = (Quadrum)Mathf.FloorToInt(offset);
            }
        }

        [HarmonyPatch(typeof(GenDate), "DayOfQuadrum")]
        public static class GenDateDayOfQuadrumPatch
        {
            [HarmonyPostfix]
            public static void FixDayOfQuadrum(ref int __result, long absTicks, float longitude)
            {
                var offset = (absTicks / 2500f / 24f) % 15;
                if (offset < 0)
                    offset = (offset + 15) % 15;
                __result = Mathf.FloorToInt(offset);
            }
        }

        [HarmonyPatch(typeof(Pawn_AgeTracker), "ExposeData")]
        public static class PawnAgeTrackerExposeDataPatch
        {
            private static readonly FieldInfo AGE_BIOLOGICAL_TICKS_INT_FIELD = AccessTools.Field(typeof(Pawn_AgeTracker), "ageBiologicalTicksInt");
            private static readonly FieldInfo BIRTH_ABS_TICKS_INT_FIELD = AccessTools.Field(typeof(Pawn_AgeTracker), "birthAbsTicksInt");

            [HarmonyPrefix]
            public static void PreFix(Pawn_AgeTracker __instance)
            {
                long birthAbsTicksInt = (long)BIRTH_ABS_TICKS_INT_FIELD.GetValue(__instance);
                long ageBiologicalTicksInt = (long)AGE_BIOLOGICAL_TICKS_INT_FIELD.GetValue(__instance);

                if (birthAbsTicksInt < 0 && Scribe.mode == LoadSaveMode.Saving)
                {
                    AGE_BIOLOGICAL_TICKS_INT_FIELD.SetValue(__instance, ageBiologicalTicksInt - Find.TickManager.gameStartAbsTick);
                }
            }

            [HarmonyPostfix]
            public static void PostFix(Pawn_AgeTracker __instance)
            {
                long birthAbsTicksInt = (long)BIRTH_ABS_TICKS_INT_FIELD.GetValue(__instance);
                long ageBiologicalTicksInt = (long)AGE_BIOLOGICAL_TICKS_INT_FIELD.GetValue(__instance);

                if (birthAbsTicksInt < 0 && (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.PostLoadInit))
                {
                    AGE_BIOLOGICAL_TICKS_INT_FIELD.SetValue(__instance, ageBiologicalTicksInt + Find.TickManager.gameStartAbsTick);
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_AgeTracker), "BirthDayOfSeasonZeroBased", MethodType.Getter)]
        //[HarmonyPatch("BirthDayOfSeasonZeroBased", PropertyMethod.Getter)]
        //[HarmonyPatch(typeof(Pawn_AgeTracker), "BirthDayOfSeasonZeroBased")]
        public static class PawnAgeTrackerBirthDayOfSeasonZeroBasedPatch
        {
            private static readonly FieldInfo BIRTH_ABS_TICKS_INT_FIELD = AccessTools.Field(typeof(Pawn_AgeTracker), "birthAbsTicksInt");

            [HarmonyPostfix]
            public static void Fix(Pawn_AgeTracker __instance, ref int __result)
            {
                long birthAbsTicksInt = (long)BIRTH_ABS_TICKS_INT_FIELD.GetValue(__instance);
                __result = GenDate.DayOfQuadrum(birthAbsTicksInt, 0f);
            }
        }
    }*/
}