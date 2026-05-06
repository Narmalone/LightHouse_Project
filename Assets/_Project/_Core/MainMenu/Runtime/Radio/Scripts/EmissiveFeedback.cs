using UnityEngine;

public enum EmissiveState
{
    Default,
    Hover,
    Active,
    Disabled
}


[RequireComponent(typeof(Renderer))]
public class EmissiveFeedback : MonoBehaviour
{
    [System.Serializable]
    public struct EmissiveSettings
    {
        public Color color;
        public float intensity;
    }

    [Header("States")]
    [SerializeField] private EmissiveSettings defaultState;
    [SerializeField] private EmissiveSettings hoverState;
    [SerializeField] private EmissiveSettings activeState;
    [SerializeField] private EmissiveSettings disabledState;

    [SerializeField] private Renderer targetRenderer;

    private MaterialPropertyBlock _mpb;
    private const string EmissiveColorID = "_EmissiveColor";

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        _mpb = new MaterialPropertyBlock();

        ApplyState(EmissiveState.Default);
    }

    public void ApplyState(EmissiveState state)
    {
        EmissiveSettings settings = state switch
        {
            EmissiveState.Hover => hoverState,
            EmissiveState.Active => activeState,
            EmissiveState.Disabled => disabledState,
            _ => defaultState
        };

        Apply(settings.color, settings.intensity);
    }

    private void Apply(Color color, float intensity)
    {
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissiveColorID, color * intensity);
        targetRenderer.SetPropertyBlock(_mpb);
    }
}