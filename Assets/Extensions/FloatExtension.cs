using System;
using System.Collections;
using System.Collections.Generic;
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
}
