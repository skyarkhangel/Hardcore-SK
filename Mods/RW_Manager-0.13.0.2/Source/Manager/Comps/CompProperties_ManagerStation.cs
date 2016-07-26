// Manager/CompProperties_ManagerStation.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:30

using System;
using Verse;

namespace FluffyManager
{
    public class CompProperties_ManagerStation : CompProperties
    {
        public int Speed;
        public CompProperties_ManagerStation() {}
        public CompProperties_ManagerStation( Type compClass ) : base( compClass ) {}
    }
}