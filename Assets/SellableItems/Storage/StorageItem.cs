using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageItem : ItemBase
{
    public override string Name { get => "Open"; set => base.Name = value; }

    [SerializeField] private CustomEvent _lockPlayerMovement;
    [SerializeField] private CustomEvent _unlockPlayerMovement;
    [SerializeField] private CustomEvent _lockPlayerCam;
    [SerializeField] private CustomEvent _unlockPlayerCam;

    public override void Use()
    {
        base.Use();

    }

    public virtual void OpenStorage()
    {
        _lockPlayerMovement?.Raise();
        _lockPlayerCam?.Raise();
    }

    public virtual void CloseStorage()
    {
        _unlockPlayerMovement?.Raise();
        _unlockPlayerCam?.Raise();
    }
}
