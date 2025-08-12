using System.Collections.Generic;

namespace LightHouse.Game.Sonar.Core
{
    /// <summary>
    /// Stocke et gère les objets sonarables ainsi que la référence au sonar principal.
    /// </summary>
    public static class SonarHandlerData
    {
        #region Fields

        /// <summary>
        /// Liste des objets actuellement détectables par le sonar.
        /// </summary>
        public static readonly List<ISonarable> SonarItems = new();

        /// <summary>
        /// Référence globale au sonar actif.
        /// </summary>
        public static Sonar Sonar;

        #endregion

        #region Public API

        /// <summary>
        /// Ajoute un objet sonarable à la liste de suivi.
        /// </summary>
        public static void Register(ISonarable sonarable)
        {
            if (sonarable != null && !SonarItems.Contains(sonarable))
                SonarItems.Add(sonarable);
        }

        /// <summary>
        /// Retire un objet sonarable de la liste de suivi.
        /// </summary>
        public static void Unregister(ISonarable sonarable)
        {
            if (sonarable != null)
                SonarItems.Remove(sonarable);
        }

        #endregion
    }
}
