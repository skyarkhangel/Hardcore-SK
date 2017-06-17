using System;
using System.Collections.Generic;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    public class ClassroomWorker : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0;
            List<Thing> allContainedThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < allContainedThings.Count; i++)
            {
                Thing thing = allContainedThings[i];
                if (thing is Building_TeachingSpot)
                {
                    num += 2;
                }
                if (thing is Building_Chalkboard)
                {
                    num++;
                }
                if (thing.def.defName == "GlobeCL")
                {
                    num++;
                }
            }
            return 30f * (float)num;
        }
    }
}
