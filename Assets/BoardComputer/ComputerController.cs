using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerController : ItemBase
{
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private BoxCollider _itemCollider;
    [SerializeField] private UiComputerController _uiComputerController;

    [SerializeField] private string _interactName = "Enter";
    public override string Name { get => _interactName; set => _interactName = value; }

    public override void Use()
    {
        base.Use();
        OpenComputer();
    }

    public void OpenComputer()
    {
        _mainCanvasGroup.alpha = 1f;
        _itemCollider.enabled = false;
    }
}

public enum ComputerTabs
{
    Current,
    Meteo,
    Shop,
    Quest,
    IslandInfos,
    Radar
}