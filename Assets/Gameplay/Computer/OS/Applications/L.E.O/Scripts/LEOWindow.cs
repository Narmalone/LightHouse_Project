using LightHouse.Game.Computer.OS;
using UnityEngine;

public abstract class LEOWindow : MonoBehaviour, ILEOWindow
{
    [SerializeField] private ELEOWindow _type;
    [SerializeField] private CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup => _canvasGroup;
    public ELEOWindow Type => _type;
    public OS OSSystem { get; set; }

    public virtual void Open()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
