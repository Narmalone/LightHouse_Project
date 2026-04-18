using Cinemachine;
using LightHouse.Core.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ComputerMenuController : MonoBehaviour
{
    [SerializeField] private Canvas _choiceCanvas;
    [SerializeField] private Button _enterInComputer;
    [SerializeField] private RaycastableMenuItem _raycastableMenuItem;
    [SerializeField] private CinemachineVirtualCamera _computerCam;

    private void Awake()
    {
        _raycastableMenuItem.OnShowInformationsEvent += RaycastableMenuItem_OnShowInformationsEvent;
        _raycastableMenuItem.OnHideInformationsEvent += RaycastableMenuItem_OnHideInformationsEvent;
        _enterInComputer.onClick.AddListener(EnterInComputerRoutine);
    }

    private void EnterInComputerRoutine()
    {
        _computerCam.SetPriority(100);
    }

    private void ExitComputerRoutine()
    {
        _computerCam.SetPriority(-1);
    }

    private void RaycastableMenuItem_OnHideInformationsEvent()
    {
        _choiceCanvas.gameObject.SetActive(false);
    }

    private void RaycastableMenuItem_OnShowInformationsEvent()
    {
        _choiceCanvas.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _raycastableMenuItem.OnShowInformationsEvent -= RaycastableMenuItem_OnShowInformationsEvent;
        _raycastableMenuItem.OnHideInformationsEvent -= RaycastableMenuItem_OnHideInformationsEvent;
        _enterInComputer.onClick.RemoveListener(EnterInComputerRoutine);
    }
}
