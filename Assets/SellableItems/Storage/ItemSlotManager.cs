using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotManager : MonoBehaviour
{
    [SerializeField] private ItemSlotController _slotPrefab;
    [SerializeField] private List<ItemSlotController> _controllers;
    [SerializeField] private GridLayoutGroup _grid;
    [SerializeField] private ScrollRect _scrollRect;

    private void Start()
    {
        UpdateContentSize();
    }

    private void UpdateContentSize()
    {
        var rectTransformContent = _scrollRect.content;
        rectTransformContent.sizeDelta = new Vector2(0f, Mathf.Abs(GetMostDownControllerValue()) + _grid.spacing.y);
    }

    public float GetMostDownControllerValue()
    {
        return _controllers[_controllers.Count - 1].RectTransform.anchoredPosition.y;
    }
}
