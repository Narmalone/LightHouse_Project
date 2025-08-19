using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

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
            return true;

        // On remplace la virgule par point si besoin
        var replaced = s?.Replace(',', '.');
        return float.TryParse(replaced, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
