using System;
using Verse;
using RimWorld;
namespace SK_FlameWeapons
{
    public class BulletIncendiary : Bullet
    {
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                hitThing.TryAttachFire(0.2f);
            }
            else
            {
                GenSpawn.Spawn(ThingDef.Named("Puddle_Fuel"), base.Position);
                FireUtility.TryStartFireIn(base.Position, 0.2f);
            }
            MoteThrower.ThrowStatic(this.Position, ThingDefOf.Mote_ShotFlash, 6f);
            MoteThrower.ThrowMicroSparks(base.Position.ToVector3Shifted());
        }
    }
}
