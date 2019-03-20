/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 18.03.2019
 * Time: 11:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Harmony;
using UnityEngine;
using Verse;

namespace RIMMSLoadUp
{
	[HarmonyPatch(typeof(Verse.ModMetaData))]
	[HarmonyPatch("PreviewImagePath", MethodType.Getter)]
	static class DelayLoadingModPreviewImages {
		static bool allowLoadingPreviewImages = false;
		
		static bool Prefix(ref string __result) {
			if ( allowLoadingPreviewImages ) return true;
            __result = string.Empty;
            return false;
        }
		
		static public void ForbidLoadingPreviewImages() {
			allowLoadingPreviewImages = false;
			//clear loaded images
			foreach ( ModMetaData meta in ModLister.AllInstalledMods ) {
				if ( meta.previewImage != null ) {
					Resources.UnloadAsset(meta.previewImage);
					meta.previewImage = null;
				}
			}
		}
		static public void AllowLoadingPreviewImages() {
			allowLoadingPreviewImages = true;
		}
	}
	
	[HarmonyPatch(typeof(RimWorld.Page_ModsConfig))]
	[HarmonyPatch("PreOpen")]
	static class AllowLoadingModPreviewImages {
		static bool Prefix() {
			DelayLoadingModPreviewImages.AllowLoadingPreviewImages();
			return true;
		}
	}
	
	[HarmonyPatch(typeof(RimWorld.Page_ModsConfig))]
	[HarmonyPatch("PostClose")]
	static class ForbidLoadingModPreviewImages {
		static void Postfix() {
			DelayLoadingModPreviewImages.ForbidLoadingPreviewImages();
		}
	}
}
