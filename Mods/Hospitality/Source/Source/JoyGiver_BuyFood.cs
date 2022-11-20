using Verse;

namespace Hospitality
{
    public class JoyGiver_BuyFood : JoyGiver_BuyStuff
    {
        protected override int OptimalMoneyForShopping => 25;

        protected override bool Qualifies(Thing thing, Pawn pawn)
        {
            return base.Qualifies(thing, pawn) && CanEat(thing, pawn);
        }
    }
}