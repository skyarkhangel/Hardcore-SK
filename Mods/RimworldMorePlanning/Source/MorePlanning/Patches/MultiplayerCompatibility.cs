using Multiplayer.API;
using Verse;

namespace MorePlanning.Patches
{
    [StaticConstructorOnStartup]
    class MultiplayerCompatibility
    {
        static MultiplayerCompatibility()
        {
            if (MP.enabled)
            {
                MP.RegisterAll();
                MP.RegisterSyncMethod(typeof(Plan.PlanColorManager), nameof(Plan.PlanColorManager.ChangeColor));
                MP.RegisterSyncWorker<Designators.AddDesignator>(SyncWorkerForPlanDesignation);
            }
        }
        static void SyncWorkerForPlanDesignation(SyncWorker sync, ref Designators.AddDesignator inst)
        {
            if (sync.isWriting)
            {
                sync.Write(MorePlanningMod.Instance.SelectedColor);
            }
            else
            {
                int colorInt = sync.Read<int>();
                MorePlanningMod.Instance.SelectedColor = colorInt;
            }
        }
    }
}
