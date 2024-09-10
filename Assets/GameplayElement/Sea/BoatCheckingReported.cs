using System.Collections.Generic;
using UnityEngine;

public class BoatCheckingReported : MonoBehaviour
{
    public enum BoatState
    {
        IDLE,
        MOVING,
        FIXING
    }

    private List<SeaReportedObject> _allReported = new List<SeaReportedObject>();

    private BoatState state;
    public BoatState State => state;

    public void ActiveChecking()
    {
        // Aller chercher le [0] du dictionnary
        // S'arrêter quand arrivé
        // handle Réparer Boat / Buoy
        // Notify réparation finit
        // Check si autre Report
            // Si oui recommencer avec nouveau point
        // Sinon retourner au spawn point
    }

    public void UpdateDictionnary(SeaReportedObject go)
    {
        _allReported.Add(go);
    }
}