using UnityEngine;
using UnityEngine.UI;

public class UiComputerController : MonoBehaviour
{
    [SerializeField] private ContentWindow[] _allWindows;
    [SerializeField] private TabBtnDisplay[] mainOnglets;

    [SerializeField] private ShopContent shopWindow;
    [SerializeField] private MeteoContent meteoWindow;
    [SerializeField] private QuestContent questWindow;
    [SerializeField] private IslandContent islandWindow;
    [SerializeField] private RadarContent radarWindow;
    [SerializeField] private TabBtnDisplay _leaveButton;//

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent _onLeftButtonCliqued;

    private ContentWindow currentWindow = null;

    private TabBtnDisplay lastCliqued;

    private void Awake()
    {
        InitTabBtns();
        InitComputer();
    }


    private void InitTabBtns()
    {
        for (int i = 0; i < mainOnglets.Length; i++)
        {
            mainOnglets[i].OnTabClicked += (nexTab) =>
            {
                SwitchButtonTab(nexTab);
                SwitchTab(nexTab.TabToDisplay);
            };
        }
        _leaveButton.OnTabClicked += ((x) =>
        {
            _onLeftButtonCliqued?.Raise();
        });
    }

    private void InitComputer()
    {
        SwitchTab(ComputerTabs.Meteo);
        SwitchButtonTab(mainOnglets[0]);
    }

    private void SwitchButtonTab(TabBtnDisplay nextTab)
    {
        if (lastCliqued != null)
        {
            lastCliqued.Unselect();
        }
        lastCliqued = nextTab;
        lastCliqued.Select();
    }

    public void SwitchTab(ComputerTabs nextTab)
    {
        if (currentWindow != null)
        {
            currentWindow.Hide();
        }
        switch (nextTab)
        {
            case ComputerTabs.Meteo:
                currentWindow = meteoWindow;
                break;
            case ComputerTabs.Shop:
                currentWindow = shopWindow;
                break;
            case ComputerTabs.Quest:
                currentWindow = questWindow;
                break;
            case ComputerTabs.IslandInfos:
                currentWindow = islandWindow;
                break;
            case ComputerTabs.Radar:
                currentWindow = radarWindow;
                break;
        }
        currentWindow?.Show();
    }
}
