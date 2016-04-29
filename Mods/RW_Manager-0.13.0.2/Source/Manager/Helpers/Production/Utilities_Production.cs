// Manager/Utilities_Production.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:31

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public static class Utilities_Production
    {
        /// <summary>
        ///     Creates a shallow and barebone copy of a Bill_Production, or Bill_ProductionWithUft cast to Bill_Production if the
        ///     input is an Uft Bill.
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        public static Bill_Production Copy( this Bill_Production bill )
        {
            Bill_Production copy;
            copy = bill is Bill_ProductionWithUft
                ? new Bill_ProductionWithUft( bill.recipe )
                : new Bill_Production( bill.recipe );

            // copy relevant attributes, others are set by manager when assigning
            // uft specific things are irrelevant here, and set by core
            copy.ingredientFilter = bill.ingredientFilter;
            copy.ingredientSearchRadius = bill.ingredientSearchRadius;
            copy.storeMode = bill.storeMode;
            copy.minSkillLevel = bill.minSkillLevel;

            return copy;
        }

        /// <summary>
        ///     Amount per worker to satsify the bill.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="workerIndex"></param>
        /// <returns></returns>
        public static int CountPerWorker( this ManagerJob_Production job, int workerIndex )
        {
            int n = job.BillGivers.CurBillGiverCount;
            int diff = job.Trigger.Count - job.Trigger.CurCount;

            int bills;

            // if diff is negative we can assume this is a destructive job.
            if ( diff < 0 )
            {
                // default input count set to 1
                float inputCount = 1;
                IEnumerable<ThingDef> filterThingDefs = job.Trigger.ThresholdFilter.AllowedThingDefs;

                // if the filter is a single thingdef, try to figure out what the input count of it is for the managed recipe.
                if ( filterThingDefs?.Count() == 1 )
                {
                    // only one thingdef in the filter, so get the first.
                    ThingDef filterThingDef = filterThingDefs.First();

                    // the ingredient list of the managed recipe.
                    List<IngredientCount> ingredients = job.Bill.recipe.ingredients;

                    // get the total input count of any ingredients that allow our thingdef (probably 0 or 1, but who knows - could be a stuff + specific listing).
                    float? recipeInput =
                        ingredients?.Where( ing => ing.filter.AllowedThingDefs.Contains( filterThingDef ) )
                                    .Select( ing => ing.CountRequiredOfFor( filterThingDef, job.Bill.recipe ) )
                                    .Sum();

                    // if this wasn't null or close to zero, set the reduction count per bill to this number.
                    if ( recipeInput != null &&
                         Math.Abs( recipeInput.Value ) > 1 )
                    {
                        inputCount = recipeInput.Value;
                    }
                }

                // divide by negative reduction amount per bill to get a positive number of bills.
                bills = Mathf.CeilToInt( diff / - inputCount );
            }
            else
            {
                // this one is a bit simpler - mainly because we already did the complicated stuff in the MainProduct class.
                bills = Mathf.CeilToInt( diff / job.MainProduct.Count );
            }

            // naive number of bills per worker (float)
            float naive = bills / (float)n;

            // round up or down to get a total that matches the desired total exactly
            if ( bills % n > workerIndex )
            {
                return (int)Math.Floor( naive );
            }
            return (int)Math.Ceiling( naive );
        }

        /// <summary>
        ///     Get all currently build buildings that use the recipe
        /// </summary>
        /// <param name="rd"></param>
        /// <returns></returns>
        public static List<Building_WorkTable> CurrentRecipeUsers( this RecipeDef rd )
        {
            List<ThingDef> recipeUsers = rd.GetRecipeUsers();
            List<Building_WorkTable> currentRecipeUsers = new List<Building_WorkTable>();

            foreach ( ThingDef td in recipeUsers )
            {
                currentRecipeUsers.AddRange(
                    Find.ListerBuildings.AllBuildingsColonistOfDef( td ).Select( b => b as Building_WorkTable ) );
            }

            return currentRecipeUsers;
        }

        /// <summary>
        ///     Get the thingdefs for everything that can potentially be a billgiver for rd.
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="includeNonBuilding"></param>
        /// <returns></returns>
        public static List<ThingDef> GetRecipeUsers( this RecipeDef rd, bool includeNonBuilding = false )
        {
            List<ThingDef> recipeUsers = new List<ThingDef>();

            // probably redundant starting point, get recipeusers as defined in the recipe.
            if ( rd.recipeUsers != null )
            {
                recipeUsers.AddRange( rd.recipeUsers );
            }

            // fetch thingdefs which have recipes, and the recipes include ours.
            recipeUsers.AddRange(
                DefDatabase<ThingDef>.AllDefsListForReading.Where( t => t.recipes != null && t.recipes.Contains( rd ) )
                                     .ToList() );
            if ( !includeNonBuilding )
            {
                recipeUsers = recipeUsers.Where( t => t.category == ThingCategory.Building ).ToList();
            }
            return recipeUsers.Distinct().ToList();
        }

        /// <summary>
        ///     Does the recipe have a building billgiver, and is it built?
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="built"></param>
        /// <returns></returns>
        public static bool HasBuildingRecipeUser( this RecipeDef rd, bool built = false )
        {
            List<ThingDef> recipeUsers = GetRecipeUsers( rd );
            return
                recipeUsers.Any(
                    t =>
                        ( t.category == ThingCategory.Building ) &&
                        ( !built ||
                          Find.ListerThings.ThingsInGroup( ThingRequestGroup.PotentialBillGiver )
                              .Select( thing => thing.def )
                              .Contains( t ) ) );
        }
    }
}