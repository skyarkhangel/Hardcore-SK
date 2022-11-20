using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld.IO;
using RuntimeAudioClipLoader;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Verse;

namespace GraphicSetter
{
    public class GraphicSetter : Mod
    {
        private static GraphicsSettings settings;

        public static GraphicSetter ModRef { get; private set; }
        
        public static GraphicsSettings Settings
        {
            get => settings;
            private set => settings = value;
        }

        public GraphicSetter(ModContentPack content) : base(content)
        {
            ModRef = this;
            Log.Message("[1.3]Graphics Setter - Loaded");
            Settings = GetSettings<GraphicsSettings>();
            //profiler = new ResourceProfiler();
            Harmony graphics = new Harmony("com.telefonmast.graphicssettings.rimworld.mod");
            
            //Maybe obsolete?
            //graphics.Patch(AccessTools.Constructor(typeof(PawnTextureAtlas)), transpiler: new(typeof(GraphicsPatches.PawnTextureAtlasCtorPatch).GetMethod(nameof(GraphicsPatches.PawnTextureAtlasCtorPatch.Transpiler)), Priority.First));
            graphics.PatchAll();
        }

        public override void WriteSettings()
        {
            Settings.Write();
            base.WriteSettings();
        }

        public override string SettingsCategory()
        {
            return "GS_MenuTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}
