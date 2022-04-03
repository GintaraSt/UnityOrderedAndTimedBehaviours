using UnityEngine;

namespace OrderedBehaviors
{
    public class OrderedBehaviorManager : MonoBehaviour
    {
        public static OrderedBehaviorManager Instance;
        private static readonly float _timedUpdateCheckInterval = 0.05f;
        private float _lastRun = 0f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            if (Instance != this)
            {
                Debug.LogWarning("There are multiple Ordered Behaviour Managers in the scene. Please ensure there's always exactly one Ordered Behaviour Manager in your scene. Any additional manager components were destroyed.");
                Destroy(this);
                return;
            }
        }

        private void Start()
        {
            CallbacksManager.RunOrderedGroupCallbacks(CallbackGroup.FirstStart);
            CallbacksManager.RunOrderedGroupCallbacks(CallbackGroup.SecondStart);
            CallbacksManager.RunOrderedGroupCallbacks(CallbackGroup.ThirdStart);
        }

        private void Update()
        {
            if (Time.unscaledTime - _lastRun >= _timedUpdateCheckInterval)
                CallbacksManager.RunTimedCallbacks();
        }
    }
}
