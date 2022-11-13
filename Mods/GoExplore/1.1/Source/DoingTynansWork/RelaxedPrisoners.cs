namespace LetsGoExplore
{
    using RimWorld;
    using RimWorld.BaseGen;
    using Verse;
    using Verse.AI.Group;

    public class SymbolResolver_RelaxedPrisonersLGE : SymbolResolver
    {
        private const int PrisonersToSpawn = 4; //or however many.

        //should come after a "Prison room" resolver, which made an enclosed space with prisoner beds, a CenterCell, and the faction of the prisoners.
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomNonHostileFaction();

            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_RelaxAsAPrisoner(rp.rect.CenterCell), map);

            for (int i = 0; i < PrisonersToSpawn; i++)
            {
                bool postGear(Pawn pawn)
                {
                    pawn.equipment.Primary?.Destroy();
                    pawn.AllComps.Add(new ThingComp_RescueMe { parent = pawn });
                    return true;
                }
                PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(faction.RandomPawnKind(), faction, validatorPostGear: postGear);

                ResolveParams resolveParams = rp;
                resolveParams.faction = faction;
                resolveParams.singlePawnLord = singlePawnLord;
                resolveParams.singlePawnGenerationRequest = pawnGenerationRequest;
                resolveParams.postThingSpawn = (pawn) => (pawn as Pawn).equipment.Primary?.Destroy();

                BaseGen.symbolStack.Push("pawn", resolveParams);
            }
        }
    }
}
