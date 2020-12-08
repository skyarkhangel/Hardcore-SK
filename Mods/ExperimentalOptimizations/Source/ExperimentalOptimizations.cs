using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ExperimentalOptimizations
{
    public enum InitStage
    {
        StaticConstructorOnStartup,
        ModInit
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FixOn : Attribute
    {
        public InitStage stage;
        public FixOn(InitStage stage) => this.stage = stage;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Optimization : Attribute
    {
        public string name;
        public Type optimizationSetting;
        public Optimization(string name, Type optimizationSetting) => (this.name, this.optimizationSetting) = (name, optimizationSetting);
        
        // optimizationSetting implement:
        //   public static bool Enabled();
        //   public static void DoSettingsWindowContents(Listing_Standard l);
        //   public static void ExposeData()
    }

    [StaticConstructorOnStartup]
    public class ExperimentalOptimizationsMod
    {
        static ExperimentalOptimizationsMod()
        {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();

            // apply fixes on StaticConstructorOnStartup
            var fixesOnStaticConstructor = allTypes.Where(t => t.TryGetAttribute(out FixOn fix) && fix.stage == InitStage.StaticConstructorOnStartup);
            foreach (var t in fixesOnStaticConstructor)
            {
                t.InvokeStaticMethod("Patch");
            }

            if (ExperimentalOptimizations.Optimizations == null)
            {
                // init optimizations
                ExperimentalOptimizations.Optimizations = allTypes.Where(t => t.TryGetAttribute<Optimization>(out _)).ToArray();
            }

            // ModInit or StaticConstructorOnStartup faster?
            if (ExperimentalOptimizations.Instance == null)
            {
                Log.Error($"Can't init settings!");
            }

            foreach (var optimization in ExperimentalOptimizations.Optimizations)
            {
                var opt = optimization.TryGetAttribute<Optimization>();
                // initialize
                optimization.InvokeStaticMethod("Init");
                // patch enabled in settings opts.
                if (opt.optimizationSetting.InvokeStaticMethod<bool>("Enabled"))
                {
                    optimization.InvokeStaticMethod("Patch");
                }
            }

            if (Settings.CheckModCompatible)
                CheckModCompatible();
        }

        static void CheckModCompatible()
        {
            var supportedTypes = new[]
            {
                // Needs
                "RimWorld.Need_Authority",
                "RimWorld.Need_Beauty",
                "RimWorld.Need_Chemical",
                "RimWorld.Need_Chemical_Any",
                "RimWorld.Need_Comfort",
                "RimWorld.Need_Food",
                "RimWorld.Need_Joy",
                "RimWorld.Need_Mood",
                "RimWorld.Need_Outdoors",
                "RimWorld.Need_Rest",
                "RimWorld.Need_RoomSize",
                "RimWorld.Need_Seeker",
                "Skynet.Need_Energy",
                "Androids.Need_Energy",
                "DubsBadHygiene.Need_Bladder",
                "DubsBadHygiene.Need_Hygiene",
                "DubsBadHygiene.Need_Thirst",
                "rjw.Need_Sex",
                // Hediffs
                "Verse.Hediff_Alcohol",
                "Verse.Hediff_ImplantWithLevel",
                "Verse.Hediff_Injury",
                "Verse.Hediff_MissingPart",
                "Verse.Hediff_Pregnant",
                "Verse.HediffWithComps",
                "RimWorld.Hediff_HeartAttack",
                "SK.Hediff_FatalRad",
                "SK.Hediff_DeathRattle",
                "SK.Hediff_Senexium",
                "Androids.AndroidLikeHediff",
                "Androids.Hediff_LoverMentality",
                "Androids.Hediff_MechaniteHive",
                "Androids.Hediff_VanometricCell",
                "Rimatomics.Hediff_FatalRad",
                "SK.ShieldHediff",
                "CONN.Hediff_FlashLightRed",
                "CONN.Hediff_FlashLightGreen",
                "CONN.Hediff_FlashLight",
                "RimWorld.HediffPsychicConversion", // PsychicAwakening
                "Adrenaline.Hediff_AdrenalineRush",
                "Adrenaline.Hediff_Adrenaline",
                "rjw.Cocoon",
                "rjw.Hediff_MicroComputer",
                "rjw.Hediff_PartBaseArtifical",
                "rjw.Hediff_InsectEgg",
                "rjw.Hediff_Bukkake",
                "rjw.Hediff_SimpleBaby",
                "rjw.Hediff_PartBaseNatural",
                "rjw.Hediff_BasePregnancy",
                // HediffComps
                "Verse.HediffComp_CauseMentalState",
                "Verse.HediffComp_ChanceToRemove",
                "Verse.HediffComp_ChangeImplantLevel",
                "Verse.HediffComp_ChangeNeed",
                "Verse.HediffComp_DamageBrain",
                "Verse.HediffComp_Disappears",
                "Verse.HediffComp_Discoverable",
                "Verse.HediffComp_Disorientation",
                "Verse.HediffComp_GrowthMode",
                "Verse.HediffComp_HealPermanentWounds",
                "Verse.HediffComp_Infecter",
                "Verse.HediffComp_KillAfterDays",
                "Verse.HediffComp_Link",
                "Verse.HediffComp_SelfHeal",
                "Verse.HediffComp_SeverityFromEntropy",
                "Verse.HediffComp_SkillDecay",
                "Verse.HediffComp_TendDuration",
                "Verse.HediffComp_VerbGiver",
                "Verse.HediffComp_SeverityPerDay",
                "RimWorld.HediffComp_PsychicHarmonizer",
                "CombatExtended.HediffComp_Prometheum",
                "CombatExtended.HediffComp_Venom",
                "CombatExtended.HediffComp_InfecterCE",
                "CombatExtended.HediffComp_Stabilize",
                "SK.HeddifComp_StandOff",
                "SK.HeddifComp_MightJoin",
                "SK.HeddifComp_Traitor",
                "rjw.HediffComp_FeelingBrokenSeverityReduce",
            };

            var allTypes = GenTypes.AllTypes.ToList();

            var hediffs = allTypes
                .Where(t => t.IsSubclassOf(typeof(Hediff)))
                .Where(t => TypeHasDeclaredMethod(t, "PostTick") || TypeHasDeclaredMethod(t, "Tick"))
                .ToList();
            var hediffComps = allTypes
                .Where(t => t.IsSubclassOf(typeof(HediffComp)))
                .Where(t => TypeHasDeclaredMethod(t, "CompPostTick"))
                .ToList();
            var needs = allTypes
                .Where(t => t.IsSubclassOf(typeof(Need)))
                .ToList();

            var unsupportedTypes = new StringBuilder();
            var all = hediffs.Concat(hediffComps).Concat(needs);
            foreach (var type in all)
            {
                if (!supportedTypes.Contains(type.FullName))
                {
                    unsupportedTypes.Append($"{type.FullName}, ");
                }
            }

            if (unsupportedTypes.Length > 0)
            {
                Log.Error($"=================WARNING=================");
                Log.Error($"THIS TYPES MAY WORK INCORRECT WITH MOD EXPERIMENTAL OPTIMIZATIONS:");
                Log.Error(unsupportedTypes.ToString());
                Log.Error($"=========================================");
            }

            // local functions
            bool TypeHasDeclaredMethod(Type t, string methodName) => t
                .GetMethods(AccessTools.all).Any(m => m.IsDeclaredMember() && m.Name.Equals(methodName));
        }
    }

    public class ExperimentalOptimizations : Mod
    {
        public static Type[] Optimizations { get; set; }

        public override void DoSettingsWindowContents(Rect inRect) => Settings.DoSettingsWindowContents(inRect);
        public override string SettingsCategory() => "ExperimentalOptimizations";

        public static ExperimentalOptimizations Instance;

        public ExperimentalOptimizations(ModContentPack content) : base(content)
        {
            Instance = this;

            // apply fixes on ModInit
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            var fixesOnModInit = allTypes.Where(t => t.TryGetAttribute(out FixOn fix) && fix.stage == InitStage.ModInit);
            foreach (var t in fixesOnModInit)
            {
                t.InvokeStaticMethod("Patch");
            }

            if (ExperimentalOptimizations.Optimizations == null)
            {
                // init optimizations
                ExperimentalOptimizations.Optimizations = allTypes.Where(t => t.TryGetAttribute<Optimization>(out _)).ToArray();
            }

            GetSettings<Settings>();
            
            Log.Message($"[ExperimentalOptimizations] initialized");
        }
    }
}
