using UnityEngine;

namespace LightHouse.Interactions.Samples
{
    public class TriggerItemDoor : MonoBehaviour
    {
        [SerializeField] private GameObject _targetObj;
        [SerializeField] private Door _door;
        [SerializeField] private TriggerEvent _triggerEvent;

        private void Awake()
        {
            _triggerEvent.OnEntered += TriggerEvent_OnTriggeredEnter;
            _triggerEvent.OnExited += TriggerEvent_OnTriggerExited;
            _door.CanBeInteracted = false;
            _door.CanBeRaycasted = false;
        }

        private void OnDestroy()
        {
            _triggerEvent.OnEntered -= TriggerEvent_OnTriggeredEnter;
            _triggerEvent.OnExited -= TriggerEvent_OnTriggerExited;
        }

        private void TriggerEvent_OnTriggeredEnter(GameObject @object)
        {
            if (@object == _targetObj)
                _door.Interact();
        }

        private void TriggerEvent_OnTriggerExited(GameObject @object)
        {
            if (@object == _targetObj)
                _door.Interact();
        }
    }

}
