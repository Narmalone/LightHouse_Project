using UnityEngine;
using UnityEngine.UI;

public class SliderParam : MonoBehaviour
{
    [SerializeField] protected Slider _slider;
    protected float _defaultValue;
    protected float _appliedValue;

    protected void Start()
    {
        _slider.value = _defaultValue;
        _appliedValue = _slider.value;
    }
}
