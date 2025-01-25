using UnityEngine;
using UnityEngine.UI;

public class UiComputerController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _mainCanvasGroup;

    public TopBarController TopBarController => _topBarController;
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

    private ContentWindow lastSelectedTab;

    public ComputerTabs LastOpenedTab = ComputerTabs.None;

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
        messagerieWindow.Hide();
        shopWindow.Hide();
        meteoWindow.Hide();
        nightVeil.Hide();
        radarWindow.Hide();
    }

    private void _onTabCliqued_handle(ComputerTabs obj)
    {
        SwitchTab(obj);
    }

    public void SwitchTab(ComputerTabs target)
    {
        LastOpenedTab = target;
        lastSelectedTab?.Hide();
        lastSelectedTab = GetWindowByEnum(target);
        lastSelectedTab?.Show();
        _topBarController.SwitchSelected(target);
    }

    public void Show()
    {
        _mainCanvasGroup.alpha = 1;
        _mainCanvasGroup.interactable = true;
        _mainCanvasGroup.blocksRaycasts = true;
        _mainCanvasGroup.ignoreParentGroups = true;
    }

    public void Hide()
    {
        _mainCanvasGroup.alpha = 0;
        _mainCanvasGroup.interactable = false;
        _mainCanvasGroup.blocksRaycasts = false;
        _mainCanvasGroup.ignoreParentGroups = false;
    }

    public void OnComputerEnter()
    {
        this.Show();
        if(lastSelectedTab != null)
        {
            lastSelectedTab.Show();
        }
        else
        {
            this.SwitchTab(ComputerTabs.Messagerie);
        }
    }
    public void OnComputerLeave()
    {
        this.Hide();
        lastSelectedTab.Hide();
    }

    public ContentWindow GetWindowByEnum(ComputerTabs target)
    {
        switch (target)
        {
            case ComputerTabs.None:
                return null;
            case ComputerTabs.Messagerie:
                return messagerieWindow;
            case ComputerTabs.Meteo:
                return meteoWindow;
            case ComputerTabs.VeilleDeNuit:
                return nightVeil;
            case ComputerTabs.Maintenance:
                return radarWindow;
            case ComputerTabs.Ravitaillement:
                return shopWindow;
        }
        return null;
    }
}
