using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.IO;
using UnityEngine;
using Verse;
using Object = System.Object;

namespace GraphicSetter
{
    public static class GraphicsPatches
    {
        //Manual Patch
        /*
        public static class PawnTextureAtlasCtorPatch
        {
            private static readonly MethodInfo Injection = AccessTools.Method(typeof(PawnTextureAtlasCtorPatch), nameof(CustomRenderTexture));
            private static readonly ConstructorInfo Comparision = AccessTools.Constructor(typeof(Object));
            private static readonly FieldInfo TextureInfo = AccessTools.Field(typeof(PawnTextureAtlas), nameof(PawnTextureAtlas.texture));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool foundObjectInit = false;
                bool initInjection = true;
                bool ignoringUntilOldRenderTex = true;

                foreach (var instruction in instructions)
                {
                    if (foundObjectInit)
                    {
                        if (initInjection)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, Injection);
                            yield return new CodeInstruction(OpCodes.Stfld, TextureInfo);
                            initInjection = false;
                        }

                        if (ignoringUntilOldRenderTex)
                        {
                            if (instruction.opcode == OpCodes.Stfld && instruction.operand is FieldInfo fInfo && fInfo == TextureInfo)
                            {
                                ignoringUntilOldRenderTex = false;
                            }
                            continue;
                        }
                    }

                    if (instruction?.operand is ConstructorInfo infoTwo && infoTwo == Comparision)
                    {
                        foundObjectInit = true;
                    }
                    yield return instruction;
                }

            }

            private static RenderTexture CustomRenderTexture()
            {
                if (GraphicsSettings.mainSettings.useCustomPawnAtlas)
                {
                    return ImprovedTextureAtlasing.CreatePawnRenderTex();
                }
                return new RenderTexture(2048, 2048, 24, RenderTextureFormat.ARGB32, 0);
            }
        }
        */

        /*
        public static class PawnTextureAtlasCtorPatch
        {
            private static MethodInfo injection = AccessTools.Method(typeof(PawnTextureAtlasCtorPatch), nameof(CustomRenderTexture));
            private static ConstructorInfo comparision = AccessTools.Constructor(typeof(Object));
            private static FieldInfo textureInfo = AccessTools.Field(typeof(PawnTextureAtlas), nameof(PawnTextureAtlas.texture));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool foundObjectInit = false;
                bool initInjection = true;

                foreach (var instruction in instructions)
                {
                    if (foundObjectInit)
                    {
                        if (initInjection && (instruction.opcode == OpCodes.Stfld && instruction.operand is FieldInfo fInfo && fInfo == textureInfo))
                        {
                            yield return new CodeInstruction(OpCodes.Callvirt, injection);
                            yield return new CodeInstruction(OpCodes.Stfld, textureInfo);
                            initInjection = false;
                            continue;
                        }
                    }

                    if (instruction?.operand is ConstructorInfo infoTwo && infoTwo == comparision)
                    {
                        foundObjectInit = true;
                    }
                    yield return instruction;
                }
            }

            private static RenderTexture CustomRenderTexture(RenderTexture rTex)
            {
                if (GraphicsSettings.mainSettings.useCustomPawnAtlas)
                {
                    return ImprovedTextureAtlasing.CreatePawnRenderTex(rTex.width, rTex.height);
                }
                return new RenderTexture(rTex.width, rTex.height, 24, RenderTextureFormat.ARGB32, 0);
            }
        }
        */

        //Automatic Patches
        [HarmonyPatch(typeof(ModContentLoader<Texture2D>))]
        [HarmonyPatch("LoadTexture")]
        public static class LoadPNGPatch
        {
            static bool Prefix(VirtualFile file, ref Texture2D __result)
            {
                __result = StaticTools.LoadTexture(file);
                return false;
            }
        }

        [HarmonyPatch(typeof(MainMenuDrawer))]
        [HarmonyPatch("DoMainMenuControls")]
        public static class DoMainMenuControlsPatch
        {
            public static float addedHeight = 45f + 7f;
            public static List<ListableOption> OptionList;
            private static MethodInfo ListingOption = SymbolExtensions.GetMethodInfo(() => AdjustList(null));

            static void AdjustList(List<ListableOption> optList)
            {
                var label = "Options".Translate();
                var idx = optList.FirstIndexOf(opt => opt.label == label);
                if (idx > 0 && idx < optList.Count) optList.Insert(idx + 1, new ListableOption("GS_MenuTitle".Translate(), delegate
                {
                    var dialog = new Dialog_ModSettings(GraphicSetter.ModRef);
                    var me = LoadedModManager.GetMod<GraphicSetter>();
                    StaticContent.selModByRef(dialog) = me;
                    Find.WindowStack.Add(dialog);
                }, null));
                OptionList = optList;
            }

            static bool Prefix(ref Rect rect, bool anyMapFiles)
            {
                rect = new Rect(rect.x, rect.y, rect.width, rect.height + addedHeight);
                return true;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var m_DrawOptionListing = SymbolExtensions.GetMethodInfo(() => OptionListingUtility.DrawOptionListing(Rect.zero, null));

                var instructionsList = instructions.ToList();
                var patched = false;
                for (var i = 0; i < instructionsList.Count; i++)
                {
                    var instruction = instructionsList[i];
                    if (i + 2 < instructionsList.Count)
                    {
                        var checkingIns = instructionsList[i + 2];
                        if (!patched && checkingIns != null && checkingIns.Calls(m_DrawOptionListing))
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_2);
                            yield return new CodeInstruction(OpCodes.Call, ListingOption);
                            patched = true;
                        }
                    }
                    yield return instruction;
                }
            }
        }

        
        [HarmonyPatch(typeof(TextureAtlasHelper), nameof(TextureAtlasHelper.MakeReadableTextureInstance))]
        public static class MakeReadableTextureInstance_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Texture2D source, ref Texture2D __result)
            {
                __result = ImprovedTextureAtlasing.MakeReadableTextureInstance_Fixed(source);
                return false;
            }
        }


        /*
        public static class RenderPatch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var patched = false;
                foreach (CodeInstruction instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Stloc_3 && !patched)
                    {
                        patched = true;
                        yield return CodeInstruction.Call(typeof(RenderPawnAt_Patch), nameof(AdjustCachingFlag));

                        yield return instruction;

                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return CodeInstruction.Call(typeof(RenderPawnAt_Patch), nameof(AdjustFlags));
                        yield return new CodeInstruction(OpCodes.Stloc_1);
                    }
                    else 
                        yield return instruction;
                }
            }

            public static bool AdjustCachingFlag(bool curFlag)
            {
                //Log.Message($"Current flag: {curFlag}");
                return !GraphicsSettings.mainSettings.disableAtlasCaching && curFlag;
            }

            public static PawnRenderFlags AdjustFlags(PawnRenderFlags pawnRenderFlags)
            {
                pawnRenderFlags |= PawnRenderFlags.Headgear;
                pawnRenderFlags |= PawnRenderFlags.Clothes;
                return pawnRenderFlags;
            }
        }
        */
    }
}
