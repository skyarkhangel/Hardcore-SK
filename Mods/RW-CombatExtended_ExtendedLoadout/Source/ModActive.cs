using Verse;

namespace CombatExtended.ExtendedLoadout;

public class ModActive
{
    private static bool _betterPawnControl = false;

    public static bool BetterPawnControl
    {
        get
        {
            return ModLister.HasActiveModWithName("Better Pawn Control");
        }
    }
}