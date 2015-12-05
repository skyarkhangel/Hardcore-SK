// Decompiled with JetBrains decompiler
// Type: Verse.Verb_Shoot
// Assembly: Assembly-CSharp, Version=0.8.5476.23593, Culture=neutral, PublicKeyToken=null
// MVID: 50EAA954-ECB6-4D9B-9A07-53C8F3E535D9
// Assembly location: E:\Downloads\RimWorld671Win\RimWorld671Win_Data\Managed\Assembly-CSharp.dll

using RimWorld;
using System.Text;
using Verse.Sound;
using Verse;

namespace TurretCollection
{
  public class Verb_ShootMini : Verb_LaunchProjectile
  {
    private static readonly SoundDef M134Shoot = SoundDef.Named("M134Shoot");

    protected override int ShotsPerBurst
    {
        get
        {
            return this.verbProps.burstShotCount;
        }
    }

    public override void WarmupComplete()
    {
      base.WarmupComplete();
      SoundStarter.PlayOneShot(Verb_ShootMini.M134Shoot, (SoundInfo)this.caster.Position);
      if (!this.CasterIsPawn || this.CasterPawn.skills == null)
        return;
      float xp = 10f;
      if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
        xp = !GenHostility.HostileTo(this.currentTarget.Thing, this.caster) ? 50f : 200f;
      this.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp);
    }

    protected override bool TryCastShot()
    {
        if (!base.TryCastShot())
        return false;
      MoteThrower.ThrowStatic(this.caster.Position, ThingDefOf.Mote_ShotFlash, 9f);
      return true;
    }
  }
}
