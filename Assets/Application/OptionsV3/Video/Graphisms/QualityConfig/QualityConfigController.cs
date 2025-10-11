using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using LightHouse.Options.V3;
using UnityEngine;

public class QualityConfigController : MonoBehaviour, IOption
{
    [Header("UI")]
    public OptionEnumPassive _qualityConfig;

    [Header("Presets (SO)")]
    public SerializedDictionary<GfxTier, VideoPresetSettings> PresetSettings;

    // états
    private GfxTier _selectedTier; // sélection UI
    private GfxTier _backupTier;   // committé (dernier apply réussi)

    private readonly List<string> _labels = new();
    private bool _opInFlight;

    private void Awake()
    {
        if (!_qualityConfig)
        {
            Debug.LogError("[QualityConfigController] OptionEnumPassive manquant.");
            enabled = false;
            return;
        }

        _qualityConfig.OnPrevClicked += OnPrev;
        _qualityConfig.OnNextClicked += OnNext;

        InitializeUI();
    }

    private void OnDestroy()
    {
        if (_qualityConfig)
        {
            _qualityConfig.OnPrevClicked -= OnPrev;
            _qualityConfig.OnNextClicked -= OnNext;
        }
    }

    private void InitializeUI()
    {
        // Labels depuis l'enum, dans l'ordre Low..Ultra
        _labels.Clear();
        foreach (GfxTier t in Enum.GetValues(typeof(GfxTier)))
            _labels.Add(t.ToString());

        _qualityConfig.SetChoices(_labels);

        // Seed : si le handler n'est pas initialisé, on force Medium au lancement
        GfxTier seed = GraphismHandlerData.IsInitialized
            ? GraphismHandlerData.CurrentTier
            : GfxTier.Medium;

        // Si pas initialisé → applique Medium une fois pour stabiliser l'état global
        if (!GraphismHandlerData.IsInitialized)
        {
            if (PresetSettings != null && PresetSettings.TryGetValue(GfxTier.Medium, out var medium))
            {
                GraphismHandlerData.SetPreset(medium, GfxTier.Medium);
            }
            else
            {
                Debug.LogWarning("[QualityConfigController] Aucun preset Medium trouvé, état global non initialisé.");
            }
        }

        _backupTier = seed;
        _selectedTier = seed;
        _qualityConfig.ShowIndex((int)_selectedTier);
    }

    private void OnPrev() => Move(-1);
    private void OnNext() => Move(+1);

    private void Move(int delta)
    {
        if (_opInFlight) return;
        int count = Mathf.Max(1, _labels.Count);
        int newIndex = ((int)_selectedTier + delta + count) % count;
        _selectedTier = (GfxTier)newIndex;
        _qualityConfig.ShowIndex(newIndex);
    }

    // ---------- IOption ----------

    public bool HasChanges() => _selectedTier != _backupTier;

    public void Apply()
    {
        Debug.Log("he passed");

        if (_opInFlight) return;

        if (!HasChanges()) return;

        if (PresetSettings == null || !PresetSettings.TryGetValue(_selectedTier, out var settings) || settings == null)
        {
            Debug.LogWarning($"[QualityConfigController] Preset manquant pour tier '{_selectedTier}'. Apply annulé.");
            return;
        }

        _opInFlight = true;

        // Applique via le handler statique (réel changement runtime)
        GraphismHandlerData.SetPreset(settings, _selectedTier);

        // Commit local
        _backupTier = _selectedTier;

        _opInFlight = false;
        Debug.Log($"[QualityConfigController] Apply → Tier={_backupTier}");
    }

    public void Revert()
    {
        if (_opInFlight) return;

        // 1) UI-only : si la sélection ne correspond pas au committé, on "snap" l'UI
        if (_selectedTier != _backupTier)
        {
            _selectedTier = _backupTier;
            _qualityConfig.ShowIndex((int)_backupTier);
            Debug.Log("[QualityConfigController] Revert(UI) → UI réalignée sur le tier committé.");
            return;
        }

        // 2) System : si l'état global != committé, on réapplique le preset committé
        if (GraphismHandlerData.CurrentTier != _backupTier)
        {
            if (PresetSettings != null && PresetSettings.TryGetValue(_backupTier, out var settings) && settings != null)
            {
                _opInFlight = true;
                GraphismHandlerData.SetPreset(settings, _backupTier);
                _opInFlight = false;
                Debug.Log($"[QualityConfigController] Revert(System) → Tier={_backupTier}");
            }
            else
            {
                Debug.LogWarning("[QualityConfigController] Revert impossible : preset committé introuvable.");
            }
        }
        // sinon rien à faire
    }
}
