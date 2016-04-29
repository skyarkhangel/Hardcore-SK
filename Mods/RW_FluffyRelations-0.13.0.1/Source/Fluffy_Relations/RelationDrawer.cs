using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Relations
{
    public enum Relation
    {
        None,
        Married,
        ExMarried,
        Engaged,
        ExEngaged,
        Lovers,
        ExLovers,
        FamilyNuclear,
        FamilyBlood,
        FamilyExtention
    }

    public static class RelationDrawer
    {
        #region Fields

        private static Dictionary<Pair<Pawn, Pawn>, Relation> _cachedRelations = new Dictionary<Pair<Pawn, Pawn>, Relation>();
        private static HashSet<Pair<Vector2, Vector2>> _drawnLinks = new HashSet<Pair<Vector2, Vector2>>();

        #endregion Fields

        #region Methods

        public static void DrawLink( this Faction faction, Faction otherFaction, Rect slotA, Rect slotB, bool selected = false )
        {
            // cop out if factions are equal (somehow faction == other sometimes doesn't do the job?)
            if ( faction == otherFaction ||
                 slotA == slotB )
                return;

            // check if link was already drawn from the other pawn (only one link per pawn, with all relevant relations).
            if ( !selected )
            {
                Pair<Vector2, Vector2> link = new Pair<Vector2, Vector2>( slotA.center, slotB.center );
                Pair<Vector2, Vector2> reverseLink = new Pair<Vector2, Vector2>( slotB.center, slotA.center );
                if ( _drawnLinks.Contains( reverseLink ) )
                    return;

                // process this link
                _drawnLinks.Add( link );
            }

            // cop out if no relation
            if ( faction.RelationWith( otherFaction, true ) == null )
                return;

            // get a color for this pair of pawns.
            Color relationColor = GetRelationColor( null, Mathf.RoundToInt( faction.GoodwillWith( otherFaction ) ) );

            // draw the line
            Widgets.DrawLine( slotA.center, slotB.center, relationColor, 3f );
        }

        public static void DrawLink( this Pawn pawn, Pawn otherPawn, Rect slotA, Rect slotB, bool selected = false )
        {
            // check if link was already drawn from the other pawn (only one link per pawn, with all relevant relations).
            if ( !selected )
            {
                Pair<Vector2, Vector2> link = new Pair<Vector2, Vector2>( slotA.center, slotB.center );
                Pair<Vector2, Vector2> reverseLink = new Pair<Vector2, Vector2>( slotB.center, slotA.center );
                if ( _drawnLinks.Contains( reverseLink ) )
                    return;

                // process this link
                _drawnLinks.Add( link );
            }

            // get relations between pawns.
            PawnRelationDef relation = pawn.GetMostImportantRelation( otherPawn );
            int opinion = pawn.relations.OpinionOf( otherPawn );

            // stop if relation not interesting enough
            if ( !selected && relation == null || selected && relation == null && Math.Abs( opinion ) < 10 )
                return;

            // get a color for this pair of pawns.
            // TODO: reconsider colouring relations
            Color relationColor = GetRelationColor( null, opinion );

            // draw the line
            Widgets.DrawLine( slotA.center, slotB.center, relationColor, 3f );

            // get an icon for this relation
            Texture2D relationIcon = GetRelationIcon( relation );
            if ( relationIcon != null )
            {
                Rect iconRect = new Rect( 0f, 0f, Settings.IconSize, Settings.IconSize );
                if ( selected )
                    // if selected, draw icon 4/5ths of the way to the target.
                    iconRect.center = ( slotA.center + 4 * slotB.center ) / 5f;
                else
                    iconRect.center = ( slotA.center + slotB.center ) / 2f;

                GUI.DrawTexture( iconRect, relationIcon );
            }
        }

        public static Color GetRelationColor( PawnRelationDef relation, int opinion )
        {
            if ( relation != null )
            {
                // lovers
                if ( relation == PawnRelationDefOf.Spouse ||
                     relation == PawnRelationDefOf.Lover ||
                     relation == PawnRelationDefOf.Fiance )
                    return Resources.ColorLover;

                // ex lovers
                if ( relation == PawnRelationDefOf.ExSpouse ||
                     relation == PawnRelationDefOf.ExLover )
                    return Resources.ColorExLover;

                // (blood) family
                if ( relation.familyByBloodRelation )
                    return Resources.ColorFamily;
            }

            // default: opinion
            Color maxColor = opinion > 0 ? Resources.ColorFriend : Resources.ColorEnemy;
            return Color.Lerp( Resources.ColorNeutral, maxColor, Math.Abs( opinion ) / 100f );
        }

        public static Texture2D GetRelationIcon( PawnRelationDef relation )
        {
            if ( relation != null )
            {
                // lovers
                if ( relation == PawnRelationDefOf.Spouse )
                    return Resources.IconMarried;
                if ( relation == PawnRelationDefOf.Lover )
                    return Resources.IconLover;
                if ( relation == PawnRelationDefOf.Fiance )
                    return Resources.IconFiancee;

                //// ex lovers
                //if ( relation == PawnRelationDefOf.ExSpouse ||
                //     relation == PawnRelationDefOf.ExLover )
                //    return Resources.ColorExLover;

                // (blood) family
                if ( relation.familyByBloodRelation )
                    return Resources.IconFamilyBlood;
            }

            return null;
        }

        public static void RecacheRelations()
        {
            _cachedRelations = new Dictionary<Pair<Pawn, Pawn>, Relation>();
        }

        public static void ResetForNextTick()
        {
            _drawnLinks.Clear();
        }

        #endregion Methods
    }
}