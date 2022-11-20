using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Autolinks
{
    [StaticConstructorOnStartup]
    public class Autolinks
    {
        static void AddLink(Def def, Def toAdd)
        {
            if (def==null || toAdd == null) return;
            if (def.descriptionHyperlinks == null) def.descriptionHyperlinks = new List<DefHyperlink>();

            if (def.descriptionHyperlinks.Any(x => x.def == toAdd)) return;

            def.descriptionHyperlinks.Add(toAdd);
        }
        static void AddMulti(Def def, Def toAdd)
        {
            AddLink(def, toAdd);
            AddLink(toAdd, def);
        }

        static void HandleResearch(ResearchProjectDef researchPrerequisite, Def unlock)
        {
            if (researchPrerequisite == null || unlock==null) return;

            AddMulti(unlock, researchPrerequisite);
        }
        static void HandleResearch(List<ResearchProjectDef> researchPrerequisites, Def unlock)
        {
            if (researchPrerequisites != null) foreach (var v in researchPrerequisites) HandleResearch(v, unlock);
        }

        static void ProcessRecipe(RecipeDef def)
        {
            HandleResearch(def.researchPrerequisite, def);
            HandleResearch(def.researchPrerequisites, def);

            if (def.products != null)
            {
                foreach (ThingDefCountClass td in def.products)
                {
                    if (td.thingDef == null) continue;

                    AddMulti(def, td.thingDef);
                }
            }

            if (def.recipeUsers != null)
            {
                foreach (ThingDef workbench in def.recipeUsers.Where(x => x.race == null))
                {
                    AddMulti(def, workbench);
                }
            }

            if (def.ingredients != null)
            {
                foreach (IngredientCount ing in def.ingredients)
                {
                    if (! ing.IsFixedIngredient) continue;

                    AddMulti(def, ing.FixedIngredient);
                }
            }

            if (def.appliedOnFixedBodyParts != null) foreach (var v in def.appliedOnFixedBodyParts) AddMulti(def, v);
            if (def.appliedOnFixedBodyPartGroups != null) foreach (var v in def.appliedOnFixedBodyPartGroups) AddMulti(def, v);
        }

        static void ProcessThing(ThingDef def)
        {
            CompProperties_Hatcher hatcher = def.GetCompProperties<CompProperties_Hatcher>();
            if (hatcher != null)
            {
                AddMulti(def, hatcher.hatcherPawn?.race);
            }

            Def corpseDef = def.race?.corpseDef;
            if (corpseDef != null && !def.race.Humanlike)
            {
                AddLink(corpseDef, def);
            }

            BuildableDef buildable = def as BuildableDef;
            if (buildable == null) return;

            HandleResearch(def.researchPrerequisites, def);

            if (buildable.costList != null)
            {
                foreach (ThingDefCountClass td in buildable.costList)
                {
                    if (td.thingDef == null) continue;

                    AddMulti(def, td.thingDef);
                }
            }
        }

        static Autolinks()
        {
            //new Harmony("com.github.automatic1111.autolinks").PatchAll(Assembly.GetExecutingAssembly());

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                ProcessThing(def);
            }

            foreach (RecipeDef def in DefDatabase<RecipeDef>.AllDefs)
            {
                ProcessRecipe(def);
            }
        }
    }
}
