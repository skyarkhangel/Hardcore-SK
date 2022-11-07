using System.Linq;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public class Gizmo_GuestBed : Gizmo_ModifyNumber<Building_GuestBed>
    {
        private readonly TaggedString attractiveness;
        private readonly TaggedString rentalFee;

        public Gizmo_GuestBed(Building_GuestBed[] beds) : base(beds)
        {
            if (beds.Length == 1)
            {
                var bed = beds.First();
                Title = bed.Stats.title;
                attractiveness = bed.Stats.attractiveness.ToString();
                rentalFee = ToStringMoney(bed.RentalFee);
            }
            else
            {
                var bed = beds.First();
                Title = bed.def.LabelCap;
                attractiveness = ToFromToString(b => b.Stats.attractiveness, i => i.ToString());
                rentalFee = ToFromToString(b => b.RentalFee, i => ToStringMoney(i));
            }
        }

        private static string ToStringMoney(float number) => number.ToStringMoney("F0");

        protected override Color ButtonColor { get; } = new Color(249 / 256f, 178 / 256f, 86 / 256f);

        protected override string Title { get; }

        protected override void ButtonDown()
        {
            foreach (var bed in selection) bed.RentalFee -= Building_GuestBed.FeeStep;
        }

        protected override void ButtonUp()
        {
            foreach (var bed in selection) bed.RentalFee += Building_GuestBed.FeeStep;
        }

        protected override void ButtonCenter()
        {
            foreach (var bed in selection) bed.RentalFee = 30;
        }

        protected override void DrawInfoRect(Rect rect)
        {
            LabelRow(ref rect, "Hospitality_VendingMachinePrice".Translate(), rentalFee);

            LabelRow(ref rect, "BedGizmoAttractiveness".Translate(), attractiveness);
        }

        protected override void DrawTooltipBox(Rect totalRect)
        {
            // Royal title info box
            if (!Mouse.IsOver(totalRect) || !ModsConfig.RoyaltyActive) return;
            if (selection.Length > 1) return;

            var windowRect = BedStatsDrawer.GetWindowRect();
            Find.WindowStack.ImmediateWindow(74975, windowRect, WindowLayer.Super, () =>
            {
                // This is only going to have one instance, even if multiple beds are selected, thus it will only calculate
                // the stats for one bed at a given time, and this is cached to only update once per second
                selection[0].UpdateRoyaltyStats();
                BedStatsDrawer.DoBedInfos(windowRect, selection[0]);
            });
        }
    }
}