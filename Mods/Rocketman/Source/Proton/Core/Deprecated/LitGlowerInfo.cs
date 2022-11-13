using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Proton
{
    public class LitGlowerInfo
    {

        private bool flooded = false;

        private bool changed = false;

        private float overlightRadius;

        private float glowRadius;

        private ColorInt glowColor;

        private Vector3 position;

        private bool floodNoCavePlants = false;

        private readonly List<LitCell> cells = new List<LitCell>();

        public CompGlower glower;

        public Vector3 Position
        {
            get => new Vector3(position.x, position.y, position.z);
        }

        public List<LitCell> AllGlowingCells
        {
            get => cells;
        }

        public List<LitCell> AllGlowingCellsNoCavePlants
        {
            get => FloodNoCavePlants ? cells : new List<LitCell>();
        }

        public bool FloodNoCavePlants
        {
            get => floodNoCavePlants;
        }

        public bool Flooded
        {
            get => flooded;
            set => flooded = value;
        }

        public bool Changed
        {
            get => changed
                || glowRadius != Props.glowRadius
                || glowColor != Props.glowColor
                || overlightRadius != Props.overlightRadius
                || position != glower.parent.TrueCenter().Yto0();
            set => changed = value;
        }

        public bool ShouldBeLitNow
        {
            get => !(glower.parent?.Destroyed ?? true) && (glower.parent?.Spawned ?? false) && glower.ShouldBeLitNow;
        }

        public CompProperties_Glower Props
        {
            get => glower.Props;
        }

        public LitGlowerInfo(CompGlower glower)
        {
            this.glower = glower;
            this.floodNoCavePlants = glower.parent.def.category != ThingCategory.Plant || !glower.parent.def.plant.cavePlant;
            this.Reset();
        }

        public bool Contains(Vector3 location)
        {
            return Vector3.Distance(location, position) - 5f < Props.glowRadius;
        }

        public void Reset()
        {
            glowRadius = Props.glowRadius;
            glowColor = Props.glowColor;
            overlightRadius = Props.overlightRadius;
            position = glower.parent.TrueCenter().Yto0();
            floodNoCavePlants = glower.parent.def.category != ThingCategory.Plant || !glower.parent.def.plant.cavePlant;
            changed = false;
            flooded = false;
            cells.Clear();
        }
    }
}
