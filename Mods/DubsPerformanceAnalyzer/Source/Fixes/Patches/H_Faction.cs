using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Analyzer.Profiling;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Analyzer.Fixes
{
    class H_FactionManager : Fix
    {
        public static bool Active = false;
        public override string Name => "fix.faction";

        public override void OnGameLoaded(Game g, Harmony h)
        {
            var factionManger = g.World.factionManager;
            foreach (var fac in factionManger.allFactions.Where(f => f.def == null))
            {
                fac.def = FactionDef.Named("OutlanderCivil");
            }

            foreach (var wo in g.World.worldObjects.worldObjects.Where(w => w.factionInt.def == null))
            {
                var faction = factionManger.allFactions.Where(f => !f.IsPlayer && (!f.hidden ?? true)).RandomElement();
                
                ThreadSafeLogger.Warning($"[Analyzer] Changed the world object {wo.Label}'s faction from {wo.factionInt.name}(Removed) to {faction.name}");
                wo.factionInt = faction;
            }

            foreach (var wp in g.World.worldPawns.AllPawnsAliveOrDead.Where(p => p.factionInt.def == null))
            {
                var faction = factionManger.allFactions.Where(f => !f.IsPlayer && (!f.hidden ?? true)).RandomElement();
                
                ThreadSafeLogger.Warning($"[Analyzer] Changed the pawn {wp.Label}'s faction from {wp.factionInt.name}(Removed) to {faction.name}");
                wp.factionInt = faction;
            }

            Active = false;
        }
    }
}