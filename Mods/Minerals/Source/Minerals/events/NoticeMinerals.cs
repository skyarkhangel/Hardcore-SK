
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace Minerals
{
    public class IncidentWorker_NoticeMineral : IncidentWorker
    {
    
        ThingDef_StaticMineral typeToSpawn;
        Pawn finder;

            
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;

            int maxMineSkill = 0;
            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (pawn.IsColonist && pawn.skills.GetSkill(SkillDefOf.Mining).Level > maxMineSkill)
                {
                    maxMineSkill = pawn.skills.GetSkill(SkillDefOf.Mining).Level;
                    finder = pawn;
                }
            }
            if (maxMineSkill < 5)
            {
                return false;
            }
            if (maxMineSkill < 10 && Rand.Bool) {
                return false;
            }

            IntVec3 outPos;
            return this.TryFindRootCell(map, out outPos);
        }

        protected bool CanSpawnAt(IntVec3 c, Map map)
        {
            //Log.Message("NoticeMineral: CanSpawnAt: c: " + c);
            //Log.Message("NoticeMineral: CanSpawnAt: map: " + map);
            //Log.Message("NoticeMineral: CanSpawnAt: c.Fogged(map): " + c.Fogged(map));
            //Log.Message("NoticeMineral: CanSpawnAt:  c.GetSnowDepth(map): " + c.GetSnowDepth(map));
            if (c.Fogged(map) || c.GetSnowDepth(map) > 0)
            {
                return false;
            }

            bool noticed = false;
            foreach (Pawn pawn in map.mapPawns.AllPawns.InRandomOrder())
            {
                if (pawn.IsColonist)
                {
                    int mineSkill = pawn.skills.GetSkill(SkillDefOf.Mining).Level;
                    if (mineSkill < 5 || (mineSkill < 10 && Rand.Bool) || pawn.Position.DistanceTo(c) > mineSkill)
                    {
                        continue;
                    }
                    noticed = true;
                    finder = pawn;
                }
            }
            if (noticed == false)
            {
                return false;
            }
            // Log.Message("NoticeMineral: CanSpawnAt: pawn: " + finder);

            foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs.InRandomOrder())
            {
                if (!mineralType.tags.NullOrEmpty() && mineralType.tags.Contains("NoticeMineral_Event") && mineralType.CanSpawnAt(map, c))
                {
                    typeToSpawn = mineralType;
                    return true;
                }
            }


            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 rootPos;
            if (!this.TryFindRootCell(map, out rootPos))
            {
                return false;
            }
            Thing thing = typeToSpawn.TrySpawnCluster(map, rootPos, Rand.Range(typeToSpawn.initialSizeMin, typeToSpawn.initialSizeMax), Rand.Range(typeToSpawn.minClusterSize, typeToSpawn.maxClusterSize));
            if (thing == null)
            {
                return false;
            }
            string text = string.Format(this.def.letterText, finder.Name, thing.def.label).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, LetterDefOf.PositiveEvent, new TargetInfo(rootPos, map, false), null, null);
            return true;
        }

        private bool TryFindRootCell(Map map, out IntVec3 cell)
        {
            return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => this.CanSpawnAt(x, map) && x.GetRoom(map).CellCount >= 64, map, out cell);
        }
    }

}
