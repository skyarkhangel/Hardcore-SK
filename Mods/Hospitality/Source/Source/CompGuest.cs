using System.Collections.Generic;
using System.Linq;
using Hospitality.Utilities;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospitality
{
    public class CompGuest : ThingComp
    {
        public List<int> boughtItems = new List<int>();

        public bool entertain;
        public bool makeFriends;
        public bool WillOnlyJoinByForce => Pawn.WillOnlyJoinByForce();

        public bool arrived;
        public bool sentAway;
        public bool rescued;
        public bool wasDowned;

        public Lord lord;

        public readonly Dictionary<Pawn, int> failedCharms = new Dictionary<Pawn, int>();

        private Area guestArea_int;
        private Area shoppingArea_int;

        public Building_GuestBed bed;
        public int lastBedCheckTick;
        public int lastFoodCheckTick;

        private bool postLoaded;

        public void ResetForGuest(Lord lord)
        {
            boughtItems.Clear();
            arrived = false;
            sentAway = false;
            failedCharms.Clear();
            this.lord = lord;
            Pawn.ownership.UnclaimBed();
        }

        private Pawn Pawn => (Pawn) parent;

        public bool HasBed => bed != null && bed.Spawned && bed.Owners().Contains(Pawn);
        

        public Area GuestArea
        {
            get
            {
                if (guestArea_int != null && guestArea_int.Map != Pawn.Map) return null;
                return guestArea_int;
            }
            set => guestArea_int = value;
        }

        public Area ShoppingArea
        {
            get
            {
                if (shoppingArea_int != null && shoppingArea_int.Map != Pawn.Map) return null;
                return shoppingArea_int;
            }
            set => shoppingArea_int = value;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref arrived, "arrived");
            Scribe_Values.Look(ref rescued, "rescued");
            Scribe_Values.Look(ref wasDowned, "wasDowned");
            Scribe_Values.Look(ref entertain, "chat");
            Scribe_Values.Look(ref makeFriends, "recruit");
            Scribe_Collections.Look(ref boughtItems, "boughtItems", LookMode.Value);
            Scribe_References.Look(ref guestArea_int, "guestArea");
            Scribe_References.Look(ref shoppingArea_int, "shoppingArea");
            Scribe_References.Look(ref bed, "bed");
            boughtItems ??= new List<int>();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (Pawn.Dead) return;

                // NOTE: Careful! All pawns have this component

                // Can't save lord (IExposable), so we just gotta find it each time
                lord = Pawn.GetLord();

                // Bed doesn't store owners
                if (bed != null && !bed.Owners().Contains(Pawn))
                {
                    // Log.Message($"Assigned {Pawn.NameShortColored} to bed {bed.Position}.");
                    bed.CompAssignableToPawn.TryAssignPawn(Pawn);
                }

                if (bed != null && Pawn.ownership.OwnedBed != bed)
                {
                    // Log.Message($"Assigned bed {bed.Position} to {Pawn.NameShortColored}.");
                    Pawn.ownership.intOwnedBed = bed;
                }
                postLoaded = true;
            }
        }

        /// <summary>
        /// Only call from Pawn_Ownership_Patch!
        /// </summary>
        internal void ClearOwnership()
        {
            // Pawn_Ownership will try to unclaim the bed if it postLoads before this component
            if (!postLoaded) return;

            // Calling this method directly crashes the game (infinite loop, somehow). So here's a copy.
            void TryUnassignPawn(CompAssignableToPawn comp)
            {
                var assignedPawns = comp.assignedPawns;
                if (!assignedPawns.Contains(Pawn)) return;
                assignedPawns.Remove(Pawn);
                comp.SortAssignedPawns();
            }

            if (bed?.CompAssignableToPawn != null)
            {
                TryUnassignPawn(bed?.CompAssignableToPawn);
            }
            bed = null;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            lord = Pawn.GetLord();
        }

        public void Arrive()
        {
            arrived = true;
            lastBedCheckTick = GenTicks.TicksGame; // Don't check right away
        }

        public void Leave(bool clearLord)
        {
            arrived = false;
            Pawn.ownership.UnclaimBed();
            
            if(clearLord) lord = null;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            map.GetMapComponent()?.PresentGuests.Remove(Pawn);
        }

        public void ClaimBed([NotNull]Building_GuestBed newBed)
        {
            if (!newBed.AnyUnownedSleepingSlot) return;

            var allOtherBeds = newBed.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(b => b != null && b != newBed);

            foreach (var otherBed in allOtherBeds)
            {
                if (otherBed.Owners().Contains(Pawn)) Log.Warning($"{Pawn.LabelShort} already owns {otherBed.Label}!");
            }

            Pawn.ownership.UnclaimBed();

            if(newBed.TryClaimBed(Pawn))
            {
                bed = newBed;
                //Log.Message($"{Pawn.LabelShort} proudly claims {newBed.Label}! bed: {bed.Owners().Select(p=>p?.Name.ToStringShort).ToCommaList()} pawn: {Pawn.ownership.OwnedBed?.Position}");
            }
        }
    }
}