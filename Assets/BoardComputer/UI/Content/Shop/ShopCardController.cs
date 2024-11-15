using MPUIKIT;
using TMPro;
using UnityEngine;

public enum ShopItemState
{
    Promotion,
    MainMarket
}

public class ShopCardController : MonoBehaviour
{
    public MPImageBasic ItemBackground;
    public MPImageBasic ItemIcon;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
    public TextMeshProUGUI ItemCost;
    public TextMeshProUGUI dollarText;
    public ShopItemState ItemState;

    public ShopItemData ItemDataInstance;
    private ShopItemData _itemData;
    protected int _baseStock = 0;

    private void Awake()
    {
        ItemDataInstance = ScriptableObject.CreateInstance<ShopItemData>();
    }

    public void SetItemData(ShopItemData item)
    {
        _itemData = item;
        ItemDataInstance.Name = _itemData.Name;
        ItemDataInstance.Description = _itemData.Description;
        ItemDataInstance.BaseCost = _itemData.BaseCost;
        ItemDataInstance.StockItems = _itemData.StockItems;
        ItemDataInstance.Prefab = _itemData.Prefab;
        ItemDataInstance.ItemID = _itemData.ItemID;

        _baseStock = item.StockItems;
        UpdateCardInfoFromItemData();
    }

    public void SetItemState(ShopItemState nextState)
    {
        ItemState = nextState;
    }

    public virtual void UpdateCardInfoFromItemData()
    {
        ItemName.text = ItemDataInstance.Name;
        ItemDescription.text = ItemDataInstance.Description;
        ItemCost.text = ItemDataInstance.BaseCost.ToString();
    }
}
