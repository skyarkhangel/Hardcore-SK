using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.sectionlayer.thingsdrawlayer", Category.Update)]
    internal class H_SectionLayer_ThingsDrawLayer
    {
        public static bool Active = false;

        [Setting("OptimizeTest")] public static bool ByDef = false;

        public static void ProfilePatch()
        {
            Modbase.Harmony.Patch(AccessTools.Method(typeof(SectionLayer_Things), nameof(SectionLayer_Things.DrawLayer)),
                new HarmonyMethod(typeof(H_SectionLayer_ThingsDrawLayer), "Prefix"));
        }

        static Matrix4x4 johnmatrix = Matrix4x4.TRS(Vector3.zeroVector, Quaternion.identityQuaternion, Vector3.oneVector);


        public static bool Prefix(MethodBase __originalMethod, SectionLayer_Things __instance)
        {
            if (!Active)
            {
                return true;
            }

            if (ByDef)
            {
                if (!__instance.Visible)
                {
                    return false;
                }
                int count = __instance.subMeshes.Count;



                for (int i = 0; i < count; i++)
                {
                    LayerSubMesh layerSubMesh = __instance.subMeshes[i];
                    if (layerSubMesh.finalized && !layerSubMesh.disabled)
                    {
                        string Namer()
                        {
                            var n = layerSubMesh.material?.mainTexture?.name ?? layerSubMesh.GetType().Name;
                            return n;
                        }

                        var name = layerSubMesh.material?.mainTexture?.name ?? layerSubMesh.GetType().Name;

                        var prof = ProfileController.Start(name, Namer, __originalMethod.GetType(), null, null, __originalMethod);
                        Graphics.Internal_DrawMesh_Injected(layerSubMesh.mesh, 0, ref johnmatrix, layerSubMesh.material, 0, null, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off, null);

                        // Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zeroVector, Quaternion.identityQuaternion, layerSubMesh.material, 0);
                        prof.Stop();
                    }
                }



                return false;
            }

            {
                if (!__instance.Visible)
                {
                    return false;
                }

                int count = __instance.subMeshes.Count;



                for (int i = 0; i < count; i++)
                {
                    LayerSubMesh layerSubMesh = __instance.subMeshes[i];
                    if (layerSubMesh.finalized && !layerSubMesh.disabled)
                    {
                        string Namer()
                        {
                            var n = layerSubMesh.material?.mainTexture?.name ?? layerSubMesh.GetType().Name;
                            return n;
                        }

                        var name = layerSubMesh.material?.mainTexture?.name ?? layerSubMesh.GetType().Name;

                        var prof = ProfileController.Start(name, Namer, __originalMethod.GetType(), null, null,
                            __originalMethod);
                        Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material,
                            0);
                        prof.Stop();
                    }
                }


            }

            return false;
        }
    }
}