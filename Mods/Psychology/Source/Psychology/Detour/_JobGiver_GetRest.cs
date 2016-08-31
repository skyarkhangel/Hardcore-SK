// Decompiled with JetBrains decompiler
// Type: RimWorld.JobGiver_GetRest
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _JobGiver_GetRest
    {
        internal static float _GetPriority(this JobGiver_GetRest j, Pawn pawn)
        {
            Need_Rest needRest = pawn.needs.rest;
            if (needRest == null || Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
                return 0.0f;
            Lord lord = pawn.GetLord();
            if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
                return 0.0f;
            TimeAssignmentDef timeAssignmentDef;
            if (pawn.RaceProps.Humanlike)
            {
                timeAssignmentDef = pawn.timetable != null ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
            }
            else
            {
                int hourOfDay = GenDate.HourOfDay;
                timeAssignmentDef = hourOfDay < 7 || hourOfDay > 21 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
            }
            float curLevel = needRest.CurLevel;
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac))
            {
                if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
                    return (double)curLevel < 0.15 ? 0.0000005f : 0.0f;
                if (timeAssignmentDef == TimeAssignmentDefOf.Work)
                    return 0.0f;
                if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
                    return (double)curLevel < 0.15 ? 0.0000005f : 0.0f;
                if (timeAssignmentDef != TimeAssignmentDefOf.Sleep)
                    throw new NotImplementedException();
                return (double)curLevel < 0.4 ? 0.0000005f : 0.0f;
            }
            if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
                return (double)curLevel < 0.300000011920929 ? 8f : 0.0f;
            if (timeAssignmentDef == TimeAssignmentDefOf.Work)
                return 0.0f;
            if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
                return (double)curLevel < 0.300000011920929 ? 8f : 0.0f;
            if (timeAssignmentDef != TimeAssignmentDefOf.Sleep)
                throw new NotImplementedException();
            return (double)curLevel < 0.75 ? 8f : 0.0f;
        }
    }
}
