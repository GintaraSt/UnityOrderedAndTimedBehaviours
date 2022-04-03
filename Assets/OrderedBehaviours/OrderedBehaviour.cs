using UnityEngine;

namespace OrderedBehaviors
{
    public class OrderedBehaviour : MonoBehaviour
    {
        private void OnEnable()
        {
            CallbacksManager.EnableCallbacksForObject(this, gameObject.GetInstanceID());
            Enable();
        }

        private void OnDisable()
        {
            CallbacksManager.DisableCallbacksForObject(gameObject.GetInstanceID());
            Disable();
        }

        private void OnDestroy()
        {
            CallbacksManager.DestroyCallbackForObject(gameObject.GetInstanceID());
            Destroy();
        }

        protected virtual void Enable()
        { }

        protected virtual void Disable()
        { }

        protected virtual void Destroy()
        { }
    }
}