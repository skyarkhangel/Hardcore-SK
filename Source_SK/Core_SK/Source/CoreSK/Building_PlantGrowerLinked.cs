using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound
using RimWorld;      // RimWorld specific functions are found here
//using RimWorld.SquadAI;

namespace HydroponicRoom
{
    /// <summary>
    /// Building_PlantGrowerLinked class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_PlantGrowerLinked : Building_PlantGrower
    {
        // Gizmo textures.
        protected static Texture2D applyToWholeRoomGizmoIcon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings");

        // ######## Gizmos ######## //

        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000300;

            List<Gizmo> gizmoList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                gizmoList.Add(gizmo);
            }
            if (this.Faction != Faction.OfColony)
            {
                return gizmoList;
            }

            Command_Action applyPlantDefToWholeRoomGizmo = new Command_Action();
            applyPlantDefToWholeRoomGizmo.icon = applyToWholeRoomGizmoIcon;
            applyPlantDefToWholeRoomGizmo.defaultDesc = "PGLDesc".Translate();
            applyPlantDefToWholeRoomGizmo.defaultLabel = "PGLLabel".Translate();
            applyPlantDefToWholeRoomGizmo.activateSound = SoundDef.Named("Click");
            applyPlantDefToWholeRoomGizmo.action = new Action(ApplyPlantDefToWholeRoom);
            applyPlantDefToWholeRoomGizmo.groupKey = groupKeyBase + 1;
            gizmoList.Add(applyPlantDefToWholeRoomGizmo);

            return gizmoList;
        }

        public void ApplyPlantDefToWholeRoom()
        {
            Room room = this.Position.GetRoom();
            if (room == null)
            {
                // Should not occur.
                return;
            }

            ThingDef plantDef = this.GetPlantDefToGrow();
            List<Thing> thingsInRoom = room.AllContainedThings;
            foreach (Thing thing in thingsInRoom)
            {
                if (thing == this)
                {
                    continue;
                }
                if (thing.def == ThingDef.Named("HydroponicsBasin"))
                {
                    (thing as Building_PlantGrower).SetPlantDefToGrow(plantDef);
                }
                if (thing.def == ThingDef.Named("NewHydroponicsBasin"))
                {
                    (thing as Building_PlantGrower).SetPlantDefToGrow(plantDef);
                }
                if (thing.def == ThingDef.Named("ClutterAlloyHydroponicsBasinVS"))
                {
                    (thing as Building_PlantGrower).SetPlantDefToGrow(plantDef);
                }
            }
        }
    }
}
