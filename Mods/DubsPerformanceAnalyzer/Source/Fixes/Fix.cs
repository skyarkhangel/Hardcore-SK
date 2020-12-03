using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Performance;
using Analyzer.Profiling;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Analyzer.Fixes
{
    public class Fix
    {
        public virtual string Name => "";
        public AccessTools.FieldRef<bool> EnabledRefAccess;

        public void Initialise(Type subType)
        {
            EnabledRefAccess = AccessTools.StaticFieldRefAccess<bool>(subType.GetField("Active", BindingFlags.Public | BindingFlags.Static));
            if (EnabledRefAccess == null)
            {
                ThreadSafeLogger.Error("Add an 'Active' field you bloody muppet");
            }
        }

        public virtual void Draw(ref Listing_Standard listing)
        {
            var name = Name.TranslateSimple();
            var tooltip = (Name + ".tooltip").TranslateSimple();

            var height = Mathf.CeilToInt((name.GetWidthCached() + 30) / (listing.ColumnWidth)) * Text.LineHeight;
            var rect = listing.GetRect(height);

            if (DubGUI.Checkbox(rect, name, ref EnabledRefAccess()))
            {

            }
            TooltipHandler.TipRegion(rect, tooltip);
        }

        public virtual void OnEnabled()
        {

        }

        public virtual void OnGameInit(Game g, Harmony h)
        {

        }

        public virtual void OnGameLoaded(Game g, Harmony h)
        {

        }
    }
}
