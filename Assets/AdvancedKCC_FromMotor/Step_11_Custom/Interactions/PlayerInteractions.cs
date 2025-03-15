using System;
using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;

namespace LightHouse.AdvancedController
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
        private IDescribable _lastDescribable = null;

        public event Action<IInteractable> OnInteractableItemDetected;
        public event Action<IDescribable> OnDescribableItemDetected;

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
                    _lastDescribable = hitObject.TryGetComponent(out IDescribable describable) ? describable : null;

                    //Start events and everything else
                    if (_lastInteractable != null)
                    {
                        OnInteractableItemDetected?.Invoke(_lastInteractable);
                        Debug.Log($"Nouveau Item détecté : {_lastObjectSeen.name}");
                    }

                    if (_lastDescribable != null)
                    {
                        OnDescribableItemDetected?.Invoke(_lastDescribable);
                        UpdateDescriptionUI();
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
                    if(_lastDescribable != null)
                    {
                        _lastDescribable.OnNameUpdated -= OnCurrentSeingObjectNameUpdate;
                        _lastDescribable.OnDescriptionUpdated -= OnCurrentSeingObjectDescriptionUpdate;
                    }

                    Debug.Log("Plus aucun objet interactif en vue.");
                    _lastObjectSeen = null;
                    _lastInteractable = null;
                    _lastDescribable = null;

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
            if (_lastDescribable != null)
            {
                string itemName = _lastDescribable.GetName();
                string itemDescription = _lastDescribable.GetDescription();

                _lastDescribable.OnNameUpdated += OnCurrentSeingObjectNameUpdate;
                _lastDescribable.OnDescriptionUpdated += OnCurrentSeingObjectDescriptionUpdate;

                if (_canvasInteraction.IsHided)
                    _canvasInteraction.Show();

                if (string.IsNullOrEmpty(itemName))
                {
                    _canvasInteraction.HideItemName();
                }
                else
                {
                    _canvasInteraction.ItemName_TMP.text = itemName;
                    _canvasInteraction.ShowItemName();
                }

                if (string.IsNullOrEmpty(itemDescription)) 
                {
                    _canvasInteraction.HideItemDescription();
                }
                else
                {
                    _canvasInteraction.ItemDescription_TMP.text = itemDescription;
                    _canvasInteraction.ShowItemDescription();
                }
            }
        }

        private void OnCurrentSeingObjectNameUpdate()
        {
            string itemName = _lastDescribable.GetName();
            _canvasInteraction.ItemName_TMP.text = string.IsNullOrEmpty(itemName) ? "" : itemName;
        }

        private void OnCurrentSeingObjectDescriptionUpdate()
        {
            string itemDescription = _lastDescribable.GetDescription();
            _canvasInteraction.ItemDescription_TMP.text = string.IsNullOrEmpty(itemDescription) ? "" : itemDescription;
        }
    }

}
