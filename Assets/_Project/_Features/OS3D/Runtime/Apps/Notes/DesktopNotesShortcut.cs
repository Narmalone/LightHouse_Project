using LightHouse.Features.TimeOfDay.TimeCore;
using UnityEngine;

namespace LightHouse.Features.Computer.NoteSystem
{
    /// <summary>
    /// Raccourci spécial de note sur le bureau : crée une nouvelle note automatiquement nommée selon l'heure actuelle.
    /// </summary>
    public class DesktopNotesShortcut : NotesShortcut
    {
        #region Overrides

        /// <summary>
        /// Génère automatiquement une nouvelle note avec un titre basé sur le jour et l'heure actuels.
        /// </summary>
        public override void OnExecute(bool playSound = true)
        {
            int day = TimeHandlerData.CurrentDay;
            float currentTime = TimeHandlerData.CurrentTime;

            // Convertit l'heure actuelle (ex: 14.5) en heure + minutes
            int hour = Mathf.FloorToInt(currentTime);
            int minute = Mathf.FloorToInt((currentTime - hour) * 60);

            // Crée une nouvelle note avec un titre horodaté (ex: Day_02_14_37)
            _note = new NoteData($"Day_{day}_{hour:D2}_{minute:D2}", "");

            // Lance l'app de note comme d’habitude
            base.OnExecute(playSound);
        }

        #endregion
    }
}
