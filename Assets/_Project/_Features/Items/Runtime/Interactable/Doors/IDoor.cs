using LightHouse.Interactions;

public interface IDoor : IInteractable
{
    bool IsOpen { get; }
    void Open();
    void Close();
}
