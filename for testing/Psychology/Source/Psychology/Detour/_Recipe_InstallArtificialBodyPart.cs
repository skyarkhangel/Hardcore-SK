// Decompiled with JetBrains decompiler
// Type: RimWorld.Recipe_InstallNaturalBodyPart
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _Recipe_InstallArtificialBodyPart
    {
        internal static void _ApplyOnPawn(this Recipe_InstallArtificialBodyPart r, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer != null)
            {
                if (_Recipe_MedicalOperation._CheckSurgeryFail(r, billDoer, pawn, ingredients))
                    return;
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, (object)billDoer, (object)pawn);
                var MedicalRecipesUtility = Type.GetType("RimWorld.MedicalRecipesUtility, Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null");
                var restore = MedicalRecipesUtility.GetMethod("RestorePartAndSpawnAllPreviousParts", BindingFlags.Static | BindingFlags.Public);
                if(restore != null)
                    restore.Invoke(MedicalRecipesUtility, new object[] { pawn, part, billDoer.Position });
                else
                    Log.ErrorOnce("Unable to reflect MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts!", 305432421);
                billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
            }
            pawn.health.AddHediff(r.recipe.addsHediff, part, new DamageInfo?());
        }
    }
}
