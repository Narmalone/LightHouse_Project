using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Items
{
    public enum ItemRole
    {
        Inventory,
        Interactable
    }

    public class ItemColliderMarker : MonoBehaviour
    {
#if UNITY_EDITOR
        public ItemRole ItemRole;
#endif
        public GameObject TargetComponent;
        [SerializeField] private bool includeChildren = false;

        [SerializeField] private List<Collider> _colliders = new();
        public List<Collider> Colliders => _colliders;

        private void Awake()
        {
            CacheColliders();
            RegisterToItem();
        }

        private void OnDestroy()
        {
            UnregisterToItem();
        }

        private void OnValidate()
        {
            CacheColliders();
        }

        public void CacheColliders()
        {
            _colliders.Clear();
            if (includeChildren)
                _colliders.AddRange(GetComponentsInChildren<Collider>(true));
            else if (TryGetComponent(out Collider singleCol))
                _colliders.Add(singleCol);
        }

        public void RegisterToItem()
        {
            foreach (var col in _colliders)
            {
                ItemRegistry.Register(col, TargetComponent);
            }
        }

        public void UnregisterToItem()
        {
            foreach (var col in _colliders)
            {
                ItemRegistry.Unregister(col);
            }
        }
    }

    public static class ItemRegistry
    {
        private static Dictionary<Collider, GameObject> _inventoryMap = new();

        public static void Register(Collider col, GameObject targetComponent)
        {
            if (col == null || targetComponent == null) return;

            if (_inventoryMap.TryGetValue(col, out var current) && current == targetComponent)
                return; // rien à changer

            _inventoryMap[col] = targetComponent;
            //Debug.Log($"[ItemRegistry] Registered {col.name} → {targetComponent.name}");
        }


        public static void Unregister(Collider col)
        {
            if (col != null)
                _inventoryMap.Remove(col);
        }

        public static bool IsMarked(Collider col, out GameObject markedObject)
        {
            markedObject = null;
            return col != null && _inventoryMap.TryGetValue(col, out markedObject) && markedObject;
        }
    }

}
