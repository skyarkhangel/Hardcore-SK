using UnityEngine;
using Verse;

namespace aRandomKiwi.RimThemes
{
    [StaticConstructorOnStartup]
    internal static class MenuTools
    {
        public static string discordlink = "https://discord.gg/rtdm9CY";
        public static string patreonlink = "https://www.patreon.com/skyarkhangel";
        public static string githublink = "https://github.com/skyarkhangel/Hardcore-SK";
        public static string hcskvklink = "https://vk.com/hardcore_sk";
        internal static Texture2D SK_Icon;

        public static Texture2D GetMenuIcon(string iconame)
        {
            if (iconame == "patreon" && (Object)MenuTools.SK_Icon == (Object)null)
                return MenuTools.SK_Icon = ContentFinder<Texture2D>.Get("UI/patreon");
            if (iconame == "discord" && (Object)MenuTools.SK_Icon == (Object)null)
                return MenuTools.SK_Icon = ContentFinder<Texture2D>.Get("UI/discord");
            return iconame == "github" && (Object)MenuTools.SK_Icon == (Object)null ? (MenuTools.SK_Icon = ContentFinder<Texture2D>.Get("UI/github")) : MenuTools.SK_Icon;
        }
    }
}
