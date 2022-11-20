namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Ability : PawnColumnWorker_Icon
    {

        protected override Texture2D GetIconFor(Pawn pawn)
        {
            AbilityDef abilityDef = def.Ext().ability;
            foreach (Ability a in pawn.abilities.abilities)
            {
                if (a.def == abilityDef)
                {
                    return abilityDef.uiIcon;
                }
            }
            return null;
        }

        public override int GetMinWidth(PawnTable table)
        {
            return 26;
        }

        protected override string GetHeaderTip(PawnTable table) => def.Ext().ability.GetTooltip() + "\n\n" + "Numbers_ColumnHeader_Tooltip".Translate();

        public override void DoHeader(Rect rect, PawnTable table)
        {
            Rect interactableHeaderRect = GetInteractableHeaderRect(rect, table);
            if (Mouse.IsOver(interactableHeaderRect))
            {
                Widgets.DrawHighlight(interactableHeaderRect);
                string headerTip = GetHeaderTip(table);
                if (!headerTip.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
                }
            }
            if (Widgets.ButtonInvisible(interactableHeaderRect))
            {
                HeaderClicked(rect, table);
            }

            Texture2D abilityIcon = def.Ext().ability.uiIcon;
            Rect position = new Rect(rect.x, rect.yMax - 26, 26, 26);
            GUI.DrawTexture(position, abilityIcon);
        }
    }
}
