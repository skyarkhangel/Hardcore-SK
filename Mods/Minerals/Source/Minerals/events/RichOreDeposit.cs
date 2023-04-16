
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using RimWorld.Planet;

namespace Minerals
{

    public class IncidentWorker_RichOreDeposit : IncidentWorker
    {
        private ThingDef oreToSpawn;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            return Find.FactionManager.RandomNonHostileFaction(false, false, false, TechLevel.Undefined) != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            // Pick ore type
            List<ThingDef> possibleOres = new List<ThingDef>();  
            foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs)
            {
                if (thing.mineable && thing.building.isResourceRock && thing.building.isNaturalRock && thing.building.mineableScatterCommonality > 0)
                {
                    possibleOres.Add(thing);
                    //Log.Message("RichOreDeposit: possible ore: " + thing);
                }
            }
            oreToSpawn = possibleOres.RandomElementByWeight((ThingDef x) => x.building.mineableScatterCommonality);

            // Find faction that sent the info 
            Faction referringFaction = parms.faction;
            if (referringFaction == null)
            {
                referringFaction = Find.FactionManager.RandomNonHostileFaction(false, false, false, TechLevel.Undefined);
            }
            if (referringFaction == null)
            {
                return false;
            }

            // Find tile to spawn world destination
            IntRange targetDistanceRange = new IntRange(3, 25);
            int targetTile;
            if (!TileFinder.TryFindNewSiteTile(out targetTile, targetDistanceRange.min, targetDistanceRange.max))
            {
                return false;
            }

            // Spawn world location
            RichOreDepositWorldObject site = (RichOreDepositWorldObject)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("ZF_RichOreDeposit"));
            site.Tile = targetTile;
            site.SetFaction(null);
            Find.WorldObjects.Add(site);

            // Start resource countdown timer (Others mining the ore)
            RichOreDepositComp comp = site.GetComponent<RichOreDepositComp>();
            if (comp == null)
            {
                Debug.LogWarning("Component is null");
            }
            else
            {
                //comp.StartScavenging(cost);
            }

            // Show letter
            string letterText = string.Format(def.letterText, referringFaction.leader.LabelShort, referringFaction.def.leaderTitle, referringFaction.Name, oreToSpawn.label).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(def.letterLabel, letterText, def.letterDef, site, referringFaction, null);
            return true;
        }

    }

    public class WorldObjectCompProperties_RichOreDeposit : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_RichOreDeposit()
        {
            compClass = typeof(RichOreDepositComp);
        }
    }

    public class RichOreDepositComp : WorldObjectComp
    {

    }

    class RichOreDepositWorldObject : MapParent
    {
        public override Texture2D ExpandingIcon => ContentFinder<Texture2D>.Get("ruinedbase");
        public override Color ExpandingIconColor => Color.white;
        private Material cachedMat;

        private bool hasStartedCountdown = false;

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(color: Faction?.Color ?? Color.white, texPath: "World/WorldObjects/Sites/GenericSite", shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
                }
                return cachedMat;
            }
        }

        public RichOreDepositWorldObject()
        {
        }

        public override void Tick()
        {
            base.Tick();
            if (HasMap && !hasStartedCountdown)
            {
                GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown(15 * 60000);
                hasStartedCountdown = true;
            }
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            bool shouldRemove = !Map.mapPawns.AnyPawnBlockingMapRemoval;
            alsoRemoveWorldObject = shouldRemove;
            return shouldRemove;
        }

        public override string GetInspectString()
        {
            return base.GetInspectString();
        }
    }


}
