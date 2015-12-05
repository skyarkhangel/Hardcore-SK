using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fluffy
{
    public static class Widgets_Animals
    {
        public static readonly Texture2D[] TrainingTextures = {
            ContentFinder<Texture2D>.Get("UI/Training/obedience"),
            ContentFinder<Texture2D>.Get("UI/Training/release"),
            ContentFinder<Texture2D>.Get("UI/Training/rescue"),
            ContentFinder<Texture2D>.Get("UI/Training/haul")
        };

        public static readonly Texture2D CheckWhite = ContentFinder<Texture2D>.Get("UI/Buttons/CheckOnWhite");

        public static readonly Texture2D BarBg = SolidColorMaterials.NewSolidColorTexture(0f, 1f, 0f, 0.8f);

        public static List<TrainableDef> Trainables
        {
            get
            {
                return TrainableUtility.TrainableDefsInListOrder;
            }
        }

        public static void SlaughterAnimal(Pawn pawn)
        {
            Find.DesignationManager.AddDesignation(new Designation(pawn, DesignationDefOf.Slaughter));
        }

        public static void UnSlaughterAnimal(Pawn pawn)
        {
            Find.DesignationManager.DesignationOn(pawn, DesignationDefOf.Slaughter).Delete();
        }

        public static void SlaughterAllAnimals(List<Pawn> pawns)
        {
            if (pawns.All(p => Find.DesignationManager.DesignationOn(p, DesignationDefOf.Slaughter) != null))
            { 
                SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                for (int i = 0; i < pawns.Count; i++)
                {
                    UnSlaughterAnimal(pawns[i]);
                }
            } else
            {
                SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                for (int i = 0; i < pawns.Count; i++)
                {
                    if (Find.DesignationManager.DesignationOn(pawns[i], DesignationDefOf.Slaughter) == null) SlaughterAnimal(pawns[i]);
                }
            }
        }

        public static void DoAllowedAreaHeaders(Rect rect, List<Pawn> pawns, AllowedAreaMode mode)
        {
            List<Area> allAreas = Find.AreaManager.AllAreas;
            int num = 1;
            for (int i = 0; i < allAreas.Count; i++)
            {
                if (allAreas[i].AssignableAsAllowed(mode))
                {
                    num++;
                }
            }
            float num2 = rect.width / num;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerCenter;
            Rect rect2 = new Rect(rect.x, rect.y, num2, rect.height);
            Widgets.Label(rect2, "NoAreaAllowed".Translate());
            if (Widgets.InvisibleButton(rect2))
            {
                for (int i = 0; i < pawns.Count; i++)
                {
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
                    pawns[i].playerSettings.AreaRestriction = null;
                }
            }
            if (Mouse.IsOver(rect2))
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
            }
            TooltipHandler.TipRegion(rect2, "NoAreaAllowed".Translate());
            int num3 = 1;
            foreach (Area area in allAreas)
            {
                if (area.AssignableAsAllowed(mode))
                {
                    float num4 = num3 * num2;
                    Rect rect3 = new Rect(rect.x + num4, rect.y, num2, rect.height);
                    Widgets.Label(rect3, area.Label);
                    if (Widgets.InvisibleButton(rect3))
                    {
                        foreach (Pawn p in pawns)
                        {
                            SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
                            p.playerSettings.AreaRestriction = area;
                        }
                    }
                    TooltipHandler.TipRegion(rect3, "Fluffy.RestrictAllTo".Translate(area.Label));
                    if (Mouse.IsOver(rect3))
                    {
                        GUI.DrawTexture(rect3, TexUI.HighlightTex);
                    }
                    num3++;
                }
            }
            Text.WordWrap = true;
        }

        public static void DoTrainingHeaders(Rect rect, List<Pawn> pawns)
        {
            List<TrainableDef> trainables = TrainableUtility.TrainableDefsInListOrder;
            float width = rect.width / trainables.Count;
            float iconSize = 16f;
            float widthOffset = (width - iconSize) / 2;
            float heightOffset = (rect.height - iconSize) / 2;
            float x = rect.xMin;
            float y = rect.yMin;

            for (int i = 0; i < trainables.Count; i++)
            {
                Rect bg = new Rect(x, y, width, rect.height);
                Rect icon = new Rect(x + widthOffset, y + heightOffset, iconSize, iconSize);
                x += width;
                if (Mouse.IsOver(bg))
                {
                    GUI.DrawTexture(bg, TexUI.HighlightTex);
#if DEBUG
                    Log.Message(trainables[i].label);
#endif
                }
                var tooltip = new StringBuilder();
                tooltip.AppendLine( "Fluffy.SortByTrainables".Translate( trainables[i].LabelCap ) );
                tooltip.AppendLine( "Fluffy.ShiftToTrainAll".Translate() ).AppendLine()
                       .Append( trainables[i].description );
                TooltipHandler.TipRegion(bg, tooltip.ToString());
                GUI.DrawTexture(icon, TrainingTextures[i]);
                if(Widgets.InvisibleButton(bg))
                {
                    if (!Event.current.shift)
                    {
                        if (MainTabWindow_Animals.Order == MainTabWindow_Animals.Orders.Training && MainTabWindow_Animals.TrainingOrder == trainables[i])
                        {
                            MainTabWindow_Animals.Asc = !MainTabWindow_Animals.Asc;
                        }
                        else
                        {
                            MainTabWindow_Animals.Order = MainTabWindow_Animals.Orders.Training;
                            MainTabWindow_Animals.Asc = false;
                            MainTabWindow_Animals.TrainingOrder = trainables[i];
                        }
                    } 
                    else if (Event.current.shift)
                    {
                        ToggleAllTraining(trainables[i], pawns);
                    }
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    MainTabWindow_Animals.IsDirty = true;
                }
            }
        }

        public static void DoTrainingRow(Rect rect, Pawn pawn)
        {
            List<TrainableDef> trainableDefs = TrainableUtility.TrainableDefsInListOrder;
            float width = rect.width / trainableDefs.Count;
            float iconSize = 16f;
            float widthOffset = (width - iconSize) / 2;
            float heightOffset = (rect.height - iconSize) / 2;
            float x = rect.xMin;
            float y = rect.yMin;

            for (int i = 0; i < trainableDefs.Count; i++)
            {
                Rect bg = new Rect(x, y, width, rect.height);
                Rect icon = new Rect(x + widthOffset, y + heightOffset, iconSize, iconSize);
                x += width;
                bool vis;
                AcceptanceReport report = pawn.training.CanAssignToTrain(trainableDefs[i], out vis);
                TooltipHandler.TipRegion(bg, GetTrainingTip(pawn, trainableDefs[i], report));
                if (vis)
                {
                    DrawTrainingButton(icon, pawn, trainableDefs[i], report);
                    if (report.Accepted && !pawn.training.IsCompleted(trainableDefs[i]))
                    {
                        if (Widgets.InvisibleButton(bg))
                        {
                            ToggleTraining(trainableDefs[i], pawn, report);
                        }
                        if (Mouse.IsOver(icon))
                        {
                            GUI.DrawTexture(icon, TexUI.HighlightTex);
                        }
                    }
                }
            }
        }

        public static void ToggleTraining(TrainableDef td, Pawn pawn, AcceptanceReport ar)
        {
            bool train = !pawn.training.GetWanted(td);
            if (ar.Accepted)
            {
                SetWantedRecursive(td, pawn, train);
            }
        }

        public static void DrawTrainingButton(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport ar)
        {
            if (ar.Accepted)
            {
                if (pawn.training.IsCompleted(td))
                {
                    GUI.DrawTexture(rect, CheckWhite);
                }
                else if (pawn.training.GetWanted(td))
                {
                    GUI.DrawTexture(rect, Widgets.CheckboxOnTex);

                    // Get method named "GetSteps", which is internal(non-public) and not static(instance)
                    var getSteps = typeof(Pawn_TrainingTracker).GetMethod( "GetSteps",
                                                                           BindingFlags.NonPublic |
                                                                           BindingFlags.Instance );
                    if (getSteps == null)
                    {
#if DEBUG
                        Log.Error("GetSteps is null!");
#endif
                        return;
                    }

                    // Call "GetSteps" from instance pawn.training, parameter is td.
                    var curSteps = getSteps.Invoke( pawn.training, new object[] {td} );
                    int steps = td.steps;
                    // Return value of Invoke(...) is Object; thus casting into int type.
                    float barHeight = (rect.height/steps)*(int) curSteps;
                    Rect bar = new Rect( rect.xMax - 5f, rect.yMax - barHeight, 3f, barHeight );
                    GUI.DrawTexture( bar, BarBg );
                }
            }
        }

        public static string GetTrainingTip(Pawn pawn, TrainableDef td, AcceptanceReport ar)
        {
            var label = new StringBuilder();
            label.AppendLine(td.LabelCap);
            if (!ar.Accepted)
            {
                label.AppendLine(ar.Reason);
            }
            else
            {
                if (pawn.training.IsCompleted(td))
                {
                    label.Append("Fluffy.TrainingCompleted".Translate());
                }
                else
                {
                    label.AppendLine(!pawn.training.GetWanted(td)
                        ? "Fluffy.NotTraining".Translate()
                        : "Fluffy.CurrentlyTraining".Translate());

                    var getSteps = typeof(Pawn_TrainingTracker).GetMethod("GetSteps",
                                                                           BindingFlags.NonPublic |
                                                                           BindingFlags.Instance);
                    if (getSteps == null)
                    {
#if DEBUG
                        Log.Error("GetSteps is null!");
#endif
                        return label.ToString();
                    }
                    
                    var curSteps = getSteps.Invoke(pawn.training, new object[] { td });
                    int steps = td.steps;
                    label.Append( "Fluffy.StepsCompleted".Translate( curSteps, steps ) );
                }
            }
            return label.ToString();
        }

        public static void ToggleAllTraining(TrainableDef td, List<Pawn> pawns)
        {
            bool[] visible = new bool[pawns.Count];
            AcceptanceReport[] canAssign = new AcceptanceReport[pawns.Count];
            bool[] assigned = pawns.Select(p => p.training.GetWanted(td)).ToArray();
            bool[] trained = pawns.Select(p => p.training.IsCompleted(td)).ToArray();
            bool all = true;

            for (int i = 0; i < pawns.Count; i++)
            {
                canAssign[i] = pawns[i].training.CanAssignToTrain(td, out visible[i]);
                if (!assigned[i] && !trained[i] && canAssign[i].Accepted) all = false;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                if (all && assigned[i])
                {
                    SetWantedRecursive(td, pawns[i], false);
                } else if (!assigned[i] && canAssign[i].Accepted && !trained[i])
                {
                    SetWantedRecursive(td, pawns[i], true);
                }
            }            
        }

        private static void SetWantedRecursive(TrainableDef td, Pawn pawn, bool checkOn)
        {
            pawn.training.SetWanted(td, checkOn);
            if (checkOn)
            {

                SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                if (td.prerequisites != null)
                {
                    foreach (TrainableDef trainable in td.prerequisites)
                    {
                        SetWantedRecursive(trainable, pawn, true);
                    }
                }
            }
            else
            {
                SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                IEnumerable<TrainableDef> enumerable = from t in DefDatabase<TrainableDef>.AllDefsListForReading
                                                       where t.prerequisites != null && t.prerequisites.Contains(td)
                                                       select t;
                foreach (TrainableDef current in enumerable)
                {
                    SetWantedRecursive(current, pawn, false);
                }
            }
        }
    }
}
