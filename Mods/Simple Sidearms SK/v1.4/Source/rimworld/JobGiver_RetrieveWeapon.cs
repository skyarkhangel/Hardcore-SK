using PeteTimesSix.SimpleSidearms;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

using static PeteTimesSix.SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.rimworld
{
    public class JobGiver_RetrieveWeapon : ThinkNode_JobGiver
    { 

        public static Job TryGiveJobStatic(Pawn pawn, bool inCombat)
        {
            if (!Settings.ReEquipOutOfCombat)
                return null;
            if (!Settings.ReEquipInCombat && inCombat)
                return null;
            if (RestraintsUtility.InRestraints(pawn))
                return null;
            else
            {
                if (!pawn.IsValidSidearmsCarrier())
                    return null;

                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory == null)
                    return null;

                //if (pawnMemory.IsUsingAutotool(true))
                //    return null;

                //WeaponAssingment.equipBestWeaponFromInventoryByPreference(pawn, Globals.DroppingModeEnum.Calm);

                if (pawnMemory.RememberedWeapons is null)
                    Log.Warning("pawnMemory of "+pawn.Label+" is missing remembered weapons");

                Dictionary<ThingDefStuffDefPair, int> dupeCounters = new Dictionary<ThingDefStuffDefPair, int>();

                foreach (ThingDefStuffDefPair weaponMemory in pawnMemory.RememberedWeapons)
                {
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    if (!pawn.hasWeaponType(weaponMemory, dupeCounters[weaponMemory]))
                    {
                        float maxDist = 1000f;
                        if (pawn.Faction != Faction.OfPlayer)
                            maxDist = 30f;
                        if (inCombat)
                            maxDist = 12f;

                        bool bladelinkable = weaponMemory.thing.HasComp(typeof(CompBladelinkWeapon));
                        bool biocodeable = weaponMemory.thing.HasComp(typeof(CompBiocodable));

                        IEnumerable<ThingWithComps> matchingWeapons = pawn.Map.listerThings.ThingsOfDef(weaponMemory.thing).OfType<ThingWithComps>().Where(t => t.Stuff == weaponMemory.stuff);
                        if (bladelinkable)
                        {
                            matchingWeapons = matchingWeapons.Where(t =>
                            {
                                CompBladelinkWeapon bladelink = t.GetComp<CompBladelinkWeapon>();
                                return (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn == pawn);
                            });
                        }
                        if (biocodeable)
                        {
                            matchingWeapons = matchingWeapons.Where(t =>
                            {
                                CompBiocodable biocode = t.GetComp<CompBiocodable>();
                                if (biocode == null)
                                    return true; //not sure how this could ever happen...
                                if (biocode.Biocoded && biocode.CodedPawn != pawn)
                                    return false;
                                return true;
                            });
                        }

                        Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, matchingWeapons, PathEndMode.OnCell, TraverseParms.For(pawn), maxDist,
                            (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t),
                            (Thing t) => Settings.ReEquipBest ? t.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, false) : 0);
                                                            //this works properly because better ranged weapons also happen to be better at pistolwhipping
                                                            //okay past me, WHAT? Why?

                        if (thing == null)
                            continue;

                        if (!inCombat)
                            return JobMaker.MakeJob(SidearmsDefOf.ReequipSecondary, thing);
                        else
                            return JobMaker.MakeJob(SidearmsDefOf.ReequipSecondaryCombat, thing, pawn.Position);

                    }

                    dupeCounters[weaponMemory]++;
                }

                return null;
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveJobStatic(pawn, false);
        }
    }
}
