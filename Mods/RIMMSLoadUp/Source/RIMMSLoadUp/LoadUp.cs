/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 18.03.2019
 * Time: 11:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;
using Harmony;
using Verse;

namespace RIMMSLoadUp
{
	/// <summary>
	/// Description of LoadUp.
	/// </summary>
	public class LoadUp : Mod
	{
		public LoadUp(ModContentPack content) : base(content) {
			HarmonyInstance harmony = HarmonyInstance.Create("RIMMSLoadUp");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
