using System;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class ScotchNeeder : IDUseItemTracker
    {
        [Header ("--- SCOTCH NEEDER ---")]
        [SerializeField] private LineRendererPath _lineRendererPath;

        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            
            LineRenderer _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer != null)
            {
                for (int i = 0; i < _lineRendererPath.LinePos.Length; i++)
                {
                    _lineRenderer.SetPosition(i, _lineRendererPath.LinePos[i].position);
                }
                _lineRenderer.enabled = true;
            }
            
        }
    }
}

