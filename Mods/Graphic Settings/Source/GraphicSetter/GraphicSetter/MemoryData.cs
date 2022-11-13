using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace GraphicSetter
{
    public class MemoryData
    {
        //private static Dictionary<ModContentPack, long> RawMemoryUsageByMod = new Dictionary<ModContentPack, long>();
        private static readonly Dictionary<ModContentPack, long> MemoryUsageByMod = new();
        private static readonly Dictionary<Pair<string, bool>, long> MemoryUsageByAtlas = new();

        private static Color ListingBG = new ColorInt(32, 36, 40).ToColor;
        private static Color NiceBlue = new ColorInt(38, 169, 224).ToColor;

        private static IEnumerable<ModContentPack> CurrentMods => LoadedModManager.RunningMods.Where(t => t.GetContentHolder<Texture2D>().contentList.Any());
        private bool shouldRecalc = false;
        private bool shouldStop = false;

        private long TotalBytes => MemoryUsageByMod.Sum(t => t.Value);
        private long TotalBytesAtlas => MemoryUsageByAtlas.Sum(t => t.Value);

        private long TotalUsage => TotalBytes + TotalBytesAtlas;

        private long LargestModSize { get; set; }
        private long LargestAtlasSize { get; set; }

        private long TotalRAM => SystemInfo.systemMemorySize * 1000000L;
        private long TotalVRAM => SystemInfo.graphicsMemorySize * 1000000L;

        private long MainMemory => TotalRAM;
        private float TotalPctUsage => (float)(TotalUsage / (double)MainMemory);

        private bool Calculating => shouldRecalc;

        private static readonly float CriticalPct = 0.8f;//0.75f;

        private bool Critical => TotalPctUsage > CriticalPct;

        public bool MEMOVERFLOW => TotalPctUsage > 1f;

        public static Texture2D ToTexture2D(RenderTexture rTex)
        {
            RenderTexture.active = rTex;
            var settings = GraphicsSettings.mainSettings;
            var size = 2048 * settings.pawnTexResScale;
            Texture2D dest = new Texture2D(size, size, TextureFormat.RGBA32, false);
            dest.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            dest.Apply(false, false);
            RenderTexture.active = null;
            return dest;
        }

        public void Notify_SettingsChanged()
        {
            shouldRecalc = false;
            shouldStop = false;
            MemoryUsageByMod.Clear();
        }

        public Coroutine routine, routine2;

        public void Notify_GetAtlasCache()
        {
            routine2 = StaticContent.CoroutineDriver.StartCoroutine(CalculateAtlasMemory());
        }

        public void Notify_ChangeState()
        {
            if (!CurrentMods.Any()) return;

            if (shouldRecalc)
            {
                shouldStop = !shouldStop;
                return;
            }
            routine = StaticContent.CoroutineDriver.StartCoroutine(DoTheThing());
            if (TotalBytesAtlas <= 0)
            {
                Notify_GetAtlasCache();
            }
            shouldRecalc = true;
        }

        public IEnumerator CalculateAtlasMemory()
        {
            MemoryUsageByAtlas.Clear();

            var allAtlases = GlobalTextureAtlasManager.staticTextureAtlases;
            foreach (var atlas in allAtlases)
            {
                var texture = TextureAtlasHelper.MakeReadableTextureInstance(atlas.ColorTexture);
                long size = texture.GetRawTextureData().Length;
                new DisposableTexture(texture).Dispose();

                var pair = new Pair<string, bool>(atlas.groupKey.@group.ToString(), atlas.groupKey.hasMask);
                MemoryUsageByAtlas.Add(pair, size);
                LargestAtlasSize = MemoryUsageByAtlas[pair] > LargestAtlasSize ? MemoryUsageByAtlas[pair] : LargestAtlasSize;
                yield return null;
            }
        }

        public IEnumerator DoTheThing()
        {
            MemoryUsageByMod.Clear();
            int count = CurrentMods.Count();
            int k = 0;
            while (shouldStop || k < count)
            {
                if (shouldStop)
                {
                    yield return null;
                    continue;
                }
                var mod = CurrentMods.ElementAt(k);
                MemoryUsageByMod.Add(mod, 0);
                Dictionary<string, FileInfo> allFilesForMod = ModContentPack.GetAllFilesForMod(mod, GenFilePaths.ContentPath<Texture2D>(), (ModContentLoader<Texture2D>.IsAcceptableExtension));
                int i = 0;
                while (shouldStop || i < allFilesForMod.Count)
                {
                    if (shouldStop)
                    {
                        yield return null;
                        continue;
                    }
                    var pair = allFilesForMod.ElementAt(i);
                    MemoryUsageByMod[mod] += StaticTools.TextureSize(new VirtualFileWrapper(pair.Value));
                    i++;
                    if (i % 3 == 0) yield return null;
                }

                LargestModSize = MemoryUsageByMod[mod] > LargestModSize ? MemoryUsageByMod[mod] : LargestModSize;
                k++;
            }
            shouldRecalc = false;
            GraphicSetter.Settings.CausedMemOverflow = MEMOVERFLOW;
        }

        private float MemoryPctOf(Pair<string, bool> pair, out long memUsage)
        {
            memUsage = 0;
            if (!MemoryUsageByAtlas.ContainsKey(pair)) return 0;
            memUsage = MemoryUsageByAtlas[pair];
            return (float)((double)memUsage / (double)TotalBytesAtlas);
        }

        private Color GetColorFor(Pair<string, bool> pair)
        {
            if (!MemoryUsageByAtlas.ContainsKey(pair)) return Color.green;
            var memUsage = MemoryUsageByAtlas[pair];
            var floatPct = (float)((double)memUsage / (double)LargestAtlasSize);
            return Color.Lerp(NiceBlue, Color.magenta, floatPct);
        }

        private float MemoryPctOf(ModContentPack mod, out long memUsage)
        {
            memUsage = 0;
            if (!MemoryUsageByMod.ContainsKey(mod)) return 0;
            memUsage = MemoryUsageByMod[mod];
            return (float)((double)memUsage / (double)TotalBytes);
        }

        private Color GetColorFor(ModContentPack mod)
        {
            if (!MemoryUsageByMod.ContainsKey(mod)) return Color.green;
            var memUsage = MemoryUsageByMod[mod];
            var floatPct = (float)((double)memUsage / (double)LargestModSize);
            return Color.Lerp(NiceBlue, Color.magenta, floatPct);
        }

        private static string MemoryString(long memUsage, long maxMem, bool cap = false)
        {
            //return memUsage + " bytes";
            if (cap && memUsage > maxMem)
            {
                return ">" + MemoryString(maxMem, maxMem);
            }
            if (memUsage < 1000)
            {
                return memUsage + " bytes";
            }

            if (memUsage < 1000000)
            {
                return Math.Round((memUsage / 1000d), 2) + "kB";
            }

            if (memUsage < 1000000000)
            {
                return Math.Round((memUsage / 1000000d), 2) + "MB";
            }
            return Math.Round((memUsage / 1000000000d), 2) + "GB";
        }

        //Render Data
        private Vector2 scrollview = new Vector2(0,0);

        public void DrawPawnAtlasMemory(Rect rect)
        {

        }

        public void DrawMemoryData(Rect rect)
        {
            Rect topHalf = rect.TopPart(0.75f);
            Rect bottomHalf = rect.BottomPart(0.25f);
            DrawModList(topHalf);
            WriteProcessingData(bottomHalf);
        }

        public void DrawModList(Rect rect)
        {
            rect = new Rect(rect.x, rect.y + 20, rect.width, rect.height - 20);
            Rect newRect = rect.ContractedBy(5);
            Rect leftSide = newRect.LeftHalf().ContractedBy(1);
            Rect rightSide = newRect.RightHalf().ContractedBy(1);

            Widgets.DrawBoxSolid(rect, ListingBG);
            GUI.color = Color.gray;
            Widgets.DrawBox(rect, 1);
            GUI.color = Color.white;

            Widgets.DrawBoxSolid(new Rect(leftSide.xMax, rect.y, 2, rect.height), Color.gray);

            Widgets.Label(new Rect(leftSide.x, rect.y - 20, leftSide.width, 20), "GS_AllTextureMemory".Translate());
            GUI.BeginGroup(leftSide);
            {
                int y = 0;
                Rect viewRect = new Rect(0, 0, leftSide.width, CurrentMods.Count() * 20);
                Widgets.BeginScrollView(new Rect(0, 0, leftSide.width, leftSide.height), ref scrollview, viewRect, false);
                var list = MemoryUsageByMod.ToList();
                list.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));
                foreach (var mod in list)
                {
                    var pct = MemoryPctOf(mod.Key, out long memUsage);
                    var text = mod.Key.Name + " (" + MemoryString(memUsage, MainMemory) + ") " + pct.ToStringPercent();
                    var tipRect = new Rect(0, y, rect.width, 20);
                    StaticTools.FillableBarLabeled(new Rect(0, y, leftSide.width, 18), pct, text, GetColorFor(mod.Key),
                        Color.clear, false);
                    //WidgetRow row = new WidgetRow(0, y, UIDirection.RightThenDown);
                    //row.FillableBar(newRect.width, 18, pct, text, GetColorFor(pct), StaticContent.clear);
                    Widgets.DrawHighlightIfMouseover(tipRect);
                    TooltipHandler.TipRegion(tipRect, text);
                    y += 20;
                }

                if (!MemoryUsageByMod.Any())
                {
                    string text = CurrentMods.Any()
                        ? "GS_ModsToProcessLabel".Translate(CurrentMods.Count())
                        : "GS_NoModsToProcess".Translate();
                    float textHeight = Text.CalcHeight(text, rect.width);
                    Widgets.Label(new Rect(0, y, rect.width, textHeight), text);
                }
                Widgets.EndScrollView();
            }
            GUI.EndGroup();

            //ATLAS
            Widgets.Label(new Rect(rightSide.x, rect.y - 20, rightSide.width, 20), "GS_CachedAtlasses".Translate());
            GUI.BeginGroup(rightSide);
            {
                int y = 0;
                Rect viewRect = new Rect(0, 0, rightSide.width, MemoryUsageByAtlas.Count() * 20);
                Widgets.BeginScrollView(new Rect(0, 0, rightSide.width, rightSide.height), ref scrollview, viewRect, false);
                var list = MemoryUsageByAtlas.ToList();
                list.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));
                foreach (var atlas in list)
                {
                    var pct = MemoryPctOf(atlas.Key, out long memUsage);
                    var text = $"{atlas.Key.First}{(atlas.Key.Second ? "[Masks]" : "")}: ({MemoryString(memUsage, MainMemory)}) {pct.ToStringPercent()}";
                    var tipRect = new Rect(0, y, rect.width, 20);
                    StaticTools.FillableBarLabeled(new Rect(0, y, rightSide.width, 18), pct, text, GetColorFor(atlas.Key), Color.clear, false);
                    Widgets.DrawHighlightIfMouseover(tipRect);
                    TooltipHandler.TipRegion(tipRect, text);
                    y += 20;
                }
                Widgets.EndScrollView();
            }
            GUI.EndGroup();
        }

        public void WriteProcessingData(Rect rect)
        {
            GUI.BeginGroup(rect);

            Rect buttons = new Rect(0, 5, rect.width * 0.20f, 22);
            Rect barRect = new Rect(buttons.xMax + 5, 5, rect.width - buttons.width - 5, 22);
            float curY = buttons.height;
            string text = shouldRecalc ? (shouldStop ? "GS_CacheContinue".Translate() : "GS_CacheStop".Translate() ) : "GS_CacheRecalc".Translate();
            if (Widgets.ButtonText(buttons, text, true, false, CurrentMods.Any()))
            {
                Notify_ChangeState();
            }

            Widgets.FillableBar(barRect, TotalPctUsage, StaticContent.blue, Texture2D.blackTexture, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            string label = MEMOVERFLOW ? "GS_CacheWarnRAM".Translate() : MemoryString(TotalUsage, MainMemory) + "/" + MemoryString(MainMemory, MainMemory);
            Widgets.Label(barRect, label);
            Text.Anchor = default;

            if (Calculating)
            {
                string calcLabel = $"{"GS_RecalcProcess".Translate()} ({MemoryUsageByMod.Count}/{CurrentMods.Count()})";
                var textSize = Text.CalcHeight(calcLabel, rect.width);
                Rect textRect = new Rect(0, curY + 5, rect.width, textSize);
                Widgets.Label(textRect, calcLabel);
                curY = textRect.yMax;
            }
            /*else if (GraphicSetter.settings.AnySettingsChanged())
            {
                string settingsChangedLabel = "Settings changed, recalculate to check the memory use!";
                var textSize = Text.CalcHeight(settingsChangedLabel, rect.width);
                Rect textRect = new Rect(0, curY + 5, rect.width, textSize);
                Widgets.Label(textRect, settingsChangedLabel);
                curY = textRect.yMax;
            }
            */
            if (Critical)
            {
                Text.Font = GameFont.Small;
                string warningLabel = "GS_CacheWarning".Translate((MEMOVERFLOW
                    ? "GS_CacheWarnOverflow".Translate()
                    : "GS_CacheWarnStruggle".Translate()));
                var textSize = Text.CalcHeight(warningLabel, rect.width);
                Rect warningLabelRect = new Rect(0, curY + 5, rect.width, textSize);
                Widgets.Label(warningLabelRect, warningLabel);
                Text.Font = GameFont.Small;
            }
            GUI.EndGroup();
        }
    }
}
