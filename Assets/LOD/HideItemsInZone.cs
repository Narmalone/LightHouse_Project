using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideItemsInZone : MonoBehaviour
{
    [SerializeField] private Renderer[] _renderers;

    public void EnableRenderers()
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = false;
        }
    }

    public void DisableRenderers()
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = true;
        }
    }
   
}
