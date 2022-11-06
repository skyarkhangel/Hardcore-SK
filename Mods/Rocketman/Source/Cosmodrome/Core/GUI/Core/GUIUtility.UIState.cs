using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RocketMan
{
    public static partial class GUIUtility
    {
        [StructLayout(LayoutKind.Auto)]
        private struct GUIState
        {
            public GameFont font;
            public FontStyle curStyle;
            public FontStyle curTextAreaReadOnlyStyle;
            public FontStyle curTextAreaStyle;
            public FontStyle curTextFieldStyle;
            public TextAnchor anchor;
            public Color color;
            public Color contentColor;
            public Color backgroundColor;
            public bool wordWrap;
            public bool useCustomFonts;
        }

        private readonly static List<GUIState> stack = new List<GUIState>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StashGUIState()
        {
            stack.Add(new GUIState()
            {
                font = Text.Font,
                curStyle = Text.CurFontStyle.fontStyle,
                curTextAreaReadOnlyStyle = Text.CurTextAreaReadOnlyStyle.fontStyle,
                curTextAreaStyle = Text.CurTextAreaStyle.fontStyle,
                curTextFieldStyle = Text.CurTextFieldStyle.fontStyle,
                anchor = Text.Anchor,
                color = GUI.color,
                wordWrap = Text.WordWrap,
                contentColor = GUI.contentColor,
                backgroundColor = GUI.backgroundColor,
                useCustomFonts = GUIFont.UseCustomFonts
            });
            if (stack.Count == 1)
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;
            }
            GUIFont.UseCustomFonts = true;
            if (stack.Count == 1)
            {
                GUIFont.Font = GUIFontSize.Tiny;
                GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
                GUIFont.Anchor = TextAnchor.UpperLeft;
                GUIFont.WordWrap = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RestoreGUIState()
        {
            GUIState config = stack.Last();
            stack.RemoveLast();
            Restore(config);
            // TODO
            // FIX THIS SHIT!
            GUIFont.RestorStyles();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearGUIState()
        {
            if (stack.Count > 0)
            {
                Log.Warning("ROCKETMAN: GUI state should be clear at exit");
                Restore(stack[0]);
            }
            stack.Clear();
            GUIFont.RestorStyles();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Restore(GUIState config)
        {
            GUIFont.UseCustomFonts = config.useCustomFonts;
            Text.Font = config.font;
            GUI.color = config.color;
            GUI.contentColor = config.contentColor;
            GUI.backgroundColor = config.backgroundColor;
            Text.WordWrap = config.wordWrap;
            Text.Anchor = config.anchor;
            Text.CurFontStyle.fontStyle = config.curStyle;
            Text.CurTextAreaReadOnlyStyle.fontStyle = config.curTextAreaReadOnlyStyle;
            Text.CurTextAreaStyle.fontStyle = config.curTextAreaStyle;
            Text.CurTextFieldStyle.fontStyle = config.curTextFieldStyle;
        }
    }
}
