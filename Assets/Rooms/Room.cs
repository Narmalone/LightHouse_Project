using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    #region PUBLIC VARIABLES
    [Header("ROOMS INFOS")]
    public GameZone ElectricityRoom;
    public List<ElectricItem> ElectricityItems;
    public Transform ElectricityItemParent;
    #endregion

    #region SERIALIZED VARIABLES

    [Header("--- EVENTS ---")]
    [Header("LISTENERS")]
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneDisabled;

    [Header("READ ONLY / DEBUG PURPOSE")]
    [SerializeField] protected bool IsElecItemsEnabled = false;

    #endregion

    #region MONO CALLBACKS
    protected virtual void Awake()
    {
        _onElecZoneEnabled.handle += _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle += _onElecZoneDisabled_handle;
    }

    private void Start()
    {
        /*foreach (ElectricItem item in ElectricityItems)
        {
            item.HasElectricity = false;
            item.OnElecDisabled();
        }*/
    }

    protected virtual void OnDestroy()
    {
        _onElecZoneEnabled.handle -= _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle -= _onElecZoneDisabled_handle; ;
    }
    #endregion

    #region DELEGATES LISTENERS
    private void _onElecZoneEnabled_handle(GameZone obj)
    {
        if (obj != ElectricityRoom) return;
        IsElecItemsEnabled = true;

        foreach(ElectricItem item in ElectricityItems)
        {
            item.HasElectricity = true;
            item.OnElecEnabled();
        }
    }

    private void _onElecZoneDisabled_handle(GameZone obj)
    {
        if (obj != ElectricityRoom) return;
        IsElecItemsEnabled = false;

        foreach (ElectricItem item in ElectricityItems)
        {
            item.HasElectricity = false;
            item.OnElecDisabled();
        }
    }
    #endregion

    private void OnValidate()
    {
        ElectricityItems = new List<ElectricItem>();
        if (ElectricityItemParent.childCount <= 0) return;
        for (int i = 0; i < ElectricityItemParent.childCount; i++)
        {
            if (ElectricityItemParent.GetChild(i).TryGetComponent(out ElectricItem item))
            {
                ElectricityItems.Add(item);
            }
        }
    }
}
