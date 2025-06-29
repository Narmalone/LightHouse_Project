using UnityEngine;

public class BoatFloatController : MonoBehaviour
{
    [SerializeField] private Transform _mesh;
    [Header("Floaters (world space)")]
    public Transform frontLeft;
    public Transform frontRight;
    public Transform backLeft;
    public Transform backRight;

    [Header("Settings")]
    public Vector3 localOffset = Vector3.zero;

    void Update()
    {
        Vector3 pFL = frontLeft.position;
        Vector3 pFR = frontRight.position;
        Vector3 pBL = backLeft.position;
        Vector3 pBR = backRight.position;

        Vector3 averagePosition = (pFL + pFR + pBL + pBR) / 4f;
        averagePosition += _mesh.transform.rotation * localOffset;
        _mesh.transform.position = averagePosition;

        // Directions
        Vector3 forwardDir = ((pFL + pFR) * 0.5f) - ((pBL + pBR) * 0.5f);
        Vector3 rightDir = ((pFR + pBR) * 0.5f) - ((pFL + pBL) * 0.5f);

        // Sécurité anti-zero
        if (forwardDir.sqrMagnitude < 0.001f || rightDir.sqrMagnitude < 0.001f)
            return;

        // Normale vers le haut
        Vector3 surfaceNormal = Vector3.Cross(forwardDir, rightDir).normalized;

        // Vérifie si la normale est vers le haut (et pas inversée)
        if (Vector3.Dot(surfaceNormal, Vector3.up) < 0)
            surfaceNormal = -surfaceNormal;

        // Appliquer la rotation
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(forwardDir, surfaceNormal), surfaceNormal);
        _mesh.transform.rotation = targetRotation;
    }
}
