using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public class Building_GuestBed : Building_Bed
    {
        private static readonly Color sheetColorForGuests = new Color(89 / 255f, 55 / 255f, 121 / 255f);

        private int feeStep = 10;

        public int rentalFee;
        private string silverLabel = " " + ThingDefOf.Silver.label;

        public int MoodEffect => Mathf.RoundToInt(rentalFee * -0.1f);

        public override Color DrawColor
        {
            get
            {
                if (def.MadeFromStuff)
                {
                    return base.DrawColor;
                }
                return DrawColorTwo;
            }
        }

        public override Color DrawColorTwo => sheetColorForGuests;

        public BedStats Stats { get; } = new BedStats();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rentalFee, "rentalFee");
        }

        public override void TickLong()
        {
            UpdateStats();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            UpdateStats();
        }

        private void UpdateStats()
        {
            Stats.lastCalculated = GenTicks.TicksGame;
            try
            {
                var owners = this.Owners().Count == 0 ? (string) "Nobody".Translate() : this.Owners().Select(o => (string) o.NameShortColored).ToCommaList(true);
                Stats.title = $"{def.LabelCap} ({owners})";
                Stats.staticBedValue = BedUtility.StaticBedValue(this, out Stats.room, out _, out _, out _);
                Stats.textAttractiveness = "BedAttractiveness".Translate(Stats.staticBedValue - rentalFee);
                Stats.textFee = rentalFee == 0 ? "FeeNone".Translate() : "FeeAmount".Translate(rentalFee);
                Stats.textAsArray = new[] {Stats.textAttractiveness, Stats.textFee};

                if (ModLister.RoyaltyInstalled)
                {
                    Stats.metRoyalTitles = GetMetRoyalTitles(Stats.room);
                    Stats.textNextTitleReq = GetNextTitleReq(Stats.room, Stats.metRoyalTitles);
                }
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"Failed to calculate stats: {e}", 834763462);
            }
        }

        private string GetNextTitleReq(Room room, RoyalTitleDef[] metRoyalTitles)
        {
            if (room == null) return string.Empty;
            if (GuestUtility.AllTitles.NullOrEmpty()) return string.Empty;
            if (!IsSingleBedroom(room)) return "NextTitleRequirements".Translate("BedroomMustBeSingle".Translate());
            foreach (var title in GuestUtility.AllTitles.Where(t=>!metRoyalTitles.Contains(t) && t.bedroomRequirements != null).OrderBy(t=>t.seniority))
            {
                return "NextTitleRequirements".Translate(title.bedroomRequirements.Where(req => !req.Met(room)).Select(req => req.Label(room)).ToCommaList(true));
            }

            return string.Empty;
        }

        private RoyalTitleDef[] GetMetRoyalTitles(Room room)
        {
            try
            {
                if (room != null)
                {
                    return GuestUtility.AllTitles.Where(t => t.bedroomRequirements.NullOrEmpty() || IsSingleBedroom(room) && t.bedroomRequirements.All(req => req.Met(room))).ToArray();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to read royalty titles or their bedroom requirements. This means you are using a mod that changes these and broke them.\n{e}");
            }

            return Array.Empty<RoyalTitleDef>();
        }

        private bool IsSingleBedroom(Room room)
        {
            return !room.ContainedBeds.Any(containedBed => containedBed != this && containedBed.def.building.bed_humanlike);
        }

        public override void Draw()
        {
            base.Draw();
            if (Medical) Medical = false;
            if (ForPrisoners) ForPrisoners = false;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            // Creating copy for iteration, since list changes during loop
            foreach (var owner in this.Owners().ToArray())
            {
                owner.ownership.UnclaimBed();
            }

            var room = this.GetRoom();
            base.DeSpawn(mode);
            room?.Notify_RoomShapeOrContainedBedsChanged();
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append(InspectStringPartsFromComps());

            stringBuilder.AppendLine();
            var owners = this.Owners();
            if (!owners.Any())
            {
                stringBuilder.Append($"{"Owner".Translate()}: {"Nobody".Translate()}");
            }
            else if (owners.Count == 1)
            {
                stringBuilder.Append($"{"Owner".Translate()}: {owners[0].LabelShortCap}");
            }
            else
            {
                stringBuilder.Append("Owners".Translate() + ": ");
                bool notFirst = false;
                foreach (var owner in owners)
                {
                    if (notFirst)
                    {
                        stringBuilder.Append(", ");
                    }
                    notFirst = true;
                    stringBuilder.Append(owner.LabelShortCap);
                }
                //if(notFirst) stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        // Note to whoever wants to add to this method (hi jptrrs!):
        // You can just do
        // foreach (Gizmo c in base.GetGizmos())
        // {
        //    yield return c;
        // }
        // yield return [whatever you want to add];
        //
        // No need to copy this method.
        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Display the original gizmos (includes the swap guest bed button via patch)
            foreach (var gizmo in base.GetGizmos())
            {
                switch (gizmo)
                {
                    case Command_Toggle toggle: {
                        // Disable prisoner and medical buttons
                        if (toggle.defaultLabel == "CommandBedSetForPrisonersLabel".Translate() || toggle.defaultLabel == "CommandBedSetAsMedicalLabel".Translate()) gizmo.Disable();
                        break;
                    }
                    case Command_Action action: {
                        // Disable set owner button
                        if (action.defaultLabel == "CommandThingSetOwnerLabel".Translate()) action.Disable();
                        break;
                    }
                }
                yield return gizmo;
            }

            // Gizmo for drawing guest room info
            if (Find.Selector.SingleSelectedObject == this)
            {
                if (Stats.lastCalculated == 0 || Stats.room == null) UpdateStats();
                yield return new Gizmo_GuestBedStats(this);
            }

            // Add buttons to decrease / increase the fee
            yield return new Command_Action
            {
                defaultLabel = "CommandBedDecreaseFeeLabel".Translate(feeStep),
                defaultDesc = "CommandBedDecreaseFeeDesc".Translate(feeStep, MoodEffect),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ChangePriceDown"),
                action = () => AdjustFee(-feeStep),
                hotKey = KeyBindingDefOf.Misc5,
                disabled = rentalFee < feeStep
            };
            yield return new Command_Action
            {
                defaultLabel = "CommandBedIncreaseFeeLabel".Translate(feeStep),
                defaultDesc = "CommandBedIncreaseFeeDesc".Translate(feeStep, MoodEffect),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ChangePriceUp"),
                action = () => AdjustFee(feeStep),
                hotKey = KeyBindingDefOf.Misc6
            };

            // Get base def
            var defName = def.defName.ReplaceFirst("Guest", string.Empty);
            var baseDef = DefDatabase<ThingDef>.GetNamed(defName);

            // Add build copy command
            Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(baseDef, Stuff);
            if (buildCopy != null) yield return buildCopy;
        }

        private void AdjustFee(int amount)
        {
            rentalFee += amount;
            if (rentalFee < 0) rentalFee = 0;
            UpdateStats();
        }

        public override void PostMake()
        {
            base.PostMake();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named("GuestBeds"), KnowledgeAmount.Total);
        }

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;

                var owners = this.Owners();
                if (!owners.Any())
                {
                    GenMapUI.DrawThingLabel(this, rentalFee + silverLabel, defaultThingLabelColor);
                }
                else if (owners.Count == 1)
                {
                    if (owners[0].InBed() && owners[0].CurrentBed() == this) return;
                    GenMapUI.DrawThingLabel(this, owners[0].LabelShort, defaultThingLabelColor);
                }
                else
                {
                    for (int index = 0; index < owners.Count; ++index)
                    {
                        if (!owners[index].InBed() || owners[index].CurrentBed() != this || !(owners[index].Position == GetSleepingSlotPos(index)))
                        {
                            var pos = Traverse.Create(this).Method("GetMultiOwnersLabelScreenPosFor", index).GetValue<Vector3>();
                            GenMapUI.DrawThingLabel(pos, owners[index].LabelShort, defaultThingLabelColor);
                        }
                    }
                }
            }
        }

        public static void Swap(Building_Bed bed)
        {
            bool forPrisoners = bed.ForPrisoners; // Store this, since it changes during spawn

            Building_Bed newBed;
            if (bed.IsGuestBed())
            {
                newBed = (Building_Bed) MakeBed(bed, bed.def.defName.Split(new[] {"Guest"}, StringSplitOptions.RemoveEmptyEntries)[0]);
            }
            else
            {
                newBed = (Building_GuestBed) MakeBed(bed, bed.def.defName + "Guest");
            }

            newBed.SetFactionDirect(bed.Faction);
            var spawnedBed = (Building_Bed) GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation);
            spawnedBed.HitPoints = bed.HitPoints;
            spawnedBed.ForPrisoners = forPrisoners;

            var compQuality = spawnedBed.TryGetComp<CompQuality>();

            compQuality?.SetQuality(bed.GetComp<CompQuality>().Quality, ArtGenerationContext.Outsider);
            //var compArt = bed.TryGetComp<CompArt>();
            //if (compArt != null)
            //{
            //    var art = spawnedBed.GetComp<CompArt>();
            //    Traverse.Create(art).Field("authorNameInt").SetValue(Traverse.Create(compArt).Field("authorNameInt").GetValue());
            //    Traverse.Create(art).Field("titleInt").SetValue(Traverse.Create(compArt).Field("titleInt").GetValue());
            //    Traverse.Create(art).Field("taleRef").SetValue(Traverse.Create(compArt).Field("taleRef").GetValue());
            //
            //    // TODO: Make this work, art is now destroyed
            //}
            Find.Selector.Select(spawnedBed, false);
        }

        private static Thing MakeBed(Building_Bed bed, string defName)
        {
            ThingDef newDef = DefDatabase<ThingDef>.GetNamed(defName);
            return ThingMaker.MakeThing(newDef, bed.Stuff);
        }

        public bool TryClaimBed(Pawn pawn)
        {
            if (!pawn.ownership.ClaimBedIfNonMedical(this)) return false;
            UpdateStats();
            return CompAssignableToPawn.AssignedPawnsForReading.Contains(pawn);
        }

        public class BedStats
        {
            public int lastCalculated;
            public Room room;
            public TaggedString textAttractiveness;
            public TaggedString textFee;
            public TaggedString title;
            public IEnumerable<TaggedString> textAsArray = new TaggedString[0];
            public int staticBedValue;
            public RoyalTitleDef[] metRoyalTitles;
            public TaggedString textNextTitleReq;
        }
    }
}
