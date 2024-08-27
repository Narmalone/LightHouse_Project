using System;
using UnityEngine;

#region ENUMS
public enum GameZone
{
    OutsideLocal,
    Bathroom,
    BedRoom,
    KitchenAndOther,
    Office,
    Lens
}
#endregion


public class GameManager : Singleton<GameManager>
{
    [SerializeField] public GameSettings gameSettings;
    [SerializeField] private CustomEvent eventStartTimeCycle;
    [SerializeField] private CustomEvent eventNextDay;

    public static event Action OnDayCycleEnd;
    public static event Action<int> OnNightCycleEnd; //int = NewDayValue
    public static event Action<int> OnWeekStart; //int = NewDayValue

    [SerializeField] private int currentDay = 0;
    public int CurrentDay => currentDay;
    private float cycleTimeLeft;

    protected override void Awake()
    {
        base.Awake();
        eventNextDay.handle += OnDayEnd;
    }

    private void OnDestroy()
    {
        eventNextDay.handle -= OnDayEnd;
    }

    private void Start()
    {
        
    }

    public TimeSpan GetCurrentInGameTime()
    {
        return TimeSpan.FromSeconds(cycleTimeLeft);
    }

    private void OnDayEnd()
    {
        currentDay++;
    }
}