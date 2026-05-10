using TMPro;
using UnityEngine;

public class InfoPanelMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _difficultyText; //Can be randomn, easy...
    [SerializeField] private RectTransform _infosContainer; //Container for the info texts
    [SerializeField] private TextMeshProUGUI _textInfosPrefab; //Prefab
    [SerializeField] private CanvasGroup _canvasGroup; //Canvas group for fade in/out

    private bool _isVisible = false;

    private void Start()
    {
        Hide();
    }

    public void Initialize(string[] configTexts)
    {
        foreach (string text in configTexts)
        {
            TextMeshProUGUI infoText = Instantiate(_textInfosPrefab, _infosContainer);
            infoText.text = text;
        }

        if(!_isVisible)
            Show();
    }

    public void Show()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _isVisible = true;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _isVisible = false;
    }
}
