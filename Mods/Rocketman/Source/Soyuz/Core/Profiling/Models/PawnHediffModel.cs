using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz.Profiling
{
    public class PawnHediffModel : IPawnModel
    {
        public PawnHediffModel(string name) : base(name)
        {
            this.grapher.TimeWindowSize = 5000;
        }
    }
}