using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace LetsGoExplore
{
    public class WorldObjectCompProperties_PrisonerRescueQuest : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_PrisonerRescueQuest()
        {
            this.compClass = typeof(PrisonerRescueQuestComp);
        }

        public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
        {
            foreach (string e in base.ConfigErrors(parentDef))
            {
                yield return e;
            }
            if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
            {
                yield return parentDef.defName + " has WorldObjectCompProperties_PrisonerRescueQuest but it's not MapParent.";
            }
            yield break;
        }
    }

    public class PrisonerRescueQuestComp : WorldObjectComp, IThingHolder
    {
        private bool active;

        public Faction requestingFaction;

        public int relationsImprovement;

        public ThingOwner rewards;

        private static List<Thing> tmpRewards = new List<Thing>();

        private bool ranDebug = false;

        public PrisonerRescueQuestComp()
        {
            this.rewards = new ThingOwner<Thing>(this);
        }

        public bool Active
        {
            get
            {
                return this.active;
            }
        }

        public void StartQuest(Faction requestingFaction, int relationsImprovement, List<Thing> rewards)
        {
            this.StopQuest();
            this.active = true;
            this.requestingFaction = requestingFaction;
            this.relationsImprovement = relationsImprovement;
            this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
            this.rewards.TryAddRangeOrTransfer(rewards, true, false);
        }

        public void StopQuest()
        {
            this.active = false;
            this.requestingFaction = null;
            this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<bool>(ref this.ranDebug, "ranDebug", false, false);
            Scribe_Values.Look<int>(ref this.relationsImprovement, "relationsImprovement", 0, false);
            Scribe_References.Look<Faction>(ref this.requestingFaction, "requestingFaction", false);
            Scribe_Deep.Look<ThingOwner>(ref this.rewards, "rewards", new object[]
            {
                this
            });
        }

        public void GiveRewardsAndSendLetter()
        {
            Map map = Find.AnyPlayerHomeMap ?? ((MapParent)this.parent).Map;
            PrisonerRescueQuestComp.tmpRewards.AddRange(this.rewards);
            this.rewards.Clear();
            IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, PrisonerRescueQuestComp.tmpRewards, 110, false, false, false);
            PrisonerRescueQuestComp.tmpRewards.Clear();
            FactionRelationKind playerRelationKind = this.requestingFaction.PlayerRelationKind;
            TaggedString text = "LetterPrisonerQuestCompletedGLE".Translate(this.requestingFaction.Name, this.relationsImprovement.ToString());
            this.requestingFaction.TryAffectGoodwillWith(Faction.OfPlayer, this.relationsImprovement, false, false, null, null);
            this.requestingFaction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, this.requestingFaction.PlayerRelationKind, null);
            Find.LetterStack.ReceiveLetter("LetterLabelDefeatAllEnemiesQuestCompleted".Translate(), text, LetterDefOf.PositiveEvent, new GlobalTargetInfo(intVec, map, false), this.requestingFaction, null);
            this.StopQuest();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.rewards;
        }

        public override void PostPostRemove()
        {
            base.PostPostRemove();
            this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
        }

        public override string CompInspectStringExtra()
        {
            if (this.active)
            {
                string value = GenThing.ThingsToCommaList(this.rewards, true, true, 5).CapitalizeFirst();
                return "QuestTargetDestroyInspectString".Translate(this.requestingFaction.Name, value, GenThing.GetMarketValue(this.rewards).ToStringMoney(null)).CapitalizeFirst();
            }
            return null;
        }

        public override void CompTick()
        {
            //fix the bug with the wrong site and comp
            base.CompTick();

            if (ranDebug != true)
            {
                List<WorldObject> wObjects = Find.WorldObjects.AllWorldObjects;
                foreach (WorldObject obj in wObjects)
                {
                    DefeatAllEnemiesQuestComp comp = obj.GetComponent<DefeatAllEnemiesQuestComp>();
                    if (comp != null)
                    {
                        if (comp.requestingFaction == null)
                        {
                            comp.StopQuest();
                        }
                    }
                }
            }

            /*
            MapParent mapParent = this.parent as MapParent;
            if (mapParent != null)
            {
                DefeatAllEnemiesQuestComp comp = mapParent.GetComponent<DefeatAllEnemiesQuestComp>();
                if (comp != null)
                {
                    comp.StopQuest();
                }
            }
            */
        }

    }
}
