using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabBtnDisplay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button btn;
    public Button Button => btn;

    public ComputerTabs TabToDisplay;

    public event Action<TabBtnDisplay> OnTabClicked;

    public Color SelectedColor;
    public Color UnSelectedColor;

    private void Awake()
    {
        btn.onClick.AddListener(() =>
        {
            OnTabClicked?.Invoke(this);
        });
    }

    public void Unselect()
    {
        btn.targetGraphic.color = UnSelectedColor;
    }

    public void Select()
    {
        btn.targetGraphic.color = SelectedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("test");
    }
}
