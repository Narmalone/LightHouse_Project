using UnityEngine;

public class FireAnomaly : BoatAnomaly
{
    public ParticleSystem fireEffect;

    public override AnomalyType Type => AnomalyType.Fire;

    public override void Apply()
    {
        fireEffect.Play();
    }

    public override void Resolve()
    {
        if (fireEffect != null)
            Destroy(fireEffect.gameObject);
    }
}
