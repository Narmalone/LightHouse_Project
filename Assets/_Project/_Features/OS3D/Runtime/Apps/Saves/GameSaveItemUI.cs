using LightHouse.Core.Save;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSaveItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _saveNameText;
    [SerializeField] private TextMeshProUGUI _lastSaveDateText;
    [SerializeField] private TextMeshProUGUI _currentDayCountText;
    [SerializeField] private TextMeshProUGUI _currentPlayersMoneyText;

    [SerializeField] private Button _loadButton;

    private string _saveName;

    public void Bind(GameData data)
    {
        _saveName = data.Name;
        _saveNameText.text = data.Name;
        //_nameText.text = data.DisplayName ?? data.Name;
        //_levelText.text = data.CurrentLevelName;

        if (DateTime.TryParse(data.LastSaveTime, out var dt))
            _lastSaveDateText.text = dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        _loadButton.onClick.AddListener(OnLoadClicked);
    }

    private void OnLoadClicked()
    {
        SaveLoadSystem.Instance.LoadGame(_saveName);
    }
}
