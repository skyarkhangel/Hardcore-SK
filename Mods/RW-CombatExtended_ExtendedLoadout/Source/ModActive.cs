using Verse;

namespace CombatExtended.ExtendedLoadout;

public class ModActive
{
    private static bool _betterPawnControl = false;

    public static bool BetterPawnControl
    {
        get
        {
            Log.Message("BPC ModActive check entered");
            return ModLister.HasActiveModWithName("Better Pawn Control");
        }
    }
}