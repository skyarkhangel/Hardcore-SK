// Decompiled with JetBrains decompiler
// Type: RimWorld.ThoughtUtility
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Psychology.Detour
{
    internal static class _ThoughtUtility
    {
        internal static void _GiveThoughtsForPawnDied(Pawn victim, DamageInfo? dinfo, Hediff hediff)
        {
            if (PawnGenerator.IsBeingGenerated(victim) || Current.ProgramState != ProgramState.MapPlaying)
                return;
            bool flag1 = dinfo.HasValue && dinfo.Value.Def == DamageDefOf.ExecutionCut || hediff != null && (hediff.def == HediffDefOf.Euthanasia || hediff.def == HediffDefOf.ShutDown);
            bool flag2 = victim.IsPrisonerOfColony && !PrisonBreakUtility.IsPrisonBreaking(victim) && !victim.InAggroMentalState;
            if (!victim.RaceProps.Humanlike)
                return;
            if (dinfo.HasValue && dinfo.Value.Def.externalViolence && dinfo.Value.Instigator != null)
            {
                Pawn pawn = dinfo.Value.Instigator as Pawn;
                if (pawn != null && !pawn.Dead && (pawn.needs.mood != null && pawn.story != null))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KilledHumanlikeBloodlust, (Pawn)null);
                    if (victim.Faction.HostileTo(pawn.Faction) && !flag1 && !flag2)
                        pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.KilledHumanlikeEnemy, (Pawn)null);
                }
            }
            List<Pawn> allPawnsSpawned = Find.MapPawns.AllPawnsSpawned;
            for (int index = 0; index < allPawnsSpawned.Count; ++index)
            {
                Pawn p = allPawnsSpawned[index];
                if (p.needs.mood != null && !flag1 && (p.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)p.MentalState).otherPawn != victim))
                {
                    if (victim.Spawned && (p.Position.InHorDistOf(victim.Position, 12f) && GenSight.LineOfSight(victim.Position, p.Position, false) && (p.Awake() && p.health.capacities.CapableOf(PawnCapacityDefOf.Sight))))
                    {

                        if (!p.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart) && !p.story.traits.HasTrait(TraitDefOf.Psychopath) && !p.story.traits.HasTrait(TraitDefOf.Bloodlust))
                        {
                            if (((p.GetHashCode() ^ (GenDate.DayOfYear + GenDate.CurrentYear + (int)(GenDate.CurrentDayPercent * 5) * 60) * 391) % 1000) == 0)
                            {
                                p.story.traits.GainTrait(new Trait(TraitDefOfPsychology.Desensitized));
                                p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.RecentlyDesensitized, (Pawn)null);
                            }
                        }

                        if (p.Faction == victim.Faction)
                        {
                            p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathAlly, (Pawn)null);
                            p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.WitnessedDeathAllyBleedingHeart, (Pawn)null);
                        }
                        else
                        {
                            p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathNonAlly, (Pawn)null);
                            p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.WitnessedDeathNonAllyBleedingHeart, (Pawn)null);
                        }
                        if (p.relations.FamilyByBlood.Contains<Pawn>(victim))
                            p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathFamily, (Pawn)null);
                        p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathBloodlust, (Pawn)null);

                    }
                    else if (victim.Faction == Faction.OfPlayer && victim.Faction == p.Faction && victim.HostFaction != p.Faction)
                        p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowColonistDied, (Pawn)null);
                    if (flag2 && p.Faction == Faction.OfPlayer && !p.IsPrisoner)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowPrisonerDiedInnocent, (Pawn)null);
                        p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.KnowPrisonerDiedInnocentBleedingHeart, (Pawn)null);
                    }
                }
            }
        }

        internal static void _GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
        {
            if (!victim.RaceProps.Humanlike)
                return;
            int forcedStage = 0;
            switch (kind)
            {
                case PawnExecutionKind.GenericBrutal:
                    forcedStage = 1;
                    break;
                case PawnExecutionKind.GenericHumane:
                    forcedStage = 0;
                    break;
                case PawnExecutionKind.OrganHarvesting:
                    forcedStage = 2;
                    break;
            }
            ThoughtDef def = !victim.IsColonist ? ThoughtDefOf.KnowGuestExecuted : ThoughtDefOf.KnowColonistExecuted;
            ThoughtDef def2 = !victim.IsColonist ? ThoughtDefOfPsychology.KnowGuestExecutedBleedingHeart : ThoughtDefOfPsychology.KnowColonistExecutedBleedingHeart;
            foreach (Pawn colonistsAndPrisoner in Find.MapPawns.FreeColonistsAndPrisoners)
            {
                colonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(def, forcedStage), (Pawn)null);
                colonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(def2, forcedStage), (Pawn)null);
            }
        }

        internal static void _GiveThoughtsForPawnOrganHarvested(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
                return;
            ThoughtDef def = (ThoughtDef)null;
            if (victim.IsColonist)
                def = ThoughtDefOf.KnowColonistOrganHarvested;
            else if (victim.HostFaction == Faction.OfPlayer)
                def = ThoughtDefOf.KnowGuestOrganHarvested;
            foreach (Pawn colonistsAndPrisoner in Find.MapPawns.FreeColonistsAndPrisoners)
            {
                if (def == ThoughtDefOf.KnowGuestOrganHarvested && colonistsAndPrisoner.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart))
                    def = ThoughtDefOfPsychology.KnowGuestOrganHarvestedBleedingHeart;
                else if (def == ThoughtDefOf.KnowColonistOrganHarvested && colonistsAndPrisoner.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart))
                    def = ThoughtDefOfPsychology.KnowColonistOrganHarvestedBleedingHeart;
                if (colonistsAndPrisoner == victim)
                    colonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.MyOrganHarvested, (Pawn)null);
                else if (def != null)
                    colonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemoryThought(def, (Pawn)null);
            }
        }
    }
}
