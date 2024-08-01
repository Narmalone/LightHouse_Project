using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private ContentWindow[] _allWindows;
    [SerializeField] private TabBtnDisplay[] mainOnglets;

    [SerializeField] private ShopContent shopWindow;
    [SerializeField] private MeteoContent meteoWindow;
    [SerializeField] private QuestContent questWindow;
    [SerializeField] private IslandContent islandWindow;
    [SerializeField] private RadarContent radarWindow;

    private ContentWindow currentWindow = null;
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
                SwitchTab(nexTab);
            };
        }
    }

    private void InitComputer()
    {
        SwitchTab(ComputerTabs.Meteo);
    }

    public void OpenComputer()
    {

    }

    public void SwitchTab(ComputerTabs nextTab)
    {
        if(currentWindow != null)
        {
            currentWindow.Hide();
        }
        switch(nextTab)
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

public enum ComputerTabs
{
    Current,
    Meteo,
    Shop,
    Quest,
    IslandInfos,
    Radar
}