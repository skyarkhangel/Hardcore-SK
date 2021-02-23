using RimWorld;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Analyzer
{
    internal class MainButton_Toggle : MainButtonWorker
    {
        public override bool Disabled
        {
            get
            {
                return Find.CurrentMap == null
                    && (!def.validWithoutMap || def == MainButtonDefOf.World) || Find.WorldRoutePlanner.Active
                    && Find.WorldRoutePlanner.FormingCaravan
                    && (!def.validWithoutMap || def == MainButtonDefOf.World);
            }
        }

        public override void Activate()
        {
            if (Event.current.button == 0)
            {
                if (Find.WindowStack.WindowOfType<Window_Analyzer>() != null)
                {
                    Find.WindowStack.RemoveWindowsOfType(typeof(Window_Analyzer));
                }
                else
                {
                    Find.WindowStack.Add(new Window_Analyzer());
                }
            }
            else
            {
                if (Find.WindowStack.WindowOfType<Window_Analyzer>() == null && Modbase.isPatched)
                {
                    var options = new List<FloatMenuOption>()
                    {
                        new FloatMenuOption("Cleanup", () => Current.Game.GetComponent<GameComponent_Analyzer>().TimeTillCleanup = 0)
                    };

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }
    }
}