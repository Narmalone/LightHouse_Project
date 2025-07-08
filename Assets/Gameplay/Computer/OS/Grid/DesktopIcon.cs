using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class DesktopIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ComputerApp CurrentApplication;
    public Image Background;
    public Color HoverColor;
    public CanvasGroup CanvasGroup;
    public bool IsSelected = false;

    private void Awake()
    {
        //CurrentApplication = GetComponentInChildren<ComputerApp>();
    }

    private void OnValidate()
    {
        if(CanvasGroup == null)
            CanvasGroup = GetComponent<CanvasGroup>();

        if(Background == null)
            Background = GetComponent<Image>();
    }

    public void Show()
    {
        CanvasGroup.alpha = 1;
    }

    public void Hide()
    {
        CanvasGroup.alpha = 0f;
        CanvasGroup.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Background.color = HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Background.color = new Color(1,1,1,0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = !IsSelected;
    }
}
