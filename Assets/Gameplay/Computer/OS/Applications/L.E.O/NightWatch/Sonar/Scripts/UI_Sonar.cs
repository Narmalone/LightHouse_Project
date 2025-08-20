using LightHouse.Audio;
using LightHouse.Game.Sonar.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch.Sonar
{
    /// <summary>
    /// UI Sonar : dots, balayage et lien visuel (dot -> label du bas).
    /// Le lien utilise un Pivot (RectTransform) + une Image enfant (barre).
    /// </summary>
    public class UI_Sonar : MonoBehaviour
    {
        #region Serialized — Références & Look

        [Header("Références UI")]
        public SonarDetector _sonar => SonarHandlerData.Sonar;
        [SerializeField] private RectTransform _sonarPanel;

        // ⚠️ Dans la scène : Pivot (RectTransform) avec une Image en enfant.
        [Header("Selected Link (Pivot -> Image)")]
        [SerializeField] private Color _selectedDorColor = Color.yellow; 
        [SerializeField] private RectTransform _selectedDotLinkPivot;  // l’objet "Pivot" (parent)
        [SerializeField] private RectTransform _selectedDotLinkImage;  // l’Image enfant (pivot = (0,0.5) recommandé)

        [SerializeField] private SonarDotController _sonarDotPrefab;
        [SerializeField] private TextMeshProUGUI _bottomInfoText;

        [Header("Balayage / Ping")]
        [SerializeField] private bool _updateRadarRoutineEnabled = true;
        [SerializeField] private float _sonarDelay = 5f;
        [SerializeField] private RectTransform _radarSweepLine;
        [SerializeField] private Material _sweepLineShader;
        [SerializeField] private float _sweepRotationSpeed = 72f;

        [Header("Ping visuel")]
        [SerializeField] private Image _sonarPingImage;
        [SerializeField] private float _pingDuration = 0.5f;
        [SerializeField] private float _pingStartSize = 100f;
        [SerializeField] private float _pingMaxSize = 500f;

        [Header("Son")]
        [SerializeField] private AudioSource _sonarPingAudioSource;
        [SerializeField] private EffectAudio _sonarPingClip;

        [Header("Link Layout")]
        [Tooltip("Décalage vertical (px) pour placer le départ juste sous le dot.")]
        [SerializeField] private float _dotStartYOffset = 8f;

        [Tooltip("Petit espace (px) laissé au départ du trait.")]
        [SerializeField] private float _startGap = 6f;

        [Tooltip("Petit espace (px) laissé avant le label.")]
        [SerializeField] private float _endGap = 12f;

        #endregion

        #region Runtime state

        private readonly Dictionary<int, SonarDotController> _activeDots = new();
        private float _currentSweepAngle;
        private Canvas _canvas;
        private Camera _uiCamera;

        // Lien actif
        private RectTransform _linkFrom;   // dot sélectionné (RectTransform du dot)
        private RectTransform _linkTo;     // label du bas (rectTransform)
        private bool _linkActive;

        #endregion

        #region Unity lifecycle

        private void OnValidate()
        {
            if (_sonarPingClip != null && _sonarPingClip.clips != null && _sonarPingClip.clips.Length > 0)
            {
                _sonarPingAudioSource.clip = _sonarPingClip.clips[0];
                _sonarPingAudioSource.spatialBlend = _sonarPingClip._spatialBlend;
                _sonarPingAudioSource.volume = _sonarPingClip.volume;
            }
        }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _uiCamera = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? _canvas.worldCamera : null;

            if (_sonarPingClip != null && _sonarPingClip.clips != null && _sonarPingClip.clips.Length > 0)
            {
                _sonarPingAudioSource.clip = _sonarPingClip.clips[0];
                _sonarPingAudioSource.spatialBlend = _sonarPingClip._spatialBlend;
                _sonarPingAudioSource.volume = _sonarPingClip.volume;
            }
        }

        private void Update()
        {
            if (_sweepLineShader != null)
            {
                _currentSweepAngle = (_currentSweepAngle + _sweepRotationSpeed * Time.deltaTime) % 360f;
                _sweepLineShader.SetFloat("_SweepAngle", _currentSweepAngle);
            }
        }

        #endregion

        #region Radar loop

        public void StartRadar() => StartCoroutine(UpdateRadarRoutine());
        public void StopRadar() => StopAllCoroutines();

        private IEnumerator UpdateRadarRoutine()
        {
            var wait = new WaitForSeconds(_sonarDelay);
            while (_updateRadarRoutineEnabled)
            {
                UpdateRadar();
                yield return wait;
            }
        }

        private void UpdateRadar()
        {
            float panelRadius = Mathf.Min(_sonarPanel.rect.width, _sonarPanel.rect.height) * 0.5f;
            Vector3 sonarPos = _sonar.transform.position;
            Quaternion invRot = Quaternion.Inverse(_sonar.transform.rotation);

            var detectedThisFrame = new HashSet<int>();

            foreach (var item in SonarHandlerData.SonarItems)
            {
                if (!item.IsDetectedBySonar) continue;

                detectedThisFrame.Add(item.UniqueID);

                Vector3 offset = item.Position - sonarPos;
                Vector2 flat = new Vector2(offset.x, offset.z);
                Vector2 local = invRot * flat;
                Vector2 uiPos = local / _sonar.DetectionRange * panelRadius;
                if (uiPos.sqrMagnitude > panelRadius * panelRadius)
                    uiPos = uiPos.normalized * panelRadius;

                if (!_activeDots.TryGetValue(item.UniqueID, out var dot))
                {
                    dot = Instantiate(_sonarDotPrefab, _sonarPanel);
                    dot.SetDotColor(item.DotColor);
                    dot.SetDotSize(item.DotSize);
                    dot.SetDotSprite(item.DotSprite);
                    dot.SetSonarElement(item);
                    dot.SonarDotClicked += OnDotClicked;
                    _activeDots[item.UniqueID] = dot;
                }

                dot.RectTransform.anchoredPosition = uiPos;
                dot.SetDotRotation(item.RotationAngles);
            }

            var toRemove = new List<int>();
            foreach (var kv in _activeDots)
            {
                if (!detectedThisFrame.Contains(kv.Key))
                {
                    kv.Value.SonarDotClicked -= OnDotClicked;
                    Destroy(kv.Value.gameObject);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var id in toRemove) _activeDots.Remove(id);

            StartCoroutine(AnimatePingRoutine());
        }

        #endregion

        #region Selection link (dot -> bottom text)

        private void OnDotClicked(string display, Image dotRect)
        {
            _bottomInfoText.text = display;

            // place the pivot just under the clicked dot (same parent as the dots)
            _selectedDotLinkPivot.gameObject.SetActive(true);
            _selectedDotLinkPivot.anchoredPosition = dotRect.rectTransform.anchoredPosition + Vector2.down * 20f;

            CalculateRotation();
        }

        private void CalculateRotation()
        {
            // On travaille dans l'espace local du parent du pivot
            var parent = (RectTransform)_selectedDotLinkPivot.parent;

            // Convertit les 2 positions monde -> espace local du parent
            Vector2 from = (Vector2)parent.InverseTransformPoint(_selectedDotLinkPivot.position);
            Vector2 to = (Vector2)parent.InverseTransformPoint(_bottomInfoText.rectTransform.position);
            CalculateDotLinkHeight(from, to);

            // Direction local pivot -> bottom text
            Vector2 dir = to - from;

            // - Si la barre pointe "vers le haut" en local :
            float angleZ = Vector2.SignedAngle(Vector2.up, dir);
            // - Si elle pointe "vers le bas", utilise Vector2.down
            // float angleZ = Vector2.SignedAngle(Vector2.down, dir);

            // Applique UNIQUEMENT une rotation locale autour de Z
            _selectedDotLinkPivot.localRotation = Quaternion.AngleAxis(angleZ, Vector3.forward);
        }

        private void CalculateDotLinkHeight(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            _selectedDotLinkImage.sizeDelta = new Vector2(_selectedDotLinkImage.rect.width, dir.magnitude);
        }



        public void ClearSelectionLink()
        {
            _linkActive = false;
            _linkFrom = _linkTo = null;
            if (_selectedDotLinkPivot != null)
                _selectedDotLinkPivot.gameObject.SetActive(false);
        }

        #endregion

        #region Ping animation

        private IEnumerator AnimatePingRoutine()
        {
            if (_sonarPingImage == null) yield break;

            _sonarPingImage.gameObject.SetActive(true);
            _sonarPingAudioSource?.Play();

            Color c0 = _sonarPingImage.color; c0.a = 0.4f;
            _sonarPingImage.color = c0;

            float t = 0f;
            while (t < _pingDuration)
            {
                t += Time.deltaTime;
                float k = t / _pingDuration;

                float size = Mathf.Lerp(_pingStartSize, _pingMaxSize, k);
                _sonarPingImage.rectTransform.sizeDelta = new Vector2(size, size);

                var c = _sonarPingImage.color;
                c.a = Mathf.Lerp(0.4f, 0f, k);
                _sonarPingImage.color = c;

                yield return null;
            }
            _sonarPingImage.gameObject.SetActive(false);
        }

        #endregion
    }
}
