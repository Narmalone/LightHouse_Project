using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class TalkieBackgroundAutoSizer : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float maxWidth = 700f;
    public float padding = 40f;
    public float smoothSpeed = 10f;

    private RectTransform self;

    private void Awake()
    {
        self = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (targetText == null) return;

      /*  // Largeur du texte visible en cours (lettres déjà affichées)
        float textWidth = targetText.preferredWidth + padding;
        float clampedWidth = Mathf.Min(textWidth, maxWidth);
        float newWidth = Mathf.Lerp(self.sizeDelta.x, clampedWidth, Time.deltaTime * smoothSpeed);

        self.sizeDelta = new Vector2(newWidth, self.sizeDelta.y);*/
    }
}
