using RimWorld;
using Verse;

namespace Hospitality.MainTab
{
    public class PawnColumnWorker_MakeFriends : PawnColumnWorker_Checkbox
    {
        protected override bool HasCheckbox(Pawn pawn) => pawn.IsGuest();

        protected override bool GetValue(Pawn pawn) => pawn.MakeFriends();

        protected override void SetValue(Pawn pawn, bool value)
        {
            var compGuest = pawn.CompGuest();
            if (compGuest != null) compGuest.makeFriends = value;
        }
    }
}
