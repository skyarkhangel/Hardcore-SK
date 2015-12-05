using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Enhanced_Development.ShieldUtils;

namespace Enhanced_Development.Shields
{
    public class Building_Shield : Building
    {
        #region Variables
        //UI elements
        private static Texture2D UI_DIRECT_ON;
        private static Texture2D UI_DIRECT_OFF;

        private static Texture2D UI_INDIRECT_ON;
        private static Texture2D UI_INDIRECT_OFF;

        private static Texture2D UI_FIRE_ON;
        private static Texture2D UI_FIRE_OFF;

        private static Texture2D UI_INTERCEPT_DROPPOD_ON;
        private static Texture2D UI_INTERCEPT_DROPPOD_OFF;

        private static Texture2D UI_REPAIR_ON;
        private static Texture2D UI_REPAIR_OFF;


        private static Texture2D UI_SHOW_ON;
        private static Texture2D UI_SHOW_OFF;

        public bool flag_direct = true;
        public bool flag_indirect = true;
        public bool flag_fireSupression = true;
        public bool flag_shieldRepairMode = true;
        public bool flag_showVisually = true;
        public bool flag_InterceptDropPod = true;

        //variables that are read in from XML
        public int shieldMaxShieldStrength;
        public int shieldInitialShieldStrength;
        public int shieldShieldRadius;

        public int shieldPowerRequiredCharging;
        public int shieldPowerRequiredSustaining;

        public bool shieldBlockIndirect;
        public bool shieldBlockDirect;
        public bool shieldFireSupression;
        public bool shieldInterceptDropPod;

        public bool shieldStructuralIntegrityMode;

        public int shieldRechargeTickDelay;
        public int shieldRecoverWarmup;

        public float colourRed;
        public float colourGreen;
        public float colourBlue;

        public List<string> SIFBuildings;

        protected ShieldField shieldField;
        CompPowerTrader power;
        //Prepared data
        private ShieldBlendingParticle[] sparksParticle = new ShieldBlendingParticle[3];


        #endregion

        //Dummy override
        public override void PostMake()
        {
            base.PostMake();
        }
        //On spawn, get the power component reference
        public override void SpawnSetup()
        {
            //Setup UI
            UI_DIRECT_OFF = ContentFinder<Texture2D>.Get("UI/DirectOff", true);
            UI_DIRECT_ON = ContentFinder<Texture2D>.Get("UI/DirectOn", true);
            UI_INDIRECT_OFF = ContentFinder<Texture2D>.Get("UI/IndirectOff", true);
            UI_INDIRECT_ON = ContentFinder<Texture2D>.Get("UI/IndirectOn", true);
            UI_FIRE_OFF = ContentFinder<Texture2D>.Get("UI/FireOff", true);
            UI_FIRE_ON = ContentFinder<Texture2D>.Get("UI/FireOn", true);
            UI_INTERCEPT_DROPPOD_OFF = ContentFinder<Texture2D>.Get("UI/FireOff", true);
            UI_INTERCEPT_DROPPOD_ON = ContentFinder<Texture2D>.Get("UI/FireOn", true);

            UI_REPAIR_ON = ContentFinder<Texture2D>.Get("UI/RepairON", true);
            UI_REPAIR_OFF = ContentFinder<Texture2D>.Get("UI/RepairOFF", true);

            UI_SHOW_ON = ContentFinder<Texture2D>.Get("UI/ShieldShowOn", true);
            UI_SHOW_OFF = ContentFinder<Texture2D>.Get("UI/ShieldShowOff", true);


            base.SpawnSetup();
            this.power = base.GetComp<CompPowerTrader>();
            if (def is ShieldBuildingThingDef)
            {
                //Read in variables from the custom MyThingDef
                shieldMaxShieldStrength = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldMaxShieldStrength;
                shieldInitialShieldStrength = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldInitialShieldStrength;
                shieldShieldRadius = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldShieldRadius;

                shieldPowerRequiredCharging = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldPowerRequiredCharging;
                shieldPowerRequiredSustaining = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldPowerRequiredSustaining;

                shieldRechargeTickDelay = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldRechargeTickDelay;
                shieldRecoverWarmup = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldRecoverWarmup;

                shieldBlockIndirect = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldBlockIndirect;
                shieldBlockDirect = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldBlockDirect;
                shieldFireSupression = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldFireSupression;
                shieldInterceptDropPod = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldInterceptDropPod;

                shieldStructuralIntegrityMode = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).shieldStructuralIntegrityMode;

                colourRed = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).colourRed;
                colourGreen = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).colourGreen;
                colourBlue = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).colourBlue;

                SIFBuildings = ((Enhanced_Development.Shields.ShieldBuildingThingDef)def).SIFBuildings;
                //Log.Error("Count:" + SIFBuildings.Count);
            }
            else
            {
                Log.Error("Shield definition not of type \"ShieldBuildingThingDef\"");
            }

            if (shieldField == null)
            {
                this.SpawnSetup_ShieldField();
            }
        }

        public virtual void SpawnSetup_ShieldField()
        {

            shieldField = new ShieldField(this, base.Position, shieldMaxShieldStrength, shieldInitialShieldStrength, shieldShieldRadius, shieldRechargeTickDelay, shieldRecoverWarmup, shieldBlockIndirect, shieldBlockDirect, shieldFireSupression, shieldInterceptDropPod, shieldStructuralIntegrityMode, colourRed, colourGreen, colourBlue);
        }


        public override void Tick()
        {
            base.Tick();

            Boolean _PowerAvalable = false;

            //Check to prevent NullPointerExceptions
            if (shieldField != null)
            {
                //Disable shield when power goes off
                if (this.power != null)
                {
                    if (this.power.PowerOn)
                    {
                        _PowerAvalable = true;
                    }

                }
                //Do tick for the shield field
                shieldField.ShieldTick(_PowerAvalable, this.flag_direct, this.flag_indirect, this.flag_fireSupression, this.flag_InterceptDropPod, this.flag_shieldRepairMode);

                //Change power requirements depending on shield status
                switch (shieldField.status)
                {
                    //Disabled shield also requires power (to avoid flickering when thing increases power requirements because it gained power...)
                    case enumShieldStatus.Disabled:
                    case enumShieldStatus.Loading:
                        {
                            this.power.powerOutputInt = shieldPowerRequiredCharging;
                            break;
                        }
                    case enumShieldStatus.Charging:
                        {
                            this.power.powerOutputInt = shieldPowerRequiredCharging;
                            break;
                        }
                    case enumShieldStatus.Sustaining:
                        {
                            this.power.powerOutputInt = shieldPowerRequiredSustaining;
                            break;
                        }
                }
            }
            else
            {
                this.SpawnSetup_ShieldField();
            }

        }

        /// <summary>
        /// Draw the shield Field
        /// </summary>
        public void DrawShieldField()
        {
            if (shieldField.isOnline() || shieldField.shieldRecoverWarmup - shieldField.warmupPower < 60)
            {
                //Draw field
                shieldField.DrawField(Vectors.IntVecToVec(base.Position));

                //Initialize the spark particle array
                if (sparksParticle[0] == null)
                {
                    for (int i = 0; i < sparksParticle.Length; i++)
                    {
                        sparksParticle[i] = new ShieldBlendingParticle(this.DrawPos, (int)Math.Round(((float)i / (float)(sparksParticle.Length - 1)) * (float)ShieldBlendingParticle.transitionMax));
                    }
                }
                //Animate spark particles
                for (int i = 0, l = sparksParticle.Length; i < l; i++)
                {
                    sparksParticle[i].DrawMe();
                }

            }
        }

        public override void Draw()
        {
            base.Draw();
            if (flag_showVisually)
            {
                DrawShieldField();
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(base.Position, shieldField.shieldShieldRadius);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(shieldField.GetInspectString());

            if (power != null)
            {
                string text = power.CompInspectStringExtra();
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

            Scribe_Deep.LookDeep(ref shieldField, "shieldField");

            shieldField.position = base.Position;

            Scribe_Values.LookValue(ref flag_direct, "flag_direct");
            Scribe_Values.LookValue(ref flag_indirect, "flag_indirect");
            Scribe_Values.LookValue(ref flag_fireSupression, "flag_fireSupression");
            Scribe_Values.LookValue(ref flag_InterceptDropPod, "flag_InterceptDropPod");

            Scribe_Values.LookValue(ref flag_shieldRepairMode, "flag_shieldRepairMode");
            Scribe_Values.LookValue(ref flag_showVisually, "flag_showVisually");

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (shieldBlockDirect)
            {
                if (flag_direct)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchDirect();
                    act.icon = UI_DIRECT_ON;
                    act.defaultLabel = "Block Direct";
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
                    act.action = () => this.SwitchDirect();
                    act.icon = UI_DIRECT_OFF;
                    act.defaultLabel = "Block Direct";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (shieldBlockIndirect)
            {
                if (flag_indirect)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchIndirect();
                    act.icon = UI_INDIRECT_ON;
                    act.defaultLabel = "Block Indirect";
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
                    act.action = () => this.SwitchIndirect();
                    act.icon = UI_INDIRECT_OFF;
                    act.defaultLabel = "Block Indirect";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (shieldFireSupression)
            {
                if (flag_fireSupression)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchFire();
                    act.icon = UI_INDIRECT_ON;
                    act.defaultLabel = "Fire Suppression";
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
                    act.action = () => this.SwitchFire();
                    act.icon = UI_INDIRECT_OFF;
                    act.defaultLabel = "Fire Suppression";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (shieldInterceptDropPod)
            {
                if (flag_InterceptDropPod)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchInterceptDropPod();
                    act.icon = UI_INTERCEPT_DROPPOD_ON;
                    act.defaultLabel = "Intercept DropPod";
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
                    act.action = () => this.SwitchInterceptDropPod();
                    act.icon = UI_INTERCEPT_DROPPOD_OFF;
                    act.defaultLabel = "Intercept DropPod";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }


            if (shieldStructuralIntegrityMode)
            {
                if (flag_shieldRepairMode)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchShieldRepairMode();
                    act.icon = UI_REPAIR_ON;
                    act.defaultLabel = "Repair Mode";
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
                    act.action = () => this.SwitchShieldRepairMode();
                    act.icon = UI_REPAIR_OFF;
                    act.defaultLabel = "Repair Mode";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }


            if (true)
            {
                if (flag_showVisually)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchVisual();
                    act.icon = UI_SHOW_ON;
                    act.defaultLabel = "Show Visually";
                    act.defaultDesc = "Show";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchVisual();
                    act.icon = UI_SHOW_OFF;
                    act.defaultLabel = "Show Visually";
                    act.defaultDesc = "Hide";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }


        }

        private void SwitchDirect()
        {
            flag_direct = !flag_direct;
        }

        private void SwitchIndirect()
        {
            flag_indirect = !flag_indirect;
        }

        private void SwitchFire()
        {
            flag_fireSupression = !flag_fireSupression;
        }

        private void SwitchInterceptDropPod()
        {
            flag_InterceptDropPod = !flag_InterceptDropPod;
        }

        private void SwitchVisual()
        {
            flag_showVisually = !flag_showVisually;
        }

        private void SwitchShieldRepairMode()
        {
            flag_shieldRepairMode = !flag_shieldRepairMode;
        }

    }
}