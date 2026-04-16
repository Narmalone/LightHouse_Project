using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RadioMenuController : MonoBehaviour
{
    [SerializeField] private AudioCue _radioMusic;
    [SerializeField] private AudioCue _radioSpeach;

    [SerializeField] private Renderer _radioOnOffMaterial;

    private MaterialPropertyBlock _radioOnOffPropertyBlock;

    [SerializeField] private float _emissiveIntensity = 10f;

    [SerializeField] private Canvas _choiceCanvas;
    [SerializeField] private Button _listenBriefButton;
    [SerializeField] private Button _stopListeningBriefButton;
    [SerializeField] private Button _continueGameButton;
    [SerializeField] private RaycastableMenuItem _raycastableMenuItem;

    private IAudioHandle _radioSpeachHandler;

    private void Awake()
    {
        InitializeMaterial();
        _listenBriefButton.onClick.AddListener(StartEventRoutine);
        _stopListeningBriefButton.onClick.AddListener(StopEventRoutine);
        _continueGameButton.onClick.AddListener(ContinueGameRoutine);
        _raycastableMenuItem.OnShowInformationsEvent += RaycastableMenuItem_OnShowInformationsEvent;
        _raycastableMenuItem.OnHideInformationsEvent += RaycastableMenuItem_OnHideInformationsEvent;
        _choiceCanvas.gameObject.SetActive(false);
    }

    private void ContinueGameRoutine()
    {
        //TO DO : implement continue game logic Check last saved files
        // and load the scene
    }

    private void RaycastableMenuItem_OnHideInformationsEvent()
    {
        _choiceCanvas.gameObject.SetActive(false);
    }

    private void RaycastableMenuItem_OnShowInformationsEvent()
    {
        _choiceCanvas.gameObject.SetActive(true);
        if(_radioSpeachHandler != null && _radioSpeachHandler.CurrentSource != null && _radioSpeachHandler.CurrentSource.isPlaying)
        {
            _stopListeningBriefButton.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        _listenBriefButton.onClick.RemoveListener(StartEventRoutine);
        _raycastableMenuItem.OnShowInformationsEvent -= RaycastableMenuItem_OnShowInformationsEvent;
        _raycastableMenuItem.OnHideInformationsEvent -= RaycastableMenuItem_OnHideInformationsEvent;
        _stopListeningBriefButton.onClick.RemoveListener(StopEventRoutine);
        _continueGameButton.onClick.RemoveListener(ContinueGameRoutine);
    }

    private void InitializeMaterial()
    {
        _radioOnOffPropertyBlock = new MaterialPropertyBlock();
        _radioOnOffPropertyBlock.SetColor("_EmissiveColor", Color.red * _emissiveIntensity);
        _radioOnOffMaterial.SetPropertyBlock(_radioOnOffPropertyBlock);
    }

    private void StartEventRoutine()
    {
        _radioOnOffPropertyBlock.SetColor("_EmissiveColor", Color.green * _emissiveIntensity);
        _radioOnOffMaterial.SetPropertyBlock(_radioOnOffPropertyBlock);

        if(_radioSpeachHandler != null && _radioSpeachHandler.CurrentSource != null)
        {
            _radioSpeachHandler.Stop();
        }
        _radioSpeachHandler = ServiceLocator.Audio.PlayAt(_radioSpeach, this.transform.position);
        _stopListeningBriefButton.gameObject.SetActive(true);
        StartCoroutine(OnSpeachEnded());
    }

    private void StopEventRoutine()
    {
        if (_radioSpeachHandler != null && _radioSpeachHandler.CurrentSource != null)
        {
            _radioSpeachHandler.Stop();
        }
        _radioOnOffPropertyBlock.SetColor("_EmissiveColor", Color.red * _emissiveIntensity);
        _radioOnOffMaterial.SetPropertyBlock(_radioOnOffPropertyBlock);
        _stopListeningBriefButton.gameObject.SetActive(true);
    }

    private IEnumerator OnSpeachEnded()
    {
        while (_radioSpeachHandler != null && _radioSpeachHandler.CurrentSource != null && _radioSpeachHandler.CurrentSource.isPlaying)
        {
            yield return null;
        }
        _radioOnOffPropertyBlock.SetColor("_EmissiveColor", Color.red * _emissiveIntensity);
        _radioOnOffMaterial.SetPropertyBlock(_radioOnOffPropertyBlock);
        _radioSpeachHandler = null;
        _stopListeningBriefButton.gameObject.SetActive(false);
    }
}
