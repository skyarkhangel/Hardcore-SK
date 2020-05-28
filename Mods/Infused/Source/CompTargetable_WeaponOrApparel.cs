using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Infused
{
    public class CompTargetable_WeaponOrApparel : CompTargetable
    {
        InfusionAllowance allowance = new InfusionAllowance();

        bool stealsInfusions;

        protected override bool PlayerChoosesTarget => true;

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            var comp = parent.GetComp<CompInfused>();
            if (comp == null || comp.InfusionCount == 0)
            {
                stealsInfusions = true;

                return;
            }

            foreach (var infusion in comp.Infusions)
            {
                var filter = infusion.filter;

                if (filter == null)
                {
                    continue;
                }

                allowance.Merge(filter.allowance);
            }
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetHumans = false,
                canTargetMechs = false,
                mapObjectTargetsMustBeAutoAttackable = false,

                canTargetItems = true,
                canTargetBuildings = true,

                mustBeSelectable = true,

                validator = Validate
            };

            bool Validate(TargetInfo x)
            {
                var thing = x.Thing;
                if (thing == null)
                {
                    return false;
                }

                if (thing.TryGetComp<CompQuality>() == null)
                {
                    return false;
                }

                int infusionCount = thing.TryGetComp<CompInfused>()?.InfusionCount ?? 0;

                if (stealsInfusions && infusionCount > 0)
                {
                    return true;
                }

                if (infusionCount >= Settings.max )
                {
                    return false;
                }

                var def = x.Thing.def;

                if (allowance.Allows(def))
                {
                    return true;
                }

                return false;
            }
        }

        public override string TransformLabel(string label)
        {
            if (stealsInfusions)
            {
                return "Empty " + base.TransformLabel(label);
            }
            return base.TransformLabel(label);
        }

        string cachedInspectStringExtra;
        public override string CompInspectStringExtra()
        {
            if (cachedInspectStringExtra == null && !stealsInfusions)
            {
                var sb = new System.Text.StringBuilder();

                bool comma = false;
                void Comma()
                {
                    if (comma)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        comma = true;
                    }
                }

                sb.Append("Allows: ");
                if (allowance.apparel)
                {
                    Comma();
                    sb.Append("apparel");
                }
                if (allowance.melee)
                {
                    Comma();
                    sb.Append("melee weapons");
                }
                if (allowance.ranged)
                {
                    Comma();
                    sb.Append("ranged weapons");
                }
                if (allowance.furniture)
                {
                    Comma();
                    sb.Append("furniture");
                }

                cachedInspectStringExtra = sb.ToString();
            }
            return cachedInspectStringExtra;
        }

        string cachedDescriptionPart;
        public override string GetDescriptionPart()
        {
            if (cachedDescriptionPart == null && !stealsInfusions)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Allows:");
                if (allowance.apparel)
                {
                    sb.AppendLine("  Apparel");
                }
                if (allowance.melee)
                {
                    sb.AppendLine("  Melee Weapons");
                }
                if (allowance.ranged)
                {
                    sb.AppendLine("  Ranged Weapons");
                }
                if (allowance.furniture)
                {
                    sb.AppendLine("  Furniture");
                }

                cachedDescriptionPart = sb.ToString();
            }
            return cachedDescriptionPart;

        }
    }
}
