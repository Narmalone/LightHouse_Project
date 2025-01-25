using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [HideInInspector] public Computer_TopBarButtonController LastCliquedTab;

    public Computer_TopBarButtonController MessagerieTopBar => _messagerie;
    public Computer_TopBarButtonController MeteoTopBar => _meteo;
    public Computer_TopBarButtonController NightVeilTopBar => _veilleDeNuit;
    public Computer_TopBarButtonController RefundTopBar => _ravitaillement;
    public Computer_TopBarButtonController MaintenanceTopBar => _maintenance;

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
    public ComputerTabs lastSelected;

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
        SwitchSelected(obj);
    }

    public void SwitchSelected(ComputerTabs target)
    {
        if (lastSelected != ComputerTabs.None)
        {
            UnSelect(lastSelected);
        }
        Select(target);
        lastSelected = target;
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

    private void Select(ComputerTabs target)
    {
        switch (target)
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

    private void UnSelect(ComputerTabs target)
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
