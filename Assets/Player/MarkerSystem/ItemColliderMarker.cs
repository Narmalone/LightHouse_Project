using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Items
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
        [SerializeField] private Collider col;
        public Collider Collider => col;

        private void Awake()
        {
            RegisterToItem();
        }

        private void OnDestroy()
        {
            UnregisterToItem();
        }

        private void OnValidate()
        {
            col = GetComponent<Collider>();
        }

        public void RegisterToItem()
        {
            ItemRegistry.Register(col, TargetComponent);

        }

        public void UnregisterToItem()
        {
            ItemRegistry.Unregister(col);

        }
    }


    public static class ItemRegistry
    {
        private static Dictionary<Collider, GameObject> _inventoryMap = new();

        public static void Register(Collider col, GameObject targetComponent)
        {
            if (col != null)
                _inventoryMap[col] = targetComponent;
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
