using System;
using UnityEngine;
using Verse;
using RimWorld;
namespace SK_DefenceShield
{
    internal class Vectors
    {
        public static double EuclDist(IntVec3 a, IntVec3 b)
        {
            return Math.Sqrt((double)((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z)));
        }
        public static double VectorSize(IntVec3 a)
        {
            return Math.Sqrt((double)(a.x * a.x + a.y * a.y + a.z * a.z));
        }
        public static IntVec3 vecFromAngle(float angle1, float angle2, float r)
        {
            IntVec3 result = new IntVec3((int)((double)r * Math.Sin((double)angle1) * Math.Cos((double)angle2)), (int)((double)r * Math.Sin((double)angle1) * Math.Sin((double)angle2)), (int)((double)r * Math.Cos((double)angle1)));
            return result;
        }
        public static double vectorAngleA(IntVec3 a)
        {
            double num = Vectors.VectorSize(a);
            return Math.Acos((double)a.z / num);
        }
        public static IntVec3 randomDirection(float r)
        {
            return Vectors.vecFromAngle((float)UnityEngine.Random.Range(0, 360), 0f, r);
        }
        public static Vector3 IntVecToVec(IntVec3 from)
        {
            return new Vector3((float)from.x, (float)from.y, (float)from.z);
        }
        public static IntVec3 VecToIntVec(Vector3 from)
        {
            return new IntVec3((int)from.x, (int)from.y, (int)from.z);
        }
    }
}
