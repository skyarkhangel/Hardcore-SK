using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using System;



  public class CompExplosiveNuke : ThingComp
  {
    private static readonly SoundDef PreImpact = SoundDef.Named("RL_PreImpact");
    private static readonly SoundDef NukeExplosion = SoundDef.Named("NukeExplosion");
    private static readonly SoundDef Null = SoundDef.Named("Null");
    private static readonly SoundDef Null1 = SoundDef.Named("Null1");
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
      SoundStarter.PlayOneShot(CompExplosiveNuke.PreImpact, (SoundInfo)this.parent.Position);
      SoundInfo info = SoundInfo.InWorld((TargetInfo)((Thing)this.parent), MaintenanceType.PerTick);
      this.wickSoundSustainer = SoundStarter.TrySpawnSustainer(CompExplosiveNuke.Null, info);
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
    }

    public void StartWick()
    {
      if (this.wickStarted)
      {
        Log.Warning("Started wick twice on " + (object) this.parent);
      }
      else
      {
        this.wickStarted = true;
        this.wickTicksLeft = 15;
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
        this.parent.Destroy(DestroyMode.Vanish);
      GenExplosion.DoExplosion(this.parent.Position, 5, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 6, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 8, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 11, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 14, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 17, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 20, this.props.explosiveDamageType, (Thing)this.parent, Null1, (ThingDef)null);
      GenExplosion.DoExplosion(this.parent.Position, 23, DamageDefOf.Flame, (Thing)this.parent, Null1, (ThingDef)null);
      SoundStarter.PlayOneShot(CompExplosiveNuke.NukeExplosion, (SoundInfo)this.parent.Position);
    }
 }