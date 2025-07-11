using UnityEngine;

public abstract class NightWatchReportWindow : MonoBehaviour
{
    [SerializeField] protected E_NightWatchMode _windowType;
    public E_NightWatchMode WindowType => _windowType;

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
