// Manager/Utilities_Hunting.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-13 23:33

using RimWorld;
using Verse;

namespace FluffyManager
{
    public static class Utilities_Hunting
    {
        public static ThingDef         HumanMeat = ThingDef.Named( "Human" ).race.meatDef;
        public static ThingCategoryDef RawMeat   = DefDatabase<ThingCategoryDef>.GetNamed( "MeatRaw" );

        public static int EstimatedMeatCount( this Pawn p )
        {
            // StatDef MeatAmount
            // note; an attempt at future proofing, but will probably just make it more likely to break :p
            return (int)( StatDefOf.MeatAmount.defaultBaseValue * p.BodySize );
        }

        public static int GetMeatCount( this Corpse c )
        {
            return EstimatedMeatCount( c.innerPawn );
        }
    }
}