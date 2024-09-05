using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ComputerClickableElement : EnterExitUiEvent
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if(CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor(CursorType.ComputerClick);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor(CursorType.ComputerDefault);
        }
    }
}
