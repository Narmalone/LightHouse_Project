using UnityEngine;

public abstract class BoatAnomaly : MonoBehaviour
{
    public abstract AnomalyType Type { get; }
    public float Severity = 1f; // utile pour priorité ou alerte
    protected Boat _boat;
    public virtual void Initialize(Boat boat) 
    {
        _boat = boat;
    }
    public abstract void Apply();
    public abstract void Resolve();
}
