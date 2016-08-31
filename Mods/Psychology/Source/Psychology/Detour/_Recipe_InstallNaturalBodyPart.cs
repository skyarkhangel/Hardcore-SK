using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _Recipe_InstallNaturalBodyPart
    {
        internal static void _ApplyOnPawn(this Recipe_InstallNaturalBodyPart r, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer != null)
            {
                if (_Recipe_MedicalOperation._CheckSurgeryFail(r,billDoer, pawn, ingredients))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                var MedicalRecipesUtility = Type.GetType("RimWorld.MedicalRecipesUtility, Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null");
                var restore = MedicalRecipesUtility.GetMethod("RestorePartAndSpawnAllPreviousParts", BindingFlags.Static | BindingFlags.Public);
                if (restore != null)
                    restore.Invoke(MedicalRecipesUtility, new object[] { pawn, part, billDoer.Position });
                else
                    Log.ErrorOnce("Unable to reflect MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts!", 305432421);
                billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
            }
        }
    }
}
