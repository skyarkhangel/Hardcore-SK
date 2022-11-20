
using UnityEngine;

namespace Hospitality.Spacer
{
    public class IncidentWorker_VisitorGroupSpacerCruise : IncidentWorker_VisitorGroupSpacer
    {
        protected override int GetGroupSize()
        {
            return Verse.Rand.Range(10, 30);
        }
    }
}
