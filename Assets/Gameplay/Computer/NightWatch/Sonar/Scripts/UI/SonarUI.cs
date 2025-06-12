using LightHouse.Audio;
using LightHouse.Game.Computer.NightWatch.Sonar;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Image _sonarPingImage;
    [SerializeField] private float sweepRotationSpeed = 72f; // degrés par seconde (360° en 5s)
    [SerializeField] private float pingDuration = 0.5f;

    [Header("Sounds")]
    [SerializeField] private AudioSource _sonarPingAudioSource;
    [SerializeField] private EffectAudio _sonarPingClip;

    private float _currentSweepAngle = 0f;
    private readonly Dictionary<int, SonarDotController> _activeDots = new();

    private void OnValidate()
    {
        if(_sonarPingClip != null)
        {
            _sonarPingAudioSource.clip = _sonarPingClip.clips[0];
            _sonarPingAudioSource.spatialBlend = _sonarPingClip._spatialBlend;
            _sonarPingAudioSource.volume = _sonarPingClip.volume;
        }
    }

    private void Awake()
    {
        _sonarPingAudioSource.clip = _sonarPingClip.clips[0];
        _sonarPingAudioSource.spatialBlend = _sonarPingClip._spatialBlend;
        _sonarPingAudioSource.volume = _sonarPingClip.volume;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(UpdateRadarRoutine());
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            StopAllCoroutines();
        }
        if (SweepLineShader != null)
        {
            // Incrémenter manuellement l'angle
            _currentSweepAngle += sweepRotationSpeed * Time.deltaTime;
            _currentSweepAngle %= 360f;

            // Envoyer au shader
            SweepLineShader.SetFloat("_SweepAngle", _currentSweepAngle);
        }
    }

    private IEnumerator UpdateRadarRoutine()
    {
        var wait = new WaitForSeconds(SonarDelay);
        while (UpdateRadarRoutineEnabled)
        {
            UpdateRadar();
            yield return wait;
        }
    }

    private void UpdateRadar()
    {
        float panelRadius = Mathf.Min(_sonnarPannel.rect.width, _sonnarPannel.rect.height) / 2f;
        Vector3 sonarPosition = _sonar.transform.position;
        Quaternion inverseRotation = Quaternion.Inverse(_sonar.transform.rotation);

        // Réutiliser une liste statique pour éviter des allocations
        var detectedThisFrame = new HashSet<int>();

        // Phase 1 : Ajout ou mise à jour des dots actifs
        foreach (var item in SonarManager.SonarItems)
        {
            if (!item.IsDetectedBySonar)
                continue;

            detectedThisFrame.Add(item.UniqueID);

            Vector3 offset = item.Position - sonarPosition;
            Vector2 flatOffset = new Vector2(offset.x, offset.z);
            Vector2 localPos = inverseRotation * flatOffset;

            Vector2 uiPos = localPos / _sonar.DetectionRange * panelRadius;
            if (uiPos.sqrMagnitude > panelRadius * panelRadius)
                uiPos = uiPos.normalized * panelRadius;

            if (!_activeDots.TryGetValue(item.UniqueID, out SonarDotController dot))
            {
                var dotInstance = Instantiate(_sonarDotPrefab, _sonnarPannel);
                dotInstance.SetDotColor(item.DotColor); // couleur personnalisée
                dotInstance.SetDotSize(item.DotSize);
                dot = dotInstance;
                _activeDots[item.UniqueID] = dot;
            }

            dot.RectTransform.anchoredPosition = uiPos;
            dot.SetDotRotation(item.RotationAngles);
        }

        // Phase 2 : Nettoyage des anciens dots
        var keysToRemove = new List<int>();
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

        StartCoroutine(AnimatePingRoutine());
    }


    private IEnumerator AnimatePingRoutine()
    {
        if (_sonarPingImage == null)
            yield break;

        _sonarPingImage.gameObject.SetActive(true);
        _sonarPingAudioSource?.Play();

        // Reset couleur
        Color startColor = _sonarPingImage.color;
        startColor.a = 0.4f;
        _sonarPingImage.color = startColor;

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
            _sonarPingImage.rectTransform.sizeDelta = new Vector2(size, size);

            // Alpha décroissant
            Color c = _sonarPingImage.color;
            c.a = Mathf.Lerp(0.4f, 0f, progress);
            _sonarPingImage.color = c;

            yield return null;
        }

        _sonarPingImage.gameObject.SetActive(false);
    }
}
