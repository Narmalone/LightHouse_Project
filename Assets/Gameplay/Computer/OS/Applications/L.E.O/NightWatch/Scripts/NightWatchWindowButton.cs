using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO.NightWatch
{
    [RequireComponent(typeof(Button))]
    public class NightWatchWindowButton : MonoBehaviour
    {
        [SerializeField] private E_NightWatchMode _targetWindow;
        [SerializeField] private NightWatchController _controller;
        [SerializeField] private Button _button;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
            else
                Debug.LogWarning($"No Button component found on {gameObject.name}");
        }

        private void OnClick()
        {
            if (_controller != null)
            {
                _controller.SwitchTo(_targetWindow);
            }
            else
            {
                Debug.LogWarning("NightWatchController reference not set.");
            }
        }
    }

}
