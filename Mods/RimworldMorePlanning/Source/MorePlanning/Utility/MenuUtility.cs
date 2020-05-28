using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace MorePlanning.Utility
{
    class MenuUtility
    {

        private static FieldInfo _resolvedDesignatorsInfo;

        public static void InitReflection()
        {
            FieldInfo field = typeof(DesignationCategoryDef).GetField("resolvedDesignators", BindingFlags.Instance | BindingFlags.NonPublic);
            _resolvedDesignatorsInfo = field;

            if (_resolvedDesignatorsInfo == null)
            {
                MorePlanningMod.LogError("Reflection failed (MenuUtility::InitReflection, DesignationCategoryDef.resolvedDesignators)");
            }
        }

        public static List<Designator> GetPlanningDesignators()
        {
            if (_resolvedDesignatorsInfo == null)
            {
                InitReflection();
            }

            var planningCategory = DefDatabase<DesignationCategoryDef>.GetNamed("Planning");

            if (planningCategory == null)
            {
                MorePlanningMod.LogError("Menu planning not found");
                return null;
            }

            return (List<Designator>)_resolvedDesignatorsInfo?.GetValue(planningCategory);
        }

        public static T GetPlanningDesignator<T>() where T: class
        {
            var designators = GetPlanningDesignators();
            foreach (var designator in designators)
            {
                var targetDesignator = designator as T;
                if (targetDesignator != null)
                {
                    return targetDesignator;
                }
            }

            return null;
        }

    }
}
