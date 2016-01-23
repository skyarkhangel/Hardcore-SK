// Manager/ManagerController.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-09-22 22:32

using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class Bootstrap : ITab
    {
        public static GameObject GameObject;

        public Bootstrap()
        {
            if ( GameObject != null )
            {
                return;
            }
            GameObject = new GameObject( "Manager_Controller" );
            GameObject.AddComponent<ManagerController>();
            Object.DontDestroyOnLoad( GameObject );
        }

        protected override void FillTab() {}
    }

    internal class ManagerController : MonoBehaviour
    {
        public readonly string GameObjectName = "Fluffy Manager";
        public void OnLevelWasLoaded() {}

        public void Start()
        {
            Log.Message( "Manager Controller loaded." );
            enabled = true;
        }
    }
}