/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 18.03.2019
 * Time: 10:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Harmony;

namespace RIMMSLoadUp
{
	[HarmonyPatch(typeof(Verse.GenTypes))]
	[HarmonyPatch("GetTypeInAnyAssembly")]
	static class GetTypeInAnyAssemblyPatch {
		static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		
		static bool Prefix(string typeName, ref Type __result, ref bool __state) {
			if ( typeCache.TryGetValue(typeName, out __result) ) {
				return __state = false;
			}
			
			return __state = true;
		}
	
		static void Postfix(string typeName, ref Type __result, bool __state) {
			if ( __state ) typeCache[typeName] = __result;
		}
		
		static public void Clear() {
			if ( typeCache != null ) typeCache.Clear();
		}
	}
}
