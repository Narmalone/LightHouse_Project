using System;
using UnityEngine;

public class DeliverySystem : Singleton<DeliverySystem>
{
    public byte baseHoursToWait = 36;
    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private CustomEvent_Int _onDayChanged;
    public AddressableLighthouseSettings _addressableLighthouseSettings;
    //chopper le minuit du prochain jour à l'aide du middnightX de weatherfordaysManager
    //Comparer les 2 résultats entre le minuit actuel et chopper la météo dans X heures
    //
    protected override void Awake()
    {
        base.Awake();
        DayNightManager.OnDayChanged += OnDayChanged; 
    }

    private void Start()
    {
    }

    private void OnDestroy()
    {
        DayNightManager.OnDayChanged -= OnDayChanged;

    }

    private void OnDayChanged(int obj)
    {
        JerricanEssence ess = _addressableLighthouseSettings.InstantiatePrefabByAddress<JerricanEssence>(GeneratedKeys.Jerrican.ToString());
        ess.transform.position = SpawnPoint.position;
        /* StartCoroutine(AddressableCustomUtility.InstantiateAddressableCoroutine<JerricanEssence>(AddressableKeywords.JERRICAN_KEY, null, (s) =>
         {
             s.transform.position = SpawnPoint.position;
         }));*/
    }

    public void SpawnItemFromShop(ShopItemData item)
    {
        if(item.Prefab == null) return;
        Instantiate(item.Prefab, SpawnPoint.position, Quaternion.identity);
    }
}
