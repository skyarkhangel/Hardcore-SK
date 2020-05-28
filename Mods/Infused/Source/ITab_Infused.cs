using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace Infused
{
    public class ITab_Infused : ITab
    {
        static Vector2 scrollPos;

        public override bool IsVisible => SelThing?.TryGetComp<CompInfused>()?.IsActive ?? false;

        public ITab_Infused()
        {
            size = new Vector2(400f, 550f);
            labelKey = "Infused.Tab";
        }

        protected override void FillTab()
        {
            var selected = SelThing.TryGetComp<CompInfused>();

            var container = new Rect(0f, 0f, size.x, size.y).ContractedBy(16f);

            // Label
            var labelView = new Rect(container.xMin, container.yMin, container.xMax * 0.85f, container.yMax);
            float labelHeight = DrawLabel(selected, labelView) + 4f;

            // Quality
            var qualityView = new Rect(container.xMin, labelView.yMin + labelHeight, container.width, container.yMax - labelHeight);
            float qualityHeight = DrawQuality(selected, qualityView);

            // Infusion descriptions
            var descView = new Rect(container.xMin, qualityView.yMin + qualityHeight, container.xMax - 6.0f, container.yMax - qualityView.yMin - qualityHeight);
            DrawInfusions(selected, descView);
        }

        static float DrawLabel(CompInfused comp, Rect view)
        {
            string label = comp.InfusedLabel;

            GUI.color = comp.InfusedLabelColor;
            Text.Font = GameFont.Medium;

            Widgets.Label(view, label);

            return Text.CalcHeight(label, view.width);
        }

        static float DrawQuality(CompInfused comp, Rect view)
        {
            comp.parent.TryGetQuality(out QualityCategory qc);

            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            var subLabelBuilder = new StringBuilder();
            subLabelBuilder.Append(qc.GetLabel().CapitalizeFirst())
                    .Append(" ")
                    .Append(ResourceBank.Strings.Quality)
                    .Append(" ");

            if (comp.parent.Stuff != null)
            {
                subLabelBuilder.Append(comp.parent.Stuff.LabelAsStuff).Append(" ");
            }

            subLabelBuilder.Append(comp.parent.def.label);

            string subLabel = subLabelBuilder.ToString();

            Widgets.Label(view, subLabel);

            return Text.CalcHeight(subLabel, view.width);
        }

        static void DrawInfusions(CompInfused comp, Rect view)
        {
            var infusions = comp.Infusions;

            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            float scrollerWidth = view.width - 20.0f;
            float totalHeight = 0;

            StringBuilder sb = new StringBuilder();
            foreach (var i in comp.Infusions)
            {
                sb.AppendLine(i.DescriptionLabel);
                sb.AppendLine(i.DescriptionStats);
                sb.Append(i.DescriptionExtras);
            }
            totalHeight = Text.CalcHeight(sb.ToString(), scrollerWidth) + 16f * comp.InfusionCount;

            var scroller = new Rect(0.0f, 0.0f, scrollerWidth, totalHeight);
            Widgets.BeginScrollView(view, ref scrollPos, scroller);

            float yOffset = 0f;
            foreach(var i in comp.Infusions)
            {
                yOffset += DrawInfusion(i, scroller, yOffset);
            }

            Widgets.EndScrollView();
        }

        static float DrawInfusion(Def infusion, Rect view, float yOffset)
        {
            float contentsHeight = Text.CalcHeight(infusion.DescriptionLabel + "\n" + infusion.DescriptionStats + infusion.DescriptionExtras, view.width);
            var container = new Rect(view.x, yOffset, view.width, contentsHeight);

            var body = new Rect(container.x, container.y + 8f, container.xMax, container.yMax - 8f);

            if (Mouse.IsOver(container))
            {
                GUI.color = Color.white;
                GUI.DrawTexture(container, TexUI.HighlightTex);
            }

            GUI.color = ResourceBank.Colors.InfusionColor(infusion.tier);
            Widgets.Label(body, infusion.DescriptionLabel);
            body.y += Text.CalcHeight(infusion.DescriptionLabel, view.width);

            GUI.color = Color.white;
            Widgets.Label(body, infusion.DescriptionStats);
            body.y += Text.CalcHeight(infusion.DescriptionStats, view.width);

            GUI.color = ResourceBank.Colors.Uncommon;
            Widgets.Label(body, infusion.DescriptionExtras);

            return contentsHeight;
        }

        public override void OnOpen()
        {
            scrollPos = Vector2.zero;
        }
    }
}
