using UnityEngine;

public class PlayerPhysicsInteraction : MonoBehaviour
{
    [Header("Physics Interaction Settings")]
    public float basePushForce = 5f;
    public float playerMass = 70f;
    public LayerMask pushableLayer;
    public float maxPushAngle = 45f;
    [Range(-1, 1f)] private float _pushFromTopLimit = 0.5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic || (pushableLayer != (pushableLayer | (1 << hit.gameObject.layer))))
            return;

        float angle = Vector3.Angle(hit.normal, Vector3.up); //get l'angle entre la normale touchée et le up

        //vérifier pour savoir si on peut pousser 
        if (angle > maxPushAngle)
            return;

        // Empêcher la poussée si on touche le dessus de l'objet
        if (hit.normal.y > 0.5f)
            return;

        // Utiliser la position du point d'impact plutôt que le centre de l'objet
        Vector3 forceDirection = hit.point - transform.position;
        forceDirection.y = 0;
        forceDirection.Normalize();

        // Ajuster la force pour ne pas être trop faible sur objets lourds
        float massRatio = Mathf.Clamp01(playerMass / (rb.mass + 1f));
        float appliedForce = basePushForce * massRatio;

        rb.AddForce(forceDirection * appliedForce, ForceMode.Impulse);
    }
}
