using System.Collections.Generic;
using UnityEngine;
using LightHouse.Interactions;

namespace LightHouse.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        private byte _inventoryCapacity = 4;
        public Dictionary<IItem, GameObject> Inventory = new(); // Associe un IItem à son GameObject
        [SerializeField] private Transform _inventoryParent = null;
        [SerializeField] private Transform _inventoryTarget = null;
        public static PlayerInventory Instance;

        private void Awake()
        {
            Instance = this;
        }

        public bool AddItem(IItem item)
        {
            if (Inventory.Count >= _inventoryCapacity)
            {
                Debug.LogWarning("Inventaire plein !");
                return false;
            }

            GameObject obj = item.GetGameObject();
            obj.SetActive(false); // Désactive l'objet dans la scène
            obj.transform.SetParent(_inventoryParent); // On le met en enfant de l'inventaire

            item.GetCollider().enabled = false;
            item.HasRigidBody().isKinematic = true;

            Inventory[item] = obj;

            Debug.Log($"Objet ajouté : {item.GetName()}");
            return true;
        }

        private void Update()
        {
            _inventoryParent.position = _inventoryTarget.position;
            _inventoryParent.rotation = _inventoryTarget.rotation;
        }

        public void RemoveItem(IItem item)
        {
            if (Inventory.ContainsKey(item))
            {
                Inventory.Remove(item);
                Debug.Log($"Objet retiré : {item.GetName()}");
            }
        }

        public void DropItem(IItem item, Vector3 dropPosition)
        {
            if (Inventory.ContainsKey(item))
            {
                GameObject obj = Inventory[item];

                obj.transform.SetParent(null); // Sort de l'inventaire
                obj.transform.position = dropPosition;
                obj.transform.rotation = Quaternion.identity;
                obj.SetActive(true); // Réactive l’objet

                RemoveItem(item); // Supprime du dictionnaire mais garde l'objet dans la scène
                Debug.Log($"Objet droppé : {item.GetName()}");
            }
        }
    }
}
