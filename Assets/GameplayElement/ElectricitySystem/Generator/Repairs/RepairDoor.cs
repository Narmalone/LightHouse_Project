using UnityEngine;

public class RepairDoor : ItemBaseAnim
{
    private string forPlayer;
    public override string Name { get => forPlayer; set => forPlayer = value; }

    [Header("Languages")]
    [SerializeField] private KeyWordLanguage _close;
    [SerializeField] private KeyWordLanguage _open;

    private void Awake()
    {
        UpdateName();
    }

    public override void ChangeAnim()
    {
        base.ChangeAnim();
        UpdateName();
    }

    private void UpdateName()
    {
        forPlayer = isEnabled ? $"{_close.CurrentValue}" : $"{_open.CurrentValue}";
        eventName?.Raise(forPlayer);
    }
}
