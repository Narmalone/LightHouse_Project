using LightHouse.Features.Boats.Anomalies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Computer.LEO.NightWatch.Boats
{
    public class AnomalyReportButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _reportText;
        public BoatAnomalyDefinition AnomalyDefinition;

        public Button AnomalyButton => _button;
        public TextMeshProUGUI AnomalyText => _reportText;

        private void OnValidate()
        {
            if (AnomalyDefinition != null)
                _reportText.text = AnomalyDefinition.DisplayName;
        }
    }

}
