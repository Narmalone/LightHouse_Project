using System.Collections.Generic;
using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;
using System.Linq;

namespace LightHouse.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        private byte _inventoryCapacity = 4;
        private byte _currentSlotIndex = 0;
        public List<IInventoryItem> Inventory = new(); // Associe un IItem à son GameObject
        [SerializeField] private Transform _inventoryParent = null;
        [SerializeField] private Transform _inventoryTarget = null;
        public static PlayerInventory Instance;
        public LayerMask mask = 1 << -1;
        public Camera _playerCamera;

        private void Awake()
        {
            Instance = this;
        }

        public bool AddItem(IInventoryItem item)
        {
            if (Inventory.Count >= _inventoryCapacity)
            {
                Debug.LogWarning("Inventaire plein !");
                return false;
            }

            GameObject obj = item.GetItem().gameObject;
            //obj.SetActive(false); // Désactive l'objet dans la scène
            obj.transform.SetParent(_inventoryParent); // On le met en enfant de l'inventaire
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            item.GetCollider().enabled = false;
            item.HasRigidBody().isKinematic = true;

            Inventory.Add(item);
            item.OnItemAddedToInventory();

            Debug.Log($"Objet ajouté : {item.GetItem().name}");
            return true;
        }

        private void Update()
        {
            _inventoryParent.position = _inventoryTarget.position;
            _inventoryParent.rotation = _inventoryTarget.rotation;

            Ray cameraRay = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            TryInteratableItem(cameraRay, 3.0f, mask, QueryTriggerInteraction.Ignore);

            if (InputManager.PLAYER_INPUTS_ACTIONS.Player.Drop.WasPerformedThisFrame())
            {
                //changer et faire en sorte de poser par rapport à la direction de la cam
                //s'assurer qu'un objet ne puisse pas traverser le sol
                //faire un système de puissance auquel si on maintien ça lance plus fort
                DropItem(Inventory[0], _inventoryParent.transform.position + _inventoryParent.forward);
                Debug.DrawRay(_inventoryParent.transform.position, _inventoryParent.forward, Color.green, 5f);
            }
        }

        private GameObject _lastObjectSeen;
        private IInventoryItem _inventoryItem;
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

                    if (hitObject.GetComponent<IInventoryItem>() != null)
                    {
                        _inventoryItem = hitObject.GetComponent<IInventoryItem>();
                    }
                    Debug.Log(_inventoryItem.GetItem().name);
                }

                //If we are seing an object and performing our interact key we interacted with the item
                if (_inventoryItem != null && InputManager.PLAYER_INPUTS_ACTIONS.Player.Pickup.WasPerformedThisFrame())
                {
                    AddItem(_inventoryItem);
                    Debug.Log("Get item and add it to the inventory");
                }
            }
            else
            {
                //Reset if we don't see anything
                if (_lastObjectSeen != null)
                {
                    

                    Debug.Log("Plus aucun objet interactif en vue.");
                    _lastObjectSeen = null;

                    if (_inventoryItem != null)
                        _inventoryItem = null;
                }
            }

            Debug.DrawRay(_playerCamera.transform.position, ray.direction * 3.0f, Color.cyan);
        }

        public void RemoveItem(IInventoryItem item)
        {
            if (Inventory.Contains(item))
            {
                Inventory.Remove(item);
                item.OnItemRemovedFromInventory();
                Debug.Log($"Objet retiré : {item.GetItem().name}");
            }
        }

        //TO DO:: lancer des délégates / fonctions aux items lorsqu'on les drop / ajoutes à l'inv
        public void DropItem(IInventoryItem item, Vector3 dropPosition)
        {
            if (Inventory.Contains(item))
            {
                GameObject obj = item.GetItem().gameObject;

                obj.transform.SetParent(null); // Sort de l'inventaire
                obj.transform.position = dropPosition;
                obj.transform.rotation = Quaternion.identity;
                item.GetCollider().enabled = true;
                item.HasRigidBody().isKinematic = false;
                obj.SetActive(true); // Réactive l’objet

                RemoveItem(item); // Supprime du dictionnaire mais garde l'objet dans la scène
                Debug.Log($"Objet droppé : {item.GetItem().name}");
            }
        }
    }
}
