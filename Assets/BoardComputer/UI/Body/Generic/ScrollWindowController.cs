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

    [SerializeField] protected float UpPower = 1f;
    [SerializeField] protected float DownPower = 1f;

    public void UpdateContentTransform()
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
}