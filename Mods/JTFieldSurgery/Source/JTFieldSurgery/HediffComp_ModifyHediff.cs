using System.Collections.Generic;
using Verse;

namespace JTFieldSurgery
{

    /**
     * <summary>Component to reduce another hediff, but only if that other hediff is already present.</summary>
     */
    class HediffComp_ModifyHediff : HediffComp
    {
        protected const int SeverityUpdateInterval = 200;

        HediffCompProperties_ModifyHediff Props {
            get {
                return (HediffCompProperties_ModifyHediff)props;
            }
        }

        public override void CompPostTick (ref float severityAdjustment)
        {
            base.CompPostTick (ref severityAdjustment);
            if (Pawn.IsHashIntervalTick (SeverityUpdateInterval)) {
                var doomed = new List<Hediff> ();
                foreach (Hediff hediff in Pawn.health.hediffSet.hediffs) {
                    foreach (ModifyHediff_HediffModifier hediffMod in this.Props.hediffModifiers) {
                        if (hediff.def == hediffMod.hediffDef) {
                            float num = ReduceSeverityPerDay (hediffMod);
                            num *= SeverityUpdateInterval / 60000f; //ticks per day
                            if (num < 0f && (hediff.Severity + num) < hediffMod.removeBelowSeverity) {
                                doomed.Add (hediff);
                            } else if ((num < 0f && (hediff.Severity + num) < hediffMod.minimumSeverityLimit) ||
                                  (num > 0f && (hediff.Severity + num) > hediffMod.maximumSeverityLimit)) {
                                hediff.Severity = num;
                            } else hediff.Severity += num;
                        }
                    }
                }
                //Remove them here to avoid any exceptions when removing entries from a list being iterated on. (C# is not a clever language.)
                for (int i = doomed.Count - 1; i >= 0; i--) {
                    Pawn.health.RemoveHediff (doomed [i]);
                }
            }
        }

        public override string CompDebugString ()
        {
            string output = "";
            foreach (ModifyHediff_HediffModifier hediffMod in Props.hediffModifiers) {
                output += hediffMod.hediffDef.defName + ": " + hediffMod.severityPerDay;
                if (!float.IsNegativeInfinity (hediffMod.removeBelowSeverity)) {
                    output += "(remove <= " + hediffMod.removeBelowSeverity + ")";
                }
                output += "\n";
            }
            return output;
        }

        protected virtual float ReduceSeverityPerDay (ModifyHediff_HediffModifier hediffMod)
        {
            return hediffMod.severityPerDay;
        }
    }

    public class HediffCompProperties_ModifyHediff : HediffCompProperties
    {
        public List<ModifyHediff_HediffModifier> hediffModifiers;

        public HediffCompProperties_ModifyHediff ()
        {
            compClass = typeof (HediffComp_ModifyHediff);
        }
    }

    public class ModifyHediff_HediffModifier
    {
        public HediffDef hediffDef;

        [Unsaved]
        ///<summary>Amount by which target health diff is modified on a daily basis.</summary>
        public float severityPerDay = -1f;
        [Unsaved]
        ///<summary>If the target health diff would be reduced below this amount with a negative severityPerDay, there is no effect.</summary>
        public float minimumSeverityLimit = float.NegativeInfinity;
        [Unsaved]
        ///<summary>If the target health diff would be increased above this amount with a positive severityPerDay, there is no effect.</summary>
        public float maximumSeverityLimit = float.PositiveInfinity;
        [Unsaved]
        ///<summary>If the target health diff is reduced below this amount, remove it.</summary>
        public float removeBelowSeverity = float.NegativeInfinity;
    }

}
