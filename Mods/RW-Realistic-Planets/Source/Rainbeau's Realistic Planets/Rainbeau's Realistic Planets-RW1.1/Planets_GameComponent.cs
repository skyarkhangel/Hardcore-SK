using Verse;

namespace Planets_Code
{
	public class Planets_GameComponent : GameComponent
	{
		public static AxialTilt axialTilt = AxialTilt.Normal;
		public static WorldType worldType = WorldType.Vanilla;
		public static string worldPreset = "Planets.Vanilla";

		public static int subcount = 10;

		public Planets_GameComponent()
		{
		}

		public Planets_GameComponent(Game game)
		{
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref axialTilt, "axialTilt", AxialTilt.Normal, true);
			Scribe_Values.Look(ref worldType, "worldType", WorldType.Vanilla, true);
			Scribe_Values.Look(ref subcount, "subcount", 10, true);
			Scribe_Values.Look(ref worldPreset, "worldPreset", "Planets.Vanilla", true);
		}
	}
	
}
