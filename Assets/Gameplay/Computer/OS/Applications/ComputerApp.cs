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
    [SerializeField] protected Button _closeButton;
    public bool IsMinimized { get; private set; }
    public abstract void OnOpen();
    public abstract void OnClose();
    public abstract void OnMinimize();

    protected virtual void Awake()
    {
        _closeButton.onClick.AddListener(OnClose);
    }

    protected virtual void OnDestroy()
    {
        _closeButton.onClick.RemoveListener(OnClose);
    }

    public void ToggleMinimize()
    {
        IsMinimized = !IsMinimized;
        OnMinimize();
    }
}
