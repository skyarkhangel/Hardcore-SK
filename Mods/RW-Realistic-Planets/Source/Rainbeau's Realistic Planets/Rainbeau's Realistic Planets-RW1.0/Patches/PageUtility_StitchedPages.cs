using Harmony;
using RimWorld;

namespace Planets_Code
{
	[HarmonyPatch(typeof(PageUtility), "StitchedPages", null)]
	public static class PageUtility_StitchedPages {
		[HarmonyPriority(Priority.High)]
		public static void Postfix(ref Page __result) {
			if (__result == null) {
				return;
			}
			if (TutorSystem.TutorialMode) {
				return;
			}
			Page _Result = __result;
			while (true) {
				Page page = _Result.next;
				if (page == null) {
					break;
				}
				if (!(page is Page_CreateWorldParams)) {
					_Result = page;
				}
				else {
					Page page1 = page.next;
					Page page2 = page.prev;
					Planets_CreateWorldParams createWorldParam = new Planets_CreateWorldParams();
					page2.next = createWorldParam;
					page1.prev = createWorldParam;
					createWorldParam.prev = page2;
					createWorldParam.next = page1;
					break;
				}
			}
		}
	}	
	
}
