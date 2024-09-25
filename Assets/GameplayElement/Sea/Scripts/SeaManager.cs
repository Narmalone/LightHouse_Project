using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField] private CustomEvent_String _eventSpawnBuoy;
    [SerializeField] private CustomEvent _eventWrongID;
    [SerializeField] private CustomEvent _eventIDCorrect;
    [SerializeField] private BoatCheckingReported _prefabBoatCheckingReported;
    [SerializeField] private Buoy _buoyPrefab;
    [SerializeField] private Transform _parentBuoy;
    [SerializeField] private Transform _parentBoatCheckingReported;
    [SerializeField] private Transform[] _spawnPointBoatCheckingReported;
    [SerializeField] private Transform[] _spawnPointBuoy;
    [SerializeField] private int _buoyAmount;

    private List<Transform> _tempSpawnPointBuoy = new List<Transform>();

    private BoatCheckingReported _currentBoatCheckingReported;

    private List<List<SeaElement>> _allSeaObjects = new List<List<SeaElement>>(2);
    private List<List<SeaElement>> _allReported = new List<List<SeaElement>>(2);

    private string _idChar = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
    private string _idCharBuoy = "0123456789";

    protected override void Awake()
    {
        base.Awake();
        _eventReportBuoy.handle += ReportBuoy;
        _eventReportBoat.handle += ReportBoat;

        _allSeaObjects.Add(new List<SeaElement>());
        _allSeaObjects.Add(new List<SeaElement>());
        _allReported.Add(new List<SeaElement>());
        _allReported.Add(new List<SeaElement>());
    }

    private void Start()
    {
        SpawnBuoys();
        SpawnPointBoatCheckingReported();
    }

    private void OnDestroy()
    {
        _eventReportBuoy.handle -= ReportBuoy;
        _eventReportBoat.handle -= ReportBoat;
    }

    internal void InitSeaElement(ReportedObjectType type, SeaElement elem)
    {
        _allSeaObjects[(int)type].Add(elem);
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

        _currentBoatCheckingReported.UpdateList(reported);

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
        var spawnTransform = GetCheckerSpawnPosition();
        _currentBoatCheckingReported = Instantiate(_prefabBoatCheckingReported, spawnTransform.position, spawnTransform.rotation, _parentBoatCheckingReported);
        _currentBoatCheckingReported.SetSpawnPoints(_spawnPointBoatCheckingReported);
    }

    public bool CheckIfIDIsReal(SeaElement obj)
    {
        if (obj == null) _eventWrongID.Raise();
        else _eventIDCorrect.Raise();

        return obj == null;
    }

    private Transform GetCheckerSpawnPosition()
    {
        return _spawnPointBoatCheckingReported[UnityEngine.Random.Range(0,_spawnPointBoatCheckingReported.Length)];
    }
    
    private Transform GetBuoySpawnPosition()
    {
        var spawnPoint = _tempSpawnPointBuoy[UnityEngine.Random.Range(0, _tempSpawnPointBuoy.Count)];
        _tempSpawnPointBuoy.Remove(spawnPoint);
        return spawnPoint;
    }

    private void SpawnBuoys()
    {
        _tempSpawnPointBuoy = _spawnPointBuoy.ToList();

        for (int i = 0; i < _buoyAmount; i++)
        {
            AddBuoy();
        }
    }

    private void AddBuoy()
    {
        var buoy = SpawnBuoy();
        buoy.Initialize(GetID(true));
        // Add buoy in list
        _allSeaObjects[(int)ReportedObjectType.BUOY].Add(buoy);
        // Raise event 
        _eventSpawnBuoy.Raise(buoy.ID);
    }

    public Buoy SpawnBuoy()
    {
        var spawn = GetBuoySpawnPosition();
        return Instantiate(_buoyPrefab, spawn.position, spawn.rotation, _parentBuoy);
    }

    private string GetID(bool isBuoy = false)
    {
        string id = "";
        string idChar = isBuoy ? _idCharBuoy : _idChar;
        int lengthId = isBuoy ? Random.Range(1,4) : 3;

        for (int i = 0; i < lengthId; i++)
        {
            id = $"{id}{idChar[Random.Range(0, idChar.Length)]}";
        }
        return id;
    }
}