using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; // Always needed
using RimWorld; // Needed
using Verse; // Needed
using Verse.AI; // Needed when you do something with the AI
using Verse.Sound; // Needed when you do something with the Sound

using CommonMisc;

namespace PowerSwitch
{
    public class Building_PowerSwitchMod : Building_PowerSwitch
    {


        #region Variables

        //Init flag
        private bool init = false;
        
        private CompFlickable flickableComp;


        private bool autoSwitchOnEnemyActive = false;
        private bool autoSwitchOffEnemyActive = false;
        private bool autoSwitchOnPawnActive = false;

        private bool switchDirectly;

        private const int ticksRequestSwitchOff = 300;
        private const int ticksRequestSwitchOn = 150;

        private int ticksSwitchOff = -1;
        private int ticksSwitchOn = -1;

        private int ticksNoUpdate;

        // work variables
        private int enemySearchCount = 0;
        private int enemySearchCountMax = 180; //search every 3s

        private float radarDistance = Find.Map.info.Size.x / 3.33f; // the search radius for enemies (MapSize.x / 3.33)

        private int pawnSearchCount = 0;
        private int pawnSearchCountMax = 60; //search every 1s

        private float radarDistancePawn = 7f; // the search radius for nearby pawns (7)

        private bool pawnSearchModeDistanceActive = false;

        private bool autoSwitchTimerActive = false;
        private int autoSwitchTimerOnTime = 19;
        private int autoSwitchTimerOffTime = 5;

        // Get textures and save them
        private static Texture2D texUI_Power;
        private static Texture2D texUI_NoPower;
        private static Texture2D texUI_PowerPawnOn;
        private static Texture2D texUI_NoPowerPawnOn;
        private static Texture2D texUI_PowerEnemyOn;
        private static Texture2D texUI_NoPowerEnemyOn;
        private static Texture2D texUI_PowerEnemyOff;
        private static Texture2D texUI_NoPowerEnemyOff;
        private static Texture2D texUI_TimerOn;
        private static Texture2D texUI_NoTimerOn;
        private static Texture2D texUI_TimerOff;
        private static Texture2D texUI_NoTimerOff;

        // Text variables
        private string txtSwitchOnOff = "Switch: On/Off.";
        private string txtAutoOnMotionRoom = "Auto ON: Motion in room.";
        private string txtAutoOnMotionNearbyRange = "Auto ON: Motion nearby. Range:";
        private string txtAutoOnEnemyNearbyRange = "Auto ON: Enemy nearby. Range:";
        private string txtAutoOffEnemyNearbyRange = "Auto OFF: Enemy nearby. Range:";
        private string txtTimerClickSetOnTime = "Timer: Click to set switch-on time.";
        private string txtTimerClickSetOffTime = "Timer: Click to set switch-off time.";
        private string txtPower = "Power:";
        private string txtOn = "On";
        private string txtOff = "Off";
        private string txtOnRange = "On - Range:";
        private string txtAutoMotion = "Auto Motion:";
        private string txtAutoEnemy = "Auto Enemy:";
        private string txtTimer = "Timer";
        private string txtTimerFormat = "{0:00}h - {1:00}h";


        private void GetTextures()
        {
            // Get textures and save them
            texUI_Power = ContentFinder<Texture2D>.Get("UI/Commands/UI_Power", true);
            texUI_NoPower = ContentFinder<Texture2D>.Get("UI/Commands/UI_NoPower", true);
            texUI_PowerPawnOn = ContentFinder<Texture2D>.Get("UI/Commands/UI_PowerPawnOn", true);
            texUI_NoPowerPawnOn = ContentFinder<Texture2D>.Get("UI/Commands/UI_NoPowerPawnOn", true);
            texUI_PowerEnemyOn = ContentFinder<Texture2D>.Get("UI/Commands/UI_PowerEnemyOn", true);
            texUI_NoPowerEnemyOn = ContentFinder<Texture2D>.Get("UI/Commands/UI_NoPowerEnemyOn", true);
            texUI_PowerEnemyOff = ContentFinder<Texture2D>.Get("UI/Commands/UI_PowerEnemyOff", true);
            texUI_NoPowerEnemyOff = ContentFinder<Texture2D>.Get("UI/Commands/UI_NoPowerEnemyOff", true);
            texUI_TimerOn = ContentFinder<Texture2D>.Get("UI/Commands/UI_TimerOn", true);
            texUI_NoTimerOn = ContentFinder<Texture2D>.Get("UI/Commands/UI_NoTimerOn", true);
            texUI_TimerOff = ContentFinder<Texture2D>.Get("UI/Commands/UI_TimerOff", true);
            texUI_NoTimerOff = ContentFinder<Texture2D>.Get("UI/Commands/UI_NoTimerOff", true);
        }

        #endregion


        #region Setup

        /// <summary>
        /// Do something after the object is spawned
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            // Translations
            txtSwitchOnOff = "PowerSwitch_SwitchOnOff".Translate(); // "Switch: On/Off.";
            txtAutoOnMotionRoom = "PowerSwitch_AutoONMotionRoom".Translate(); // "Auto ON: Motion in room.";
            txtAutoOnMotionNearbyRange = "PowerSwitch_AutoONMotionNearbyRange".Translate(); // "Auto ON: Motion nearby. Range:";
            txtAutoOnEnemyNearbyRange = "PowerSwitch_AutoONEnemyNearbyRange".Translate(); // "Auto ON: Enemy nearby. Range:";
            txtAutoOffEnemyNearbyRange = "PowerSwitch_AutoOFFEnemyNearbyRange".Translate(); // "Auto OFF: Enemy nearby. Range:";
            txtTimerClickSetOnTime = "PowerSwitch_TimerClickSwitchOnTime".Translate(); // "Timer: Click to set switch-on time.";
            txtTimerClickSetOffTime = "PowerSwitch_TimerClickSwitchOffTime".Translate(); // "Timer: Click to set switch-off time.";
            txtPower = "PowerSwitch_Power".Translate(); // "Power:";
            txtOn = "PowerSwitch_On".Translate(); // "On";
            txtOff = "PowerSwitch_Off".Translate(); // "Off";
            txtOnRange = "PowerSwitch_OnRange".Translate(); // "On - Range:";
            txtAutoMotion = "PowerSwitch_AutoMotion".Translate(); // "Auto Motion:";
            txtAutoEnemy = "PowerSwitch_AutoEnemy".Translate(); // "Auto Enemy:";
            txtTimer = "PowerSwitch_Timer".Translate(); // "Timer";
            txtTimerFormat = "PowerSwitch_TimeOnTimeOff".Translate(); // "{0:00}h - {1:00}h";

            GetTextures();

            DestroyOtherPowerTransmitter();

            //// Wall: can't have this deactive!
            //if (isWall)
            //    pawnSearchModeDistanceActive = true;


            this.flickableComp = base.GetComp<CompFlickable>();

            // Initialized
            init = true;

        }


        /// <summary>
        /// Do something after the object is initialized, but before it is spawned
        /// </summary>
        public override void PostMake()
        {
            base.PostMake();
        }

        /// <summary>
        /// This writes the status variables to and from the savegame
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            //Scribe_Values.LookValue(ref transmitsPower, "transmitsPowerActive");
            Scribe_Values.LookValue(ref autoSwitchOnPawnActive, "autoSwitchOnPawnActive");
            Scribe_Values.LookValue(ref autoSwitchOnEnemyActive, "autoSwitchOnEnemyActive");
            Scribe_Values.LookValue(ref autoSwitchOffEnemyActive, "autoSwitchOffEnemyActive");
            Scribe_Values.LookValue(ref autoSwitchTimerActive, "autoSwitchTimerActive");
            Scribe_Values.LookValue(ref autoSwitchTimerOnTime, "autoSwitchTimerOnTime");
            Scribe_Values.LookValue(ref autoSwitchTimerOffTime, "autoSwitchTimerOffTime");
            Scribe_Values.LookValue(ref pawnSearchModeDistanceActive, "pawnSearchModeDistanceActive");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.flickableComp == null)
                {
                    this.flickableComp = base.GetComp<CompFlickable>();
                }
            }

            // Initialized
            init = true;
        }

        #endregion



        #region Ticker

        /// <summary>
        /// This is used, when the Ticker is changed from Normal to Rare
        /// This is a tick thats done once every 5s = 3000Ticks
        /// </summary>
        public override void TickRare()
        {
            //base.TickRare(); //Base doesn't have a TickRare() Except an exception throw!

            // Do nothing further if it isn't initialized yet
            if (!init)
                return;
        }


        /// <summary>
        /// This is used, when the Ticker is set to Normal
        /// This Tick is done often (60 times per second)
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            // Do nothing further if it isn't initialized yet
            if (!init)
                return;

            // Update ticks for timed switching
            UpdateTimedSwitch();

            // If updated, no other update until counter reach value
            if (ticksNoUpdate > 0)
            {
                ticksNoUpdate--;
                return;
            }


            // Search for enemies within reach..
            if ((autoSwitchOnEnemyActive) || (autoSwitchOffEnemyActive))
            {
                // Enemy search active >> Search every x Ticks
                enemySearchCount += 1;
                if (enemySearchCount >= enemySearchCountMax)
                {
                    enemySearchCount = 0;
                    bool found = SearchForEnemy();

                    // Power on when enemy in range!
                    if (autoSwitchOnEnemyActive)
                    {
                        // Switch on undelayed
                        if (found && (!flickableComp.SwitchIsOn || ticksSwitchOff >= 0))
                        {
                            ResetTimedSwitch();
                            //SwitchOn = true;
                            SetWantSwitchOn(true);
                        }
                        // Switch off delayed
                        if (!found && flickableComp.SwitchIsOn && (ticksSwitchOff < 0))
                        {
                            ResetTimedSwitch();
                            SetTimedSwitch(false, true);
                        }
                    }

                    // Power off when enemy in range!
                    if (autoSwitchOffEnemyActive)
                    {
                        // Switch off undelayed
                        if (found && (flickableComp.SwitchIsOn || ticksSwitchOn >= 0))
                        {
                            ResetTimedSwitch();
                            //SwitchOn = false;
                            SetWantSwitchOn(false);
                        }
                        // Switch on delayed
                        if (!found && !flickableComp.SwitchIsOn && (ticksSwitchOn < 0))
                        {
                            ResetTimedSwitch();
                            SetTimedSwitch(true, false);
                        }
                    }

                }
            }
            if ((!autoSwitchOnEnemyActive) && (!autoSwitchOffEnemyActive))
            {
                // Not active >> Reset counter
                enemySearchCount = 0;
            }


            // Search for pawns in the room..
            if (autoSwitchOnPawnActive)
            {
                // Pawn search active >> Search every x Ticks
                pawnSearchCount += 1;
                if (pawnSearchCount >= pawnSearchCountMax)
                {
                    pawnSearchCount = 0;
                    bool found = SearchForPawnInRoom();

                    // Power is ON >> switch it off, when there is no pawn in the room!
                    // Switch on - undelayed
                    if (found && (!flickableComp.SwitchIsOn || ticksSwitchOff >= 0))
                    {
                        ResetTimedSwitch();
                        flickableComp.SwitchIsOn = true; // No designator usage!
                        SetWantSwitchOn(true);
                    }
                    // Switch off - delayed
                    if (!found && flickableComp.SwitchIsOn && (ticksSwitchOff < 0))
                    {
                        ResetTimedSwitch();
                        SetTimedSwitch(false, true, true); // No designator usage!
                    }
                }
            }
            else
            {
                // Not active >> Reset counter
                pawnSearchCount = 0;
            }


            // Timer switching
            if (autoSwitchTimerActive)
            {
                // Time = Off-Time => Switch OFF
                if (GenDate.HourInt == autoSwitchTimerOffTime && flickableComp.SwitchIsOn)
                {
                    flickableComp.SwitchIsOn = false; // No designator usage!
                    SetWantSwitchOn(false);
                }

                // Time = On-Time => Switch ON
                if (GenDate.HourInt == autoSwitchTimerOnTime && !flickableComp.SwitchIsOn)
                {
                    flickableComp.SwitchIsOn = true; // No designator usage!
                    SetWantSwitchOn(true);
                }
            }

        }

        #endregion


        #region Inspections

        /// <summary>
        /// This creates new selection buttons with a new graphic
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> baseGizmos = base.GetGizmos().ToList();
            for (int i = 0; i < baseGizmos.Count; i++)
            {
                Command baseGizmo = baseGizmos[i] as Command;
                if (baseGizmo != null && baseGizmo.defaultLabel == "CommandTogglePowerLabel".Translate())
                {
                    // Suppress base power on/off switch, as I need the functionality to disable the other functions
                    continue;
                }

                yield return baseGizmos[i];
            }


            // Key-Binding F - Manual On/Off
            Command_Action optF;
            optF = new Command_Action();
            if (flickableComp.SwitchIsOn)
                optF.icon = texUI_Power;
            else
                optF.icon = texUI_NoPower;
            optF.defaultLabel = "CommandTogglePowerLabel".Translate();
            optF.defaultDesc = "CommandTogglePowerDesc".Translate();
            optF.hotKey = KeyBindingDefOf.Misc1; //KeyCode.F;
            optF.activateSound = SoundDef.Named("Click");
            optF.action = switchPowerOnOff;
            optF.groupKey = 313123001;
            yield return optF;


            // Key-Binding C - Auto Pawn ON
            // Enabled when research is done.
            if ((DefDatabase<ResearchProjectDef>.GetNamedSilentFail("ResearchAutoPowerSwitch") != null) && (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchAutoPowerSwitch"))))
            {
                Command_Action optC;
                optC = new Command_Action();
                if (autoSwitchOnPawnActive)
                    optC.icon = texUI_PowerPawnOn;
                else
                    optC.icon = texUI_NoPowerPawnOn;
                optC.hotKey = KeyBindingDefOf.Misc2; //KeyCode.C;
                optC.disabled = false;
                if (!pawnSearchModeDistanceActive)
                    optC.defaultDesc = txtAutoOnMotionRoom;
                else
                    optC.defaultDesc = txtAutoOnMotionNearbyRange + " " + ((int)radarDistancePawn).ToString();
                optC.activateSound = SoundDef.Named("Click");
                optC.action = SwitchPawnActiveOnOff;
                optC.groupKey = 313123002;
                yield return optC;
            }


            // Key-Binding Y - Auto Enemy ON
            // Enabled when research is done.
            if ((DefDatabase<ResearchProjectDef>.GetNamedSilentFail("ResearchAutoEnemySwitch") != null) && (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchAutoEnemySwitch"))))
            {
                Command_Action optY;
                optY = new Command_Action();
                if (autoSwitchOnEnemyActive)
                    optY.icon = texUI_PowerEnemyOn;
                else
                    optY.icon = texUI_NoPowerEnemyOn;
                optY.hotKey = KeyBindingDefOf.Misc3; //KeyCode.Y;
                optY.disabled = false;
                optY.defaultDesc = txtAutoOnEnemyNearbyRange + " " + ((int)radarDistance).ToString();
                optY.activateSound = SoundDef.Named("Click");
                optY.action = SwitchEnemyOnActiveOnOff;
                optY.groupKey = 313123003;
                yield return optY;
            }


            // Key-Binding X - Auto Enemy OFF
            // Enabled when research is done.
            if ((DefDatabase<ResearchProjectDef>.GetNamedSilentFail("ResearchAutoEnemySwitch") != null) && (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchAutoEnemySwitch"))))
            {
                Command_Action optX;
                optX = new Command_Action();
                if (autoSwitchOffEnemyActive)
                    optX.icon = texUI_PowerEnemyOff;
                else
                    optX.icon = texUI_NoPowerEnemyOff;
                optX.hotKey = KeyBindingDefOf.Misc4; //KeyCode.X;
                optX.disabled = false;
                optX.defaultDesc = txtAutoOffEnemyNearbyRange + " " + ((int)radarDistance).ToString();
                optX.activateSound = SoundDef.Named("Click");
                optX.action = SwitchEnemyOffActiveOnOff;
                optX.groupKey = 313123004;
                yield return optX;
            }


            // Key-Binding M - SwitchOnTimer
            // Enabled when research is done.
            if ((DefDatabase<ResearchProjectDef>.GetNamedSilentFail("ResearchTimerSwitch") != null) && (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchTimerSwitch"))))
            {
                Command_Action optM;
                optM = new Command_Action();
                if (autoSwitchTimerActive)
                    optM.icon = texUI_TimerOn;
                else
                    optM.icon = texUI_NoTimerOn;
                optM.hotKey = KeyBindingDefOf.Misc5; //KeyCode.M;
                optM.disabled = false;
                optM.defaultDesc = txtTimerClickSetOnTime;
                optM.activateSound = SoundDef.Named("Click");
                optM.action = TimerOnClicked;
                optM.groupKey = 313123005;
                yield return optM;
            }

            // Key-Binding N - SwitchOffTimer
            // Enabled when research is done.
            if ((DefDatabase<ResearchProjectDef>.GetNamedSilentFail("ResearchTimerSwitch") != null) && (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchTimerSwitch"))))
            {
                Command_Action optN;
                optN = new Command_Action();
                if (autoSwitchTimerActive)
                    optN.icon = texUI_TimerOff;
                else
                    optN.icon = texUI_NoTimerOff;
                optN.hotKey = KeyBindingDefOf.Misc6; //KeyCode.N;
                optN.disabled = false;
                optN.defaultDesc = txtTimerClickSetOffTime;
                optN.activateSound = SoundDef.Named("Click");
                optN.action = TimerOffClicked;
                optN.groupKey = 313123006;
                yield return optN;
            }


        }


        /// <summary>
        /// This string will be shown when the object is selected (focus)
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            //// Power ON - > Done in base!
            //stringBuilder.AppendLine();
            //stringBuilder.Append(txtPower + " ");
            //if (TransmitsPowerNow)
            //    stringBuilder.Append(txtOn);
            //else
            //    stringBuilder.Append(txtOff);

            // Motion ON
            //stringBuilder.Append("\t");
            stringBuilder.AppendLine();
            stringBuilder.Append(txtAutoMotion + " ");
            if (autoSwitchOnPawnActive)
                if (!pawnSearchModeDistanceActive)
                    stringBuilder.Append(txtOn);
                else
                    stringBuilder.Append(txtOnRange + " " + radarDistancePawn.ToString());
            else
                stringBuilder.Append(txtOff);

            // Enemy ON
            stringBuilder.AppendLine();
            stringBuilder.Append(txtAutoEnemy + " ");
            if (autoSwitchOnEnemyActive || autoSwitchOffEnemyActive)
                stringBuilder.Append(txtOn);
            else
                stringBuilder.Append(txtOff);

            // Timer ON
            stringBuilder.AppendLine();
            stringBuilder.Append(txtTimer + " ");
            stringBuilder.Append(string.Format(txtTimerFormat, autoSwitchTimerOnTime, autoSwitchTimerOffTime));
            stringBuilder.Append(". ");
            if (autoSwitchTimerActive)
                stringBuilder.Append(txtOn);
            else
                stringBuilder.Append(txtOff);

            return stringBuilder.ToString();
        }

        #endregion



        #region Search Functions

        /// <summary>
        /// Search for enemies in range
        /// </summary>
        /// <returns></returns>
        private bool SearchForEnemy()
        {
            IEnumerable<Pawn> pawns = Radar.FindEnemyPawns(this.Position, radarDistance);

            return (pawns.Count() > 0);
        }


        /// <summary>
        /// Search for pawn in the room
        /// </summary>
        /// <returns></returns>
        private bool SearchForPawnInRoom()
        {
            IEnumerable<Pawn> pawns;
            if (!pawnSearchModeDistanceActive)
                pawns = Radar.FindAllPawnsInRoom(RoomQuery.RoomAt(Position));
            else
                pawns = Radar.FindAllPawns(this.Position, radarDistancePawn);

            return (pawns.Count() > 0);
        }


        private void DestroyOtherPowerTransmitter()
        {
            // Destroy other building with power transmit ability
            List<Thing> thingsHere = Find.ThingGrid.ThingsListAt(Position);
            for (int i = 0; i < thingsHere.Count; i++)
            {
                Thing t = thingsHere[i];
                if (t != null && t != this)
                {
                    // Check for power transmitter
                    CompPowerTransmitter cpt = t.TryGetComp<CompPowerTransmitter>();
                    if (cpt != null)
                    {
                        //t.Destroy(DestroyMode.Deconstruct);
                        t.Destroy(DestroyMode.Vanish);
                    }

                    // Check if it is a plant
                    if (t.def.category == ThingCategory.Plant)
                    {
                        t.Destroy(DestroyMode.Vanish);
                    }

                }
            }
        }

        #endregion

        #region Switch Functions

        /// <summary>
        /// Switch to the other power transfer state
        /// </summary>
        private void switchPowerOnOff()
        {
            //Called by user >> deactivate automatic modes!
            autoSwitchOnEnemyActive = false;
            autoSwitchOffEnemyActive = false;
            autoSwitchOnPawnActive = false;
            autoSwitchTimerActive = false;

            // Invert power active
            SetWantSwitchOn ( !flickableComp.SwitchIsOn);

            // Set Pawn mode to room
            pawnSearchModeDistanceActive = false;
        }


        /// <summary>
        /// Switch Auto ON EnemySearch On/Off
        /// </summary>
        private void SwitchEnemyOnActiveOnOff()
        {
            autoSwitchOnEnemyActive = !autoSwitchOnEnemyActive;
            autoSwitchOffEnemyActive = false;
            autoSwitchOnPawnActive = false;
            autoSwitchTimerActive = false;
        }


        /// <summary>
        /// Switch Auto OFF EnemySearch On/Off
        /// </summary>
        private void SwitchEnemyOffActiveOnOff()
        {
            autoSwitchOffEnemyActive = !autoSwitchOffEnemyActive;
            autoSwitchOnEnemyActive = false;
            autoSwitchOnPawnActive = false;
            autoSwitchTimerActive = false;
        }


        /// <summary>
        /// Switch MotionSearch On => Switch between modes
        /// </summary>
        private void SwitchPawnActiveOnOff()
        {
            if (!autoSwitchOnPawnActive)
            {
                autoSwitchOnPawnActive = true; // !autoSwitchOnPawnActive;
                autoSwitchOnEnemyActive = false;
                autoSwitchOffEnemyActive = false;
                autoSwitchTimerActive = false;

                // if outdoors, preset to distance
                //if (Position.  == null)
                //    pawnSearchModeDistanceActive = true;

                //if (isWall)
                //    pawnSearchModeDistanceActive = true;

                return;
            }

            //switch between room search and distance search
            //if (!isWall)
                pawnSearchModeDistanceActive = !pawnSearchModeDistanceActive;
            //else
            //    pawnSearchModeDistanceActive = true;

        }


        /// <summary>
        /// Switch Off was clicked, activate or update time
        /// </summary>
        private void TimerOffClicked()
        {
            if (!autoSwitchTimerActive)
            {
                //Called by timer >> deactivate other modes!
                autoSwitchOnEnemyActive = false;
                autoSwitchOffEnemyActive = false;
                autoSwitchOnPawnActive = false;
                autoSwitchTimerActive = true;
                return;
            }

            autoSwitchTimerOffTime += 1;

            if (autoSwitchTimerOffTime > 23)
                autoSwitchTimerOffTime = 0;

            if (autoSwitchTimerOffTime == autoSwitchTimerOnTime)
                autoSwitchTimerOffTime += 1;

            //needs to be checked twice!
            if (autoSwitchTimerOffTime > 23)
                autoSwitchTimerOffTime = 0;

        }


        /// <summary>
        /// Switch On was clicked, activate or update time
        /// </summary>
        private void TimerOnClicked()
        {
            if (!autoSwitchTimerActive)
            {
                //Called by timer >> deactivate other modes!
                autoSwitchOnEnemyActive = false;
                autoSwitchOffEnemyActive = false;
                autoSwitchOnPawnActive = false;
                autoSwitchTimerActive = true;
                return;
            }

            autoSwitchTimerOnTime += 1;

            if (autoSwitchTimerOnTime > 23)
                autoSwitchTimerOnTime = 0;

            if (autoSwitchTimerOnTime == autoSwitchTimerOffTime)
                autoSwitchTimerOnTime += 1;

            //needs to be checked twice!
            if (autoSwitchTimerOnTime > 23)
                autoSwitchTimerOnTime = 0;
        }


        /// <summary>
        /// Set the ticks for a timed switching
        /// </summary>
        /// <param name="setOffTimed"></param>
        /// <param name="setOnTimed"></param>
        private void SetTimedSwitch(bool setOnTimed, bool setOffTimed, bool setDirect = false)
        {
            if (setOffTimed)
                ticksSwitchOff = ticksRequestSwitchOff;

            if (setOnTimed)
                ticksSwitchOn = ticksRequestSwitchOn;

            if (setDirect)
                switchDirectly = true;
        }


        /// <summary>
        /// Reset timed switching
        /// </summary>
        private void ResetTimedSwitch()
        {
            if ((ticksSwitchOn < 0) && (ticksSwitchOff < 0))
                return;

            ticksSwitchOff = -1;
            ticksSwitchOn = -1;
        }


        /// <summary>
        /// Update the timed switching every tick
        /// </summary>
        private void UpdateTimedSwitch()
        {
            if ((ticksSwitchOff < 0) && (ticksSwitchOn < 0))
                return;

            if (ticksSwitchOff >= 0)
                ticksSwitchOff -= 1;

            if (ticksSwitchOn >= 0)
                ticksSwitchOn -= 1;

            if (ticksSwitchOff == 0)
            {
                if (flickableComp.SwitchIsOn)
                {
                    if (switchDirectly)
                        flickableComp.SwitchIsOn = false;
                    SetWantSwitchOn(false);

                    switchDirectly = false;
                }

                ResetTimedSwitch();
            }

            if (ticksSwitchOn == 0)
            {
                if (!flickableComp.SwitchIsOn)
                {
                    if (switchDirectly)
                        flickableComp.SwitchIsOn = true;
                    SetWantSwitchOn(true);

                    switchDirectly = false;
                }

                ResetTimedSwitch();
            }

        }



        public void SetWantSwitchOn(bool on)
        {
            // With this Reflection you can access a private variable! Here: The private list "wantSwitchOn" is set 
            System.Reflection.FieldInfo fi = typeof(CompFlickable).GetField("wantSwitchOn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            fi.SetValue(flickableComp, on);

            FlickUtility.UpdateFlickDesignation(this);
        }

        #endregion

    }
}
