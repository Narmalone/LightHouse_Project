using System;
using UnityEngine;

#region ENUMS
public enum GameZone
{
    //MAIN ZONES
    OutsideLocal,
    Bathroom,
    BedRoom,
    Kitchen,
    Office,
    Lens,
    RDC,
    OutsideLightHouse,
    Beach
}

#endregion


public class GameManager : Singleton<GameManager>, ISerializationCallbackReceiver
{
    [SerializeField] public GameSettings gameSettings;
    [SerializeField] private CustomEvent eventStartTimeCycle;
    [SerializeField] private CustomEvent eventNextDay;
    [SerializeField] private CustomEvent_GameZone _onGameZoneChanged;
    [SerializeField] private GameZoneTypeInfo _gameZoneTypeInfoSettings;
    public static GameZone CurrentPlayerZone;

    public static event Action OnDayCycleEnd;
    public static event Action<int> OnNightCycleEnd; //int = NewDayValue
    public static event Action<int> OnWeekStart; //int = NewDayValue

    [SerializeField] private int currentDay = 0;
    public int CurrentDay => currentDay;
    private float cycleTimeLeft;

    [SerializeField, Range(0, 10)] private float _globalSpeedTime = 1.0f;
    public static float GlobalSpeedTime = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        eventNextDay.handle += OnDayEnd;
        _onGameZoneChanged.handle += _onGameZoneChanged_handle;
    }

    private void OnDestroy()
    {
        eventNextDay.handle -= OnDayEnd;
        _onGameZoneChanged.handle -= _onGameZoneChanged_handle;
    }

    private void Start()
    {
        CurrentPlayerZone = GameZone.OutsideLightHouse;
        _onGameZoneChanged?.Raise(CurrentPlayerZone);
    }

    public TimeSpan GetCurrentInGameTime()
    {
        return TimeSpan.FromSeconds(cycleTimeLeft);
    }

    private void OnDayEnd()
    {
        currentDay++;
        if(currentDay >= gameSettings.TotalDays)
        {
            EndGame();  
        }
    }

    public void EndGame()
    {
        LightHouseSceneManager.Instance.LoadAsync(LightHouseSceneManager.BuildScenes.Credits);
    }

    public GMTypeInfo GetPlayerLocation()
    {
        foreach(GameZone outsideZones in _gameZoneTypeInfoSettings.OutsideZones)
        {
            if(outsideZones == CurrentPlayerZone)
            {
                return GMTypeInfo.Outside;
            }
        }

        foreach(GameZone insideZones in _gameZoneTypeInfoSettings.InsideZones)
        {
            if(insideZones == CurrentPlayerZone)
            {
                return GMTypeInfo.Inside;
            }
        }

        return GMTypeInfo.Outside;
    }

    private void _onGameZoneChanged_handle(GameZone obj)
    {
        if (CurrentPlayerZone != obj)
            CurrentPlayerZone = obj;
    }

    public void OnBeforeSerialize()
    {
        GlobalSpeedTime = _globalSpeedTime;
    }

    public void OnAfterDeserialize()
    {
        
    }
}