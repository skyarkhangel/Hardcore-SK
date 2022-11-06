using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan
{
    internal class MainButton_Toggle : MainButtonWorker
    {
        public override bool Disabled
        {
            get
            {
                this.def.buttonVisible = RocketPrefs.MainButtonToggle;
                return !RocketPrefs.MainButtonToggle
                  && Find.CurrentMap == null
                  && (!def.validWithoutMap || def == MainButtonDefOf.World) || Find.WorldRoutePlanner.Active
                  && Find.WorldRoutePlanner.FormingCaravan
                  && (!def.validWithoutMap || def == MainButtonDefOf.World);
            }
        }

        public override float ButtonBarPercent => RocketPrefs.MainButtonToggle ? base.ButtonBarPercent : 0f;

        public override void Activate()
        {
            if (Event.current.button == 0)
            {
                if (Find.WindowStack.WindowOfType<Window_Main>() != null)
                {
                    Find.WindowStack.RemoveWindowsOfType(typeof(Window_Main));
                    Finder.RocketManWindow = null;
                }
                else
                {
                    Find.WindowStack.Add(
                        Finder.RocketManWindow == null ? Finder.RocketManWindow = new Window_Main() : Finder.RocketManWindow);
                }
            }
            else
            {
                if (Find.WindowStack.WindowOfType<Window_Main>() == null) Find.WindowStack.Add(
                    Finder.RocketManWindow == null ? Finder.RocketManWindow = new Window_Main() : Finder.RocketManWindow);
            }
        }
    }
}