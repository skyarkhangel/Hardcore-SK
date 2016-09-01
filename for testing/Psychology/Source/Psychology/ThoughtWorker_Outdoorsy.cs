// Decompiled with JetBrains decompiler
// Type: RimWorld.ThoughtWorker_HardWorkerVsLazy
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_Outdoorsy : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Outdoorsy))
                return ThoughtState.Inactive;
            if (p.Position.Roofed())
                return ThoughtState.Inactive;
            else
            {
                if (p.Position.GetRoom().PsychologicallyOutdoors)
                    return ThoughtState.ActiveAtStage(1);
                return ThoughtState.ActiveAtStage(0);
            }
        }
    }
}
