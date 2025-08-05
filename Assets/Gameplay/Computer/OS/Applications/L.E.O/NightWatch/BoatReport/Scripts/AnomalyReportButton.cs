using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnomalyReportButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _reportText;
    public BoatAnomalyDefinition AnomalyDefinition;

    public Button AnomalyButton => _button;
    public TextMeshProUGUI AnomalyText => _reportText;

    private void OnValidate()
    {
        if(AnomalyDefinition != null)
            _reportText.text = AnomalyDefinition.AnomalyName;
    }
}
