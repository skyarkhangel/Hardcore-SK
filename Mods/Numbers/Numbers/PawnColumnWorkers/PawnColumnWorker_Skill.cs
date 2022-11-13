namespace Numbers
{
    using System.Reflection;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    //mostly from Koisama
    public class PawnColumnWorker_Skill : PawnColumnWorker
    {

        private static readonly Texture2D passionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");
        private static readonly Texture2D passionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");
        private static readonly Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.25f));
        private static readonly Texture2D SkillBarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.07f));
        private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

        private static readonly MethodInfo mGetSkillDescription = typeof(SkillUI).GetMethod("GetSkillDescription", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, new[] { typeof(SkillRecord) }, null);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.RaceProps.IsMechanoid)
                return;
            if (pawn.RaceProps.Animal)
                return;

            SkillRecord skill = pawn.skills?.GetSkill(def.Ext().skill);

            if (skill == null)
                return;

            GUI.BeginGroup(rect);
            Rect position = new Rect(3f, 3f, 24f, 24f);
            if (skill.passion > Passion.None)
            {
                Texture2D image = (skill.passion != Passion.Major) ? passionMinorIcon : passionMajorIcon;
                GUI.DrawTexture(position, image);
            }
            if (!skill.TotallyDisabled)
            {
                Rect rect3 = new Rect(position.xMax, 0f, rect.width - position.xMax, rect.height);
                Widgets.FillableBar(rect3, skill.Level / 20f, SkillBarFillTex, SkillBarBgTex, false);
            }
            Rect rect4 = new Rect(position.xMax + 4f, 0f, 999f, rect.height);
            rect4.yMin += 3f;
            string label;
            if (skill.TotallyDisabled)
            {
                GUI.color = DisabledSkillColor;
                label = "-";
            }
            else
            {
                label = skill.Level.ToStringCached();
            }
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Widgets.Label(rect4, label);
            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            GUI.EndGroup();
            string tip = GetTip(pawn, skill);
            if (!tip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tip);
            }
        }

        protected string GetTip(Pawn pawn, SkillRecord skill) => (string)mGetSkillDescription.Invoke(null, new[] { skill });

        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 120);

        public override int Compare(Pawn a, Pawn b)
        {
            return (a.skills?.GetSkill(def.Ext().skill)?.XpTotalEarned ?? 0).CompareTo(b.skills?.GetSkill(def.Ext().skill)?.XpTotalEarned ?? 0);
        }
    }
}
