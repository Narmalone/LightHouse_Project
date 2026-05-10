using UnityEngine;
using UnityEngine.UI;

public class RestartController : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _aliveSprite;
    [SerializeField] private Sprite _deadSprite;

    public void SetAlive()
    {
        _iconImage.sprite = _aliveSprite;
    }

    public void SetDead()
    {
        _iconImage.sprite = _deadSprite;

    }
}
