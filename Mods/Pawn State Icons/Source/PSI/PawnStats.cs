using UnityEngine;

namespace PSI
{
    internal class PawnStats
    {
        public float TotalEfficiency = 1f;
        public float TooCold = -1f;
        public float TooHot = -1f;
        public float BleedRate = -1f;
        public Vector3 TargetPos = Vector3.zero;
        public float DiseaseDisappearance = 1f;
        public float ApparelHealth = 1f;
        public bool IsNudist;
        public float Drunkness;
    }
}
