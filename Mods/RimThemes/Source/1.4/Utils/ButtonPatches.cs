using aRandomKiwi.RimThemes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace aRandomKiwi.RimThemes
{
  [StaticConstructorOnStartup]
  public static class ButtonPatches
  {
    public const string LogLabel = "[XmlPatchHelper]";
    internal static Texture2D XPathHelperIcon = ContentFinder<Texture2D>.Get("XmlPatchHelperMenuIcon");

    internal static string CurrentVersion { get; private set; }

    internal static Harmony HarmonyInstance { get; private set; }

    static ButtonPatches()
    {
      ButtonPatches.HarmonyInstance = new Harmony("smashphil.xmlpatchhelper");
      Version version = Assembly.GetExecutingAssembly().GetName().Version;
      ButtonPatches.CurrentVersion = string.Format("{0}.{1}.{2}", (object) version.Major, (object) version.Minor, (object) version.Build);
      Log.Message("<color=orange>[XmlPatchHelper]</color> version " + ButtonPatches.CurrentVersion);
      ButtonPatches.HarmonyInstance.Patch((MethodBase) AccessTools.Method(typeof (OptionListingUtility), "DrawOptionListing"), new HarmonyMethod(typeof (ButtonPatches), "InsertXmlPatcherButton"));
    }

    public static void InsertXmlPatcherButton(Rect rect, ref List<ListableOption> optList)
    {
      if (optList.FirstOrDefault<ListableOption>((Predicate<ListableOption>) (opt => opt.label == (string) "BuySoundtrack".Translate())) == null)
        return;

        Text.Font = GameFont.Small;
        if ((UnityEngine.Object)DoMainMenuControls_Patch.SK_IconWorld == null)
            DoMainMenuControls_Patch.SK_IconWorld = ContentFinder<Texture2D>.Get("UI/world");
        if ((UnityEngine.Object)DoMainMenuControls_Patch.SK_IconSK == null)
            DoMainMenuControls_Patch.SK_IconSK = ContentFinder<Texture2D>.Get("UI/sk");
        if ((UnityEngine.Object)DoMainMenuControls_Patch.SK_IconGithub == null)
            DoMainMenuControls_Patch.SK_IconGithub = ContentFinder<Texture2D>.Get("UI/github");
        if ((UnityEngine.Object)DoMainMenuControls_Patch.SK_IconVK == null)
            DoMainMenuControls_Patch.SK_IconVK = ContentFinder<Texture2D>.Get("UI/vk");
        if ((UnityEngine.Object)DoMainMenuControls_Patch.SK_IconPatreon == null)
            DoMainMenuControls_Patch.SK_IconPatreon = ContentFinder<Texture2D>.Get("UI/patreon");
        if ((UnityEngine.Object)DoMainMenuControls_Patch.SK_IconDiscord == null)
            DoMainMenuControls_Patch.SK_IconDiscord = ContentFinder<Texture2D>.Get("UI/discord");
        GUI.contentColor = new Color(1f, 1f, 1f, 1f);
        optList.Add((ListableOption)new ListableOption_WebLink("HSK Patreon".Translate(), MenuTools.patreonlink, DoMainMenuControls_Patch.SK_IconPatreon));
        optList.Add((ListableOption)new ListableOption_WebLink("HSK GitHub".Translate(), MenuTools.githublink, DoMainMenuControls_Patch.SK_IconGithub));
        if (DoMainMenuControls_Patch.isRussian)
        {
            ListableOption listableOption11 = (ListableOption)new ListableOption_WebLink("Hardcoresk VK".Translate(), MenuTools.hcskvklink, DoMainMenuControls_Patch.SK_IconVK);
            optList.Add(listableOption11);
        }
        ListableOption listableOption12 = (ListableOption)new ListableOption_WebLink("HSK Discord".Translate(), MenuTools.discordlink, DoMainMenuControls_Patch.SK_IconDiscord);
        optList.Add(listableOption12);
    }
  }
}
