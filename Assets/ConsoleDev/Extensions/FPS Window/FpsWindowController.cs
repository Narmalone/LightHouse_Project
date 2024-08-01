using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class FpsWindowController : MonoBehaviour
{
    [SerializeField] private ResizableConsoleWidth resizableConsole;
    [SerializeField] private TextMeshProUGUI TMP_fps;
    [SerializeField] private TextMeshProUGUI TMP_framesDelay;
    [SerializeField] private TextMeshProUGUI TMP_setPassCallsCount;
    [SerializeField] private TextMeshProUGUI TMP_drawCallsCount;
    [SerializeField] private TextMeshProUGUI TMP_trianglesCount;
    [SerializeField] private TextMeshProUGUI TMP_verticesCount;
    [SerializeField] private TextMeshProUGUI TMP_gcMemory;
    [SerializeField] private TextMeshProUGUI TMP_mainThreadTime;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeWindowButton;

    ProfilerRecorder setPassCallsRecorder;
    ProfilerRecorder drawCallsRecorder;
    ProfilerRecorder trianglesRecorder;
    ProfilerRecorder verticesRecorder;
    ProfilerRecorder systemMemoryRecorder;
    ProfilerRecorder gcMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;

    private float deltaTime = 0.0f;
    public bool IsShowed = false;

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }

        return r;
    }

    private void Awake()
    {
        if (canvasGroup.alpha == 0f)
            Disable();
        else
            Enable();

        closeWindowButton.onClick.AddListener(() =>
        {
            Disable();
        });
    }

    void OnEnable()
    {
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");

        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
    }

    void OnDisable()
    {
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        trianglesRecorder.Dispose();
        verticesRecorder.Dispose();

        systemMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
    }

    void Update()
    {
        if (canvasGroup.alpha <= 0f) return;

        if (setPassCallsRecorder.Valid)
            TMP_setPassCallsCount.text = $"SetPass Calls: {setPassCallsRecorder.LastValue}";

        if (drawCallsRecorder.Valid)
            TMP_drawCallsCount.text = $"Batches Calls: {drawCallsRecorder.LastValue}";

        if (trianglesRecorder.Valid)
            TMP_trianglesCount.text = $"Triangles: {trianglesRecorder.LastValue}";

        if (verticesRecorder.Valid)
            TMP_verticesCount.text = $"Vertices: {verticesRecorder.LastValue}";

        if(mainThreadTimeRecorder.Valid)
            TMP_framesDelay.text = $"Frame Time: {GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1} ms";

        if (systemMemoryRecorder.Valid)
            TMP_gcMemory.text = $"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024)} MB";

        if (gcMemoryRecorder.Valid)
            TMP_mainThreadTime.text = $"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB";

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        TMP_fps.text = Mathf.Round(fps).ToString() + " FPS";
    }

    public void ResetWindow()
    {
        resizableConsole.consoleRect.anchoredPosition = resizableConsole.StartPos;
        resizableConsole.consoleRect.sizeDelta = new Vector2(resizableConsole.InitialSize.x, resizableConsole.InitialSize.y);
    }

    public void Enable()
    {
        IsShowed = true;
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Disable()
    {
        IsShowed = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
