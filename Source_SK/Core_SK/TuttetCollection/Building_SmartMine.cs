using Verse;
using RimWorld;
namespace TurretCollection
{
    public class Building_SmartMine : Building_TurretGun
	{
        private bool WarmingUp
        {
            get
            {
                return this.burstWarmupTicksLeft > 0;
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (this.powerComp != null && !this.powerComp.PowerOn || this.mannableComp != null && !this.mannableComp.MannedNow)
                return;
            this.GunCompEq.verbTracker.VerbsTick();
            if (this.stunner.Stunned || this.GunCompEq.PrimaryVerb.state == VerbState.Bursting)
                return;
            if (this.WarmingUp)
            {
                --this.burstWarmupTicksLeft;
                if (this.burstWarmupTicksLeft == 0)
                    this.BeginBurst();
            }
            else
            {
                if (this.burstCooldownTicksLeft > 0)
                    Find.ThingGrid.ThingAt(this.Position, ThingDef.Named("Turret_SmartMine")).Destroy(DestroyMode.Vanish);
            }
            this.top.TurretTopTick();
        }
	}
}