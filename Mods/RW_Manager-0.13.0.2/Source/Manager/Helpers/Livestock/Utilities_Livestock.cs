// Manager/Utilities_Livestock.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-29 21:15

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FluffyManager
{
    public static class Utilities_Livestock
    {
        public enum AgeAndSex
        {
            AdultFemale = 0,
            AdultMale = 1,
            JuvenileFemale = 2,
            JuvenileMale = 3
        }

        public static AgeAndSex[] AgeSexArray = (AgeAndSex[])Enum.GetValues( typeof (AgeAndSex) );

        private static Dictionary<PawnKindDef, Utilities.CachedValue<IEnumerable<Pawn>>> _allCache =
            new Dictionary<PawnKindDef, Utilities.CachedValue<IEnumerable<Pawn>>>();

        public static bool PawnIsOfAgeSex( this Pawn p, AgeAndSex ageSex )
        {
            // we're making the assumption here that anything with a lifestage index of 2 or greater is adult - so 3 lifestages.
            // this works for vanilla and all modded animals that I know off.

            switch ( ageSex )
            {
                case AgeAndSex.AdultFemale:
                    return p.gender == Gender.Female && p.ageTracker.CurLifeStageIndex >= 2;
                case AgeAndSex.AdultMale:
                    return p.gender == Gender.Male && p.ageTracker.CurLifeStageIndex >= 2;
                case AgeAndSex.JuvenileFemale:
                    return p.gender == Gender.Female && p.ageTracker.CurLifeStageIndex < 2;
                case AgeAndSex.JuvenileMale:
                default:
                    return p.gender == Gender.Male && p.ageTracker.CurLifeStageIndex < 2;
            }
        }

        public static IEnumerable<Pawn> GetAll( this PawnKindDef pawnKind )
        {
            // check if we have a cached version
            IEnumerable<Pawn> cached;

            // does it exist at all?
            bool cacheExists = _allCache.ContainsKey( pawnKind );

            // is it up to date?
            if ( cacheExists &&
                 _allCache[pawnKind].TryGetValue( out cached ) &&
                 cached != null )
            {
                return cached;
            }

            // if not, get a new list.
            cached = Find.MapPawns.AllPawns
                         .Where( p => p.RaceProps.Animal // is animal
                                      && !p.Dead // is alive
                                      && p.kindDef == pawnKind ); // is our managed pawnkind

            // update if key exists
            if ( cacheExists )
            {
                _allCache[pawnKind].Update( cached );
            }

            // else add it
            else
            {
                _allCache.Add( pawnKind, new Utilities.CachedValue<IEnumerable<Pawn>>( cached ) );
            }
            return cached;
        }

        public static List<Pawn> GetWild( this PawnKindDef pawnKind )
        {
            return pawnKind.GetAll().Where( p => p.Faction == null ).ToList();
        }

        public static List<Pawn> GetTame( this PawnKindDef pawnKind )
        {
            return pawnKind.GetAll().Where( p => p.Faction == Faction.OfColony ).ToList();
        }

        public static IEnumerable<Pawn> GetAll( this PawnKindDef pawnKind, AgeAndSex ageSex )
        {
            return pawnKind.GetAll().Where( p => PawnIsOfAgeSex( p, ageSex ) ); // is of age and sex we want
        }

        public static List<Pawn> GetWild( this PawnKindDef pawnKind, AgeAndSex ageSex )
        {
#if DEBUG_LIFESTOCK_COUNTS
            foreach (Pawn p in GetAll( ageSex )) Log.Message(p.Faction?.GetCallLabel() ?? "NULL" );
            List<Pawn> wild = GetAll( ageSex ).Where( p => p.Faction == null ).ToList();
            Log.Message( "Wildcount " + ageSex + ": " + wild.Count );
            return wild;
#else
            return pawnKind.GetAll( ageSex ).Where( p => p.Faction == null ).ToList();
#endif
        }

        public static List<Pawn> GetTame( this PawnKindDef pawnKind, AgeAndSex ageSex )
        {
#if DEBUG_LIFESTOCK_COUNTS
            List<Pawn> tame = GetAll( ageSex ).Where( p => p.Faction == Faction.OfColony ).ToList();
            Log.Message( "Tamecount " + ageSex + ": " + tame.Count );
            return tame;
#else
            return pawnKind.GetAll( ageSex ).Where( p => p.Faction == Faction.OfColony ).ToList();
#endif
        }

        private static Dictionary<PawnKindDef, Utilities.CachedValue<bool>> _milkablePawnkind = new Dictionary<PawnKindDef, Utilities.CachedValue<bool>>();
        private static Dictionary<Pawn, Utilities.CachedValue<bool>> _milkablePawn = new Dictionary<Pawn, Utilities.CachedValue<bool>>();
        private static Dictionary<PawnKindDef, Utilities.CachedValue<bool>> _shearablePawnkind = new Dictionary<PawnKindDef, Utilities.CachedValue<bool>>();
        private static Dictionary<Pawn, Utilities.CachedValue<bool>> _shearablePawn = new Dictionary<Pawn, Utilities.CachedValue<bool>>();

        public static bool Milkable( this PawnKindDef pawnKind )
        {
            if ( pawnKind == null ) return false;
            bool ret = false;
            if ( _milkablePawnkind.ContainsKey( pawnKind ) )
            {
                if ( _milkablePawnkind[pawnKind].TryGetValue( out ret ) )
                {
                    return ret;
                }
                ret = pawnKind.race.comps.OfType<CompProperties_Milkable>().Any( cp => cp.milkDef != null );
                _milkablePawnkind[pawnKind].Update( ret );
                return ret;
            }
            ret = pawnKind.race.comps.OfType<CompProperties_Milkable>().Any( cp => cp.milkDef != null );
            _milkablePawnkind.Add( pawnKind, new Utilities.CachedValue<bool>( ret, Int32.MaxValue ) );
            return ret;
        }

        public static bool Milkable( this Pawn pawn )
        {
            bool ret = false;
            if ( _milkablePawn.ContainsKey( pawn ) )
            {
                if ( _milkablePawn[pawn].TryGetValue( out ret ) )
                {
                    return ret;
                }
                ret = pawn._milkable();
                _milkablePawn[pawn].Update( ret );
                return ret;
            }
            ret = pawn._milkable();
            _milkablePawn.Add( pawn, new Utilities.CachedValue<bool>( ret, 5000 ) );
            return ret;
        }

        private static bool _milkable( this Pawn pawn )
        {
            CompMilkable comp = pawn?.TryGetComp<CompMilkable>();
            object active = false;
            if( comp != null )
            {
                active = comp.GetPrivatePropertyValue( "Active" );
            }
            return (bool)active;
        }

        public static bool Shearable( this PawnKindDef pawnKind )
        {
            if( pawnKind == null ) return false;
            bool ret = false;
            if( _shearablePawnkind.ContainsKey( pawnKind ) )
            {
                if( _shearablePawnkind[pawnKind].TryGetValue( out ret ) )
                {
                    return ret;
                }
                ret = pawnKind.race.comps.OfType<CompProperties_Shearable>().Any( cp => cp.woolDef != null );
                _shearablePawnkind[pawnKind].Update( ret );
                return ret;
            }
            ret = pawnKind.race.comps.OfType<CompProperties_Shearable>().Any( cp => cp.woolDef != null );
            _shearablePawnkind.Add( pawnKind, new Utilities.CachedValue<bool>( ret, Int32.MaxValue ) );
            return ret;
        }

        public static bool Shearable( this Pawn pawn )
        {
            bool ret = false;
            if( _shearablePawn.ContainsKey( pawn ) )
            {
                if( _shearablePawn[pawn].TryGetValue( out ret ) )
                {
                    return ret;
                }
                ret = pawn._shearable();
                _shearablePawn[pawn].Update( ret );
                return ret;
            }
            ret = pawn._shearable();
            _shearablePawn.Add( pawn, new Utilities.CachedValue<bool>( ret, 5000 ) );
            return ret;
        }

        private static bool _shearable( this Pawn pawn )
        {
            CompShearable comp = pawn?.TryGetComp<CompShearable>();
            object active = false;
            if( comp != null )
            {
                active = comp.GetPrivatePropertyValue( "Active" );
            }
            return (bool)active;
        }
    }
}