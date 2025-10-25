using LightHouse.Handlers;
using UnityEngine;

/// <summary>
/// Centre ce GameObject au-dessus d'une destination :
/// - Aligne XZ sur la cible
/// - Conserve/force une hauteur (offset Y) au-dessus de la cible
/// - Ne bouge que si la distance planaire dépasse une zone morte
/// - Mouvement lissé (SmoothDamp) sans à-coups
/// </summary>
public class HoverOverTarget : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform à suivre (ex: le joueur). Si null, on utilisera TargetPoint.")]
    public Transform Target;
    [Tooltip("Point monde si aucun Transform n'est fourni.")]
    public Vector3 TargetPoint;

    [Header("Positionnement")]
    [Tooltip("Hauteur au-dessus de la cible (en mètres).")]
    public float HoverHeight = 10f;
    [Tooltip("Zone morte (rayon sur XZ) : en-deçà, on ne bouge pas.")]
    public float DeadZoneRadius = 1f;
    [Tooltip("Snap si on est dans la zone morte (sinon, on laisse la position).")]
    public bool SnapInsideDeadZone = false;

    [Header("Mouvement")]
    [Tooltip("Temps de lissage pour SmoothDamp (0.1–0.5 conseillé).")]
    public float SmoothTime = 0.2f;
    [Tooltip("Vitesse max (m/s) pour le SmoothDamp.")]
    public float MaxSpeed = 50f;

    [Header("Options")]
    [Tooltip("Conserver la hauteur Y actuelle au lieu d'utiliser HoverHeight au-dessus de la cible.")]
    public bool KeepCurrentY = false;
    [Tooltip("Faire pivoter ce GameObject pour regarder la cible (yaw uniquement).")]
    public bool FaceTarget = false;

    // vitesse interne pour SmoothDamp
    private Vector3 _velocity;

    private void Start()
    {
        Target = Camera.main.transform;
    }

    void Update()
    {
        Vector3 targetPos = (Target != null) ? Target.position : TargetPoint;
        TargetPoint = PlayerHandlerData.MainPlayer.Character.transform.position;

        // Position visée : centré au-dessus (XZ aligné), Y = hauteur au-dessus ou Y courant
        float desiredY = KeepCurrentY ? transform.position.y : targetPos.y + HoverHeight;
        Vector3 desired = new Vector3(targetPos.x, desiredY, targetPos.z);

        // Distance sur le plan XZ (on ignore la hauteur pour décider de bouger)
        Vector2 curXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 desXZ = new Vector2(desired.x, desired.z);
        float planarDist = Vector2.Distance(curXZ, desXZ);

        bool shouldMovePlanar = planarDist > DeadZoneRadius;
        bool shouldMoveY = !Mathf.Approximately(transform.position.y, desiredY);

        if (shouldMovePlanar || shouldMoveY)
        {
            // Mouvement lissé et borné en vitesse
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _velocity,
                Mathf.Max(0.0001f, SmoothTime),
                MaxSpeed,
                Application.isPlaying ? Time.deltaTime : 0f // évite les bonds en mode Edit
            );
        }
        else if (SnapInsideDeadZone)
        {
            // Optionnel : snap direct dans la zone morte
            transform.position = desired;
            _velocity = Vector3.zero;
        }

        if (FaceTarget)
        {
            Vector3 lookAt = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            Vector3 dir = (lookAt - transform.position);
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir.normalized, Vector3.up),
                    (Application.isPlaying ? Time.deltaTime : 1f) * 10f
                );
        }
    }

    void OnDrawGizmosSelected()
    {
        // Gizmos : rayon de zone morte sur XZ autour de la cible
        Vector3 center = (Target != null) ? Target.position : TargetPoint;
        Vector3 planeCenter = new Vector3(center.x, KeepCurrentY ? transform.position.y : center.y + HoverHeight, center.z);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(planeCenter, 0.2f);
#if UNITY_EDITOR
        // Cercle XZ
        UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.6f);
        UnityEditor.Handles.DrawWireDisc(planeCenter, Vector3.up, DeadZoneRadius);
#endif
    }
}
