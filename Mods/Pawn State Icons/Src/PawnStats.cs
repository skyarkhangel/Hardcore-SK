using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// by Dan Sadler (me@sadler.su)
namespace RimWorldIconMod
{
    class PawnStats
    {
        public float total_efficiency = 1.0f;
        public float tooCold = -1.0f;
        public float tooHot = -1.0f;
        public float bleedRate = -1.0f;
        public Vector3 targetPos = Vector3.zero;

        public bool isNudist = false;

        public float diseaseDisappearance = 1.0f;

        public float apparelHealth = 1.0f;
        public float drunkness = 0.0f;
              
    }
}
