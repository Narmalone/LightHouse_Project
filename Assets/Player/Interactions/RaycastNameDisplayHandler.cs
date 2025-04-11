using LightHouse.Interactions;
using UnityEngine;

public class RaycastNameDisplayHandler
{
    [SerializeField] private CanvasInteraction _interactionCanvas;

    private IItemName currentItem;

    public bool HasTarget => currentItem != null;

    public RaycastNameDisplayHandler(CanvasInteraction interactionCanva)
    {
        _interactionCanvas = interactionCanva;
    }

    public void SetTarget(IItemName item)
    {
        if (currentItem != null)
        {
            currentItem.OnNameUpdated -= UpdateName;
            currentItem.IsItemRaycasted = false;
        }

        currentItem = item;

        if (currentItem == null)
        {
            _interactionCanvas.HideItemName();
            return;
        }

        currentItem.IsItemRaycasted = true;
        currentItem.OnNameUpdated += UpdateName;
        UpdateName();
    }

    private void UpdateName()
    {
        if (currentItem == null) return;

        if (!currentItem.CanBeRaycasted || string.IsNullOrEmpty(currentItem.GetName()))
        {
            _interactionCanvas.HideItemName();
        }
        else
        {
            _interactionCanvas.ItemName_TMP.text = currentItem.GetName();
            _interactionCanvas.ShowItemName();
        }
    }
}
