using MorePlanning.Plan;
using Verse;

namespace MorePlanning.Utility
{
    public static class MapUtility
    {
        public static void RemoveAllPlanDesignationAt(IntVec3 c, Map map)
        {
            foreach (DesignationDef def in MorePlanningMod.PlanDesDefs)
            {
                Designation desAt = map.designationManager.DesignationAt(c, def);
                desAt?.Delete();
            }
        }

        /// <summary>
        /// Returns a plan designation if it exists at the position.
        /// </summary>
        public static Designation GetPlanDesignationAt(IntVec3 c, Map map)
        {
            foreach (DesignationDef def in MorePlanningMod.PlanDesDefs)
            {
                var des = map.designationManager.DesignationAt(c, def);
                if (des != null)
                {
                    return des;
                }
            }

            return null;
        }

        public static bool HasAnyPlanDesignationAt(IntVec3 c, Map map)
        {
            foreach (DesignationDef def in MorePlanningMod.PlanDesDefs)
            {
                if (map.designationManager.DesignationAt(c, def) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasPlanDesignationAt(IntVec3 c, Map map, int color)
        {
            var designation = GetPlanDesignationAt(c, map) as PlanDesignation;
            
            if ((designation != null) && (designation.Color == color))
            {
                return true;
            }

            return false;
        }

    }
}
