using System;
using UnityEngine;

public class LineRendererPath : MonoBehaviour
{
    [SerializeField] private Transform[] _linePos;
    
    public Transform[] LinePos
    {
        get => _linePos;
        set => _linePos = value;
    }

}
