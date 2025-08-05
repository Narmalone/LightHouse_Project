using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

[System.Serializable]
public class HeightsLerpers
{
    public float MainWindowSize;
    public float ContentwindowSize;
}

public enum DataStatus
{
    None,
    Loading,
    Failed,
    DataMissmatch,
    Success
}

public class NightWatchSendDatas : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private Image _reportResultHeaderImage;
    [SerializeField] private TextMeshProUGUI _reportResultHeaderText;
    [SerializeField] private HeightLerper _mainLerp;
    [SerializeField] private HeightLerper _contentLerper;
    [SerializeField] private Button _okCancelButton;

    [Header("Heights Settings")]
    [SerializeField] private HeightsLerpers _pending;
    [SerializeField] private HeightsLerpers _success;
    [SerializeField] private HeightsLerpers _failed;
    [SerializeField] private HeightsLerpers _dataMissmatch;

    [Header("Loading Animation")]
    [SerializeField] private GameObject _loadingContent;
    [SerializeField] private AnimationCurve _loadingCurve;
    [SerializeField] private float _loadingDuration = 3f;

    [Header("Success")]
    [SerializeField] private GameObject _successContent;
    [SerializeField] private Color _successImageColor = Color.green;
    [SerializeField] private string _sucessText;

    [Header("Failure")]
    [SerializeField] private GameObject _failureContent;
    [SerializeField] private Color _failedImageColor = Color.red;
    [SerializeField] private string _failedText;

    [Header("Missmatch Datas")]
    [SerializeField] private GameObject _missmatchContent;
    [SerializeField] private Color _missMatchColor = Color.red;
    [SerializeField] private string _missmatchText;

    public DataStatus CurrentStatus { get; private set; } = DataStatus.None;
    public bool IsSuccessfull { get; set; } = false;
    private Coroutine _loadingRoutine;
    private GameObject _lastContentOpened;

    private void Awake()
    {
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

    private void Start()
    {
        EnterPendingState();
        SwitchTo(DataStatus.Loading);
    }

    /// <summary>
    /// Switch to a new status
    /// </summary>
    public void SwitchTo(DataStatus nextStatus)
    {
        if (CurrentStatus == nextStatus) return;
        CurrentStatus = nextStatus;
        _lastContentOpened?.SetActive(false);
        switch (nextStatus)
        {
            case DataStatus.Loading:
                _lastContentOpened = _loadingContent;
                EnterLoadingState();
                break;
            case DataStatus.Success:
                _lastContentOpened = _successContent;
                EnterSuccessState();
                break;
            case DataStatus.Failed:
                _lastContentOpened = _failureContent;
                EnterFailedState();
                break;
            case DataStatus.DataMissmatch:
                _lastContentOpened = _missmatchContent;
                EnterDataMissmatchState();
                break;
        }
    }

    #region State Handling

    private void EnterPendingState()
    {
        _mainLerp.SetTargetHeight(_pending.MainWindowSize);
        _contentLerper.SetTargetHeight(_pending.ContentwindowSize);
        _progressBar.value = 0f;
        _reportResultHeaderImage.gameObject.SetActive(false);
        CurrentStatus = DataStatus.None;
    }

    private void EnterLoadingState()
    {
        _loadingContent.SetActive(true);
        _mainLerp.SetTargetHeight(_pending.MainWindowSize);
        _contentLerper.SetTargetHeight(_pending.ContentwindowSize);

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
            float normalizedTime = Mathf.Clamp01(timer / _loadingDuration);
            _progressBar.value = _loadingCurve.Evaluate(normalizedTime);
            yield return null;
        }

        // Une fois le "faux chargement" fini, on peut déclencher une vérification
        // Exemple : ici on simule un succès par défaut
        OnLoadingFinished(IsSuccessfull);
    }

    private void OnLoadingFinished(bool success)
    {
        _reportResultHeaderImage.gameObject.SetActive(true);
        SwitchTo(success ? DataStatus.Success : DataStatus.DataMissmatch);
    }

    private void EnterSuccessState()
    {
        _successContent.gameObject.SetActive(true);
        _mainLerp.SetTargetHeight(_success.MainWindowSize);
        _contentLerper.SetTargetHeight(_success.ContentwindowSize);
        _progressBar.value = 1f;
        _reportResultHeaderImage.color = _successImageColor;
        _reportResultHeaderText.text = _sucessText;
        Debug.Log("✅ SUCCESS: Report sent!");
    }

    private void EnterFailedState()
    {
        _failureContent.gameObject.SetActive(true);
        _mainLerp.SetTargetHeight(_failed.MainWindowSize);
        _contentLerper.SetTargetHeight(_failed.ContentwindowSize);
        _progressBar.value = 0f;
        _reportResultHeaderImage.color = _failedImageColor;
        _reportResultHeaderText.text = _failedText;
        Debug.Log("❌ FAILED: Report could not be sent.");
    }

    private void EnterDataMissmatchState()
    {
        _missmatchContent.gameObject.SetActive(true);   
        _mainLerp.SetTargetHeight(_dataMissmatch.MainWindowSize);
        _contentLerper.SetTargetHeight(_dataMissmatch.ContentwindowSize);
        _progressBar.value = 0f;
        _reportResultHeaderImage.color = _missMatchColor;
        _reportResultHeaderText.text = _missmatchText;
        Debug.Log("❌ FAILED: Report could not be sent.");
    }

    #endregion
}
