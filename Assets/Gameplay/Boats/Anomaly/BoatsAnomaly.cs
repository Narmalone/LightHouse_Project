using UnityEngine;

public abstract class BoatAnomaly : MonoBehaviour
{
    public string AnomalyName;
    public string Description;
    public float Severity = 1f; // utile pour priorité ou alerte

    public virtual void Initialize(Boat boat) { }
    public virtual void StartAnomaly(Boat boat) { }
    public virtual void UpdateAnomaly(Boat boat) { }
    public virtual void Resolve(Boat boat) { }
}
