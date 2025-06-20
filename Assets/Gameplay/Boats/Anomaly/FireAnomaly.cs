using UnityEngine;

public class FireAnomaly : BoatAnomaly
{
    public ParticleSystem fireEffect;

    public override void Initialize(Boat boat)
    {
        AnomalyName = "Incendie";
        Description = "Un feu s'est déclaré à bord.";
        fireEffect.Play();
    }

    public override void Resolve(Boat boat)
    {
        if (fireEffect != null)
            Destroy(fireEffect.gameObject);
    }
}
