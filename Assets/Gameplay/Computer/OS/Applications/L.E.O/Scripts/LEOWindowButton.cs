using UnityEngine;
using UnityEngine.UI;

public class LEOWindowButton : MonoBehaviour
{
    [SerializeField] private LEOApplication _app;
    [SerializeField] private Button _button;
    [SerializeField] private ELEOWindow _target;

    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void OnClick()
    {
        _app.ShowWindow(_target);
    }
}
