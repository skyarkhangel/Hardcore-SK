using RimWorld;
using UnityEngine;
using Verse;

namespace RT_SolarFlareShield
{
	public class CompProperties_RTSolarFlareShield : CompProperties
	{
		public float shieldingPowerDrain = 0.0f;
		public float heatingPerTick = 0.0f;
		public float rotatorSpeedActive = 10.0f;
		public float rotatorSpeedIdle = 0.5f;

		public CompProperties_RTSolarFlareShield()
		{
			compClass = typeof(CompRTSolarFlareShield);
		}
	}

	public class CompRTSolarFlareShield : ThingComp
	{
		public CompProperties_RTSolarFlareShield Properties
		{
			get
			{
				return (CompProperties_RTSolarFlareShield)props;
			}
		}

		public bool Active
		{
			get
			{
				return compPowerTrader == null || compPowerTrader.PowerOn;
			}
		}

		private CompPowerTrader compPowerTrader;
		private MapComponent_ShieldCoordinator coordinator;
		private float rotatorAngle = (float)Rand.Range(0, 360);

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			compPowerTrader = parent.TryGetComp<CompPowerTrader>();
			if (compPowerTrader == null)
			{
				Log.Error("[RT Solar Flare Shield]: Could not get CompPowerTrader of " + parent);
			}
			coordinator = parent.Map.GetShieldCoordinator();
			coordinator.shields.Add(this);
		}

		public override void PostDeSpawn(Map map)
		{
			coordinator.shields.Remove(this);
			base.PostDeSpawn(map);
		}

		public override string CompInspectStringExtra()
		{
			return "CompRTSolarFlareShield_FlareProtection".Translate();
		}

		public override void CompTick()
		{
			SolarFlareShieldTick(1);
		}

		public override void PostDraw()
		{       // Thanks Skullywag!
			Vector3 vector = new Vector3(2.0f, 2.0f, 2.0f);
			vector.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(
				parent.DrawPos + Altitudes.AltIncVect,
				Quaternion.AngleAxis(rotatorAngle, Vector3.up),
				vector);
			Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.rotatorTexture, 0);
		}

		private void SolarFlareShieldTick(int tickAmount)
		{
			if ((Find.TickManager.TicksGame) % tickAmount == 0)
			{
				if (Active)
				{
					if (Find.World.GameConditionManager.ElectricityDisabled || parent.Map.GameConditionManager.ElectricityDisabled)
					{
						compPowerTrader.PowerOutput = -Properties.shieldingPowerDrain;
						rotatorAngle += Properties.rotatorSpeedActive * tickAmount;
						var map = parent.Map;
						if (map != null)
						{
							GenTemperature.PushHeat(parent.Position, parent.Map, Properties.heatingPerTick * tickAmount);
						}
						if ((Find.TickManager.TicksGame) % (5 * tickAmount) == 0)
						{
							foreach (Thing building in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
							{
								if (building is Building_CommsConsole console)
								{
									CompPowerTrader consoleCompPowerTrader = console.TryGetComp<CompPowerTrader>();
									if (consoleCompPowerTrader != null)
									{
										consoleCompPowerTrader.PowerOn = false;
									}
								}
							}
						}
					}
					else
					{
						compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption;
						rotatorAngle += Properties.rotatorSpeedIdle * tickAmount;
					}
				}
			}
		}
	}
}