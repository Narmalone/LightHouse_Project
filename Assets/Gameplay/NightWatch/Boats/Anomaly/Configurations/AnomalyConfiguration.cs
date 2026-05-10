using UnityEngine;
using System;
using System.Collections.Generic;
using LightHouse.Features.Weather;

namespace LightHouse.Features.Boats.Anomalies
{
    public enum AnomalyType
    {
        FireAboard,
        MedicalEmergency,
        HostileBehaviour,
        DamagedBoat,
        FuelLeak,
        LightsOutage,
        Quarantine
    }

    [CreateAssetMenu(fileName = "SO_Boats_AnomalyConfig_", menuName = GlobalAssetsMenuPaths.BoatsAssetsMenuPath + "New Anomaly Config")]
    public class AnomalyConfiguration : ScriptableObject
    {
        [Header("Configuration météo")]
        public WeatherType weatherType;

        [Header("Anomalies et leurs chances d’apparition")]
        public List<AnomalyChance> anomalies = new();

        public AnomalyType? PickRandomAnomaly()
        {
            float total = 0f;
            foreach (var anomaly in anomalies)
                total += anomaly.probability;

            float pick = UnityEngine.Random.Range(0f, total);
            float sum = 0f;

            foreach (var anomaly in anomalies)
            {
                sum += anomaly.probability;
                if (pick <= sum)
                    return anomaly.anomaly;
            }

            return AnomalyType.FireAboard;
        }

        [Serializable]
        public class AnomalyChance
        {
            public AnomalyType anomaly;
            [Range(0f, 1f)]
            public float probability = 0.1f;
        }
    }
   
}

