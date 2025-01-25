public class MainDoorElectricalController : ItemBaseAnim
{
    private string forPlayer = "Close";
    public override string Name { get => forPlayer; set => forPlayer = value; }

    private void Awake()
    {
        UpdateName();
    }

    private void UpdateName()
    {
        forPlayer = IsEnabled ? "Close" : "Open";
        eventName?.Raise(forPlayer);
    }

    public override void ChangeAnim()
    {
        base.ChangeAnim();
        UpdateName();
    }
}
