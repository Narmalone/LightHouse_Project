using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;
using static SeaManager;

public class SeaElement : MonoBehaviour
{
    [SerializeField] public string _id;
    [SerializeField] private float _fixingDuration;
    public string ID { get => _id; }
    public GameObject This { get => gameObject; }

    [Header("STATES")]
    [SerializeField] public ReportedObjectType _type;
    [SerializeField] protected bool _startDamaged = false;

    [Header("EVENTS")]
    [SerializeField] private CustomEvent _eventFixing_Started;
    [SerializeField] private CustomEvent _eventFixing_Update;
    [SerializeField] private CustomEvent _eventFixing_Finish;

    [Header("COMPONENTS")]
    [SerializeField] private GameObject _destroyedMesh;
    [SerializeField] private GameObject _normalMesh;

    protected virtual void Start()
    {
        SeaManager.Instance.InitSeaElement(_type, this);
        HandleDamagedMesh(_startDamaged);
    }

    private void HandleDamagedMesh(bool displayDamaged)
    {
        _destroyedMesh.SetActive(displayDamaged);
        _normalMesh.SetActive(!displayDamaged);
    }

    public void InitializeForFixing()
    {
        _eventFixing_Started.handle += OnStartFixing;
        _eventFixing_Update.handle += OnUpdateFixing;
        _eventFixing_Finish.handle += OnEndFixing;
    }
    
    private void OnStartFixing()
    {
        StartFixing();
    }

    public void OnUpdateFixing()
    {
        UpdateFixing();
    }

    private void OnEndFixing()
    {
        // Handle End Fixing

        _eventFixing_Started.handle -= OnStartFixing;
        _eventFixing_Update.handle -= OnUpdateFixing;
        _eventFixing_Finish.handle -= OnEndFixing;
        EndFixing();
    }

    protected virtual void StartFixing(){}

    protected virtual void UpdateFixing(){}

    protected virtual void EndFixing(){ HandleDamagedMesh(false); }
    
    internal float GetFixingDuration()
    {
        return _fixingDuration;
    }
}