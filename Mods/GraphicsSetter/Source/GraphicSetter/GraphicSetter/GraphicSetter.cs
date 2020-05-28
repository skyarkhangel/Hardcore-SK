using System;
using System.Collections.Generic;
using System.IO;
using RimWorld.IO;
using RuntimeAudioClipLoader;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GraphicSetter
{
    public class GraphicSetter : Mod
    {
        public static GraphicsSettings settings;

        public GraphicSetter(ModContentPack content) : base(content)
        {
            Log.Message("Graphics Setter - Loaded");
            settings = GetSettings<GraphicsSettings>();
            Harmony graphics = new Harmony("com.telefonmast.graphicssettings.rimworld.mod");
            graphics.PatchAll();
        }

        [HarmonyPatch(typeof(ModContentLoader<Texture2D>))]
        [HarmonyPatch("LoadTexture")]
        public static class LoadPNGPatch
        {
            static bool Prefix(VirtualFile file, ref Texture2D __result)
            {
                GraphicsSettings.LoadTexture(file, ref __result);
                return false;
            }
        }

        public override void WriteSettings()
        {
            settings.Write();
            base.WriteSettings();
        }

        public override string SettingsCategory()
        {
            return "Graphics Setter";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }
    }

    public class SettingsGroup
    {
        public int anisoLevel = 2;
        public FilterMode filterMode = FilterMode.Bilinear;
        public bool useMipMap = true;
        public bool compressImages = true;
        public float mipMapBias = 0f;
    }

    public class GraphicsSettings : ModSettings
    {
        private SettingsGroup lastSettings = new SettingsGroup();

        public int anisoLevel = 2;
        public FilterMode filterMode = FilterMode.Bilinear;
        public bool useMipMap = true;
        public bool compressImages = true;
        public float mipMapBias = 0f;

        public readonly FloatRange anisoRange = new FloatRange(1, 9);
        public readonly FloatRange MipMapBiasRange = new FloatRange(-1f, 1f);

        public void DoSettingsWindowContents(Rect inRect)
        {
            float curY = 50f;
            CheckBox("Compress Textures", ref curY, ref compressImages);
            CheckBox("(Recommended) Activate Mip-Mapping", ref curY, ref useMipMap);
            Text.Anchor = TextAnchor.UpperCenter;
            if (useMipMap)
                mipMapBias = LabeledSlider("MipMap Bias", ref curY, MipMapBiasRange, mipMapBias, "Sharpest", "Blurriest", "This value changes how blurry textures can get depending on zoom, it is recommended to be equal or below 0.", 0.05f);
            curY += 10f;
            anisoLevel = (int)LabeledSlider("Anisotropic Filter Level", ref curY, anisoRange, (float)anisoLevel, "", "", "Set the level of anisotropic filtering, higher levels may reduce performance on older graphics cards.", 1);
            Text.Anchor = 0;
            curY += 10f;
            SetFilter(curY);

            float x1 = (inRect.width - 120)/2;
            Text.Anchor = TextAnchor.UpperCenter;
            Rect presetLabel = new Rect(x1, 50, 120, 25);
            Widgets.Label(presetLabel, "- PRESETS -");
            Text.Anchor = 0;
            Rect vanilla = new Rect(x1,presetLabel.yMax,120,35);
            Rect better = new Rect(x1, vanilla.yMax + 5, 120, 35);
            Rect ultra = new Rect(x1, better.yMax + 5, 120, 35);
            if (Widgets.ButtonText(vanilla, "Vanilla"))
            {
                anisoLevel = 2;
                filterMode = FilterMode.Bilinear;
                compressImages = true;
                useMipMap = true;
                mipMapBias = 0;
            }
            if (Widgets.ButtonText(better, "Better"))
            {
                anisoLevel = 2;
                filterMode = FilterMode.Trilinear;
                compressImages = false;
                useMipMap = true;
                mipMapBias = 0;
            }
            if (Widgets.ButtonText(ultra, "Redefined"))
            {
                anisoLevel = 9;
                filterMode = FilterMode.Trilinear;
                compressImages = false;
                useMipMap = true;
                mipMapBias = -0.6f;
            }

            if (AnySettingsChanged())
            {
                GUI.color = Color.red;
                Text.Font = GameFont.Medium;
                //string text = "Reloading may take a while and freeze your game.";
                string text = "You will have to restart the game to apply changes!";
                Vector2 size = Text.CalcSize(text);
                float x2 = (inRect.width - size.x) / 2f;
                float x3 = (inRect.width - 150) / 2f;
                float y2 = inRect.yMax - 100;
                Widgets.Label(new Rect(x2, y2, size.x, size.y), text);
                if (Widgets.ButtonText(new Rect(x3, inRect.yMax - 55, 150, 45), "Restart Game", true, true))
                {
                    this.Write();
                    GenCommandLine.Restart();
                }
            }
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private bool AnySettingsChanged()
        {
            if (anisoLevel != lastSettings.anisoLevel)
                return true;
            if (compressImages != lastSettings.compressImages)
                return true;
            if (filterMode != lastSettings.filterMode)
                return true;
            if (mipMapBias != lastSettings.mipMapBias)
                return true;
            if (useMipMap != lastSettings.useMipMap)
                return true;
            return false;
        }

        public static void LoadTexture(VirtualFile file, ref Texture2D __result)
        {
            Texture2D texture2D = null;
            var settings = GraphicSetter.settings;
            if (file.Exists)
            {
                byte[] data = file.ReadAllBytes();
                texture2D = new Texture2D(2, 2, TextureFormat.Alpha8, settings.useMipMap);
                texture2D.LoadImage(data);
                if (settings.compressImages)
                {
                    texture2D.Compress(true);
                }
                texture2D.name = Path.GetFileNameWithoutExtension(file.Name);
                texture2D.filterMode = settings.filterMode;
                texture2D.anisoLevel = settings.anisoLevel;
                texture2D.mipMapBias = settings.mipMapBias;
                texture2D.Apply(true, true);
            }
            __result = texture2D;
        }

        //TODO: Figure a way to use this
        private void ReloadTextures()
        {
            int i = 0,
                k = 0;
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                i++;
                ModContentHolder<Texture2D> mch = Traverse.Create(mod).Field("textures").GetValue<ModContentHolder<Texture2D>>();
                Log.Message("Got modcontentholder for " + mod.PackageId + " with " + mch.contentList.Count + " textures.");
                mod.assemblies.ReloadAll();
                mch.ClearDestroy();
                mch.ReloadAll();
                mod.AllDefs.Do<Def>(delegate (Def def)
                {
                    if(def is ThingDef thingDef)
                    {
                        if(thingDef.graphic != null)
                            thingDef.graphic = null;
                        if (thingDef.graphicData != null)
                        {
                            Traverse.Create(thingDef.graphicData).Field("cachedGraphic").SetValue(null);
                            thingDef.graphic = thingDef.graphicData.Graphic;
                        }
                    }
                });
                k += mch.contentList.Count;
            }
            Log.Message("Reloaded " + k + " textures for: " + i + " mods");
        }

        public void CheckBox(string label, ref float curY, ref bool flag)
        {
            Vector2 size = Text.CalcSize(label);
            Rect rect = new Rect(0, curY, size.x + 25, size.y);
            Widgets.CheckboxLabeled(rect, label, ref flag);
            curY = rect.yMax;
        }

        public float LabeledSlider(string label, ref float curY, FloatRange range, float value, string leftLabel, string rightLabel, string tooltip = null, float roundTo = 0.1f)
        {
            float val = 0;
            Vector2 size = Text.CalcSize(label);
            Rect toolTipRect = new Rect(0, curY, 200f, size.y *2);
            Widgets.Label(toolTipRect.TopHalf(), label);
            val = Widgets.HorizontalSlider(toolTipRect.BottomHalf(), value, range.min, range.max, false, value.ToString(), leftLabel, rightLabel, roundTo);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(toolTipRect, tooltip);
            }
            curY = toolTipRect.yMax;
            return val;
        }

        public void TwoEndedLabel(string label1, string label2, float width, ref float curY)
        {
            Text.Font = GameFont.Tiny;
            Vector2 size1 = Text.CalcSize(label1);
            Vector2 size2 = Text.CalcSize(label2);
            Rect left = new Rect(new Vector2(0, curY),size1);
            Rect right = new Rect(new Vector2(width-size2.x, curY),size2);
            Widgets.Label(left,label1);
            Widgets.Label(right, label2);
            curY += size1.y;
            Text.Font = GameFont.Small;
        }

        public void SetFilter(float curY)
        {
            Widgets.Label(new Rect(0, curY, 200, 25f), "Texture Filtering: ");
            Rect button1 = new Rect(20, curY + 25f, 85, 22f);
            Rect button2 = new Rect(button1.x, button1.yMax, button1.width, button1.height);
            if (Widgets.RadioButtonLabeled(button1,FilterMode.Bilinear.ToString(), filterMode == FilterMode.Bilinear))
            {
                filterMode = FilterMode.Bilinear;
            }
            if (Widgets.RadioButtonLabeled(button2, FilterMode.Trilinear.ToString(), filterMode == FilterMode.Trilinear))
            {
                filterMode = FilterMode.Trilinear;
            }
        }

        public override void ExposeData()
        {         
            base.ExposeData();
            Scribe_Values.Look(ref anisoLevel, "anisoLevel");
            Scribe_Values.Look(ref useMipMap, "useMipMap");
            Scribe_Values.Look(ref compressImages, "compressImages");
            Scribe_Values.Look(ref filterMode, "filterMode");
            Scribe_Values.Look(ref mipMapBias, "mipMapBias");

            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                lastSettings = new SettingsGroup();
                lastSettings.anisoLevel = anisoLevel;
                lastSettings.useMipMap = useMipMap;
                lastSettings.compressImages = compressImages;
                lastSettings.filterMode = filterMode;
                lastSettings.mipMapBias = mipMapBias;
            }
        }
    }
}
