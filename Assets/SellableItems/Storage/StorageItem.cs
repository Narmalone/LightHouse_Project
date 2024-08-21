using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageItem : ItemBase
{
    public override string Name { get => "Open"; set => base.Name = value; }
    public override void Use()
    {
        base.Use();

    }

    public virtual void OpenStorage()
    {

    }

    public virtual void CloseStorage()
    {

    }
}
