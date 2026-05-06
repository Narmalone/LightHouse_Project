using UnityEngine;

/// <summary>
/// Interface ou lorsque l'on appuie sur sauvegarder, lancera cette fonction
/// N'est pas obliggatoire mais vise à améliorer les performances pour éviter de set les données de sauvegarde dans un update.
/// </summary> 
public interface ICapturableState
{
    void CaptureState();
}
