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

    private void Awake()
    {
        _takeButton.onClick.AddListener(() =>
        {
            //Retirer l'item de ce truc
            _fromStorageToInventory?.Raise(Item);
        });
    }

    private void OnDestroy()
    {
        Item = null;
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
            Debug.Log("Item pas nul & double clique");
            //Voir aussi s'il y'a de la place dans l'inventaire
            if (PlayerManager.Instance._inventory.IsInventoryFull)
            {
                Debug.Log("INVENTAIRE FULL");
                //petite animation, ou effet visuel pour montrer que c'est pas possible
            }
            else
            {
                Debug.Log("Inventaire pas full");
                _fromStorageToInventory?.Raise(Item);
                Item = null;
            }
        }
    }
}
