using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace  Enhanced_Development.Stargate
{
    class Building_OffWorldStargate : Building
    {

        #region Variables

        private static Texture2D UI_ACTIVATE_GATE;

        public int warned = 0;

        #endregion

        #region Override

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            UI_ACTIVATE_GATE = ContentFinder<Texture2D>.Get("UI/nuke", true);
        }

        //Saving game
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void TickRare()
        {
            base.TickRare();
        }

        #endregion

        #region Commands

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }


            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ActivateGate();
                act.icon = UI_ACTIVATE_GATE;
                act.defaultLabel = "Activate Gate";
                act.defaultDesc = "Activate Gate";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

        /*
        public override IEnumerable<Command> GetCommands()
        {
            IList<Command> CommandList = new List<Command>();
            IEnumerable<Command> baseCommands = base.GetCommands();

            if (baseCommands != null)
            {
                CommandList = baseCommands.ToList();
            }

            if (true)
            {
                //Upgrading
                Command_Action command_Action_AddResources = new Command_Action();

                command_Action_AddResources.defaultLabel = "Activate Gate";

                command_Action_AddResources.icon = UI_ACTIVATE_GATE;
                command_Action_AddResources.defaultDesc = "Activate Gate";

                command_Action_AddResources.activateSound = SoundDef.Named("Click");
                command_Action_AddResources.action = new Action(this.ActivateGate);

                CommandList.Add(command_Action_AddResources);
            }

            return CommandList.AsEnumerable<Command>();
        }
        */
        public void ActivateGate()
        {
            if (warned == 0)
            {
                Messages.Message("Warning this will remove all Colonists from the Map!", MessageSound.SeriousAlert);
                warned += 1;
            }
            else if (warned == 1)
            {
                Messages.Message("Are you really certain you want to do that?", MessageSound.SeriousAlert);
                warned += 1;
            }
            else if (warned >= 2)
            {
                Messages.Message("Ok Fine.", MessageSound.SeriousAlert);

                this.Destroy(DestroyMode.Vanish);
                GenSpawn.Spawn(ThingDef.Named("Stargate"), this.Position);

                foreach (Pawn currentPawn in Find.ListerPawns.ColonistsAndPrisoners.ToList())
                {
                    currentPawn.Destroy(DestroyMode.Vanish);
                }
            }
        }

        #endregion

        #region Graphics-text
        public override string GetInspectString()
        {

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("WARNING: Activating this Gate will remove all Colonists from the Map.");
            stringBuilder.AppendLine("but will create a gate for other teams to arrive from.");

            return stringBuilder.ToString();
        }

        #endregion


    }
}
