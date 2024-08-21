using UnityEngine;

public class SwitchController : ItemBaseAnim
{
    [Header("SWITCH INFOS")]
    public float CostPower = 30f;
    public GameZone elecZone;

    [Header("SWITCH REFS")]
    public Collider Col;

    [Header("EVENTS")]
    [Header("Raise")]
    [SerializeField] private CustomEvent_ElectricZone _onSwitchEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onSwitchDisabled;

    private string forPlayer = "Switch to ";
    public override string Name { get => forPlayer; set => forPlayer = value; }

    private void Awake()
    {
        UpdateName();
    }

    private void UpdateName()
    {
        if (isEnabled)
        {
            forPlayer = "Switch to Off";
            _onSwitchEnabled?.Raise(elecZone);
        }
        else
        {
            forPlayer = "Switch to On";
            _onSwitchDisabled?.Raise(elecZone);
        }
    }

    public override void ChangeAnim()
    {
        base.ChangeAnim();
        UpdateName();
        eventName?.Raise(forPlayer);
    }
}