using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace LetsGoExplore
{
    public class SymbolResolver_PrisonCampLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            int zOffset = 7;
            int xOffset = 6;
            rp.faction = rp.faction ?? BaseGen.globalSettings.map.ParentFaction; //Find.FactionManager.RandomNonHostileFaction();
            Faction enemyFaction = FindEnemyFaction(rp.faction);

            CellRect settlementRect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, rp.rect.Height - zOffset);
            CellRect prisonerRect = new CellRect(rp.rect.minX + xOffset, rp.rect.minZ + rp.rect.Height - zOffset +1, rp.rect.Width - xOffset * 2, zOffset);

            ResolveParams resolveParmsPrisoners = rp;
            resolveParmsPrisoners.rect = prisonerRect.ContractedBy(1);
            //resolveParmsPrisoners.faction = rp.faction ?? Find.FactionManager.RandomNonHostileFaction();
            BaseGen.symbolStack.Push("relaxedPrisonersLGE", resolveParmsPrisoners);

            ResolveParams resolveParmsInterior = rp;
            resolveParmsInterior.faction = enemyFaction;
            resolveParmsInterior.rect = prisonerRect.ContractedBy(1);
            //Multiple times for multiple beds and some additional food
            BaseGen.symbolStack.Push("interior_prisonCell", resolveParmsInterior);
            BaseGen.symbolStack.Push("interior_prisonCell", resolveParmsInterior);

            MapGenerator.rootsToUnfog.Add(resolveParmsInterior.rect.CenterCell);

            ResolveParams resolveParmsDoors = rp;
            resolveParmsDoors.rect = prisonerRect;
            resolveParmsDoors.faction = enemyFaction;
            BaseGen.symbolStack.Push("doors", resolveParmsDoors);

            ResolveParams resolveParmsRoom = rp;
            resolveParmsRoom.rect = prisonerRect;
            resolveParmsRoom.faction = enemyFaction;
            BaseGen.symbolStack.Push("emptyRoom", resolveParmsRoom);

            /*ResolveParams resolveParmsPrisonRoom = rp;
            resolveParmsPrisonRoom.rect = prisonerRect;
            BaseGen.symbolStack.Push("prisonRoomLGE", resolveParmsPrisonRoom);
            */

            ResolveParams resolveParmsSettlement = rp;
            resolveParmsSettlement.rect = settlementRect;
            resolveParmsSettlement.faction = enemyFaction;
            BaseGen.symbolStack.Push("settlement", resolveParmsSettlement);
        }

        public Faction FindEnemyFaction(Faction prisonerFacction)
        {
            Faction enemyFaction;
            (from x in Find.FactionManager.AllFactionsVisible
             where !x.IsPlayer && (!x.def.hidden) && (!x.defeated) && (x.def.humanlikeFaction) && x.HostileTo(Faction.OfPlayer) && x.HostileTo(prisonerFacction)
             select x).TryRandomElement(out enemyFaction);
            return enemyFaction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
        }
    }

    public class SymbolResolver_PrisonRoomLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParmsPrisoners = rp;
            resolveParmsPrisoners.rect.ContractedBy(1);
            //resolveParmsPrisoners.faction = rp.faction ?? Find.FactionManager.RandomNonHostileFaction();
            BaseGen.symbolStack.Push("relaxedPrisonersLGE", resolveParmsPrisoners);

            ResolveParams resolveParmsInterior = rp;
            resolveParmsInterior.rect.ContractedBy(1);
            //Multiple times for multiple beds and some additional food
            BaseGen.symbolStack.Push("interior_prisonCell", resolveParmsInterior);
            BaseGen.symbolStack.Push("interior_prisonCell", resolveParmsInterior);

            MapGenerator.rootsToUnfog.Add(resolveParmsInterior.rect.CenterCell);

            ResolveParams resolveParmsDoors = rp;
            BaseGen.symbolStack.Push("doors", resolveParmsDoors);

            ResolveParams resolveParmsRoom = rp;
            BaseGen.symbolStack.Push("emptyRoom", resolveParmsRoom);

        }
    }
}
