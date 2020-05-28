using HugsLib.Settings;
using UnityEngine;
using Resources = MorePlanning.Common.Resources;
using Multiplayer.API;

namespace MorePlanning.Plan
{
    public class PlanColorManager
    {
        public const int NumPlans = 10;

        public static Color[] PlanColor = new Color[NumPlans];

        private static SettingHandle<string>[] _planColorSetting = new SettingHandle<string>[NumPlans];

        public static readonly string[] DefaultColors = new string[] {
            "a9a9a9",
            "2095f2",
            "4bae4f",
            "f34235",
            "feea3a",
            "ff00f0",
            "00fffc",
            "8400ff",
            "ffa200",
            "000000"
        };

        private static string GetDefaultColor(int i)
        {
            return DefaultColors[i];
        }

        public static void Load(ModSettingsPack settings)
        {
            for (int i = 0; i < NumPlans; i++)
            {
                _planColorSetting[i] = settings.GetHandle("planColor" + i, "planColor" + i, "planColor" + i, GetDefaultColor(i));
                _planColorSetting[i].NeverVisible = true;
            }

            for (int i = 0; i < NumPlans; i++)
            {
                ColorChanged(i);
            }
        }

        [SyncMethod]
        public static void ChangeColor(int colorNum, string hexColor)
        {
            _planColorSetting[colorNum].Value = hexColor;
            ColorChanged(colorNum);
        }

        private static void ColorChanged(int numColor)
        {
            ColorUtility.TryParseHtmlString("#" + _planColorSetting[numColor], out var color);

            PlanColor[numColor] = color;

            color.a = MorePlanningMod.Instance.ModSettings.PlanOpacity / 100f;
            Resources.PlanMatColor[numColor].SetColor("_Color", color);
        }
    }
}
