using UnityEngine;
using UnityEngine.UI;

public class UiComputerController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private TopBarController _topBarController;

    [SerializeField] public Messagerie messagerieWindow;
    [SerializeField] public ShopContent shopWindow;
    [SerializeField] public MeteoContent meteoWindow;
    [SerializeField] public NightVeilContent nightVeil;
    [SerializeField] public RadarContent radarWindow;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent _onLeftButtonCliqued;

    [Header("LISTENERS")]
    [SerializeField] private CustomEvent_ComputerTopBarButton _onTabCliqued;

    private ContentWindow currentWindow = null;

    private void Awake()
    {
        _onTabCliqued.handle += _onTabCliqued_handle;
    }

    private void OnDestroy()
    {
        _onTabCliqued.handle -= _onTabCliqued_handle;
    }

    private void Start()
    {
        messagerieWindow.Show();
        shopWindow.Hide();
        meteoWindow.Hide();
        nightVeil.Hide();
        
        radarWindow.Hide();
    }

    private void _onTabCliqued_handle(ComputerTabs obj)
    {
        
    }

    public void SwitchTab(ComputerTabs target)
    {
        _topBarController.SwitchSelected(target);
    }

    public void Show()
    {
        _mainCanvasGroup.alpha = 1;
    }

    public void Hide()
    {
        _mainCanvasGroup.alpha = 0;
    }
}
