using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_InventoryDropAll : PawnColumnWorker_Icon
    {
        protected override Texture2D GetIconFor(Pawn pawn)
        {
            var playerControlled = pawn.Faction != null && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || (pawn.AnimalOrWildMan() && pawn.Faction.IsPlayer));

            return playerControlled && pawn.inventory.FirstUnloadableThing != default ? StaticConstructorOnGameStart.Drop : null;
        }

        protected override void ClickedIcon(Pawn pawn)
        {
            var playerControlled = pawn.Faction != null && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || (pawn.AnimalOrWildMan() && pawn.Faction.IsPlayer));

            if (playerControlled && pawn.inventory.FirstUnloadableThing != default)
            {
                pawn.inventory.DropAllNearPawn(pawn.PositionHeld);
            }
        }
    }
}
