using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospitality
{
    /// <summary>
    /// This class is for debugging purposes
    /// </summary>
    public class IncidentWorker_VisitorGroupSelectFaction : IncidentWorker_VisitorGroup
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var list = new List<DebugMenuOption>();
            foreach (var faction in Find.FactionManager.AllFactions)
            {
                if (faction.IsPlayer) continue;
                if (faction.HostileTo(Faction.OfPlayer)) continue;
                if (faction.def.pawnGroupMakers == null) continue;
                if (!faction.def.pawnGroupMakers.Any(m => m?.kindDef == PawnGroupKindDefOf.Peaceful)) continue;
                list.Add(new DebugMenuOption($"{faction.Name} ({faction.PlayerGoodwill})", DebugMenuOptionMode.Action, delegate {
                    parms.faction = faction;
                    base.TryExecuteWorker(parms);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            return true;
        }
    }
}
