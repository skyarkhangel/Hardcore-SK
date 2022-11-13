using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;

namespace LetsGoExplore
{
    public class MotherAmbrosiaLGE : Plant
    {
        private List<Pawn> spawnedPawns = new List<Pawn>();


        private Lord Lord
        {
            get
            {
                Predicate<Pawn> hasDefendHiveLord = delegate (Pawn x)
                {
                    Lord lord = x.GetLord();
                    return lord != null && lord.LordJob is LordJob_AmbrosiaDefenseLGE;
                };
                Pawn foundPawn = this.spawnedPawns.Find(hasDefendHiveLord);
                if (base.Spawned)
                {
                    if (foundPawn == null)
                    {
                        RegionTraverser.BreadthFirstTraverse(this.GetRegion(RegionType.Set_Passable), (Region from, Region to) => true, delegate (Region r)
                        {
                            List<Thing> list = r.ListerThings.ThingsOfDef(DefsOfLGE.Plant_MotherAmbrosiaLGE);
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (list[i] != this)
                                {
                                        foundPawn = ((MotherAmbrosiaLGE)list[i]).spawnedPawns.Find(hasDefendHiveLord);
                                        if (foundPawn != null)
                                        {
                                            return true;
                                        }
                                }
                            }
                            return false;
                        }, 20, RegionType.Set_Passable);
                    }
                    if (foundPawn != null)
                    {
                        return foundPawn.GetLord();
                    }
                }
                return null;
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = base.Map;
            base.DeSpawn();
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                lords[i].ReceiveMemo(Hive.MemoDeSpawned);
            }
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
            {
                Lord lord = this.Lord;
                if (lord != null)
                {
                    lord.ReceiveMemo(Hive.MemoAttackedByEnemy);
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        private Lord CreateNewLord()
        {
            return LordMaker.MakeNewLord(base.Faction, new LordJob_AmbrosiaDefenseLGE(), base.Map, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.Reference, new object[0]);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.spawnedPawns.RemoveAll((Pawn x) => x == null);
            }
        }

        public void SpawnAnimals(PawnKindDef animalsKind, float points)
        {
            float curPoints = 0;
            while (curPoints < points)
            {
                Pawn pawn;
                pawn = PawnGenerator.GeneratePawn(animalsKind, Faction.OfInsects);
                pawn.health.AddHediff(ThingDefOfVanilla.AmbrosiaAddiction);
                this.spawnedPawns.Add(pawn);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 6, null), base.Map, WipeMode.Vanish);
                Lord lord = this.Lord;
                if (lord == null)
                {
                    lord = this.CreateNewLord();
                }
                lord.AddPawn(pawn);
                curPoints += animalsKind.combatPower;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate ()
                    {
                        this.SpawnAnimals(ThingDefOfVanilla.Warg, 400f);
                    }
                };
            }
            yield break;
        }
    }


    public class LordJob_AmbrosiaDefenseLGE : LordJob
    {
        public override bool CanBlockHostileVisitors
        {
            get
            {
                return false;
            }
        }

        public override bool AddFleeToil
        {
            get
            {
                return false;
            }
        }

        public override StateGraph CreateGraph()
        {
            //TODO: This might use some love to expand the Job a bit. Pawn harmed for example. There are some triggers and states that weren't copied.
            StateGraph stateGraph = new StateGraph();
            LordToil_AmbroisaDefenseLGE lordToil_DefendAmbrosia = new LordToil_AmbroisaDefenseLGE();
            lordToil_DefendAmbrosia.distToHiveToAttack = 20f;
            stateGraph.StartingToil = lordToil_DefendAmbrosia;
            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
            stateGraph.AddToil(lordToil_AssaultColony);
            Transition transition = new Transition(lordToil_DefendAmbrosia, lordToil_AssaultColony);
            transition.AddTrigger(new Trigger_PawnHarmed(0.5f, true));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddTrigger(new Trigger_Memo(Hive.MemoAttackedByEnemy));
            transition.AddTrigger(new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition);
            Transition transition2 = new Transition(lordToil_DefendAmbrosia, lordToil_AssaultColony);
            transition2.canMoveToSameState = true;
            transition2.AddSource(lordToil_AssaultColony);
            transition2.AddTrigger(new Trigger_Memo(Hive.MemoDeSpawned));
            stateGraph.AddTransition(transition2);
            Transition transition3 = new Transition(lordToil_AssaultColony, lordToil_DefendAmbrosia);
            transition3.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1200, new string[]
            {
                Hive.MemoAttackedByEnemy,
                HediffGiver_Heat.MemoPawnBurnedByAir
            }));
            transition3.AddPostAction(new TransitionAction_EndAttackBuildingJobs());
            stateGraph.AddTransition(transition3);
            return stateGraph;
        }
    }

    public class LordToil_AmbroisaDefenseLGE : LordToil
    {
        public float distToHiveToAttack = 10f;

        private LordToilData_AmbrosiaDefenseLGE Data
        {
            get
            {
                return (LordToilData_AmbrosiaDefenseLGE)this.data;
            }
        }

        public LordToil_AmbroisaDefenseLGE()
        {
            this.data = new LordToilData_AmbrosiaDefenseLGE();
        }

        public override void UpdateAllDuties()
        {
            this.FilterOutUnspawnedHives();
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                MotherAmbrosiaLGE hiveFor = this.GetHiveFor(this.lord.ownedPawns[i]);
                PawnDuty duty = new PawnDuty(DefsOfLGE.DefendAmbrosiaSproutLGE, hiveFor, this.distToHiveToAttack);
                this.lord.ownedPawns[i].mindState.duty = duty;
            }
        }

        private void FilterOutUnspawnedHives()
        {
            this.Data.assignedHives.RemoveAll((KeyValuePair<Pawn, MotherAmbrosiaLGE> x) => x.Value == null || !x.Value.Spawned);
        }

        private MotherAmbrosiaLGE GetHiveFor(Pawn pawn)
        {
            MotherAmbrosiaLGE hive;
            if (this.Data.assignedHives.TryGetValue(pawn, out hive))
            {
                return hive;
            }
            hive = this.FindClosestHive(pawn);
            if (hive != null)
            {
                this.Data.assignedHives.Add(pawn, hive);
            }
            return hive;
        }

        private MotherAmbrosiaLGE FindClosestHive(Pawn pawn)
        {
            return (MotherAmbrosiaLGE)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(DefsOfLGE.Plant_MotherAmbrosiaLGE), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, null, null, 0, 30, false, RegionType.Set_Passable, false);
        }
    }

    public class LordToilData_AmbrosiaDefenseLGE : LordToilData
    {
        public Dictionary<Pawn, MotherAmbrosiaLGE> assignedHives = new Dictionary<Pawn, MotherAmbrosiaLGE>();

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.assignedHives.RemoveAll((KeyValuePair<Pawn, MotherAmbrosiaLGE> x) => x.Key.Destroyed);
            }
            Scribe_Collections.Look<Pawn, MotherAmbrosiaLGE>(ref this.assignedHives, "assignedHives", LookMode.Reference, LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.assignedHives.RemoveAll((KeyValuePair<Pawn, MotherAmbrosiaLGE> x) => x.Value == null);
            }
        }
    }

    public class JobGiver_AmbrosiaDefenseLGE : JobGiver_HiveDefense
    {
        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            MotherAmbrosiaLGE hive = pawn.mindState.duty.focus.Thing as MotherAmbrosiaLGE;
            if (hive != null && hive.Spawned)
            {
                return hive.Position;
            }
            return pawn.Position;
        }
    }

    public class JobGiver_WanderAmbrosiaLGE : JobGiver_Wander
    {
        public JobGiver_WanderAmbrosiaLGE()
        {
            this.wanderRadius = 7.5f;
            this.ticksBetweenWandersRange = new IntRange(125, 200);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            MotherAmbrosiaLGE hive = pawn.mindState.duty.focus.Thing as MotherAmbrosiaLGE;
            if (hive == null || !hive.Spawned)
            {
                return pawn.Position;
            }
            return hive.Position;
        }
    }
}
