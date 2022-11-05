namespace Numbers
{
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class PawnColumnOptionDef : Def
    {
        public List<PawnColumnDef> options;
    }

    [DefOf]
    public class PawnColumnOptionDefOf
    {
        public static PawnColumnOptionDef EquipmentBearers;
        public static PawnColumnOptionDef LivingThings;
        public static PawnColumnOptionDef Prisoners;
        public static PawnColumnOptionDef Animals;
        public static PawnColumnOptionDef MainTable;
        public static PawnColumnOptionDef WildAnimals;
        public static PawnColumnOptionDef DeadThings;

        public PawnColumnOptionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnColumnOptionDef));
        }
    }
}
