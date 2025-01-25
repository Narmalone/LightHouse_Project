using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ContentWindow : MonoBehaviour
{
    [SerializeField] protected CanvasGroup _canvasGroup;
    public bool IsShowed = false;
    [field: SerializeField] public ComputerTabs ComputerTabs { get; private set; }

    public void Show()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.ignoreParentGroups = true;
        _canvasGroup.interactable = true;
        IsShowed = true;
        OnShow();
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.ignoreParentGroups = false;
        _canvasGroup.interactable = false;
        IsShowed = false;
        OnHide();
    }

    protected virtual void OnShow(){ }

    protected virtual void OnHide() { }
}
