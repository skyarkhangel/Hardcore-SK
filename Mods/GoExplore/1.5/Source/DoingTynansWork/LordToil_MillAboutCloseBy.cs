using RimWorld;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;

namespace LetsGoExplore
{
    public class LordToil_MillAboutCloseBy : LordToil
    {
        private readonly IntVec3 locus;
        private bool done;

        public LordToil_MillAboutCloseBy(IntVec3 chillpoint)
        {
            this.locus = chillpoint;
        }

        //setting it earlier is no bueno: upon map gen, the avoidgrid gets cleared (!)
        public override void LordToilTick()
        {
            if (done)
                return;

            Lord enemyLord = Map.lordManager.lords.Find(x => x.LordJob is LordJob_DefendBase);
            enemyLord.Graph.lordToils.ForEach(x => x.useAvoidGrid = true);

            foreach (IntVec3 intVec in lord.ownedPawns[0].GetRoom().Cells.Concat(lord.ownedPawns[0].GetRoom().BorderCells)) //gotta get the door as well.
            {
                if (intVec.InBounds(enemyLord.Map) && intVec.Walkable(enemyLord.Map))
                {
                    IncrementAvoidGrid(intVec, 255, enemyLord.Map.avoidGrid.Grid);
                }
            }

            //Log.Message("grid " + GridToString(enemyLord.Map.avoidGrid.Grid, enemyLord.Map));

            done = true;
        }

        public override void UpdateAllDuties()
        {
            foreach (var item in lord.ownedPawns)
            {
                item.mindState.duty = new PawnDuty(DefsOfLGE.ChillAsPrisonerLGE, locus, 8f);
            }
        }

        private void IncrementAvoidGrid(IntVec3 c, int num, ByteGrid grid)
        {
            byte b = grid[c];
            b = (byte)Mathf.Min(255, b + num);
            grid[c] = b;
        }

        //used as helper to verify, debugsettings => draw avoid grid is better once the grid sticks.
        private string GridToString(ByteGrid grid, Map map)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < map.Size.x; i++)
            {
                stringBuilder.AppendLine();
                for (int j = 0; j < map.Size.z; j++)
                {
                    stringBuilder.Append(grid[CellIndicesUtility.CellToIndex(i, j, map.Size.x)]);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
