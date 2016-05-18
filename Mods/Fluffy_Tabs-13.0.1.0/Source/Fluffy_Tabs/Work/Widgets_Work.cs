using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Fluffy_Tabs
{
    public static partial class Widgets
    {
        #region Fields

        private static int _clickLength = 0;
        private static int _clickThreshold = 25;
        private static Pair<Pawn, Def> _mouseDownOn;

        #endregion Fields

        #region Methods

        public static void DecrementPriorities( this WorkTypeDef worktype, List<Pawn> pawns, bool toggle, bool scheduler, List<int> hours )
        {
            bool valid = false;

            foreach ( Pawn pawn in pawns )
            {
                if ( !( pawn.story == null || pawn.story.WorkTypeIsDisabled( worktype ) ) )
                {
                    int cur = pawn.worktypePriorities().GetPriority( worktype, hours.First() );
                    if ( !toggle && cur > 0 && cur < Settings.MaxPriority )
                    {
                        if ( scheduler )
                            pawn.worktypePriorities().SetPriority( worktype, cur + 1, hours );
                        else
                            pawn.worktypePriorities().SetPriority( worktype, cur + 1 );
                        valid = true;
                    }
                    if ( cur == Settings.MaxPriority || ( toggle && cur > 0 ) )
                    {
                        if ( scheduler )
                            pawn.worktypePriorities().SetPriority( worktype, 0, hours );
                        else
                            pawn.worktypePriorities().SetPriority( worktype, 0 );
                        valid = true;
                    }
                }
            }
            if ( valid && toggle )
                SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
            if ( valid && !toggle )
                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
            if ( !valid )
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
        }

        public static void DecrementPriorities( this WorkGiverDef workgiver, List<Pawn> pawns, bool toggle, bool scheduler, List<int> hours )
        {
            bool valid = false;

            foreach ( Pawn pawn in pawns )
            {
                if ( pawn.CapableOf( workgiver ) )
                {
                    int cur = pawn.workgiverPriorities().GetPriority( workgiver, hours.First() );
                    if ( !toggle && cur > 0 && cur < Settings.MaxPriority )
                    {
                        if ( scheduler )
                            pawn.workgiverPriorities().SetPriority( workgiver, cur + 1, hours );
                        else
                            pawn.workgiverPriorities().SetPriority( workgiver, cur + 1 );
                        valid = true;
                    }
                    if ( cur == Settings.MaxPriority || ( toggle && cur > 0 ) )
                    {
                        if ( scheduler )
                            pawn.workgiverPriorities().SetPriority( workgiver, 0, hours );
                        else
                            pawn.workgiverPriorities().SetPriority( workgiver, 0 );
                        valid = true;
                    }
                }
            }
            if ( valid && toggle )
                SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
            if ( valid && !toggle )
                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
            if ( !valid )
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
        }

        public static void DecrementPriority( this Pawn pawn, WorkTypeDef worktype, bool toggle, bool scheduler, List<int> hours )
        {
            var oldPrio = pawn.worktypePriorities().GetPriority( worktype, hours.First() );
            var newPrio = toggle ? 0 : oldPrio < Settings.MaxPriority ? oldPrio + 1 : 0;
            if ( oldPrio != newPrio )
            {
                if ( scheduler )
                    pawn.worktypePriorities().SetPriority( worktype, newPrio, hours );
                else
                    pawn.worktypePriorities().SetPriority( worktype, newPrio );

                if ( toggle )
                    SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                else
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
            }
        }

        public static void DecrementPriority( this Pawn pawn, WorkGiverDef workgiver, bool toggle, bool scheduler, List<int> hours )
        {
            var oldPrio = pawn.workgiverPriorities().GetPriority( workgiver, hours.First() );
            var newPrio = toggle ? 0 : oldPrio < Settings.MaxPriority ? oldPrio + 1 : 0;

            if ( oldPrio != newPrio )
            {
                if ( scheduler )
                    pawn.workgiverPriorities().SetPriority( workgiver, newPrio, hours );
                else
                    pawn.workgiverPriorities().SetPriority( workgiver, newPrio );

                if ( toggle )
                    SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                else
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
            }
        }

        public static string DefaultIconPath( this JobDef job )
        {
            // long list of vanilla jobs with presets
            // combat
            if ( job == JobDefOf.AttackMelee || job == JobDefOf.AttackStatic || job == JobDefOf.FleeAndCower || job == JobDefOf.ManTurret || job == JobDefOf.WaitCombat )
                return "UI/Icons/Various/combat";

            // warden
            if ( job == JobDefOf.Arrest || job == JobDefOf.Capture || job == JobDefOf.EscortPrisonerToBed || job == JobDefOf.PrisonerAttemptRecruit || job == JobDefOf.PrisonerExecution || job == JobDefOf.PrisonerFriendlyChat || job == JobDefOf.ReleasePrisoner || job == JobDefOf.TakeWoundedPrisonerToBed )
                return "UI/Icons/Various/handcuffs";

            // doctor
            if ( job == JobDefOf.FeedPatient || job == JobDefOf.Rescue || job == JobDefOf.TakeToBedToOperate || job == JobDefOf.TendPatient || job == JobDefOf.VisitSickPawn )
                return "UI/Icons/Various/health";

            // haul
            if ( job == JobDefOf.CarryToCryptosleepCasket || job == JobDefOf.BuryCorpse || job == JobDefOf.HaulToCell || job == JobDefOf.HaulToContainer || job == JobDefOf.Refuel )
                return "UI/Icons/Various/haul";

            // clean
            if ( job == JobDefOf.Clean || job == JobDefOf.ClearSnow )
                return "UI/Icons/Various/clean";

            // farm
            if ( job == JobDefOf.CutPlant || job == JobDefOf.Harvest || job == JobDefOf.Sow )
                return "UI/Icons/Various/farming";

            // animal handling
            if ( job == JobDefOf.Milk || job == JobDefOf.Shear || job == JobDefOf.Slaughter || job == JobDefOf.Tame )
                return "UI/Icons/Various/livestock";

            // hunting
            if ( job == JobDefOf.Hunt )
                return "UI/Icons/Various/hunt";

            // sleep
            if ( job == JobDefOf.LayDown )
                return "UI/Icons/Various/zzz";

            // social
            if ( job == JobDefOf.SocialRelax || job == JobDefOf.SpectateCeremony || job == JobDefOf.StandAndBeSociallyActive )
                return "UI/Icons/Various/social";

            // eat
            if ( job == JobDefOf.Ingest )
                return "UI/Icons/Various/eat";

            // fire extinguishing
            if ( job == JobDefOf.BeatFire || job == JobDefOf.TriggerFirefoamPopper || job == JobDefOf.ExtinguishSelf )
                return "UI/Icons/Various/extinguish";

            // moving
            if ( job == JobDefOf.Goto || job == JobDefOf.GotoSafeTemperature )
                return "UI/Icons/Various/move";

            // research
            if ( job == JobDefOf.Research )
                return "UI/Icons/Various/research";

            // mining
            if ( job == JobDefOf.Mine )
                return "UI/Icons/Various/mine";

            // construct
            if ( job == JobDefOf.FinishFrame || job == JobDefOf.PlaceNoCostFrame || job == JobDefOf.RemoveFloor || job == JobDefOf.SmoothFloor || job == JobDefOf.Uninstall )
                return "UI/Icons/Various/hammer";

            // repair
            if ( job == JobDefOf.FixBrokenDownBuilding || job == JobDefOf.Repair )
                return "UI/Icons/Various/wrench";

            // bills
            // TODO: Try to split bills to type
            if ( job == JobDefOf.DoBill )
                return "UI/Icons/Various/star";

            return Settings.DefaultJobIconPath;
        }

        public static void DrawFavouritesCell( Rect cell, float IconSize, Pawn pawn, bool dwarfTherapistMode )
        {
            Rect iconRect = new Rect( 0f, 0f, IconSize, IconSize );
            iconRect.center = cell.center;

            Texture2D icon = Resources.Favourite;
            WorkFavourite favourite;
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            string tip = "";

            // stuff for DT mode
            if ( dwarfTherapistMode )
            {
                // is there a preset assigned?
                favourite = pawn.workgiverPriorities().currentFavourite;
                if ( favourite != null )
                {
                    icon = pawn.workgiverPriorities().currentFavourite.Icon;
                    tip += "FluffyTabs.CurrentFavourite".Translate( favourite.label );
                }
            }

            // 'vanilla' mode
            else
            {
                // is there a preset assigned?
                favourite = pawn.worktypePriorities().currentFavourite;
                if ( favourite != null )
                {
                    icon = pawn.worktypePriorities().currentFavourite.Icon;
                    tip += "FluffyTabs.CurrentFavourite".Translate( favourite.label );
                }
            }

            // no favourite, add option to create one
            if ( favourite == null )
            {
                tip += "FluffyTabs.ClickToAssignOrCreateFavourite".Translate();
                options.Add( new FloatMenuOption( "FluffyTabs.CreateNewFavouriteTitle".Translate(), delegate
                { Find.WindowStack.Add( new Dialog_CreateFavourite( pawn, dwarfTherapistMode ) ); } ) );
            }
            else
            {
                tip += "FluffyTabs.ClickToAssignFavourite".Translate();
            }

            // add options for assigning favourites
            foreach ( var _favourite in MapComponent_Favourites.Instance.favourites.Where( fav => fav.dwarfTherapistMode == dwarfTherapistMode ) )
            {
                options.Add( new FloatMenuOption( "FluffyTabs.AssignFavouriteX".Translate( _favourite.label ), delegate
                {
                    if ( dwarfTherapistMode )
                        pawn.workgiverPriorities().AssignFavourite( _favourite );
                    else
                        pawn.worktypePriorities().AssignFavourite( _favourite );
                } ) );
            }

            if ( Widgets.ImageButton( iconRect, icon, tip ) )
                Find.WindowStack.Add( new FloatMenu( options ) );
        }

        public static void DrawMoodCell( Rect cell, float IconSize, Pawn pawn )
        {
            Rect iconRect = new Rect( 0f, 0f, IconSize, IconSize );
            iconRect.center = cell.center;

            TooltipHandler.TipRegion( cell, pawn.needs.mood.GetTipString() );

            // if currently broken, we can be done early
            if ( pawn.mindState.mentalStateHandler?.CurStateDef?.stateType == MentalStateType.Hard )
            {
                GUI.color = Color.red;
                GUI.DrawTexture( iconRect, Resources.MoodBroken );
                GUI.color = Color.white;
                return;
            }
            if ( pawn.mindState.mentalStateHandler?.CurStateDef?.stateType == MentalStateType.Soft )
            {
                GUI.color = Color.yellow;
                GUI.DrawTexture( iconRect, Resources.MoodBroken );
                GUI.color = Color.white;
                return;
            }

            // current level
            var mood = pawn.needs.mood.CurLevelPercentage;
            var hardBreak = pawn.mindState.mentalStateStarter.StartHardMentalStateThreshold;
            var softBreak = pawn.mindState.mentalStateStarter.StartSoftMentalStateThreshold;

            // color and icon
            Color color;
            Texture2D texture;

            if ( mood < hardBreak )
            {
                color = Color.red;
                texture = Resources.MoodUnhappy;
            }
            else if ( mood < softBreak )
            {
                color = Color.Lerp( Color.red, Color.yellow, ( mood - hardBreak ) / ( softBreak - hardBreak ) );
                texture = Resources.MoodUnhappy;
            }
            else if ( mood < .5 )
            {
                color = Color.Lerp( Color.yellow, Color.grey, ( mood - softBreak ) / ( .5f - softBreak ) );
                texture = Resources.MoodDiscontent;
            }
            else if ( mood < .9 )
            {
                color = Color.Lerp( Color.grey, Color.green, ( mood - .5f ) / .4f );
                texture = Resources.MoodContent;
            }
            else
            {
                color = Color.green;
                texture = Resources.MoodHappy;
            }

            GUI.color = color;
            GUI.DrawTexture( iconRect, texture );
            GUI.color = Color.white;
        }

        public static void DrawStatusCell( Rect cell, float IconSize, Pawn pawn )
        {
            var job = pawn?.CurJob?.def;
            if ( job == null )
                return;

            Rect iconRect = new Rect( 0f, 0f, IconSize, IconSize );
            iconRect.center = cell.center;
            TooltipHandler.TipRegion( cell, pawn.jobs?.curDriver?.GetReport() ?? "" );
            GUI.DrawTexture( iconRect, job.StatusIcon() );
        }

        public static void DrawWorkBoxFor( WorkGiverDef workgiver, Rect cell, Pawn pawn, bool scheduler, List<int> hours )
        {
            // draw colour code
            DrawBackground( cell, Settings.WorktypeColors[workgiver.workType], Settings.WorkgiverColorOpacity );

            // bug out if pawn can't do this type of work
            if ( pawn.story == null || pawn.story.WorkTypeIsDisabled( workgiver.workType ) || !pawn.CapableOf( workgiver ) )
                return;

            // create and position rect
            float size = ( new float[] { cell.width - 2f, cell.height - 2f, Settings.WorkgiverBoxSize } ).Min();
            Rect boxRect = new Rect( 0f, 0f, size, size );
            boxRect.center = cell.center;

            // draw background, handle tooltip
            bool partiallyScheduled = pawn.workgiverPriorities().IsPartiallyScheduledFor( workgiver );
            DrawWorkBoxBackground( boxRect, pawn, workgiver.workType, partiallyScheduled );
            TooltipHandler.TipRegion( boxRect, TipForPawnWorker( pawn, workgiver ) );
            if ( partiallyScheduled )
                TooltipHandler.TipRegion( boxRect, pawn.workgiverPriorities().PartiallyScheduledTip( workgiver ) );

            // handle label and interactions
            int priority = pawn.workgiverPriorities().GetPriority( workgiver, hours.First() );
            if ( Find.PlaySettings.useWorkPriorities )
            {
                string label;
                if ( priority > 0 )
                {
                    label = priority.ToString();
                }
                else
                {
                    label = string.Empty;
                }

                Label( boxRect, label, ColorOfPriority( priority ), GameFont.Small, TextAnchor.MiddleCenter );

                if ( Mouse.IsOver( boxRect ) )
                {
                    if ( ( Event.current.type == EventType.MouseDown && Event.current.button == 0 ) || ( Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f ) )
                    {
                        pawn.IncrementPriority( workgiver, false, scheduler, hours );
                        Event.current.Use();
                    }
                    if ( ( Event.current.type == EventType.MouseDown && Event.current.button == 1 ) || ( Event.current.type == EventType.ScrollWheel && Event.current.delta.y > 0f ) )
                    {
                        pawn.DecrementPriority( workgiver, false, scheduler, hours );
                        Event.current.Use();
                    }
                }
            }
            else
            {
                if ( priority > 0 )
                {
                    GUI.DrawTexture( boxRect, Resources.WorkBoxCheckTex );
                }
                if ( Mouse.IsOver( boxRect ) )
                {
                    // catch clicks
                    // for some reason down & up get called 4 times, make sure
                    if ( Input.GetMouseButtonDown( 0 ) && _mouseDownOn.First == null )
                    {
                        _mouseDownOn = new Pair<Pawn, Def>( pawn, workgiver );
                        _clickLength = 0;
                    }
                    if ( Input.GetMouseButtonUp( 0 ) )
                    {
                        if ( pawn == _mouseDownOn.First && workgiver == _mouseDownOn.Second )
                        {
                            if ( priority < 1 )
                            {
                                pawn.IncrementPriority( workgiver, true, scheduler, hours );
                            }
                            else
                            {
                                pawn.DecrementPriority( workgiver, true, scheduler, hours );
                            }
                        }
                        // for some reason mouseDown & mouseUp get registered 4 times
                        // set the tracker to null to avoid further down calls and immediate resets
                        _mouseDownOn = new Pair<Pawn, Def>( null, null );
                        _clickLength = 0;
                    }
                    // catch drags
                    if ( Input.GetMouseButton( 0 ) )
                    {
                        // if this is the cell that a click originated in, delay dragging action until threshold is reached.
                        if ( pawn == _mouseDownOn.First && workgiver == _mouseDownOn.Second )
                        {
                            if ( _clickLength++ < _clickThreshold )
                            {
                                return;
                            }
                        }

                        // Log.Message(p.workgiverPriorities.GetPriority(wType).ToString());
                        // Priority of 'active' is 1 when manual is disabled, even if set to 3
                        pawn.IncrementPriority( workgiver, true, scheduler, hours );
                    }
                    else if ( Input.GetMouseButton( 1 ) )
                    {
                        pawn.DecrementPriority( workgiver, true, scheduler, hours );
                    }
                }
            }
        }

        public static void DrawWorkBoxFor( WorkTypeDef worktype, Rect cell, Pawn pawn, bool scheduler, List<int> hours )
        {
            // bug out if pawn can't do this type of work
            if ( pawn.story == null || pawn.story.WorkTypeIsDisabled( worktype ) )
                return;

            // create and position rect
            Rect boxRect = new Rect( 0f, 0f, Settings.WorktypeBoxSize, Settings.WorktypeBoxSize );
            boxRect.center = cell.center;

            // draw background, handle tooltip
            bool partiallyScheduled = pawn.worktypePriorities().IsPartiallyScheduledFor( worktype );
            DrawWorkBoxBackground( boxRect, pawn, worktype, partiallyScheduled );
            TooltipHandler.TipRegion( boxRect, TipForPawnWorker( pawn, worktype ) );
            if ( partiallyScheduled )
                TooltipHandler.TipRegion( boxRect, pawn.worktypePriorities().PartiallyScheduledTip( worktype ) );

            // handle label and interactions
            int priority = pawn.worktypePriorities().GetPriority( worktype, hours.First() );
            if ( Find.PlaySettings.useWorkPriorities )
            {
                string label;
                if ( priority > 0 )
                {
                    label = priority.ToString();
                }
                else
                {
                    label = string.Empty;
                }

                Label( boxRect, label, ColorOfPriority( priority ), GameFont.Medium, TextAnchor.MiddleCenter );

                if ( Mouse.IsOver( boxRect ) )
                {
                    if ( ( Event.current.type == EventType.MouseDown && Event.current.button == 0 ) || ( Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f ) )
                    {
                        pawn.IncrementPriority( worktype, false, scheduler, hours );
                        Event.current.Use();
                    }
                    if ( ( Event.current.type == EventType.MouseDown && Event.current.button == 1 ) || ( Event.current.type == EventType.ScrollWheel && Event.current.delta.y > 0f ) )
                    {
                        pawn.DecrementPriority( worktype, false, scheduler, hours );
                        Event.current.Use();
                    }
                }
            }
            else
            {
                if ( priority > 0 )
                {
                    GUI.DrawTexture( boxRect, Resources.WorkBoxCheckTex );
                }
                if ( Mouse.IsOver( boxRect ) )
                {
                    // catch clicks
                    // for some reason down & up get called 4 times, make sure
                    if ( Input.GetMouseButtonDown( 0 ) && _mouseDownOn.First == null )
                    {
                        _mouseDownOn = new Pair<Pawn, Def>( pawn, worktype );
                        _clickLength = 0;
                    }
                    if ( Input.GetMouseButtonUp( 0 ) )
                    {
                        if ( pawn == _mouseDownOn.First && worktype == _mouseDownOn.Second )
                        {
                            if ( priority < 1 )
                            {
                                pawn.IncrementPriority( worktype, true, scheduler, hours );
                            }
                            else
                            {
                                pawn.DecrementPriority( worktype, true, scheduler, hours );
                            }
                        }
                        // for some reason mouseDown & mouseUp get registered 4 times
                        // set the tracker to null to avoid further down calls and immediate resets
                        _mouseDownOn = new Pair<Pawn, Def>( null, null );
                        _clickLength = 0;
                    }
                    // catch drags
                    if ( Input.GetMouseButton( 0 ) )
                    {
                        // if this is the cell that a click originated in, delay dragging action until threshold is reached.
                        if ( pawn == _mouseDownOn.First && worktype == _mouseDownOn.Second )
                        {
                            if ( _clickLength++ < _clickThreshold )
                            {
                                return;
                            }
                        }

                        // Log.Message(p.worktypePriorities().GetPriority(wType).ToString());
                        // Priority of 'active' is 1 when manual is disabled, even if set to 3
                        pawn.IncrementPriority( worktype, true, scheduler, hours );
                    }
                    else if ( Input.GetMouseButton( 1 ) )
                    {
                        pawn.DecrementPriority( worktype, true, scheduler, hours );
                    }
                }
            }
        }

        public static string FormatHour( this int hour )
        {
            if ( MapComponent_PawnPriorities.Instance.TwentyFourHourMode )
                return hour.ToString( "D2" ) + ":00";
            else
            {
                int noon = GenDate.HoursPerDay / 2;
                if ( hour == 0 )
                    return "midnight".Translate();
                if ( hour == noon )
                    return "noon".Translate();
                else
                    return hour % noon + ( hour > noon ? " p.m." : " a.m." );
            }
        }

        public static void IncrementPriorities( this WorkTypeDef worktype, List<Pawn> pawns, bool toggle, bool scheduler, List<int> hours )
        {
            int start = toggle ? 3 : Settings.MaxPriority;
            bool valid = false;

            foreach ( Pawn pawn in pawns )
            {
                if ( !( pawn.story == null || pawn.story.WorkTypeIsDisabled( worktype ) ) )
                {
                    int cur = pawn.worktypePriorities().GetPriority( worktype, hours.First() );
                    if ( cur > 1 )
                    {
                        if ( scheduler )
                            pawn.worktypePriorities().SetPriority( worktype, cur - 1, hours );
                        else
                            pawn.worktypePriorities().SetPriority( worktype, cur - 1 );

                        valid = true;
                    }
                    if ( cur == 0 )
                    {
                        if ( scheduler )
                            pawn.worktypePriorities().SetPriority( worktype, start, hours );
                        else
                            pawn.worktypePriorities().SetPriority( worktype, start );
                        valid = true;
                    }
                }
            }

            if ( valid && toggle )
                SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
            if ( valid && !toggle )
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
            if ( !valid )
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
        }

        public static void IncrementPriorities( this WorkGiverDef workgiver, List<Pawn> pawns, bool toggle, bool scheduler, List<int> hours )
        {
            int start = toggle ? 3 : Settings.MaxPriority;
            bool valid = false;

            foreach ( Pawn pawn in pawns )
            {
                if ( pawn.CapableOf( workgiver ) )
                {
                    int cur = pawn.workgiverPriorities().GetPriority( workgiver, hours.First() );
                    if ( cur > 1 )
                    {
                        if ( scheduler )
                            pawn.workgiverPriorities().SetPriority( workgiver, cur - 1, hours );
                        else
                            pawn.workgiverPriorities().SetPriority( workgiver, cur - 1 );

                        valid = true;
                    }
                    if ( cur == 0 )
                    {
                        if ( scheduler )
                            pawn.workgiverPriorities().SetPriority( workgiver, start, hours );
                        else
                            pawn.workgiverPriorities().SetPriority( workgiver, start );
                        valid = true;
                    }
                }
            }

            if ( valid && toggle )
                SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
            if ( valid && !toggle )
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
            if ( !valid )
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
        }

        public static void IncrementPriority( this Pawn pawn, WorkTypeDef worktype, bool toggle, bool scheduler, List<int> hours )
        {
            var oldPrio = pawn.worktypePriorities().GetPriority( worktype, hours.First() );
            var newPrio = toggle ? 1 : oldPrio > 0 ? oldPrio - 1 : Settings.MaxPriority;
            if ( oldPrio != newPrio )
            {
                if ( scheduler )
                    pawn.worktypePriorities().SetPriority( worktype, newPrio, hours );
                else
                    pawn.worktypePriorities().SetPriority( worktype, newPrio );

                if ( toggle )
                    SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                else
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
            }
        }

        public static void IncrementPriority( this Pawn pawn, WorkGiverDef workgiver, bool toggle, bool scheduler, List<int> hours )
        {
            var oldPrio = pawn.workgiverPriorities().GetPriority( workgiver, hours.First() );
            var newPrio = toggle ? 1 : oldPrio > 0 ? oldPrio - 1 : Settings.MaxPriority;

            if ( oldPrio != newPrio )
            {
                if ( scheduler )
                    pawn.workgiverPriorities().SetPriority( workgiver, newPrio, hours );
                else
                    pawn.workgiverPriorities().SetPriority( workgiver, newPrio );

                if ( toggle )
                    SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                else
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
            }
        }

        public static TipSignal TipForPawnWorker( Pawn pawn, WorkGiverDef workgiver )
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( workgiver.verb );
            string text = string.Empty;
            if ( workgiver.workType.relevantSkills.Count == 0 )
            {
                text = "NoneBrackets".Translate();
            }
            else
            {
                foreach ( SkillDef current in workgiver.workType.relevantSkills )
                {
                    text = text + current.skillLabel + ", ";
                }
                text = text.Substring( 0, text.Length - 2 );
            }
            stringBuilder.AppendLine( "RelevantSkills".Translate( text, pawn.skills.AverageOfRelevantSkillsFor( workgiver.workType ).ToString(), 20 ) );
            stringBuilder.AppendLine();
            stringBuilder.Append( workgiver.description );
            return stringBuilder.ToString();
        }

        public static TipSignal TipForPawnWorker( Pawn pawn, WorkTypeDef worktype )
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( worktype.gerundLabel );
            if ( pawn.story.WorkTypeIsDisabled( worktype ) )
            {
                stringBuilder.Append( "CannotDoThisWork".Translate( pawn.NameStringShort ) );
            }
            else
            {
                string text = string.Empty;
                if ( worktype.relevantSkills.Count == 0 )
                {
                    text = "NoneBrackets".Translate();
                }
                else
                {
                    foreach ( SkillDef current in worktype.relevantSkills )
                    {
                        text = text + current.skillLabel + ", ";
                    }
                    text = text.Substring( 0, text.Length - 2 );
                }
                stringBuilder.AppendLine( "RelevantSkills".Translate( text, pawn.skills.AverageOfRelevantSkillsFor( worktype ).ToString(), 20 ) );
                stringBuilder.AppendLine();
                stringBuilder.Append( worktype.description );
            }
            return stringBuilder.ToString();
        }

        public static PawnWorkgiverPrioritiesTracker workgiverPriorities( this Pawn pawn )
        {
            return MapComponent_PawnPriorities.Instance.WorkgiverTracker( pawn );
        }

        public static PawnWorktypePrioritiesTracker worktypePriorities( this Pawn pawn )
        {
            return MapComponent_PawnPriorities.Instance.WorktypeTracker( pawn );
        }

        private static bool CapableOf( this Pawn pawn, WorkGiverDef workgiver )
        {
            foreach ( var capacity in workgiver.requiredCapacities )
            {
                if ( !pawn.health.capacities.CapableOf( capacity ) )
                    return false;
            }
            return true;
        }

        private static Color ColorOfPriority( int priority )
        {
            int midPriority = Settings.MaxPriority / 2;
            if ( priority == 1 )
                return Resources.HighPriority;
            if ( priority == midPriority )
                return Resources.MidPriority;
            if ( priority == Settings.MaxPriority )
                return Resources.LowPriority;
            if ( priority > 0 && priority < midPriority )
                return Color.Lerp( Resources.HighPriority, Resources.MidPriority, (float)priority / midPriority );
            if ( priority > midPriority && priority < Settings.MaxPriority )
                return Color.Lerp( Resources.MidPriority, Resources.LowPriority, (float)( priority - midPriority ) / ( Settings.MaxPriority - midPriority ) );
            return Color.blue;
        }

        private static void DrawWorkBoxBackground( Rect canvas, Pawn pawn, WorkTypeDef worktype, bool partiallyAssigned )
        {
            // skill based colours
            float averageSkill = pawn.skills.AverageOfRelevantSkillsFor( worktype );
            Texture2D background;
            Texture2D foreground;
            float opacity;
            if ( averageSkill <= Settings.MidAptCutOff )
            {
                background = Resources.WorkBoxBGTex_Bad;
                foreground = Resources.WorkBoxBGTex_Mid;
                opacity = averageSkill / Settings.MidAptCutOff;
            }
            else
            {
                background = Resources.WorkBoxBGTex_Mid;
                foreground = Resources.WorkBoxBGTex_Excellent;
                opacity = ( averageSkill - Settings.MidAptCutOff ) / 6f;
            }
            GUI.DrawTexture( canvas, background );
            GUI.color = new Color( 1f, 1f, 1f, opacity );
            GUI.DrawTexture( canvas, foreground );

            // passion icon
            Passion passion = pawn.skills.MaxPassionOfRelevantSkillsFor( worktype );
            if ( passion > Passion.None )
            {
                GUI.color = new Color( 1f, 1f, 1f, Settings.PassionOpacity );

                Rect passionRect = canvas;
                passionRect.xMin = canvas.center.x;
                passionRect.yMin = canvas.center.y;
                if ( passion == Passion.Minor )
                {
                    GUI.DrawTexture( passionRect, Resources.PassionWorkboxMinorIcon );
                }
                else if ( passion == Passion.Major )
                {
                    GUI.DrawTexture( passionRect, Resources.PassionWorkboxMajorIcon );
                }
            }

            // partially scheduled icon
            if ( partiallyAssigned )
            {
                GUI.color = new Color( 1f, 1f, 1f, .5f );
                Rect partiallyScheduledRect = new Rect( canvas );
                partiallyScheduledRect.xMax = canvas.center.x;
                partiallyScheduledRect.yMin = canvas.center.y;

                GUI.DrawTexture( partiallyScheduledRect, Resources.Clock );
            }
            GUI.color = Color.white;
        }

        #endregion Methods
    }
}