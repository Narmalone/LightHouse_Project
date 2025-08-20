using LightHouse.Game.Boats;
using TMPro;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.NightWatch.Boats
{
    public class UI_BoatsLowestTimerAlert : MonoBehaviour
    {
        [SerializeField] private BoatAnomaliesDatabase _database;
        [SerializeField] private TextMeshProUGUI _timerText;

        private void Start()
        {
            // On cache par défaut si aucune anomalie
            _timerText.text = 0f.ToString("00:00");
        }

        private void Update()
        {
            // 1) On fait enfin décrémenter les timers DANS la database
            _database.TickTimers(Time.deltaTime);

            // 2) On récupère la liste active
            var anomalies = _database.GetAnomalies();
            if (anomalies.Count == 0)
            {
                _timerText.text = 0f.ToString("00:00");
                return;
            }

            // 3) On affiche toujours la première anomalie (la plus “vieille” si ta liste est ordonnée par insertion)
            var first = anomalies[0];
            float remaining = first.RemainingTime;
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);

            _timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}


