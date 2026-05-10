using LightHouse.Core.Audio;
using LightHouse.Core.Services;

using LightHouse.Features.Computer.OS;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

namespace LightHouse.Features.Computer.LEO
{
    public enum DataStatus
    {
        None,
        Loading,
        Failed,
        DataMissmatch,
        Success
    }

    public class UI_ReportDatasPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Image _reportResultHeaderImage;
        [SerializeField] private TextMeshProUGUI _windowTitleText;
        [SerializeField] private TextMeshProUGUI _reportResultHeaderText;
        [SerializeField] private HeightLerper _mainLerp;
        [SerializeField] private HeightLerper _contentLerper;
        [SerializeField] private Button _okCancelButton;

        [Header("Loading Animation")]
        [SerializeField] private RectTransform _loadingContent;
        [SerializeField] private AnimationCurve _loadingCurve;
        [SerializeField] private float _loadingDuration = 3f;
        [SerializeField] private SO_AudioCue _loadingSound;

        [Header("Success")]
        [SerializeField] private RectTransform _bodyParentContent;
        [SerializeField] private Color _successImageColor = Color.green;
        [SerializeField] private string _successText;
        [SerializeField] private SO_AudioCue _successSound;

        [Header("Failure")]
        [SerializeField] private Color _failedImageColor = Color.red;
        [SerializeField] private string _failedText;

        [Header("Missmatch Datas")]
        [SerializeField] private RectTransform _missmatchContent;
        [SerializeField] private Color _missMatchColor = Color.red;
        [SerializeField] private string _missmatchText;
        [SerializeField] private SO_AudioCue _missmatchSound;

        public DataStatus CurrentStatus { get; private set; } = DataStatus.None;
        private Coroutine _loadingRoutine;
        private GameObject _lastContentOpened;

        // Delegate injecté par le caller pour décider du résultat
        private Func<DataStatus> _resolveStatus;
        private IAudioHandle _loadingHandle;
        public event Action<DataStatus> OnLoadingCompleted;
        public RectTransform BodyParentContent => _bodyParentContent;

        private void Awake()
        {
            _okCancelButton.interactable = false;
            _okCancelButton.onClick.AddListener(OnOkCliqued);
        }

        private void OnDestroy()
        {
            _okCancelButton.onClick.RemoveListener(OnOkCliqued);
        }

        private void OnOkCliqued()
        {
            Destroy(this.gameObject);
        }

        public void StartLoading(Func<DataStatus> resolveStatus)
        {
            _resolveStatus = resolveStatus; // source de vérité unique
            if(ServiceLocator.Audio != null && _loadingSound)
            {
                _loadingHandle = ServiceLocator.Audio.PlayAt(_loadingSound, this.transform.position);
            }
            EnterPendingState();
            SwitchTo(DataStatus.Loading);
        }

        public void SwitchTo(DataStatus nextStatus)
        {
            if (CurrentStatus == nextStatus)
                return;

            CurrentStatus = nextStatus;

            // Désactive l'ancien contenu
            _lastContentOpened?.SetActive(false);

            // Active le nouveau contenu
            _lastContentOpened = GetContentForStatus(nextStatus);
            if (_lastContentOpened != null)
                _lastContentOpened.SetActive(true);

            // Applique le style/header
            ApplyHeaderForStatus(nextStatus);

            // Forcer un rebuild avant de set les hauteurs
            RefreshLayouts();
        }

        private GameObject GetContentForStatus(DataStatus status)
        {
            return status switch
            {
                DataStatus.Loading => _loadingContent.gameObject,
                DataStatus.Success => _bodyParentContent.gameObject,
                DataStatus.Failed => _bodyParentContent.gameObject,
                DataStatus.DataMissmatch => _missmatchContent.gameObject,
                _ => null
            };
        }

        private void ApplyHeaderForStatus(DataStatus status)
        {
            switch (status)
            {
                case DataStatus.Success:
                    _progressBar.value = 1f;
                    _reportResultHeaderImage.color = _successImageColor;
                    _reportResultHeaderText.text = _successText;
                    break;
                case DataStatus.Failed:
                    _progressBar.value = 0f;
                    _reportResultHeaderImage.color = _failedImageColor;
                    _reportResultHeaderText.text = _failedText;
                    break;
                case DataStatus.DataMissmatch:
                    _progressBar.value = 0f;
                    _reportResultHeaderImage.color = _missMatchColor;
                    _reportResultHeaderText.text = _missmatchText;
                    break;
                case DataStatus.Loading:
                    _progressBar.value = 0f;
                    _reportResultHeaderImage.gameObject.SetActive(false);
                    break;
            }
        }

        #region State Handling

        private void EnterPendingState()
        {
            _progressBar.value = 0f;
            _reportResultHeaderImage.gameObject.SetActive(false);
            CurrentStatus = DataStatus.None;

            if (_loadingRoutine != null) StopCoroutine(_loadingRoutine);
            _loadingRoutine = StartCoroutine(LoadingRoutine());
        }

        private IEnumerator LoadingRoutine()
        {
            float timer = 0f;
            _progressBar.value = 0f;
            _reportResultHeaderImage.gameObject.SetActive(false);

            while (timer < _loadingDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / _loadingDuration);
                _progressBar.value = _loadingCurve.Evaluate(t);
                yield return null;
            }

            // Récupère le résultat depuis la seule source
            DataStatus status = _resolveStatus != null ? _resolveStatus() : DataStatus.Success;

            _reportResultHeaderImage.gameObject.SetActive(true);

            if(_loadingHandle != null)
            {
                _loadingHandle.Stop();
                _loadingHandle = null;
            }
            
            _okCancelButton.interactable = true;

            PlayPopupEndedSound(status);

            OnLoadingCompleted?.Invoke(status);
            SwitchTo(status);
        }

        private void PlayPopupEndedSound(DataStatus status)
        {
            if(ServiceLocator.Audio == null) return;
            switch (status)
            {
                case DataStatus.Success:
                    if (_successSound != null) ServiceLocator.Audio.PlayAt(_successSound, this.transform.position);
                    break;
                case DataStatus.Failed:
                    if (_missmatchSound != null) ServiceLocator.Audio.PlayAt(_missmatchSound, this.transform.position);
                    break;
                case DataStatus.DataMissmatch:
                    if (_missmatchSound != null) ServiceLocator.Audio.PlayAt(_missmatchSound, this.transform.position);
                    break;
                default:
                    if (_missmatchSound != null) ServiceLocator.Audio.PlayAt(_missmatchSound, this.transform.position);
                    break;
            }

        }

        internal void RefreshLayouts()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_lastContentOpened.transform);

            var s = (RectTransform)_lastContentOpened.transform;
            float contentH = s.rect.height;
            float headerH = _reportResultHeaderImage.rectTransform.rect.height;
            float footerH = _okCancelButton.targetGraphic.rectTransform.rect.height;
            _contentLerper.SetTargetHeight(contentH);
            _mainLerp.SetTargetHeight((headerH + contentH + footerH) + 150f);
        }

        #endregion
    }

}
