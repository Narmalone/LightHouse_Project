using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [HideInInspector] public Computer_TopBarButtonController LastCliquedTab;

    [SerializeField] private RectTransform _horizontalLayoutTransform;
    [SerializeField] private Computer_TopBarButtonController[] Buttons;
    [SerializeField] private Computer_TopBarButtonController _messagerie;
    [SerializeField] private Computer_TopBarButtonController _meteo;
    [SerializeField] private Computer_TopBarButtonController _veilleDeNuit;
    [SerializeField] private Computer_TopBarButtonController _maintenance;
    [SerializeField] private Computer_TopBarButtonController _ravitaillement;

    [SerializeField] private CustomEvent_ComputerTopBarButton _onTabButtonCliqued;

    [SerializeField] private Button _leaveButton;
    [SerializeField] private CustomEvent _onLeaveTabCliqued;

    private void Awake()
    {
        InitButtons();

        _onTabButtonCliqued.handle += _onTabButtonCliqued_handle;

        _leaveButton.onClick.AddListener(() =>
        {
            _onLeaveTabCliqued?.Raise();
        });
    }

    private void OnDestroy()
    {
        _onTabButtonCliqued.handle -= _onTabButtonCliqued_handle;
    }

    private void _onTabButtonCliqued_handle(ComputerTabs obj)
    {
        UnSelect(obj);
    }

    private void InitButtons()
    {
        foreach(Computer_TopBarButtonController button in Buttons)
        {
            button.SetTransformParent(_horizontalLayoutTransform);
        }
    }

    public Computer_TopBarButtonController GetTabButtonByID(ComputerTabs target)
    {
        switch (target)
        {
            case ComputerTabs.Messagerie:
                return _messagerie;
            case ComputerTabs.Meteo:
                return _meteo;
            case ComputerTabs.VeilleDeNuit:
                return _veilleDeNuit;

            case ComputerTabs.Maintenance:
                return _maintenance;

            case ComputerTabs.Ravitaillement:
                return _ravitaillement;

        }
        return null;
    }

    public void Select(ComputerTabs target)
    {
        switch(target)
        {
            case ComputerTabs.Messagerie:
                _messagerie.Select();
                break;
            case ComputerTabs.Meteo:
                _meteo.Select();
                break;
            case ComputerTabs.VeilleDeNuit:
                _veilleDeNuit.Select();
                break;
            case ComputerTabs.Maintenance:
                _maintenance.Select();
                break;
            case ComputerTabs.Ravitaillement:
                _ravitaillement.Select();
                break;
        }
    }

    public void UnSelect(ComputerTabs target)
    {
        switch (target)
        {
            case ComputerTabs.Messagerie:
                _messagerie.Unselect();
                break;
            case ComputerTabs.Meteo:
                _meteo.Unselect();
                break;
            case ComputerTabs.VeilleDeNuit:
                _veilleDeNuit.Unselect();
                break;
            case ComputerTabs.Maintenance:
                _maintenance.Unselect();
                break;
            case ComputerTabs.Ravitaillement:
                _ravitaillement.Unselect();
                break;
        }
    }
}
