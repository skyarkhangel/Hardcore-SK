// Manager/MainProductTracker.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:30

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FluffyManager
{
    public class MainProductTracker
    {
        public enum Types
        {
            Thing,
            Category,
            None,
            Unknown
        }

        private ThingCategoryDef   _categoryDef;
        private string             _label;
        private readonly RecipeDef _recipe;
        private ThingDef           _thingDef;
        public Types               Type = Types.Unknown;

        public ThingCategoryDef CategoryDef
        {
            get
            {
                if ( Type != Types.Category )
                {
                    return null;
                }

                return _categoryDef;
            }
        }

        public ThingDef ThingDef
        {
            get
            {
                if ( Type != Types.Thing )
                {
                    return null;
                }

                return _thingDef;
            }
        }

        public string Label
        {
            get
            {
                if ( _label != null )
                {
                    return _label;
                }

                switch ( Type )
                {
                    case Types.Thing:
                        _label = _thingDef.LabelCap;
                        break;

                    case Types.Category:
                        _label = _categoryDef.LabelCap;
                        break;

                    case Types.None:
                        _label = "None";
                        break;

                    case Types.Unknown:
                        _label = "Unkown";
                        break;

                    default:
                        _label = "Error";
                        break;
                }

                return _label;
            }
        }

        /// <summary>
        ///     Default max to Trigger count slider.
        /// </summary>
        public int MaxUpperThreshold
        {
            get
            {
                // go from stacksize
                if ( ThingDef != null )
                {
                    return Math.Max( ThingDef.stackLimit * 40, 100 ); // stacksize * 40, 100 min.
                }

                // if product is not resolved (stone blocks, weapon smelting, category)
                return Trigger_Threshold.DefaultMaxUpperThreshold;
            }
        }

        /// <summary>
        ///     Number of output for product
        /// </summary>
        public int Count { get; private set; } = 1;

        public MainProductTracker( RecipeDef recipe )
        {
            _recipe = recipe;
            Set();
        }

        public void Clear()
        {
            Type = Types.Unknown;
            _thingDef = null;
            _categoryDef = null;
            _label = null;
            Count = 1;
        }

        public void Set()
        {
            try
            {
                // get the (main) product
                if ( _recipe.products != null &&
                     _recipe.products.Count > 0 &&
                     _recipe.products.First().thingDef.BaseMarketValue > 0 )
                {
                    Clear();
                    _thingDef = _recipe.products.First().thingDef;
                    Type = Types.Thing;
                    Count = _recipe.products.First().count;
                    return;
                }

                // no main, is there a special?
                if ( _recipe.specialProducts == null )
                {
                    Clear();
                    Type = Types.None;
                    Count = 0;
                }
                if ( _recipe.specialProducts != null &&
                     _recipe.specialProducts.Count > 0 )
                {
                    // get the first special product of the first thingdef allowed by the fixedFilter.
                    if ( _recipe.defaultIngredientFilter.AllowedThingDefs == null )
                    {
                        throw new Exception( "AllowedThingDefs NULL" );
                    }
                    ThingDef allowedThingDef =
                        _recipe.fixedIngredientFilter.AllowedThingDefs.DefaultIfEmpty( null ).FirstOrDefault();
                    if ( allowedThingDef == null )
                    {
                        throw new Exception( "AllowedThingDef NULL" );
                    }

                    if ( _recipe.specialProducts[0] == SpecialProductType.Butchery )
                    {
                        if ( allowedThingDef.butcherProducts != null &&
                             allowedThingDef.butcherProducts.Count > 0 )
                        {
                            // butcherproducts are defined, no problem.
                            List<ThingCount> butcherProducts = allowedThingDef.butcherProducts;
                            if ( butcherProducts.Count == 0 )
                            {
                                throw new Exception( "No butcherproducts defined: " + allowedThingDef.defName );
                            }

                            Clear();
                            _thingDef = butcherProducts.First().thingDef;
                            Type = Types.Thing;
                            Count = butcherProducts.First().count;
                            return;
                        }

                        // still not defined, see if we can catch corpses.
                        if ( allowedThingDef.defName.Contains( "Corpse" ) &&
                             !allowedThingDef.defName.Contains( "Mechanoid" ) )
                        {
                            // meat for non-mech corpses
                            Clear();
                            _categoryDef = ThingCategoryDef.Named( "MeatRaw" );
                            Type = Types.Category;
                            Count = 50;
                        }
                        else if ( allowedThingDef.defName.Contains( "Corpse" ) &&
                                  allowedThingDef.defName.Contains( "Mechanoid" ) )
                        {
                            // plasteel for mech corpses
                            Clear();
                            _thingDef = ThingDef.Named( "Plasteel" );
                            Type = Types.Thing;
                            Count = 20;
                        }
                        else
                        {
                            Clear();
                            return;
                        }
                    }

                    if ( _recipe.specialProducts[0] == SpecialProductType.Smelted )
                    {
                        if ( allowedThingDef.smeltProducts == null )
                        {
                            Clear();
                            return;
                        }

                        List<ThingCount> smeltingProducts = allowedThingDef.smeltProducts;
                        if ( smeltingProducts.Count == 0 )
                        {
                            Clear();
                            return;
                        }

                        Clear();
                        _thingDef = smeltingProducts.First().thingDef;
                        Type = Types.Thing;
                        Count = smeltingProducts.First().count;
                        if ( _thingDef == null )
                        {
                            Clear();
                        }
                    }
                }
            }

                // ReSharper disable once UnusedVariable
            catch ( Exception e )
            {
#if DEBUG
                Log.Warning( e.Message );
#endif
                Clear();
            }
        }
    }
}