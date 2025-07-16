using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LEOWindowButton : MonoBehaviour
{
    public LEOApplication App;
    [SerializeField] protected Button _button;
    [SerializeField] protected ELEOWindow _target;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    protected virtual void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    protected virtual void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public virtual void OnClick()
    {
        App?.ShowWindow(_target);
    }
}
