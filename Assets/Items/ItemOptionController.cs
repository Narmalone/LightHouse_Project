using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemOptionController : MonoBehaviour
{
    public bool HideOnStart = true;
    [SerializeField] private Button prefabCommand;
    public Button Prefab => prefabCommand;

    public VerticalLayoutGroup buttonLayout;

    public List<Button> controllers = new List<Button>();

    public CanvasGroup canvasGroup;

    [SerializeField] private TextMeshProUGUI itemName;
    public TextMeshProUGUI ItemName => itemName;

    private void Awake()
    {
        if (HideOnStart)
            this.Hide();
    }

    public void Hide() 
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.ignoreParentGroups = false;
    }
    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.ignoreParentGroups = true;
    }

    public Button AddButton()
    {
        var btn = Instantiate(prefabCommand, buttonLayout.transform);
        controllers.Add(btn);
        return btn;
    }

    public void ClearButtons()
    {
        var l = controllers;
        foreach(var btn in l)
        {
            Destroy(btn.gameObject);
        }
        controllers.Clear();
    }
}
