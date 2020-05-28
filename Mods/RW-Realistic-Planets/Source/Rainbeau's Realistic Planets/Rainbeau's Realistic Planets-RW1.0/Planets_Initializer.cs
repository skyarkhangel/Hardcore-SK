using RimWorld;
using System.Linq;
using Verse;

namespace Planets_Code
{
	[StaticConstructorOnStartup]
	internal static class Planets_Initializer {
		static Planets_Initializer() {
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("My Little Planet"))) {
				Controller.Settings.usingMLP = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Faction Control"))) {
				Controller.Settings.usingFactionControl = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Nature's Pretty Sweet"))) {
				Controller.Settings.otherGrassland = true;
				Controller.Settings.otherSavanna = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Terra Project (Core)"))) {
				Controller.Settings.otherSavanna = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Advanced Biomes"))) {
				Controller.Settings.otherSavanna = true;
			}
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefsListForReading) {
				if (current.plant != null) {
					if (current.plant.wildBiomes != null) {
						for (int j = 0; j < current.plant.wildBiomes.Count; j++) {
							if (current.plant.wildBiomes[j].biome.defName == "Tundra") {
								PlantBiomeRecord newRecord = new PlantBiomeRecord();
								newRecord.biome = BiomeDef.Named("RRP_Permafrost");
								newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
								current.plant.wildBiomes.Add(newRecord);
							}
							if (current.plant.wildBiomes[j].biome.defName == "TemperateForest") {
								if (!current.defName.Contains("Tree")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_Grassland");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
								}
							}
							if (current.plant.wildBiomes[j].biome.defName == "BorealForest") {
								if (!current.defName.Contains("Tree")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_Steppes");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
								}
							}
							if (current.plant.wildBiomes[j].biome.defName == "AridShrubland") {
								if (!current.defName.Contains("Acacia")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_Savanna");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
								}
							}
							if (current.plant.wildBiomes[j].biome.defName == "Desert") {
								if (!current.defName.Contains("Acacia")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_TemperateDesert");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
									PlantBiomeRecord newRecord2 = new PlantBiomeRecord();
									newRecord2.biome = BiomeDef.Named("RRP_Oasis");
									newRecord2.commonality = current.plant.wildBiomes[j].commonality;
									current.plant.wildBiomes.Add(newRecord2);
								}
							}
						}
					}
				}
			}
			foreach (PawnKindDef current in DefDatabase<PawnKindDef>.AllDefs) {
				if (current.RaceProps.wildBiomes != null && current.defName != "Cobra") {
					for (int j = 0; j < current.RaceProps.wildBiomes.Count; j++) {
						if (current.RaceProps.wildBiomes[j].biome.defName == "Tundra") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Permafrost");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "TemperateForest") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Grassland");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "BorealForest") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Steppes");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "AridShrubland") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Savanna");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "Desert") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_TemperateDesert");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
							AnimalBiomeRecord newRecord2 = new AnimalBiomeRecord();
							newRecord2.biome = BiomeDef.Named("RRP_Oasis");
							newRecord2.commonality = current.RaceProps.wildBiomes[j].commonality;
							current.RaceProps.wildBiomes.Add(newRecord2);
						}
					}
				}
			}
		}
	}
	
}
