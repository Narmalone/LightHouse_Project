using UnityEngine;
using UnityEngine.Timeline;

public class TimelineTest : MonoBehaviour
{
    public SignalAsset asset;
    public SignalReceiver receiver;

    public void Log()
    {
        Debug.Log("signal reçu");
    }
}
