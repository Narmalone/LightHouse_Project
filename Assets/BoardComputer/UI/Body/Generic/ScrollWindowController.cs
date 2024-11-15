using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollWindowController : MonoBehaviour
{
    [SerializeField] protected RectTransform ThisTransform;
    [SerializeField] public RectTransform Content;
    [SerializeField] protected Button UpButton;
    [SerializeField] protected Button DownButton;
    public Scrollbar ScrollBar;
    public ScrollRect ScrollRect;

    [SerializeField] protected float UpPower = 1f;
    [SerializeField] protected float DownPower = 1f;

    /*public void UpdateContentTransform()
    {
        if (Content.childCount <= 0) return;
        var child = Content.GetChild(Content.childCount - 1).GetComponent<RectTransform>();

        // Récupérez la position et la hauteur de l'élément le plus bas
        float childPosY = child.anchoredPosition.y;
        float childHeight = child.rect.height;

        // Calculez la nouvelle hauteur du Content
        float newContentHeight = Content.rect.height + childHeight;

        // Mettez à jour la hauteur et la position du Content
        Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newContentHeight);
        Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, childPosY - childHeight);
    }
*/

    private void Start()
    {
        UpButton.onClick.AddListener(OnUpCliqued);
        DownButton.onClick.AddListener(OnDownCliqued);
    }

    private void OnDestroy()
    {
        DownButton.onClick.RemoveAllListeners();
        UpButton.onClick.RemoveAllListeners();
    }
    public void UpdateContentTransform()
    {
        if (Content.childCount <= 0) return;

        float totalHeight = 0f;
        for (int i = 0; i < Content.childCount; i++)
        {
            var child = (RectTransform)Content.GetChild(i);
            totalHeight += child.rect.height;
        }

        // Check if the total height exceeds the current content height
        if (totalHeight > Content.rect.height)
        {
            // Update the content height to fit all children
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            /*ScrollBar.Rebuild(CanvasUpdate.MaxUpdateValue);
            ScrollRect.Rebuild(CanvasUpdate.MaxUpdateValue);*/
        }
    }

    private void OnDownCliqued()
    {
        ScrollBar.value -= 0.1f;
    }

    private void OnUpCliqued()
    {
        ScrollBar.value += 0.1f;
    }

    private void OnValidate()
    {
        if(ScrollBar == null)
        {
            ScrollBar = GetComponentInChildren<Scrollbar>();
        }

        if(ScrollRect == null)
        {
            ScrollRect = GetComponent<ScrollRect>();
        }
    }

}