using TMPro;
using UnityEngine;

public class AutoSizeText : MonoBehaviour
{
    public TextMeshProUGUI textMesh; // votre TextMesh Pro
    public float minYPosition = 0f; // position Y minimale
    public float maxHeight = 100f; // hauteur maximale
    public RectTransform parentRectTransform; // le RectTransform du parent

    public void AdjustTextSize()
    {
        float preferredHeight = textMesh.preferredHeight;
        float height = Mathf.Min(maxHeight, preferredHeight);

        textMesh.transform.localPosition = new Vector3(textMesh.transform.localPosition.x, minYPosition - height, textMesh.transform.localPosition.z);
        textMesh.rectTransform.sizeDelta = new Vector2(textMesh.rectTransform.sizeDelta.x, height);

        if(height + minYPosition > parentRectTransform.sizeDelta.y)
        {
            parentRectTransform.sizeDelta = new Vector2(parentRectTransform.sizeDelta.x, height + minYPosition);
            parentRectTransform.localPosition = new Vector3(parentRectTransform.localPosition.x, -height / 2f, parentRectTransform.localPosition.z);
        }
    }
}