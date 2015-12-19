// Decompiled with JetBrains decompiler
// Type: AtomicPowerMod.Building_PowerPlantAtomicPower
// Assembly: RimWorld_AtomicPowerMod, Version=0.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3BA90F32-070E-4A95-8557-409C6CD87E36
// Assembly location: E:\Downloads\RimWorld671Win\Mods\AtomicPower\Assemblies\RimWorld_AtomicPowerMod.dll

using SK_MNP;
using SK_NPP;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace SK_MNP
{
    public class Building_MNPD : Building
    {
        private int damageCounterStart = 600;
        private int damageSelfCounterStart = 600;
        private float damageRange = -1f;
        private string damageDefName = "Radiation";
        private bool doDamageActive = false;
        private bool scanDone = false;
        public bool spawnInit = false;
        public bool spawnDisabled = false;
        private Random random = new Random();
        private int damageAppliedMin;
        private int damageAppliedMax;
        private int damageAppliedSelfMin;
        private int damageAppliedSelfMax;
        private int damageApplied;
        private int damageAppliedSelf;
        private int doDamageCounter;
        private int doDamageSelfCounter;
        private int thingsWorkValue;
        private int pawnsWorkValue;
        private IEnumerable<Pawn> pawns;
        public CompGlower compGlowerMNPD;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.compGlowerMNPD = this.GetComp<CompGlower>();
            this.compGlowerMNPD.Lit = true;
            this.SetWorkVariables();
            this.doDamageCounter = this.damageCounterStart;
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            this.SetWorkVariables();
            this.doDamageCounter = this.damageCounterStart;
        }

        private void SetWorkVariables()
        {
            ThingDef_MNPD thingDefMNPD = (ThingDef_MNPD)this.def;
            this.damageAppliedMin = thingDefMNPD.DamageAppliedMin;
            this.damageAppliedMax = thingDefMNPD.DamageAppliedMax;
            this.damageCounterStart = thingDefMNPD.DamageApplyCounter;
            this.damageAppliedSelfMin = thingDefMNPD.DamageAppliedSelfMin;
            this.damageAppliedSelfMax = thingDefMNPD.DamageAppliedSelfMax;
            this.damageSelfCounterStart = thingDefMNPD.DamageApplySelfCounter;
            this.damageDefName = thingDefMNPD.DamageDefName;
        }

        public override void Tick()
        {
            base.Tick();
            this.CheckDamageRange();
            --this.doDamageCounter;
            if (this.doDamageCounter <= 0)
            {
                this.doDamageCounter = this.damageCounterStart;
                this.doDamageActive = true;
                this.scanDone = false;
            }
            if (this.doDamageActive)
                this.DoDamage();
            --this.doDamageSelfCounter;
            if (this.doDamageSelfCounter > 0)
                return;
            this.doDamageSelfCounter = this.damageSelfCounterStart;
            this.DoDamageSelf();
        }

        private void DoDamage()
        {
            if (!this.scanDone)
            {
                IEnumerable<IntVec3> source = GenAdj.CellsAdjacent8WayAndInside((Thing)this);
                IntVec3 c = IntVec3.Invalid;
                if (source != null && Enumerable.Count<IntVec3>(source) > 0)
                    c = Enumerable.FirstOrDefault<IntVec3>(Enumerable.Where<IntVec3>(source, (Func<IntVec3, bool>)(p1 => RoomQuery.RoomAt(p1) != null)));
                if (c == IntVec3.Invalid)
                {
                    Log.Error("pos == null");
                    this.doDamageActive = false;
                }
                else
                {
                    this.pawns = Radar.FindAllPawnsInRoom(this.Position, RoomQuery.RoomAt(c), this.damageRange);
                    this.thingsWorkValue = 0;
                    this.pawnsWorkValue = Enumerable.Count<Pawn>(this.pawns);
                    this.damageApplied = this.random.Next(this.damageAppliedMin, this.damageAppliedMax);
                    this.scanDone = true;
                }
            }
            else if (this.damageApplied == 0)
            {
                this.doDamageActive = false;
            }
            else
            {
                DamageInfo dinfo = new DamageInfo(DefDatabase<DamageDef>.GetNamed(this.damageDefName, true), this.damageApplied, (Thing)this, new BodyPartDamageInfo?(new BodyPartDamageInfo(new BodyPartHeight?(), new BodyPartDepth?(BodyPartDepth.Inside))), (ThingDef)null);
                int num = Enumerable.Count<Pawn>(this.pawns);
                if (this.pawnsWorkValue > 0)
                {
                    Pawn pawn = (Pawn)null;
                    if (this.pawnsWorkValue > num)
                        this.pawnsWorkValue = num;
                    else
                        pawn = Enumerable.ElementAtOrDefault<Pawn>(this.pawns, this.pawnsWorkValue - 1);
                    --this.pawnsWorkValue;
                    if (pawn != null && !pawn.Destroyed)
                    {
                        pawn.TakeDamage(dinfo);
                    }
                }
                if (this.thingsWorkValue > 0 || this.pawnsWorkValue > 0)
                    return;
                this.doDamageActive = false;
            }
        }

        private void DoDamageSelf()
        {
            this.damageAppliedSelf = this.random.Next(this.damageAppliedSelfMin, this.damageAppliedSelfMax);
            if (this.damageAppliedSelf == 0)
                return;
            this.TakeDamage(new DamageInfo(DamageDefOf.Bullet, this.damageAppliedSelf, (Thing)this, new BodyPartDamageInfo?(), (ThingDef)null));
        }

        private void CheckDamageRange()
        {
            if ((double)this.damageRange >= 0.0)
                return;
            this.damageRange = this.compGlowerMNPD.props.glowRadius;
        }
    }
}
