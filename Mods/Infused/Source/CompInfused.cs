using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Infused
{
    public class CompInfused : ThingComp
    {
        bool isNew;
        List<Def> infusions;

        Def cachedInfusedStrongest;
        string cachedInfusedLabel;
        string cachedInfusedLabelShort;
        string cachedInfusedLabelAbbr;
        string cachedInfusedDescriptionPart;

        public IEnumerable<Def> Infusions => infusions ?? Enumerable.Empty<Def>();
        public int InfusionCount => IsActive ? infusions.Count : 0;

        Def InfusedStrongest => cachedInfusedStrongest ?? (cachedInfusedStrongest = infusions.MaxBy(i => i.tier));

        public Color InfusedLabelColor => ResourceBank.Colors.InfusionColor(InfusedStrongest.tier);

        public string InfusedLabel => cachedInfusedLabel ?? (cachedInfusedLabel = GetInfusionLabel(false));
        public string InfusedLabelShort => cachedInfusedLabelShort ?? (cachedInfusedLabelShort = GetInfusionLabel(true));
        public string InfusedLabelAbbr => cachedInfusedLabelAbbr ?? (cachedInfusedLabelAbbr = GetInfusionLabelAbbr());

        public bool IsActive => !infusions.NullOrEmpty();

        void InvalidateCache()
        {
            cachedInfusedStrongest = null;
            cachedInfusedLabel = null;
            cachedInfusedLabelShort = null;
            cachedInfusedLabelAbbr = null;
            cachedInfusedDescriptionPart = null;
        }

        public void Attach(Def def)
        {
            if (infusions == null)
            {
                infusions = new List<Def>();
                isNew = true;
            }

            if (infusions.Contains(def))
            {
                return;
            }

            infusions.Add(def);
            infusions = infusions.OrderByDescending(i => i.tier).ToList();

            InvalidateCache();
        }

        public void SetInfusions(List<Def> list)
        {
            infusions = list.Distinct().OrderByDescending(i => i.tier).ToList();
            isNew = true;

            InvalidateCache();
        }

        public List<Def> RemoveRandom(int v)
        {
            var randomInfusions = infusions.TakeRandom(v).ToList();

            foreach(var infusion in randomInfusions)
            {
                infusions.Remove(infusion);
            }

            InvalidateCache();

            return randomInfusions;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostPostMake()
        {
            if (props is CompProperties_Enchant infusedProps)
            {
                #if DEBUG
                Log.Message($"Infused :: {parent} will be infused");
                #endif
                // This is ONLY called when Infused is set in XML
                infusions = InfusedMod.Infuse(
                    parent,
                    infusedProps.quality,
                    max:Settings.max,
                    skipThingFilter: true
                ).ToList();

                parent.HitPoints = parent.MaxHitPoints;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // We only throw notifications for newly spawned items.
            if (isNew && IsActive)
            {
                ThrowMote();
            }
        }

        internal void ThrowMote()
        {
            CompQuality compQuality = parent.TryGetComp<CompQuality>();
            if (compQuality == null)
            {
                return;
            }
            string qualityLabel = compQuality.Quality.GetLabel();

            var msg = new StringBuilder();
            msg.Append(qualityLabel);
            msg.Append(" ");
            if (parent.Stuff != null)
            {
                msg.Append(parent.Stuff.LabelAsStuff);
                msg.Append(" ");
            }
            msg.Append(parent.def.label);
            Messages.Message(ResourceBank.Strings.Notice(msg.ToString()), new RimWorld.Planet.GlobalTargetInfo(parent), MessageTypeDefOf.SilentInput);

            ResourceBank.Sounds.Infused.PlayOneShotOnCamera();

            if (parent.Map != null)
                MoteMaker.ThrowText(parent.Position.ToVector3Shifted(), parent.Map, ResourceBank.Strings.Mote, ResourceBank.Colors.Legendary);

            isNew = false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // let's not bloat saves...
                if (infusions != null)
                {
                    if (infusions.Count > 1)
                    {
                        infusions = infusions.OrderBy(i => i.labelReversed).ToList();
                    }
                    Scribe_Collections.Look(ref infusions, "infusions", LookMode.Def);
                }

            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {

                // Easy loading
                Scribe_Collections.Look(ref infusions, "infusions", LookMode.Def);

                infusions?.RemoveAll(item => item == null);

                isNew = false;
            }
        }

        public override bool AllowStackWith(Thing other)
        {
            return false;
        }

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest)
            {
                return;
            }

            if (!IsActive)
            {
                return;
            }

            GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(parent, -0.66f), InfusedLabelAbbr, InfusedLabelColor);
        }

        public override string TransformLabel(string label)
        {
            // When this function is called, our infusion is no longer new.
            isNew = false;

            if (IsActive)
            {
                return GetInfusionLabel();
            }

            return base.TransformLabel(label);
        }

        string GetInfusionLabelAbbr()
        {
            if (infusions.Count > 1)
            {
                return $"{InfusedStrongest.labelShort}(+{infusions.Count - 1})";
            }
            return InfusedStrongest.labelShort;
        }

        string GetInfusionLabel(bool shorten = true)
        {
            var result = new StringBuilder();

            string label;

            if (parent.Stuff != null)
            {
                label = "ThingMadeOfStuffLabel".Translate(parent.Stuff.LabelAsStuff, parent.def.label);
            }
            else
            {
                label = parent.def.label;
            }

            IEnumerable<string> filter(bool reversed) => infusions.Where(i => i.labelReversed == reversed).SelectMany(i => i.labels).Distinct();

            var prefixes = filter(false);
            var suffixes = filter(true);

            foreach (var str in prefixes)
            {
                result.Append(str);
                result.Append(" ");
            }
            result.Append(label);
            foreach (var str in suffixes)
            {
                result.Append(" ");
                result.Append(str);
            }

            if (parent.TryGetQuality(out QualityCategory qc))
            {
                result.Append(" (");

                if (shorten && result.Length > 20)
                {
                    result.Append(qc.GetLabelShort());
                }
                else
                {
                    result.Append(qc.GetLabel());
                }

                if ((!shorten || result.Length <= 30) && parent.HitPoints < parent.MaxHitPoints)
                {
                    result.Append(" ");
                    result.Append(((float)parent.HitPoints / parent.MaxHitPoints).ToStringPercent());
                }

                if ((parent as Apparel)?.WornByCorpse ?? false)
                {
                    result.Append(" ");
                    result.Append("WornByCorpseChar".Translate());
                }

                result.Append(")");
            }

            return result.ToString().ToLower().CapitalizeFirst();
        }

        public override string GetDescriptionPart()
        {
            if (cachedInfusedDescriptionPart == null)
            {
                if (IsActive)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(base.GetDescriptionPart());

                    foreach (var infusion in infusions)
                    {
                        sb.AppendLine(infusion.DescriptionLabel);
                        sb.Append(infusion.DescriptionStats);
                        sb.Append(infusion.DescriptionExtras);
                        sb.AppendLine();
                    }

                    cachedInfusedDescriptionPart = sb.ToString();
                }
            }
            return cachedInfusedDescriptionPart;
        }

        public static bool TryGetInfusedComp(ThingWithComps thing, out CompInfused comp)
        {
            comp = thing.GetComp<CompInfused>();
            return comp != null && comp.IsActive;
        }

        public static bool TryGetInfusedComp(Thing thing, out CompInfused comp)
        {
            comp = thing.TryGetComp<CompInfused>();
            return comp != null && comp.IsActive;
        }
    }
}
