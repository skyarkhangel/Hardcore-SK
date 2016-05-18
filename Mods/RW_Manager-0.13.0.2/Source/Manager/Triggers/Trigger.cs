// Manager/Trigger.cs
//
// Copyright Karel Kroeze, 2015.
//
// Created 2015-11-04 19:28

using UnityEngine;
using Verse;
using static System.String;

namespace FluffyManager
{
    public abstract class Trigger : IExposable
    {
        #region Properties

        public abstract bool State { get; }
        public virtual string StatusTooltip { get; } = Empty;

        #endregion Properties

        #region Methods

        public virtual void DrawProgressBar( Rect progressRect, bool active )
        {
        }

        public abstract void DrawTriggerConfig( ref Vector2 cur, float width, float entryHeight, bool alt = false, string label = null, string tooltip = null );

        public abstract void ExposeData();

        #endregion Methods
    }
}