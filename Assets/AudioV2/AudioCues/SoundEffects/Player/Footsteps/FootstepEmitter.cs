using UnityEngine;

public class FootstepEmitter : MonoBehaviour
{
    [Header("Refs")]
    public LayerMask _groundMask = ~0;
    public float _rayLength = 0.9f;

    [SerializeField] private CombinedSurfaceProvider _surfaceProvider;
    [SerializeField] private FootstepSet _footStepSet;

    [Header("Ajustement son (optionnel)")]
    public float crouchVolMul = 0.85f;
    public float runVolMul = 1.05f;

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f) return;
        if (Physics.Raycast(this.transform.position + Vector3.up * 0.05f, Vector3.down, out var hit, _rayLength, _groundMask, QueryTriggerInteraction.Ignore))
        {
            var surface = _surfaceProvider.GetSurface(in hit);
            var cue = _footStepSet.GetCue(surface);
            if (!cue) return;

            // Volume/pitch simple selon l’état
            float volMul = 1f;

            //Si on veut modifier le volume car il est crouch ou il sprint on peut.

            float baseVol = cue.Volume * volMul;
            //float basePitch = cue.Pitch * (_state == MoveState.Crouch ? 0.98f : 1f);
            float basePitch = cue.Pitch;

            if (ServiceLocator.Audio != null)
            {
                var s = ServiceLocator.Audio.PlayAt(cue, hit.point);
                s.SetVolume(baseVol);
                s.SetPitch(basePitch);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        Debug.Log("on land trigger");
        //Play land sound
    }
}
