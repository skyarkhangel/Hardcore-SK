using UnityEngine;
using Verse;

namespace PeteTimesSix.SimpleSidearms.UI
{
    public static class UIComponents
    {
        private static readonly Color BadValueOutlineColor = new Color(.9f, .1f, .1f, 1f);


        public static void DrawBadTextValueOutline(Rect rect)
        {
            var prevColor = GUI.color;
            GUI.color = BadValueOutlineColor;
            Widgets.DrawBox(rect);
            GUI.color = prevColor;
        }
    }
}
