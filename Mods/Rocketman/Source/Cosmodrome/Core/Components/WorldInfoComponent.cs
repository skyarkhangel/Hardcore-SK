using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;
namespace RocketMan
{
    public class WorldInfoComponent : WorldComponent
    {
        private int initialMapHeight;

        private int initialMapWidth;

        public bool useCustomMapSizes = false;

        public Vector3 IntialMapSize
        {
            get
            {
                Vector3 vector = new Vector3();
                vector.y = world.info.initialMapSize.y;
                if (!useCustomMapSizes)
                {
                    vector.x = world.info.initialMapSize.x;
                    vector.z = world.info.initialMapSize.z;
                }
                else
                {
                    vector.x = initialMapWidth;
                    vector.z = initialMapHeight;
                }
                return vector;
            }
        }

        public int InitialMapHeight
        {
            get => !useCustomMapSizes ? initialMapHeight = Find.World.info.initialMapSize.z : initialMapHeight;
            set => initialMapHeight = value;
        }

        public int InitialMapWidth
        {
            get => !useCustomMapSizes ? initialMapWidth = Find.World.info.initialMapSize.x : initialMapWidth;
            set => initialMapWidth = value;
        }

        public WorldInfoComponent(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useCustomMapSizes, "useCustomMapSizes", false);
            Scribe_Values.Look(ref initialMapWidth, "initialMapWidth", 250);
            Scribe_Values.Look(ref initialMapHeight, "initialMapHeight", 250);
        }
    }
}
