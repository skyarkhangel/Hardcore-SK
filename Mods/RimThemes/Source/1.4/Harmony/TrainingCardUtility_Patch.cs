using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using HarmonyLib;

namespace aRandomKiwi.RimThemes
{
    /*
     * PATCH use to enlarge the learning mastery agichage space which can mess depending on the font size
     */
    [HarmonyPatch(typeof(TrainingCardUtility), "TryDrawTrainableRow"), StaticConstructorOnStartup]
    class TrainingCardUtility_FinalizeSaving_Patch
    {

        [HarmonyPrefix]
        static bool Prefix(ref bool __result,Rect rect, Pawn pawn, TrainableDef td)
        {
            try
            {
                bool flag = pawn.training.HasLearned(td);
                bool flag2;
                AcceptanceReport canTrain = pawn.training.CanAssignToTrain(td, out flag2);
                if (!flag2)
                {
                    __result = false;
                    return false;
                }
                Widgets.DrawHighlightIfMouseover(rect);
                Rect rect2 = rect;
                rect2.width -= 70f;
                rect2.xMin += (float)td.indent * 10f;
                Rect rect3 = rect;
                rect3.xMin = rect3.xMax - 70f + 17f;
                TrainingCardUtility.DoTrainableCheckbox(rect2, pawn, td, canTrain, true, false);
                if (flag)
                {
                    GUI.color = Color.green;
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect3, ((DefMap<TrainableDef, int>)Traverse.Create(pawn.training).Field("steps").GetValue())[td] + " / " + td.steps);
                Text.Anchor = TextAnchor.UpperLeft;
                if (DebugSettings.godMode && !pawn.training.HasLearned(td))
                {
                    Rect rect4 = rect3;
                    rect4.yMin = rect4.yMax - 10f;
                    rect4.xMin = rect4.xMax - 10f;
                    if (Widgets.ButtonText(rect4, "+", true, false, true))
                    {
                        pawn.training.Train(td, pawn.Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>(), false);
                    }
                }
                //TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
                TooltipHandler.TipRegion(rect, delegate
                {
                    string text = td.LabelCap + "\n\n" + td.description;
                    if (!canTrain.Accepted)
                    {
                        text = text + "\n\n" + canTrain.Reason;
                    }
                    else if (!td.prerequisites.NullOrEmpty<TrainableDef>())
                    {
                        text += "\n";
                        for (int i = 0; i < td.prerequisites.Count; i++)
                        {
                            if (!pawn.training.HasLearned(td.prerequisites[i]))
                            {
                                text = text + "\n" + "TrainingNeedsPrerequisite".Translate(td.prerequisites[i].LabelCap);
                            }
                        }
                    }
                    return text;
                }, (int)(rect.y * 612f + rect.x));

                GUI.color = Color.white;
                __result = true;
                return false;
            }
            catch(Exception e)
            {
                Themes.LogError("TrainingCardUtility Patch : " + e.Message);
                return true;
            }
        }
    }
}
