using CommunityCoreLibrary;
using CommunityCoreLibrary.Controller;
using CommunityCoreLibrary.Detour;
using Psychology.Detour;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace Psychology
{
    public class DetourInjector : SpecialInjector
    {
        public override bool Inject()
        {
            return Detours.TryDetourFromTo(typeof(ThoughtUtility).GetMethod("GiveThoughtsForPawnDied", BindingFlags.Static | BindingFlags.Public), typeof(_ThoughtUtility).GetMethod("_GiveThoughtsForPawnDied", BindingFlags.Static | BindingFlags.NonPublic)) 
                && Detours.TryDetourFromTo(typeof(ThoughtUtility).GetMethod("GiveThoughtsForPawnExecuted", BindingFlags.Static | BindingFlags.Public), typeof(_ThoughtUtility).GetMethod("_GiveThoughtsForPawnExecuted", BindingFlags.Static | BindingFlags.NonPublic)) 
                && Detours.TryDetourFromTo(typeof(ThoughtUtility).GetMethod("GiveThoughtsForPawnOrganHarvested", BindingFlags.Static | BindingFlags.Public), typeof(_ThoughtUtility).GetMethod("_GiveThoughtsForPawnOrganHarvested", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Pawn_RelationsTracker).GetMethod("Notify_RescuedBy", BindingFlags.Instance | BindingFlags.Public), typeof(_Pawn_RelationsTracker).GetMethod("_Notify_RescuedBy", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Pawn_RelationsTracker).GetMethod("AttractionTo", BindingFlags.Instance | BindingFlags.Public), typeof(_Pawn_RelationsTracker).GetMethod("_AttractionTo", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Recipe_InstallArtificialBodyPart).GetMethod("ApplyOnPawn", BindingFlags.Instance | BindingFlags.Public), typeof(_Recipe_InstallArtificialBodyPart).GetMethod("_ApplyOnPawn", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Recipe_InstallNaturalBodyPart).GetMethod("ApplyOnPawn", BindingFlags.Instance | BindingFlags.Public), typeof(_Recipe_InstallNaturalBodyPart).GetMethod("_ApplyOnPawn", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Recipe_InstallImplant).GetMethod("ApplyOnPawn", BindingFlags.Instance | BindingFlags.Public), typeof(_Recipe_InstallImplant).GetMethod("_ApplyOnPawn", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Recipe_MedicalOperation).GetMethod("CheckSurgeryFail", BindingFlags.Instance | BindingFlags.NonPublic), typeof(_Recipe_MedicalOperation).GetMethod("_CheckSurgeryFail", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(TendUtility).GetMethod("DoTend", BindingFlags.Static | BindingFlags.Public), typeof(_TendUtility).GetMethod("_DoTend", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Tradeable_Pawn).GetMethod("ResolveTrade", BindingFlags.Instance | BindingFlags.Public), typeof(_Tradeable_Pawn).GetMethod("_ResolveTrade", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(Need_Rest).GetMethod("NeedInterval", BindingFlags.Instance | BindingFlags.Public), typeof(_Need_Rest).GetMethod("_NeedInterval", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(RecordsUtility).GetMethod("Notify_BillDone", BindingFlags.Static | BindingFlags.Public), typeof(_RecordsUtility).GetMethod("_Notify_BillDone", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(FoodUtility).GetMethod("ThoughtsFromIngesting", BindingFlags.Static | BindingFlags.Public), typeof(_FoodUtility).GetMethod("_ThoughtsFromIngesting", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(InteractionWorker_RomanceAttempt).GetMethod("SuccessChance", BindingFlags.Instance | BindingFlags.Public), typeof(_InteractionWorker_RomanceAttempt).GetMethod("_SuccessChance", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(InteractionWorker_RomanceAttempt).GetMethod("RandomSelectionWeight", BindingFlags.Instance | BindingFlags.Public), typeof(_InteractionWorker_RomanceAttempt).GetMethod("_RandomSelectionWeight", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(InteractionWorker_RomanceAttempt).GetMethod("Interacted", BindingFlags.Instance | BindingFlags.Public), typeof(_InteractionWorker_RomanceAttempt).GetMethod("_Interacted", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(InteractionWorker_RomanceAttempt).GetMethod("TryAddCheaterThought", BindingFlags.Instance | BindingFlags.NonPublic), typeof(_InteractionWorker_RomanceAttempt).GetMethod("_TryAddCheaterThought", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(InteractionWorker_Breakup).GetMethod("RandomSelectionWeight", BindingFlags.Instance | BindingFlags.Public), typeof(_InteractionWorker_Breakup).GetMethod("_RandomSelectionWeight", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(InteractionWorker_Breakup).GetMethod("Interacted", BindingFlags.Instance | BindingFlags.Public), typeof(_InteractionWorker_Breakup).GetMethod("_Interacted", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(ChildRelationUtility).GetMethod("ChanceOfBecomingChildOf", BindingFlags.Static | BindingFlags.Public), typeof(_ChildRelationUtility).GetMethod("_ChanceOfBecomingChildOf", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(LovePartnerRelationUtility).GetMethod("LovePartnerRelationGenerationChance", BindingFlags.Static | BindingFlags.Public), typeof(_LovePartnerRelationUtility).GetMethod("_LovePartnerRelationGenerationChance", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(PawnRelationWorker_Sibling).GetMethod("GenerateParent", BindingFlags.Static | BindingFlags.NonPublic), typeof(_PawnRelationWorker_Sibling).GetMethod("_GenerateParent", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(PawnObserver).GetMethod("ObserveSurroundingThings", BindingFlags.Instance | BindingFlags.NonPublic), typeof(_PawnObserver).GetMethod("_ObserveSurroundingThings", BindingFlags.Static | BindingFlags.NonPublic))
                && Detours.TryDetourFromTo(typeof(JobGiver_GetRest).GetMethod("GetPriority", BindingFlags.Instance | BindingFlags.Public), typeof(_JobGiver_GetRest).GetMethod("_GetPriority", BindingFlags.Static | BindingFlags.NonPublic));
        }
    }
}