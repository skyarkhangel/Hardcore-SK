using System;
using System.ComponentModel;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    public class ProfileLog
    {
        public float percent;
        public string percentString;
        public double average;
        public string key;
        public string label;
        public string mod;
        public float max;
        public float total;
        public float calls;
        public float maxCalls;
        public Type type;
        public Def def;
        public MethodBase meth;

        public ProfileLog(string label, string percentString, double average, float max, Def def, string key, string mod, float percent, float total, float calls, float maxCalls, Type type, MethodBase meth = null)
        {
            this.label = label;
            this.percentString = percentString;
            this.average = average;
            this.def = def;
            this.key = key;
            this.max = max;
            this.mod = mod;
            this.percent = percent;
            this.type = type;
            this.meth = meth;
            this.total = total;
            this.calls = calls;
            this.maxCalls = maxCalls;
        }
    }
}