using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Hospitality.MainTab
{
    public class MainTabWindow_Hospitality : MainTabWindow_PawnTable
    {
        private static PawnTableDef pawnTableDef;

        protected override PawnTableDef PawnTableDef => pawnTableDef ??= DefDatabase<PawnTableDef>.GetNamed("Guests");

        protected override IEnumerable<Pawn> Pawns => Find.CurrentMap.GetMapComponent().PresentGuests;

        public override void PostOpen()
        {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }
    }
}
