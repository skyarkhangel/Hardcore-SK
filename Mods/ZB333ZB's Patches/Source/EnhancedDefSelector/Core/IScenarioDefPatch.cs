using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EnhancedDefSelector.Core
{
    public interface IScenarioDefPatch
    {
        IEnumerable<Def> GetAvailableDefs();
        void OnDefSelected(Def selectedDef);
        string GetCurrentDefLabel();
        Rect GetEditRect(Listing_ScenEdit listing);
    }
}