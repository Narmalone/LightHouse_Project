using UnityEngine;
using UnityEngine.VFX;

public class RainSplashCPUReceiver : MonoBehaviour
{
    public VisualEffect RainVFX;
    public GameObject SplashGroundPrefab;
    public GameObject SplashWaterPrefab;      // optionnel (proxy collider conseillé)
    public LayerMask SplashMask;              // ex: Terrain, Default, WaterProxy
    public float backOffset = 0.05f;          // recule le départ du raycast
    public float rayLen = 1.0f;            // distance max de recherche
    public float throttle = 1.0f;            // 1=100% des events, 0.25=1/4 seulement
    public string WaterTag = "Water";         // si tu tagues ton eau/proxy “Water”

    static readonly int EVT_OnHit = Shader.PropertyToID("OnRainHit"); // nom de l’Event Output

    void OnEnable() { if (RainVFX) RainVFX.outputEventReceived += OnVFXEvent; }
    void OnDisable() { if (RainVFX) RainVFX.outputEventReceived -= OnVFXEvent; }

    void OnVFXEvent(VFXOutputEventArgs e)
    {
        if (e.nameId != EVT_OnHit) return;

        // Throttle CPU si besoin
        if (throttle < 0.999f && Random.value > throttle) return;

        var a = e.eventAttribute;
        Vector3 pos = a.HasVector3("position") ? a.GetVector3("position") : transform.position;
        Vector3 nrm = a.HasVector3("normal") ? a.GetVector3("normal") : Vector3.up;
        Vector3 vel = a.HasVector3("velocity") ? a.GetVector3("velocity") : Vector3.down * 5f;

        // Direction de recherche = direction d’impact (vers le sol)
        Vector3 dir = (-vel.sqrMagnitude > 0.0001f) ? -vel.normalized : Vector3.down;

        // On recule un poil, puis on raycast dans la direction de contact
        Vector3 origin = pos - dir * backOffset;
        Debug.Log(origin);

        if (Physics.Raycast(origin, dir, out var hit, rayLen, SplashMask, QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
            nrm = hit.normal;

            Debug.Log("raycasting");
            // Choix prefab selon surface (eau/sol)
            GameObject prefab = (hit.collider.CompareTag(WaterTag) && SplashWaterPrefab)
                                ? SplashWaterPrefab
                                : SplashGroundPrefab;

            if (prefab)
            {
                // Orientation = tangente au sol (avant = vélocité projetée sur le plan)
                Vector3 forward = Vector3.ProjectOnPlane(vel, nrm);
                if (forward.sqrMagnitude < 1e-6f) forward = Vector3.Cross(nrm, Vector3.right);
                Quaternion rot = Quaternion.LookRotation(forward, nrm);

                var fx = Instantiate(prefab, pos, rot);
                Destroy(fx, 3f); // ou renvoie au pool
            }
        }
        else
        {
            // Pas de hit (ex: vide) → on peut ignorer ou fallback sol générique
        }
    }
}
