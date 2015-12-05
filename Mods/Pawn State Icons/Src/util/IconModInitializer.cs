using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine; 
using RimWorld;
using Verse;
using System.Threading;

namespace RimWorldIconMod
{
    class IconModInitializer : ITab//ThoughtWorker
    {
        protected GameObject iconControllerObject;


        public IconModInitializer()
        {            
            Log.Message("Initialized the Pawn State Icons mod");
            iconControllerObject = new GameObject("IconModInitializer");
            iconControllerObject.AddComponent<IconModInitializerBehaviour>();
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)iconControllerObject);                
        }

        protected override void FillTab()
        {

        }
    }

    class IconModInitializerBehaviour : MonoBehaviour
    {
        protected GameObject PSIObject;
        protected bool reinjectNeeded = false;
        protected float reinjectTime = 0;

        public void OnLevelWasLoaded(int level)
        {
//            Log.Message("DEBUG: OnLevelWasLoaded "+level);
            reinjectNeeded = true;
            if (level >= 0) reinjectTime = 1;
            else reinjectTime = 0;
        }

        public void FixedUpdate()
        {
            if (reinjectNeeded)
            {
                reinjectTime -= Time.fixedDeltaTime;

                if (reinjectTime <= 0)
                {
                    reinjectNeeded = false;
                    reinjectTime = 0;

                    PSIObject = GameObject.Find("IconModMain");
                    if (PSIObject == null) PSIObject = new GameObject("IconModMain");
                    PSIObject.AddComponent<PSI>();

                    Log.Message("PSI Injected");
                }
            }

        }

        public void Start()
        {
            OnLevelWasLoaded(-1);
        }

        
    }
}
