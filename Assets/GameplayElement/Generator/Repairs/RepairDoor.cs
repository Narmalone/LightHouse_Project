public class RepairDoor : ItemBaseAnim
{
    private string forPlayer;
    public override string Name { get => forPlayer; set => forPlayer = value; }

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
        forPlayer = isEnabled ? "Close" : "Open";
        eventName?.Raise(forPlayer);
    }
}
