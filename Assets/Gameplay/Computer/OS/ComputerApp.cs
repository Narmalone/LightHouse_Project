using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum E_ComputerAppState
{
    None,
    Opened,
    Closed,
    Minimized
}

public abstract class ComputerApp : MonoBehaviour
{
    public E_ComputerAppState State;
    [field: SerializeField] public string AppName { get; protected set; }
    [field: SerializeField] public TextMeshProUGUI AppText { get; protected set; }
    public bool IsMinimized { get; private set; }
    [field: SerializeField] public Image Icon { get; protected set; }

    public abstract void OnOpen();
    public abstract void OnClose();
    public abstract void OnMinimize();

    public void ToggleMinimize()
    {
        IsMinimized = !IsMinimized;
        OnMinimize();
    }
}
