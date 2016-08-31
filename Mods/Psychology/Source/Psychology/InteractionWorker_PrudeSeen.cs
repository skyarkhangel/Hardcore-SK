// Decompiled with JetBrains decompiler
// Type: RimWorld.InteractionWorker_MarriageProposal
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology
{
    public class InteractionWorker_PrudeSeen : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Prude) && recipient.apparel.PsychologicallyNude)
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
    }
}
