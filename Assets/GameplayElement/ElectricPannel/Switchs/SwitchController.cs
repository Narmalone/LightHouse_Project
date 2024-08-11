using UnityEngine;

public class SwitchController : ItemBaseAnim
{
    private string forPlayer = "Switch to ";
    public override string Name { get => forPlayer; set => forPlayer = value; }
    public float CostPower = 30f;
    public ElectricityZones elecZone;
    public Collider Col;

    private void Awake()
    {
        UpdateName();
    }

    private void UpdateName()
    {
        if (isEnabled)
        {
            forPlayer = "Switch to Off";
        }
        else
        {
            forPlayer = "Switch to On";
        }
    }

    public override void ChangeAnim()
    {
        base.ChangeAnim();
        UpdateName();
        eventName?.Raise(forPlayer);
    }
}