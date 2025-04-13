using LightHouse.Utilities;
using UnityEngine;

namespace LightHouse.Interactions
{
    /// <summary>
    /// The main class to handle Interactions with world and the items name.
    /// </summary>
    public class PlayerInteractions : MonoBehaviour
    {
        #region FIELDS
        [Header("Setting Fields")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _raycastDistance = 3.0f;
        [SerializeField] private LayerMask _targetLayersLayer;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction;
        [SerializeField] private CanvasInteraction _interactionCanvas;

        [Header("Submodules")]
        [SerializeField] private RaycastNameDisplayHandler _nameDisplayHandler;
        [SerializeField] private RaycastInteractionHandler _interactionHandler;
        private GameObject lastSeenObject;
        #endregion

        #region MONO'S CALLBACK
        private void Awake()
        {
            _nameDisplayHandler = new RaycastNameDisplayHandler(_interactionCanvas);
            _interactionHandler = new RaycastInteractionHandler(_interactionCanvas);
        }

        private void Update()
        {
            Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (RaycastUtility.TryRaycast(ray, _raycastDistance, _targetLayersLayer, _triggerInteraction, out RaycastHit hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                //if we raycast something different than the previous we try to get the datas
                if (hitObject != lastSeenObject)
                {
                    lastSeenObject = hitObject;
                    if (hitObject.TryGetComponent<IItemName>(out var itemName))
                        _nameDisplayHandler.SetTarget(itemName);
                    else
                        _nameDisplayHandler.SetTarget(null);

                    if (hitObject.TryGetComponent<IInteractable>(out var interactable))
                        _interactionHandler.SetTarget(interactable);
                    else
                        _interactionHandler.SetTarget(null);
                }
                _interactionHandler.Update();
            }
            else
            {
                //else we stop raycast anything we stop display the datas on the screen
                if(lastSeenObject != null)
                {
                    _nameDisplayHandler.SetTarget(null);
                    _interactionHandler.SetTarget(null);
                    lastSeenObject = null;
                }
                //if an object is destroyed it will be setted as null but the ui is still there
                else
                {
                    if(_nameDisplayHandler.HasTarget) _nameDisplayHandler.SetTarget(null);
                    if(_interactionHandler.HasTarget) _interactionHandler.SetTarget(null);
                }
            }
        }
        #endregion
    }

}
