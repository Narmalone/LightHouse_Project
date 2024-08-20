using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabBtnDisplay : MonoBehaviour
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
            Debug.Log("test");
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
}
