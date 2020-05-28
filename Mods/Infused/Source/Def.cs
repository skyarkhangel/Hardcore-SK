using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infused
{
    public enum InfusionTier
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Artifact
    }

    public struct TechLevelRange
    {
        public TechLevel min;
        public TechLevel max;

        public bool Allows(TechLevel tech)
        {
            return tech >= min
                && tech <= max;
        }

        public void Merge(TechLevelRange other)
        {
            if (min > other.min)
                min = other.min;
            if (max < other.max)
                max = other.max;
        }
    }

    public struct InfusionAllowance
    {
        public bool all;
        public bool amplifier;
        public bool melee;
        public bool ranged;
        public bool apparel;
        public bool furniture;

        public bool Allows(ThingDef def)
        {
            return all
                || amplifier && def == ResourceBank.Things.InfusedAmplifier
                || melee && def.IsMeleeWeapon
                || ranged && def.IsRangedWeapon
                || apparel && def.IsApparel
                || furniture && def.building != null;
        }

        public void Merge(InfusionAllowance other)
        {
            all       |= other.all;
            amplifier |= other.amplifier;
            melee     |= other.melee;
            ranged    |= other.ranged;
            apparel   |= other.apparel;
            furniture |= other.furniture;
        }
    }

    public class Filter
    {
        public readonly InfusionAllowance allowance = new InfusionAllowance()
        {
            all = true,
            amplifier = true,
            melee = true,
            ranged = true,
            apparel = true,
            furniture = true
        };

        public readonly TechLevelRange techLevel = new TechLevelRange()
        {
            min = TechLevel.Undefined,
            max = TechLevel.Archotech
        };

        public readonly ThingFilter match;

        public bool Allows(Thing thing)
        {
            return techLevel.Allows(thing.def.techLevel)
                && allowance.Allows(thing.def)
                && (match?.Allows(thing) ?? true);
        }

        public void ResolveReferences()
        {
            match?.ResolveReferences();
        }
    }

    public class Def : Verse.Def
    {
        [Unsaved]
        public string[] labels;

        public string labelShort = "#NN";
        public bool labelReversed;

        public InfusionTier tier;

        public Dictionary<StatDef, StatMod> stats = new Dictionary<StatDef, StatMod>();

        public int weight = 1;

        public List<string> tags;

        public Filter filter;

        // Cached Strings for ITAb
        public string DescriptionLabel { get; private set; }
        public string DescriptionStats { get; private set; }
        public string DescriptionExtras { get; private set; }

        // Get matching StatMod for given StatDef. Returns false when none.
        public bool TryGetStatValue(StatDef stat, out StatMod mod)
        {
            return stats.TryGetValue(stat, out mod);
        }

        public override void ResolveReferences()
        {
            // search if we already added the StatPart
            Predicate<StatPart> predicate = (StatPart part) => part.GetType() == typeof(StatPart_InfusedModifier);

            foreach (StatDef statDef in stats.Keys)
            {
                if (statDef.forInformationOnly)
                {
                    Log.Warning($"Infused :: {defName} -> {statDef.defName} is ForInformationOnly");
                }

                if (statDef.parts == null)
                {
                    statDef.parts = new List<StatPart>(1);
                }
                else if (statDef.parts.Any(predicate))
                {
                    continue;
                }

                statDef.parts.Add(new StatPart_InfusedModifier(statDef));
            }

            filter?.ResolveReferences();

            BuildStringCache();
        }

        void BuildStringCache()
        {
            labels = label.Split(' ');

            StringBuilder sb = new StringBuilder();

            // Label
            sb.Append(label);
            sb.Append("(");
            sb.Append(ResourceBank.Strings.Tier(tier));
            sb.Append(")");
            DescriptionLabel = sb.ToString();

            // Stats
            sb.Clear();
            foreach (var current in stats)
            {
                if (Math.Abs(current.Value.offset) > 0.0001f)
                {
                    sb.Append("     ");
                    sb.Append(current.Value.offset > 0 ? "+" : "-");
                    if (current.Key == StatDefOf.ComfyTemperatureMax || current.Key == StatDefOf.ComfyTemperatureMin)
                    {
                        sb.Append(Mathf.Abs(current.Value.offset).ToStringTemperatureOffset());
                    }
                    else if (current.Key.parts.Find(s => s is StatPart_InfusedModifier) is var modifier)
                    {
                        sb.Append(modifier.parentStat.ValueToString(Mathf.Abs(current.Value.offset)));
                    }
                    sb.Append(" ");
                    sb.AppendLine(current.Key.LabelCap);
                }
                if (Math.Abs(current.Value.multiplier - 1) > 0.0001f)
                {
                    sb.Append("     ");
                    sb.Append(Mathf.Abs(current.Value.multiplier).ToStringPercent());
                    sb.Append(" ");
                    sb.AppendLine(current.Key.LabelCap);
                }
            }
            DescriptionStats = sb.ToString();

            // Extras
            sb.Clear();

            DescriptionExtras = sb.ToString();
        }

        public class StatMod
        {
            public float offset;
            public float multiplier = 1;

            public override string ToString()
            {
                return string.Format("[StatMod offset={0}, multiplier={1}]", offset, multiplier);
            }
        }

        public struct Info
        {
            public string label;
            public string stats;
            public string extras;
        }
    }

    public class ChanceDef : Verse.Def
    {
        public List<string> allowTags;

        public Filter filter;

        public int slotBonusPerParts = 0;

        public List<float> slots = new List<float>{ 1f };

        public QualityChances chances;

        public float Chance(QualityCategory qc)
        {
            switch (qc)
            {
                case QualityCategory.Awful:
                    return chances.awful;
                case QualityCategory.Poor:
                    return chances.poor;
                case QualityCategory.Normal:
                    return chances.normal;
                case QualityCategory.Good:
                    return chances.good;
                case QualityCategory.Excellent:
                    return chances.excellent;
                case QualityCategory.Masterwork:
                    return chances.masterwork;
                case QualityCategory.Legendary:
                    return chances.legendary;
                default:
                    return 0f;
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            filter?.ResolveReferences();
        }

        public struct QualityChances
        {
            public float awful;
            public float poor;
            public float normal;
            public float good;
            public float excellent;
            public float masterwork;
            public float legendary;
        }
    }

    public class OnHitDef : Verse.Def
    {
        public DamageDef damage;

        public StatDef amount;
        public StatDef chance;

        public override void PostLoad()
        {
            defName = "OnHitDef" + number++;
        }

        static int number;
    }
}
