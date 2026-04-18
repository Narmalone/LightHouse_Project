using Eflatun.SceneReference;
using LightHouse.Core.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AddressableLoadRule
{
    [AddressableLabelSelector] public string label;
}

public class BootStrap : MonoBehaviour
{
    public static BootStrap Instance;

    public static event Action OnGameLoaded;

    [Header("Scene Labels")]
    [AddressableLabelSelector] public string menuSceneLabel;
    [AddressableLabelSelector] public string gameSceneLabel;

    [Header("Scene Names")]
    public SceneReference bootstrapScene;
    public SceneReference menuScene;
    public SceneReference gameScene;

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
        _sceneLoader.LoadedScenes.Add(
            gameObject.scene.name,
            gameObject.scene
        );

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
                        loadingScreen.SetSubLabel(Clean(assetName)); // 👈 IMPORTANT
                        loadingScreen.SetProgress(progress);
                    }
                )
            );
        }

        // ───────────── LOAD MENU ─────────────
        yield return _loader.RunStep(
            "Loading Menu Scene",
            () => _sceneLoader.LoadByLabel(
                menuSceneLabel,
                (assetName, progress) =>
                {
                    loadingScreen.SetSubLabel(Clean(assetName));
                    loadingScreen.SetProgress(progress);
                }
            )
        );

        _sceneLoader.SetActive(menuScene.Name);

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

        loadingScreen.Hide();
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
            () => _sceneLoader.Unload(menuScene.Name)
        );

        // ───────────── LOAD GAME SCENE ─────────────
        yield return _loader.RunStep(
            "Loading Game Scene",
            () => _sceneLoader.LoadByLabel(
                gameSceneLabel,
                (assetName, progress) =>
                {
                    loadingScreen.SetSubLabel(Clean(assetName));
                    loadingScreen.SetProgress(progress);
                }
            )
        );

        _sceneLoader.SetActive(gameScene.Name);

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
                )
            );
        }

        // ───────────── DONE ─────────────
        OnGameLoaded?.Invoke();

        yield return new WaitForSeconds(0.2f);
        loadingScreen.Hide();
    }

    private IEnumerator SwitchToBootstrap()
    {
        _sceneLoader.SetActive(bootstrapScene.Name);
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
            () => _sceneLoader.Unload(gameScene.Name)
        );

        // ───────────── LOAD MENU SCENE ─────────────
        yield return _loader.RunStep(
            "Loading Menu Scene",
            () => _sceneLoader.LoadByLabel(
                menuSceneLabel,
                (assetName, progress) =>
                {
                    loadingScreen.SetSubLabel(Clean(assetName));
                    loadingScreen.SetProgress(progress);
                }
            )
        );

        _sceneLoader.SetActive(menuScene.Name);

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