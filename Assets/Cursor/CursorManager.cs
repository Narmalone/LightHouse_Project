using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : Singleton<CursorManager>
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
