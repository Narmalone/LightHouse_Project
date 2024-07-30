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

    private ContentWindow currentWindow = null;
    private void Awake()
    {
        InitTabBtns();
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

    public void SwitchTab(ComputerTabs nextTab)
    {
        if(currentWindow != null)
        {
            currentWindow.Hide();
            Debug.Log(currentWindow);
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
                break;
            case ComputerTabs.IslandInfos:
                break;
            case ComputerTabs.Radar:
                break;
        }
        currentWindow.Show();
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