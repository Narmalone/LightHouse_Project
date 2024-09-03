using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] private CustomEvent _onComputerEnter;
    [SerializeField] private CustomEvent _onStorageEnter;

    [SerializeField] private CustomEvent _onComputerLeave;
    [SerializeField] private CustomEvent _onStorageLeave;

    protected override void Awake()
    {
        base.Awake();

        _onComputerEnter.handle += ReleaseCursor;
        _onStorageEnter.handle += ReleaseCursor;

        _onComputerLeave.handle += LockCursor;
        _onStorageLeave.handle += LockCursor;
    }

    private void OnDestroy()
    {
        _onComputerEnter.handle -= ReleaseCursor;
        _onStorageEnter.handle -= ReleaseCursor;

        _onComputerLeave.handle -= LockCursor;
        _onStorageLeave.handle -= LockCursor;
    }

    private void Start()
    {
        LockCursor();
    }

    public void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
