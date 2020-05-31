using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    public class FactionDialogMaker_Patch
    {
        [HarmonyPatch(typeof(FactionDialogMaker), "FactionDialogFor")]
        public class FactionDialogFor
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn negotiator, Faction faction, ref DiaNode __result)
            {
                if (negotiator.Map?.IsPlayerHome == true)
                {
                    __result.options.Insert(0, InviteGuestsOption(negotiator.Map, faction, negotiator));
                }
            }

            private static DiaOption InviteGuestsOption(Map map, Faction faction, Pawn negotiator)
            {
                string text = "InviteGuests".Translate();
                if (faction.PlayerRelationKind == FactionRelationKind.Hostile)
                {
                    DiaOption optionNoHostiles = new DiaOption(text);
                    optionNoHostiles.Disable("GuestsCantBeHostile".Translate());
                    return optionNoHostiles;
                }
                var nextVisit = map.GetMapComponent().GetNextVisit(faction);
                float travelDays = GenericUtility.GetTravelDays(faction, map);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (travelDays == GenericUtility.NoBasesLeft)
                {
                    DiaOption optionNoBases = new DiaOption(text);
                    optionNoBases.Disable("GuestsDontWantToCome".Translate());
                    return optionNoBases;
                }
                if (nextVisit != null)
                {
                    DiaOption optionAlreadyQueued = new DiaOption(text);
                    var ticksToVisit = nextVisit.FireTick-GenTicks.TicksGame;
                    var daysToVisit = ticksToVisit.TicksToDays();
                    if (daysToVisit < travelDays+7)
                        optionAlreadyQueued.Disable("GuestsAlreadyComing".Translate(ticksToVisit.ToStringTicksToPeriodVague()));
                    else // if (daysToVisit > GenericUtility.GetTravelDays(faction, map) + 20)
                        optionAlreadyQueued.Disable("GuestsDontWantToCome".Translate());
                    return optionAlreadyQueued;
                }
                if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
                {
                    DiaOption optionBadTemperature = new DiaOption(text);
                    optionBadTemperature.Disable("BadTemperature".Translate());
                    return optionBadTemperature;
                }
                DiaOption optionInvite = new DiaOption(text);
                int travelTicks = (int) (travelDays*GenDate.TicksPerDay);
                DiaNode nodeOnTheirWay = new DiaNode("GuestsOnTheirWay".Translate(travelTicks.ToStringTicksToPeriodVague()));
                DiaOption optionOK = Traverse.Create(typeof(FactionDialogMaker)).Method("OKToRoot", faction, negotiator).GetValue<DiaOption>();
                nodeOnTheirWay.options.Add(optionOK);
                optionInvite.link = nodeOnTheirWay;
                optionInvite.action = delegate
                {
                    GenericUtility.TryCreateVisit(map, 0, faction);
                };
                return optionInvite;
            }
        }
    }
}