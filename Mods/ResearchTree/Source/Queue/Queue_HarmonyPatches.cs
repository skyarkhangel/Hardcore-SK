// Queue_HarmonyPatches.cs
// Copyright Karel Kroeze, 2020-2020

using HarmonyLib;
using RimWorld;
using Verse;

namespace FluffyResearchTree
{
    public class HarmonyPatches_Queue
    {
        [HarmonyPatch( typeof( ResearchManager ), "ResearchPerformed", typeof( float ), typeof( Pawn ) )]
        public class ResearchPerformed
        {
            // check if last active project was finished. If so, try start the next project.
            // Thanks to NotFood for this nice simplification, I've adapted his/her code;
            // https://github.com/notfood/RimWorld-ResearchPal/blob/master/Source/Injectors/ResearchManagerPatch.cs
            private static void Prefix( ResearchManager __instance, ref ResearchProjectDef __state )
            {
                __state = __instance.currentProj;
                Log.Debug( "{0} progress: {1}", __state.LabelCap, __state.ProgressPercent );
            }

            private static void Postfix( ResearchProjectDef __state )
            {
                Log.Debug( "{0} finished?: {1}", __state, __state?.IsFinished );
                if ( __state?.IsFinished ?? false )
                {
                    Log.Debug( "{0} finished", __state.LabelCap );
                    Queue.TryStartNext( __state );
                }
            }
        }

        [HarmonyPatch( typeof( ResearchManager ), "FinishProject" )]
        public class DoCompletionDialog
        {
            // suppress vanilla completion dialog, we never want to show it.
            private static void Prefix( ref bool doCompletionDialog )
            {
                doCompletionDialog = false;
            }
        }
    }
}