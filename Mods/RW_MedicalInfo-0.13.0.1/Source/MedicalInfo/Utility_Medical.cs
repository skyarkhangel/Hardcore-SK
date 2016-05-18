using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fluffy
{
    public static class Utility_Medical
    {
        public static Texture2D[] CareTextures =
        {
            ContentFinder<Texture2D>.Get( "UI/Icons/Medical/NoCare" ),
            ContentFinder<Texture2D>.Get( "UI/Icons/Medical/NoMeds" ),
            ThingDefOf.HerbalMedicine.uiIcon,
            ThingDefOf.Medicine.uiIcon,
            ThingDefOf.GlitterworldMedicine.uiIcon
        };

        public static Texture2D BloodTexture = ContentFinder<Texture2D>.Get( "UI/Icons/Medical/Bleeding" ),
                                BloodTextureWhite = ContentFinder<Texture2D>.Get( "UI/Buttons/blood" ),
                                OpTexture = ContentFinder<Texture2D>.Get( "UI/Buttons/medical" );

        private static readonly Queue<BodyPartRecord> MissingBodyPartQueue = new Queue<BodyPartRecord>( );

        public static void MedicalCareSetter( Rect rect, ref MedicalCareCategory medCare )
        {
            float iconSize = rect.width / 5f;
            float iconHeightOffset = ( rect.height - iconSize ) / 2;
            Rect rect2 = new Rect( rect.x, rect.y + iconHeightOffset, iconSize, iconSize );
            for ( int i = 0; i < 5; i++ )
            {
                MedicalCareCategory mc = (MedicalCareCategory) i;
                Widgets.DrawHighlightIfMouseover( rect2 );
                GUI.DrawTexture( rect2, CareTextures[i] );
                if ( Widgets.InvisibleButton( rect2 ) )
                {
                    medCare = mc;
                    SoundDefOf.TickHigh.PlayOneShotOnCamera( );
                }
                if ( medCare == mc )
                {
                    GUI.DrawTexture( rect2, Widgets.CheckboxOnTex );
                }
                TooltipHandler.TipRegion( rect2, ( ) => mc.GetLabel( ), 632165 + i * 17 );
                rect2.x += rect2.width;
            }
        }

        public static List<BodyPartRecord> NonMissingParts( Pawn p )
        {
            HediffSet diffSet = p.health.hediffSet;
            List<BodyPartRecord> nonMissingParts = new List<BodyPartRecord>( );
            MissingBodyPartQueue.Clear( );
            MissingBodyPartQueue.Enqueue( p.def.race.body.corePart );
            while ( MissingBodyPartQueue.Count != 0 )
            {
                BodyPartRecord node = MissingBodyPartQueue.Dequeue( );
                if ( !diffSet.PartOrAnyAncestorHasDirectlyAddedParts( node ) )
                {
                    Hediff_MissingPart hediffMissingPart = ( from x in diffSet.GetHediffs<Hediff_MissingPart>( )
                                               where x.Part == node
                                               select x ).FirstOrDefault( );
                    if ( hediffMissingPart == null )
                    {
                        nonMissingParts.Add( node );
                        foreach ( BodyPartRecord t in node.parts ) {
                            MissingBodyPartQueue.Enqueue( t );
                        }
                    }
                }
            }
            return nonMissingParts;
        }

        public static void DoHediffTooltip( Rect rect, Pawn p, PawnCapacityDef capDef )
        {
            StringBuilder tooltip = new StringBuilder( );
            bool tip = false;
            try
            {
                // get parts that matter for this capDef
                List<string> activityGroups = p.RaceProps.body.GetActivityGroups( capDef );
                List<BodyPartRecord> relevantParts = new List<BodyPartRecord>( );
                foreach ( string t in activityGroups ) {
                    relevantParts.AddRange( p.RaceProps.body.GetParts( capDef, t ) );
                }
                relevantParts = relevantParts.Distinct().ToList();

                // the following is an incredible hacky way to show all diffs, but not child nodes of missing body parts
                // if you care about good code, look away.
                // remove missing parts
                relevantParts.RemoveAll(
                    bp => p.health.hediffSet.GetHediffs<Hediff_MissingPart>( ).Select( h => h.Part ).Contains( bp ) );

                // add common ancestors back in
                relevantParts.AddRange( p.health.hediffSet.GetMissingPartsCommonAncestors( ).Select( h => h.Part ) );

                // hediffs with a direct effect listed (CapMods), or affecting a relevant part.
                IEnumerable<Hediff> hediffs = p.health.hediffSet.GetHediffs<Hediff>( ).Where( h => h.Visible &&
                                                                                   ( ( h.CapMods != null &&
                                                                                       h.CapMods.Count > 0 &&
                                                                                       h.CapMods.Any(
                                                                                           cm => cm.capacity == capDef ) ) ||
                                                                                     relevantParts.Contains( h.Part ) ) );
                foreach ( Hediff diff in hediffs )
                {
                    tip = true;
                    tooltip.AppendLine( ( diff.Part == null ? "Whole body" : diff.Part.def.LabelCap ) + ": " +
                                        diff.LabelCap );
                }
            }
            catch ( Exception )
            {
                Log.Message( "Error getting tooltip for medical info." );
            }

            if ( !tip )
            {
                tooltip.AppendLine( "OK" );
            }

            TooltipHandler.TipRegion( rect, tooltip.ToString( ) );
        }

        public static void MedicalCareSetterAll( List<Pawn> pawns )
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>( );
            for ( int i = 0; i < 5; i++ )
            {
                MedicalCareCategory mc = (MedicalCareCategory) i;
                FloatMenuOption option = new FloatMenuOption( mc.GetLabel( ), delegate
                {
                    foreach ( Pawn t in pawns ) {
                        t.playerSettings.medCare = mc;
                    }
                    SoundDefOf.TickHigh.PlayOneShotOnCamera( );
                    MainTabWindow_Medical.IsDirty = true;
                } );
                list.Add( option );
            }
            Find.WindowStack.Add( new FloatMenu( list, true ) );
        }

        public static void RecipeOptionsMaker( Pawn pawn )
        {
            Thing thingForMedBills = pawn;
            List<FloatMenuOption> list = new List<FloatMenuOption>( );
            foreach ( RecipeDef current in thingForMedBills.def.AllRecipes )
            {
                if ( current.AvailableNow )
                {
                    IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients( null );
                    IEnumerable<ThingDef> thingDefs = enumerable as ThingDef[] ?? enumerable.ToArray( );
                    if ( !thingDefs.Any( x => x.isBodyPartOrImplant ) )
                    {
                        IEnumerable<BodyPartRecord> partsToApplyOn = current.Worker.GetPartsToApplyOn( pawn, current );
                        IEnumerable<BodyPartRecord> bodyPartRecords = partsToApplyOn as BodyPartRecord[] ?? partsToApplyOn.ToArray( );
                        if ( bodyPartRecords.Any( ) )
                        {
                            foreach ( BodyPartRecord current2 in bodyPartRecords )
                            {
                                RecipeDef localRecipe = current;
                                BodyPartRecord localPart = current2;
                                string text = localRecipe == RecipeDefOf.RemoveBodyPart ? HealthCardUtility.RemoveBodyPartSpecialLabel( pawn, current2 ) : localRecipe.LabelCap;
                                if ( !current.hideBodyPartNames )
                                {
                                    text = text + " (" + current2.def.label + ")";
                                }
                                Action action = null;
                                if ( thingDefs.Any( ) )
                                {
                                    text += " (";
                                    bool flag = true;
                                    foreach ( ThingDef current3 in thingDefs )
                                    {
                                        if ( !flag )
                                        {
                                            text += ", ";
                                        }
                                        flag = false;
                                        text += "MissingMedicalBillIngredient".Translate( current3.label );
                                    }
                                    text += ")";
                                }
                                else
                                {
                                    action = delegate
                                    {
                                        if (
                                            !Find.MapPawns.FreeColonists.Any( col => localRecipe.PawnSatisfiesSkillRequirements( col ) ) )
                                        {
                                            Bill.CreateNoPawnsWithSkillDialog( localRecipe );
                                        }
                                        Pawn pawn2 = thingForMedBills as Pawn;
                                        if ( pawn2 != null && !pawn.InBed( ) && pawn.RaceProps.Humanlike )
                                        {
                                            if (
                                                !Find.ListerBuildings.allBuildingsColonist.Any( x => x is Building_Bed && ( (Building_Bed) x ).Medical ) )
                                            {
                                                Messages.Message( "MessageNoMedicalBeds".Translate( ),
                                                                  MessageSound.Negative );
                                            }
                                        }
                                        Bill_Medical billMedical = new Bill_Medical( localRecipe );
                                        if ( pawn2 != null ) {
                                            pawn2.BillStack.AddBill( billMedical );
                                            billMedical.Part = localPart;
                                            if ( pawn2.Faction != null && !pawn2.Faction.def.hidden &&
                                                 !pawn2.Faction.HostileTo( Faction.OfColony ) &&
                                                 localRecipe.Worker.IsViolationOnPawn( pawn2, localPart, Faction.OfColony ) )
                                            {
                                                Messages.Message(
                                                    "MessageMedicalOperationWillAngerFaction".Translate( pawn2.Faction ),
                                                    MessageSound.Negative );
                                            }
                                        }
                                    };
                                }
                                list.Add( new FloatMenuOption( text, action ) );
                            }
                        }
                    }
                }
            }
            Find.WindowStack.Add( new FloatMenu( list ) );
        }
    }
}