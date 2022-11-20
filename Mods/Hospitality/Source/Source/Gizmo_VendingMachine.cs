using System.Linq;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public class Gizmo_VendingMachine : Gizmo_ModifyNumber<CompVendingMachine>
    {
        private readonly CompVendingMachine vendingMachine;

        public Gizmo_VendingMachine(CompVendingMachine[] vendingMachines) : base(vendingMachines)
        {
            vendingMachine = vendingMachines.First();
        }

        public override bool GroupsWith(Gizmo other) => false;

        protected override Color ButtonColor { get; } = new Color(249 / 256f, 178 / 256f, 86 / 256f);

        protected override string Title => "Hospitality_VendingMachine".Translate();

        protected override void ButtonDown() => vendingMachine.CurrentPrice -= vendingMachine.Properties.priceSteps;

        protected override void ButtonUp() => vendingMachine.CurrentPrice += vendingMachine.Properties.priceSteps;

        protected override void ButtonCenter() => vendingMachine.SetAutoPricing();

        protected override void DrawInfoRect(Rect rect)
        {
            LabelRow(ref rect, "Hospitality_VendingMachinePrice".Translate(), ((float)vendingMachine.CurrentPrice).ToStringMoney("F0"));
            GUI.color = ButtonColor;
            LabelRow(ref rect, "Hospitality_VendingMachineContains".Translate(), ((float)vendingMachine.TotalSold).ToStringMoney("F0"));
            GUI.color = Color.white;
        }

        protected override void DrawTooltipBox(Rect totalRect) { }
    }
}
