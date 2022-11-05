using RimWorld;
using Verse;

namespace Revolus.AnimalsAreFun {
    [DefOf]
    public class AnimalsAreFunDefOf {
        // go walkies
        public static JoyGiverDef Revolus_AnimalsAreFun_Walkies_JoyGiver;
        public static JobDef Revolus_AnimalsAreFun_Walkies_Job;
        
        // play fetch
        public static JoyGiverDef Revolus_AnimalsAreFun_Fetch_JoyGiver;
        public static JobDef Revolus_AnimalsAreFun_Fetch_Job;
        public static JobDef Revolus_AnimalsAreFun_Walkies_Job_Animal;

        static AnimalsAreFunDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(AnimalsAreFunDefOf));
        }
    }
}
