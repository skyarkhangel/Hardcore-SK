using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

// by Dan Sadler (me@sadler.su)
namespace RimWorldIconMod
{

    class PSI : MonoBehaviour
    {

        private static double fDelta = 0;
        public static bool inGame = false;

        private static PawnCapacityDef[] pawnCapacities;


        private static Dictionary<Pawn, PawnStats> stats_dict = new Dictionary<Pawn, PawnStats>();
        private static bool iconsEnabled = true;        
        private static Dialog_IconModSettings modSettingsDialog = new Dialog_IconModSettings();     
   
        public static ModSettings settings = new ModSettings();

        
        private static Vector3[] iconPosVectors;
        private static readonly Color targetColor = new Color(1, 1, 1, 0.6f);
        private static float worldScale = 1.0f;

        public static string[] iconSets = {"default"};

        public static Materials materials = new Materials("default");

        public static void reinit(bool reloadSettings = true, bool reloadIconSet = true, bool recalcIconPos = true)
        {
            pawnCapacities = new PawnCapacityDef[]{
                                                 PawnCapacityDefOf.BloodFiltration, 
                                                 PawnCapacityDefOf.BloodPumping,
                                                 PawnCapacityDefOf.Breathing, 
                                                 PawnCapacityDefOf.Consciousness,
                                                 PawnCapacityDefOf.Eating, 
                                                 PawnCapacityDefOf.Hearing,
                                                 PawnCapacityDefOf.Manipulation, 
                                                 PawnCapacityDefOf.Metabolism,
                                                 PawnCapacityDefOf.Moving, 
                                                 PawnCapacityDefOf.Sight,
                                                 PawnCapacityDefOf.Talking 
                                                 };  

            if (reloadSettings) settings = loadSettings();

            if (reloadIconSet)
            {
                
                materials = new Materials(settings.iconSet);
                ModSettings isets = XmlLoader.ItemFromXmlFile<ModSettings>(GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Textures/UI/Overlays/PawnStateIcons/" + settings.iconSet + "/iconset.cfg");
                settings.iconSizeMult = isets.iconSizeMult;

                materials.reloadTextures(true);
            }

            if (recalcIconPos) recalcIconPositions();

        }

        public static ModSettings loadSettings(string path = "psi-settings.cfg") {
            //try {
                ModSettings sets = XmlLoader.ItemFromXmlFile<ModSettings>(path, true);

                string path2 = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Textures/UI/Overlays/PawnStateIcons/";
                if (Directory.Exists(path2))
                {
                    iconSets = Directory.GetDirectories(path2);
                    for (int i = 0; i < iconSets.Length; i++) iconSets[i] = (new DirectoryInfo(iconSets[i])).Name;

                }

                return sets;

            //} catch(Exception ex) {}            
        }

        public static void saveSettings(string path = "psi-settings.cfg")
        {
            XmlSaver.SaveDataObject((object)settings, path);

        }

        private void DrawIcon(Vector3 bodyPos, Vector3 posOffset, Icons icon, Color c)
        {
            Material m = materials[icon];

            if (m == null) return;
            m.color = c;
                        
            Color oc = GUI.color;
            GUI.color = c;

            Vector2 scrPosVec;

            if (settings.iconsScreenScale)
            {
                scrPosVec = bodyPos.ToScreenPosition();
                scrPosVec.x += posOffset.x*45;
                scrPosVec.y -= posOffset.z*45;
            }
            else
            {
                scrPosVec = (bodyPos+posOffset).ToScreenPosition();
            }

            float ws = worldScale;
            if (settings.iconsScreenScale) ws = 45;
            ws *= settings.iconSizeMult * 0.5f;

            Rect scrPos = new Rect(scrPosVec.x, scrPosVec.y, ws * settings.iconSize, ws * settings.iconSize);
            scrPos.x -= scrPos.width * 0.5f;
            scrPos.y -= scrPos.height * 0.5f;
            
            GUI.DrawTexture(scrPos, m.mainTexture,ScaleMode.ScaleToFit,true);
            
            GUI.color = oc;            
        }

        private void DrawIcon(Vector3 bodyPos, int num, Icons icon, Color c)
        {            
            DrawIcon(bodyPos,iconPosVectors[num], icon, c);
        }

        private void DrawIcon(Vector3 bodyPos, int num, Icons icon, float v)
        {
            DrawIcon(bodyPos, num, icon, new Color(1, v, v));
        }

        private void DrawIcon(Vector3 bodyPos, int num, Icons icon, float v, Color c1, Color c2)
        {         
            DrawIcon(bodyPos, num, icon, Color.Lerp(c1, c2, v));
        }

        private void DrawIcon(Vector3 bodyPos, int num, Icons icon, float v, Color c1, Color c2, Color c3)
        {            
            if (v < 0.5) DrawIcon(bodyPos, num, icon, Color.Lerp(c1, c2, v*2));
            else DrawIcon(bodyPos, num, icon, Color.Lerp(c2, c3, (v-0.5f) * 2));
        }

        public static void recalcIconPositions() {
            // regenerate icon positions
            iconPosVectors = new Vector3[18];
            for (int i = 0; i < iconPosVectors.Length;i++ )
            {
                int n = (i / settings.iconsInColumn);
                int m = (i % settings.iconsInColumn);

                if (settings.iconsHorizontal)
                {
                    int t = n;
                    n = m;
                    m = t;
                }

                iconPosVectors[i] = new Vector3(
                    -0.6f * settings.iconDistanceX - 0.55f * settings.iconSize * settings.iconOffsetX * n, 
                    3.0f,
                    -0.6f * settings.iconDistanceY + 0.55f * settings.iconSize * settings.iconOffsetY * m
                    );                
            }
        }

        public PSI()
        {
            reinit();
        }

        // stats recalculation routine
        public void updateColonistStats(Pawn colonist)
        {
            if (!stats_dict.ContainsKey(colonist)) stats_dict.Add(colonist, new PawnStats());
            PawnStats pawnStats = stats_dict[colonist];
            ///////////////////////////////////////////////////////////////

            pawnStats.isNudist = false;

            foreach (Trait trait in colonist.story.traits.allTraits)
            {
                switch (trait.def.defName)
                {
                    case "Nudist":
                        pawnStats.isNudist = true;
                        break;
                }
            }

            // efficiency
            float efficiency = 10;            

            foreach (PawnCapacityDef act in pawnCapacities)
            {
                
                if (act != PawnCapacityDefOf.Consciousness) efficiency = Math.Min(efficiency, colonist.health.capacities.GetEfficiency(act));
            }

            if (efficiency < 0) efficiency = 0;
            pawnStats.total_efficiency = efficiency;

            // target
            pawnStats.targetPos = Vector3.zero;

            if (colonist.jobs.curJob != null)
            {
                JobDriver curDriver = colonist.jobs.curDriver;
                Job curJob = colonist.jobs.curJob;
                TargetInfo tp = curJob.targetA;

                if (curDriver is JobDriver_HaulToContainer || curDriver is JobDriver_HaulToCell ||
                    curDriver is JobDriver_FoodDeliver || curDriver is JobDriver_FoodFeedPatient ||
                    curDriver is JobDriver_TakeToBed) tp = curJob.targetB;

                if (curDriver is JobDriver_DoBill)
                {
                    JobDriver_DoBill bill = (JobDriver_DoBill)curDriver;
                    if (bill.workLeft == 0.0f) tp = curJob.targetA;
                    else if (bill.workLeft <= 0.01f) tp = curJob.targetB;

                    //Log.Message("" + ((JobDriver_DoBill)colonist.jobs.curDriver).workLeft);
                }

                if (curDriver is JobDriver_Hunt)
                {
                    if (colonist.carryHands != null && colonist.carryHands.CarriedThing != null)
                        tp = curJob.targetB;
                }
                
                if (curJob.def == JobDefOf.Wait) tp = null;
                if (curDriver is JobDriver_Ingest) tp = null;
                if (curJob.def == JobDefOf.LayDown && colonist.InBed()) tp = null;
                if (!curJob.playerForced && curJob.def == JobDefOf.Goto) tp = null;                

                //Log.Message(colonist.jobs.curJob.def.ToString()+" "+colonist.jobs.curDriver.GetType().ToString());

                if (tp != null && tp.Cell != null)
                {
                    Vector3 pos = tp.Cell.ToVector3Shifted();
                    pawnStats.targetPos = pos + new Vector3(0.0f, 3.0f, 0.0f);
                }
            }

            // temperature

            float temper = GenTemperature.GetTemperatureForCell(colonist.Position);
            pawnStats.tooCold = (colonist.ComfortableTemperatureRange().min - settings.limit_tempComfortOffset - temper) / 10.0f;
            pawnStats.tooHot = (temper - colonist.ComfortableTemperatureRange().max - settings.limit_tempComfortOffset) / 10.0f;

            pawnStats.tooCold = Mathf.Clamp(pawnStats.tooCold, 0, 2);
            pawnStats.tooHot = Mathf.Clamp(pawnStats.tooHot, 0, 2);

            // diseases

            pawnStats.diseaseDisappearance = 1;
            pawnStats.drunkness = DrugUtility.DrunknessPercent(colonist);

            using (IEnumerator<Hediff_Staged> enumerator = colonist.health.hediffSet.GetDiseases().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Hediff_Staged disease = enumerator.Current;
                    if (disease == null || disease.FullyImmune || !disease.Visible) continue;
                    if (disease.CurStage != null && !disease.CurStage.everVisible) continue;                    
                    if (!disease.def.Treatable && !disease.def.naturallyHealed) continue;

                    pawnStats.diseaseDisappearance = Math.Min(pawnStats.diseaseDisappearance, disease.Immunity);
                }

            }

            // apparel problems

            float worstApparel = 999f;

            List<Apparel> apparelListForReading = colonist.apparel.WornApparel;

            for (int index = 0; index < apparelListForReading.Count; ++index)
            {
                
                
                float curApparel = (float)apparelListForReading[index].HitPoints / (float)apparelListForReading[index].MaxHitPoints;
                if (curApparel >= 0 && curApparel < worstApparel) worstApparel = curApparel;
            }

            pawnStats.apparelHealth = worstApparel;

            // bloodloss

            pawnStats.bleedRate = Mathf.Clamp01(colonist.health.hediffSet.BleedingRate * settings.limit_bleedMult);

            ////////////////////////////////////////////////////
            stats_dict[colonist] = pawnStats;
        }

        
        public virtual void FixedUpdate()
        {

            fDelta += Time.fixedDeltaTime;
            if (fDelta < 0.1) return; // recalc 10 times per second
            fDelta = 0;
            inGame = GameObject.Find("CameraMap");

            if (inGame && iconsEnabled)
            {
                foreach (Pawn colonist in Find.ListerPawns.ColonistsAndPrisoners)
                {
                    try
                    {
                        updateColonistStats(colonist);
                    }
                    catch (Exception e)
                    {
                        Log.Notify_Exception(e);
                    }
                }
            }

        }

        public void updateOptionsDialog() {
                    Dialog_Options optionsDialog = (Dialog_Options)Find.LayerStack.TopLayerOfType(typeof(Dialog_Options));
                    bool optionsOpened = optionsDialog != null;
                    bool psiSettingsShowed = Find.LayerStack.HasLayerOfType(typeof(Dialog_IconModSettings));
                    if (optionsOpened && psiSettingsShowed)
                    {
                        modSettingsDialog.optionsDialog = optionsDialog;
                        recalcIconPositions();
                    }
                    else if (optionsOpened && !psiSettingsShowed)
                    {
                        if (!modSettingsDialog.closeButtonClicked)
                        {
                            Find.UIRoot.layers.Add(modSettingsDialog);
                            modSettingsDialog.page = "main";
                        }
                        else optionsDialog.Close();
                    }
                    else if (!optionsOpened && psiSettingsShowed)
                    {
                        modSettingsDialog.Close(false);
                    }
                    else if (!optionsOpened && !psiSettingsShowed)
                    {
                        modSettingsDialog.closeButtonClicked = false;
                    }
        }

        public void drawAnimalIcons(Pawn animal)
        {
            int iconNum = 0;
            if (animal.Dead || animal.holder != null) return; // if we are dead or somebody hauls our body
            Vector3 bodyLoc = animal.DrawPos;

            if (settings.showAggressive && (animal.BrokenStateDef == BrokenStateDefOf.Berserk || animal.BrokenStateDef == BrokenStateDefOf.Manhunter))
                DrawIcon(bodyLoc, iconNum++, Icons.Aggressive, Color.red);
        }

        public void drawColonistIcons(Pawn colonist) {
                    PawnStats pawnStats;
                    int iconNum = 0;

                    if (colonist.Dead || colonist.holder != null) return; // if we are dead or somebody hauls our body
                    if (!stats_dict.TryGetValue(colonist, out pawnStats)) return;
                    
                    if (colonist.playerController == null) return;
                    if (colonist.skills == null) return; // Just in case

                    Vector3 bodyLoc = colonist.DrawPos;

                    if (colonist.skills.GetSkill(SkillDefOf.Melee).TotallyDisabled 
                        && colonist.skills.GetSkill(SkillDefOf.Shooting).TotallyDisabled) 
                    {
                        if (settings.showPacific) DrawIcon(bodyLoc, iconNum++, Icons.Pacific, Color.white);                        
                    }
                    else
                    if (settings.showUnarmed && colonist.equipment.Primary == null && !colonist.IsPrisonerOfColony)
                    {
                         DrawIcon(bodyLoc, iconNum++, Icons.Unarmed, Color.white);
                    }

                    if (settings.showIdle && colonist.mindState.IsIdle)
                        DrawIcon(bodyLoc, iconNum++, Icons.Idle, Color.white);
                    if (settings.showDraft && colonist.playerController.Drafted)
                        DrawIcon(bodyLoc, iconNum++, Icons.Draft, Color.white);

            
                    if (settings.showSad && colonist.needs.mood.CurLevel < settings.limit_MoodLess)
                        DrawIcon(bodyLoc, iconNum++, Icons.Sad, colonist.needs.mood.CurLevel / settings.limit_MoodLess);
                    if (settings.showHungry && colonist.needs.food.CurLevel < settings.limit_FoodLess)
                        DrawIcon(bodyLoc, iconNum++, Icons.Hungry, colonist.needs.food.CurLevel / settings.limit_FoodLess);
                    if (settings.showTired && colonist.needs.rest.CurLevel < settings.limit_RestLess)
                        DrawIcon(bodyLoc, iconNum++, Icons.Tired, colonist.needs.rest.CurLevel / settings.limit_RestLess);


                    if (settings.showNaked && !pawnStats.isNudist && colonist.apparel.PsychologicallyNude)
                        DrawIcon(bodyLoc, iconNum++, Icons.Naked, Color.white);


                    if (settings.showCold && pawnStats.tooCold > 0)
                    {
                        if (pawnStats.tooCold < 0) { }
                        else if (pawnStats.tooCold <= 1)
                            DrawIcon(bodyLoc, iconNum++, Icons.Freezing, pawnStats.tooCold, new Color(1.0f, 1.0f, 1.0f, 0.3f), new Color(0.86f, 0.86f, 1f, 1.0f));
                        else if (pawnStats.tooCold <= 1.5)
                            DrawIcon(bodyLoc, iconNum++, Icons.Freezing, (pawnStats.tooCold - 1) * 2.0f, new Color(0.86f, 0.86f, 1.0f, 1.0f), new Color(1f, 0.86f, 0.86f));
                        else
                            DrawIcon(bodyLoc, iconNum++, Icons.Freezing, (pawnStats.tooCold - 1.5f) * 2.0f, new Color(1f, 0.86f, 0.86f), Color.red);

                    }
                    else if (settings.showHot && pawnStats.tooHot > 0)
                    {
                        if (pawnStats.tooCold < 0) { }
                        else if (pawnStats.tooHot <= 1)
                            DrawIcon(bodyLoc, iconNum++, Icons.Hot, pawnStats.tooHot, new Color(1.0f, 1.0f, 1.0f, 0.3f), new Color(1.0f, 0.7f, 0.0f, 1.0f));
                        else
                            DrawIcon(bodyLoc, iconNum++, Icons.Hot, pawnStats.tooHot - 1, new Color(1.0f, 0.7f, 0.0f, 1.0f), Color.red);
                    }

                    
            
                    if (settings.showAggressive && colonist.BrokenStateDef == BrokenStateDefOf.Berserk)
                        DrawIcon(bodyLoc, iconNum++, Icons.Aggressive, Color.red);
                    

                    if (settings.showLeave && colonist.BrokenStateDef == BrokenStateDefOf.GiveUpExit)
                        DrawIcon(bodyLoc, iconNum++, Icons.Leave, Color.red);
                    

                    if (settings.showDazed && colonist.BrokenStateDef == BrokenStateDefOf.DazedWander)
                        DrawIcon(bodyLoc, iconNum++, Icons.Dazed, Color.red);

                    if (colonist.BrokenStateDef == BrokenStateDefOf.PanicFlee)
                        DrawIcon(bodyLoc, iconNum++, Icons.Panic, Color.yellow);

                    if (settings.showDrunk)
                    {
                        if (colonist.BrokenStateDef == BrokenStateDefOf.BingingAlcohol)
                        {
                            DrawIcon(bodyLoc, iconNum++, Icons.Drunk, Color.red);
                        }
                        else
                        if (pawnStats.drunkness > 0.05)
                        {
                            DrawIcon(bodyLoc, iconNum++, Icons.Drunk, pawnStats.drunkness, new Color(1.0f, 1.0f, 1.0f, 0.2f), Color.white, new Color(1.0f, 0.1f, 0.0f));
                        }
                    }
     

                    if (settings.showEffectiveness && pawnStats.total_efficiency < settings.limit_EfficiencyLess)
                        DrawIcon(bodyLoc, iconNum++, Icons.Effectiveness, pawnStats.total_efficiency / settings.limit_EfficiencyLess);
                    if (settings.showDisease && pawnStats.diseaseDisappearance < settings.limit_diseaseLess)
                        DrawIcon(bodyLoc, iconNum++, Icons.Disease, pawnStats.diseaseDisappearance / settings.limit_diseaseLess);
                    if (settings.showBloodloss && pawnStats.bleedRate > 0)
                        DrawIcon(bodyLoc, iconNum++, Icons.Bloodloss, new Color(1, 0, 0, pawnStats.bleedRate));

                    //apparel health
                    if (settings.showApparelHealth && pawnStats.apparelHealth < settings.limit_apparelHealthLess)
                        DrawIcon(bodyLoc, iconNum++, Icons.ApparelHealth, pawnStats.apparelHealth / settings.limit_apparelHealthLess);

                    // Target icon
                    if (settings.showTargetPoint && pawnStats.targetPos != Vector3.zero)
                        DrawIcon(pawnStats.targetPos, Vector3.zero, Icons.Target, targetColor);
            
        }


        public virtual void OnGUI()
        {

            if (!inGame || Find.TickManager.Paused) updateOptionsDialog(); // Options dialog hook

            if (iconsEnabled && inGame)
            {
                foreach (Pawn pawn in Find.ListerPawns.AllPawns) {
                    if (pawn == null) continue;
                    if (pawn.RaceProps == null) continue;

                    if (pawn.RaceProps.Animal) {
                        drawAnimalIcons(pawn);
                    }
                    else
                    if (pawn.IsColonist || pawn.IsPrisonerOfColony) {
                        drawColonistIcons(pawn);
                    }
                }

                /*
                foreach (Pawn colonist in Find.ListerPawns.ColonistsAndPrisoners)
                {
                    drawColonistIcons(colonist);
                }
                */
                
            }

        }

        // draw loop
        public virtual void Update()
        {
            if (inGame)
            {

                if (Input.GetKeyUp(KeyCode.F11))
                {
                    iconsEnabled = !iconsEnabled;

                    if (iconsEnabled)
                        Messages.Message(Translator.Translate("PSI.Enabled"), MessageSound.Standard);
                    else
                        Messages.Message(Translator.Translate("PSI.Disabled"), MessageSound.Standard);
                }

                worldScale = Screen.height / (2 * Camera.current.orthographicSize);
            }

          //  if (Input.GetKeyUp(KeyCode.RightControl))
            if (false)
            {
                IntVec2 wSize = Find.World.Size;
                float minTemp = float.MaxValue;
                IntVec2 minTempPos = new IntVec2();
                

                for (int x = 0; x < wSize.x; x++)
                for (int z = 0; z < wSize.z; z++)
                {                    
                    
                    WorldSquare ws = Find.World.grid.Get(new IntVec2(x,z));
//                    if (ws.biome == BiomeDefOf.IceSheet)
                    //{
                        //ws.biome = BiomeDefOf.Tundra;
                    ws.temperature -= 1;
                    //}

/*                    if (ws != null)
                    {
                        if (ws.temperature < minTemp && ws.biome.canBuildBase == true)
                        {
                            minTemp = ws.temperature;
                            minTempPos = new IntVec2(x, z);
                            Log.Message(minTemp + "\t" + minTempPos.ToString());                         
                        }
                    }*/
                }
            
            }

        }

    }
}
