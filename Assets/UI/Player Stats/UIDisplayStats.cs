using TMPro;
using UnityEngine;

public class UIDisplayStats : MonoBehaviour
{
    [SerializeField] private CustomEvent_Float _eventUpdate;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private string _prefixe;
    [SerializeField] private string _suffixe;

    private void Awake()
    {
        _eventUpdate.handle += UpdateText;
    }

    private void OnDestroy()
    {
        _eventUpdate.handle -= UpdateText;
    }

    private void UpdateText(float value)
    {
        _text.text = $"{_prefixe}{EditValueText(value)}{_suffixe}";
    }

    protected virtual string EditValueText(float value)
    {
        return value.ToString();
    }
}