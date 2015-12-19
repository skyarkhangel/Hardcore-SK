using Verse;
using RimWorld;
using System;
using Verse.Sound;

namespace TurretCollection
{
    public class Building_WarheadDeployed : Building
	{
        private static readonly SoundDef Siren = SoundDef.Named("Siren");
        private int Burnticks = 2400;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public override void Tick()
        {
            base.Tick();
            Burnticks--;
            if (Burnticks == 600)
            {
                Messages.Message(Translator.Translate("NuclearStrikeIncoming"), MessageSound.Standard);
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
            if (Burnticks == 480)
            {
                SoundStarter.PlayOneShotOnCamera(Building_WarheadDeployed.Siren);
            }
            if (Burnticks == 0)
            {
                this.GetComp<CompExplosiveNuke>().StartWick();
            } 
        }
	}
}