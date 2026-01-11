using LightHouse.Features.Interactions;

namespace LightHouse.Features.Items.Interactable.Doors
{
    public interface IDoor : IInteractable
    {
        bool IsOpen { get; }
        void Open();
        void Close();
    }
}
