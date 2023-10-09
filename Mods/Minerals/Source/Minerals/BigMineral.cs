using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace Minerals
{
    /// <summary>
    /// BigMineral class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class BigMineral : StaticMineral
    {
        
        public float propExtracted = 0f;

        public new ThingDef_BigMineral attributes
        {
            get
            {
                return base.attributes as ThingDef_BigMineral;
            }
        }

        public override void incPctYeild(float amount, Pawn miner)
        {
            float minerYield = 1f;
            if (miner.def.race.IsMechanoid)
            {
                minerYield = ((SkillNeed_Direct)DefDatabase<StatDef>.GetNamed("MiningYield").skillNeedFactors.Find(s => s.skill.defName == "Mining" && s is SkillNeed_Direct)).valuesPerLevel[miner.RaceProps.mechFixedSkillLevel];
            }
            else
            {
                minerYield = miner.GetStatValue(StatDefOf.MiningYield, true);
            }
            propExtracted += (float)Mathf.Min(amount, HitPoints) / (float)MaxHitPoints * minerYield + (minerYield -  attributes.extractionDifficulty) * 0.2f + Rand.Range(-0.2f, 0.2f);
            if (propExtracted >= 1f)
            {
                propExtracted = 0f;
                ThingDef myThingDef = DefDatabase<ThingDef>.GetNamed(attributes.defName + "Trophy", true);
                if (myThingDef != null)
                {
                    BigMineralTrophy thing = (BigMineralTrophy)ThingMaker.MakeThing(myThingDef, null);
                    Thing miniThing = thing.MakeMinified();
                    GenPlace.TryPlaceThing(miniThing, Position, Map, ThingPlaceMode.Near, null);
                    Messages.Message("Rare mineral trophy extracted!".CapitalizeFirst(), MessageTypeDefOf.NeutralEvent, true);
                    yieldPct = 0.1f;
                }
            }
            else
            {
                base.incPctYeild(amount, miner);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (DebugSettings.godMode)
            {
                stringBuilder.AppendLine("Size: " + size.ToStringPercent());
                stringBuilder.AppendLine("Extraction progress: " + propExtracted.ToStringPercent());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

    }       


    /// <summary>
    /// ThingDef_StaticMineral class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_BigMineral : ThingDef_StaticMineral
    {
        // The radius that will be searched to replace things
        public int replaceRadius = 1;
        // The minmum propotion of things in radius to replace for a replacement to happen 
        public float repalceThreshold = 0.3f;
        // How likly an extraction is to be successful 
        public float extractionDifficulty = 0.9f;

        public override Thing ThingToReplaceAtPos(Map map, IntVec3 position)
        {
            // if (defName == "BigColdstoneCrystal") Log.Message("ThingToReplaceAtPos (bg): checking for " + defName +  " at " + position);
            Thing toReplace = base.ThingToReplaceAtPos(map, position);
            if (toReplace == null)
            {
                return(null);
            }
            // if (defName == "BigColdstoneCrystal") Log.Message("ThingToReplaceAtPos (bg): found thing to replace at " + position, true);
            int spotsChecked = 0;
            float replaceCount = 0;
            for (int xOffset = -replaceRadius; xOffset <= replaceRadius; xOffset++)
            {
                for (int zOffset = -replaceRadius; zOffset <= replaceRadius; zOffset++)
                {
                    spotsChecked = spotsChecked + 1;
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        foreach (Thing thing in map.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if (thing == null || thing.def == null)
                            {
                                continue;
                            }

                            if (ThingsToReplace.Any(thing.def.defName.Equals))
                            {
                                if (StaticMineral.isMineral(thing))
                                {
                                    replaceCount += ((StaticMineral) thing).size;
                                }
                                else
                                {
                                    replaceCount += 1;
                                }
                            }
                        }
                    }
                }
            }
            if (((float)replaceCount) / ((float)spotsChecked) > repalceThreshold)
            {
                //Log.Message(this.defName + " can replace at " + position, true);
                return(toReplace);
            }
            else
            {
                //Log.Message(this.defName + " can not replace at " + position + " with density " + ((float)replaceCount) / ((float)spotsChecked), true);
                return(null);
            }

        }


    }




    /// <summary>
    /// BigMineralTrophy class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class BigMineralTrophy : Building
    {
        // the path to the image printed
        public string printedTexturePath = null;
    
        public override void Print(SectionLayer layer)
        {
            Rand.PushState();
            Rand.Seed = ThingID.GetHashCode();

            // Get location
            Vector3 center = this.TrueCenter();
            if (def.graphicData.drawSize.y > def.size.z)
            {
                center.z += (def.graphicData.drawSize.y - def.size.z) / 2;
            }

            // Print image
            Material matSingle = Graphic.MatSingle;
            Printer_Plane.PrintPlane(layer, center, def.graphicData.drawSize, matSingle, 0, Rand.Bool, null, null, 0.01f, 0f);

            Rand.PopState();
        }

        public override Graphic Graphic
        {
            get
            {
                string printedTexturePath = getTexturePath();
                Graphic printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexturePath, def.graphicData.shaderType.Shader);
                printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexture.path, printedTexture.Shader, printedTexture.drawSize, DrawColor, DrawColorTwo, printedTexture.data);
                return printedTexture;
            }
        }

        public virtual void initTexturePath()
        {
            // Get paths to textures
            string textureName = System.IO.Path.GetFileName(def.graphicData.texPath);
            List<string> texturePaths = new List<string> { };
            List<string> versions = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            foreach (string letter in versions)
            {
                string a_path = def.graphicData.texPath + "/" + textureName + letter;
                if (ContentFinder<Texture2D>.Get(a_path, false) != null)
                {
                    texturePaths.Add(a_path);
                }
            }

            // Pick a random texture
            printedTexturePath = texturePaths.RandomElement();

        }

        public virtual string getTexturePath()
        {
            // initalize the array if it has not already been initalized
            if (printedTexturePath == null)
            {
                initTexturePath();
            }

            return(printedTexturePath);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref printedTexturePath, "printedTexturePath", null);
        }
    }       


    /// <summary>
    /// ThingDef_BigMineralTrophy class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_BigMineralTrophy : ThingDef
    {


    }




}
