
using UnityEngine;

namespace Hospitality.Spacer
{
    public class IncidentWorker_VisitorGroupSpacerCruise : IncidentWorker_VisitorGroupSpacer
    {
        protected override int GetGroupSize()
        {
            return Random.Range(10, 30);
        }
    }
}