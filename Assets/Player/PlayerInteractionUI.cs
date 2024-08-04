using System;
using TMPro;
using UnityEngine;

public class PlayerInteractionUI : MonoBehaviour
{
    [SerializeField] private Transform _crosshair;
    [SerializeField] private GameObject _useIndicator;
    [SerializeField] private GameObject _pickupIndicator;
    [SerializeField] private TextMeshProUGUI _nameObject;
    [SerializeField] private CustomEvent_IItem _DisplaySelection;
    [SerializeField] private CustomEvent _HideSelection;

    private void Awake()
    {
        _DisplaySelection.handle += DisplaySelection;
        _HideSelection.handle += HideSelection;
    }

    private void Start()
    {
        HideSelection();
    }

    private void OnDestroy()
    {
        _DisplaySelection.handle -= DisplaySelection;
        _HideSelection.handle -= HideSelection;
    }

    private void DisplaySelection(IItem item)
    {
        _nameObject.text = item.ItemDatas.itemName;
        _crosshair.localScale = Vector3.one * 2;

        HandleIndicators(item.ItemDatas.IsInventoryItem, item.ItemDatas.IsUsable);
    }

    private void HideSelection()
    {
        _nameObject.text = string.Empty;
        _crosshair.localScale = Vector3.one;

        HandleIndicators(false, false);
    }

    private void HandleIndicators(bool isInventoryItem, bool isUsable)
    {
        _useIndicator.SetActive(isUsable);
        _pickupIndicator.SetActive(isInventoryItem);
    }
}
