using System;
using System.Collections.Generic;
using RimWorld;
using RocketMan;
using Verse.Noise;

namespace Proton
{
    public static class AlertsUtility
    {
        public static string GetName(this Alert alert)
        {
            string typeName = alert.GetType().Name;
            return typeName.Replace("Alert_", string.Empty).SplitStringByCapitalLetters();
        }

        public static string GetNameLower(this Alert alert)
        {
            return alert.GetName().ToLower();
        }
    }
}
