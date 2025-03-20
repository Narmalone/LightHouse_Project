using System;
using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;

namespace Narmalone.AdvancedController
{
    public class PlayerInteractions : MonoBehaviour
    {
        [SerializeField] private float _interactionDistance = 3.0f;
        [SerializeField] private LayerMask _interactableLayer = 1 << 0;
        [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private bool _enableInteraction = true;
        [SerializeField] private CanvasInteraction _canvasInteraction;

        private GameObject _lastObjectSeen = null;  //Store last object seen
        private IInteractable _lastInteractable = null;

        public event Action<IInteractable> OnInteractableItemDetected;

        private void Start()
        {
            _canvasInteraction.Hide();
        }

        private void Update()
        {
            if (!_enableInteraction) return;

            Ray cameraRay = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            TryInteratableItem(cameraRay, _interactionDistance, _interactableLayer, _queryTriggerInteraction);
        }

        private void TryInteratableItem(Ray ray, float interactionDistance, int interactableLayer, QueryTriggerInteraction qti)
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(ray, out hit, interactionDistance, interactableLayer, qti);

            if (hasHit)
            {
                GameObject hitObject = hit.collider.gameObject;

                //check if current hited object is different and apply it once
                if (hitObject != _lastObjectSeen)
                {
                    _lastObjectSeen = hitObject;

                    //Get and READS the components only once to avoid bad performances
                    _lastInteractable = hitObject.TryGetComponent(out IInteractable interactable) ? interactable : null;

                    //Start events and everything else
                    if (_lastInteractable != null)
                    {
                        OnInteractableItemDetected?.Invoke(_lastInteractable);
                        Debug.Log($"Nouveau Item détecté : {_lastObjectSeen.name}");
                    }
                }

                //If we are seing an object and performing our interact key we interacted with the item
                if (_lastInteractable != null && InputManager.Interact.WasPerformedThisFrame())
                {
                    _lastInteractable.Interact();
                }
            }
            else
            {
                //Reset if we don't see anything
                if (_lastObjectSeen != null)
                {
                   

                    Debug.Log("Plus aucun objet interactif en vue.");
                    _lastObjectSeen = null;
                    _lastInteractable = null;

                    if(!_canvasInteraction.IsHided)
                        _canvasInteraction.Hide();
                }
            }

            Debug.DrawRay(_playerCamera.transform.position, ray.direction * _interactionDistance, Color.magenta);
        }

        /// <summary>
        /// Called only once when we detect a new IDesribable
        /// </summary>
        private void UpdateDescriptionUI()
        {
           
        }

    }

}
