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
                //Remove l'item de la slot
                Item = null;
                Destroy(this.gameObject);
            }
        });
    }

    public void SetItem(ItemBase item)
    {
        Item = item;
        _itemNameText.text = item.Name;
        //ITEM PRICE
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
                //Remove l'item de la slot
                Item = null;
                Destroy(this.gameObject);
            }
        }
    }
}
