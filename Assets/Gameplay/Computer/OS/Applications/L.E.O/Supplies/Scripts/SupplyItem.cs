using LightHouse.Game.Computer.LEO.Supplies;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupplyItem : MonoBehaviour
{
    [SerializeField] private Button _minusButton;
    [SerializeField] private Button _plusButton;
    [SerializeField] private TextMeshProUGUI _itemNameTxt;
    [SerializeField] private TextMeshProUGUI _itemsInStockText;
    [SerializeField] private TextMeshProUGUI _itemCostText;
    [SerializeField] private TextMeshProUGUI _itemAmountText;

    public event Action<SupplyItemDatas> MinusCliqued;
    public event Action<SupplyItemDatas> PlusCliqued;

    public SupplyItemDatas Mydatas { get; private set; }

    private void Awake()
    {
        _minusButton.onClick.AddListener(OnMinusCliqued);
        _plusButton.onClick.AddListener(OnPlusCliqued);

        _minusButton.interactable = false;
    }

    private void OnDestroy()
    {
        _minusButton.onClick.RemoveListener(OnMinusCliqued);
        _plusButton.onClick.RemoveListener(OnPlusCliqued);
    }

    private void OnPlusCliqued()
    {
        if (Mydatas == null)
            return;
        PlusCliqued?.Invoke(Mydatas);
    }

    private void OnMinusCliqued()
    {
        if (Mydatas == null)
            return;
        MinusCliqued?.Invoke(Mydatas);
    }

    public void Initialize(SupplyItemDatas datas)
    {
        Mydatas = datas;
        _itemNameTxt.text = datas.Name;
        _itemsInStockText.text = "Coming soon...";
        _itemCostText.text = datas.Cost.ToString("000");
        _itemAmountText.text = 0.ToString();
    }

    public void UpdateSelectedCountText()
    {
        if (Mydatas.SelectedAmountToBuy <= 0 && _minusButton.interactable) _minusButton.interactable = false;
        else if (Mydatas.SelectedAmountToBuy > 0 && !_minusButton.interactable) _minusButton.interactable = true;
            _itemAmountText.text = Mydatas.SelectedAmountToBuy.ToString();
    }
}
