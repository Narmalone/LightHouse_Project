using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace LightHouse.Features.Boats.Frequencies
{
    [CreateAssetMenu(fileName = "BoatFrequencyAllocator", menuName = "LightHouse/Boats/Frequency Allocator")]
    public class BoatFrequencyAllocator : ScriptableObject
    {
        [Header("Plage (MHz)")]
        [SerializeField] private float minFrequencyMHz = 157f;
        [SerializeField] private float maxFrequencyMHz = 162f;

        [Tooltip("Pas en MHz (0.01 = 10 kHz, 0.05 = 50 kHz, etc.)")]
        [SerializeField] private float frequencyStepMHz = 0.01f;

        // On stocke des floats, mais TOUJOURS quantifiés à 2 décimales
        private readonly HashSet<float> _usedFrequenciesMHz = new();

        /// <summary>Quantifie à 2 décimales (paliers de 0.01 MHz).</summary>
        private static float Quantize2(float valueMHz) => Mathf.Round(valueMHz * 100f) * 0.01f;

        private void OnEnable() => _usedFrequenciesMHz.Clear();

        /// <summary>Alloue une fréquence unique (MHz), quantifiée à 2 décimales.</summary>
        public float AllocateUniqueFrequencyMHz()
        {
            float start = Quantize2(minFrequencyMHz);
            float end = Quantize2(maxFrequencyMHz);
            float step = Mathf.Max(0.01f, Quantize2(frequencyStepMHz)); // min 0.01 MHz

            if (end < start)
            {
                Debug.LogError("[BoatFrequencyAllocator] Plage invalide (max < min).");
                return start;
            }

            int count = Mathf.FloorToInt((end - start) / step) + 1;
            if (count <= 0)
            {
                Debug.LogError("[BoatFrequencyAllocator] Aucune fréquence possible dans la plage.");
                return start;
            }

            // Point de départ aléatoire pour répartir l'usage des fréquences
            int offset = Random.Range(0, count);

            for (int i = 0; i < count; i++)
            {
                int idx = (offset + i) % count;
                float candidate = Quantize2(start + idx * step);

                // Par sécurité (arrondis), on s'assure que le candidat reste dans la plage
                if (candidate < start || candidate > end) continue;

                if (_usedFrequenciesMHz.Add(candidate)) // OK si encore libre
                    return candidate;
            }

            Debug.LogWarning("[BoatFrequencyAllocator] Plus de fréquences disponibles.");
            return start;
        }

        /// <summary>Libère une fréquence précédemment allouée.</summary>
        public void ReleaseFrequencyMHz(float frequencyMHz)
        {
            _usedFrequenciesMHz.Remove(Quantize2(frequencyMHz));
        }

        /// <summary>Formatte pour l'affichage FR avec exactement 2 décimales.</summary>
        public static string FormatFrequencyForDisplay(float frequencyMHz)
        {
            return frequencyMHz.ToString("F2", CultureInfo.GetCultureInfo("fr-FR")) + " MHz";
        }
    }
}
