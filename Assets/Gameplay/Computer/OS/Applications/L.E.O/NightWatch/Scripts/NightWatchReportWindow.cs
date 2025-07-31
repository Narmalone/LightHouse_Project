using UnityEngine;

public abstract class NightWatchReportWindow : MonoBehaviour
{
    [SerializeField] protected E_NightWatchMode _windowType;
    [SerializeField] protected NightWatchController _nightWatch;
    public E_NightWatchMode WindowType => _windowType;

    public void SetNightWatch(NightWatchController nightWatch)
    {
        _nightWatch = nightWatch;
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
