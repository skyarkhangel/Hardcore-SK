using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infused
{
    public class InfusedMod : Mod
    {
        public InfusedMod(ModContentPack content) : base(content)
        {
            new Harmony("rimworld.infused").PatchAll(Assembly.GetExecutingAssembly());

            LongEventHandler.ExecuteWhenFinished(Inject);

            GetSettings<Settings>();
        }

        public override string SettingsCategory() { return ResourceBank.Strings.Infused; }
        public override void DoSettingsWindowContents(Rect inRect) => Settings.DoSettingsWindowContents(inRect);

        static void Inject() {
            var defs = DefDatabase<ThingDef>.AllDefs.Where(InjectPredicate).ToList();

            var tabType = typeof(ITab_Infused);
            var tab = InspectTabManager.GetSharedInstance(tabType);
            var compProperties = new Verse.CompProperties { compClass = typeof(CompInfused) };

            foreach (var def in defs)
            {
                def.comps.Add(compProperties);
                #if DEBUG
                Log.Message("Infused :: Component added to " + def.label);
                #endif

                if (def.inspectorTabs == null || def.inspectorTabs.Count == 0)
                {
                    def.inspectorTabs = new List<Type>();
                    def.inspectorTabsResolved = new List<InspectTabBase>();
                }

                def.inspectorTabs.Add(tabType);
                def.inspectorTabsResolved.Add(tab);
            }
            #if DEBUG
            Log.Message("Infused :: Injected " + defs.Count + "/" + DefDatabase<ThingDef>.AllDefs.Count());
            #endif
        }

        static bool InjectPredicate(ThingDef def)
        {
            if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }
            if (def.HasComp(typeof(CompInfused)))
            {
                return false;
            }
            if (def.Verbs.Any(v => typeof(Verb_ShootOneUse).IsAssignableFrom(v.GetType())))
            {
                return false;
            }
            return true;
        }

        public static IEnumerable<Def> Infuse(Thing thing, QualityCategory q, int min = 0, int max = 1, bool skipThingFilter = false)
        {
            // pick the first chancedef available
            var chanceDef = DefDatabase<ChanceDef>.AllDefs.FirstOrDefault(c => c.filter?.Allows(thing) ?? false);

            if (chanceDef == null)
            {
                yield break;
            }

            #if DEBUG
            Log.Message($" > Chance to roll from {chanceDef.defName} for {q} {thing.def.label}");
            #endif

            IEnumerator<float> roller = RollChance(chanceDef, thing, q);
            IEnumerator<Def> available = null;

            for (int roll = 0; roll < max; roll++)
            {
                // ensure min slots, then next ones are up to the first time the roller says false or max rolls
                if (roll >= min && !roller.MoveNext())
                {
                    yield break;
                }

                // if we got here, we need to satisfy the request at least once, initialize available Infusion Defs
                if (available == null)
                {
                    available = RollInfusion(chanceDef, thing, q, skipThingFilter);
                }

                // Give an Infusion or stop the moment we don't have any to give
                if (available.MoveNext())
                {
                    #if DEBUG
                    Log.Message($" > Gave {available.Current.tier} {available.Current} to {q} {thing.def.label} with a {roller.Current} chance");
                    #endif

                    yield return available.Current;
                }
                else
                {
                    yield break;
                }
            }
        }

        static IEnumerator<float> RollChance(ChanceDef chanceDef, Thing thing, QualityCategory q)
        {
            foreach (var slotChance in chanceDef.slots)
            {
                float chance = chanceDef.Chance(q) * slotChance;
                bool roll = Rand.Chance(chance);
                #if DEBUG
                Log.Message($"   > Rolled {roll} with {chance}");
                #endif
                if (roll)
                {
                    yield return chance;
                }
                else
                {
                    break;
                }
            }

            if (thing.def.apparel != null && chanceDef.slotBonusPerParts > 0)
            {
                int bonus = thing.def.apparel.bodyPartGroups.Count % chanceDef.slotBonusPerParts;

                float firstSlotChance = chanceDef.slots.FirstOrFallback(1f);

                #if DEBUG
                Log.Message($"   + Bonus {bonus}");
                #endif

                for (int i = 1; i <= bonus; i++)
                {
                    float chance = chanceDef.Chance(q) * firstSlotChance * 0.8f / i;

                    if (Rand.Chance(chance))
                    {
                        yield return chance;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        static IEnumerator<Def> RollInfusion(ChanceDef chanceDef, Thing thing, QualityCategory q, bool skipThingFilter) {
            IEnumerable<Def> available = DefDatabase<Def>.AllDefs;

            bool squash = false;

            if (chanceDef.allowTags != null)
            {
                available = available.Where(i => chanceDef.allowTags.Intersect(i.tags).Any());

                squash = true;
            }

            if (!skipThingFilter)
            {
                available = available.Where(i => i.filter?.Allows(thing) ?? true);

                squash = true;
            }

            if (squash)
            {
                available = available.ToList();
            }

            while (true)
            {
                var tier = RollTier(q);

                // select Infused Defs
                List<Def> selected;
                do
                {
                    selected = available.Where(i => i.tier == tier).ToList();

                    #if DEBUG
                    if (selected.Count == 0)
                    {
                        Log.Message($"   > No available infusions in {tier} tier for {thing.def.label}");
                    }
                    #endif

                    tier--;
                } while (selected.Count == 0 && tier >= 0);


                if (selected.Count == 0)
                {
                    #if DEBUG
                    Log.Warning($"   > No available infusions for {thing.def.label}");
                    #endif

                    yield break;
                }

                yield return selected.RandomElementByWeight(i => i.weight);
            }
        }

        static InfusionTier RollTier(QualityCategory q)
        {
            float roll = Mathf.Pow(Rand.Value, 1 + (float) q * Settings.bias);
            #if DEBUG
            Log.Message($"   > tier chance is {roll}");
            #endif
            if (roll < Settings.weight_artifact)
            {
                return InfusionTier.Artifact;
            }
            if (roll < Settings.weight_legendary)
            {
                return InfusionTier.Legendary;
            }
            if (roll < Settings.weight_epic)
            {
                return InfusionTier.Epic;
            }
            if (roll < Settings.weight_rare)
            {
                return InfusionTier.Rare;
            }
            if (roll < Settings.weight_uncommon)
            {
                return InfusionTier.Uncommon;
            }
            return InfusionTier.Common;
        }

        internal static void ModifyDamageFor(Thing thing, DamageInfo dinfo, DamageWorker.DamageResult damageResult)
        {
            {
                if (!(dinfo.Instigator is Pawn instigator))
                {
                    return;
                }

                if (!instigator.equipment?.HasAnything() ?? false)
                {
                    return;
                }

                if (damageResult.totalDamageDealt <= 0)
                {
                    return;
                }
            }

            foreach (var onHitDef in DefDatabase<OnHitDef>.AllDefs)
            {
                if (thing.Destroyed)
                {
                    return;
                }

                float amount = dinfo.Instigator.GetStatValue(onHitDef.amount);
                float chance = dinfo.Instigator.GetStatValue(onHitDef.chance);

                if (amount > 0f && chance <= 0f)
                {
                    chance = 0.1f;
                }

                if (amount > 0f && chance > 0f && Rand.Chance(chance))
                {
                    DamageInfo extra = new DamageInfo(
                        onHitDef.damage,
                        amount,
                        dinfo.ArmorPenetrationInt,
                        dinfo.Angle,
                        dinfo.Instigator,
                        dinfo.HitPart,
                        dinfo.Weapon,
                        dinfo.Category,
                        dinfo.IntendedTarget);

                    thing.PreApplyDamage(ref extra, out bool absorbed);
                    if (absorbed)
                    {
                        continue;
                    }

                    DamageWorker.DamageResult result = onHitDef.damage.Worker.Apply(extra, thing);

                    if (result.totalDamageDealt <= 0)
                    {
                        continue;
                    }

                    thing.PostApplyDamage(extra, result.totalDamageDealt);

                    damageResult.wounded                |= result.wounded;
                    damageResult.headshot               |= result.headshot;
                    damageResult.deflected              |= result.deflected;
                    damageResult.stunned                |= result.stunned;
                    damageResult.deflectedByMetalArmor  |= result.deflectedByMetalArmor;
                    damageResult.diminished             |= result.diminished;
                    damageResult.diminishedByMetalArmor |= result.diminishedByMetalArmor;

                    if (result.parts != null)
                    {
                        foreach (var part in result.parts)
                        {
                            damageResult.AddPart(damageResult.hitThing, part);
                        }
                    }

                    if (result.hediffs != null)
                    {
                        foreach (var hediff in result.hediffs)
                        {
                            damageResult.AddHediff(hediff);
                        }
                    }

                    if (onHitDef.damage.harmsHealth)
                    {
                        damageResult.totalDamageDealt += result.totalDamageDealt;
                    }

                }
            }
        }
    }

    [HarmonyPatch(typeof(CompQuality), nameof(CompQuality.SetQuality))]
    static class CompQuality_SetQuality_Patch
    {
        static void Postfix(CompQuality __instance, QualityCategory q)
        {
            var thing = __instance.parent;

            // Can we be infused?
            var infusions = InfusedMod.Infuse(thing, q, max: Settings.max).ToList();
            if (infusions.Count > 0)
            {
                thing.TryGetComp<CompInfused>()?.SetInfusions(infusions);

                __instance.parent.HitPoints = __instance.parent.MaxHitPoints;
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    static class DamageResult_Ctor_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> e)
        {
            MethodInfo method = AccessTools.Method(typeof(InfusedMod), nameof(InfusedMod.ModifyDamageFor));
            MethodInfo target = AccessTools.Method(typeof(DamageWorker), nameof(DamageWorker.Apply));

            var instructionList = e.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                var inst = instructionList[i];

                yield return inst;

                if (inst.Calls(target))
                {
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, method);

                    i++;
                }
            }
        }
    }


}
