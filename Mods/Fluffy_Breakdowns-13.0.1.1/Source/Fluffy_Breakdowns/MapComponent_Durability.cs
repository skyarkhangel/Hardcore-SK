using CommunityCoreLibrary.UI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System;

namespace Fluffy_Breakdowns
{
    public class MapComponent_Durability : MapComponent
    {
        #region Fields

        public const int componentLifetime = GenDate.TicksPerSeason;
        private const int _moteIntervalRequiresCriticalRepair = 15;
        private const int _moteIntervalRequiresRepair = 30;
        private const float maintenanceThreshold = .9f;
        private static Dictionary<CompBreakdownable, float> _durabilities = new Dictionary<CompBreakdownable, float>();
        private static List<DurabilityPair> _durabilityScribeHelper;

        static Dictionary<string, DurabilityPair> asd;

        #endregion Fields

        public class DurabilityPair : IExposable
        {
            public ThingWithComps thing;
            public float durability = 1f;

            public DurabilityPair()
            {
                // scribe
            }

            public DurabilityPair( ThingWithComps thing, float durability = 1f )
            {
                this.thing = thing;
                this.durability = durability;
            }

            public void ExposeData()
            {
                Scribe_References.LookReference( ref thing, "thing" );
                Scribe_Values.LookValue( ref durability, "durability" );
            }
        }

        #region Properties

        public static IEnumerable<Thing> potentialMaintenanceThings
        {
            get
            {
                return _durabilities.Select( p => p.Key.parent ).Where( twc => twc?.Spawned ?? false ).Cast<Thing>();
            }
        }

        #endregion Properties

        #region Methods

        public override void ExposeData()
        {
            // create a list of saveable thing/durability pairs
            if (Scribe.mode == LoadSaveMode.Saving )
            {
                _durabilityScribeHelper = _durabilities.Select( pair => new DurabilityPair( pair.Key.parent, pair.Value ) ).ToList();
            }

            // save/load the list
            Scribe_Collections.LookList( ref _durabilityScribeHelper, "durabilities", LookMode.Deep );

            // reconstruct durability dictionary from saved list
            if (Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                foreach( var helper in _durabilityScribeHelper )
                {
                    var comp = helper?.thing?.TryGetComp<CompBreakdownable>();
                    if ( comp != null && !_durabilities.ContainsKey( comp ) )
                    {
                        _durabilities.Add( comp, helper.durability );
                    }
                }
            }
        }

        public static float GetDurability( CompBreakdownable comp )
        {
            float durability;
            if ( !_durabilities.TryGetValue( comp, out durability ) )
            {
                durability = 1f;
                _durabilities.Add( comp, durability );
            }
            return durability;
        }

        public static float GetDurability( Building building )
        {
            CompBreakdownable comp = building.TryGetComp<CompBreakdownable>();
            if ( comp == null )
                return 1f;
            else
                return GetDurability( comp );
        }

        public static bool RequiresMaintenance( CompBreakdownable comp )
        {
            return GetDurability( comp ) < maintenanceThreshold;
        }

        public static void SetDurability( CompBreakdownable comp, float durability )
        {
            _durabilities[comp] = Mathf.Clamp( durability, .001f, 1f );
        }

        public static void SetDurability( Building building, float durability )
        {
            CompBreakdownable comp = building.TryGetComp<CompBreakdownable>();
            if ( comp != null )
                SetDurability( comp, durability );
        }

#if DEBUG
        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            Rect statusRect = new Rect( 0f, Screen.height * 1/3f, Screen.width * 1/2f, Screen.height * 1/3f );
            CCL_Widgets.Label( statusRect, _durabilities.Select( p => p.Key.parent.LabelCap + ": " + p.Value.ToStringPercent() ).StringJoin( "\n" ) );
        }
#endif

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            int tick = Find.TickManager.TicksGame;

            foreach ( var _dur in _durabilities )
            {
                float durability = _dur.Value;
                var comp = _dur.Key;
                if ( comp?.parent?.Spawned ?? false )
                {
                    if ( durability < .5 && ( tick + comp.GetHashCode() ) % _moteIntervalRequiresRepair == 0 )
                        MoteThrower.ThrowSmoke( comp.parent.DrawPos, ( 1f - durability ) * 1/2f );

                    if ( durability < .25 && ( tick + comp.GetHashCode() ) % _moteIntervalRequiresCriticalRepair == 0 )
                        MoteThrower.ThrowMicroSparks( comp.parent.DrawPos );
                }
            }
        }

        #endregion Methods
    }
}