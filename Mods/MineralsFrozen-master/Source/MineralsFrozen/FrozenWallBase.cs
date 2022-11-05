
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace MineralsFrozen
{
    /// <summary>
    /// FrozenWallBase class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class FrozenWallBase : Building
    {

        // Cache for texture indexes
        protected int textureIndex = -100;


        public virtual ThingDef_FrozenWallBase attributes
        {
            get
            {
                return def as ThingDef_FrozenWallBase;
            }
        }

        public virtual float currentTemp
        {
            get
            {

                float outTemp;
                if (GenTemperature.TryGetAirTemperatureAroundThing(this, out outTemp))
                {
                    return outTemp;
                }
                else
                {
                    if (Position.Roofed(Map))
                    {
                        return Map.mapTemperature.OutdoorTemp - 10f;
                    }
                    else
                    {
                        return Map.mapTemperature.OutdoorTemp - 5f;
                    }
                }

            }
        }

        // https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object/2742288
        public static bool isSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                || potentialDescendant == potentialBase;
        }

        public static bool isFrozenWall(Thing thing)
        {
            return isSameOrSubclass(typeof(FrozenWallBase), thing.GetType());
        }

        public virtual bool isMelting
        {
            get
            {
                return currentTemp > attributes.meltTemp;
            }
        }

        public virtual bool isHealing
        {
            get
            {
                return currentTemp < attributes.healTemp && canHeal;
            }
        }

        public virtual bool canHeal
        {
            get
            {
                return (HitPoints < MaxHitPoints) && (HitPoints >= Math.Ceiling(MaxHitPoints * attributes.maxHealHP));
            }
        }

        public virtual float currentMeltRate
        {
            get
            {
                float temp = currentTemp;
                float rate = 0f;
                if (temp > attributes.meltTemp)
                {
                    rate = ((temp - attributes.meltTemp) / 20) * attributes.meltRate;
                }
                if (temp < attributes.healTemp && canHeal)
                {
                    rate = - ((attributes.healTemp - temp) / 20) * attributes.healRate;
                }
                if (Math.Abs(rate) > attributes.maxChangeRate)
                {
                    rate = attributes.maxChangeRate * Math.Sign(rate);
                }
                return rate;
            }
        }


        public override void TickLong()
        {
            
            float rate = currentMeltRate;
            if (Math.Abs(rate) > 0)
            {
              // calculate hit points lossed/gained
              float rateInHitpoints = rate * MaxHitPoints;
              if (Math.Abs(rateInHitpoints) < 1)
              {
                if (Rand.Range(0f, 1f) < Math.Abs(rateInHitpoints))
                {
                  rateInHitpoints = Math.Sign(rateInHitpoints);
                }
                else
                {
                  rateInHitpoints = 0f;
                }
              }
              else
              {
                rateInHitpoints = (float) Math.Floor(rateInHitpoints);
              }
            
              // Dont heal past max health
              float newHitpoints = HitPoints - rateInHitpoints;
              if (newHitpoints > MaxHitPoints)
              {
                rateInHitpoints = MaxHitPoints - HitPoints;
              }
              
              // Apply health and temperature change
              if (Math.Abs(rateInHitpoints) > 0) 
              {
                float tempChange = (rateInHitpoints / MaxHitPoints) * attributes.maxStoredHeat;
                GenTemperature.PushHeat(this, -tempChange);
                TakeDamage(new DamageInfo(DamageDefOf.Deterioration, rateInHitpoints, 0, -1, null, null, null));
              }
            }
        }


        public virtual void initializeTextures() {
            Rand.PushState();
            Rand.Seed = Position.GetHashCode();
            textureIndex = Rand.Range(0, attributes.getTexturePaths().Count);
            Rand.PopState();
        }

        public virtual string getTexturePath()
        {
            // initalize the array if it has not already been initalized
            if (textureIndex == -100)
            {
                initializeTextures();
            }

            return(attributes.getTexturePaths()[textureIndex]);
        }

        public override void Print(SectionLayer layer)
        {
             Rand.PushState();
             Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode();
             base.Print(layer);
             Rand.PopState();
        }

        public override Graphic Graphic
        {
            get
            {
                // Pick a random path 
                string printedTexturePath = getTexturePath();
                Graphic printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexturePath, attributes.graphicData.shaderType.Shader);

                // convert to corner filler
                printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexture.path, printedTexture.Shader, printedTexture.drawSize, DrawColor, DrawColorTwo, printedTexture.data);
                return new Graphic_LinkedCornerFiller(printedTexture);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Effective temperature: " + Math.Round(currentTemp) + "C");
            stringBuilder.AppendLine("Melt Rate: " + currentMeltRate);
            if (isMelting)
            {
                stringBuilder.AppendLine("Melting.");
            }
            else if (isHealing)
            {
                stringBuilder.AppendLine("Freezing.");
            }
            else
            {
                stringBuilder.AppendLine("Frozen.");
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }


    }       


    /// <summary>
    /// ThingDef_FrozenWallBase class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_FrozenWallBase : ThingDef
    {
        public List<string> texturePaths;

        // Maximum stable temperature
        public float meltTemp = 1f;
        // Minimum temperature the wall will heal
        public float healTemp = -1f;
        // Minimum proportion of hit point needed to heal
        public float maxHealHP = 0.95f;
        // The proportion of health restored each tick at 20C below heal temperature
        public float healRate = 0.1f;
        // The proportion of health lossed each tick at 20C above melt temperature
        public float meltRate = 0.1f;
        // The proportion of health lossed each tick at 20C above melt temperature
        public float maxChangeRate = 0.2f;
        // The difference in stored energy between the solid and liquid
        public float maxStoredHeat = 1000f;

        public virtual void initTexturePaths()
        {
            // Get paths to textures
            string textureName = System.IO.Path.GetFileName(graphicData.texPath);
            texturePaths = new List<string> { };
            List<string> versions = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
            foreach (string letter in versions)
            {
                string a_path = graphicData.texPath + "/" + textureName + letter;
                if (ContentFinder<Texture2D>.Get(a_path, false) != null)
                {
                    texturePaths.Add(a_path);
                }
            }

        }

        public virtual List<string> getTexturePaths()
        {
            if (texturePaths == null)
            {
                initTexturePaths();
            }
            return texturePaths;
        }


    }
}
