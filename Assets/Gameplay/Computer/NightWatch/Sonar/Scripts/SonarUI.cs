using LightHouse.Game.Computer.NightWatch.Sonar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SonarUI : MonoBehaviour
{
    [Header("Références UI")]
    public Sonar _sonar;                          // Référence au script de détection
    [SerializeField] private RectTransform _sonnarPannel;             // UI circulaire représentant le radar
    [SerializeField] private SonarDotController _sonarDotPrefab;            // Prefab représentant un bateau

    [SerializeField] private bool UpdateRadarRoutineEnabled = true;
    [SerializeField] private float SonarDelay = 5.0f;

    [Header("Effets visuels")]
    [SerializeField] private RectTransform radarSweepLine;
    [SerializeField] private Material SweepLineShader;
    [SerializeField] private RectTransform radarPingFX;
    [SerializeField] private float sweepRotationSpeed = 72f; // degrés par seconde (360° en 5s)
    [SerializeField] private float pingDuration = 0.5f;


    private readonly Dictionary<string, RectTransform> _activeDots = new();

    void Start()
    {
        StartCoroutine(UpdateRadarRoutine());
    }

    IEnumerator UpdateRadarRoutine()
    {
        var wait = new WaitForSeconds(SonarDelay);
        while (UpdateRadarRoutineEnabled)
        {
            UpdateRadar();
            yield return wait;
        }
    }

    private float sweepAngle = 0f;
    private void Update()
    {
        if (SweepLineShader != null)
        {
            // Incrémenter manuellement l'angle
            sweepAngle += sweepRotationSpeed * Time.deltaTime;
            sweepAngle %= 360f;

            // Envoyer au shader
            SweepLineShader.SetFloat("_SweepAngle", sweepAngle);
        }
    }


    private void UpdateRadar()
    {
        float panelRadius = Mathf.Min(_sonnarPannel.rect.width, _sonnarPannel.rect.height) / 2f;
        Vector3 sonarPosition = _sonar.transform.position;
        Quaternion inverseRotation = Quaternion.Inverse(_sonar.transform.rotation);

        // Réutiliser une liste statique pour éviter des allocations
        var detectedThisFrame = new HashSet<string>();

        // Phase 1 : Ajout ou mise à jour des dots actifs
        foreach (var item in SonarManager.SonarItems)
        {
            if (!item.IsDetectedBySonar)
                continue;

            detectedThisFrame.Add(item.Name);

            Vector3 offset = item.Position - sonarPosition;
            Vector2 flatOffset = new Vector2(offset.x, offset.z);
            Vector2 localPos = inverseRotation * flatOffset;

            Vector2 uiPos = localPos / _sonar.DetectionRange * panelRadius;
            if (uiPos.sqrMagnitude > panelRadius * panelRadius)
                uiPos = uiPos.normalized * panelRadius;

            if (!_activeDots.TryGetValue(item.Name, out RectTransform dot))
            {
                var dotInstance = Instantiate(_sonarDotPrefab, _sonnarPannel);
                dotInstance.SetDotColor(item.DotColor); // couleur personnalisée
                dotInstance.SetDotSize(item.DotSize);
                dot = dotInstance.RectTransform;
                _activeDots[item.Name] = dot;
            }

            dot.anchoredPosition = uiPos;
        }

        // Phase 2 : Nettoyage des anciens dots
        var keysToRemove = new List<string>();
        foreach (var kvp in _activeDots)
        {
            if (!detectedThisFrame.Contains(kvp.Key))
            {
                Destroy(kvp.Value.gameObject);
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _activeDots.Remove(key);
        }

        StartCoroutine(AnimatePing());
    }


    IEnumerator AnimatePing()
    {
        if (radarPingFX == null)
            yield break;

        radarPingFX.gameObject.SetActive(true);

        // Reset couleur
        Image pingImage = radarPingFX.GetComponent<Image>();
        Color startColor = pingImage.color;
        startColor.a = 0.4f;
        pingImage.color = startColor;

        // Taille initiale et finale
        float t = 0f;
        float pingStartSize = 100f; // pixels (par ex, commence petit)
        float pingMaxSize = 1600f;  // dépasse largement le radar (dépend de ton canvas)

        while (t < pingDuration)
        {
            t += Time.deltaTime;
            float progress = t / pingDuration;

            // Taille croissante
            float size = Mathf.Lerp(pingStartSize, pingMaxSize, progress);
            radarPingFX.sizeDelta = new Vector2(size, size);

            // Alpha décroissant
            Color c = pingImage.color;
            c.a = Mathf.Lerp(0.4f, 0f, progress);
            pingImage.color = c;

            yield return null;
        }

        radarPingFX.gameObject.SetActive(false);
    }



}
