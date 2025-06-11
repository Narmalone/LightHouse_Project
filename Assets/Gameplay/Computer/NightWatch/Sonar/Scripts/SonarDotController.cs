using UnityEngine;
using UnityEngine.UI;

public class SonarDotController : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _dotImage;

    public Image DotImage => _dotImage;
    public RectTransform RectTransform => _rectTransform;

    public int _glowColorKey = Shader.PropertyToID("_GlowColor");

    private Material _instanceMaterial;

    void Awake()
    {
        if (_dotImage != null && _dotImage.material != null)
        {
            _instanceMaterial = new Material(_dotImage.material); // Clone
            _dotImage.material = _instanceMaterial;
        }
    }


    public void SetDotSize(Vector2 size)
    {
        if (_rectTransform != null)
            _rectTransform.sizeDelta = size;
    }

    public void SetDotSize(float size)
    {
        SetDotSize(new Vector2(size, size));
    }

    public void SetDotColor(Color color)
    {
        if (_instanceMaterial != null)
            _instanceMaterial.SetColor(_glowColorKey, color);

        _dotImage.color = color; // (optionnel selon visuel)
    }


    public void SetDotSprite(Sprite dotSprite)
    {
        _dotImage.sprite = dotSprite;
    }
}
