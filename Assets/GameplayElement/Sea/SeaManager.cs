using System.Collections.Generic;
using UnityEngine;

public enum BoatReportType
{
    DANGEREUX,
    SOS,
    PANNE,
    INFRACTION
}

public class SeaManager : Singleton<SeaManager>
{
    public enum ReportedType{
        BUOY = 0,
        BOAT = 1
    }

    [SerializeField] private CustomEvent_Float _eventReportBuoy;
    [SerializeField] private CustomEvent_Float _eventReportBoat;
    [SerializeField] private BoatCheckingReported _prefabBoatCheckingReported;
    [SerializeField] private Transform _parentBoatCheckingReported;
    [SerializeField] private Transform _spawnPointBoatCheckingReported;
    [SerializeField] private SeaReportedObject[] _allBuoy;
    [SerializeField] private SeaReportedObject[] _allBoat;

    private BoatCheckingReported _currentBoatCheckingReported;

    private List<List<SeaReportedObject>> _allSeaObjects = new List<List<SeaReportedObject>>();
    private List<List<SeaReportedObject>> _allReported = new List<List<SeaReportedObject>>();

    protected override void Awake()
    {
        base.Awake();
        _eventReportBuoy.handle += ReportBuoy;
        _eventReportBoat.handle += ReportBoat;
    }

    private void Start()
    {
        SpawnPointBoatCheckingReported();

        _allSeaObjects[(int)ReportedType.BUOY].AddRange(_allBuoy);
        _allSeaObjects[(int)ReportedType.BOAT].AddRange(_allBuoy);
    }

    private void OnDestroy()
    {
        _eventReportBuoy.handle -= ReportBuoy;
        _eventReportBoat.handle -= ReportBoat;
    }

    public void ReportBuoy(float id)
    {
        Report(ReportedType.BUOY, id);
    }

    public void ReportBoat(float id)
    {
        Report(ReportedType.BOAT, id);
    }

    private void Report(ReportedType type, float id)
    {
        // Ajouter dans la list des bouée a check
        var reported = GetReportedObject(type, id);
        _allReported[(int)type].Add(reported);

        _currentBoatCheckingReported.UpdateDictionnary(reported);

        // Si pas de check en cours
        if (_currentBoatCheckingReported.State == BoatCheckingReported.BoatState.IDLE)
        {
            // Lancer le check de List[0]
            _currentBoatCheckingReported.ActiveChecking();
        }
    }

    private SeaReportedObject GetReportedObject(ReportedType type, float id)
    {
        return _allSeaObjects[(int)type].Find(x => x.ID == id);
    }

    private void SpawnPointBoatCheckingReported()
    {
        _currentBoatCheckingReported = Instantiate(_prefabBoatCheckingReported, _spawnPointBoatCheckingReported.position, _spawnPointBoatCheckingReported.rotation, _parentBoatCheckingReported);
    }
}