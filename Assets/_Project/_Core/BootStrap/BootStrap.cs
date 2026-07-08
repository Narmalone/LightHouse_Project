using LightHouse.Core.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class AddressableLoadRule
{
    [AddressableLabelSelector] public string label;
}

public class BootStrap : MonoBehaviour
{
    public static BootStrap Instance;

    public static event Action OnGameLoaded;

    [Header("Addressable Scenes References")]
    public AssetReference menuScene;
    public AssetReference gameScene;

    [Header("Preload Rules")]
    public List<AddressableLoadRule> initialPreload;
    public List<AddressableLoadRule> menuPreload;
    public List<AddressableLoadRule> gamePreload;

    private SceneLoader _sceneLoader;
    private AssetPreloader _assetPreloader;

    [Header("Bootstrap Managers")]
    [SerializeField] private List<BootstrapEntry> bootstrapEntries;

    private BootstrapInstaller _installer;

    private LoadingScreen loadingScreen => LoadingScreen.Instance;
    private LoadingRunner _loader;

    private string _runtimeMainMenuSceneName;
    private string _runtimeGameSceneName;
    private string _runtimeBootstrapSceneName;

    private void Awake()
    {
        Instance = this;

        _sceneLoader = new SceneLoader();
        _assetPreloader = new AssetPreloader();
        _loader = new LoadingRunner(loadingScreen);
    }

    private void Start()
    {
        StartCoroutine(StartPreload());
    }

    private IEnumerator StartPreload()
    {

        _installer = new BootstrapInstaller();

        loadingScreen.Show();

        // ───────────── BOOTSTRAP MANAGERS ─────────────
        yield return _loader.RunStep(
            "Initializing systems...",
            () => _installer.Install(bootstrapEntries)
        );

        // ───────────── INITIAL PRELOAD ─────────────
        foreach (var rule in initialPreload)
        {
            yield return _loader.RunStep(
                $"Preloading {rule.label}",
                () => _assetPreloader.Preload(
                    rule.label,
                    (assetName, progress) =>
                    {
                        loadingScreen.SetSubLabel(Clean(assetName));
                        loadingScreen.SetProgress(progress);
                    }
                )
            );
        }

        // ───────────── LOAD MENU ─────────────
        yield return _loader.RunStep(
            "Loading Menu Scene",
            () => _sceneLoader.LoadScene(
                menuScene,
                (loadedSceneName) =>
                {
                    _runtimeMainMenuSceneName = loadedSceneName;
                }
            )
        );

        // ───────────── MENU PRELOAD ─────────────
        foreach (var rule in menuPreload)
        {
            yield return _loader.RunStep(
                $"Loading {rule.label}",
                () => _assetPreloader.Preload(
                    rule.label,
                    (assetName, progress) =>
                    {
                        loadingScreen.SetSubLabel(Clean(assetName));
                        loadingScreen.SetProgress(progress);
                    }
                )
            );
        }
    }
    private string Clean(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";

        var split = raw.Split('/');
        return split[split.Length - 1];
    }
    // ─────────────────────────────────────

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        loadingScreen.Show();

        // ───────────── SWITCH TO BOOTSTRAP ─────────────
        yield return _loader.RunStep(
            "Preparing transition...",
            () => SwitchToBootstrap()
        );

        // ───────────── UNLOAD MENU ─────────────
        yield return _loader.RunStep(
            "Unloading Menu",
            () => _sceneLoader.UnloadScene(_runtimeMainMenuSceneName)
        );

        // ───────────── LOAD GAME SCENE ─────────────
        yield return _loader.RunStep(
            "Loading Game Scene",
            () => _sceneLoader.LoadScene(
                gameScene,
                (loadedSceneName) =>
                {
                    _runtimeGameSceneName = loadedSceneName;
                }
            )
        );

        // ───────────── GAME PRELOAD ─────────────
        foreach (var rule in gamePreload)
        {
            yield return _loader.RunStep(
                $"Loading {rule.label}",
                () => _assetPreloader.Preload(
                    rule.label,
                    (assetName, progress) =>
                    {
                        loadingScreen.SetSubLabel(Clean(assetName));
                        loadingScreen.SetProgress(progress);
                    }
                    , true
                )
            );
        }

        // ───────────── DONE ─────────────
        OnGameLoaded?.Invoke();
    }

    private IEnumerator SwitchToBootstrap()
    {
        //_sceneLoader.SetActive("BootStrap");
        yield return null;
    }

    // ─────────────────────────────────────

    public void ReturnToMenu()
    {
        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        loadingScreen.Show();

        // ───────────── SWITCH TO BOOTSTRAP ─────────────
        yield return _loader.RunStep(
            "Preparing transition...",
            () => SwitchToBootstrap()
        );

        // ───────────── UNLOAD GAME ─────────────
        yield return _loader.RunStep(
            "Unloading Game",
            () => _sceneLoader.UnloadScene(_runtimeGameSceneName)
        );

        // ───────────── LOAD MENU SCENE ─────────────
        yield return _loader.RunStep(
            "Loading Menu Scene",
            () => _sceneLoader.LoadScene(
                menuScene,
                (loadedSceneName) =>
                {
                    _runtimeMainMenuSceneName = loadedSceneName;
                }
            )
        );

        _sceneLoader.SetActive(menuScene.SubObjectName);

        // ───────────── MENU PRELOAD ─────────────
        foreach (var rule in menuPreload)
        {
            yield return _loader.RunStep(
                $"Loading {rule.label}",
                () => _assetPreloader.Preload(
                    rule.label,
                    (assetName, progress) =>
                    {
                        loadingScreen.SetSubLabel(Clean(assetName));
                        loadingScreen.SetProgress(progress);
                    }
                )
            );
        }

        // ───────────── DONE ─────────────
        yield return new WaitForSeconds(0.2f);
        loadingScreen.Hide();
    }
}