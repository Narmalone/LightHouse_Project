using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OS : TabCanvas
{
    public List<ComputerApp> apps = new List<ComputerApp>();
    public List<ShortCutController> ShortCuts = new List<ShortCutController>();
    [SerializeField] private RectTransform _runningAppsParent;

    public RectTransform RunningAppParent => _runningAppsParent;

    private void OnValidate()
    {
        ShortCuts = GetComponentsInChildren<ShortCutController>().ToList();
    }

    private void Awake()
    {
        foreach (var shortcut in ShortCuts) 
        {
            shortcut.Initialize(this);
        }
    }
}
