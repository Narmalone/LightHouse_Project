using LightHouse.Handlers;
using UnityEngine;

public class FootstepEmitter : MonoBehaviour
{
    [Header("Refs")]
    public Transform footLeft;        // optionnel
    public Transform footRight;       // optionnel
    public Transform fallbackOrigin;  // si pas de pieds assignés
    public LayerMask groundMask = ~0;
    public float rayLength = 0.9f;

    public CombinedSurfaceProvider surfaceProvider;
    public FootstepSet footstepSet;

    [Header("State (injecté par le controller)")]
    public float moveSpeed;   // m/s (ex: PlayerHandlerData.MainPlayer.Character.CurrentSpeed)
    public bool isCrouched;   // depuis le Stance du perso
    [Tooltip("Seuil sous lequel on coupe les pas")]
    public float minSpeedForSteps = 0.25f;
    [Tooltip("Au-delà de ce seuil de vitesse, on considère 'Run'")]
    public float runSpeedThreshold = 6.0f; // vu que tu es ~4 walk / ~8 sprint

    [Header("Intervalles par état (secondes entre PAS)")]
    public float walkInterval = 0.50f; // 2 pas/s
    public float runInterval = 0.33f; // ~3 pas/s
    public float crouchInterval = 0.60f; // ~1.66 pas/s
    [Tooltip("Variation aléatoire ±% appliquée à chaque pas")]
    [Range(0f, 0.5f)] public float intervalJitter = 0.07f;
    [Tooltip("Anti-spam global (escaliers, clips)")]
    public float minIntervalSec = 0.12f;

    [Header("Ajustement son (optionnel)")]
    public float crouchVolMul = 0.85f;
    public float runVolMul = 1.05f;

    // internes
    private bool _leftNext = true;
    private float _timer;
    private float _intervalThisStep;
    private float _timeSinceLastStep;
    private bool _initialized;

    private enum MoveState { Idle, Walk, Run, Crouch }
    private MoveState _state = MoveState.Idle;

    void Reset()
    {
        if (!fallbackOrigin) fallbackOrigin = transform;
    }

    void Update()
    {
        // Récupérer l’état depuis ton handler
        if (PlayerHandlerData.MainPlayer != null)
        {
            var ch = PlayerHandlerData.MainPlayer.Character;
            moveSpeed = ch.CurrentSpeed;
            isCrouched = ch.GetState().Stance == LightHouse.KinematicCharacterController.Stance.Crouch;
        }

        if (!footstepSet || !surfaceProvider || (!fallbackOrigin && !footLeft && !footRight))
            return;

        // Déterminer l’état
        var nextState = ComputeState(moveSpeed, isCrouched);
        if (!_initialized || nextState != _state)
        {
            _state = nextState;
            // On relance un interval propre quand l’état change
            _timer = 0f;
            _timeSinceLastStep = minIntervalSec; // permet un pas rapide si on vient d’accélérer
            _intervalThisStep = PickIntervalForState(_state);
            _initialized = true;
        }

        if (_state == MoveState.Idle)
        {
            _timer = 0f;
            return;
        }

        // Avancer le timer
        _timer += Time.deltaTime;
        _timeSinceLastStep += Time.deltaTime;

        // Déclencher quand on a atteint l’intervalle et respecté l’anti-spam
        if (_timer >= _intervalThisStep && _timeSinceLastStep >= minIntervalSec)
        {
            DoFootstep(_leftNext ? "L" : "R");
            _leftNext = !_leftNext;

            _timer = 0f;
            _timeSinceLastStep = 0f;
            _intervalThisStep = PickIntervalForState(_state); // nouveau jitter à chaque pas
        }
    }

    private MoveState ComputeState(float speed, bool crouched)
    {
        if (crouched && speed >= minSpeedForSteps) return MoveState.Crouch;
        if (speed < minSpeedForSteps) return MoveState.Idle;
        if (speed >= runSpeedThreshold) return MoveState.Run;
        return MoveState.Walk;
    }

    private float PickIntervalForState(MoveState s)
    {
        float baseInterval = s switch
        {
            MoveState.Crouch => crouchInterval,
            MoveState.Run => runInterval,
            MoveState.Walk => walkInterval,
            _ => 0f
        };

        if (baseInterval <= 0f) return 0f;
        float jitter = 1f + Random.Range(-intervalJitter, intervalJitter);
        return Mathf.Max(minIntervalSec, baseInterval * jitter);
    }

    private void DoFootstep(string which)
    {
        var foot = (which == "L") ? footLeft : footRight;
        Vector3 origin = (foot ? foot.position : (fallbackOrigin ? fallbackOrigin.position : transform.position));

        if (Physics.Raycast(origin + Vector3.up * 0.05f, Vector3.down, out var hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            var surface = surfaceProvider.GetSurface(in hit);
            var cue = footstepSet.GetCue(surface);
            if (!cue) return;

            // Volume/pitch simple selon l’état
            float volMul = 1f;
            if (_state == MoveState.Crouch) volMul *= crouchVolMul;
            else if (_state == MoveState.Run) volMul *= runVolMul;

            float baseVol = cue.Volume * volMul;
            float basePitch = cue.Pitch * (_state == MoveState.Crouch ? 0.98f : 1f);

            if (ServiceLocator.Audio != null)
            {
                var s = ServiceLocator.Audio.PlayAt(cue, hit.point);
                s.SetVolume(baseVol);
                s.SetPitch(basePitch);
            }
        }
    }
}
