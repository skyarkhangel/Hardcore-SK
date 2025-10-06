using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;


namespace NOQ_CoordReadout
{
    public class Main : Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("noq.coordread");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            Settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory() { return "CoordRead.CoordReadout".Translate(); }
    }
}
