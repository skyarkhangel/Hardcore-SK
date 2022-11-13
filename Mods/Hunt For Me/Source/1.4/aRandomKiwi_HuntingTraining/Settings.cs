using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace aRandomKiwi.HFM
{
    class Settings : ModSettings
    {
        public static Settings settings;
        public static List<string> ignoredPreys = new List<string>() { "Boomrat", "Boomalope", "HPLovecraft_MistStalker", "HPLovecraft_MistStalkerTwo" };
        public static List<string> ignoredRangedAttack = new List<string>() { "Green_Dragon_Race", "Black_Dragon_Race",
            "Brown_Dragon_Race", "Gold_Dragon_Race", "Silver_Dragon_Race", "Red_Dragon_Race", "Blue_Dragon_Race", "White_Dragon_Race",
        "Purple_Dragon_Race", "Cryo_Dragon_Race", "Flamingo_Dragon_Race", "Rock_Dragon_Race", "Yellow_Dragon_Race", "True_Dragon_Race", "Royal_Dragon_Race"};
        public static List<string> cats = new List<string>() { "Cat", "akaNEKO_shironeko", "akaNEKO_kuroneko", "akaNEKO_kijitora", "akaNEKO_A_Shorthair", "akaNEKO_Russian_Blue",
        "akaNEKO_Siamese", "akaNEKO_J_Bobtail", "akaNEKO_Persian", "akaNEKO_Scottish_Fold","akaNEKO_Scottish_Fold_Long", "akaNEKO_Maine_Coon", "akaNEKO_N_Forest_Cat"};
        public static int maxHuntMPack = 16;
        public static bool allowCatGift = true;
        public static int timeToWaitBeforeTryHunt = 20000;
        public static float radiusBetweenPackMembers = 220f;
        public static bool allowDiffSpeciesPack = true;
        public static bool disallowManhunter = true;
        public static bool allowRangedAttack = true;
        public static bool notifNewHuntingPack = true;
        public static bool notifNewHunting = true;
        public static bool allowAllToHunt = false;
        public static bool disallowHuntingWhenThreat = false;
        public static bool disableSoloHunting = false;
        public static bool sameSpeedPacks = false;
        public static bool showExtraTools = false;

        public static Vector2 scrollPosition = Vector2.zero;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            if (ignoredPreys == null)
                resetDefaultIgnoredPreys();
            inRect.yMin += 15f;
            inRect.yMax -= 15f;

            var defaultColumnWidth = (inRect.width - 50);
            Listing_Standard list = new Listing_Standard() { ColumnWidth = defaultColumnWidth };


            var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var scrollRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height * 2.2f + 400+ (50 * ignoredRangedAttack.Count) + (50 * ignoredPreys.Count) + (50 * cats.Count));
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);

            list.Begin(scrollRect);

            //allow cats to bring back a gift
            list.CheckboxLabeled("HuntingForMe_SettingsAllowCatGift".Translate(), ref allowCatGift);

            //authorize the packing of animals of different species
            list.CheckboxLabeled("HuntingForMe_SettingsAllowDiffSpeciesPack".Translate(), ref allowDiffSpeciesPack);

            //authorize the packing of animals of different species
            list.CheckboxLabeled("HuntingForMe_SettingsDisallowManhunter".Translate(), ref disallowManhunter);

            //allow remote attacks
            list.CheckboxLabeled("HuntingForMe_SettingsAllowRangedAttack".Translate(), ref allowRangedAttack);

            //Notify natural pack formation
            list.CheckboxLabeled("HuntingForMe_SettingsNotifNewHuntingPack".Translate(), ref notifNewHuntingPack);

            //Notify when animal goes solo hunting
            list.CheckboxLabeled("HuntingForMe_SettingsNotifNewHunting".Translate(), ref notifNewHunting);

            //Notify when animal goes solo hunting
            list.CheckboxLabeled("HuntingForMe_SettingsDisallowHuntingWhenThreat".Translate(), ref disallowHuntingWhenThreat);


            //Disable solo hunting
            list.CheckboxLabeled("HuntingForMe_SettingsNoSoloHunt".Translate(), ref disableSoloHunting);

            //Allow learning of 'Hunt' to all animals
            list.CheckboxLabeled("HuntingForMe_SettingsAllowAllToHunt".Translate(), ref allowAllToHunt);
            Utils.setAllowAllToHuntState();

            list.CheckboxLabeled("HuntingForMe_SettingsSameSpeedPack".Translate(), ref sameSpeedPacks);

            list.CheckboxLabeled("HuntingForMe_SettingsShowExtraTools".Translate(), ref showExtraTools);


            list.Gap(10f);
            list.GapLine();
            //Maximum packing of hunters (by default 4)
            //MaxHunterPack capping
            if (maxHuntMPack > 30)
                maxHuntMPack = 16;

            list.Label("HuntingForMe_SettingsMaxAllowedHunterPacking".Translate(maxHuntMPack));
            maxHuntMPack = (int)list.Slider(maxHuntMPack, 1, 30);

            //Time to wait before being able to give a manual order (by default 8h)
            list.Label("HuntingForMe_SettingsTimeToWaitBeforeTryHunt".Translate(Utils.TranslateTicksToTextIRLSeconds(timeToWaitBeforeTryHunt)));
            timeToWaitBeforeTryHunt = (int)list.Slider(timeToWaitBeforeTryHunt, 2500, 120000);

            //Max distance between the components of a pack
            list.Label("HuntingForMe_SettingsMaxRadiusBetweenPackMembers".Translate(radiusBetweenPackMembers));
            radiusBetweenPackMembers = list.Slider(radiusBetweenPackMembers, 1, 250);


            list.GapLine();
            list.Label("HuntingForMe_SettingsIgnoredPreys".Translate());
            list.Gap(10f);

            if (list.ButtonText("+"))
                ignoredPreys.Add("");

            if (list.ButtonText("-"))
            {
                if (ignoredPreys.Count != 0)
                    ignoredPreys.RemoveLast();
            }

            //nbField = (int)list.Slider(nbField, 1, 100);


            for (var i = 0; i != ignoredPreys.Count; i++)
            {
                list.Label("HuntingForMe_SettingsIgnoredPreyListNumber".Translate(i));
                ignoredPreys[i] = list.TextEntry(ignoredPreys[i]);
                list.Gap(4f);
            }

            list.Gap(10f);
            if (list.ButtonText("HuntingForMe_SettingsResetIgnoredPreys".Translate()))
                resetDefaultIgnoredPreys();


            // Ranged attacking creature exception list
            list.Gap(25f);
            list.GapLine();

            list.Label("HuntingForMe_SettingsIgnoredRangedAttack".Translate());
            list.Gap(10f);

            if (list.ButtonText("+"))
                ignoredRangedAttack.Add("");

            if (list.ButtonText("-"))
            {
                if (ignoredRangedAttack.Count != 0)
                    ignoredRangedAttack.RemoveLast();
            }

            for (var i = 0; i != ignoredRangedAttack.Count; i++)
            {
                list.Label("#" + i.ToString());
                ignoredRangedAttack[i] = list.TextEntry(ignoredRangedAttack[i]);
                list.Gap(4f);
            }

            list.Gap(10f);
            if (list.ButtonText("HuntingForMe_SettingsResetIgnoredRangedAttack".Translate()))
                resetDefaultIgnoredRangedAttack();

            list.Gap(25f);
            list.GapLine();


            //List of entities recognized as a cat
            list.Label("HuntingForMe_SettingsCats".Translate());
            list.Gap(10f);

            if (list.ButtonText("+"))
                cats.Add("");

            if (list.ButtonText("-"))
            {
                if (cats.Count != 0)
                    cats.RemoveLast();
            }

            for (var i = 0; i != cats.Count; i++)
            {
                list.Label("#" + i.ToString());
                cats[i] = list.TextEntry(cats[i]);
                list.Gap(4f);
            }

            list.Gap(10f);
            if (list.ButtonText("HuntingForMe_SettingsResetCats".Translate()))
                resetCats();

            list.End();
            Widgets.EndScrollView();
            //settings.Write();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref maxHuntMPack, "maxHunterMPack", 12);
            Scribe_Values.Look<int>(ref timeToWaitBeforeTryHunt, "timeToWaitBeforeTryHunt", 20000);
            Scribe_Values.Look<float>(ref radiusBetweenPackMembers, "radiusMaxBetweenPackMembers", 220f);
            Scribe_Values.Look<bool>(ref allowCatGift, "allowCatGift", true);
            Scribe_Values.Look<bool>(ref allowDiffSpeciesPack, "allowDiffSpeciesPack", true);
            Scribe_Collections.Look<string>(ref ignoredPreys, "ignoredPreys", LookMode.Value);
            Scribe_Collections.Look<string>(ref ignoredRangedAttack, "ignoredRangedAttack", LookMode.Value);
            Scribe_Collections.Look<string>(ref cats, "cats", LookMode.Value);
            Scribe_Values.Look<bool>(ref disallowManhunter, "disallowManhunter", true);
            Scribe_Values.Look<bool>(ref allowRangedAttack, "allowRangedAttack", true);
            Scribe_Values.Look<bool>(ref allowAllToHunt, "allowAllToHunt", false);
            Scribe_Values.Look<bool>(ref notifNewHunting, "notifNewHunting", true);
            Scribe_Values.Look<bool>(ref notifNewHuntingPack, "notifNewHuntingPack", true);
            Scribe_Values.Look<bool>(ref disallowHuntingWhenThreat, "disallowHuntingWhenThreat", false);
            Scribe_Values.Look<bool>(ref disableSoloHunting, "disableSoloHunting", false);
            Scribe_Values.Look<bool>(ref sameSpeedPacks, "sameSpeedPacks", false);
            Scribe_Values.Look<bool>(ref showExtraTools, "showExtraTools", false);


            if (ignoredPreys == null)
            {
                resetDefaultIgnoredPreys();
            }
            if (ignoredRangedAttack == null)
            {
                resetDefaultIgnoredRangedAttack();
            }
            if(cats == null)
            {
                resetCats();
            }
        }

        static private void resetDefaultIgnoredPreys()
        {
            ignoredPreys = new List<string>();
            ignoredPreys.Add("Boomrat");
            ignoredPreys.Add("Boomalope");
            ignoredPreys.Add("HPLovecraft_MistStalker");
            ignoredPreys.Add("HPLovecraft_MistStalkerTwo");
        }

        static private void resetDefaultIgnoredRangedAttack()
        {
            ignoredRangedAttack = new List<string>() { "Green_Dragon_Race", "Black_Dragon_Race",
            "Brown_Dragon_Race", "Gold_Dragon_Race", "Silver_Dragon_Race", "Red_Dragon_Race", "Blue_Dragon_Race", "White_Dragon_Race",
        "Purple_Dragon_Race", "Cryo_Dragon_Race", "Flamingo_Dragon_Race", "Rock_Dragon_Race", "Yellow_Dragon_Race", "True_Dragon_Race", "Royal_Dragon_Race"};
        }

        static private void resetCats()
        {
            cats = new List<string>() {
                "Cat", "akaNEKO_shironeko", "akaNEKO_kuroneko", "akaNEKO_kijitora", "akaNEKO_A_Shorthair", "akaNEKO_Russian_Blue",
            "akaNEKO_Siamese", "akaNEKO_J_Bobtail", "akaNEKO_Persian", "akaNEKO_Scottish_Fold","akaNEKO_Scottish_Fold_Long", "akaNEKO_Maine_Coon", "akaNEKO_N_Forest_Cat"};
        }
    }
}