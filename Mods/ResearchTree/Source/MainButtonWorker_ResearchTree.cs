// MainButtonWorker_ResearchTree.cs
// Copyright Karel Kroeze, 2018-2020

using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyResearchTree
{
    public class MainButtonWorker_ResearchTree : MainButtonWorker_ToggleResearchTab
    {
        public override void DoButton( Rect rect )
        {
            base.DoButton( rect );

            if ( Queue.NumQueued > 0 )
            {
                var queueRect = new Rect(
                    rect.xMax - Constants.SmallQueueLabelSize - Constants.Margin,
                    0f,
                    Constants.SmallQueueLabelSize,
                    Constants.SmallQueueLabelSize ).CenteredOnYIn( rect );
                Queue.DrawLabel( queueRect, Color.white, Color.grey, Queue.NumQueued );
            }
        }
    }
}