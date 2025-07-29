using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoatAnomalyTimerController : MonoBehaviour
{
    [SerializeField] private BoatAnomaliesDatabase _database;
    [SerializeField] private TextMeshProUGUI _timerText;
    private float anomalyDuration = 60f; // 5 min par défaut

    private List<float> _timers = new List<float>();

    private void Start()
    {
        anomalyDuration = _database.TimeToReportAnomalies * 60f;
        if (_database != null)
        {
            _database.OnAnomalyAdded += HandleAnomalyAdded;
        }
    }

    private void OnDestroy()
    {
        if (_database != null)
        {
            _database.OnAnomalyAdded -= HandleAnomalyAdded;
        }
    }

    private void HandleAnomalyAdded()
    {
        // Ajoute un timer à 5 minutes pour chaque anomalie ajoutée
        _timers.Add(anomalyDuration);
        Debug.Log($"🚩 Nouvelle anomalie ajoutée ! Timers actifs : {_timers.Count}");
    }

    private void Update()
    {
        if (_timers.Count == 0) return;

        // ✅ Décrémenter TOUS les timers
        for (int i = 0; i < _timers.Count; i++)
        {
            _timers[i] -= Time.deltaTime;
        }

        // ✅ Supprimer ceux arrivés à zéro ou moins
        _timers.RemoveAll(t => t <= 0f);

        // ✅ Afficher uniquement le premier timer (le plus vieux)
        if (_timers.Count > 0)
        {
            float remaining = _timers[0];
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);

            _timerText.text = $"{minutes:00}:{seconds:00}";
            //Debug.Log($"⏱️ Premier timer (le plus vieux) : {minutes:00}:{seconds:00}");
        }
    }
}
