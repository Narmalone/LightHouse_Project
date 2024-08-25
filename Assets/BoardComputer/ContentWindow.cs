using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentWindow : MonoBehaviour
{
    [SerializeField] protected CanvasGroup _canvasGroup;

    public void Show()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.ignoreParentGroups = true;
        _canvasGroup.interactable = true;
        OnShow();
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.ignoreParentGroups = false;
        _canvasGroup.interactable = false;
        OnHide();
    }

    public virtual void OnShow(){ }

    public virtual void OnHide() { }
}
