using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using HarmonyLib;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;

namespace Soyuz
{
    public enum JobThrottleFilter
    {
        Animals = 1,

        Humanlikes = 2,

        All = 3
    }
}