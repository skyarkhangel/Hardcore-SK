using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace aRandomKiwi.RimThemes
{
    class ColorsSubstitution
    {

        static Dictionary<string, Dictionary<Color,Color>> cacheColor = new Dictionary<string, Dictionary<Color, Color>>();
        static Dictionary<string, Dictionary<Color, Color>> cacheTextColor = new Dictionary<string, Dictionary<Color, Color>>();

        /*
         * Obtaining substitution color from a GUI.color color for the current applicable theme
         */
        public static Color getTextSubstitutionColor(Color color)
        {
            if (Utils.tempDisableDynColor)
                return color;

            Color c;
            string theme = Settings.curTheme;

            if (!cacheColor.TryGetValue(theme, out Dictionary<Color, Color> p1))
                cacheColor[theme] = new Dictionary<Color, Color>();

            //Already hidden, we return the cache
            if (cacheColor[theme].TryGetValue(color, out Color p2))
            {
                return p2;
            }

            Color curColor;
            if (Themes.DBTextColorWhite.TryGetValue(theme, out curColor))
            {
                c = curColor;
                if (Mathf.Approximately(color.r, Color.white.r) && Mathf.Approximately(color.g, Color.white.g) && Mathf.Approximately(color.b, Color.white.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorYellow.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorYellow[theme];
                if (Mathf.Approximately(color.r, Color.yellow.r) && Mathf.Approximately(color.g, Color.yellow.g) && Mathf.Approximately(color.b, Color.yellow.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorRed.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorRed[theme];
                if (Mathf.Approximately(color.r, Color.red.r) && Mathf.Approximately(color.g, Color.red.g) && Mathf.Approximately(color.b, Color.red.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorGreen.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorGreen[theme];
                if (Mathf.Approximately(color.r, Color.green.r) && Mathf.Approximately(color.g, Color.green.g) && Mathf.Approximately(color.b, Color.green.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorBlue.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorBlue[theme];
                if (Mathf.Approximately(color.r, Color.blue.r) && Mathf.Approximately(color.g, Color.blue.g) && Mathf.Approximately(color.b, Color.blue.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorCyan.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorCyan[theme];
                if (Mathf.Approximately(color.r, Color.cyan.r) && Mathf.Approximately(color.g, Color.cyan.g) && Mathf.Approximately(color.b, Color.cyan.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorGray.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorGray[theme];
                if (Mathf.Approximately(color.r, Color.gray.r) && Mathf.Approximately(color.g, Color.gray.g) && Mathf.Approximately(color.b, Color.gray.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTextColorMagenta.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTextColorMagenta[theme];
                if (Mathf.Approximately(color.r, Color.magenta.r) && Mathf.Approximately(color.g, Color.magenta.g) && Mathf.Approximately(color.b, Color.magenta.b))
                {
                    c.a = color.a;
                    cacheColor[theme][color] = c;
                    return c;
                }
            }

            //DynColors
            if (Themes.DBDynColor.ContainsKey(theme) && Themes.DBDynColor[theme].ContainsKey(color))
            {
                cacheColor[theme][color] = Themes.DBDynColor[theme][color];
                return cacheColor[theme][color];
            }

            cacheColor[theme][color] = color;
            return color;
        }



        /*
          * Obtaining Texture substitution color from a GUI.color color for the current applicable theme
          */
        public static Color getTextureSubstitutionColor(Color color)
        {
            if (Utils.tempDisableDynColor)
                return color;

            Color c;
            string theme = Settings.curTheme;

            if (!cacheTextColor.TryGetValue(theme, out Dictionary<Color, Color> p1))
                cacheTextColor[theme] = new Dictionary<Color, Color>();

            //Already hidden, we return the cache
            if (cacheTextColor[theme].TryGetValue(color, out Color p2))
            {
                return p2;
            }


            Color curColor;
            if (Themes.DBTexColorWhite.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorWhite[theme];
                if (Mathf.Approximately(color.r, Color.white.r) && Mathf.Approximately(color.g, Color.white.g) && Mathf.Approximately(color.b, Color.white.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorYellow.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorYellow[theme];
                if (Mathf.Approximately(color.r, Color.yellow.r) && Mathf.Approximately(color.g, Color.yellow.g) && Mathf.Approximately(color.b, Color.yellow.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorRed.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorRed[theme];
                if (Mathf.Approximately(color.r, Color.red.r) && Mathf.Approximately(color.g, Color.red.g) && Mathf.Approximately(color.b, Color.red.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorGreen.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorGreen[theme];
                if (Mathf.Approximately(color.r, Color.green.r) && Mathf.Approximately(color.g, Color.green.g) && Mathf.Approximately(color.b, Color.green.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorBlue.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorBlue[theme];
                if (Mathf.Approximately(color.r, Color.blue.r) && Mathf.Approximately(color.g, Color.blue.g) && Mathf.Approximately(color.b, Color.blue.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorCyan.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorCyan[theme];
                if (Mathf.Approximately(color.r, Color.cyan.r) && Mathf.Approximately(color.g, Color.cyan.g) && Mathf.Approximately(color.b, Color.cyan.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorGray.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorGray[theme];
                if (Mathf.Approximately(color.r, Color.gray.r) && Mathf.Approximately(color.g, Color.gray.g) && Mathf.Approximately(color.b, Color.gray.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }
            if (Themes.DBTexColorMagenta.TryGetValue(theme, out curColor))
            {
                c = Themes.DBTexColorMagenta[theme];
                if (Mathf.Approximately(color.r, Color.magenta.r) && Mathf.Approximately(color.g, Color.magenta.g) && Mathf.Approximately(color.b, Color.magenta.b))
                {
                    c.a = color.a;
                    cacheTextColor[theme][color] = c;
                    return c;
                }
            }

            //DynColors
            if (Themes.DBDynColor.ContainsKey(theme) && Themes.DBDynColor[theme].ContainsKey(color))
            {
                cacheTextColor[theme][color] = Themes.DBDynColor[theme][color];
                return cacheTextColor[theme][color];
            }

            cacheTextColor[theme][color] = color;
            return color;
        }

    }
}
