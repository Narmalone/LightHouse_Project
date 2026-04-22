using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

public class AudioFeedback : MonoBehaviour
{
    [SerializeField] private AudioCue hoverSound;
    [SerializeField] private AudioCue clickSound;

    private InteractableBase interactable;

    private bool _canReceiveFeedback = true;

    public void SetEnable(bool enable)
    {
        _canReceiveFeedback = enable;
    }
    private void OnDestroy()
    {
        if(interactable != null)
        {
            interactable.OnHoverEnter -= Interactable_OnHoverEnter;
            interactable.OnClickDown -= Interactable_OnClickDown;
        }
    }

    public void Bind(InteractableBase interactable)
    {
        interactable.OnHoverEnter += Interactable_OnHoverEnter;
        interactable.OnClickDown += Interactable_OnClickDown;
    }

    private void Interactable_OnClickDown()
    {
        if (!_canReceiveFeedback) return;
        if (clickSound != null)
            ServiceLocator.Audio.PlayAt(clickSound, transform.position);
    }

    private void Interactable_OnHoverEnter()
    {
        if (!_canReceiveFeedback) return;
        if (hoverSound != null)
            ServiceLocator.Audio.PlayAt(hoverSound, transform.position);
    }
}