using RimWorld;
using Verse;

namespace RocketMan
{
    public partial class RocketMod
    {
        [Main.OnTickRare]
        [Main.OnDefsLoaded]
        public static void UpdateExceptions()
        {
            if (StatDefOf.MarketValue != null && StatDefOf.MarketValueIgnoreHp != null)
            {
                RocketStates.StatExpiry[StatDefOf.MarketValue.index] = 0;
                RocketStates.StatExpiry[StatDefOf.MarketValueIgnoreHp.index] = 0;
            }
        }
    }
}