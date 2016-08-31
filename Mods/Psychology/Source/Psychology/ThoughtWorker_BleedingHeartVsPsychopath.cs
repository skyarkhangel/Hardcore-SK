// Decompiled with JetBrains decompiler
// Type: RimWorld.ThoughtWorker_HardWorkerVsLazy
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_BleedingHeartVsPsychopath : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!p.RaceProps.Humanlike)
                return (ThoughtState)false;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart))
                return (ThoughtState)false;
            if (!other.RaceProps.Humanlike)
                return (ThoughtState)false;
            if (!RelationsUtility.PawnsKnowEachOther(p, other))
                return (ThoughtState)false;
            if (!other.story.traits.HasTrait(TraitDefOf.Psychopath))
                return (ThoughtState)false;
            return (ThoughtState)true;
        }
    }
}
