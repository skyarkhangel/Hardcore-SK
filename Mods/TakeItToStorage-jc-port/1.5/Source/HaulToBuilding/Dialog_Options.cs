using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HaulToBuilding
{
    public class Dialog_Options : Window
    {
        private readonly Func<List<ToggleOption>> getOptions;
        private readonly bool onlyOne;
        private List<ToggleOption> options;
        private Vector2 scrollPosition = new Vector2(0, 0);
        private string searchText = "";

        public Dialog_Options(Func<List<ToggleOption>> getOptions, bool single)
        {
            options = getOptions();
            this.getOptions = getOptions;
            doCloseX = true;
            doCloseButton = true;
            onlyOne = single;
        }

        public override Vector2 InitialSize => new Vector2(620f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            var outRect = new Rect(inRect);
            outRect.yMin += 20f;
            outRect.yMax -= 40f;
            outRect.width -= 16f;
            searchText = Widgets.TextField(outRect.TopPartPixels(35f), searchText);
            outRect.yMin += 40f;
            var shownOptions = options.Where(opt => opt.Label.ToLower().Contains(searchText.ToLower())).ToList();
            var viewRect = new Rect(0f, 0f, outRect.width - 16f,
                shownOptions.Count * 30f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                var y = 0f;
                foreach (var opt in shownOptions)
                {
                    var height = Mathf.Min(Text.CalcHeight(opt.Label, viewRect.width - 24f), 24f);
                    var rect2 = new Rect(0f, y, viewRect.width, height);
                    if (onlyOne)
                    {
                        if (!opt.Enabled) GUI.color = Color.gray;
                        if (Widgets.RadioButtonLabeled(rect2, opt.Label, opt.State) && opt.Enabled)
                        {
                            opt.Toggle();
                            Close();
                            break;
                        }
                    }
                    else
                    {
                        var temp = opt.State;
                        Widgets.CheckboxLabeled(rect2, opt.Label, ref temp, !opt.Enabled);
                        if (opt.State != temp && opt.Enabled)
                        {
                            opt.Toggle();
                            options = getOptions();
                            break;
                        }
                    }

                    GUI.color = Color.white;
                    y += height + 6f;
                }
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }
    }
}