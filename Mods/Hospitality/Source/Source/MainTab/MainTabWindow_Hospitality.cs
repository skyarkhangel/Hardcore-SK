using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    public class MainTabWindow_Hospitality : MainTabWindow_PawnTable
    {
        private static PawnTableDef pawnTableDef;

        public override PawnTableDef PawnTableDef => pawnTableDef ??= DefDatabase<PawnTableDef>.GetNamed("Guests");

        public override IEnumerable<Pawn> Pawns => Find.CurrentMap.GetMapComponent().PresentGuests;

        public override void PostOpen()
        {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }

        public override void DoWindowContents(Rect rect)
        {
            if (Multiplayer.IsRunning)
                Multiplayer.WatchBegin();

            base.DoWindowContents(rect);

            if (Multiplayer.IsRunning)
                Multiplayer.WatchEnd();
        }
    }
}
