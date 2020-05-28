using MorePlanning.Plan;
using UnityEngine;
using Verse;

namespace MorePlanning.Common
{
    [StaticConstructorOnStartup]
    class Resources
    {
        public static readonly Texture2D IconVisible = ContentFinder<Texture2D>.Get("UI/PlanVisible");
        public static readonly Texture2D IconInvisible = ContentFinder<Texture2D>.Get("UI/PlanInvisible");

        public static readonly Texture2D ToolBoxColor = ContentFinder<Texture2D>.Get("UI/ToolBoxColor");
        public static readonly Texture2D ToolBoxColorSelected = ContentFinder<Texture2D>.Get("UI/ToolBoxColorSelected");

        public static readonly Texture2D Plan = ContentFinder<Texture2D>.Get("UI/Plan");

        public static readonly Texture2D RemoveIcon = ContentFinder<Texture2D>.Get("UI/RemoveIcon");
        public static readonly Texture2D PlanToolRemoveAll = ContentFinder<Texture2D>.Get("UI/PlanToolRemoveAll");

        public static readonly Texture2D ColorPickerSelect = ContentFinder<Texture2D>.Get("UI/ColorPickerSelect");

        public static readonly Texture2D ColorPickerOverlay = ContentFinder<Texture2D>.Get("UI/ColorPickerOverlay");
        public static readonly Texture2D HsvSlider = ContentFinder<Texture2D>.Get("UI/HsvSlider");

        public static Material[] PlanMatColor = new Material[PlanColorManager.NumPlans];

        public static PlanDesignationDef PlanDesignationDef;

        static Resources()
        {
            for (int i = 0; i < PlanMatColor.Length; i++)
            {
                Color c = new Color(0, 0, i);
                PlanMatColor[i] = MaterialPool.MatFrom("UI/PlanBase" + i, ShaderDatabase.MetaOverlay, c);
            }
        }
    }
}
