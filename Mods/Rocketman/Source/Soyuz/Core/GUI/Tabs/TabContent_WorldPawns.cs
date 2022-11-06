using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
//using Mono.Security.X509.Extensions;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_WorldPawns : ITabContent
    {
        public enum WorldPawnState
        {
            Alive = 0,

            Dead = 1,

            Mothballed = 2
        }

        public struct WorldPawnRecord
        {
            public Pawn pawn;

            public WorldPawnState state;
        }

        private Pawn currentPawn;

        private Vector2 scrollPosition = Vector2.zero;

        private readonly List<WorldPawnRecord> records = new List<WorldPawnRecord>();

        private readonly Listing_Collapsible collapsible_statistic = new Listing_Collapsible();

        public override Texture2D Icon => TexTab.World;

        public override bool ShouldShow => RocketPrefs.Enabled && RocketDebugPrefs.Debug;

        public override string Label => "World Pawns";

        public TabContent_WorldPawns()
        {
            Refresh();
        }

        public override void DoContent(Rect inRect)
        {
            collapsible_statistic.Begin(inRect, "World Pawns Statistic");
            collapsible_statistic.Label("General information about world pawns.");
            collapsible_statistic.Gap(5);
            collapsible_statistic.Label($"<color=green>Alive</color> world pawns count: <color=orange>{Find.WorldPawns.pawnsAlive.Count}</color>");
            collapsible_statistic.Label($"<color=red>Dead</color> world pawns count: <color=orange>{Find.WorldPawns.pawnsDead.Count}</color>");
            collapsible_statistic.Label($"<color=yellow>Suspended</color> world pawns count: <color=orange>{Find.WorldPawns.pawnsMothballed.Count}</color>");
            collapsible_statistic.End(ref inRect);
            inRect.yMin += 5;
            bool prev = WorldPawnsTicker.isActive;
            WorldPawnsTicker.isActive = true;
            if (currentPawn != null && currentPawn.Destroyed)
            {
                currentPawn = null;
            }
            if (currentPawn != null)
            {
                var model = ContextualExtensions.GetPerformanceModel(currentPawn);
                model.DrawGraph(ref inRect);
            }
            RocketMan.GUIUtility.ScrollView(inRect, ref scrollPosition, records,
                heightLambda: (record) =>
                {
                    return (record.pawn == null || record.pawn.Destroyed || record.pawn.Spawned) ? 0f : 25f;
                },
                elementLambda: (elementRect, record) =>
                {
                    var sidebarColor = Color.gray;
                    var state = string.Empty;
                    switch (record.state)
                    {
                        case WorldPawnState.Alive:
                            state = "<color=green>Alive</color>";
                            sidebarColor = Color.green;
                            break;
                        case WorldPawnState.Dead:
                            state = "<color=red>Dead</color>";
                            sidebarColor = Color.red;
                            break;
                        case WorldPawnState.Mothballed:
                            state = "<color=yellow>Suspended</color>";
                            sidebarColor = Color.yellow;
                            break;
                    }
                    // return RocketPrefs.TimeDilationWorldPawns && !pawn.IsCaravanMember() && pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer && !HasHediffPreventingThrottling(pawn);
                    Widgets.DrawBoxSolid(elementRect.LeftPartPixels(3), sidebarColor);
                    elementRect.xMin += 5;
                    RocketMan.GUIUtility.Row(elementRect, new List<Action<Rect>>()
                    {
                        (rect) =>
                        {
                            Widgets.Label(elementRect, $"{record.pawn}");
                        },
                        (rect) =>
                        {
                            Widgets.Label(rect, state);
                        },
                        (rect) =>
                        {                            
                            if(record.state == WorldPawnState.Alive)
                            {
                                Widgets.Label(rect, record.pawn.IsCaravanMember() ? "Caravaning" : " ");
                            }                            
                        },
                        (rect) =>
                        {
                            if(record.state == WorldPawnState.Alive)
                            {
                                Widgets.Label(rect, record.pawn.IsColonist ? "<color=green>Colonist</color>" : "");
                            }                           
                        },
                        (rect) =>
                        {
                            if (record.state == WorldPawnState.Alive)
                            {
                                HediffDef hediff = Find.WorldPawns.DefPreventingMothball(record.pawn);
                                if(hediff != null)
                                {
                                    Widgets.Label(rect, $"nm={hediff.label},ia={hediff.IsAddiction},cls={hediff.hediffClass}");
                                }
                            }
                        },
                        (rect) =>
                        {
                            if(record.state == WorldPawnState.Alive)
                            {
                                Widgets.Label(rect, (record.pawn.Faction?.name ?? "") + "/"+ record.pawn.HostFaction?.name ?? "");
                            }
                        }
                    }, drawDivider: false);
                    if (Widgets.ButtonInvisible(elementRect))
                    {
                        currentPawn = record.pawn;
                    } 
                }
            );
            WorldPawnsTicker.isActive = prev;
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            Refresh();

            base.OnSelect();
        }

        private void Refresh()
        {
            records.Clear();
            records.AddRange(
                Find.WorldPawns.pawnsAlive.Select(p => new WorldPawnRecord() { pawn = p, state = WorldPawnState.Alive })
                );
            records.AddRange(
                Find.WorldPawns.pawnsDead.Select(p => new WorldPawnRecord() { pawn = p, state = WorldPawnState.Dead })
                );
            records.AddRange(
                Find.WorldPawns.pawnsMothballed.Select(p => new WorldPawnRecord() { pawn = p, state = WorldPawnState.Mothballed })
                );
            records.RemoveAll(r => r.pawn == null || r.pawn.Destroyed);
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_WorldPawns();
    }
}
