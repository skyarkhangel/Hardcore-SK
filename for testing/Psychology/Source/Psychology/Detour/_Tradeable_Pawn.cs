// Decompiled with JetBrains decompiler
// Type: RimWorld.Tradeable_Pawn
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _Tradeable_Pawn
    {
        internal static void _ResolveTrade(this Tradeable_Pawn t)
        {
            if (t.ActionToDo == TradeAction.PlayerSells)
            {
                List<Pawn> list = t.thingsColony.Take<Thing>(-t.countToDrop).Cast<Pawn>().ToList<Pawn>();
                for (int index = 0; index < list.Count; ++index)
                {
                    Pawn pawn = list[index];
                    pawn.PreTraded(TradeAction.PlayerSells, TradeSession.playerNegotiator, TradeSession.trader);
                    TradeSession.trader.AddToStock((Thing)pawn);
                    if (pawn.RaceProps.Humanlike)
                    {
                        foreach (Pawn colonistsAndPrisoner in Find.MapPawns.FreeColonistsAndPrisoners)
                        {
                            colonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowPrisonerSold, (Pawn)null);
                            colonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.KnowPrisonerSoldBleedingHeart, (Pawn)null);
                        }
                    }
                }
            }
            else
            {
                if (t.ActionToDo != TradeAction.PlayerBuys)
                    return;
                List<Pawn> list = t.thingsTrader.Take<Thing>(t.countToDrop).Cast<Pawn>().ToList<Pawn>();
                for (int index = 0; index < list.Count; ++index)
                {
                    Pawn pawn = list[index];
                    TradeSession.trader.GiveSoldThingToBuyer((Thing)pawn, (Thing)pawn);
                    pawn.PreTraded(TradeAction.PlayerBuys, TradeSession.playerNegotiator, TradeSession.trader);
                }
            }
        }
    }
}
