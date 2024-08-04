using TMPro;
using UnityEngine;

public class PlayerInteractionUI : MonoBehaviour
{
    [SerializeField] private Transform _crosshair;
    [SerializeField] private TextMeshProUGUI _nameObject;
    [SerializeField] private CustomEvent_IItem _DisplaySelection;
    [SerializeField] private CustomEvent _HideSelection;

    private void Awake()
    {
        _DisplaySelection.handle += DisplaySelection;
        _HideSelection.handle += HideSelection;
    }

    private void Start()
    {
        HideSelection();
    }

    private void OnDestroy()
    {
        _DisplaySelection.handle -= DisplaySelection;
        _HideSelection.handle -= HideSelection;
    }

    private void DisplaySelection(IItem item)
    {
        _nameObject.text = item.ItemDatas.itemName;
        _crosshair.localScale = Vector3.one * 2;
    }

    private void HideSelection()
    {
        _nameObject.text = string.Empty;
        _crosshair.localScale = Vector3.one;
        
    }
}
