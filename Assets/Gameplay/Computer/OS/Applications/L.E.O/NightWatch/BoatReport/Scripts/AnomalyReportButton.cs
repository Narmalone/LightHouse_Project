using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnomalyReportButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _reportText;

    public Button AnomalyButton => _button;
    public TextMeshProUGUI AnomalyText => _reportText;
}
