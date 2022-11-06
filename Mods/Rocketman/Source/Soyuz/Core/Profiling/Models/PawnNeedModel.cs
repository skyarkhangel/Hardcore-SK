using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz.Profiling
{
    public class PawnNeedModel : IPawnModel
    {
        public PawnNeedModel(string name) : base(name)
        {
            this.grapher.TimeWindowSize = 18000;
        }
    }
}