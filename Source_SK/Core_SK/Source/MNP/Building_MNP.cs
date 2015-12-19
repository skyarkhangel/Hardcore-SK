using Verse;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SK_MNP
{
    public class Building_MNP : Building_WorkTable
    {
        private int Burnticks = 50;
        private int damageSelfCounterStart = 600;
        private int damageAppliedSelfMin;
        private int damageAppliedSelfMax;
        private int damageAppliedSelf;
        private int doDamageSelfCounter;
        private int overheartThreshold;
        private int burnDelay;
        private int pufferSmoke;
        private System.Random random = new System.Random();

        public CompPowerTrader powerComp;
        public CompGlower glowerComp;

        public bool IsFlareActive
        {
            get
            {
                return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare);
            }
        }

        public bool CanBurnNow
        {
            get
            {
                return this.HasFuelInHopper && IsFlareActive;
            }
        }

        public bool HasFuelInHopper
        {
            get
            {
                return this.FuelInHopper != null;
            }
        }

        public Building AnyAdjacentHopper
        {
            get
            {
                ThingDef thingDef = ThingDef.Named("NuclearFeeder");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    Building building = current.GetEdifice();
                    if (building != null && building.def == thingDef)
                    {
                        return (Building_Storage)building;
                    }
                }
                return null;
            }
        }
        private Thing FuelInHopper
        {
            get
            {
                ThingDef thingdef = ThingDef.Named("HEU");
                ThingDef thingdef2 = ThingDef.Named("NuclearFeeder");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    Thing thing = null;
                    Thing thing2 = null;
                    foreach (Thing current2 in Find.ThingGrid.ThingsAt(current))
                    {
                        if (current2.def == thingdef)
                        {
                            thing = current2;
                        }
                        if (current2.def == thingdef2)
                        {
                            thing2 = current2;
                        }
                    }
                    if (thing2 != null && thing != null)
                    {
                        return thing;
                    }
                }
                return null;
            }
        }
        
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            powerComp = base.GetComp<CompPowerTrader>();
            glowerComp = base.GetComp<CompGlower>();
            glowerComp.Lit = false;
            this.SetWorkVariables();
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            this.SetWorkVariables();
        }

        private void SetWorkVariables()
        {
            ThingDef_MNP thingDefMNP = (ThingDef_MNP)this.def;
            this.damageAppliedSelfMin = thingDefMNP.DamageAppliedSelfMin;
            this.damageAppliedSelfMax = thingDefMNP.DamageAppliedSelfMax;
            this.damageSelfCounterStart = thingDefMNP.DamageApplySelfCounter;
            this.overheartThreshold = thingDefMNP.OverheatThreshold;
        }

        public override void Tick()
        {
            base.Tick();
            this.burnDelay--;
            this.pufferSmoke--;
            if (!this.CanBurnNow)
            {
                glowerComp.Lit = false;
                this.powerComp.PowerOutput = -5000f;
                burnDelay = 0;
            }
            else
                if (pufferSmoke <= 0)
                {
                    MoteThrower.ThrowSmoke(this.TrueCenter(), 2f);
                    pufferSmoke = 90;
                    if (this.burnDelay <= 0)
                    {
                        glowerComp.Lit = true;
                        int num = 0;
                        List<ThingDef> list = new List<ThingDef>();
                        Thing FuelInHopper = this.FuelInHopper;
                        int num2 = Mathf.Min(FuelInHopper.stackCount, 1);
                        num += num2;
                        list.Add(FuelInHopper.def);
                        FuelInHopper.SplitOff(num2);
                        FuelInHopper = this.FuelInHopper;
                        this.powerComp.PowerOutput = 12000f;
                        this.burnDelay = 12000;
                    }
                }



            if (--Burnticks == 0)
            {
                MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 3f);
                MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 3f);
                MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 4f);
                Burnticks = 50;
            }
            
            --this.doDamageSelfCounter;
            if (this.doDamageSelfCounter > 0)
                return;
            this.doDamageSelfCounter = this.damageSelfCounterStart;
            this.DoDamageSelf();

            }

        private void DoDamageSelf()
        {
            this.damageAppliedSelf = this.random.Next(this.damageAppliedSelfMin, this.damageAppliedSelfMax);
            if (this.damageAppliedSelf == 0 || base.HitPoints > this.overheartThreshold)
                return;
            this.TakeDamage(new DamageInfo(DamageDefOf.Flame, this.damageAppliedSelf, (Thing)this, new BodyPartDamageInfo?(), (ThingDef)null));
            Messages.Message(Translator.Translate("NuclearMeltdownStart"), MessageSound.SeriousAlert);
        }
    }
}