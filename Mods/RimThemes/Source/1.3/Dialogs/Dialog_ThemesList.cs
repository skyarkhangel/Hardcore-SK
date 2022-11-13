using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text.RegularExpressions;

namespace aRandomKiwi.RimThemes
{
    public class Dialog_ThemesList : Window
    {
        protected float bottomAreaHeight;

        protected Vector2 scrollPosition = Vector2.zero;

        protected string typingName = string.Empty;

        private static string filter;

        private static string selCategorieID = "all";

        protected const float EntryHeight = 100f;

        private static readonly Color titleColor = new Color(1f, 1f, 0.6f);

        private static readonly Color descriptionColor = Color.white;

        public static string[] categoriesID = { "all", "future", "medieval", "nature", "horror","wtf","games","historic","technology","other"};

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(620f, 750f);
            }
        }

        public Dialog_ThemesList()
        {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Display sponsor heading
            Text.Font = GameFont.Medium;
            TextAnchor prev = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect sponsorRect = new Rect(8f, 15f, inRect.width - 10f, 40f);
            if (Widgets.ButtonText(sponsorRect, "RimTheme_SupportedBy".Translate( Utils.sponsor ) + " <3", true, true))
            {
                Application.OpenURL("https://ko-fi.com/arandomkiwi");
            }
            TooltipHandler.TipRegion(sponsorRect, "RimTheme_BecomeASponsor".Translate());
            Text.Anchor = prev;
            Text.Font = GameFont.Tiny;

            //Display search text fields
            Rect searchRect = new Rect(8f, 65f, inRect.width - 16f - 150f, 25f);
            filter = Widgets.TextField(searchRect, filter);

            Rect catRect = new Rect(inRect.width - 16f - 150f + 15f, 65f, 150f, 26f);
            if(Widgets.ButtonText(catRect, ("RimThemes_Cat" + selCategorieID.CapitalizeFirst()).Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                for(int i=0;i!=categoriesID.Length;i++)
                {
                    string scat = categoriesID[i];
                    string scatID = categoriesID[i];
                    list.Add(new FloatMenuOption(("RimThemes_Cat"+categoriesID[i].CapitalizeFirst()).Translate(), delegate
                    {
                        selCategorieID = scatID;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Vector2 vector = new Vector2(inRect.width - 16f, 100f);
            inRect.height -= 95f;
            float y = vector.y;
            //Window size adaptation
            float height = (float)getNbMatchingThemes(filter) * y;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
            Rect outRect = new Rect(inRect.AtZero());
            outRect.height -= (this.bottomAreaHeight+50);
            outRect.y += 100f;
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float num = 0f;
            int num2 = 0;
            string modID,modName,modNameDisplay;
            Dictionary<string,string> curMod;

            var items = from pair in Themes.DBTex
                        orderby pair.Key.Split('§')[1] ascending
                        select pair;

            foreach (var theme in items)
            {
                try
                {
                    string[] parts = theme.Key.Split('§');
                    modName = parts[1];
                    modNameDisplay = modName.Trim();
                    modID = parts[0];

                    //We remove information from default theme for screen display
                    if (modNameDisplay.StartsWith("-"))
                        modNameDisplay = modNameDisplay.Substring(1);

                    curMod = Themes.DBModInfo[modID];

                    //If the filter is defined, check if the modName matches the latter OR if the category differs from all check category of the mod
                    if ( (filter != "" && !(modName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                        || (selCategorieID != "all" && selCategorieID != Themes.getText("category",theme.Key)) )
                    {
                        continue;
                    }

                    if (num + vector.y >= this.scrollPosition.y && num <= this.scrollPosition.y + outRect.height)
                    {
                        Rect rect = new Rect(0f, num, vector.x, vector.y);
                        if (num2 % 2 == 0)
                        {
                            Widgets.DrawAltRect(rect);
                        }

                        GUI.BeginGroup(rect);

                        //Dessin logo mod
                        Rect rect2 = new Rect(8f, (rect.height - 96f) / 2f, 96f, 96f);
                        Widgets.ButtonImage(rect2, Themes.getThemeIcon(theme.Key), Color.white, Color.white);

                        //Font definition as being that of the current theme
                        Themes.forcedFontTheme = theme.Key;

                        //Add mod label + description
                        float textWidth = rect.width - (100 + 16 + 64 + 20);
                        Rect rect5 = new Rect(120f, 0f, textWidth, 100);
                        GUI.BeginGroup(rect5);

                        //Title
                        Rect rectTitle = new Rect(4, 0f, textWidth, 34);
                        GUI.color = titleColor;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Text.Font = GameFont.Medium;
                        Widgets.Label(rectTitle, modNameDisplay);


                        //Description
                        Rect rectDesc = new Rect(0, 38f, textWidth, 66);
                        GUI.color = descriptionColor;
                        Text.Anchor = TextAnchor.UpperCenter;
                        Text.Font = GameFont.Small;
                        Widgets.Label(rectDesc, Themes.getText("description", theme.Key));


                        GUI.EndGroup();

                        GUI.color = Color.white;
                        Text.Anchor = TextAnchor.UpperLeft;

                        //Drawing theme application button
                        Text.Font = GameFont.Small;
                        Rect rect3 = new Rect(rect.width - (64 + 8), 18f, 64f, 64f);
                        if (Settings.curTheme == (parts[0] + "§" + parts[1]))
                        {
                            Widgets.ButtonImage(rect3, Loader.logoSmallSelTex);
                        }
                        else
                        {
                            if (Widgets.ButtonImage(rect3, Loader.logoSmallTex))
                            {
                                Themes.changeThemeNow(parts[0], parts[1],true);
                            }
                        }
                        GUI.EndGroup();
                    }
                    num += vector.y;
                    num2++;
                }
                catch (Exception)
                {

                }
            }
            //Deactivation forcing theme
            Themes.forcedFontTheme = "";

            Widgets.EndScrollView();
        }

        private static int getNbMatchingThemes(string filter)
        {
            int ret = 0;
            foreach (var theme in Themes.DBTex)
            {
                string[] parts = theme.Key.Split('§');

                //If filter defined we check if the modName matches this last
                if (parts[1].IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 && (selCategorieID == "all" || Themes.getText("category", theme.Key) == selCategorieID))
                {
                    ret++;
                }
            }

            return ret;
        }
    }
}