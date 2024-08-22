using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour, IPointerClickHandler
{
    public RectTransform RectTransform;
    public IItem Item;
    [SerializeField] private HorizontalLayoutGroup _itemDatasParent;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemPriceText;
    [SerializeField] private Button _takeButton;
    [SerializeField] private CustomEvent_IItem _fromStorageToInventory;

    private void Awake()
    {
        _takeButton.onClick.AddListener(() =>
        {
            if (Item == null) return;
            //Retirer l'item de ce truc
            _fromStorageToInventory?.Raise(Item);
        });
    }

    public void SetItem(string objName, int objPrice)
    {
        _itemNameText.text = objName;
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
