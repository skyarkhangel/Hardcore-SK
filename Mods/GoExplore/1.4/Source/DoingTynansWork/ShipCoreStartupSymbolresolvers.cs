﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.BaseGen;
using System.Reflection;

namespace LetsGoExplore
{
    public class SymbolResolver_ShipCoreStartupLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            int offsetX = 5;
            Faction enemyFaction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Industrial);
            rp.faction = enemyFaction ?? Faction.OfAncientsHostile;
            rp.pawnGroupKindDef = PawnGroupKindDefOf.Settlement;
            rp.edgeDefenseWidth = 4;

            //split up the area into 4 parts with one settlement each
            CellRect rectSW = new CellRect(rp.rect.minX - offsetX, rp.rect.minZ, rp.rect.Width / 2, rp.rect.Height / 2).ContractedBy(1);
            CellRect rectSE = new CellRect(rp.rect.minX + rp.rect.Width / 2 + offsetX +1, rp.rect.minZ, rp.rect.Width / 2, rp.rect.Height / 2).ContractedBy(1);
            CellRect rectNW = new CellRect(rp.rect.minX - offsetX + 3, rp.rect.minZ + rp.rect.Height / 2, rp.rect.Width / 2, rp.rect.Height / 2).ContractedBy(1);
            CellRect rectNE = new CellRect(rp.rect.minX + rp.rect.Width / 2 + offsetX - 3, rp.rect.minZ + rp.rect.Height / 2, rp.rect.Width / 2, rp.rect.Height / 2).ContractedBy(1);

            //change ship reactor status to powering up
            ResolveParams resolveParmsReactorStatus = rp;
            BaseGen.symbolStack.Push("activateShipReactorLGE", resolveParmsReactorStatus);

            //Charge up batteries
            ResolveParams resolveParmsBatterieCharge = rp;
            //run it multiple times so they are fully charged
            BaseGen.symbolStack.Push("chargeBatteries", resolveParmsBatterieCharge);
            BaseGen.symbolStack.Push("chargeBatteries", resolveParmsBatterieCharge);
            BaseGen.symbolStack.Push("chargeBatteries", resolveParmsBatterieCharge);
            BaseGen.symbolStack.Push("chargeBatteries", resolveParmsBatterieCharge);
            BaseGen.symbolStack.Push("chargeBatteries", resolveParmsBatterieCharge);

            //scatter some mortar ammo
            ResolveParams resolveParmsShells = rp;
            resolveParmsShells.stockpileConcreteContents = IncidentUtilityLGE.GenerateShellStocks(Rand.RangeInclusive(4, 6));
            BaseGen.symbolStack.Push("spawnStockpileLGE", resolveParmsShells);

            //Spawn additional mortar down in the middle of the ship gap
            ResolveParams resolveParmsMortar = rp;
            resolveParmsMortar.rect = new CellRect(rectSW.maxX, rectSW.minZ + 3, rectSE.minX - rectSW.maxX, 5);
            BaseGen.symbolStack.Push("mannedMortar", resolveParmsMortar);
            BaseGen.symbolStack.Push("mannedMortar", resolveParmsMortar);

            //Spawn the pregen ship in the middle
                ResolveParams resolveParmsShip = rp;
                BaseGen.symbolStack.Push("shipGenerationLGE", resolveParmsShip);

            //create the 4 settlements
            ResolveParams resolveParmsSettlementSW = rp;
            resolveParmsSettlementSW.rect = rectSW;
            resolveParmsSettlementSW.edgeDefenseMortarsCount = 1;
            BaseGen.symbolStack.Push("settlement", resolveParmsSettlementSW);

            ResolveParams resolveParmsSettlementSE = rp;
            resolveParmsSettlementSE.rect = rectSE;
            resolveParmsSettlementSE.edgeDefenseMortarsCount = 1;
            BaseGen.symbolStack.Push("settlement", resolveParmsSettlementSE);

            ResolveParams resolveParmsSettlementNW = rp;
            resolveParmsSettlementNW.rect = rectNW;
            resolveParmsSettlementNW.edgeDefenseMortarsCount = Rand.RangeInclusive(1, 2);
            BaseGen.symbolStack.Push("settlement", resolveParmsSettlementNW);

            ResolveParams resolveParmsSettlementNE = rp;
            resolveParmsSettlementNE.rect = rectNE;
            resolveParmsSettlementNE.edgeDefenseMortarsCount = Rand.RangeInclusive(1, 2);
            BaseGen.symbolStack.Push("settlement", resolveParmsSettlementNE);

            //clear the area before spawning to get rid of roofs and mountains
            ResolveParams resolveParmsClear = rp;
            resolveParmsClear.clearRoof = true;
            resolveParmsClear.rect = rectNE;
            BaseGen.symbolStack.Push("clear", resolveParmsClear);
            resolveParmsClear.rect = rectNW;
            BaseGen.symbolStack.Push("clear", resolveParmsClear);
            resolveParmsClear.rect = rectSE;
            BaseGen.symbolStack.Push("clear", resolveParmsClear);
            resolveParmsClear.rect = rectSW;
            BaseGen.symbolStack.Push("clear", resolveParmsClear);

        }
    }

    public class SymbolResolver_ActivateShipReactorLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            List<Thing> list = map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.Ship_Reactor));
            for (int i = 0; i < list.Count; i++)
            {
                CompHibernatable compHibernatable = list[i].TryGetComp<CompHibernatable>();
                if (compHibernatable != null)
                {
                    compHibernatable.State = HibernatableStateDefOf.Running;
                }
            }
        }
    }

    public class SymbolResolver_ShipGenerationLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            //Compatibility with Save our Ship 2
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Save Our Ship 2"))
            {
                Type calledType = Type.GetType("RimWorld.ShipCombatManager,ShipsHaveInsides");
                if (calledType == null)
                {
                    Log.Warning("SOS2 type was unable to load.");
                }
                else
                {
                    MethodInfo method = calledType.GetMethod("GenerateShip", BindingFlags.Public | BindingFlags.Static);
                    Building core = new Building();
                    try
                    {
                        Type defDatabase = typeof(DefDatabase<>).MakeGenericType(new Type[] { Type.GetType("RimWorld.EnemyShipDef,ShipsHaveInsides") });

                        if (defDatabase == null)
                        {
                            Log.Warning("SOS2 Def-type was unable to load.");
                        }
                        else
                        {
                            MethodInfo defMethod = defDatabase.GetMethod("GetNamed", BindingFlags.Public | BindingFlags.Static);
                            if (defMethod == null)
                            {
                                Log.Warning("SOS2 Def-type-method was unable to load.");
                            }
                            else
                            {
                                Object defToPass = defMethod.Invoke(null, new object[] { "HeavyFighterMKI", true });
                                method.Invoke(null, new object[] { defToPass, BaseGen.globalSettings.map, null, rp.faction, null, core, true, false });
                            }
                        }
                    }
                    catch
                    {
                        Log.Warning("Ship could not be called. Enjoy the tough fight anyway.");
                    }
                }
            }
            else
            {
                ResolveParams resolveParmsShip = rp;
                BaseGen.symbolStack.Push("ship_pregen", resolveParmsShip);
            }

        }
    }
}
