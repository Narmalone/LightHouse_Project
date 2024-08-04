using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    private PlayerManager _manager;
    public ItemOptionController controller;
    [SerializeField] private byte slots = 4;
    [SerializeField] private byte currentUsedSlots = 0;
    public List<GameObject> objectsInInventory = new List<GameObject>();
    public List<InventorySlot> buttonsInInventory = new List<InventorySlot>();
    private int selectedSlot;

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
    }

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }

    void SelectSlot(int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, slots - 1);

        buttonsInInventory[selectedSlot].iconImage.color = Color.white;
        selectedSlot = slotIndex;
        buttonsInInventory[selectedSlot].iconImage.color = Color.green;
    }

    public void AddItemToInventory(GameObject obj, InventoryItemData item)
    {
        if(currentUsedSlots >= slots)
        {
            Debug.Log("Slots max utilisées");
            return;
        }
        buttonsInInventory[currentUsedSlots].iconImage.color = Color.gray;
        currentUsedSlots++;
        objectsInInventory.Add(obj);
    }

    public void RemoveItemFromInventory(GameObject obj)
    {
        if(objectsInInventory.Contains(obj))
        {
            buttonsInInventory[currentUsedSlots].iconImage.color = Color.white;
            currentUsedSlots--;
            objectsInInventory.Remove(obj);
        }
    }

    public void RemoveItemFromInventoryAtIndex(int index)
    {
        buttonsInInventory[index].iconImage.color = Color.white;
        currentUsedSlots--;
        objectsInInventory.RemoveAt(index);
    }
}