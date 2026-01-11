
namespace LightHouse.Features.Computer.OS
{
    public class LEOShortcut : ShortCutController
    {
        private void Start()
        {
            OnExecute(false);
            _currentInstance.OnClose(false);
        }
    }
}
