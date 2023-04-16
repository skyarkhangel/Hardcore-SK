
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace Minerals
{

    public class IncidentWorker_BlueSnow : IncidentWorker_MakeGameCondition
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            if (MineralsMain.Settings.includeFictionalSetting == false)
            {
                return false;
            }

            Map map = (Map)parms.target;

            if (map.mapTemperature.OutdoorTemp > 0f || map.mapTemperature.OutdoorTemp < -20 || map.weatherManager.SnowRate < 0.1f)
            {
                return false;
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Map map = (Map)parms.target;
            //Find.LetterStack.ReceiveLetter(this.def.letterLabel, def.letterText, LetterDefOf.NeutralEvent);
            parms.customLetterDef = LetterDefOf.NeutralEvent;
            parms.customLetterLabel = def.letterLabel;
            parms.customLetterText = def.letterText;
            //map.gameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(DefDatabase<GameConditionDef>.GetNamed("BlueSnowCondition")));
            return base.TryExecuteWorker(parms); // for some reason, this causes an error even though it works
        }


    }


    public class GameCondition_BlueSnow : GameCondition
    {

        public ThingDef_DynamicMineral coldstoneDef = DefDatabase<ThingDef_DynamicMineral>.GetNamed("ColdstoneCrystal");
        public int ticksPerSpawn = 100;
        public int currentTick = 1;

        public override float SkyGazeChanceFactor(Map map)
        {
            return base.SkyGazeChanceFactor(map) * 2;
        }

        public override float SkyGazeJoyGainFactor(Map map)
        {
            return base.SkyGazeJoyGainFactor(map) * 2;
        }
            
        public override void GameConditionTick()
        {
            currentTick += 1;
            foreach (Map aMap in this.AffectedMaps)
            {
                if (aMap.weatherManager.curWeather.defName != "BlueSnow")
                {
                    int previousWeatherAge = aMap.weatherManager.curWeatherAge;
                    aMap.weatherManager.TransitionTo(DefDatabase<WeatherDef>.GetNamed("BlueSnow"));
                    if (previousWeatherAge < 4000)
                    {
                        aMap.weatherManager.curWeatherAge = 4000 - previousWeatherAge;
                    }
                }
                if (aMap.mapTemperature.OutdoorTemp > 5f || aMap.mapTemperature.OutdoorTemp < -50f)
                {
                    this.End();
                }
                if (currentTick > ticksPerSpawn)
                {
                    IntVec3 spawnPos = CellFinder.RandomCell(aMap);
                    coldstoneDef.TrySpawnAt(spawnPos, aMap, 0.2f);
                    currentTick = 1;
                }
            }
        }
            
    }


    public class WeatherOverlay_BlueSnow : WeatherOverlay_SnowHard
    {

        static Material SnowOverlayWorld;

        public WeatherOverlay_BlueSnow()
        {
            this.worldOverlayMat = WeatherOverlay_BlueSnow.SnowOverlayWorld;
            this.worldOverlayPanSpeed1 = 0.005f;
            this.worldPanDir1 = new Vector2(-0.3f, -1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.006f;
            this.worldPanDir2 = new Vector2(-0.29f, -1f);
            this.worldPanDir2.Normalize();
            this.OverlayColor = new Color(0.3f,0.3f,1f);
        }

        static WeatherOverlay_BlueSnow()
        {
            WeatherOverlay_BlueSnow.SnowOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);
        }
    }


}
