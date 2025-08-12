
namespace LightHouse.Game.Signals
{
    public interface ISignal
    {
        /// <summary>Clé unique, utilisée pour l’UI et la suppression.</summary>
        string Key { get; }

        /// <summary>Texte à afficher (BoatName, “Buoy ID: 3”, etc.)</summary>
        string DisplayText { get; set; }

        /// <summary>Temps restant en secondes.</summary>
        float RemainingTime { get; set; }
    }
}
