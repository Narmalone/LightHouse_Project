using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabBtnDisplay : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private Image _underLine;
    public Button Button => btn;

    public ComputerTabs TabToDisplay;

    public event Action<TabBtnDisplay> OnTabClicked;

    private void Awake()
    {
        btn.onClick.AddListener(() =>
        {
            OnTabClicked?.Invoke(this);
        });
    }

    public void Unselect()
    {
        _underLine.gameObject.SetActive(false);
    }

    public void Select()
    {
        _underLine.gameObject.SetActive(true);
    }
}
