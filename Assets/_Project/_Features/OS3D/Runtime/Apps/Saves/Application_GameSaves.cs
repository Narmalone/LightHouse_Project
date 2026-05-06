using LightHouse.Core.Save;
using LightHouse.Core.Services;
using LightHouse.Features.Computer.OS;
using UnityEngine;

public class Application_GameSaves : ComputerApp
{
    [SerializeField] private GameSaveItemUI _saveItemPrefab;
    [SerializeField] private RectTransform _contentParent;
    public override void OnClose(bool playSound = true)
    {
        if(ServiceLocator.Audio != null && playSound) ServiceLocator.Audio.PlayAt(_onCloseSound, this.transform.position);
        Destroy(gameObject);
    }

    public override void OnMinimize()
    {

    }

    public override void OnOpen(bool playSound = true)
    {
        RandomizePositionOnComputer();
        if (SaveLoadSystem.Instance == null) return;
        SaveLoadSystem.Instance.GetSaveMetadataList().ForEach(gameData =>
        {
            var saveItem = Instantiate(_saveItemPrefab, _contentParent);
            saveItem.Bind(gameData);
        });
    }
}
