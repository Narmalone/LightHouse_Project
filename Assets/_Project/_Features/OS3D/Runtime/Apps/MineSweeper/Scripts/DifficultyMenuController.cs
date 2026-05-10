using System;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyMenuController : MonoBehaviour
{
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button impossibleButton;
    [SerializeField] private Button closeButton;

    public event Action<int> OnDifficultySelected;

    private void Awake()
    {
        easyButton.onClick.AddListener(OnEasyButtonClicked);
        mediumButton.onClick.AddListener(OnMediumButtonClicked);
        hardButton.onClick.AddListener(OnHardButtonClicked);
        impossibleButton.onClick.AddListener(OnImpossibleButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnDestroy()
    {
        easyButton.onClick.RemoveListener(OnEasyButtonClicked);
        mediumButton.onClick.RemoveListener(OnMediumButtonClicked);
        hardButton.onClick.RemoveListener(OnHardButtonClicked);
        impossibleButton.onClick.RemoveListener(OnImpossibleButtonClicked);
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    private void OnEasyButtonClicked()
    {
        OnDifficultySelected?.Invoke(0);
        gameObject.SetActive(false);
    }

    private void OnMediumButtonClicked()
    {
        OnDifficultySelected?.Invoke(1);
        gameObject.SetActive(false);
    }

    private void OnHardButtonClicked()
    {
        OnDifficultySelected?.Invoke(2);
        gameObject.SetActive(false);
    }

    private void OnImpossibleButtonClicked()
    {
        OnDifficultySelected?.Invoke(3);
        gameObject.SetActive(false);
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
