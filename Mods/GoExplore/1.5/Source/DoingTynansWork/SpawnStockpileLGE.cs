using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace LetsGoExplore
{
    public class SymbolResolver_SpawnStockpileLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            if (rp.stockpileConcreteContents != null && (rp.stockpileConcreteContents.Count > 0))
            {
                for (int i = 0; i < rp.stockpileConcreteContents.Count; i++)
                {
                    ResolveParams resolveParams = rp;
                    resolveParams.singleThingToSpawn = rp.stockpileConcreteContents[i];
                    BaseGen.symbolStack.Push("thing", resolveParams);
                }
            }
        }
    }
}
