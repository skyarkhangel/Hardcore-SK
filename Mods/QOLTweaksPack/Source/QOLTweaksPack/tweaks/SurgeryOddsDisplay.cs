using Harmony;
using QOLTweaksPack.rimworld;
using QOLTweaksPack.utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using static QOLTweaksPack.tweaks.UIRoot_Play_UIRootOnGUI_Prefix;

namespace QOLTweaksPack.tweaks
{
    [HarmonyPatch(typeof(JobDriver_DoBill), "GetReport")]
    static class JobDriver_DoBill_GetReport_Postfix
    {
        [HarmonyPostfix]
        private static void GetReport(JobDriver_DoBill __instance)
        {
            bool isSurgery = (hasSurgeryAsJob(__instance.pawn));
            //Log.Message("isSurgery:" + isSurgery + " (" + __instance.pawn.jobs.curJob.RecipeDef.workerClass + ")");
            if (!isSurgery)
            {
                UIRoot_Play_UIRootOnGUI_Prefix.shouldDrawSurgeryRect = false;
                return;
            }
            Bill bill = __instance.pawn.jobs.curJob.bill;
            Recipe_Surgery surgery = bill.recipe.Worker as Recipe_Surgery;
            
            Pawn surgeon = __instance.pawn;
            Pawn patient = bill.billStack.billGiver as Pawn;

            if (surgeon != UIRoot_Play_UIRootOnGUI_Prefix.surgeon || patient != UIRoot_Play_UIRootOnGUI_Prefix.patient)
                UIRoot_Play_UIRootOnGUI_Prefix.ResetToEmpty();

            Medicine medicine = null;
            BodyPartRecord part = (bill as Bill_Medical).Part;

            if(surgeon.CurJob.placedThings != null)
            {
                List<ThingStackPartClass> placedThings = surgeon.CurJob.placedThings;
                for (int i = 0; i < placedThings.Count; i++)
                {
                    if (placedThings[i].thing is Medicine)
                    {
                        medicine = placedThings[i].thing as Medicine;
                        break;
                    }
                }
            }
            if (medicine == null)
            {
                if (surgeon.carryTracker.CarriedThing != null)
                    medicine = surgeon.carryTracker.CarriedThing as Medicine;
            }
            if (medicine != null)
            {
                cachedMedicine = medicine;
            }

            SurgeryOdds data = new SurgeryOdds() {};

            SurgeryFailChance(data, surgeon, patient, cachedMedicine, part, surgery);

        }

        private static void SurgeryFailChance(SurgeryOdds data, Pawn surgeon, Pawn patient, Medicine medicine, BodyPartRecord part, Recipe_Surgery surgery)
        {
            float num = 1f;
            num *= surgeon.GetStatValue((!patient.RaceProps.IsMechanoid) ? StatDefOf.MedicalSurgerySuccessChance : StatDefOf.MechanoidOperationSuccessChance, true);
            Room room = patient.GetRoom(RegionType.Set_Passable);
            if (room != null && !patient.RaceProps.IsMechanoid)
            {
                num *= room.GetStat(RoomStatDefOf.SurgerySuccessChanceFactor);
            }
            num *= GetMedicalPotency(medicine);
            num *= surgery.recipe.surgerySuccessChanceFactor;

            calculateChances(num, surgery.recipe.deathOnFailedSurgeryChance, ref data);           

            SetSurgeryChancesForDisplay(data, surgeon, patient);
        }

        private static void SetSurgeryChancesForDisplay(SurgeryOdds data, Pawn surgeon, Pawn patient)
        {
            UIRoot_Play_UIRootOnGUI_Prefix.shouldDrawSurgeryRect = true;
            UIRoot_Play_UIRootOnGUI_Prefix.sentSurgeryOdds = data;
            UIRoot_Play_UIRootOnGUI_Prefix.surgeon = surgeon;
            UIRoot_Play_UIRootOnGUI_Prefix.patient = patient;
        }

        public static void calculateChances(float randNum, float deathOdds, ref SurgeryOdds data)
        {
            if (randNum > 1f)
                randNum = 1f;
            data.chanceSuccess = randNum;
            data.chanceFailDeadly = (1f - data.chanceSuccess) * deathOdds;
            float evens = ((1f - data.chanceSuccess) * (1f - deathOdds)) / 2f;
            data.chanceFailRidiculous = evens * 0.1f;
            data.chanceFailCatastrophic = evens * 0.9f;
            data.chanceFailMinor = evens;
        }

        private static float GetMedicalPotency(Medicine medicine)
        {
            return 1f;
            //TODO: remove when the bug is fixed

            if (medicine == null)
            {
                return 1f;
            }
             
            //herbal: 0.6
            //regular: 1.0
            //glitterworld: 2.2
            return medicine.GetStatValue(StatDefOf.MedicalPotency, true);
        }
    }

    [HarmonyPatch(typeof(UIRoot_Play), "UIRootOnGUI")]
    static class UIRoot_Play_UIRootOnGUI_Prefix
    {
        public struct SurgeryOdds
        {
            public float chanceSuccess;
            public float chanceFailMinor;
            public float chanceFailCatastrophic;
            public float chanceFailRidiculous;
            public float chanceFailDeadly;

            public void Clamp(float min, float max)
            {
                chanceSuccess = chanceSuccess.Clamp(min, max);
                chanceFailMinor = chanceFailMinor.Clamp(min, max);
                chanceFailCatastrophic = chanceFailCatastrophic.Clamp(min, max);
                chanceFailRidiculous = chanceFailRidiculous.Clamp(min, max);
                chanceFailDeadly = chanceFailDeadly.Clamp(min, max);
            }

            public SurgeryOdds addSpec(float addition)
            {
                float newChanceSuccess = chanceSuccess + addition;

                SurgeryOdds newOdds = new SurgeryOdds();

                if (newChanceSuccess >= 1f)
                {
                    newOdds.chanceSuccess = 1f;         //div by 0 protection
                    newOdds.chanceFailMinor = 0;                
                    newOdds.chanceFailCatastrophic = 0;
                    newOdds.chanceFailRidiculous = 0;
                    newOdds.chanceFailDeadly = 0;
                    return newOdds;
                }
                if (newChanceSuccess <= 0f)
                {
                    newChanceSuccess = 0f;
                }
                float mult = (1f - newChanceSuccess) / (1f - chanceSuccess);
                newOdds.chanceSuccess = newChanceSuccess;
                newOdds.chanceFailMinor = chanceFailMinor * mult;
                newOdds.chanceFailCatastrophic = chanceFailCatastrophic * mult;
                newOdds.chanceFailRidiculous = chanceFailRidiculous * mult;
                newOdds.chanceFailDeadly = chanceFailDeadly * mult;
                return newOdds;
            }

            public SurgeryOdds multSpec(float mod)
            {
                float newChanceSuccess = chanceSuccess + (1f - chanceSuccess) * mod;

                //float diff = newChanceSuccess - chanceSuccess;
                float mult = (1f - mod);

                SurgeryOdds newOdds = new SurgeryOdds();

                newOdds.chanceSuccess = newChanceSuccess;
                newOdds.chanceFailMinor = chanceFailMinor * mult;
                newOdds.chanceFailCatastrophic = chanceFailCatastrophic * mult;
                newOdds.chanceFailRidiculous = chanceFailRidiculous * mult;
                newOdds.chanceFailDeadly = chanceFailDeadly * mult;
                return newOdds;
            }
        }

        public static bool shouldDrawSurgeryRect = false;

        public static Pawn surgeon = null;
        public static Pawn patient = null;
        public static Medicine cachedMedicine = null;
        public static SurgeryOdds sentSurgeryOdds;

        private const float InfoboxHeight = 100f;
        private const float InfoboxWidth = 172f;
        private const float InfoboxItemWidth = 42f;
        private const float InfoboxVerticalOffset = 80f;

        private const float ReportBarStart = 5f;
        private const float ReportBarEnd = 5f;
        private const float ReportBarHeight = 15f;
        private const float ReportBarOffset = 2f;

        private const float ReportTextOffsetVert = 3f;
        private const float ReportTextOffsetHori = 5f;

        private static readonly Color BarColorSuccess = new Color(0.0f, 0.8f, 0.0f, 0.75f);
        private static readonly Color BarColorFailMinor = new Color(0.8f, 0.8f, 0.0f, 0.75f);
        private static readonly Color BarColorFailCatastrophic = new Color(0.8f, 0.4f, 0.0f, 0.75f);
        private static readonly Color BarColorFailRidiculous = new Color(0.8f, 0.0f, 0.0f, 0.75f);
        private static readonly Color BarColorFailDeadly = new Color(0.8f, 0.0f, 0.4f, 0.75f);

        private static readonly Color TextColorSuccess = new Color(0.3f, 1.0f, 0.3f, 1f);
        private static readonly Color TextColorFailMinor = new Color(1.0f, 1.0f, 0.3f, 1f);
        private static readonly Color TextColorFailCatastrophic = new Color(1.0f, 0.7f, 0.3f, 1f);
        private static readonly Color TextColorFailRidiculous = new Color(1.0f, 0.3f, 0.3f, 1f);
        private static readonly Color TextColorFailDeadly = new Color(1.0f, 0.3f, 0.7f, 1f);

        [HarmonyPrefix]
        private static void UIRootOnGUI()
        {
            if (!shouldDrawSurgeryRect)
                return;
            else
            {
                CameraZoomRange zoom = Find.CameraDriver.CurrentZoom;
                if (!(zoom == CameraZoomRange.Closest || zoom == CameraZoomRange.Close))
                    return;

                Vector2 pawnPosScreenSpace = patient.DrawPos.MapToUIPosition();

                Rect surgeryRect = new Rect(pawnPosScreenSpace.x - InfoboxWidth / 2f, (pawnPosScreenSpace.y - (InfoboxHeight / 2f)) - InfoboxVerticalOffset, InfoboxWidth, InfoboxHeight);

                WidgetsExtensions.DrawWindowBackgroundTransparent(surgeryRect,0.75f);
                float bottom;
                WidgetsExtensions.DrawGadgetWindowLabel("SurgeryEstimateWindowLabel".Translate(), surgeryRect, Color.white, out bottom);
                WidgetsExtensions.DrawHorizontalDivider(surgeryRect, bottom);
                Rect innerRect = new Rect(surgeryRect);
                innerRect.y += bottom;
                innerRect.height -= bottom;
                innerRect = innerRect.ContractedBy(1f);
                
                Rect medicineRect = new Rect(innerRect);
                medicineRect.width = 42;
                Rect reportRect = new Rect(innerRect);
                reportRect.width = reportRect.width - InfoboxItemWidth;
                reportRect.x = reportRect.x + InfoboxItemWidth;

                WidgetsExtensions.DrawGadgetWindowLabel("UsingMedicineKind".Translate(), medicineRect, Color.white, out bottom);

                GUI.color = Color.white;
                Rect itemIconRect = new Rect(medicineRect.x + 5, medicineRect.yMax - ((InfoboxItemWidth - 10) + 5), 32, 32);
                if (cachedMedicine != null)
                    Widgets.ThingIcon(itemIconRect, cachedMedicine.def);
                else
                {
                    GUI.DrawTexture(itemIconRect, TextureResources.missingMedicine);
                }

                WidgetsExtensions.DrawVerticalDivider(reportRect, 0);

                DrawReport(reportRect);
            }
        }

        private static void DrawReport(Rect reportRect)
        {
            GameFont activeFont = Text.Font;
            Text.Font = GameFont.Small;

            bool detailed = false;
            switch (QOLTweaksPack.SurgeryEstimationMode.Value)
            {
                case QOLTweaksPack.SurgeryEstimateMode.AlwaysAccurate:
                    detailed = true;
                    break;
                case QOLTweaksPack.SurgeryEstimateMode.AccurateIfDoctor:
                    detailed = (surgeon.skills.GetSkill(SkillDefOf.Medicine).Level >= 5) ? true : false;
                    break;
                case QOLTweaksPack.SurgeryEstimateMode.AccurateIfGoodDoctor:
                    detailed = (surgeon.skills.GetSkill(SkillDefOf.Medicine).Level >= 10) ? true : false;
                    break;
                case QOLTweaksPack.SurgeryEstimateMode.AccurateIfAmazingDoctor:
                    detailed = (surgeon.skills.GetSkill(SkillDefOf.Medicine).Level >= 15) ? true : false;
                    break;
                case QOLTweaksPack.SurgeryEstimateMode.NeverAccurate:
                    detailed = false;
                    break;
            }

            //Log.Message("chances:" + surgeryOdds.chanceSuccess + " " + surgeryOdds.chanceFailMinor + " " + surgeryOdds.chanceFailCatastrophic + " " + surgeryOdds.chanceFailRidiculous + " " + surgeryOdds.chanceFailDeadly);

            Color textColor;
            string text;
            bool usesSpecialText;
            bool usesSpecialColor;
            
            SurgeryOdds surgeryOdds = ModifiedOdds(sentSurgeryOdds, surgeon, patient, out usesSpecialText, out usesSpecialColor, out text, out textColor);

            if (detailed)  
            {
                float acc = 0;
                float diff = reportRect.width - ReportBarStart - ReportBarEnd;

                Rect reportTextRect = new Rect(reportRect.x, reportRect.y, reportRect.width, reportRect.height - (ReportBarHeight + ReportBarOffset * 2));
                float oneTextWidth = (reportTextRect.width - 4) / 3f;
                float oneTextHeight = (reportTextRect.height - 4) / 2f;

                if (surgeryOdds.chanceSuccess > 0.0f)
                {
                    GUI.color = BarColorSuccess;
                    GUI.DrawTexture(new Rect(reportRect.x + ReportBarStart + (diff * acc), reportRect.yMax - (ReportBarHeight + ReportBarOffset), diff * surgeryOdds.chanceSuccess, ReportBarHeight), BaseContent.WhiteTex);
                    GUI.color = TextColorSuccess;
                    Rect localRect = new Rect(reportTextRect.x + ReportTextOffsetHori, reportTextRect.y + ReportTextOffsetVert, oneTextWidth, oneTextHeight);
                    GUI.Label(localRect, ((Mathf.Round(surgeryOdds.chanceSuccess * 100)).ToString("F0") + "%"));
                    TooltipHandler.TipRegion(localRect, "Surgery_detailedMouseover_OddsOfSuccess".Translate());
                    acc += surgeryOdds.chanceSuccess;
                }
                if (surgeryOdds.chanceFailMinor > 0.0f)
                {
                    GUI.color = BarColorFailMinor;
                    GUI.DrawTexture(new Rect(reportRect.x + ReportBarStart + (diff * acc), reportRect.yMax - (ReportBarHeight + ReportBarOffset), diff * surgeryOdds.chanceFailMinor, ReportBarHeight), BaseContent.WhiteTex);
                    GUI.color = TextColorFailMinor;
                    Rect localRect = new Rect(reportTextRect.x + ReportTextOffsetHori + oneTextWidth, reportTextRect.y + ReportTextOffsetVert, oneTextWidth, oneTextHeight);
                    GUI.Label(localRect, ((Mathf.Round(surgeryOdds.chanceFailMinor * 100)).ToString("F0") + "%"));
                    TooltipHandler.TipRegion(localRect, "Surgery_detailedMouseover_OddsOfFailMinor".Translate());
                    acc += surgeryOdds.chanceFailMinor;
                }
                if (surgeryOdds.chanceFailCatastrophic > 0.0f)
                {
                    GUI.color = BarColorFailCatastrophic;
                    GUI.DrawTexture(new Rect(reportRect.x + ReportBarStart + (diff * acc), reportRect.yMax - (ReportBarHeight + ReportBarOffset), diff * surgeryOdds.chanceFailCatastrophic, ReportBarHeight), BaseContent.WhiteTex);
                    GUI.color = TextColorFailCatastrophic;
                    Rect localRect = new Rect(reportTextRect.x + ReportTextOffsetHori + oneTextWidth * 2, reportTextRect.y + ReportTextOffsetVert, oneTextWidth, oneTextHeight);
                    GUI.Label(localRect, ((Mathf.Round(surgeryOdds.chanceFailCatastrophic * 100)).ToString("F0") + "%"));
                    TooltipHandler.TipRegion(localRect, "Surgery_detailedMouseover_OddsOfFailCatastrophic".Translate());
                    acc += surgeryOdds.chanceFailCatastrophic; 
                }
                if (surgeryOdds.chanceFailRidiculous > 0.0f)
                {
                    GUI.color = BarColorFailRidiculous;
                    GUI.DrawTexture(new Rect(reportRect.x + ReportBarStart + (diff * acc), reportRect.yMax - (ReportBarHeight + ReportBarOffset), diff * surgeryOdds.chanceFailRidiculous, ReportBarHeight), BaseContent.WhiteTex);
                    GUI.color = TextColorFailRidiculous;
                    Rect localRect = new Rect(reportTextRect.x + ReportTextOffsetHori + oneTextWidth, reportTextRect.y + ReportTextOffsetVert + oneTextHeight, oneTextWidth, oneTextHeight);
                    GUI.Label(localRect, ((Mathf.Round(surgeryOdds.chanceFailRidiculous * 100)).ToString("F0") + "%"));
                    TooltipHandler.TipRegion(localRect, "Surgery_detailedMouseover_OddsOfFailRidiculous".Translate());
                    acc += surgeryOdds.chanceFailRidiculous;
                }
                if(surgeryOdds.chanceFailDeadly > 0.0f)
                {
                    GUI.color = BarColorFailDeadly;
                    GUI.DrawTexture(new Rect(reportRect.x + ReportBarStart + (diff * acc), reportRect.yMax - (ReportBarHeight + ReportBarOffset), diff * surgeryOdds.chanceFailDeadly, ReportBarHeight), BaseContent.WhiteTex);
                    GUI.color = TextColorFailDeadly;
                    Rect localRect = new Rect(reportTextRect.x + ReportTextOffsetHori + oneTextWidth * 2, reportTextRect.y + ReportTextOffsetVert + oneTextHeight, oneTextWidth, oneTextHeight);
                    GUI.Label(localRect, ((Mathf.Round(surgeryOdds.chanceFailDeadly * 100)).ToString("F0") + "%"));
                    TooltipHandler.TipRegion(localRect, "Surgery_detailedMouseover_OddsOfFailDeadly".Translate());
                    acc += surgeryOdds.chanceFailDeadly;
                }
                

                GUI.color = Color.white;
            }
            else
            {
                float assumedOdds = surgeryOdds.chanceSuccess + surgeryOdds.chanceFailMinor * MinorFailAllowanceMult;

                if (assumedOdds < ImpossibleOdds)
                {
                    if (!usesSpecialText) text = ImpossibleOddsText;
                    if (!usesSpecialColor) textColor = ImpossibleOddsColor;
                }
                else if (assumedOdds < TerribleOdds)
                {
                    if (!usesSpecialText) text = TerribleOddsText;
                    if (!usesSpecialColor) textColor = TerribleOddsColor;
                }
                else if (assumedOdds < BadOdds)
                {
                    if (!usesSpecialText) text = BadOddsText;
                    if (!usesSpecialColor) textColor = BadOddsColor;
                }
                else if (assumedOdds < AcceptableOdds)
                {
                    if (!usesSpecialText) text = AcceptableOddsText;
                    if (!usesSpecialColor) textColor = AcceptableOddsColor;
                }
                else if (assumedOdds < GoodOdds)
                {
                    if (!usesSpecialText) text = GoodOddsText;
                    if (!usesSpecialColor) textColor = GoodOddsColor;
                }
                else if (assumedOdds < GreatOdds)
                {
                    if (!usesSpecialText) text = GreatOddsText;
                    if (!usesSpecialColor) textColor = GreatOddsColor;
                }
                else
                {
                    if (!usesSpecialText) text = AmazingOddsText;
                    if (!usesSpecialColor) textColor = AmazingOddsColor;
                }
                float bottom;

                reportRect = reportRect.ContractedBy(2f);

                WidgetsExtensions.DrawGadgetWindowLabel(text.Translate(), reportRect, textColor, out bottom);
            }

            Text.Font = activeFont;
        }

        private static SurgeryOdds ModifiedOdds(SurgeryOdds surgeryOdds, Pawn surgeon, Pawn patient, out bool usesSpecialText, out bool usesSpecialColor, out string specialText, out Color specialColor)
        {
            if (!QOLTweaksPack.SurgeryEstimateAccountForTraits)
            {
                usesSpecialText = false;
                usesSpecialColor = false;
                specialText = "";
                specialColor = Color.white;
                return surgeryOdds;
            }
            else
            {
                //Log.Message("trait check");

                float skillMod = ((20f - surgeon.skills.GetSkill(SkillDefOf.Medicine).Level) / 20f);
                //first apply modifiers

                /*foreach(Trait trait in surgeon.story.traits.allTraits)
                {
                    Log.Message(trait.Label);
                }*/

                if (surgeon.story.traits.HasTrait(TraitDefOf.NaturalMood))
                {
                    float addition = (surgeon.story.traits.GetTrait(TraitDefOf.NaturalMood).Degree * 0.2f * skillMod);

                    surgeryOdds = surgeryOdds.addSpec(addition);

                    //Log.Message("modified odds by " + addition + " due to mood trait");
                }


                if (surgeon.story.traits.HasTrait(TraitDefOf.Psychopath)
                    || (surgeon.story.traits.HasTrait(TraitDefOf.Bloodlust) && patient.Faction != null && !patient.Faction.IsPlayer))
                {
                    usesSpecialText = true;
                    usesSpecialColor = true;
                    specialText = "SurgerySpecial_Psychopath";
                    specialColor = new Color(0.9f, 0.1f, 0.1f);

                    float mult = skillMod;

                    surgeryOdds = surgeryOdds.multSpec(mult);

                    //Log.Message("modified odds by " + mult + " due to psychopath trait");
                    return surgeryOdds;
                }

                usesSpecialText = false;
                usesSpecialColor = false;
                specialText = "";
                specialColor = Color.white;
                return surgeryOdds;
            }
        }

        private const float MinorFailAllowanceMult = 1f / 3f;

        private const float ImpossibleOdds = 0.15f;
        private const string ImpossibleOddsText = "Surgery_ImpossibleOdds";
        private static Color ImpossibleOddsColor = new Color(0.7f, 0.1f, 0.1f);
        private const float TerribleOdds = 0.3f;
        private const string TerribleOddsText = "Surgery_TerribleOdds";
        private static Color TerribleOddsColor = new Color(1.0f, 0.3f, 0.3f);
        private const float BadOdds = 0.45f;
        private const string BadOddsText = "Surgery_BadOdds";
        private static Color BadOddsColor = new Color(1.0f, 0.7f, 0.3f);
        private const float AcceptableOdds = 0.6f;
        private const string AcceptableOddsText = "Surgery_AcceptableOdds";
        private static Color AcceptableOddsColor = new Color(1.0f, 1.0f, 0.3f);
        private const float GoodOdds = 0.75f;
        private const string GoodOddsText = "Surgery_GoodOdds";
        private static Color GoodOddsColor = new Color(0.7f, 1.0f, 0.3f);
        private const float GreatOdds = 0.9f;
        private const string GreatOddsText = "Surgery_GreatOdds";
        private static Color GreatOddsColor = new Color(0.3f, 1.0f, 0.3f);
        private const float AmazingOdds = 1.0f;
        private const string AmazingOddsText = "Surgery_AmazingOdds";
        private static Color AmazingOddsColor = new Color(0.3f, 1.0f, 0.7f);

        internal static void Validate()
        {
            if(surgeon == null || Find.Selector.FirstSelectedObject != surgeon || !hasSurgeryAsJob(surgeon))
            {
                ResetToEmpty();
            }
        }

        public static void ResetToEmpty()
        { 
            surgeon = null;
            patient = null;
            cachedMedicine = null;
            shouldDrawSurgeryRect = false;
        }

        public static bool hasSurgeryAsJob(Pawn pawn)
        {
            if (pawn.jobs == null || pawn.jobs.curJob == null || pawn.jobs.curJob.bill == null || pawn.jobs.curJob.RecipeDef == null || pawn.jobs.curJob.RecipeDef.workerClass == null)
                return false;
            return pawn.jobs.curJob.RecipeDef.workerClass.IsSubclassOf(typeof(Recipe_Surgery)) || pawn.jobs.curJob.RecipeDef.workerClass == typeof(Recipe_Surgery);
        }
    }
}
