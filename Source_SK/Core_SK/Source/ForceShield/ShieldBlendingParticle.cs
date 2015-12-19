using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Enhanced_Development.Shields
{
    class ShieldBlendingParticle
    {
        private static readonly Material ShieldSparksMat = MaterialPool.MatFrom("Things/ShieldSparks", MatBases.LightOverlay);

        //Current rotation of the sprite
        private float currentAngle = UnityEngine.Random.Range(0f, 360f);

        //Current step of the transition
        private int transitionStatus = 0;
        //Whether fading in or out
        private int transitionDirection = 1;
        //Maximum step of transition
        public const int transitionMax = 80;
        //Size of one transition step
        private int transitionStep = UnityEngine.Random.Range(1, 1);

        private Vector3 drawPosition;

        public int currentStep {
            get
            {
                return transitionStatus;
            }
        }
        public int currentDir
        {
            get
            {
                return transitionDirection;
            }
        }

        public ShieldBlendingParticle(Vector3 pos)
        {
            drawPosition = pos;
        }
        public ShieldBlendingParticle(Vector3 pos, int step)
        {
            drawPosition = pos;
            //Log.Message("Creating particle with starting step at " + step);
            //I'm carefully limiting it between 0 and maximum
            transitionStatus = Math.Max(Math.Min(transitionMax, step), 0);
        }

        public void DrawMe()
        {
            DrawMe(drawPosition);
        }
        public void DrawMe(Vector3 location)
        {
            doTransitionStep();

            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(location + Altitudes.AltIncVect, Quaternion.Euler(0f, currentAngle, 0f), UnityEngine.Vector3.one);
            UnityEngine.Graphics.DrawMesh(MeshPool.plane20, matrix, FadedMaterialPool.FadedVersionOf(ShieldBlendingParticle.ShieldSparksMat, 0.2f + (transitionStatus / (float)transitionMax) * 0.7f), 0);
        }
        private void doTransitionStep()
        {
            transitionStatus += transitionStep * transitionDirection;
            //If we reached maximum
            if (transitionStatus >= transitionMax)
            {
                transitionDirection = -1;
                transitionStatus = transitionMax;
            }
            //And if minimum
            else if (transitionStatus <= 0)
            {
                //Generate new rotation
                currentAngle = UnityEngine.Random.Range(0f, 360f);
                //Positive direction
                transitionDirection = 1;
                //Bottom cap
                transitionStatus = 0;
            }
        }
    }
}
