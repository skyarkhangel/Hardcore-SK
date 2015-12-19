using Verse;
using RimWorld;
namespace TurretCollection
{
	public class Building_SpotlightGlower : Building
	{
        private int Burnticks = 1800;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public override void Tick()
        {
            base.Tick();
            Burnticks--;
            if (Burnticks == 0)
            {
                this.Destroy();
            }
            
        }
	}
}