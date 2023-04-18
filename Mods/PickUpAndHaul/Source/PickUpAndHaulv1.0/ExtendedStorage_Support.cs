using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace PickUpAndHaul
{
    class ExtendedStorage_Support
    {
        public static bool CapacityAt(Thing thing, IntVec3 storeCell, Map map, out int capacity)
        {
            if (ModCompatibilityCheck.ExtendedStorageIsActive)
            {
                try
                {
                    return CapacityAtEx(thing.def, storeCell, map, out capacity);
                }
                catch (Exception e)
                {
                    Verse.Log.Warning($"Pick Up And Haul tried to get Extended Storage capacity but it failed: {e}");
                }
            }
            capacity = 0;
            return false;
        }

        public static bool CapacityAtEx(ThingDef def, IntVec3 storeCell, Map map, out int capacity)
        {
            foreach (Thing thing in storeCell.GetThingList(map))
            {
                //thing.Position seems to be the input cell, which should be clear if the storage is not near capacity
                //so the game will choose that as best storage, so let's add capacity to that cell
                if (thing.Position != storeCell)
                {
                    continue;
                }

                if (!(thing is ExtendedStorage.Building_ExtendedStorage storage))
                {
                    continue;
                }

                if (storage.StoredThingTotal == 0)
                {
                    capacity = (int)(def.stackLimit * storage.GetStatValue(ExtendedStorage.DefReferences.Stat_ES_StorageFactor));
                }
                else
                {
                    capacity = storage.ApparentMaxStorage - storage.StoredThingTotal;
                }

                Log.Message($"AT {storeCell} ES: {capacity} = {storage.ApparentMaxStorage} - {storage.StoredThingTotal}");
                return true;
            }
            capacity = 0;
            return false;
        }

        public static bool StackableAt(ThingDef def, IntVec3 storeCell, Map map)
        {
            if (ModCompatibilityCheck.ExtendedStorageIsActive)
            {
                try
                {
                    return StackableAtEx(def, storeCell, map);
                }
                catch (Exception e)
                {
                    Verse.Log.Warning($"Pick Up And Haul tried to get find Extended Storage building but it failed: {e}");
                }
            }
            return false;
        }

        public static bool StackableAtEx(ThingDef def, IntVec3 storeCell, Map map)
        {
            Log.Message($"StackableAtEx {def}@{storeCell}, {storeCell.GetFirstThing<ExtendedStorage.Building_ExtendedStorage>(map) != null}");
            return storeCell.GetFirstThing<ExtendedStorage.Building_ExtendedStorage>(map) != null;
                // && storage.StoredThingDef == def //StoredThingDef is internal? okay then
        }
    }
}
