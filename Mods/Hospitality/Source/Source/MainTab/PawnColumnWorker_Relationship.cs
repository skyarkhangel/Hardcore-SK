using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Relationship : PawnColumnWorker_Icon
    {
        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Tab/Relationship");
        private static Dictionary<int, RelationInfo> relationCache = new Dictionary<int, RelationInfo>();
        private bool mayDrawLordGroups;

        private class RelationInfo
        {
            public bool hasRelationship;
            public string tooltip;
            public int lastUpdateTick;
        }

        private struct Relation
        {
            public Pawn colonist;
            public PawnRelationDef def;
        }

        protected override Texture2D GetIconFor(Pawn pawn)
        {
            if (pawn == null) return null;
            return GetRelationInfo(pawn).hasRelationship ? Icon : null;
        }

        private static RelationInfo GetRelationInfo(Pawn pawn)
        {
            if (!relationCache.TryGetValue(pawn.thingIDNumber, out var info)) return CreateRelationInfo(pawn);

            // Update infos twice per minute
            if (GenTicks.TicksGame - GenTicks.TickLongInterval > info.lastUpdateTick)
                UpdateInfo(info, pawn);

            return info;
        }

        private static RelationInfo CreateRelationInfo(Pawn guest)
        {
            var info = new RelationInfo();
            UpdateInfo(info, guest);
            relationCache.Add(guest.thingIDNumber, info);
            return info;
        }

        private static void UpdateInfo(RelationInfo info, Pawn guest)
        {
            var relations = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(p => p.relations != null).SelectMany(p => GetRelations(guest, p)).ToArray();
            info.hasRelationship = relations.Length > 0;
            info.lastUpdateTick = GenTicks.TicksGame;
            var sb = new StringBuilder();
            foreach (var relation in relations)
            {
                sb.AppendLine("Relationship".Translate(relation.def.GetGenderSpecificLabelCap(guest), relation.colonist.Label, relation.colonist));
            }

            info.tooltip = sb.ToString();
        }

        private static IEnumerable<Relation> GetRelations(Pawn guest, Pawn p)
        {
            return p.GetRelations(guest).Select(relationDef => new Relation {colonist = p, def = relationDef});
        }

        protected override string GetIconTip(Pawn pawn)
        {
            return GetRelationInfo(pawn).tooltip;
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            base.DoCell(rect, pawn, table);

            // Only run once
            if (!mayDrawLordGroups) return;
            mayDrawLordGroups = false;

            MainTabUtility.DrawLordGroups(rect, table, pawn);
        }

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            mayDrawLordGroups = true;
        }
    }
}
