using UnityEngine;

namespace Hospitality.Spacer
{
    public class IncidentWorker_VisitorGroupSpacerLuxury : IncidentWorker_VisitorGroupSpacer
    {
        protected override int GetGroupSize()
        {
            return Random.Range(2, 6);
        }
    }
}
