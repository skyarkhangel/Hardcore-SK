using System;
using System.Collections.Generic;
//using System.Security.Cryptography.Xml;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz
{
    public abstract class IPawnModel
    {
        public Grapher grapher;

        public List<Tuple<float, float, bool>> queue = new List<Tuple<float, float, bool>>();

        public IPawnModel(string name)
        {
            this.grapher = new Grapher(name.CapitalizeFirst());
        }

        public virtual void AddResult(float value)
        {
            queue.Add(new Tuple<float, float, bool>(GenTicks.TicksGame, value, RocketPrefs.TimeDilation));
        }

        public void DrawGraph(ref Rect rect)
        {
            foreach (Tuple<float, float, bool> p in queue)
            {
                grapher.Add(p.Item1, p.Item2, p.Item3 ? Color.cyan : Color.yellow);
            }
            grapher.Plot(ref rect);
            queue.Clear();
        }
    }
}
