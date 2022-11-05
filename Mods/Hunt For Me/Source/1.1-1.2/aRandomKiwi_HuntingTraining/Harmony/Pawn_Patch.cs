using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System;

namespace aRandomKiwi.HFM
{
    internal class Pawn_Patch
    {
        /*
         * Patch to avoid an error related to the mod which causes either jobs or curdriver == null when calling this function
         */
        [HarmonyPatch(typeof(Pawn), "get_TicksPerMoveCardinal")]
        public class TicksPerMoveCardinal
        {
            [HarmonyPostfix]
            public static void Listener(Pawn __instance, ref int __result)
            {
                if (Utils.disableMoveSpeedPatch || !Settings.sameSpeedPacks)
                    return;

                try
                {
                    Comp_Hunting comp = __instance.TryGetComp<Comp_Hunting>();
                    if (comp != null)
                    {
                        Comp_Hunting origComp = comp;
                        Pawn master = null;

                        //We go back to the master if necessary
                        if (comp.huntingPackMaster != null)
                        {
                            master = comp.huntingPackMaster;
                            comp = comp.huntingPackMaster.TryGetComp<Comp_Hunting>();
                        }
                        else
                            master = __instance;

                        //If animal and in pack hunting mode
                        if (comp != null && comp.huntingPackMembers != null && comp.huntingPackMembers.Count > 0 && origComp.huntingArrivedToWaitingPoint)
                        {
                            Utils.disableMoveSpeedPatch = true;

                            int ret = -1;
                            //Lowest TicksPerMoveCardinal Deduction in the Pack
                            foreach (var m in comp.huntingPackMembers)
                            {
                                if (m == null)
                                    continue;
                                int cur = m.TicksPerMoveCardinal;
                                if (ret == -1 || ret < cur)
                                    ret = cur;
                            }

                            if (master != null && master.TicksPerMoveCardinal > ret)
                                __result = master.TicksPerMoveCardinal;
                            else
                                __result = ret;

                            Utils.disableMoveSpeedPatch = false;
                        }
                    }
                }
                catch(Exception e)
                {
                    Log.Message("[KFM_Error] get_TicksPerMoveCardinal " + e.Message);
                }
            }
        }

        /*
         * Patch to avoid an error related to the mod which causes either jobs or curdriver == null when calling this function
         */
        [HarmonyPatch(typeof(Pawn), "get_TicksPerMoveDiagonal")]
        public class TicksPerMoveDiagonal
        {
            [HarmonyPostfix]
            public static void Listener(Pawn __instance, ref int __result)
            {
                if (Utils.disableMoveSpeedPatch || !Settings.sameSpeedPacks)
                    return;
                try
                {
                    Comp_Hunting comp = __instance.TryGetComp<Comp_Hunting>();
                    if (comp != null)
                    {
                        Comp_Hunting origComp = comp;
                        Pawn master = null;

                        //We go back to the master if necessary
                        if (comp.huntingPackMaster != null)
                        {
                            master = comp.huntingPackMaster;
                            comp = comp.huntingPackMaster.TryGetComp<Comp_Hunting>();
                        }
                        else
                            master = __instance;

                        //If animal and in pack hunting mode
                        if (comp != null && comp.huntingPackMembers != null && comp.huntingPackMembers.Count > 0 && origComp.huntingArrivedToWaitingPoint)
                        {
                            Utils.disableMoveSpeedPatch = true;

                            int ret = -1;
                            //Lowest TicksPerMoveCardinal Deduction in the Pack
                            foreach (var m in comp.huntingPackMembers)
                            {
                                if (m == null)
                                    continue;
                                int cur = m.TicksPerMoveDiagonal;
                                if (ret == -1 || ret < cur)
                                    ret = cur;
                            }

                            if (master != null && master.TicksPerMoveDiagonal > ret)
                                __result = master.TicksPerMoveDiagonal;
                            else
                                __result = ret;

                            Utils.disableMoveSpeedPatch = false;
                        }
                    }
                }
                catch(Exception e)
                {
                    Log.Message("[KFM_Error] get_TicksPerMoveDiagonal " + e.Message);
                }
            }
        }
    }
}