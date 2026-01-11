using System.Globalization;
using UnityEngine;

namespace LightHouse.Core.Extensions
{
    public static class FloatExtension
    {
        public static float CalculateProximityPercent(this float a, float b)
        {
            if (Mathf.Approximately(a, b))
                return 100f;

            float max = Mathf.Max(Mathf.Abs(a), Mathf.Abs(b));
            float diff = Mathf.Abs(a - b);
            float proximityPercent = (1f - diff / max) * 100;
            return Mathf.Clamp(proximityPercent, 0f, 100f);
        }

        /// <summary>
        /// Parse un float en tolérant '.' et ',' comme séparateur décimal.
        /// </summary>
        public static bool TryParse(string s, out float value)
        {
            // On tente d’abord la culture invariante (point)
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                Debug.Log("return true");
                return true;
            }

            // On remplace la virgule par point si besoin
            var replaced = s?.Replace(',', '.');
            Debug.Log(replaced);
            return float.TryParse(replaced, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}