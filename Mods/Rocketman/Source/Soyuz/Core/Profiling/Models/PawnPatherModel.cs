using System;
using Verse;

namespace Soyuz.Profiling
{
    public class PawnPatherModel : IPawnModel
    {
        public PawnPatherModel(string name) : base(name)
        {
            this.grapher.TimeWindowSize = 2500;
        }

        public override void AddResult(float value)
        {
            int tick = GenTicks.TicksGame;
            grapher.Add(tick, value);
            grapher.Add(tick + 1, 0);
        }
    }
}
