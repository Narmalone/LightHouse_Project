using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;
using static SeaManager;

public class SeaElement : MonoBehaviour
{
    [SerializeField] public string _id;
    [SerializeField] private float _fixingDuration;
    public int ID { get => _id.GetHashCode(); }
    public GameObject This { get => gameObject; }

    [SerializeField] protected ReportedObjectType _type;
    [SerializeField] private CustomEvent _eventFixing_Started;
    [SerializeField] private CustomEvent _eventFixing_Update;
    [SerializeField] private CustomEvent _eventFixing_Finish;

    protected virtual void Start()
    {
        SeaManager.Instance.InitSeaElement(_type, this);
    }

    public void InitializeForFixing()
    {
        _eventFixing_Started.handle += OnStartFixing;
        _eventFixing_Update.handle += OnUpdateFixing;
        _eventFixing_Finish.handle += OnEndFixing;
    }
    
    private void OnStartFixing()
    {
        Debug.Log($"Start Fixing Element {_id}");
    }

    public void OnUpdateFixing()
    {
        Debug.Log($"Fixing Element {_id}");
    }

    private void OnEndFixing()
    {
        // Handle End Fixing
        Debug.Log($"End Fixing Element {_id}");

        _eventFixing_Finish.handle -= OnEndFixing;
    }

    internal float GetFixingDuration()
    {
        return _fixingDuration;
    }
}