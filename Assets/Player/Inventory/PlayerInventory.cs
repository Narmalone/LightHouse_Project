using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    private PlayerManager _manager;
    [SerializeField] private byte slots = 4;
    [SerializeField] private byte currentUsedSlots = 0;
    public GameObject previewObjectParent; // Reference to the 3D preview object
    public List<InventorySlot> listInventorySlots = new List<InventorySlot>();
    private List<GameObject> listPreviewObject = new List<GameObject> { null, null, null, null};
    private int selectedSlot;

    public static bool IsInventoryFull = false;
    public static Action<ItemBaseInventory> TakeItemAction;
    
    private GameObject previewObject;

    private void Awake()
    {
        TakeItemAction += TakeItem;
        listPreviewObject = new List<GameObject> { null, null, null, null };
    }

    private void Start()
    {
        SelectSlot(0);
    }

    private void OnDestroy()
    {
        TakeItemAction -= TakeItem;
    }

    void Update()
    {
        // Check for keyboard input to select a slot
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSlot(3);
        }

        // Check for mouse wheel input to select a slot
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            int scrollDirection = Input.GetAxis("Mouse ScrollWheel") > 0 ? -1 : 1;
            SelectSlot(selectedSlot + scrollDirection);
        }

        if (Input.GetMouseButtonDown(1))
        {
            //if(selectedSlot)
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }
    }

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }

    private void TakeItem(ItemBaseInventory item)
    {
        // Ajouter dans le slot (Choix slot, Nom, Icon)
        var slotIndex = GetEmptySlot();
        
        // If no empty slot
        if(slotIndex == -1) return;

        listInventorySlots[slotIndex].SetItem(item.ItemDatas);

        AddPreviewObject(slotIndex, item.ItemDatas.mesh);

        // Enlever l'item du jeux
        item.gameObject.SetActive(false);

        Destroy(item.gameObject);
    }

    private void DropItem()
    {
        if (listInventorySlots[selectedSlot].item == null) return;
        // Drop prefab
        Instantiate(listInventorySlots[selectedSlot].item.prefab, _manager.transform.position + _manager._data.playerCamera.transform.forward * 2, Quaternion.identity);

        // Empty the item slot
        listInventorySlots[selectedSlot].SetItem(null);

        if(listPreviewObject[selectedSlot] != null)
        {
            Destroy(listPreviewObject[selectedSlot]);
        }
    }

    private int GetEmptySlot()
    {
        if (currentUsedSlots >= slots) return -1;

        for (int i = 0; i < listInventorySlots.Count; i++)
        {
            if (listInventorySlots[i].isEmpty == false) continue;
            return i;
        }
        return -1;
    }

    void SelectSlot(int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, slots - 1);

        UpdatePreviewObject(slotIndex);
        listInventorySlots[selectedSlot].OnDeselect();
        selectedSlot = slotIndex;
        listInventorySlots[selectedSlot].OnSelect();

    }

    private void AddPreviewObject(int index, GameObject mesh)
    {
        if (listPreviewObject[index] != null) Destroy(listPreviewObject[index]);
        listPreviewObject[index] = Instantiate(mesh, previewObjectParent.transform.position, previewObjectParent.transform.rotation, previewObjectParent.transform);
    }

    private void UpdatePreviewObject(int nextIndex)
    {
        listPreviewObject[selectedSlot]?.SetActive(false);
        listPreviewObject[nextIndex]?.SetActive(true);
    }
    /*
   public void RemoveItemFromInventory(GameObject obj)
   {
       if(objectsInInventory.Contains(obj))
       {
           listInventorySlots[currentUsedSlots].iconImage.color = Color.white;
           currentUsedSlots--;
           objectsInInventory.Remove(obj);
       }
   }*/
}