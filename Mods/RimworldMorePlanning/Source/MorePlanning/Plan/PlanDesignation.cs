using Verse;

namespace MorePlanning // Need to be outside of Plan for compability with older saves
{
    public class PlanDesignation : Designation
    {
        public int Color;

        public PlanDesignation(LocalTargetInfo target, DesignationDef def, int color) : base(target, def)
        {
            Color = color;
        }

        public PlanDesignation()
        {
        }
        
    }
}
