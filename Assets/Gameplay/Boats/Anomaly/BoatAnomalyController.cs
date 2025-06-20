using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BoatAnomalyController : MonoBehaviour
{
    private List<BoatAnomaly> activeAnomalies = new();

    public void AddAnomaly(BoatAnomaly anomalyPrefab)
    {
        BoatAnomaly anomaly = Instantiate(anomalyPrefab, transform);
        anomaly.Initialize(GetComponent<Boat>());
        activeAnomalies.Add(anomaly);
    }

    public void RemoveAnomaly(BoatAnomaly anomaly)
    {
        anomaly.Resolve(GetComponent<Boat>());
        activeAnomalies.Remove(anomaly);
        Destroy(anomaly);
    }

    public IReadOnlyList<BoatAnomaly> GetActiveAnomalies() => activeAnomalies;
}
