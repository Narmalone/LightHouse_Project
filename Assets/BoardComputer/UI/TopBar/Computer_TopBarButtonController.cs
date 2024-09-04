using NUnit.Framework.Internal;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Computer_TopBarButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button btn;
    [SerializeField] private RectTransform thisRect;
    private RectTransform rectParent;

    public bool IsSelected = false;
    public Button Button => btn;

    public ComputerTabs TabDisplay;

    [SerializeField] private CustomEvent_ComputerTopBarButton _onTabCliqued;

    public Color Selected;
    public Color UnSelected;

    private Vector2 initDeltaSize;

    private void Awake()
    {
        btn.onClick.AddListener(() =>
        {
            if (IsSelected) return;
            _onTabCliqued?.Raise(this.TabDisplay);
        });

        initDeltaSize = thisRect.sizeDelta;
    }

    public void SetTransformParent(RectTransform rectParent)
    {
        this.rectParent = rectParent;
    }

    public void Unselect()
    {
        IsSelected = false;
        btn.targetGraphic.color = UnSelected;
        thisRect.sizeDelta = initDeltaSize;
    }

    public void Select()
    {
        btn.targetGraphic.color = Selected;
        thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, rectParent.rect.height);
        IsSelected = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorType.ComputerClick);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorType.ComputerDefault);
    }
}
