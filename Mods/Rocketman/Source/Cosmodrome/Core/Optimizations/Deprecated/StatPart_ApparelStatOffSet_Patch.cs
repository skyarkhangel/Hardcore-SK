using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{
     [RocketPatch(typeof(StatPart_ApparelStatOffset), nameof(StatPart_ApparelStatOffset.TransformValue))]
    public static class StatPart_ApparelStatOffSet_Skipper_Patch
    {
        private static float curValue;

        private static int curKey = -1;

        private static bool skip;

        private static readonly CachedDict<int, Pair<float, int>> cache = new CachedDict<int, Pair<float, int>>();

        public static bool Prefix(StatPart_ApparelStatOffset __instance, StatRequest req,
            ref float val)
        {
            if (RocketPrefs.Enabled && RocketPrefs.StatGearCachingEnabled
                && req.HasThing
                && req.thingInt is Pawn pawn
                && RocketStates.Context == ContextFlag.Ticking
                && !IgnoreMeDatabase.ShouldIgnore(__instance.apparelStat)
                && Finder.SessionTicks >= 600)
            {
                var stat = __instance.apparelStat ?? __instance.parentStat;
                int key;
                unchecked
                {
                    key = HashUtility.HashOne(__instance.includeWeapon ? 1 : 0);
                    key = HashUtility.HashOne(stat.shortHash, key);
                    key = HashUtility.HashOne(Tools.GetKey(req), key);
                }

                curKey = key;
                curValue = val;
                if (cache.TryGetValue(key, out var store, 2500))
                {
                    if (store.second == pawn.GetSignature())
                    {
                        val += store.first * (__instance.subtract ? -1 : 1);
                        skip = true;
                        return false;
                    }
                    cache.Remove(key);
                }

                skip = false;
                return true;
            }
            skip = true;
            return true;
        }

        public static void Postfix(StatPart_ApparelStatOffset __instance, StatRequest req,
            ref float val)
        {
            if (!skip)
            {
                cache[curKey] = new Pair<float, int>(Mathf.Abs(val - curValue), (req.thingInt as Pawn).GetSignature());
                skip = true;
            }
        }

        public static void Dirty(Pawn pawn)
        {
            pawn.GetSignature(true);
        }
    }
}