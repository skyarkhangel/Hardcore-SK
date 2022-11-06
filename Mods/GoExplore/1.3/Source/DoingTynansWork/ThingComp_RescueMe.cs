using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace LetsGoExplore
{
    public class ThingComp_RescueMe : ThingComp
    {
        public override void PostDraw()
        {
            base.PostDraw();
            //TODO: Albion, do we want to draw a floating texture to indicate they want rescue? - M.
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            if ((this.parent as Pawn)?.GetLord()?.CurLordToil is LordToil_MillAboutCloseBy)
            {
                if (selPawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly))
                {
                    void setFree()
                    {
                        selPawn.jobs.TryTakeOrderedJob(new Job(DefsOfLGE.OfferRescueLGE, this.parent));
                    }
                    MenuOptionPriority priority = MenuOptionPriority.RescueOrCapture;
                    Thing revalidateClickTarget = this.parent;
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("FreePrisoner".Translate(), setFree, priority, null, revalidateClickTarget), selPawn, this.parent);
                }
                else
                {
                    yield return new FloatMenuOption("CannotGoNoPath".Translate(), null);
                }
            }
        }
    }
}
