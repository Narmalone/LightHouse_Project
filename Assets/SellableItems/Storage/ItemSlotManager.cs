using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotManager : MonoBehaviour
{
    public CanvasGroup MainGroup;
    [SerializeField] private ItemSlotController _slotPrefab;
    [SerializeField] private List<ItemSlotController> _controllers = new List<ItemSlotController>();
    [SerializeField] private GridLayoutGroup _grid;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private TextMeshProUGUI _noOneItemText;

    public void AddItem(string objName, int objPrice)
    {
        if(_controllers.Count <= 0)
        {
            _noOneItemText.gameObject.SetActive(false);
        }
        Debug.Log(_grid);
        ItemSlotController newItem = Instantiate(_slotPrefab, _grid.transform);
        _controllers.Add(newItem);
        //newItem.SetItem(objName, objPrice);
        UpdateContentSize();
    }

    public void RemoveItem()
    {

        //à mettre tout à la fin quand l'objet a été remove
        if (_controllers.Count >= 0)
        {
            _noOneItemText.gameObject.SetActive(true);
        }
    }

    private void UpdateContentSize()
    {
        RectTransform rectTransformContent = _scrollRect.content;
        rectTransformContent.sizeDelta = new Vector2(0f, Mathf.Abs(GetMostDownControllerValue()) + _grid.spacing.y);
    }

    public float GetMostDownControllerValue()
    {
        if (_controllers.Count <= 0) return 0f;
        return _controllers[_controllers.Count - 1].RectTransform.anchoredPosition.y;
    }
}
