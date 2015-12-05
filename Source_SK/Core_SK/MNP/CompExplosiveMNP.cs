using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SK_MNP
{
    public class CompExplosiveMNP : ThingComp
    {
        private static readonly SoundDef WickStartSound = SoundDef.Named("MetalHitImportant");
        private static readonly SoundDef WickLoopSound = SoundDef.Named("HissSmall");
        public bool wickStarted;
        protected int wickTicksLeft;
        protected Sustainer wickSoundSustainer;
        private bool detonated;

        protected int StartWickThreshold
        {
            get
            {
                return Mathf.RoundToInt(this.props.startWickHitPointsPercent * (float)this.parent.MaxHitPoints);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<bool>(ref this.wickStarted, "wickStarted", false, false);
            Scribe_Values.LookValue<int>(ref this.wickTicksLeft, "wickTicksLeft", 0, false);
        }

        public override void CompTick()
        {
            if (!this.wickStarted)
                return;
            if (this.wickSoundSustainer == null)
                this.StartWickSustainer();
            else
                this.wickSoundSustainer.Maintain();
            --this.wickTicksLeft;
            if (this.wickTicksLeft > 0)
                return;
            this.Detonate();
        }

        private void StartWickSustainer()
        {
            SoundStarter.PlayOneShot(CompExplosiveMNP.WickStartSound, (SoundInfo)this.parent.Position);
            SoundInfo info = SoundInfo.InWorld((TargetInfo)((Thing)this.parent), MaintenanceType.PerTick);
            this.wickSoundSustainer = SoundStarter.TrySpawnSustainer(CompExplosiveMNP.WickLoopSound, info);
        }

        public override void PostDraw()
        {
            if (!this.wickStarted)
                return;
            OverlayDrawer.DrawOverlay((Thing)this.parent, OverlayTypes.BurningWick);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (this.parent.HitPoints <= 0)
            {
                if (!dinfo.Def.externalViolence)
                    return;
                this.Detonate();
            }
            else if (this.wickStarted && dinfo.Def == DamageDefOf.Stun)
            {
                this.StopWick();
            }
            else
            {
                if (this.wickStarted || this.parent.HitPoints > this.StartWickThreshold || !dinfo.Def.externalViolence)
                    return;
                this.StartWick();
            }
        }

        public override void PostDestroy(DestroyMode mode)
        {
            if (mode != DestroyMode.Kill)
                return;
            this.Detonate();
            GenSpawn.Spawn(ThingDef.Named("MNPD"), this.parent.Position);
        }

        public void StartWick()
        {
            if (this.wickStarted)
            {
                Log.Warning("Started wick twice on " + (object)this.parent);
            }
            else
            {
                this.wickStarted = true;
                this.wickTicksLeft = this.props.wickTicks.RandomInRange;
                this.StartWickSustainer();
            }
        }

        public void StopWick()
        {
            this.wickStarted = false;
        }

        protected void Detonate()
        {
            if (this.detonated)
                return;
            this.detonated = true;
            if (!this.parent.Destroyed)
                this.parent.Destroy(DestroyMode.Kill);
            GenExplosion.DoExplosion(this.parent.Position, this.props.explosiveRadius, this.props.explosiveDamageType, (Thing)this.parent, (SoundDef)null, (ThingDef)null);
        }
    }
}