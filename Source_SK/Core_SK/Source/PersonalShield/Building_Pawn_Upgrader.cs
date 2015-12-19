using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Core_SK.PersonalShields.Nano;

namespace Core_SK.PersonalShields
{
    public class Building_Pawn_Upgrader : Building
    {
        #region Variables

        public float MAX_DISTANCE = 2.0f;
        public bool flag_charge = false;
        CompPowerTrader power;
        //NanoConnector nanoConnector;

        private static Texture2D UI_UPGRADE;
        private static Texture2D UI_CHARGE_OFF;
        private static Texture2D UI_CHARGE_ON;

        #endregion

        //Dummy override
        public override void PostMake()
        {
            base.PostMake();
        }
        //On spawn, get the power component reference
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.power = base.GetComp<CompPowerTrader>();

            UI_UPGRADE = ContentFinder<Texture2D>.Get("UI/Upgrade", true);
            UI_CHARGE_OFF = ContentFinder<Texture2D>.Get("UI/ChargeOFF", true);
            UI_CHARGE_ON = ContentFinder<Texture2D>.Get("UI/ChargeON", true);
        }

        public override void Tick()
        {
            //Log.Message("Tick");
            base.Tick();

            if (this.power.PowerOn == true)
            {
                NanoManager.tick();
                if (this.flag_charge)
                {
                    this.rechargePawns();
                }
            }
        }


        public override void Draw()
        {
            base.Draw();
        }

        public override void DrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(base.Position, this.MAX_DISTANCE);
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());
            ///stringBuilder.Append(shieldField.GetInspectString());

            /*
            for (int i = 0, l = sparksParticle.Length; i < l; i++)
            {
                stringBuilder.AppendLine("   " + (i + 1) + ". " + sparksParticle[i].currentDir + " -> " + sparksParticle[i].currentStep);
            }*/

            string text;

            text = "Nano Charge: " + NanoManager.getCurrentCharge() + " / " + NanoManager.getMaxCharge();
            stringBuilder.AppendLine(text);

            if (power != null)
            {
                text = power.CompInspectStringExtra();
                if (!text.NullOrEmpty())
                {
                    stringBuilder.AppendLine(text);
                }
            }


            return stringBuilder.ToString();
        }
        //Saving game
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref flag_charge, "flag_charge");
            Scribe_Values.LookValue(ref NanoManager.currentCharge, "currentCharge");
            /*
            Scribe_Deep.LookDeep(ref shieldField, "shieldField");

            shieldField.position = base.Position;

            Scribe_Values.LookValue(ref flag_direct, "flag_direct");
            Scribe_Values.LookValue(ref flag_indirect, "flag_indirect");
            Scribe_Values.LookValue(ref flag_fireSupression, "flag_fireSupression");
            Scribe_Values.LookValue(ref flag_shieldRepairMode, "flag_shieldRepairMode");*/
        }

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
                act.action = () => this.tryReplacePawn();
                act.icon = UI_UPGRADE;
                act.defaultLabel = "Upgrade To NanoShield";
                act.defaultDesc = "Upgrade To NanoShield";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

            if (flag_charge)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.SwitchCharge();
                act.icon = UI_CHARGE_ON;
                act.defaultLabel = "Charge Shields";
                act.defaultDesc = "On";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
            else
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.SwitchCharge();
                act.icon = UI_CHARGE_OFF;
                act.defaultLabel = "Charge Shields";
                act.defaultDesc = "Off";
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
                Command_Action command_Action_InstallShield = new Command_Action();

                command_Action_InstallShield.defaultLabel = "Upgrade To NanoShield";

                command_Action_InstallShield.icon = UI_UPGRADE;
                command_Action_InstallShield.defaultDesc = "Upgrade To NanoShield";

                command_Action_InstallShield.activateSound = SoundDef.Named("Click");
                command_Action_InstallShield.action = new Action(this.tryReplacePawn);

                CommandList.Add(command_Action_InstallShield);
            }

            //Charging
            if (true)
            {
                //switchDirect
                Command_Action command_Action_switchCharge = new Command_Action();

                command_Action_switchCharge.defaultLabel = "Charge Shields";
                if (flag_charge)
                {
                    command_Action_switchCharge.icon = UI_CHARGE_ON;
                    command_Action_switchCharge.defaultDesc = "On";

                }
                else
                {
                    command_Action_switchCharge.icon = UI_CHARGE_OFF;
                    command_Action_switchCharge.defaultDesc = "Off";
                }

                command_Action_switchCharge.activateSound = SoundDef.Named("Click");
                command_Action_switchCharge.action = new Action(this.SwitchCharge);

                CommandList.Add(command_Action_switchCharge);
            }


            return CommandList.AsEnumerable<Command>();
        }
        */
        private void SwitchCharge()
        {
            flag_charge = !flag_charge;
        }

        private void tryReplacePawn()
        {
            if (this.power.PowerOn == true)
            {
                this.upgradePawns();
            }
        }

        private bool upgradePawns()
        {
            IEnumerable<Pawn> closePawns = Core_SK.Utilities.Utilities.findPawnsInColony(this.Position, this.MAX_DISTANCE);

            if (closePawns != null)
            {
                foreach (Pawn currentPawn in closePawns.ToList())
                {
                    if (currentPawn.apparel != null)
                    {
                        ThingDef personalShieldDef = ThingDef.Named("Apparel_PersonalNanoShield");

                        ThingDef stuff = GenStuff.RandomStuffFor(personalShieldDef);
                        Thing personalShield = ThingMaker.MakeThing(personalShieldDef, stuff);
                        currentPawn.apparel.Wear((Apparel)personalShield);

                    }
                    else if (currentPawn.GetType() == typeof(Core_SK.PersonalShields.Animal.ShieldPawn))
                    {
                        Core_SK.PersonalShields.Animal.ShieldPawn currentShieldPawn;
                        currentShieldPawn = (Core_SK.PersonalShields.Animal.ShieldPawn)currentPawn;

                        if (currentShieldPawn.ShieldState == Animal.ShieldStatePawn.Inactive)
                        {
                            currentShieldPawn.recharge(1);
                        }
                    }
                }
            }

            return false;
        }

        public void rechargePawns()
        {
            int currentTick = Find.TickManager.TicksGame;
            //Only every 10 ticks
            if (currentTick % 10 == 0)
            {

                IEnumerable<Pawn> pawns = Find.ListerPawns.ColonistsAndPrisoners;

                if (pawns != null)
                {                    
                    IEnumerable<Pawn> closePawns = Core_SK.Utilities.Utilities.findPawnsInColony(this.Position, this.MAX_DISTANCE);

                    if (closePawns != null)
                    {
                        foreach (Pawn currentPawn in closePawns.ToList())
                        {
                            if (currentPawn.apparel != null)
                            {
                                List<RimWorld.Apparel> currentInventory = currentPawn.apparel.WornApparel;

                                foreach (Thing currentThing in currentInventory)
                                {
                                    if (currentThing.def.defName == "Apparel_PersonalNanoShield")
                                    {
                                        //Log.Message("Found:" + currentThing.def.defName);
                                        Apparel_PersonalNanoShield currentShield = (Apparel_PersonalNanoShield)currentThing;

                                        if (!currentShield.isCharged())
                                        {
                                            //currentShield.Energy += 10.0f;

                                            int chargeAmmount = 1;

                                            if (NanoManager.requestCharge(chargeAmmount))
                                            {
                                                currentShield.recharge(chargeAmmount);
                                            }
                                        }
                                    }
                                }
                            }
                            
                            if (currentPawn.GetType() == typeof(Core_SK.PersonalShields.Animal.ShieldPawn))
                            {
                                Core_SK.PersonalShields.Animal.ShieldPawn currentShieldPawn;
                                currentShieldPawn = (Core_SK.PersonalShields.Animal.ShieldPawn)currentPawn;

                                if (!currentShieldPawn.isCharged())
                                {
                                    int chargeAmmount = 1;

                                    if (NanoManager.requestCharge(chargeAmmount))
                                    {
                                        currentShieldPawn.recharge(chargeAmmount);
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

    }
}