using LightHouse.Interactions;
using LightHouse.Utilities;
using UnityEngine;

namespace LightHouse.Interactions
{
    public class PlayerInteractions : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 3.0f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private QueryTriggerInteraction triggerInteraction;
        [SerializeField] private CanvasInteraction _interactionCanva;

        [Header("Submodules")]
        [SerializeField] private RaycastNameDisplayHandler nameDisplayHandler;
        [SerializeField] private RaycastInteractionHandler interactionHandler;

        private GameObject lastSeenObject;

        private void Awake()
        {
            nameDisplayHandler = new RaycastNameDisplayHandler(_interactionCanva);
            interactionHandler = new RaycastInteractionHandler(_interactionCanva);
        }

        private void Update()
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (RaycastUtility.TryRaycast(ray, interactionDistance, interactableLayer, triggerInteraction, out RaycastHit hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject != lastSeenObject)
                {
                    lastSeenObject = hitObject;
                    if (hitObject.TryGetComponent<IItemName>(out var itemName))
                        nameDisplayHandler.SetTarget(itemName);
                    else
                        nameDisplayHandler.SetTarget(null);

                    if (hitObject.TryGetComponent<IInteractable>(out var interactable))
                        interactionHandler.SetTarget(interactable);
                    else
                        interactionHandler.SetTarget(null);
                }
                interactionHandler.Update();
            }
            else
            {
                if(lastSeenObject != null)
                {
                    nameDisplayHandler.SetTarget(null);
                    interactionHandler.SetTarget(null);
                    lastSeenObject = null;
                }
                //if an object is destroyed it will be setted as null but the ui will still be there
                else
                {
                    if(nameDisplayHandler.HasTarget) nameDisplayHandler.SetTarget(null);
                    if(interactionHandler.HasTarget) interactionHandler.SetTarget(null);
                }
            }
        }
    }

}
