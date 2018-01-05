using System.Text;
using RimWorld;
using Verse;

namespace JTFieldSurgery
{

    class Comp_DeteriorateWhenFrozen : ThingComp
    {
        public bool IsFrozen { get { return _frozen; } }
        bool _frozen = false;

        [Unsaved]
        static bool _warned = false;

        public CompProperties_DeteriorateWhenFrozen Props {
            get {
                return (CompProperties_DeteriorateWhenFrozen)this.props;
            }
        }

        float degradation = 0;

        public override void PostExposeData ()
        {
            base.PostExposeData ();
            Scribe_Values.Look<float> (ref degradation, "degradation", 0f, false);
        }

        public override void CompTick ()
        {
            if (!_warned) {
                Messages.Message ("WARNING: third-party mod author used " + GetType ().Name + " in Thing " + parent + " with <tickerRate>Normal</tickerRate>; this component should only be used with <tickerRate>Rare</tickerRate>", MessageTypeDefOf.NegativeEvent);
                Log.ErrorOnce ("WARNING: third-party mod author used " + GetType ().Name + " in Thing " + parent + " with <tickerRate>Normal</tickerRate>; this component should only be used with <tickerRate>Rare</tickerRate>", 0x7A7F0001);
                _warned = true;
            }
            if (Find.TickManager.TicksAbs % GenTicks.TickRareInterval == 0) CompTickRare ();
        }

        public override void CompTickRare ()
        {
            float temperature = GenTemperature.GetTemperatureForCell (this.parent.PositionHeld, this.parent.MapHeld);
            if (temperature >= 0.001f && _frozen) {
                _frozen = false;
                if (Props.thawDamage > 0) {
                    int damage = GenMath.RoundRandom (Props.thawDamage);
                    if (parent.HitPoints <= damage) {
                        Messages.Message ("JTFieldSurgeryFreezeDestroyed".Translate (new object [] { parent.Label }).CapitalizeFirst (), MessageTypeDefOf.SilentInput);
                    }
                    parent.TakeDamage (new DamageInfo (DamageDefOf.Deterioration, damage, -1f, null, null, null));
                }
            } else if (temperature < 0.001f && !_frozen) {
                _frozen = true;
                if (Props.freezeDamage > 0) {
                    int damage = GenMath.RoundRandom (Props.freezeDamage);
                    if (parent.HitPoints <= damage) {
                        Messages.Message ("JTFieldSurgeryThawDestroyed".Translate (new object [] { parent.Label }).CapitalizeFirst (), MessageTypeDefOf.SilentInput);
                    }
                    parent.TakeDamage (new DamageInfo (DamageDefOf.Deterioration, damage, -1f, null, null, null));
                }
            } else if (temperature < 0.001f) {
                CheckDegradation ();
            }
        }

        public override string CompInspectStringExtra ()
        {
            if (_frozen) {
                int degradation_time_ticks = (int)(Props.daysToDeteriorateCompletely * ((float)parent.HitPoints / (float)parent.MaxHitPoints) * GenDate.TicksPerDay);
                var newstring = new StringBuilder ();
                newstring.Append (
                    "JTFieldSurgeryFreezeDeteriorate".Translate (new object [] { GenDate.ToStringTicksToPeriodVagueMax (degradation_time_ticks) })
                    );
                newstring.Append (".");
                return newstring.ToString ();
            }
            return base.CompInspectStringExtra ();
        }

        void CheckDegradation ()
        {
            if (Props.daysToDeteriorateCompletely > 0f) {
                float degrade_intervals = Props.daysToDeteriorateCompletely * (float)GenDate.TicksPerDay / (float)parent.MaxHitPoints / (float)GenTicks.TickRareInterval;
                degradation += 1;
                while (degradation >= degrade_intervals) {
                    degradation -= degrade_intervals;
                    if (parent.HitPoints <= 1) {
                        Messages.Message ("JTFieldSurgeryFreezeDestroyed".Translate (new object []
                        {
                        parent.Label
                        }).CapitalizeFirst (), MessageTypeDefOf.SilentInput);
                    }
                    parent.TakeDamage (new DamageInfo (DamageDefOf.Deterioration, 1, -1f, null, null, null));
                }
            }
        }
    }

    class CompProperties_DeteriorateWhenFrozen : CompProperties
    {
        ///<summary>Number of days for hit points to reach zero (when at max hit points -- freeze-thaw will shorten life considerably).  Less than/equal to 0 to disable.</summary>
        public float daysToDeteriorateCompletely = 30.0f;
        ///<summary>Hit points to reduce from item upon dropping below freezing.  Less than/equal to zero to disable.</summary>
        public float freezeDamage = 10f;
        ///<summary>Hit points to reduce from item upon thaw.  Less than/equal to zero to disable.</summary>
        public float thawDamage = 0f;

        public CompProperties_DeteriorateWhenFrozen ()
        {
            compClass = typeof (Comp_DeteriorateWhenFrozen);
        }
    }

}
