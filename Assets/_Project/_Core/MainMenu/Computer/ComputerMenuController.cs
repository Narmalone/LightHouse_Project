using Cinemachine;
using System;
using UnityEngine;

public class ComputerMenuController : MonoBehaviour, IRaycastable
{
    [SerializeField] private CinemachineVirtualCamera _computerCam;

    public void OnClicked()
    {
        EnterInComputerRoutine();
    }

    public void OnClickReleased() { }

    public void OnRaycastEnter() { }

    public void OnRaycastLeave() { }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _computerCam.Priority = -1;
    }

    private void EnterInComputerRoutine()
    {
        _computerCam.Priority = 100;
    }

    private void ExitComputerRoutine()
    {
        _computerCam.Priority = -1;
    }
}
