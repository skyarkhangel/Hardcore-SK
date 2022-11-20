using System;
using System.IO;
using RimWorld.IO;
using UnityEngine;
using Verse;

namespace GraphicSetter
{
    public static class StaticTools
    {
        public static void FillableBarLabeled(Rect rect, float fillPercent, string label, Color? fillCol, Color? bgCol, bool doBorder)
        {
            FillableBar(rect, fillPercent, fillCol, bgCol, doBorder);
            if (!label.NullOrEmpty())
            {
                Rect rect2 = rect;
                rect2.xMin += 2f;
                rect2.xMax -= 2f;
                if (Text.Anchor >= TextAnchor.UpperLeft)
                {
                    rect2.height += 14f;
                }

                Text.Font = GameFont.Tiny;
                Text.WordWrap = false;
                Widgets.Label(rect2, label);
                Text.WordWrap = true;
                Text.Font = GameFont.Small;
            }
        }

        public static void FillableBar(Rect rect, float fillPercent, Color? fillCol, Color? bgCol, bool doBorder)
        {
            if (doBorder)
            {
                GUI.DrawTexture(rect, BaseContent.BlackTex);
                rect = rect.ContractedBy(3f);
            }
            if (bgCol != null)
            {
                Widgets.DrawBoxSolid(rect, bgCol.Value);
                //GUI.DrawTexture(rect, bgTex);
            }
            rect.width *= fillPercent;
            Widgets.DrawBoxSolid(rect, fillCol.Value);
        }

        //
        public static Texture2D LoadTexture(VirtualFile file, bool readable = false)
        {
            Texture2D texture2D = null;
            var settings = GraphicsSettings.mainSettings;
            try
            {
                string ddsExtensionPath = Path.ChangeExtension(file.FullPath, ".dds");
                if (File.Exists(ddsExtensionPath))
                {
                    texture2D = DDSLoader.LoadDDS(ddsExtensionPath);
                    if(!DDSLoader.error.NullOrEmpty())
                        Log.Warning($"DDS ERROR at '{file.FullPath}': {DDSLoader.error}");
                    if(texture2D == null)
                        Log.Warning($"Couldn't load .dds from {file.Name} loading as png instead.");
                }
                if (texture2D == null && file.Exists)
                {
                    byte[] data = file.ReadAllBytes();
                    texture2D = new Texture2D(2, 2, TextureFormat.Alpha8, settings.useMipMap);
                    texture2D.LoadImage(data);
                }
                if (texture2D == null)
                {
                    throw new Exception($"Could not load texture at {file.FullPath}");
                }

                texture2D.Compress(true);
                texture2D.name = Path.GetFileNameWithoutExtension(file.Name);
                texture2D.filterMode = settings.filterMode;
                texture2D.anisoLevel = settings.anisoLevel;
                texture2D.mipMapBias = settings.mipMapBias;
                texture2D.Apply(true, !readable);

            }
            catch (Exception exception)
            {
                Log.Error($"[Graphics Settings][{(file?.Name ?? "Missing File...")}] {exception}");
            }
            return texture2D;
        }
    

        public static long TextureSize(VirtualFile file)
        {
            var texture2D = LoadTexture(file, true);
            if (texture2D == null) return 0;

            long size = texture2D.GetRawTextureData().Length;
            new DisposableTexture(texture2D).Dispose();
            return size;
        }

        //Widgets
        public static float LabeledSlider(this Listing_Standard listing, string label, FloatRange range, float value, string leftLabel = null, string rightLabel = null, string tooltip = null, float roundTo = 0.1f)
        {
            Vector2 size = Text.CalcSize(label);
            var rect = listing.GetRect(size.y * 2);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect.TopHalf(), label);
            Text.Anchor = default;

            value = Widgets.HorizontalSlider(rect.BottomHalf(), value, range.min, range.max, false, value.ToString(), leftLabel, rightLabel, roundTo);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
            return value;
        }

        public static void RenderInListing(this Listing_Standard listing, float untilY, Action<Rect> renderAction)
        {
            Rect rect = listing.GetRect(untilY - listing.curY);
            renderAction.Invoke(rect);
        }

        public static bool ButtonTextEnabled(this Listing_Standard listing, string label, bool isEnabled, string disabledToolTip = null)
        {
            Rect rect = listing.GetRect(30f);
            bool result = false;
            if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                if (!isEnabled)
                    GUI.color = Color.gray;
                result = Widgets.ButtonText(rect, label, true, true, isEnabled);
                GUI.color = Color.white;
                if (!isEnabled && disabledToolTip != null)
                {
                    TooltipHandler.TipRegion(rect, disabledToolTip);
                }
            }
            listing.Gap(listing.verticalSpacing);
            return result;
        }
    }
}
