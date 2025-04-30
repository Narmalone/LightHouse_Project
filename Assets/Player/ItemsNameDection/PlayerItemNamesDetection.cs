using LightHouse.Interactions;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    public class PlayerItemNamesDetection : MonoBehaviour
    {
        #region FIELDS
        [Header("Settings")]
        [SerializeField] private CanvasInteraction _interactionCanvas;

        //Controllers
        private RaycastNameDisplayHandler _nameDisplayHandler;

        [SerializeField] private UnifiedRaycastSystem _unifiedRaycastSystem;
        private RaycastDetector<IItemName> _raycastItemName;
        #endregion

        #region MONO CALLBACKS

        private void Awake()
        {
            _nameDisplayHandler = new RaycastNameDisplayHandler(_interactionCanvas);

            _raycastItemName = _unifiedRaycastSystem.ItemNameDetector;

            _raycastItemName.OnDetected += item => _nameDisplayHandler.SetTarget(item);
            _raycastItemName.OnItemLost += () => _nameDisplayHandler.SetTarget(null);
        }

        private void Update()
        {
        }

        private void OnDestroy()
        {
            _raycastItemName.OnDetected -= item => _nameDisplayHandler.SetTarget(item);
            _raycastItemName.OnItemLost -= () => _nameDisplayHandler.SetTarget(null);
        }
        #endregion
    }
}

