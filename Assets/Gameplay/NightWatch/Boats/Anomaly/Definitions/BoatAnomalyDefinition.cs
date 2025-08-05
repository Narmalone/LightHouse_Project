using UnityEngine;

[CreateAssetMenu(menuName = "LightHouse/Boats/Anomaly Definition")]
public class BoatAnomalyDefinition : ScriptableObject
{
    public AnomalyType Type;
    public string AnomalyName;
    public BoatAnomaly Prefab;

    public BoatAnomaly InstantiateAndAttach(Boat boat)
    {
        var instance = GameObject.Instantiate(Prefab, boat.RB.transform);
        instance.Initialize(boat);
        instance.Apply();
        return instance;
    }
}
