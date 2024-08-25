using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour, IPointerClickHandler
{
    public RectTransform RectTransform;
    public ItemBase Item;
    [SerializeField] private HorizontalLayoutGroup _itemDatasParent;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemPriceText;
    [SerializeField] private Button _takeButton;
    [SerializeField] private CustomEvent_ItemBase _fromStorageToInventory;

    public event Action<ItemSlotController> FromStorageToInventorySlot;

    private StorePointController _storePointController;

    public void SetStorePointController(StorePointController spc)
    {
        _storePointController = spc;
    }

    public StorePointController GetStorePointController() => _storePointController;

    private void Awake()
    {
        _takeButton.onClick.AddListener(() =>
        {
            //Retirer l'item de ce truc
            if (PlayerManager.Instance._inventory.IsInventoryFull)
            {
                Debug.Log("INVENTAIRE FULL");
                //petite animation, ou effet visuel pour montrer que c'est pas possible
            }
            else
            {
                _fromStorageToInventory?.Raise(Item);
                FromStorageToInventorySlot?.Invoke(this);
                Item = null;
                Destroy(this.gameObject);
            }
        });
    }

    public void SetItem(ItemBase item)
    {
        Item = item;
        _itemNameText.text = item.Name;

        var itmPrice = item.GetItemPrice();
        //L'item n'a aucun prix donc afficher autre chose
        if(itmPrice < 0)
        {
            _itemPriceText.text = "NO PRICE NEEDED";
        }
        else
        {
            _itemPriceText.text = item.GetItemPrice().ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(Item != null && eventData.clickCount >= 2)
        {
            //Voir aussi s'il y'a de la place dans l'inventaire
            if (PlayerManager.Instance._inventory.IsInventoryFull)
            {
                Debug.Log("INVENTAIRE FULL");
                //petite animation, ou effet visuel pour montrer que c'est pas possible
            }
            else
            {
                _fromStorageToInventory?.Raise(Item);
                FromStorageToInventorySlot?.Invoke(this);

                //Remove l'item de la slot
                Item = null;
                Destroy(this.gameObject);
            }
        }
    }
}
