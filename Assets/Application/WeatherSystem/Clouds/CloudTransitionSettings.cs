using UnityEngine;

[System.Serializable]
public struct CloudTransition
{
    public CloudSettings Clear;
    public CloudSettings Stormy;
    public AnimationCurve InfluenceCurve; // courbe pour affiner la transition selon l'humidité, pression, etc.
}
