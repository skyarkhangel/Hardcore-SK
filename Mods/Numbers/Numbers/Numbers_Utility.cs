namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public static class Numbers_Utility
    {
        public static bool IsEnemy(this Pawn p)
            => p.HostileTo(Faction.OfPlayer);

        public static bool IsGuest(this Pawn p)
            => p.guest != null
            && !p.guest.IsPrisoner
            && p.Faction != null
            && !p.Faction.HostileTo(Faction.OfPlayer)
            && p.Faction != Faction.OfPlayer;

        public static bool IsVisible(this Pawn p)
            => p.SpawnedOrAnyParentSpawned
            && p.PositionHeld != IntVec3.Invalid
            && !p.PositionHeld.Fogged(p.MapHeld);

        public static bool IsWildAnimal(this Pawn p)
            => p.Faction == null
            && p.AnimalOrWildMan();

        public static DefModExtension_PawnColumnDefs Ext(this PawnColumnDef def, bool logError = true)
        {
            var ext = def.GetModExtension<DefModExtension_PawnColumnDefs>();

            if (logError && ext == null)
            {
                Log.Error($"Numbers expected DefModExtension PawnColumnDefs, got null for def {def.defName}");
            }

            return ext;
        }

        public static DefModExtension_PawnTableDefs Ext(this PawnTableDef def)
        {
            var ext = def.GetModExtension<DefModExtension_PawnTableDefs>();

            if (ext == null)
            {
                Log.Error($"Numbers expected DefModExtension PawnTableDef, got null for def {def.defName}");
            }

            return ext;
        }

        public static string WordWrapAt(this string text, float length, PawnTable table = null)
        {
            if (table != null && !(table is PawnTable_NumbersMain))
                return text.Truncate(length);

            IEnumerable<Pair<char, int>> source = from p in text.Select((c, idx) => new Pair<char, int>(c, idx))
                                                  where p.First == ' '
                                                  select p;
            if (!source.Any())
            {
                return text;
            }
            Pair<char, int> pair = source.MinBy(p => Mathf.Abs(Text.CalcSize(text.Substring(0, p.Second)).x - Text.CalcSize(text.Substring(p.Second + 1)).x));
            return text.Substring(0, pair.Second) + "\n" + text.Substring(pair.Second + 1);
        }

        public static bool InfoCardButton(float x, float y, string text)
        {
            if (InfoCardButtonWorker(x, y))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(text));
                return true;
            }
            return false;
        }

        private static bool InfoCardButtonWorker(float x, float y)
        {
            Rect rect = new Rect(x, y, 24f, 24f);
            TooltipHandler.TipRegion(rect, "DefInfoTip".Translate());
            bool result = Widgets.ButtonImage(rect, StaticConstructorOnGameStart.Info, GUI.color);
            UIHighlighter.HighlightOpportunity(rect, "InfoCard");
            return result;
        }
    }

    [DefOf]
    public class NumbersDefOf
    {
        public static PawnTableDef Numbers_MainTable; //aka Colonists
        public static PawnTableDef Numbers_Enemies;
        public static PawnTableDef Numbers_Prisoners;
        public static PawnTableDef Numbers_Guests;
        public static PawnTableDef Numbers_Animals;
        public static PawnTableDef Numbers_WildAnimals;
        public static PawnTableDef Numbers_Corpses;
        public static PawnTableDef Numbers_AnimalCorpses;
    }
}
