using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public enum BoatReportType
{
    NULL,
    DANGEREUX,
    SOS,
    PANNE,
    INFRACTION
}

public class SeaManager : Singleton<SeaManager>
{
    public enum ReportedObjectType{
        BUOY = 0,
        BOAT = 1
    }

    [SerializeField] private CustomEvent_String _eventReportBuoy;
    [SerializeField] private CustomEvent_String _eventReportBoat;
    [SerializeField] private CustomEvent _eventWrongID;
    [SerializeField] private CustomEvent _eventIDCorrect;
    [SerializeField] private BoatCheckingReported _prefabBoatCheckingReported;
    [SerializeField] private Transform _parentBoatCheckingReported;
    [SerializeField] private Transform _spawnPointBoatCheckingReported;
    [SerializeField] private SeaElement[] _allBuoyObject;
    [SerializeField] private SeaElement[] _allBoatObject;

    private BoatCheckingReported _currentBoatCheckingReported;

    private List<List<SeaElement>> _allSeaObjects = new List<List<SeaElement>>(2);
    private List<List<SeaElement>> _allReported = new List<List<SeaElement>>(2);

    protected override void Awake()
    {
        base.Awake();
        _eventReportBuoy.handle += ReportBuoy;
        _eventReportBoat.handle += ReportBoat;
    }

    private void Start()
    {
        SpawnPointBoatCheckingReported();

        _allSeaObjects.Add(new List<SeaElement>());
        _allSeaObjects.Add(new List<SeaElement>());
        _allReported.Add(new List<SeaElement>());
        _allReported.Add(new List<SeaElement>());
        _allSeaObjects[(int)ReportedObjectType.BUOY].AddRange(_allBuoyObject);
        _allSeaObjects[(int)ReportedObjectType.BOAT].AddRange(_allBoatObject);
    }

    private void OnDestroy()
    {
        _eventReportBuoy.handle -= ReportBuoy;
        _eventReportBoat.handle -= ReportBoat;
    }

    public void ReportBuoy(string id)
    {
        Report(ReportedObjectType.BUOY, id);
    }

    public void ReportBoat(string id)
    {
        Report(ReportedObjectType.BOAT, id);
    }

    private void Report(ReportedObjectType type, string id)
    {
        // Ajouter dans la list des bouée a check
        var reported = GetReportedObject(type, id);

        if (CheckIfIDIsReal(reported)) return;

        Debug.Log($"Boat {id} has bean reported for {type} reason, send somebody to repair it.");

        _allReported[(int)type].Add(reported);

        _currentBoatCheckingReported.UpdateDictionnary(reported);

        // Si pas de check en cours
        if (_currentBoatCheckingReported.State == BoatCheckingReported.BoatState.IDLE)
        {
            // Lancer le check de List[0]
            _currentBoatCheckingReported.ActiveChecking();
        }
    }

    private SeaElement GetReportedObject(ReportedObjectType type, string id)
    {
        return _allSeaObjects[(int)type].Find(x => id.Equals(x._id, StringComparison.CurrentCultureIgnoreCase));
    }

    private void SpawnPointBoatCheckingReported()
    {
        _currentBoatCheckingReported = Instantiate(_prefabBoatCheckingReported, _spawnPointBoatCheckingReported.position, _spawnPointBoatCheckingReported.rotation, _parentBoatCheckingReported);
    }

    public bool CheckIfIDIsReal(SeaElement obj)
    {
        if (obj == null) _eventWrongID.Raise();
        else _eventIDCorrect.Raise();

        return obj == null;
    }
}