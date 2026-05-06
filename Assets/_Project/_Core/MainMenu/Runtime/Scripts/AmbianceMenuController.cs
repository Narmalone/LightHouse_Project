using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Features.Menu.Ambiance
{
    /// <summary>
    /// Gère les sons d'ambiance du menu (horror, pluie, vent).
    /// </summary>
    public class AmbianceMenuController : MonoBehaviour
    {
        #region ===== Settings =====

        [SerializeField] private AudioCue _ambianceHorror;
        [SerializeField] private AudioCue _rain;
        [SerializeField] private AudioCue _wind;

        #endregion

        #region ===== State =====

        private IAudioHandle _ambianceHandle;
        private IAudioHandle _rainHandle;
        private IAudioHandle _windHandle;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Start()
        {
            PlayAmbiance();
        }

        private void OnDestroy()
        {
            StopAmbiance();
        }

        #endregion

        #region ===== Audio Control =====

        private void PlayAmbiance()
        {
            if (ServiceLocator.Audio == null) return;

            _ambianceHandle = Play(_ambianceHorror);
            _rainHandle = Play(_rain);
            _windHandle = Play(_wind);
        }

        private void StopAmbiance()
        {
            _ambianceHandle?.Stop();
            _rainHandle?.Stop();
            _windHandle?.Stop();
        }

        private IAudioHandle Play(AudioCue cue)
        {
            if (cue == null) return null;

            return ServiceLocator.Audio.PlayAt(cue, transform.position);
        }

        #endregion
    }
}