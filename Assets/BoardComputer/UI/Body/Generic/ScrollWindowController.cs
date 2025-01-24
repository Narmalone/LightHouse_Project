using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollWindowController : MonoBehaviour
{
    [SerializeField] protected RectTransform ThisTransform;
    [SerializeField] public RectTransform Content;

    public Scrollbar ScrollBar;
    public ScrollRect ScrollRect;
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