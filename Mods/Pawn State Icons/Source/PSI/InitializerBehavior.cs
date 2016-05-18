using UnityEngine;
using Verse;

namespace PSI
{
    internal class InitializerBehavior : MonoBehaviour
    {
        private GameObject _psiObject;
        private bool _reinjectNeeded;
        private float _reinjectTime;

        private void OnLevelWasLoaded(int level)
        {
            _reinjectNeeded = true;
            _reinjectTime = level >= 0 ? 1f : 0.0f;
        }

        public void FixedUpdate()
        {
            if (!_reinjectNeeded)
                return;
            _reinjectTime -= Time.fixedDeltaTime;
            if (_reinjectTime > 0.0)
                return;
            _reinjectNeeded = false;
            _reinjectTime = 0.0f;
            _psiObject = GameObject.Find("PSIMain") ?? new GameObject("PSIMain");
            _psiObject.AddComponent<PSI>();
            Log.Message("PSI Injected!!");
        }

        public void Start()
        {
            OnLevelWasLoaded(0);
            enabled = true;
        }
    }
}
