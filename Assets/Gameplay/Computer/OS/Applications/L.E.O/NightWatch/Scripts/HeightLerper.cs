using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class HeightLerper : MonoBehaviour
{
    public float TargetHeight = 200f;
    public float LerpSpeed = 5f;

    private RectTransform _rectTransform;
    private float _currentVelocity;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnValidate()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float currentHeight = _rectTransform.sizeDelta.y;

        if (Mathf.Abs(currentHeight - TargetHeight) > 0.1f)
        {
            float newHeight = Mathf.SmoothDamp(currentHeight, TargetHeight, ref _currentVelocity, 1f / LerpSpeed);
            Vector2 size = _rectTransform.sizeDelta;
            size.y = newHeight;
            _rectTransform.sizeDelta = size;
        }
    }

    public void SetTargetHeight(float newHeight)
    {
        TargetHeight = newHeight;
    }
}
