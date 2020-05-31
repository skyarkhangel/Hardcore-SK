using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Hospitality
{
    public class TransitionAction_SendAway : TransitionAction
    {
        public override void DoAction(Transition trans)
        {
            var lord = trans.target.lord;
            var map = trans.Map;
            var faction = lord?.faction;
            if (lord == null || map == null || faction == null)
            {
                Log.Error($"Couldn't send guests away. Something was null: lord? {lord == null} map? {map == null} faction? {faction == null}");
                return;
            }

            StopPawns(lord.ownedPawns);
            LordToil_VisitPoint.DisplayLeaveMessage(Mathf.InverseLerp(-100, 100, faction.PlayerGoodwill), faction, lord.ownedPawns?.Count ?? 0, map, true);
        }

        private static void StopPawns(IEnumerable<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                pawn.pather.StopDead();
                pawn.ClearMind();
            }
        }
    }
}
