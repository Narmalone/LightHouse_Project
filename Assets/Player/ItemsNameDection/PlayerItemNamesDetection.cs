using LightHouse.Interactions;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    public class PlayerItemNamesDetection : MonoBehaviour
    {
        #region FIELDS
        [Header("Settings")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _raycastDistance = 3.0f;
        [SerializeField] private LayerMask _targetLayersLayer;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction;
        [SerializeField] private CanvasInteraction _interactionCanvas;

        //Controllers
        private RaycastNameDisplayHandler _nameDisplayHandler;
        private RaycastDetector<IItemName> _raycastItemName;
        #endregion

        #region MONO CALLBACKS

        private void Awake()
        {
            _nameDisplayHandler = new RaycastNameDisplayHandler(_interactionCanvas);

            _raycastItemName = new RaycastDetector<IItemName>(
                _playerCamera, _raycastDistance, _targetLayersLayer, _triggerInteraction
            );

            _raycastItemName.OnDetected += item => _nameDisplayHandler.SetTarget(item);
            _raycastItemName.OnItemLost += () => _nameDisplayHandler.SetTarget(null);
        }

        private void Update()
        {
            _raycastItemName.UpdateRay();
        }

        private void OnDestroy()
        {
            _raycastItemName.OnDetected -= item => _nameDisplayHandler.SetTarget(item);
            _raycastItemName.OnItemLost -= () => _nameDisplayHandler.SetTarget(null);
        }
        #endregion
    }
}

