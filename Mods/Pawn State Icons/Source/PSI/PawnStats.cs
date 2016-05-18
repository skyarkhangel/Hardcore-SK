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

        public float Drunkness = 0f;

        public bool HasBed = false;

        public bool IsSick = false;

        public int CrowdedMoodLevel = 0;

        public int PainMoodLevel = 0;

        public float ToxicBuildUp = 0;
    }
}
